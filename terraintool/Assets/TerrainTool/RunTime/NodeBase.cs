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

    public class NodeContainer:ScriptableObject
    {
        public string name;
        public Vector2 pos;
        public NodeBase node;
        [NonSerialized]
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
                for (int i = 0; i < inputs.Count; i++) {
                    if (inputs[i] != null) {
                        rlt ^= inputs[i].GetHashCode();
                    }
                }
            }
            return rlt;
        }
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

    public abstract class NodeBase:ScriptableObject
    {
        public abstract NodeType getNodeType();
        public virtual string[] GetInputNames(){return new string[0];}
        [NonSerialized]
        public NodeBase[] inputs;
        public abstract float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f);
    }

    public class NodeConstValue : NodeBase
    {
        public float value = 1.0f;
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


    public class NodePerlin : NodeBase
    {
        public float scale = 1.0f;
        public float bias = 0f;
        public float size = 100f;
        public int octaveCount = 4;
        public int localSeed = 0;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        LibNoise.Unity.Generator.Perlin generator = new LibNoise.Unity.Generator.Perlin();
        public override NodeType getNodeType()
        {
            return NodeType.Generator;
        }
        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            generator.Frequency = 1.0 / size;
            generator.OctaveCount = 1;
            generator.Seed = seed + localSeed;
            generator.Quality = LibNoise.Unity.QualityMode.High;

            string key = PerlinCache.makeKey("perlin", generator.Frequency, generator.Seed, octaveCount, x, y, w, h, scaleX, scaleY, curve);
            float[,] temp = null;
            if (PerlinCache.hasKey(key)) {
                temp = PerlinCache.getCache(key);
            }
            else {
                temp = new float[w, h];
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        float tempX = x + i;
                        float tempY = y + j;
                        float val = 0f;
                        float cp = 0.5f;
                        for (int o = 0; o < octaveCount; o++) {
                            float signal = (float)generator.GetValue(tempX * scaleX, tempY * scaleY, 0);
                            val += (curve.Evaluate(signal * 0.4f + 0.5f) - 0.5f) * 2f * cp;
                            tempX *= 1.93456789f;
                            tempY *= 1.93456789f;
                            cp *= 0.5f;
                        }
                        temp[i, j] = (val * 0.5f + 0.5f);
                    }
                }
                PerlinCache.addCache(key, temp);
            }
            float[,] values = new float[w, h];
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    values[i, j] = temp[i, j] * scale + bias;
                }
            }
            return values;
        }
    }


    public class NodeImport : NodeBase
    {
        public float scale = 1.0f;
        public float bias = 0f;

        public Texture2D texture;

        public override NodeType getNodeType()
        {
            return NodeType.Generator;
        }

        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            if (texture != null) {
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        float u = i / (float)w;
                        float v = j / (float)h;
                        values[i, j] = texture.GetPixelBilinear(u, v).r * scale + bias;
                    }
                }
            }
            return values;
        }
    }


    public class NodeCurve : NodeBase
    {
        public float scale = 1.0f;
        public float bias = 0f;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
        public override NodeType getNodeType()
        {
            return NodeType.UnaryOperator;
        }
        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            if (inputs[0] != null) {
                float[,] a = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        values[i, j] = curve.Evaluate(a[i, j]);
                    }
                }
            }
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    values[i, j] = values[i, j] * scale + bias;
                }
            }
            return values;
        }
        public override string[] GetInputNames()
        {
            return new string[] { "i" };
        }

    }

    public class NodeNormal : NodeBase
    {
        public float scale = 1.0f;
        public float bias = 0f;
        public float height = 100.0f;
        public enum NormalType
        {
            NT_X,
            NT_Y,
            NT_Z,
        }
        public NormalType normalType = NormalType.NT_Y;
        public override NodeType getNodeType()
        {
            return NodeType.UnaryOperator;
        }
        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            if (inputs[0] != null) {
                float[,] a = inputs[0].update(seed, x, y, w + 1, h + 1, scaleX, scaleY);
                Vector3[,] normal = new Vector3[w, h];
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        Vector3 pos_x = new Vector3(1, (a[i + 1, j] - a[i, j]) * height, 0);
                        Vector3 pos_z = new Vector3(0, (a[i, j + 1] - a[i, j]) * height, 1);
                        normal[i, j] = Vector3.Cross(pos_x, -pos_z).normalized;
                    }
                }
                if (normalType == NormalType.NT_X) {
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = normal[i, j].x;
                        }
                    }
                }
                else if (normalType == NormalType.NT_Z) {
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = normal[i, j].z;
                        }
                    }
                }
                else if (normalType == NormalType.NT_Y) {
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = normal[i, j].y;
                        }
                    }
                }
            }
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    values[i, j] = values[i, j] * scale + bias;
                }
            }
            return values;
        }
        public override string[] GetInputNames()
        {
            return new string[] { "i" };
        }
    }

    public class NodeErosion : NodeBase
    {
        public float scale = 1.0f;
        public float bias = 0f;
        public override NodeType getNodeType()
        {
            return NodeType.UnaryOperator;
        }
        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            if (inputs[0] != null) {
                float[,] heights = inputs[0].update(seed, x, y, w + 1, h + 1, scaleX, scaleY);
                Vector3[,] normal = new Vector3[w, h];
                int height = 100;
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        Vector3 pos_x = new Vector3(1, (heights[i + 1, j] - heights[i, j]) * height, 0);
                        Vector3 pos_z = new Vector3(0, (heights[i, j + 1] - heights[i, j]) * height, 1);
                        normal[i, j] = Vector3.Cross(pos_x, -pos_z).normalized;
                    }
                }
                float[,] water = new float[w, h];
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        water[i, j] += 0.01f;//rain
                    }
                }
                for (int i = 0; i < w - 1; i++) {
                    for (int j = 0; j < h - 1; j++) {
                        water[i, j] = 0;
                    }
                }
            }
            return values;
        }
        public override string[] GetInputNames()
        {
            return new string[] { "i" };
        }
    }

    public class NodeBinaryOperator : NodeBase
    {
        public float scale = 1.0f;
        public float bias = 0f;
        public BinaryOperatorType operatorType;
        public override NodeType getNodeType()
        {
            return NodeType.BinaryOperator;
        }
        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            if (operatorType == BinaryOperatorType.Add) {
                if (inputs[0] != null && inputs[1] != null) {
                    float[,] a = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
                    float[,] b = inputs[1].update(seed, x, y, w, h, scaleX, scaleY);
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = a[i, j] + b[i, j];
                        }
                    }
                }
            }
            else if (operatorType == BinaryOperatorType.Sub) {
                if (inputs[0] != null && inputs[1] != null) {
                    float[,] a = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
                    float[,] b = inputs[1].update(seed, x, y, w, h, scaleX, scaleY);
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = a[i, j] - b[i, j];
                        }
                    }
                }
            }
            else if (operatorType == BinaryOperatorType.Mul) {
                if (inputs[0] != null && inputs[1] != null) {
                    float[,] a = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
                    float[,] b = inputs[1].update(seed, x, y, w, h, scaleX, scaleY);
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = a[i, j] * b[i, j];
                        }
                    }
                }
            }
            else if (operatorType == BinaryOperatorType.Max) {
                if (inputs[0] != null && inputs[1] != null) {
                    float[,] a = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
                    float[,] b = inputs[1].update(seed, x, y, w, h, scaleX, scaleY);
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = a[i, j] > b[i, j] ? a[i, j] : b[i, j];
                        }
                    }
                }
            }
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    values[i, j] = values[i, j] * scale + bias;
                }
            }
            return values;
        }
        public override string[] GetInputNames()
        {
            return new string[] { "a", "b" };
        }
    }


    public class NodeTernaryOperator : NodeBase
    {
        public float scale = 1.0f;
        public float bias = 0f;
        public TernaryOperatorType operatorType;
        public override NodeType getNodeType()
        {
            return NodeType.TernaryOperator;
        }
        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            if (operatorType == TernaryOperatorType.Lerp) {
                if (inputs[0] != null && inputs[1] != null && inputs[2] != null) {
                    float[,] a = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
                    float[,] s = inputs[1].update(seed, x, y, w, h, scaleX, scaleY);
                    float[,] b = inputs[2].update(seed, x, y, w, h, scaleX, scaleY);
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = Mathf.Lerp(a[i, j], b[i, j], s[i, j]);
                        }
                    }
                }
            }
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    values[i, j] = values[i, j] * scale + bias;
                }
            }
            return values;
        }
        public override string[] GetInputNames()
        {
            return new string[] { "a", "s", "b" };
        }
    }

    public class HeightOutput : NodeBase
    {
        public float scale = 1.0f;
        public float bias = 0f;
        public override NodeType getNodeType()
        {
            return NodeType.HeightOutput;
        }
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
    }

    public class TextureOutput : NodeBase
    {
        public float scale = 1.0f;
        public float bias = 0f;
        public int paintOrder = 0;
        public int textureIndex = 0;
        public Texture2D texture;
        public Texture2D normal;
        public int texSizeX = 15;
        public int texSizeY = 15;
        public override NodeType getNodeType()
        {
            return NodeType.TextureOutput;
        }
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
    }

    public class GrassOutput : NodeBase
    {
        public float scale = 1.0f;
        public float bias = 0f;
        public int grassIndex = 0;
        public Texture2D texture;
        public float minSize = 1.0f;
        public float maxSize = 1.0f;
        public bool bBillboard = true;
        private bool bShowInMain = true;
        public Texture2D Texture { get { return texture; } }

        public override NodeType getNodeType()
        {
            return NodeType.GrassOutput;
        }
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
    }

    public class TreeOutput : NodeBase
    {
        public float scale = 1.0f;
        public float bias = 0f;
        public float density = 0.01f;
        public int treeIndex = 0;
        public float bendFactor;
        public float maxSize = 1.2f;
        public float minSize = 0.8f;
        public GameObject objTree;
        public bool bEntity = false;
        public override NodeType getNodeType()
        {
            return NodeType.TreeOutput;
        }
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
        private static long getHash(long a)
        {
            a = (a ^ 61) ^ (a >> 16);
            a = a + (a << 3);
            a = a ^ (a >> 4);
            a = a * 0x27d4eb2d;
            a = a ^ (a >> 15);
            return a;
        }
        public static int getTreeNum(int x, int y, float val, float density, int layer)
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
        public static Vector2 getTreePos(int x, int y, int index, float maxOffset, int layer)
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
        public static float GetAngle(int x, int y, int index, float maxOffset, int layer)
        {
            long hashCode = getHash(x * 123456789 + y + index * 123456 + layer * 12345678);
            hashCode = getHash(hashCode);
            hashCode = getHash(hashCode);
            hashCode = getHash(hashCode);
            float angle = (hashCode & 0xffffffff) / (float)0xffffffff;
            return angle;
        }
        public static float GetScale(int x, int y, int index, float maxOffset, int layer)
        {
            long hashCode = getHash(x * 1234567 + y + index * 1234567 + layer * 1234567);
            hashCode = getHash(hashCode);
            hashCode = getHash(hashCode);
            hashCode = getHash(hashCode);
            float angle = (hashCode & 0xffffffff) / (float)0xffffffff;
            return angle;
        }
    }
}

