using UnityEngine;
using System.Collections;

namespace TerrainTool
{
    public class NodeBinaryOperator : NodeBase
    {
        public enum BinaryOperatorType
        {
            Add,
            Sub,
            Mul,
            Max,
        }
        public BinaryOperatorType operatorType;
        public override float[,] update(int seed, int width, int height, Rect rect)
        {
            float[,] values = new float[width, height];
            if (operatorType == BinaryOperatorType.Add) {
                if (inputs[0] != null && inputs[1] != null) {
                    float[,] a = inputs[0].update(seed, width, height, rect);
                    float[,] b = inputs[1].update(seed, width, height, rect);
                    for (int i = 0; i < width; i++) {
                        for (int j = 0; j < height; j++) {
                            values[i, j] = a[i, j] + b[i, j];
                        }
                    }
                }
            }
            else if (operatorType == BinaryOperatorType.Sub) {
                if (inputs[0] != null && inputs[1] != null) {
                    float[,] a = inputs[0].update(seed, width, height, rect);
                    float[,] b = inputs[1].update(seed, width, height, rect);
                    for (int i = 0; i < width; i++) {
                        for (int j = 0; j < height; j++) {
                            values[i, j] = a[i, j] - b[i, j];
                        }
                    }
                }
            }
            else if (operatorType == BinaryOperatorType.Mul) {
                if (inputs[0] != null && inputs[1] != null) {
                    float[,] a = inputs[0].update(seed, width, height, rect);
                    float[,] b = inputs[1].update(seed, width, height, rect);
                    for (int i = 0; i < width; i++) {
                        for (int j = 0; j < height; j++) {
                            values[i, j] = a[i, j] * b[i, j];
                        }
                    }
                }
            }
            else if (operatorType == BinaryOperatorType.Max) {
                if (inputs[0] != null && inputs[1] != null) {
                    float[,] a = inputs[0].update(seed, width, height, rect);
                    float[,] b = inputs[1].update(seed, width, height, rect);
                    for (int i = 0; i < width; i++) {
                        for (int j = 0; j < height; j++) {
                            values[i, j] = a[i, j] > b[i, j] ? a[i, j] : b[i, j];
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
            return new string[] { "a", "b" };
        }
        public override string GetExpression()
        {
            switch (operatorType) {
                case BinaryOperatorType.Add:return "o = a + b";
                case BinaryOperatorType.Sub:return "o = a - b";
                case BinaryOperatorType.Mul:return "o = a * b";
                case BinaryOperatorType.Max:return "o = max(a,b)";
                default:return "";
            }
        }
    }
}