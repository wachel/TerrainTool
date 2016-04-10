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
        public NodeBinaryOperator(BinaryOperatorType op)
        {
            operatorType = op;
        }
        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            float[,] values = new float[w, h];
            if (operatorType == BinaryOperatorType.Add) {
                if (inputs[0] != null && inputs[1] != null) {
                    float[,] a = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
                    float[,] b = inputs[1].update(seed, x, y, w, h, scaleX, scaleY);
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = a[i, j] + b[i, j];
                        }
                    }
                }
            }
            else if (operatorType == BinaryOperatorType.Sub) {
                if (inputs[0] != null && inputs[1] != null) {
                    float[,] a = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
                    float[,] b = inputs[1].update(seed, x, y, w, h, scaleX, scaleY);
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = a[i, j] - b[i, j];
                        }
                    }
                }
            }
            else if (operatorType == BinaryOperatorType.Mul) {
                if (inputs[0] != null && inputs[1] != null) {
                    float[,] a = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
                    float[,] b = inputs[1].update(seed, x, y, w, h, scaleX, scaleY);
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = a[i, j] * b[i, j];
                        }
                    }
                }
            }
            else if (operatorType == BinaryOperatorType.Max) {
                if (inputs[0] != null && inputs[1] != null) {
                    float[,] a = inputs[0].update(seed, x, y, w, h, scaleX, scaleY);
                    float[,] b = inputs[1].update(seed, x, y, w, h, scaleX, scaleY);
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            values[i, j] = a[i, j] > b[i, j] ? a[i, j] : b[i, j];
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
            return new string[] { "a", "b" };
        }
    }
}