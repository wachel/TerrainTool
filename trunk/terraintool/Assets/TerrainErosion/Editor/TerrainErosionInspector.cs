using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Reflection;

[CustomEditor(typeof(TerrainErosion))]
public class TerrainErosionInspector : Editor
{
    Projector globalProjector;
    TerrainErosion terrainErosion;
    private static GUIStyle ToggleButtonStyleNormal = null;
    private static GUIStyle ToggleButtonStyleToggled = null;

    bool painting = false;

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        if (ToggleButtonStyleNormal == null) {
            ToggleButtonStyleNormal = new GUIStyle("Button");
            ToggleButtonStyleToggled = new GUIStyle("Button");
            ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Local Erosion", terrainErosion.editType == ErosionEditType.Local? ToggleButtonStyleToggled:ToggleButtonStyleNormal)) {
            terrainErosion.editType = ErosionEditType.Local;
        }
        if (GUILayout.Button("Global Erosion", terrainErosion.editType == ErosionEditType.Global? ToggleButtonStyleToggled:ToggleButtonStyleNormal)) {
            terrainErosion.editType = ErosionEditType.Global;
        }
        GUILayout.EndHorizontal();

        if (terrainErosion.editType == ErosionEditType.Local) {
            terrainErosion.rainPointSpeed = EditorGUILayout.Slider("Rain Density",terrainErosion.rainPointSpeed,0,10);//DrawProperty("Rain Density",terrainErosion.rainPointSpeed);
            terrainErosion.rainPointSize = EditorGUILayout.Slider("Rain Size", terrainErosion.rainPointSize, 0.001f, 0.1f);
            terrainErosion.rainHeight = EditorGUILayout.Slider("Rain Height", terrainErosion.rainHeight, 0.001f, 0.1f);
            terrainErosion.evaporateSpeed = EditorGUILayout.Slider("Evaporate Speed", terrainErosion.evaporateSpeed, 0, 0.01f);
        }

        if (GUILayout.Button("生成")) {
            terrainErosion.StartErosion();
            globalProjector = CreatePreviewProjector();
            globalProjector.enabled = true;
            globalProjector.material.mainTexture = terrainErosion.height_a;
            globalProjector.material.SetFloat("_Scale", 500);
            globalProjector.orthographicSize = terrainErosion.terrain.terrainData.size.x / 2;
            globalProjector.transform.position = terrainErosion.terrain.transform.position + terrainErosion.terrain.terrainData.size / 2 + Vector3.up * 500;
        }
    }
    public void OnEnable()
    {
        terrainErosion = target as TerrainErosion;

        EditorApplication.update += Update;
    }

    public void OnDisable()
    {
        EditorApplication.update -= Update;
        if (globalProjector != null) {
            GameObject.DestroyImmediate(globalProjector.gameObject);
            globalProjector = null;
        }
    }

    public void OnSceneGUI()
    {
        //Debug.Log(a.ToString());
        //a++;
    }

    public void UnselectTerrainInspectorTool()
    {
        System.Type terrainType = null;
        System.Type[] tmp = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetTypes();
        for (int i = tmp.Length - 1; i >= 0; i--) {
            if (tmp[i].Name == "TerrainInspector") { terrainType = tmp[i]; break; }
        }
        object[] editors = Resources.FindObjectsOfTypeAll(terrainType);
        for (int i = 0; i < editors.Length; i++) {
            PropertyInfo toolProp = terrainType.GetProperty("selectedTool", BindingFlags.Instance | BindingFlags.NonPublic);
            toolProp.SetValue(editors[i], -1, null);
        }
    }

    public void Update()
    {
        terrainErosion.EditorUpdate(()=> {
            GameObject.DestroyImmediate(globalProjector.gameObject);
            globalProjector = null;
        });
    }

    public void Dispose()
    {
        int a = 0;
    }

    private float DrawProperty(string label,float val)
    {
        return EditorGUILayout.FloatField(label,val);
    }
    private int DrawProperty(string label, int val)
    {
        return EditorGUILayout.IntField(label, val);
    }


    private Projector CreatePreviewProjector()
    {
        GameObject gameObject = EditorUtility.CreateGameObjectWithHideFlags("TerrainErosionPreview", HideFlags.DontSave, new Type[]{typeof(Projector)});
        Projector projector;
        projector = (gameObject.GetComponent(typeof(Projector)) as Projector);
        projector.enabled = false;
        projector.nearClipPlane = -1000f;
        projector.farClipPlane = 1000f;
        projector.orthographic = true;
        projector.orthographicSize = 10f;
        projector.transform.Rotate(90f, 0f, 0f);
        projector.material = new Material(Shader.Find("Hidden/WaterPreview"));
        projector.enabled = false;
        return projector;
    }
}
