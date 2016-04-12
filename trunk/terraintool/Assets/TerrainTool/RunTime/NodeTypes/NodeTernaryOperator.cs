using UnityEngine;
using System.Collections;

namespace TerrainTool
{
    public class NodeTernaryOperator : NodeBase
    {
        public enum TernaryOperatorType
        {
            Lerp,
        }
        public TernaryOperatorType operatorType;

        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            if (operatorType == TernaryOperatorType.Lerp) {
                if (inputs[0] != null && inputs[1] != null && inputs[2] != null) {
                    float[,] a = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
                    float[,] s = inputs[1].update(seed, x, y, w, h, scaleX, scaleY);
                    float[,] b = inputs[2].update(seed, x, y, w, h, scaleX, scaleY);
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = Mathf.Lerp(a[i, j], b[i, j], s[i, j]);
                        }
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
            return new string[] { "a", "s", "b" };
        }
        public override string GetExpression()
        {
            switch (operatorType) {
                case TernaryOperatorType.Lerp:return "o = a*(1-s) + b*s";
                default:return "";
            }
        }
    }
}
