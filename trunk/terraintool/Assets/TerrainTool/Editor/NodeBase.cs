using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System;
using System.IO;

public enum NodeType
{
    None,
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
    Import,
}
public enum UnaryOperatorType
{
    Curve,
    Normal,
    Erosion,
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
public abstract class NodeBase:ScriptableObject
{
    public NodeBase() {
        label = getDefaultName();
    }
    public string label = "name";
    public delegate NodeBase FindNodeFun(string guid);
    public string guid;
    private float[,] previewTexture;
    public string[] inputs = null;
    public abstract NodeType getNodeType();
    public abstract float[,] update(int seed,int x, int y, int w, int h,float scaleX=1.0f,float scaleY=1.0f);
    public abstract string getDefaultName();
    public float[,] getPreview() {
        return previewTexture;
    }
    public virtual void OnWindowGUI(){}
    public abstract void OnGUI();
    public virtual void OnMainGUI(){}

    private FindNodeFun funFindNode;
    public virtual GeneratorType getGeneratorType() {
        return GeneratorType.Const_Value;
    }
    public virtual UnaryOperatorType getUnaryOperatorType() {
        return UnaryOperatorType.Curve;
    }
    public virtual BinaryOperatorType getBinaryOperatorType() {
        return BinaryOperatorType.Add;
    }
    public virtual TernaryOperatorType getTernaryOperatorType() {
        return TernaryOperatorType.Lerp;
    }
    public virtual void beforeSave(){}
    public virtual void postLoaded(){}
    public void copy(NodeBase src) {
        guid = src.guid;
        label = src.label;
        funFindNode = src.funFindNode;
    }
    public void setFun(FindNodeFun funFind){
        funFindNode = funFind;
    }
    public void updatePreview(int seed, int x, int y, int w, int h) {
        previewTexture = update(seed, x, y, w, h);
    }
    public static NodeBase createNewNode(NodeType t) {
        NodeBase rlt = null;
        if (t == NodeType.Generator) {
            rlt = createNewGenerate(GeneratorType.Const_Value);
        }
        else if (t == NodeType.UnaryOperator) {
            rlt = createNewUnaryOperator(UnaryOperatorType.Curve);
        }
        else if (t == NodeType.BinaryOperator) {
            rlt = createNewBinaryOperator(BinaryOperatorType.Add);
        }
        else if (t == NodeType.TernaryOperator) {
            rlt = createNewTernaryOperator(TernaryOperatorType.Lerp);
        }
        return rlt;
    }
    public static NodeBase createNewGenerate(GeneratorType t) {
        NodeBase rlt = null;
        if (t == GeneratorType.Const_Value) {
            rlt = new NodeConst();
        }
        else if (t == GeneratorType.Noise) {
            rlt = new NodePerlin();
        }
        else if(t == GeneratorType.Import) {
            rlt = new NodeImport();
        }
        rlt.initInput();
        rlt.guid = Guid.NewGuid().ToString();
        return rlt;
    }
    public static NodeBase createNewUnaryOperator(UnaryOperatorType t) {
        NodeBase rlt = null;
        if (t == UnaryOperatorType.Curve) {
            rlt = new NodeCurve();
        }
        else if (t == UnaryOperatorType.Normal) {
            rlt = new NodeNormal();
        }
        else if (t == UnaryOperatorType.Erosion) {
            rlt = new NodeErosion();
        }
        rlt.initInput();
        rlt.guid = Guid.NewGuid().ToString();
        return rlt;
    }
    public static NodeBase createNewBinaryOperator(BinaryOperatorType t) {
        NodeBase rlt = null;
        rlt = new NodeBinaryOperator();
        ((NodeBinaryOperator)rlt).operatorType = t;
        rlt.initInput();
        rlt.guid = Guid.NewGuid().ToString();
        return rlt;
    }
    public static NodeBase createNewTernaryOperator(TernaryOperatorType t) {
        NodeBase rlt = null;
        rlt = new NodeTernaryOperator();
        ((NodeTernaryOperator)rlt).operatorType = t;
        rlt.initInput();
        rlt.guid = Guid.NewGuid().ToString();
        return rlt;
    }
    public static NodeBase createNewHeightOutput() {
        NodeBase rlt = new HeightOutput();
        rlt.initInput();
        rlt.guid = Guid.NewGuid().ToString();
        return rlt;
    }
    public static NodeBase createNewTextureOutput() {
        NodeBase rlt = new TextureOutput();
        rlt.initInput();
        rlt.guid = Guid.NewGuid().ToString();
        return rlt;
    }
    public static NodeBase createNewGrassOutput() {
        NodeBase rlt = new GrassOutput();
        rlt.initInput();
        rlt.guid = Guid.NewGuid().ToString();
        return rlt;
    }
    public static NodeBase createNewTreeOutput() {
        NodeBase rlt = new TreeOutput();
        rlt.initInput();
        rlt.guid = Guid.NewGuid().ToString();
        return rlt;
    }
    public override int GetHashCode() {
        int rlt = 0;
        foreach(var f in this.GetType().GetFields()){
            var val = f.GetValue(this);
            if (val != null) {
                if (f.FieldType.ToString() == "UnityEngine.AnimationCurve") {
                    AnimationCurve c = (AnimationCurve)val;
                    rlt ^= PerlinCache.getCurveHash(c);
                }
                rlt ^= val.GetHashCode();
            }
        }
        for (int i = 0; i < getInputNum(); i++) {
            if (getInputNode(i) != null) {
                rlt ^= getInputNode(i).GetHashCode();
            }
        }
        return rlt;
    }
    public virtual int getInputNum() {
        return 0;
    }
    public virtual string getInputName(int index) {
        return "";
    }
    public NodeBase getInputNode(int index) {
        return funFindNode(getInputGuid(index));
    }
    public string getInputGuid(int index) {
        return inputs[index];
    }
    public void setInputGuid(int index,string guid) {
        inputs[index] = guid;
    }
    private void initInput() {
        inputs = new string[getInputNum()];
        for (int i = 0; i < inputs.Length; i++ ) {
            inputs[i] = "";
        }
    }
    public bool hasOutput() {
        return (    getNodeType() == NodeType.Generator
                 || getNodeType() == NodeType.BinaryOperator
                 || getNodeType() == NodeType.UnaryOperator
                 || getNodeType() == NodeType.TernaryOperator
                );
    }
}

public class PerlinCache
{
    static Dictionary<string, float[,]> cachedDatas = new Dictionary<string, float[,]>();
    private static string Md5Sum(string strToEncrypt) {
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
    public static string makeKey(string model, double frequency,int octaveCount, int seed, int x, int y, int w, int h, float scaleX, float scaleY,AnimationCurve curve) {
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
    public static int getCurveHash(AnimationCurve val) {
        int rlt = 0;
        AnimationCurve c = (AnimationCurve)val;
        foreach (Keyframe k in c.keys) {
            rlt ^= (k.inTangent * 1.122112 + k.outTangent * 2.123123 + k.tangentMode * 0.1324123 + k.time * 4.2343113 + k.value * 2.34233).GetHashCode();
        }
        return rlt;
    }
    public static void addCache(string key, float[,] data) {
        cachedDatas.Add(key, data);
    }
    public static bool hasKey(string key) {
        return cachedDatas.ContainsKey(key);
    }
    public static float[,] getCache(string key) {
        if (cachedDatas.ContainsKey(key)) {
            return cachedDatas[key];
        }
        return null;
    }
}

[Serializable]
//for xml serialize
public class NodeWrapper:IXmlSerializable
{
    public NodeBase value;
    public NodeWrapper() {

    }
    public NodeWrapper(NodeBase node) {
        value = node;
    }
    System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema() {
        return null;
    }
    void IXmlSerializable.ReadXml(XmlReader reader) {
        string rootName = reader.Name;
        string fullTypeName = string.Empty;
        string shortTypeName = string.Empty;
        XmlSerializer serial = null;

        while (reader.Read()) {
            if (reader.NodeType == XmlNodeType.Element) {
                if (reader.Name == "value") {
                    fullTypeName = reader.GetAttribute("type");
                    serial = new XmlSerializer(System.Type.GetType(fullTypeName));
                    shortTypeName = System.Type.GetType(fullTypeName).Name;
                }
                if (fullTypeName != string.Empty) {
                    if (reader.Name == shortTypeName) {
                        if (serial != null)
                            value = (NodeBase)serial.Deserialize(reader);
                    }
                }
            }
            if (reader.NodeType == XmlNodeType.EndElement) {
                if (reader.Name == rootName) {
                    reader.ReadEndElement();
                    break;
                }
            }
        }
    }

    void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer) {
        writer.WriteStartElement("value");
        string strType = value.GetType().FullName;
        writer.WriteAttributeString("type", strType);
        XmlSerializer serial = new XmlSerializer(System.Type.GetType(strType));
        serial.Serialize(writer, value);
        writer.WriteEndElement();
    }
}

public class NodeConst : NodeBase
{
    public float value = 1.0f;
    public bool bPublic = false;
    private float[] texture;
    public override NodeType getNodeType() {
        return NodeType.Generator;
    }
    public override GeneratorType getGeneratorType() {
        return GeneratorType.Const_Value;
    }
    public override string getDefaultName() {
        return "generater";
    }
    public override void OnWindowGUI() {
        GUIStyle style = new GUIStyle();style.normal.textColor = Color.green;
        EditorGUILayout.LabelField("Const_Value", style);
    }
    public override void OnGUI() {
        label = EditorGUILayout.TextField("name", label);
        value = EditorGUILayout.Slider("value",value, 0f, 1f);
        bPublic = EditorGUILayout.Toggle("public", bPublic);
    }
    public override void OnMainGUI() {
        if (bPublic) {
            value = EditorGUILayout.Slider("value of " + label, value, 0f, 1f);
        }
    }
    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f) {
        float[,] values = new float[w,h];
        for (int i = 0; i < w; i++) {
            for (int j = 0; j < h; j++) {
                values[i,j] = value;
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
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1,1);

    private float[] texture;
    LibNoise.Unity.Generator.Perlin generator = new LibNoise.Unity.Generator.Perlin();
    public override NodeType getNodeType() {
        return NodeType.Generator;
    }
    public override string getDefaultName() {
        return "generater";
    }
    public override void OnWindowGUI() {
        GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;
        EditorGUILayout.LabelField("perlin noise", style);
    }
    public override void OnGUI() {
        label = EditorGUILayout.TextField("name", label);
        scale = EditorGUILayout.Slider("scale", scale,0f,10f);
        bias = EditorGUILayout.Slider("bias", bias,-5f,5f);
        localSeed = EditorGUILayout.IntField("local seed", localSeed);
        size = EditorGUILayout.FloatField("size",size);
        octaveCount = EditorGUILayout.IntSlider("octaveCount",octaveCount,1,(int)Mathf.Log(size,2)+2);
        curve = EditorGUILayout.CurveField("curve", curve);
    }
    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f) {
        generator.Frequency = 1.0/size;
        generator.OctaveCount = 1;
        generator.Seed = seed + localSeed;
        generator.Quality = LibNoise.Unity.QualityMode.High;

        string key = PerlinCache.makeKey("perlin", generator.Frequency, generator.Seed,octaveCount, x, y, w, h, scaleX, scaleY, curve);
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
                        tempX *= 1.83456789f;
                        tempY *= 1.83456789f;
                        cp *= 0.5f;
                    }
                    temp[i, j] = (val * 0.5f + 0.5f);
                }
            }
            PerlinCache.addCache(key, temp);
        }
        float[,] values = new float[w,h];
        for (int i = 0; i < w; i++) {
            for (int j = 0; j < h; j++) {
                values[i, j] = temp[i, j] * scale + bias;
            }
        }        
        return values;
    }
    public override GeneratorType getGeneratorType() {
        return GeneratorType.Noise;
    }
}

