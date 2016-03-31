using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainTool.TerrainTool))]
public class NodeEditor : Editor
{
    //private TerrainTool.TerrainTool terrainTool;
    SerializedProperty prop;
    SerializedObject so;

    public void OnEnable()
    {
        so = new SerializedObject(target);
    }

    public override void OnInspectorGUI()
    {
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

        prop = so.GetIterator();
        while (prop.NextVisible(true)) {
            if (prop.name == "container") {
                //SerializedObject childobj = prop.serializedObject;
                //SerializedProperty childprop = childobj.GetIterator();
                //while (childprop.NextVisible(true)) {
                //    EditorGUILayout.PropertyField(childprop);
                //}
            }
            else {
                EditorGUILayout.PropertyField(prop);
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Woop!")) {
            Debug.Log("Woop!");
        }

        if (GUI.changed) {
            so.ApplyModifiedProperties();
        }
    }
}

[CustomEditor(typeof(TerrainTool.NodeContainer))]
public class ContainerEditor:Editor
{
    SerializedProperty prop;
    SerializedObject so;
    public override void OnInspectorGUI()
    {
        prop = so.GetIterator();
        while (prop.NextVisible(true)) {
            EditorGUILayout.PropertyField(prop);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Woop!")) {
            Debug.Log("Woop!");
        }

        if (GUI.changed) {
            so.ApplyModifiedProperties();
        }
    }
}
