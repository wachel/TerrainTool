using UnityEngine;
using System.Collections;

namespace TerrainTool
{
    public class NodeImport : NodeBase
    {
        public Texture2D texture;
        public Rect rect;

        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            if (texture != null) {
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        float u = i / (float)w;
                        float v = j / (float)h;
                        values[i, j] = texture.GetPixelBilinear(u, v).r * scale + bias;
                    }
                }
            }
            return values;
        }
    }
}