using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainTool.TerrainTool))]
public class NodeEditor : Editor
{
    private TerrainTool.TerrainTool terrainTool;
    SerializedProperty sp;

    public void OnEnable()
    {
        terrainTool = target as TerrainTool.TerrainTool;
        sp = serializedObject.FindProperty("container");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //serializedObject.Update();
        ///EditorGUILayout.PropertyField(sp);
        //if (sp.isExpanded) {
        //    foreach (SerializedProperty p in sp)
        //        EditorGUILayout.PropertyField(p);
        //}
        //DrawDefaultInspector();
        //serializedObject.Update();
        //EditorGUILayout.PropertyField(lookAtPoint);
        //serializedObject.ApplyModifiedProperties();
        //GUILayout.Button("测试");
        //EditorGUILayout.PropertyField(sp);
    }
}

[CustomEditor(typeof(TerrainTool.NodeContainer))]
public class ContainerEditor:Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Button("测试");
    }
}
