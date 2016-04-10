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
        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    values[i, j] = value;
                }
            }
            return values;
        }
    }
}