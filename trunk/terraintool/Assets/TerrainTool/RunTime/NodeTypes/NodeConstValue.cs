using UnityEngine;
using System.Collections;

namespace TerrainTool
{
    public class NodeConstValue : NodeBase
    {
        public float value = 1.0f;
        public NodeConstValue()
        {
            inputs = new NodeBase[0];
        }
        public override float[,] update(int seed, int width, int height, Rect rect)
        {
            float[,] values = new float[width, height];
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    values[i, j] = value;
                }
            }
            return values;
        }
    }
}