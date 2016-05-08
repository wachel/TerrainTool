using UnityEngine;
using System.Collections;
using System;

public enum ErosionEditType
{
    Local,
    Global,
}
//
//[Serializable]
//public class ErosionRainConfig:UnityEngine.Object
//{
//    public float rainPointSpeed = 20;
//    public float rainPointSize = 0.01f;
//    public float rainPointHeight = 0.002f;
//    public float evaporateSpeed = 0.00001f;
//}


[ExecuteInEditMode]
public class TerrainErosion : MonoBehaviour
{
#if UNITY_EDITOR
    [HideInInspector]
    public ErosionEditType editType;

    //public ErosionRainConfig rainConfig;
    
    public int simulateStep = 100;

    public float rainPointSpeed = 0;
    public float rainPointSize = 0.01f;
    public float rainHeight = 0.02f;

    public float evaporateSpeed = 0.0001f;
    public float globalRainSpeed = 0.00002f;

    [HideInInspector]
    public RenderTexture height_a;
    [HideInInspector]
    public Terrain terrain;

    private RenderTexture outflow_a;
    private RenderTexture height_b;
    private RenderTexture outflow_b;
    private Material matErosion;
    private Material matRain;
    private Texture2D rainTexture;
    private int remainStep = 0;

    private RenderTexture CreateRenderTexture(int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBFloat);
        rt.generateMips = false;
        rt.useMipMap = false;
        rt.filterMode = FilterMode.Bilinear;
        rt.wrapMode = TextureWrapMode.Clamp;
        return rt;
    }

    private Texture2D CreateCircleTexture(int size,Color centerColor)
    {
        Color[] colors = new Color[size * size];
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                float dist = Mathf.Sqrt(i*i + j*j);
                colors[j*size + i] = Color.Lerp(centerColor,new Color(0,0,0,0),dist/size);
            }
        }
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBAFloat, true);
        tex.SetPixels(colors);
        tex.Apply(true);
        return tex;
    }

    private float[,] GetHardness(int width,int height)
    {
        LibNoise.Unity.Generator.Perlin generator = new LibNoise.Unity.Generator.Perlin();
        generator.Frequency = 1.0 / 6;
        generator.OctaveCount = 4;
        generator.Seed = 123498765;
        generator.Quality = LibNoise.Unity.QualityMode.Low;

        float[,] values = new float[width ,height];
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                values[i, j] = (float)generator.GetValue(i, j, 0) * 0.4f + 0.5f;
            }
        }
        return values;
    }

    void Start()
    {
        matErosion = new Material(Shader.Find("Hidden/Erosion"));
        matRain = new Material(Shader.Find("Hidden/Rain"));
        rainTexture = CreateCircleTexture(64,new Color(0,1,0,0));
    }

    public void OnEnable()
    {
        
    }

    public void OnDisable()
    {

    }

    public void EditorUpdate(Action completeCallback)
    {
        if (remainStep > 0) {
            SimulateStep();
            remainStep -= 1;
            if(remainStep == 0) {
                UpdateTerrain();
                if(completeCallback != null) {
                    completeCallback();
                }
            }
        }
    }

    public void StartErosion()
    {
        terrain = GetComponent<Terrain>();
        int size = terrain.terrainData.heightmapResolution - 1;
        height_a = CreateRenderTexture(size, size);
        outflow_a = CreateRenderTexture(size, size);
        height_b = CreateRenderTexture(size, size);
        outflow_b = CreateRenderTexture(size, size);

        Clear(height_a);
        Clear(outflow_a);
        Clear(height_b);
        Clear(outflow_b);

        Texture2D startTexture = new Texture2D(size, size, TextureFormat.RGBAFloat, false);
        float[,] pixels = terrain.terrainData.GetHeights(0, 0, size, size);
        Color[] colors = new Color[size * size];
        float[,] hardness = GetHardness(size,size);
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                colors[j * size + i] = new Color(pixels[j, i], 0, 0, hardness[i, j] * 0.1f);
            }
        }
        startTexture.SetPixels(colors);
        startTexture.Apply(false);

        Draw(startTexture, outflow_b, outflow_a, 0);
        Draw(startTexture, outflow_a, height_a, 1);

        matErosion.SetFloat("_EvaporateSpeed", evaporateSpeed);
        matErosion.SetFloat("_RainSpeed", globalRainSpeed);
        matRain.SetFloat("_Height", rainHeight);

        remainStep = simulateStep;
    }

    void SimulateStep()
    {
        //Debug.Log("step remain " + remainStep);
        Draw(height_a, outflow_a, outflow_b, 0);
        Draw(height_a, outflow_b, height_b, 1);

        Draw(height_b, outflow_b, outflow_a, 0);
        Draw(height_b, outflow_a, height_a, 1);

        float probabilityOfRain = rainPointSpeed * 1;//画雨点的概率
        while (UnityEngine.Random.Range(0.0f, 1f) < probabilityOfRain) {
            DrawRain(rainTexture, height_a, new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)), rainPointSize);
            probabilityOfRain -= 1;
        }
    }

    void UpdateTerrain()
    {
        RenderTexture.active = height_b;
        Texture2D temp = new Texture2D(height_b.width, height_b.height, TextureFormat.RGBAFloat, false);
        temp.ReadPixels(new Rect(0, 0, height_b.width, height_b.height), 0, 0);
        temp.Apply();
        Color[] pixels = temp.GetPixels();
        float[,] heights = new float[height_b.width, height_b.height];
        for (int i = 0; i < height_b.width; i++) {
            for (int j = 0; j < height_b.height; j++) {
                heights[j, i] = pixels[j * height_b.width + i].r + pixels[j * height_b.width + i].b;
            }
        }
        terrain.terrainData.SetHeights(0, 0, heights);
        RenderTexture.active = null;
    }

    void Clear(RenderTexture rt)
    {
        Graphics.SetRenderTarget(rt);
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        Graphics.SetRenderTarget(null);
    }


    void Draw(Texture srcHeight, Texture srcOutflow, RenderTexture renderTarget, int passIndex)
    {
        matErosion.SetTexture("_Outflow", srcOutflow);
        Graphics.SetRenderTarget(renderTarget);
        Graphics.Blit(srcHeight, matErosion, passIndex);
        Graphics.SetRenderTarget(null);
    }

    void DrawRain(Texture2D rainTexture, RenderTexture target, Vector2 pos, float size)
    {
        GL.PushMatrix();
        float w = 1 / size;
        float h = 1 / size;
        float x = -pos.x / size;
        float y = -pos.y / size;
        GL.LoadPixelMatrix(x, x + w, y, y + w);
        Graphics.SetRenderTarget(target);
        Graphics.DrawTexture(new Rect(0, 0, 1, 1), rainTexture, matRain);
        Graphics.SetRenderTarget(null);
        GL.PopMatrix();
    }


#endif
}
