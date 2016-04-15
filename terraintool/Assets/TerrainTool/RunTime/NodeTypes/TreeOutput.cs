﻿using UnityEngine;
using System.Collections;

namespace TerrainTool
{
    public class TreeOutput : NodeBase
    {
        public float density = 0.01f;
        public float bendFactor;
        public float maxSize = 1.2f;
        public float minSize = 0.8f;
        public GameObject[] prefabs = new GameObject[1];
        public bool isEntity = false;
        [HideInInspector]
        public int startTreePropertyIndex = 0;

        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = null;
            if (inputs[0] != null) {
                values = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
            }
            else {
                values = new float[w, h];
            }
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    values[i, j] = Mathf.Clamp(values[i, j] * scale + bias, 0f, 1f);
                }
            }
            return values;
        }
        public override string[] GetInputNames()
        {
            return new string[] { "i" };
        }
        public override bool hasOutput()
        {
            return false;
        }
        private static long getHash(long a)
        {
            a = (a ^ 61) ^ (a >> 16);
            a = a + (a << 3);
            a = a ^ (a >> 4);
            a = a * 0x27d4eb2d;
            a = a ^ (a >> 15);
            return a;
        }
        public int getTreeNum(int x, int y, float val, float density, int layer)
        {
            long hashCode = getHash(x * 123456789 + y + layer * 1234567);
            hashCode = getHash(hashCode);
            hashCode = getHash(hashCode);
            hashCode = getHash(hashCode);
            float rand = (hashCode & 0xffffffff) / (float)0xffffffff;
            float realDensity = val * density;
            int a = (int)realDensity;
            if (rand < (realDensity - a)) {
                a += 1;
            }
            return a;
        }
        public Vector2 getTreePos(int x, int y, int index, float maxOffset, int layer)
        {
            long hashCode = getHash(x * 123456789 + y + index * 123456 + layer * 12345678);
            hashCode = getHash(hashCode);
            hashCode = getHash(hashCode);
            hashCode = getHash(hashCode);
            float randX = (hashCode & 0xffffffff) / (float)0xffffffff;
            float randY = ((hashCode >> 32) & 0xffffffff) / (float)0xffffffff;
            Vector2 rlt = new Vector2((randX - 0.5f) * maxOffset, (randY - 0.5f) * maxOffset);
            return rlt;
        }
        public float GetAngle(int x, int y, int index, float maxOffset, int layer)
        {
            long hashCode = getHash(x * 123456789 + y + index * 123456 + layer * 12345678);
            hashCode = getHash(hashCode);
            hashCode = getHash(hashCode);
            hashCode = getHash(hashCode);
            float angle = (hashCode & 0xffffffff) / (float)0xffffffff;
            return angle;
        }
        public float GetScale(int x, int y, int index, float maxOffset, int layer)
        {
            long hashCode = getHash(x * 1234567 + y + index * 1234567 + layer * 1234567);
            hashCode = getHash(hashCode);
            hashCode = getHash(hashCode);
            hashCode = getHash(hashCode);
            float angle = (hashCode & 0xffffffff) / (float)0xffffffff;
            return angle;
        }
        public int GetPrefabIndex(int x, int y, int index, float maxOffset, int layer)
        {
            if (prefabs.Length > 0) {
                long hashCode = getHash(x * 1234567 + y + index * 1234567 + layer * 1234567);
                hashCode = getHash(hashCode);
                hashCode = getHash(hashCode);
                hashCode = getHash(hashCode);
                return (int)(hashCode & 0xffffffff) % prefabs.Length;
            };
            return 0;
        }
    }
}