public class NodeImport : NodeBase
{
    public float scale = 1.0f;
    public float bias = 0f;

    private Texture2D texture;
    public Texture2D Texture { get { return texture; } }
    public string texturePath;
    private Texture2D textureReadable;

    public override NodeType getNodeType()
    {
        return NodeType.Generator;
    }
    public override string getDefaultName()
    {
        return "import";
    }
    public override void postLoaded()
    {
        texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
        textureReadable = LoadTexture(texturePath, texture.width, texture.height);
    }
    public override void OnWindowGUI()
    {
        GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;
        EditorGUILayout.LabelField("import", style);
    }
    public override void OnGUI()
    {
        label = EditorGUILayout.TextField("name", label);
        scale = EditorGUILayout.Slider("scale", scale, 0f, 10f);
        bias = EditorGUILayout.Slider("bias", bias, -5f, 5f);
        texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), true, GUILayout.Width(64), GUILayout.Height(64));
        string newTexturePath = AssetDatabase.GetAssetPath(texture);
        if(newTexturePath != texturePath) {
            texturePath = newTexturePath;
            textureReadable = LoadTexture(texturePath, texture.width,texture.height);
        }
    }

    private static Texture2D LoadTexture(string filepath, int w,int h)
    {
        Texture2D texture = new Texture2D(w, h,TextureFormat.ARGB32,false);
        byte[] array = null;
        try {
            array = File.ReadAllBytes(filepath);
        }
        catch {
        }
        if (filepath == string.Empty || array == null || !texture.LoadImage(array)) {
            Color[] pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++) {
                pixels[i] = new Color(0,0,0,0);
            }
            texture.SetPixels(pixels);
            texture.Apply();
        }
        return texture;
    }

    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
    {
        float[,] values = new float[w, h];
        if (textureReadable != null) {
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    float u = i / (float)w;
                    float v = j / (float)h;
                    values[i, j] = textureReadable.GetPixelBilinear(u, v).r * scale + bias;
                }
            }
        }
        return values;
    }
    public override GeneratorType getGeneratorType()
    {
        return GeneratorType.Import;
    }
}

