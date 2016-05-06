using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(TerrainErosion))]
public class TerrainErosionInspector : Editor
{
    Projector globalProjector;
    TerrainErosion terrainErosion;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("生成")) {
            terrainErosion.StartErosion();
            globalProjector = CreatePreviewProjector();
            globalProjector.enabled = true;
            globalProjector.material.mainTexture = terrainErosion.height_a;
            globalProjector.material.SetFloat("_Scale", 100);
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
