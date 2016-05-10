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
            brushPreviewProjector.enabled = true;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Brush Size");
            terrainErosion.brushSizeFactor = GUILayout.HorizontalSlider(terrainErosion.brushSizeFactor, 0, 1,GUILayout.ExpandWidth(true));
            GUILayout.Label(terrainErosion.GetRealBrushSize().ToString(),GUILayout.Width(50));
            GUILayout.EndHorizontal();
            //terrainErosion.brushSizeFactor = EditorGUILayout.Slider("Brush Size", terrainErosion.brushSizeFactor,0,1);
        }

        if (terrainErosion.editType == ErosionEditType.Global) {
            brushPreviewProjector.enabled = false;
            terrainErosion.simulateStep = Mathf.Max(1, EditorGUILayout.IntField("Simulate Step", terrainErosion.simulateStep));
            //GUILayout.BeginArea()
        }

        if (terrainErosion.randomRaindrop = EditorGUILayout.Toggle("Random Raindrop", terrainErosion.randomRaindrop)) {
            terrainErosion.raindropDensity = EditorGUILayout.Slider("Raindrop Density", terrainErosion.raindropDensity, 0, 100);
            terrainErosion.rainPointSize = EditorGUILayout.Slider("Raindrop Size", terrainErosion.rainPointSize, 0.01f, 5f);
            //terrainErosion.rainHeight = EditorGUILayout.Slider("Raindrop Height", terrainErosion.rainHeight, 0.001f, 0.1f);
        }
        else {
        }


        terrainErosion.rainSpeed = EditorGUILayout.Slider("Rain Speed", terrainErosion.rainSpeed, 0, 0.001f);
        terrainErosion.evaporateSpeed = EditorGUILayout.Slider("Evaporate Speed", terrainErosion.evaporateSpeed, 0, 0.01f);
        terrainErosion.viewWaterDensity = EditorGUILayout.Slider("View Water Density", terrainErosion.viewWaterDensity, 0, 1);
        if (terrainErosion.editType == ErosionEditType.Global) {
            if (terrainErosion.GetRemainStep() == 0) {
                if (GUILayout.Button("Start")) {
                    StartErosion();
                    terrainErosion.StartGlobalRain();
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
        if(brushPreviewProjector != null) {
            brushPreviewProjector.orthographicSize = terrainErosion.GetRealBrushSize();
        }
    }
    public void OnEnable()
    {
        terrainErosion = target as TerrainErosion;

        globalProjector = CreatePreviewProjector("TerrainErosionPreview");
        brushPreviewProjector = CreatePreviewProjector("BrushPreview");
        brushPreviewProjector.material = new Material(Shader.Find("Hidden/ErosionBrushPreview"));
        brushPreviewProjector.material.mainTexture = terrainErosion.brushPreviewTexture;

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
        if (brushPreviewProjector != null) {
            GameObject.DestroyImmediate(brushPreviewProjector.gameObject);
            brushPreviewProjector = null;
        }
    }

    public void OnSceneGUI()
    {
        Vector2 uv;
        Vector3 pos;
        if (terrainErosion.editType == ErosionEditType.Brush) {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            if (Raycast(out uv, out pos)) {
                Vector3 newPos = terrainErosion.terrain.transform.TransformPoint(pos);
                //Debug.Log(newPos.ToString());
                brushPreviewProjector.transform.position = newPos + Vector3.up * 500;
                terrainErosion.brushPreviewUV = uv;
                if (Event.current.type == EventType.MouseDrag && Event.current.button == 0 && !Event.current.control && !Event.current.shift && !Event.current.alt) {
                    terrainErosion.isPainting = true;
                    if (terrainErosion.GetPaintDelayStep() == 0) {
                        StartErosion();
                    }
                }
                SceneView.RepaintAll();
            }
        }

        if (Event.current.type == EventType.MouseUp) {
            terrainErosion.isPainting = false;
        }
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
                globalProjector.enabled = false;
            }
            Repaint();
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

    private Projector CreatePreviewProjector(string name)
    {
        GameObject gameObject = EditorUtility.CreateGameObjectWithHideFlags(name, HideFlags.DontSave, new Type[]{typeof(Projector)});
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
