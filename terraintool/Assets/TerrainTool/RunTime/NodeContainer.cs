using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace TerrainTool
{
    public class NodeContainer : ScriptableObject
    {
        public bool foldout = true;
        public Rect rect;
        public NodeBase node;
        //public List<NodeContainer> inputs = new List<NodeContainer>();
        [NonSerialized]
        public int winID;
        public float[,] previewTexture;

        public Texture2D tex;
        public bool bNeedUpdate;

        public void SetNode(NodeBase node)
        {
            this.node = node;
            node.container = this;
            //while (node.inputs.Length > inputs.Count) {
            //    inputs.Add(null);
            //}
            //while (node.inputs.Length < inputs.Count) {
            //    inputs.RemoveAt(inputs.Count - 1);
            //}
        }
        public float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            if (node != null) {
                return node.update(seed, x, y, w, h, scaleX, scaleY);
            }
            return new float[w, h];
        }
        //public void updateInputNode()
        //{
        //    if (node != null) {
        //        for (int i = 0; i < node.inputs.Length; i++) {
        //            node.inputs[i] = inputs[i].node;
        //        }
        //    }
        //}
        public void updatePreview(int seed, int x, int y, int w, int h)
        {
            previewTexture = update(seed, x, y, w, h);
        }
        public int GetHashCode()
        {
            int rlt = 0;
            if (node != null) {
                foreach (var f in node.GetType().GetFields()) {
                    var val = f.GetValue(this);
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
                        rlt ^= node.inputs[i].GetHashCode();
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
    }
}