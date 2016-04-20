using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System;

internal class TerrainWizard : ScriptableWizard
{
    internal const int kMaxResolution = 4097;

    //protected Terrain m_Terrain;

    //protected TerrainData terrainData {
    //    get {
    //        if (this.m_Terrain != null) {
    //            return this.m_Terrain.terrainData;
    //        }
    //        return null;
    //    }
    //}

    internal virtual void OnWizardUpdate()
    {
        base.isValid = true;
        base.errorString = string.Empty;
        //if (this.m_Terrain == null || this.m_Terrain.terrainData == null) {
        //    base.isValid = false;
        //    base.errorString = "Terrain does not exist";
        //}
    }

    internal void InitializeDefaults(Terrain terrain)
    {
        //this.m_Terrain = terrain;
        this.OnWizardUpdate();
    }

    //internal void FlushHeightmapModification()
    //{
    //    //this.m_Terrain.Flush();
    //}

    internal static T DisplayTerrainWizard<T>(string title, string button) where T : TerrainWizard
    {
        T[] array = Resources.FindObjectsOfTypeAll<T>();
        if (array.Length > 0) {
            T result = array[0];
            result.titleContent = new GUIContent(title);
            result.createButtonName = button;
            result.otherButtonName = string.Empty;
            result.Focus();
            return result;
        }
        return ScriptableWizard.DisplayWizard<T>(title, button);
    }
}

class ImportRawHeightmap : TerrainWizard
{
    internal enum Depth
    {
        Bit8 = 1,
        Bit16
    }

    internal enum ByteOrder
    {
        Mac = 1,
        Windows
    }

    public ImportRawHeightmap.Depth m_Depth = ImportRawHeightmap.Depth.Bit16;

    public int m_Width = 1;

    public int m_Height = 1;

    public ImportRawHeightmap.ByteOrder m_ByteOrder = ImportRawHeightmap.ByteOrder.Windows;

    public bool m_FlipVertically;

    //public Vector3 m_TerrainSize = new Vector3(2000f, 600f, 2000f);

    private string m_Path;
    private Action<float[,]> funUpdate;

    private void PickRawDefaults(string path)
    {
        FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read);
        int num = (int)fileStream.Length;
        fileStream.Close();
        //this.m_TerrainSize = base.terrainData.size;
        //if (base.terrainData.heightmapWidth * base.terrainData.heightmapHeight == num) {
        //    this.m_Width = base.terrainData.heightmapWidth;
        //    this.m_Height = base.terrainData.heightmapHeight;
        //    this.m_Depth = ImportRawHeightmap.Depth.Bit8;
        //}
        //else if (base.terrainData.heightmapWidth * base.terrainData.heightmapHeight * 2 == num) {
        //    this.m_Width = base.terrainData.heightmapWidth;
        //    this.m_Height = base.terrainData.heightmapHeight;
        //    this.m_Depth = ImportRawHeightmap.Depth.Bit16;
        //}
        //else 
        {
            this.m_Depth = ImportRawHeightmap.Depth.Bit16;
            int num2 = num / (int)this.m_Depth;
            int num3 = Mathf.RoundToInt(Mathf.Sqrt((float)num2));
            int num4 = Mathf.RoundToInt(Mathf.Sqrt((float)num2));
            if (num3 * num4 * (int)this.m_Depth == num) {
                this.m_Width = num3;
                this.m_Height = num4;
                return;
            }
            this.m_Depth = ImportRawHeightmap.Depth.Bit8;
            num2 = num / (int)this.m_Depth;
            num3 = Mathf.RoundToInt(Mathf.Sqrt((float)num2));
            num4 = Mathf.RoundToInt(Mathf.Sqrt((float)num2));
            if (num3 * num4 * (int)this.m_Depth == num) {
                this.m_Width = num3;
                this.m_Height = num4;
                return;
            }
            this.m_Depth = ImportRawHeightmap.Depth.Bit16;
        }
    }

    internal void OnWizardCreate()
    {
        //if (this.m_Terrain == null) {
        //    base.isValid = false;
        //    base.errorString = "Terrain does not exist";
        //}
        if (this.m_Width > 4097 || this.m_Height > 4097) {
            base.isValid = false;
            base.errorString = "Heightmaps above 4097x4097 in resolution are not supported";
            Debug.LogError(base.errorString);
        }
        if (File.Exists(this.m_Path) && base.isValid) {
            //Undo.RegisterCompleteObjectUndo(base.terrainData, "Import Raw heightmap");
            //base.terrainData.heightmapResolution = Mathf.Max(this.m_Width, this.m_Height);
            //base.terrainData.size = this.m_TerrainSize;
            this.ReadRaw(this.m_Path);
            //base.FlushHeightmapModification();
        }
    }

    private void ReadRaw(string path)
    {
        byte[] array;
        using (BinaryReader binaryReader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read))) {
            array = binaryReader.ReadBytes(this.m_Width * this.m_Height * (int)this.m_Depth);
            binaryReader.Close();
        }
        int heightmapWidth = this.m_Width;
        int heightmapHeight = this.m_Height;
        float[,] array2 = new float[heightmapHeight, heightmapWidth];
        if (this.m_Depth == ImportRawHeightmap.Depth.Bit16) {
            float num = 1.52587891E-05f;
            for (int i = 0; i < heightmapHeight; i++) {
                for (int j = 0; j < heightmapWidth; j++) {
                    int num2 = Mathf.Clamp(j, 0, this.m_Width - 1) + Mathf.Clamp(i, 0, this.m_Height - 1) * this.m_Width;
                    if (this.m_ByteOrder == ImportRawHeightmap.ByteOrder.Mac == BitConverter.IsLittleEndian) {
                        byte b = array[num2 * 2];
                        array[num2 * 2] = array[num2 * 2 + 1];
                        array[num2 * 2 + 1] = b;
                    }
                    ushort num3 = BitConverter.ToUInt16(array, num2 * 2);
                    float num4 = (float)num3 * num;
                    int num5 = (!this.m_FlipVertically) ? i : (heightmapHeight - 1 - i);
                    array2[num5, j] = num4;
                }
            }
        }
        else {
            float num6 = 0.00390625f;
            for (int k = 0; k < heightmapHeight; k++) {
                for (int l = 0; l < heightmapWidth; l++) {
                    int num7 = Mathf.Clamp(l, 0, this.m_Width - 1) + Mathf.Clamp(k, 0, this.m_Height - 1) * this.m_Width;
                    byte b2 = array[num7];
                    float num8 = (float)b2 * num6;
                    int num9 = (!this.m_FlipVertically) ? k : (heightmapHeight - 1 - k);
                    array2[num9, l] = num8;
                }
            }
        }
        if(funUpdate != null) {
            funUpdate(array2);
        }
        //base.terrainData.SetHeights(0, 0, array2);
    }

    internal void InitializeImportRaw(string path,Action<float[,]>funUpdate)
    {
        this.funUpdate = funUpdate;
        this.m_Path = path;
        this.PickRawDefaults(this.m_Path);
        base.helpString = "Raw files must use a single channel and be either 8 or 16 bit.";
        this.OnWizardUpdate();
    }
}

