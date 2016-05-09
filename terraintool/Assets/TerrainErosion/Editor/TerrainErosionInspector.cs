using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Reflection;

[CustomEditor(typeof(TerrainErosion))]
public class TerrainErosionInspector : Editor
{
    Projector globalProjector;
    Projector brushPreviewProjector;
    TerrainErosion terrainErosion;
    private static GUIStyle ToggleButtonStyleNormal = null;
    private static GUIStyle ToggleButtonStyleToggled = null;

    object terrainEditor;
    PropertyInfo selectedTool;

    bool painting = false;

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        if (ToggleButtonStyleNormal == null) {
            ToggleButtonStyleNormal = new GUIStyle("Button");
            ToggleButtonStyleToggled = new GUIStyle("Button");
            ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
        }

        if((int)selectedTool.GetValue(terrainEditor,null) != -1) {
            terrainErosion.editType = ErosionEditType.Global;
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Brush Erosion", terrainErosion.editType == ErosionEditType.Brush? ToggleButtonStyleToggled:ToggleButtonStyleNormal)) {
            terrainErosion.editType = ErosionEditType.Brush;
        }
        if (GUILayout.Button("Global Erosion", terrainErosion.editType == ErosionEditType.Global? ToggleButtonStyleToggled:ToggleButtonStyleNormal)) {
            terrainErosion.editType = ErosionEditType.Global;
        }
        GUILayout.EndHorizontal();

        if (terrainErosion.editType == ErosionEditType.Brush) {
            selectedTool.SetValue(terrainEditor, -1, null);
        }

        if (terrainErosion.editType == ErosionEditType.Global) {
            terrainErosion.simulateStep = Mathf.Max(1, EditorGUILayout.IntField("Simulate Step", terrainErosion.simulateStep));
            //GUILayout.BeginArea()
        }

        if (terrainErosion.randomRaindrop = EditorGUILayout.Toggle("Random Raindrop", terrainErosion.randomRaindrop)) {
            terrainErosion.rainPointSpeed = EditorGUILayout.Slider("Rain Speed", terrainErosion.rainPointSpeed, 0, 100);
            terrainErosion.rainPointSize = EditorGUILayout.Slider("Raindrop Size", terrainErosion.rainPointSize, 0.001f, 0.1f);
            terrainErosion.rainHeight = EditorGUILayout.Slider("Raindrop Height", terrainErosion.rainHeight, 0.001f, 0.1f);
        }
        else {
            terrainErosion.globalRainSpeed = EditorGUILayout.Slider("Rain Speed", terrainErosion.globalRainSpeed, 0, 0.001f);
        }
        terrainErosion.evaporateSpeed = EditorGUILayout.Slider("Evaporate Speed", terrainErosion.evaporateSpeed, 0, 0.01f);
        terrainErosion.viewWaterDensity = EditorGUILayout.Slider("View Water Density", terrainErosion.viewWaterDensity, 0, 1);
        if (terrainErosion.editType == ErosionEditType.Global) {
            if (terrainErosion.GetRemainStep() == 0) {
                if (GUILayout.Button("Start")) {
                    StartErosion();
                }
            }
            else {
                if (GUILayout.Button("Stop")) {
                    terrainErosion.StopErosion();
                }
            }
        }

        if (globalProjector != null) {
            globalProjector.material.SetFloat("_Scale", terrainErosion.GetViewWaterHeight());
        }

    }
    public void OnEnable()
    {
        terrainErosion = target as TerrainErosion;

        globalProjector = CreatePreviewProjector();
        brushPreviewProjector = CreatePreviewProjector();

        UpdateTerrainInspectorTool();
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

    public void UpdateTerrainInspectorTool()
    {
        Type TerrainInspectorType = FindTypeFromReflection(typeof(UnityEditor.Editor), "TerrainInspector");
        selectedTool = TerrainInspectorType.GetProperty("selectedTool", BindingFlags.Instance | BindingFlags.NonPublic);
        object[] objs = Resources.FindObjectsOfTypeAll(TerrainInspectorType);
        if(objs.Length > 0) {
            terrainEditor = objs[0];
        }
    }

    public void Update()
    {
        terrainErosion.EditorUpdate(()=> {
            if (globalProjector != null) {
                GameObject.DestroyImmediate(globalProjector.gameObject);
                globalProjector = null;
            }
        });
    }

    private bool Raycast(out Vector2 uv, out Vector3 pos)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit raycastHit;
        if (terrainErosion.terrain.GetComponent<Collider>().Raycast(ray, out raycastHit, float.PositiveInfinity)) {
            uv = raycastHit.textureCoord;
            pos = raycastHit.point;
            return true;
        }
        uv = Vector2.zero;
        pos = Vector3.zero;
        return false;
    }

    private void StartErosion()
    {
        terrainErosion.StartErosion();
        globalProjector.enabled = true;
        globalProjector.material.mainTexture = terrainErosion.height_a;
        globalProjector.material.SetFloat("_Scale", terrainErosion.GetViewWaterHeight());
        globalProjector.orthographicSize = terrainErosion.terrain.terrainData.size.x / 2;
        globalProjector.transform.position = terrainErosion.terrain.transform.position + terrainErosion.terrain.terrainData.size / 2 + Vector3.up * 500;
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

    Type FindTypeFromReflection(Type libType, string typeName)
    {
        Type[] types = Assembly.GetAssembly(libType).GetTypes();
        for (int i = 0; i < types.Length; i++) {
            if (types[i].Name == typeName) {
                return types[i];
            }
        }
        return null;
    }
}
