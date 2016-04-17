using UnityEngine;
using System.Collections;

namespace TerrainTool
{

    public class NodeErosion : NodeBase
    {
        public override float[,] update(int seed, int width, int height ,Rect rect)
        {
            float[,] values = new float[width, height];
            if (inputs[0] != null) {
                float[,] heights = inputs[0].update(seed, width + 1, height + 1, rect);
                Vector3[,] normal = new Vector3[width, height];
                int terrainHeight = 100;
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < terrainHeight; j++) {
                        Vector3 pos_x = new Vector3(1, (heights[i + 1, j] - heights[i, j]) * terrainHeight, 0);
                        Vector3 pos_z = new Vector3(0, (heights[i, j + 1] - heights[i, j]) * terrainHeight, 1);
                        normal[i, j] = Vector3.Cross(pos_x, -pos_z).normalized;
                    }
                }
                float[,] water = new float[width, height];
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        water[i, j] += 0.01f;//rain
                    }
                }
                for (int i = 0; i < width - 1; i++) {
                    for (int j = 0; j < height - 1; j++) {
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