using UnityEngine;
using System.Collections;

namespace TerrainTool
{

    public class NodeErosion : NodeBase
    {
        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            if (inputs[0] != null) {
                float[,] heights = inputs[0].update(seed, x, y, w + 1, h + 1, scaleX, scaleY);
                Vector3[,] normal = new Vector3[w, h];
                int height = 100;
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        Vector3 pos_x = new Vector3(1, (heights[i + 1, j] - heights[i, j]) * height, 0);
                        Vector3 pos_z = new Vector3(0, (heights[i, j + 1] - heights[i, j]) * height, 1);
                        normal[i, j] = Vector3.Cross(pos_x, -pos_z).normalized;
                    }
                }
                float[,] water = new float[w, h];
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        water[i, j] += 0.01f;//rain
                    }
                }
                for (int i = 0; i < w - 1; i++) {
                    for (int j = 0; j < h - 1; j++) {
                        water[i, j] = 0;
                    }
                }
            }
            return values;
        }
        public override string[] GetInputNames()
        {
            return new string[] { "i" };
        }
    }
}