//public class GeneratorRigged : NodeBase
//{
//    public float scale = 1.0f;
//    public float bias = 0f;
//    public float size = 100f;
//    public int octaveCount = 4;
//    public int localSeed = 0;
//    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

//    LibNoise.Unity.Generator.RiggedMultifractal generator = new LibNoise.Unity.Generator.RiggedMultifractal();
//    public override NodeType getNodeType() {
//        return NodeType.Generator;
//    }
//    public override string getDefaultName() {
//        return "generater";
//    }
//    public override void OnWindowGUI() {
//        GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;
//        EditorGUILayout.LabelField("rigged", style);
//    }
//    public override void OnGUI() {
//        label = EditorGUILayout.TextField("name", label);
//        scale = EditorGUILayout.Slider("scale", scale, 0f, 10f);
//        bias = EditorGUILayout.Slider("bias", bias, -5f, 5f);
//        localSeed = EditorGUILayout.IntField("local seed", localSeed);
//        size = EditorGUILayout.FloatField("size", size);
//        octaveCount = EditorGUILayout.IntSlider("octaveCount", octaveCount, 1, (int)Mathf.Log(size, 2) + 2);
//        curve = EditorGUILayout.CurveField("curve", curve);
//    }
//    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f) {
//        generator.Frequency = 1.0 / size;
//        generator.OctaveCount = 1;
//        generator.Seed = seed + localSeed;

