using UnityEngine;
using System.Collections;
using UnityEditor;
using TerrainTool;

namespace TerrainTool
{
    [CustomEditor(typeof(TerrainTool))]
    public class TerrainToolEditor : Editor
    {
        private TerrainTool terrainTool;
        public void OnEnable()
        {
            terrainTool = target as TerrainTool;
        }

        private delegate void FunDraw();
        private void GUILayoutTab(int width, FunDraw fun)
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
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Terrain")) {

            }
            if (GUILayout.Button("Open Editor")) {
                AdvanceEditor advanceWindow = (AdvanceEditor)EditorWindow.GetWindow(typeof(AdvanceEditor));
                advanceWindow.SetNodeContainers(terrainTool.nodeContainers);
                advanceWindow.init();
            }
            if (GUILayout.Button("Export")) {

            }
            GUILayout.EndHorizontal();

            DrawDefaultInspector();

            GUILayoutTab(10, () => {
                for (int i = 0; i < terrainTool.nodeContainers.Count; i++) {
                    NodeContainer container = terrainTool.nodeContainers[i];
                    GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);
                    myFoldoutStyle.fontStyle = FontStyle.Bold;
                    if (container.foldout = EditorGUILayout.Foldout(container.foldout, container.name,myFoldoutStyle)) {
                        SerializedObject so = new SerializedObject(container.node);
                        DrawPropertiesExcluding(so, "m_Script");
                        if (GUI.changed) {
                            so.ApplyModifiedProperties();
                            GUI.changed = false;
                        }
                    }
                }
            });
        }
    }
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
//SerializedProperty prop = soContainer.GetIterator();
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

//if (GUI.changed) {
//    soContainer.ApplyModifiedProperties();
//    soNode.ApplyModifiedProperties();
//}