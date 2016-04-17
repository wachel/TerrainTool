using UnityEngine;
using System.Collections;

namespace TerrainTool
{

    public class NodeNormal : NodeBase
    {
        public float height = 100.0f;
        public enum NormalType
        {
            NT_X,
            NT_Y,
            NT_Z,
        }
        public NormalType normalType = NormalType.NT_Y;

        public override float[,] update(int seed, int width, int height, Rect rect)
        {
            float[,] values = new float[width, height];
            if (inputs[0] != null) {
                float[,] a = inputs[0].update(seed, width + 1, height + 1, rect);
                Vector3[,] normal = new Vector3[width, height];
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        Vector3 pos_x = new Vector3(1, (a[i + 1, j] - a[i, j]) * height, 0);
                        Vector3 pos_z = new Vector3(0, (a[i, j + 1] - a[i, j]) * height, 1);
                        normal[i, j] = Vector3.Cross(pos_x, -pos_z).normalized;
                    }
                }
                if (normalType == NormalType.NT_X) {
                    for (int i = 0; i < width; i++) {
                        for (int j = 0; j < height; j++) {
                            values[i, j] = normal[i, j].x;
                        }
                    }
                }
                else if (normalType == NormalType.NT_Z) {
                    for (int i = 0; i < width; i++) {
                        for (int j = 0; j < height; j++) {
                            values[i, j] = normal[i, j].z;
                        }
                    }
                }
                else if (normalType == NormalType.NT_Y) {
                    for (int i = 0; i < width; i++) {
                        for (int j = 0; j < height; j++) {
                            values[i, j] = normal[i, j].y;
                        }
                    }
                }
            }
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    values[i, j] = values[i, j] * scale + bias;
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