//        string key = PerlinCache.makeKey("rigged", generator.Frequency, generator.Seed,octaveCount, x, y, w, h, scaleX, scaleY, curve);
//        float[,] values = null;
//        if (PerlinCache.hasKey(key)) {
//            values = PerlinCache.getCache(key);
//        }
//        else {
//            values = new float[w, h];
//            for (int i = 0; i < w; i++) {
//                for (int j = 0; j < h; j++) {
//                    float tempX = x + i;
//                    float tempY = y + j;
//                    float val = 0f;
//                    float cp = 0.5f;
//                    for (int o = 0; o < octaveCount; o++) {
//                        float signal = (float)generator.GetValue(tempX * 0.987654321 * scaleX, tempY * 0.987654321 * scaleY, 0);
//                        val += (curve.Evaluate(signal * 0.5f + 0.5f) - 0.5f) * 2f * cp;
//                        tempX *= 2.0123456789f;
//                        tempY *= 2.0123456789f;
//                        cp *= 0.5f;
//                    }
//                    values[i, j] = (val * 0.5f + 0.5f);
//                }
//            }
//            PerlinCache.addCache(key, values);
//        }

//        for (int i = 0; i < w; i++) {
//            for (int j = 0; j < h; j++) {
//                values[i, j] = values[i, j] * scale + bias;
//            }
//        }
//        return values;
//    }
//    public override GeneratorType getGeneratorType() {
//        return GeneratorType.Rigged;
//    }
//}

