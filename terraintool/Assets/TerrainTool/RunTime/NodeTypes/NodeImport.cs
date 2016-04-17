using UnityEngine;
using System.Collections;

namespace TerrainTool
{
    public class NodeImport : NodeBase
    {
        public Rect rect = new Rect(0,0,100,100);
        public Texture2D texture;
        public float defaultValue;

        public override float[,] update(int seed, int width, int height, Rect rect)
        {
            float[,] values = new float[width, height];
            for(int i = 0; i<width; i++){
                for(int j = 0;j<height;j++){
                    values[i,j] = defaultValue;
                }
            }
            if (texture != null && this.rect.width > 0 && this.rect.height > 0) {
                float minU = (rect.x - this.rect.x) / this.rect.width;
                float maxU = (rect.xMax - this.rect.x) / this.rect.width;
                float minV = (rect.y - this.rect.y) / this.rect.height;
                float MaxV = (rect.yMax - this.rect.y) / this.rect.height;
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        float u = Mathf.Lerp(minU,maxU,i / (float)width);
                        float v = Mathf.Lerp(minV, MaxV,j / (float)height);
                        if (u >= 0 && u <= 1 && v >= 0 && v <= 1) {
                            values[i, j] = texture.GetPixelBilinear(u, v).r * scale + bias;
                        }
                    }
                }
            }
            return values;
        }
    }
}