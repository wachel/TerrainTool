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
        private NodeBase[] inputsNode;
        public float[,] previewTexture;
        public void SetNode(NodeBase node)
        {
            this.node = node;
            while(node.inputs.Length > inputs.Count) {
                inputs.Add(null);
            }
            while(node.inputs.Length < inputs.Count) {
                inputs.RemoveAt(inputs.Count - 1);
            }
        }
        public float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            if(node != null) {
                return node.update(seed, x, y, w, h, scaleX, scaleY);
            }
            return new float[w, h];
        }
        public void updateInput()
        {
            if (node != null) {
                for (int i = 0; i < node.inputs.Length; i++) {
                    node.inputs[i] = inputs[i].node;
                }
            }
        }
        public void updatePreview(int seed, int x, int y, int w, int h)
        {
            previewTexture = update(seed, x, y, w, h);
        }
        public override int GetHashCode()
        {
            int rlt = 0;
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
            for (int i = 0; i < inputs.Count; i++) {
                if (inputs[i] != null) {
                    rlt ^= inputs[i].GetHashCode();
                }
            }
            return rlt;
        }
    }

    public class PerlinCache
    {
        static Dictionary<string, float[,]> cachedDatas = new Dictionary<string, float[,]>();
        private static string Md5Sum(string strToEncrypt)
        {
            System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
            byte[] bytes = ue.GetBytes(strToEncrypt);
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);
            string hashString = "";
            for (int i = 0; i < hashBytes.Length; i++) {
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }
            return hashString.PadLeft(32, '0');
        }

        public static string makeKey(string model, double frequency, int octaveCount, int seed, int x, int y, int w, int h, float scaleX, float scaleY, AnimationCurve curve)
        {
            string temp = "";
            temp += model;
            temp += "f" + frequency.ToString();
            temp += "s" + seed.ToString();
            temp += "o" + octaveCount.ToString();
            temp += "x" + x.ToString();
            temp += "y" + y.ToString();
            temp += "w" + w.ToString();
            temp += "h" + h.ToString();
            temp += "sx" + scaleX.ToString();
            temp += "sy" + scaleY.ToString();
            temp += "c" + getCurveHash(curve).ToString();
            return Md5Sum(temp);
        }

        public static int getCurveHash(AnimationCurve val)
        {
            int rlt = 0;
            AnimationCurve c = (AnimationCurve)val;
            foreach (Keyframe k in c.keys) {
                rlt ^= (k.inTangent * 1.122112 + k.outTangent * 2.123123 + k.tangentMode * 0.1324123 + k.time * 4.2343113 + k.value * 2.34233).GetHashCode();
            }
            return rlt;
        }
        public static void addCache(string key, float[,] data)
        {
            cachedDatas.Add(key, data);
        }
        public static bool hasKey(string key)
        {
            return cachedDatas.ContainsKey(key);
        }
        public static float[,] getCache(string key)
        {
            if (cachedDatas.ContainsKey(key)) {
                return cachedDatas[key];
            }
            return null;
        }
    }

    [Serializable]
    public abstract class NodeBase
    {
        public abstract NodeType getNodeType();
        public NodeBase[] inputs;
        public abstract float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f);
    }

    public class NodeConstValue : NodeBase
    {
        public float value = 1.0f;
        public bool bPublic = false;
        public NodeConstValue()
        {
            inputs = new NodeBase[0];
        }
        public override NodeType getNodeType()
        {
            return NodeType.Generator;
        }
        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    values[i, j] = value;
                }
            }
            return values;
        }
    }

}

