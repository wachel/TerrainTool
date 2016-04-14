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
        Output,
    }

    public abstract class NodeBase : ScriptableObject
    {
        [HideInInspector]
        public NodeType nodeType;
        [HideInInspector]
        public string subType;
        [HideInInspector]
        public int subTypeIndex;
        [HideInInspector]
        public NodeContainer container;
        public string name;
        public bool isPublic = false;
        public float scale = 1.0f;
        public float bias = 0f;
        public virtual bool hasOutput(){return true;}
        public virtual string[] GetInputNames() { return new string[0]; }
        public virtual string GetExpression() { return ""; }
        [HideInInspector]
        public NodeBase[] inputs = new NodeBase[0];
        public abstract float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f);
    }

    public static class PerlinCache
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

    public class NodeMaker
    {
        private static NodeMaker _instance;
        public static NodeMaker Instance
        {
            get
            {
                if (_instance == null) {
                    _instance = new NodeMaker();
                    _instance.Init();
                }
                return _instance;
            }
        }
        private List<NodeItem> nodes = new List<NodeItem>();
        private Dictionary<NodeType, string[]> subTypes = new Dictionary<NodeType,string[]>();
        private class NodeItem
        {
            public NodeItem(NodeType type, string subType, Func<NodeBase> funNewNode)
            {
                this.type = type;
                this.subType = subType;
                this.funNewNode = funNewNode;
            }
            public NodeType type;
            public string subType;
            public Func<NodeBase> funNewNode;
        }
        private void AddNodeType(NodeType type, string subType, Func<NodeBase> funNewNode)
        {
            nodes.Add(new NodeItem(type, subType, funNewNode));
        }
        private NodeBase CreateBinaryOp(NodeBinaryOperator.BinaryOperatorType op)
        {
            NodeBinaryOperator nbo = ScriptableObject.CreateInstance<NodeBinaryOperator>();
            nbo.operatorType = op;
            return nbo;
        }
        private NodeBase CreateTernaryOp(NodeTernaryOperator.TernaryOperatorType op)
        {
            NodeTernaryOperator nto = ScriptableObject.CreateInstance<NodeTernaryOperator>();
            nto.operatorType = op;
            return nto;
        }
        public void Init()
        {
            AddNodeType(NodeType.Generator, "Const Value", () => { return ScriptableObject.CreateInstance<NodeConstValue>(); });
            AddNodeType(NodeType.Generator, "Perlin Noise", () => { return ScriptableObject.CreateInstance<NodePerlin>(); });
            AddNodeType(NodeType.Generator, "Import", () => { return ScriptableObject.CreateInstance<NodeImport>(); });
            AddNodeType(NodeType.UnaryOperator, "Curve", () => { return ScriptableObject.CreateInstance<NodeCurve>(); });
            AddNodeType(NodeType.UnaryOperator, "Normal", () => { return ScriptableObject.CreateInstance<NodeNormal>(); });
            AddNodeType(NodeType.BinaryOperator, "Add", () => { return CreateBinaryOp(NodeBinaryOperator.BinaryOperatorType.Add); });
            AddNodeType(NodeType.BinaryOperator, "Sub", () => { return CreateBinaryOp(NodeBinaryOperator.BinaryOperatorType.Sub); });
            AddNodeType(NodeType.BinaryOperator, "Mul", () => { return CreateBinaryOp(NodeBinaryOperator.BinaryOperatorType.Mul); });
            AddNodeType(NodeType.BinaryOperator, "Max", () => { return CreateBinaryOp(NodeBinaryOperator.BinaryOperatorType.Max); });
            AddNodeType(NodeType.TernaryOperator, "Lerp", () => { return CreateTernaryOp(NodeTernaryOperator.TernaryOperatorType.Lerp); });
            AddNodeType(NodeType.Output, "Height", () => { return ScriptableObject.CreateInstance<HeightOutput>(); });
            AddNodeType(NodeType.Output, "Texture", () => { return ScriptableObject.CreateInstance<TextureOutput>(); });
            AddNodeType(NodeType.Output, "Grass", () => { return ScriptableObject.CreateInstance<GrassOutput>(); });
            AddNodeType(NodeType.Output, "Tree", () => { return ScriptableObject.CreateInstance<TreeOutput>(); });
        }
        public string[] GetSubTypes(NodeType type)
        {
            if(!subTypes.ContainsKey(type)) {
                List<string> types = new List<string>();
                for (int i = 0; i < nodes.Count; i++) {
                    if (nodes[i].type == type) {
                        types.Add(nodes[i].subType);
                    }
                }
                subTypes[type] = types.ToArray();
            }
            return subTypes[type];
        }
        public int GetSubTypeIndex(NodeType type, string subTypeName)
        {
            if (subTypes.ContainsKey(type)) {
                string[] names = subTypes[type];
                for (int i = 0; i < names.Length; i++) {
                    if (names[i] == subTypeName) {
                        return i;
                    }
                }
            }
            return -1;
        }
        public NodeBase CreateNode(NodeType type)
        {
            for (int i = 0; i < nodes.Count; i++) {
                if (nodes[i].type == type) {
                    return CreateNode(nodes[i]);
                }
            }
            return null;
        }
        public NodeBase CreateNode(NodeType type,string subType)
        {
            for (int i = 0; i < nodes.Count; i++) {
                if (nodes[i].type == type && nodes[i].subType == subType) {
                    return CreateNode(nodes[i]);
                }
            }
            return null;
        }
        private NodeBase CreateNode(NodeItem nodeType)
        {
            NodeBase rlt = nodeType.funNewNode();
            rlt.nodeType = nodeType.type;
            rlt.subType = nodeType.subType;
            rlt.subTypeIndex = GetSubTypeIndex(nodeType.type,rlt.subType);
            rlt.name = nodeType.subType;
            rlt.inputs = new NodeBase[rlt.GetInputNames().Length];
            if(nodeType.type == NodeType.Output) {
                rlt.isPublic = true;
            }
            return rlt;
        }
    }


}

