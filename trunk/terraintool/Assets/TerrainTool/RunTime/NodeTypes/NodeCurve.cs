using UnityEngine;
using System.Collections;

namespace TerrainTool
{
    public class NodeCurve : NodeBase
    {
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        public override float[,] update(int seed, int width, int height, Rect rect)
        {
            float[,] values = new float[width, height];
            if (inputs[0] != null) {
                float[,] a = inputs[0].update(seed, width, height, rect);
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        values[i, j] = curve.Evaluate(a[i, j]);
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