[CustomPropertyDrawer(typeof(RawFileData))]
public class RawFileDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label(label);
        RawFileData data = null;
        if (property.objectReferenceValue is RawFileData) {
            data = property.objectReferenceValue as RawFileData;
        }
        if (data != null && data.previewTexture != null) {
            GUILayout.Box(data.previewTexture, GUILayout.Width(64), GUILayout.Height(64));
        }
        GUILayout.BeginVertical();
        if (GUILayout.Button("Import...")) {
            string text = EditorUtility.OpenFilePanel("Import Raw Heightmap", string.Empty, "raw");
            if (text != string.Empty) {
                ImportRawHeightmap importRawHeightmap = TerrainWizard.DisplayTerrainWizard<ImportRawHeightmap>("Import Heightmap", "Import");
                importRawHeightmap.InitializeImportRaw(text, (float[,] values) => { UpdateTexture(data, values); });
            }
        }
        if(data != null && data.previewTexture != null) {
            if (GUILayout.Button("Clear")) {
                Clear(data);
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
    private void UpdateTexture(RawFileData data,float[,]values)
    {
        Undo.RecordObject(data,"select raw file");
        int w = values.GetLength(0);
        int h = values.GetLength(1);
        Texture2D tex = new Texture2D(64,64);
        Color[] colors = new Color[tex.width * tex.height];
        for(int i = 0; i < tex.width; i++) {
            for(int j = 0; j < tex.height; j++) {
                float value = values[i * w / tex.width, j * h / tex.height];
                Color color = new Color();
                color.a = 1;
                color.r = color.g = color.b = value;
                colors[j * tex.width + i] = color;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        data.previewTexture = tex;
        data.width = w;
        data.height = h;
        data.values = new float[w * h];
        for(int i =0; i<data.width; i++) {
            for(int j = 0; j < data.height; j++) {
                data.values[j*data.width + i] = values[i,j];
            }
        }
    }
    private void Clear(RawFileData data)
    {
        Undo.RecordObject(data, "clear raw data");
        data.width = 0;
        data.height = 0;
        data.previewTexture = null;
        data.values = null;
    }
}
