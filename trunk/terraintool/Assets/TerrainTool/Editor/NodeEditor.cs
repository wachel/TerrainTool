using UnityEngine;
using System.Collections;
using UnityEditor;
using TerrainTool;

[CustomEditor(typeof(TerrainTool.TerrainTool))]
public class NodeEditor : Editor
{
    private TerrainTool.TerrainTool terrainTool;


    SerializedObject soContainer;
    SerializedObject soNode;

    public void OnEnable()
    {
        terrainTool = target as TerrainTool.TerrainTool;
        soContainer = new SerializedObject(terrainTool.container);
        soNode = new SerializedObject(terrainTool.container.node);
    }

    private delegate void FunDraw();
    private void GUILayoutTab(int width,FunDraw fun)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(width);
        GUILayout.BeginVertical();
        fun();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Editor")) {

        }
        //DrawDefaultInspector();
        //serializedObject.Update();
        //EditorGUILayout.PropertyField(sp);
        // if (sp.isExpanded) {
        //foreach (SerializedProperty p in sp) {
        //    if (!EditorGUILayout.PropertyField(p)) {
        //        break;
        //    }
        //}
        //}
        //serializedObject.ApplyModifiedProperties();
        //DrawDefaultInspector();
        //serializedObject.Update();
        //EditorGUILayout.PropertyField(lookAtPoint);
        //serializedObject.ApplyModifiedProperties();
        //GUILayout.Button("测试");
        //EditorGUILayout.PropertyField(sp);
        GUILayoutTab(10, () => {
            if (terrainTool.container.foldout = EditorGUILayout.Foldout(terrainTool.container.foldout, terrainTool.container.name)) {
                //DrawPropertiesExcluding(soContainer, "name","foldout", "m_Script", "node");
                DrawPropertiesExcluding(soNode, "m_Script");
            }
        });

        SerializedProperty prop = soContainer.GetIterator();
        //while (prop.NextVisible(true)) {
        //    int a = 0;
        //    if (prop.name == "container") {
        //        //SerializedObject childobj = new SerializedObject(prop.objectReferenceValue);
        //        //SerializedProperty childprop = childobj.GetIterator();
        //        //while (childprop.NextVisible(true)) {
        //        //    EditorGUILayout.PropertyField(childprop);
        //        //}
        //    }
        //    else {
        //        EditorGUILayout.PropertyField(prop);
        //    }
       // }

        if (GUI.changed) {
            soContainer.ApplyModifiedProperties();
            soNode.ApplyModifiedProperties();
        }
    }
}
