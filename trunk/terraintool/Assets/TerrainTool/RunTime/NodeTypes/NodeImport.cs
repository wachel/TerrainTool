using UnityEngine;
using System.Collections;

namespace TerrainTool
{
    public class NodeImport : NodeBase
    {
        public Rect rect = new Rect(0,0,100,100);
        public RawFileData rawFile = null;
        public float defaultValue;
        public override void Init()
        {
            rawFile = new RawFileData();
        }
        public override float[,] update(int seed, int width, int height, Rect rect)
        {
            float[,] values = new float[width, height];
            for(int i = 0; i<width; i++){
                for(int j = 0;j<height;j++){
                    values[i,j] = defaultValue;
                }
            }
            if (rawFile != null && rawFile.values != null && this.rect.width > 0 && this.rect.height > 0) {
                float minU = (rect.x - this.rect.x) / this.rect.width;
                float maxU = (rect.xMax - this.rect.x) / this.rect.width;
                float minV = (rect.y - this.rect.y) / this.rect.height;
                float MaxV = (rect.yMax - this.rect.y) / this.rect.height;
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        float u = Mathf.Lerp(minU, maxU,i / (float)width);
                        float v = Mathf.Lerp(minV, MaxV,j / (float)height);
                        int x = (int)(u * rawFile.width);
                        int y = (int)(v * rawFile.height);
                        if (x >= 0 && x < rawFile.width && y >= 0 && y < rawFile.height) {
                            values[i, j] = rawFile.values[y * rawFile.width + x] * scale + bias;
                        }
                    }
                }
            }
            return values;
        }
        //public override float[,] updatePreview(int seed, int width, int height)
        //{
        //    if (rawFile != null && rawFile.width > 0 && rawFile.height > 0) {
        //        return update(seed, width, height, new Rect(0, 0, rawFile.width, rawFile.height));
        //    }
        //    else {
        //        return new float[width, height];
        //    }
        //}
    }
}