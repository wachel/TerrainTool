using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace TerrainTool
{
    public enum NodeType
    {
        Generator,
        UnaryOperator,
        BinaryOperator,
        TernaryOperator,
        HeightOutput,
        TextureOutput,
        TreeOutput,
        GrassOutput,
    }

    public enum GeneratorType
    {
        Const_Value,
        Noise,
        //Import,
    }
    public enum UnaryOperatorType
    {
        Curve,
        Normal,
        //Erosion,
    }
    public enum BinaryOperatorType
    {
        Add,
        Sub,
        Mul,
        Max,
    }
    public enum TernaryOperatorType
    {
        Lerp,
    }

    public enum OutputType
    {
        Height_Output,
    }

    [Serializable]
    public class NodeContainer
    {
        public string name;
        public Vector2 pos;
        public NodeBase node;
        public List<NodeContainer> inputs = new List<NodeContainer>();
    }

    [Serializable]
    public abstract class NodeBase
    {
        NodeContainer container;
        private float[,] previewTexture;
        
        public abstract NodeType getNodeType();
        public abstract float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f);
        public float[,] getPreview()
        {
            return previewTexture;
        }
        public void updatePreview(int seed, int x, int y, int w, int h)
        {
            previewTexture = update(seed, x, y, w, h);
        }

        //public override int GetHashCode()
        //{
        //    int rlt = 0;
        //    foreach (var f in this.GetType().GetFields()) {
        //        var val = f.GetValue(this);
        //        if (val != null) {
        //            if (f.FieldType.ToString() == "UnityEngine.AnimationCurve") {
        //                AnimationCurve c = (AnimationCurve)val;
        //                rlt ^= PerlinCache.getCurveHash(c);
        //            }
        //            rlt ^= val.GetHashCode();
        //        }
        //    }
        //    for (int i = 0; i < getInputNum(); i++) {
        //        if (getInputNode(i) != null) {
        //            rlt ^= getInputNode(i).GetHashCode();
        //        }
        //    }
        //    return rlt;
        //}
    }
}

