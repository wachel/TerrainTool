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

        public override float[,] update(int seed, int width, int height, Rect rect)
        {
            float[,] values = new float[width, height];
            if (operatorType == TernaryOperatorType.Lerp) {
                if (inputs[0] != null && inputs[1] != null && inputs[2] != null) {
                    float[,] a = inputs[0].update(seed, width, height, rect);
                    float[,] s = inputs[1].update(seed, width, height, rect);
                    float[,] b = inputs[2].update(seed, width, height, rect);
                    for (int i = 0; i < width; i++) {
                        for (int j = 0; j < height; j++) {
                            values[i, j] = Mathf.Lerp(a[i, j], b[i, j], s[i, j]);
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