public class NodeCurve:NodeBase
{
    public float scale = 1.0f;
    public float bias = 0f;
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
    public override NodeType getNodeType() {
        return NodeType.UnaryOperator;
    }
    public override UnaryOperatorType getUnaryOperatorType() {
        return UnaryOperatorType.Curve;
    }
    public override string getDefaultName() {
        return "curve";
    }
    public override void OnWindowGUI() {
        //GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;
        //EditorGUILayout.LabelField("curve", style);
        Vector3[] points = new Vector3[41];
        for (int i = 0; i < 41; i++) {
            float x0 = i / 40.0f;
            float y0 = curve.Evaluate(x0);
            points[i].Set(x0 * 128,128 + 12 - y0 * 120,0);
        }
        Handles.color = Color.green;
        Handles.DrawAAPolyLine(points);
    }
    public override void OnGUI() {
        label = EditorGUILayout.TextField("name", label);
        scale = EditorGUILayout.Slider("scale", scale, 0f, 10f);
        bias = EditorGUILayout.Slider("bias", bias, -5f, 5f);
        curve = EditorGUILayout.CurveField("curve", curve);
    }
    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f) {
        float[,] values = new float[w, h];
        if (getInputNode(0) != null) {
            float[,] a = getInputNode(0).update(seed, x, y, w, h,scaleX,scaleY);
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    values[i, j] = curve.Evaluate(a[i,j]);
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
    public override int getInputNum() {
        return 1;
    }
    public override string getInputName(int index) {
        return "i";
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
    public override NodeType getNodeType() {
        return NodeType.UnaryOperator;
    }
    public override UnaryOperatorType getUnaryOperatorType() {
        return UnaryOperatorType.Normal;
    }
    public override string getDefaultName() {
        return "normal";
    }
    public override void OnWindowGUI() {
        GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;
        string[] ss = { "normal.x", "normal.y", "normal.z" };
        EditorGUILayout.LabelField(ss[(int)normalType],style);
    }
    public override void OnGUI() {
        label = EditorGUILayout.TextField("name", label);
        scale = EditorGUILayout.Slider("scale", scale, 0f, 10f);
        bias = EditorGUILayout.Slider("bias", bias, -5f, 5f);
        normalType = (NormalType)EditorGUILayout.EnumPopup("normal type", normalType);
        height = EditorGUILayout.FloatField("height", height);
    }
    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f) {
        float[,] values = new float[w, h];
        if (getInputNode(0) != null) {
            float[,] a = getInputNode(0).update(seed, x, y, w+1, h+1, scaleX, scaleY);
            Vector3[,] normal = new Vector3[w,h];
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    Vector3 pos_x = new Vector3(1, (a[i + 1, j] - a[i,j]) * height, 0);
                    Vector3 pos_z = new Vector3(0, (a[i, j + 1] - a[i,j]) * height, 1);
                    normal[i,j] = Vector3.Cross(pos_x,-pos_z).normalized;
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
    public override int getInputNum() {
        return 1;
    }
    public override string getInputName(int index) {
        return "i";
    }
}

public class NodeErosion : NodeBase
{
    public float scale = 1.0f;
    public float bias = 0f;
    public override NodeType getNodeType() {
        return NodeType.UnaryOperator;
    }
    public override UnaryOperatorType getUnaryOperatorType() {
        return UnaryOperatorType.Erosion;
    }
    public override string getDefaultName() {
        return "erosion";
    }
    public override void OnWindowGUI() {
        GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;
        EditorGUILayout.LabelField("erosion", style);
    }
    public override void OnGUI() {
        label = EditorGUILayout.TextField("name", label);
        scale = EditorGUILayout.Slider("scale", scale, 0f, 10f);
        bias = EditorGUILayout.Slider("bias", bias, -5f, 5f);
    }
    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f) {
        float[,] values = new float[w, h];
        if (getInputNode(0) != null) {
            float[,] heights = getInputNode(0).update(seed, x, y, w + 1, h + 1, scaleX, scaleY);
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
    public override int getInputNum() {
        return 1;
    }
    public override string getInputName(int index) {
        return "i";
    }
}
public class NodeBinaryOperator : NodeBase
{
    public float scale = 1.0f;
    public float bias = 0f;
    public BinaryOperatorType operatorType;
    public override NodeType getNodeType() {
        return NodeType.BinaryOperator;
    }
    public override BinaryOperatorType getBinaryOperatorType() {
        return operatorType;
    }
    public override string getDefaultName() {
        return "operator";
    }
    public override void OnWindowGUI() {
        GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;
        if (operatorType == BinaryOperatorType.Add) {
            EditorGUILayout.LabelField(String.Format("o = a + b"),style);
        }
        else if (operatorType == BinaryOperatorType.Sub) {
            EditorGUILayout.LabelField(String.Format("o = a - b"), style);
        }
        else if (operatorType == BinaryOperatorType.Mul) {
            EditorGUILayout.LabelField(String.Format("o = a * b"), style);
        }
        else if (operatorType == BinaryOperatorType.Max) {
            EditorGUILayout.LabelField("o = max(a,b)",style);
        }
    }
    public override void OnGUI() {
        label = EditorGUILayout.TextField("name", label);
        scale = EditorGUILayout.Slider("scale", scale, 0f, 10f);
        bias = EditorGUILayout.Slider("bias", bias, -5f, 5f);
    }
    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f) {
        float[,] values = new float[w, h];
        if (operatorType == BinaryOperatorType.Add) {
            if (getInputNode(0) != null && getInputNode(1) != null) {
                float[,] a = getInputNode(0).update(seed, x, y, w, h,scaleX,scaleY);
                float[,] b = getInputNode(1).update(seed, x, y, w, h,scaleX,scaleY);
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        values[i, j] = a[i, j] + b[i, j];
                    }
                }
            }
        }
        else if (operatorType == BinaryOperatorType.Sub) {
            if (getInputNode(0) != null && getInputNode(1) != null) {
                float[,] a = getInputNode(0).update(seed, x, y, w, h, scaleX, scaleY);
                float[,] b = getInputNode(1).update(seed, x, y, w, h, scaleX, scaleY);
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        values[i, j] = a[i, j] - b[i, j];
                    }
                }
            }
        }
        else if (operatorType == BinaryOperatorType.Mul) {
            if (getInputNode(0) != null && getInputNode(1) != null) {
                float[,] a = getInputNode(0).update(seed, x, y, w, h, scaleX, scaleY);
                float[,] b = getInputNode(1).update(seed, x, y, w, h, scaleX, scaleY);
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        values[i, j] = a[i, j] * b[i, j];
                    }
                }
            }
        }
        else if (operatorType == BinaryOperatorType.Max) {
            if (getInputNode(0) != null && getInputNode(1) != null) {
                float[,] a = getInputNode(0).update(seed, x, y, w, h, scaleX, scaleY);
                float[,] b = getInputNode(1).update(seed, x, y, w, h, scaleX, scaleY);
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
    public override int getInputNum() {
        return 2;
    }
    public override string getInputName(int index) {
        return index == 0 ? "a" : "b";
    }
}
public class NodeTernaryOperator : NodeBase
{
    public float scale = 1.0f;
    public float bias = 0f;
    public TernaryOperatorType operatorType;
    public override NodeType getNodeType() {
        return NodeType.TernaryOperator;
    }
    public override TernaryOperatorType getTernaryOperatorType() {
        return operatorType;
    }
    public override string getDefaultName() {
        return "operator";
    }
    public override void OnWindowGUI() {
        GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;
        EditorGUILayout.LabelField("lerp", style);
        if (operatorType == TernaryOperatorType.Lerp) {
            EditorGUILayout.LabelField("o = a*(1-s) + b*s", style);
        }
    }
    public override void OnGUI() {
        label = EditorGUILayout.TextField("name", label);
        scale = EditorGUILayout.Slider("scale", scale, 0f, 10f);
        bias = EditorGUILayout.Slider("bias", bias, -5f, 5f);
    }
    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f) {
        float[,] values = new float[w, h];
        if (operatorType == TernaryOperatorType.Lerp) {
            if (getInputNode(0) != null && getInputNode(1) != null && getInputNode(2) != null) {
                float[,] a = getInputNode(0).update(seed, x, y, w, h, scaleX, scaleY);
                float[,] s = getInputNode(1).update(seed, x, y, w, h, scaleX, scaleY);
                float[,] b = getInputNode(2).update(seed, x, y, w, h, scaleX, scaleY);
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
    public override int getInputNum() {
        return 3;
    }
    public override string getInputName(int index) {
        return index == 1 ? "s":(index == 0 ? "a" : "b");
    }
}
public class HeightOutput : NodeBase
{
    public float scale = 1.0f;
    public float bias = 0f;
    public override NodeType getNodeType() {
        return NodeType.HeightOutput;
    }
    public override string getDefaultName() {
        return "height";
    }
    public override void OnWindowGUI() {
        GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;
        EditorGUILayout.LabelField("height output", style);
    }
    public override void OnGUI() {
        label = EditorGUILayout.TextField("name", label);
        scale = EditorGUILayout.Slider("scale", scale, 0f, 10f);
        bias = EditorGUILayout.Slider("bias", bias, -5f, 5f);
    }
    public override void OnMainGUI() {
        scale = EditorGUILayout.Slider("scale for output", scale, 0f, 10f);
    }
    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f) {
        float[,] values = null;
        if (getInputNode(0) != null) {
            values = getInputNode(0).update(seed, x, y, w, h, scaleX, scaleY);
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
    public override int getInputNum() {
        return 1;
    }
}

public class TextureOutput : NodeBase
{
    public float scale = 1.0f;
    public float bias = 0f;
    public int paintOrder = 0;
    public int textureIndex = 0;
    private Texture2D texture;
    public Texture2D Texture {get{return texture;}}
    public string texturePath;
    private Texture2D normal;
    public int texSizeX = 15;
    public int texSizeY = 15;
    public Texture2D Normal { get { return normal; } }
    public string normalPath;
    private bool bShowInMain = true;
    public override NodeType getNodeType() {
        return NodeType.TextureOutput;
    }
    public override string getDefaultName() {
        return "texture";
    }
    public override void beforeSave() {
        //texturePath = AssetDatabase.GetAssetPath(texture);
    }
    public override void postLoaded() {
        texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
    }
    public override void OnWindowGUI() {
        GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;
        //EditorGUILayout.LabelField("texture:" + (texture != null ? texture.name : "null"));
        EditorGUILayout.LabelField("texture:" + paintOrder.ToString(), style);
    }
    private void doTextureGUI() {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("", GUILayout.Width(10));
            EditorGUILayout.LabelField("texture", GUILayout.Width(64));
            EditorGUILayout.LabelField("normal", GUILayout.Width(64));
            EditorGUILayout.LabelField("size");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("", GUILayout.Width(10));
            texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), true, GUILayout.Width(64), GUILayout.Height(64));
            texturePath = AssetDatabase.GetAssetPath(texture);
            normal = (Texture2D)EditorGUILayout.ObjectField(normal, typeof(Texture2D), true, GUILayout.Width(64), GUILayout.Height(64));
            normalPath = AssetDatabase.GetAssetPath(normal);
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("size x:", GUILayout.Width(40));
                    texSizeX = EditorGUILayout.IntField(texSizeX);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("size y:", GUILayout.Width(40));
                    texSizeY = EditorGUILayout.IntField(texSizeY);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }
    public override void OnGUI() {
        label = EditorGUILayout.TextField("name", label);
        scale = EditorGUILayout.Slider("scale", scale, 0f, 10f);
        bias = EditorGUILayout.Slider("bias", bias, -5f, 5f);
        paintOrder = EditorGUILayout.IntField("paint order", paintOrder);
        //texture = (Texture2D)EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), true);
        //texturePath = AssetDatabase.GetAssetPath(texture);
        doTextureGUI();
    }
    public override void OnMainGUI() {
        GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);
        myFoldoutStyle.fontStyle = FontStyle.Bold;
        bShowInMain = EditorGUILayout.Foldout(bShowInMain, label, myFoldoutStyle);
        if (bShowInMain) {
            doTextureGUI();
        }
    }
    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f) {
        float[,] values = null;
        if (getInputNode(0) != null) {
            values = getInputNode(0).update(seed, x, y, w, h, scaleX, scaleY);
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
    public override int getInputNum() {
        return 1;
    }
}

public class GrassOutput : NodeBase
{
    public float scale = 1.0f;
    public float bias = 0f;
    public int grassIndex = 0;
    private Texture2D texture;
    public string texturePath;
    public float minSize = 1.0f;
    public float maxSize = 1.0f;
    public bool bBillboard = true;
    private bool bShowInMain = true;
    public Texture2D Texture { get { return texture; } }
  
    public override NodeType getNodeType() {
        return NodeType.GrassOutput;
    }
    public override string getDefaultName() {
        return "grass";
    }
    public override void postLoaded() {
        texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
    }
    public override void OnWindowGUI() {
        GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;
        EditorGUILayout.LabelField("grass", style);
    }
    private void doTextureGUI() {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("", GUILayout.Width(10));
            EditorGUILayout.LabelField("texture", GUILayout.Width(64));
            EditorGUILayout.LabelField("");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("", GUILayout.Width(10));
            texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), true, GUILayout.Width(64), GUILayout.Height(64));
            texturePath = AssetDatabase.GetAssetPath(texture);
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("min size:", GUILayout.Width(60));
                    minSize = EditorGUILayout.FloatField(minSize);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("max size:", GUILayout.Width(60));
                    maxSize = EditorGUILayout.FloatField(maxSize);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("billboard:", GUILayout.Width(60));
                    bBillboard = EditorGUILayout.Toggle(bBillboard);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }
    public override void OnGUI() {
        label = EditorGUILayout.TextField("name", label);
        scale = EditorGUILayout.Slider("scale", scale, 0f, 10f);
        bias = EditorGUILayout.Slider("bias", bias, -5f, 5f);
        //grassIndex = EditorGUILayout.IntField("grass index", grassIndex);
        texture = (Texture2D)EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), true);
        texturePath = AssetDatabase.GetAssetPath(texture);
        doTextureGUI();
    }
    public override void OnMainGUI() {
        //grassIndex = EditorGUILayout.IntField("index of " + label, grassIndex);
        //texture = (Texture2D)EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), true);
        //texturePath = AssetDatabase.GetAssetPath(texture);
        GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);
        myFoldoutStyle.fontStyle = FontStyle.Bold;
        bShowInMain = EditorGUILayout.Foldout(bShowInMain, label, myFoldoutStyle);
        if (bShowInMain) {
            doTextureGUI();
        }
    }
    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f) {
        float[,] values = null;
        if (getInputNode(0) != null) {
            values = getInputNode(0).update(seed, x, y, w, h, scaleX, scaleY);
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
    public override int getInputNum() {
        return 1;
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
    private GameObject objTree;
    public GameObject ObjTree { get { return objTree; } }
    public string treePath;
    public bool bEntity = false;
    private bool bShowInMain = true;
    public override NodeType getNodeType() {
        return NodeType.TreeOutput;
    }
    public override string getDefaultName() {
        return "tree";
    }
    public override void postLoaded() {
        objTree = (GameObject)AssetDatabase.LoadAssetAtPath(treePath, typeof(GameObject));
    }
    private void doTreeGUI(bool bIndent)
    {
        EditorGUILayout.BeginHorizontal();
        {
            if(bIndent){
                EditorGUILayout.LabelField("", GUILayout.Width(10));
            }
            EditorGUILayout.BeginVertical();
            {
                objTree = (GameObject)EditorGUILayout.ObjectField("tree", objTree, typeof(GameObject), true);
                EditorGUILayout.MinMaxSlider(new GUIContent("size(" + minSize.ToString("0.00") + "~" + maxSize.ToString("0.00") + ")"), ref minSize, ref maxSize, 0.1f, 3f);
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        treePath = AssetDatabase.GetAssetPath(objTree);

        EditorGUILayout.BeginHorizontal();
        {
            if (bIndent) {
                EditorGUILayout.LabelField("", GUILayout.Width(10));
            }
            bEntity = EditorGUILayout.Toggle("Is Entity", bEntity);
        }
        EditorGUILayout.EndHorizontal();
    }
    public override void OnGUI() {
        label = EditorGUILayout.TextField("name", label);
        scale = EditorGUILayout.Slider("scale", scale, 0f, 10f);
        bias = EditorGUILayout.Slider("bias", bias, -5f, 5f);
        //treeIndex = EditorGUILayout.IntField("tree index", treeIndex);
        //texture = (Texture2D)EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), true);
        doTreeGUI(false);
        density = EditorGUILayout.FloatField("density", density);
    }
    public override void OnWindowGUI() {
        GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;
        EditorGUILayout.LabelField("tree:" + treeIndex.ToString(), style);
    }
    public override void OnMainGUI() {
        //treeIndex = EditorGUILayout.IntField("index of " + label, treeIndex);
        //texture = (Texture2D)EditorGUILayout.ObjectField(name, texture, typeof(Texture2D), true);
        GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);
        myFoldoutStyle.fontStyle = FontStyle.Bold;
        bShowInMain = EditorGUILayout.Foldout(bShowInMain, label, myFoldoutStyle);
        if (bShowInMain) {
            doTreeGUI(true);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("", GUILayout.Width(10));
                density = EditorGUILayout.FloatField("density of " + label, density);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f) {
        float[,] values = null;
        if (getInputNode(0) != null) {
            values = getInputNode(0).update(seed, x, y, w, h, scaleX, scaleY);
        }
        else {
            values = new float[w, h];
        }
        for (int i = 0; i < w; i++) {
            for (int j = 0; j < h; j++) {
                values[i, j] = Mathf.Clamp(values[i, j] * scale + bias,0f,1f);
            }
        }
        return values;
    }
    public override int getInputNum() {
        return 1;
    }
    private static long getHash(long a) {
        a = (a ^ 61) ^ (a >> 16);
        a = a + (a << 3);
        a = a ^ (a >> 4);
        a = a * 0x27d4eb2d;
        a = a ^ (a >> 15);
        return a;
    }
    public static int getTreeNum(int x,int y,float val,float density,int layer) {
        long hashCode = getHash(x*123456789+y + layer*1234567);
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
    public static Vector2 getTreePos(int x, int y, int index, float maxOffset, int layer) {
        long hashCode = getHash(x * 123456789 + y + index*123456 + layer * 12345678);
        hashCode = getHash(hashCode);
        hashCode = getHash(hashCode);
        hashCode = getHash(hashCode);
        float randX = (hashCode & 0xffffffff) / (float)0xffffffff;
        float randY = ((hashCode>>32) & 0xffffffff) / (float)0xffffffff;
        Vector2 rlt = new Vector2((randX-0.5f) * maxOffset, (randY-0.5f) * maxOffset);
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