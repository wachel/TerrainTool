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
        public override float[,] update(int seed, int x, int y, int w, int h, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            if (size < 0.01) {
                return new float[w, h];
            }
            generator.Frequency = 1.0 / size;
            generator.OctaveCount = 1;
            generator.Seed = seed + localSeed;
            generator.Quality = LibNoise.Unity.QualityMode.High;

            string key = PerlinCache.makeKey("perlin", generator.Frequency, generator.Seed, octaveCount, x, y, w, h, scaleX, scaleY, curve);
            float[,] temp = null;
            if (PerlinCache.hasKey(key)) {
                temp = PerlinCache.getCache(key);
            }
            else {
                temp = new float[w, h];
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < h; j++) {
                        float tempX = x + i;
                        float tempY = y + j;
                        float val = 0f;
                        float cp = 0.5f;
                        for (int o = 0; o < octaveCount; o++) {
                            float signal = (float)generator.GetValue(tempX * scaleX, tempY * scaleY, 0);
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
            float[,] values = new float[w, h];
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    values[i, j] = temp[i, j] * scale + bias;
                }
            }
            return values;
        }
    }
} 
