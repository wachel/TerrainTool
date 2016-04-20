using UnityEngine;
using System.Collections;

[System.Serializable]
public class RawFileData:ScriptableObject
{
    public Texture2D previewTexture;
    [HideInInspector]
    public int width;
    [HideInInspector]
    public int height;
    public float[] values;
}