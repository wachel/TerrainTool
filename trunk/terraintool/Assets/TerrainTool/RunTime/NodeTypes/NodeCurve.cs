using UnityEngine;
using System.Collections;

namespace TerrainTool
{
    public class NodeCurve : NodeBase
    {
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            if (inputs[0] != null) {
                float[,] a = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        values[i, j] = curve.Evaluate(a[i, j]);
                    }
                }
            }
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
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