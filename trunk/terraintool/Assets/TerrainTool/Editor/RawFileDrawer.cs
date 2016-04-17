using UnityEngine;
using System.Collections;
using UnityEditor;


[CustomPropertyDrawer(typeof(RawFileAttribute))]
public class RawFileDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;// base.GetPropertyHeight(property, label);
    }
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        if (GUILayout.Button("Import...")) {
            EditorUtility.OpenFilePanel("select raw file", "", "raw");
        }
        GUILayout.EndHorizontal();
    }
}
