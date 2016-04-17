using UnityEngine;
using System.Collections;

namespace TerrainTool
{
    public class GrassOutput : NodeBase
    {
        public Texture2D texture;
        public float minSize = 1.0f;
        public float maxSize = 1.0f;
        public bool bBillboard = true;
        public Texture2D Texture { get { return texture; } }

        public override float[,] update(int seed, int width, int height, Rect rect)
        {
            float[,] values = null;
            if (inputs[0] != null) {
                values = inputs[0].update(seed, width, height, rect);
            }
            else {
                values = new float[width, height];
            }
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    values[i, j] = Mathf.Clamp(values[i, j] * scale + bias, 0f, 1f);
                }
            }
            return values;
        }
        public override string[] GetInputNames()
        {
            return new string[] { "i" };
        }
        public override bool hasOutput()
        {
            return false;
        }
    }
}