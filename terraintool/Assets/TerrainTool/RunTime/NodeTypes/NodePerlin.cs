using UnityEngine;
using System.Collections;

namespace TerrainTool
{
    public class NodePerlin : NodeBase
    {
        public float size = 100f;
        public int octaveCount = 4;
        public int localSeed = 0;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        LibNoise.Unity.Generator.Perlin generator = new LibNoise.Unity.Generator.Perlin();
        public override float[,] update(int seed, int width, int height, Rect rect)
        {
            if (size < 0.01) {
                return new float[width, height];
            }
            generator.Frequency = 1.0 / size;
            generator.OctaveCount = 1;
            generator.Seed = seed + localSeed;
            generator.Quality = LibNoise.Unity.QualityMode.High;

            string key = PerlinCache.makeKey("perlin", generator.Frequency, generator.Seed, octaveCount, width, height, rect, curve);
            float[,] temp = null;
            if (PerlinCache.hasKey(key)) {
                temp = PerlinCache.getCache(key);
            }
            else {
                temp = new float[width, height];
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        float tempX = rect.x + rect.width*i/(float)width;
                        float tempY = rect.y + rect.height*j/(float)height;;
                        float val = 0f;
                        float cp = 0.5f;
                        for (int o = 0; o < octaveCount; o++) {
                            float signal = (float)generator.GetValue(tempX, tempY, 0);
                            val += (curve.Evaluate(signal * 0.4f + 0.5f) - 0.5f) * 2f * cp;
                            tempX *= 1.93456789f;
                            tempY *= 1.93456789f;
                            cp *= 0.5f;
                        }
                        temp[i, j] = (val * 0.5f + 0.5f);
                    }
                }
                PerlinCache.addCache(key, temp);
            }
            float[,] values = new float[width, height];
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    values[i, j] = temp[i, j] * scale + bias;
                }
            }
            return values;
        }
    }
} 
