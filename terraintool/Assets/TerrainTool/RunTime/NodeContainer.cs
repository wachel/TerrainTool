﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using System.Reflection;

namespace TerrainTool
{
    public class NodeContainer : ScriptableObject
    {
        public bool foldout = true;
        public Rect rect;
        public NodeBase node;
        [NonSerialized]
        public int winID;
        public Texture2D texture;
        public int oldHashCode;

        public void SetNode(NodeBase node)
        {
            this.node = node;
            node.container = this;
        }
        public float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            if (node != null) {
                return node.update(seed, x, y, w, h, scaleX, scaleY);
            }
            return new float[w, h];
        }
        public void updatePreviewTexture(int seed, int x, int y, int w, int h)
        {
            float[,] data = update(seed, x, y, w, h);
            texture = new Texture2D(w, h);
            Color[] colors = new Color[w * h];
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    colors[j * w + i] = new Color(data[i, j], data[i, j], data[i, j]);
                }
            }
            texture.SetPixels(colors);
            texture.Apply();
        }
        public int GetMyHashCode()
        {
            int rlt = 0;
            if (node != null) {
                foreach (FieldInfo f in node.GetType().GetFields()) {
                    var val = f.GetValue(node);
                    if (val != null) {
                        if (f.FieldType == typeof(AnimationCurve)) {
                            AnimationCurve c = (AnimationCurve)val;
                            rlt ^= PerlinCache.getCurveHash(c);
                        }
                        rlt ^= val.GetHashCode();
                    }
                }
                for (int i = 0; i < node.inputs.Length; i++) {
                    if (node.inputs[i] != null) {
                        rlt ^= node.inputs[i].container.GetMyHashCode();
                    }
                }
            }
            return rlt;
        }
        public bool hasOutput()
        {
            return node.hasOutput();
        }
        public int getInputNum()
        {
            return node.GetInputNames().Length;
        }
        public string GetInputLabel(int index)
        {
            return node.GetInputNames()[index];
        }
        public Rect getInputPortRect(int index)
        {
            const int portSize = 16;

            float a = 0.1f + (index + 1) * ((1f - 0.2f) / (getInputNum() + 1f));
            return new Rect(rect.xMax, rect.yMin + a * rect.height - portSize / 2, 14, 16);
        }

        public Rect getOutputPortRect()
        {
            return new Rect(rect.x - 14, rect.center.y - 8, 14, 16);
        }
        public int GetSortValue()
        {
            int rlt = (int)node.nodeType * 1000;
            rlt += node.subTypeIndex * 100;
            if(node is TextureOutput) {
                rlt += (node as TextureOutput).paintOrder;
            }
            return rlt;
        }
    }
}