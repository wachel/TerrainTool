using UnityEngine;
using System.Collections;
using System;

public enum ErosionEditType
{
    Brush,
    Global,
}

[ExecuteInEditMode]
public class TerrainErosion : MonoBehaviour
{
#if UNITY_EDITOR
    public ErosionEditType editType;//brush or global
    public bool randomRaindrop;     //use raindrop

    public int simulateStep = 100;//for global

    //public float rainPointSpeed = 0.1f;
    public float rainPointSize = 1f;
    public float raindropDensity = 0.00001f;
    //public float rainHeight = 0.02f;

    public float evaporateSpeed = 0.0001f;
    public float rainSpeed = 0.00002f;

    public float brushSizeFactor = 0.5f;//control brush size
    public bool isPainting = false;


    public Terrain terrain;
    public float viewWaterDensity = 0.5f;
    public Texture2D brushPreviewTexture;
    public Vector2 brushPreviewUV;

    public RenderTexture height_a;
    private RenderTexture outflow_a;
    private RenderTexture height_b;
    private RenderTexture outflow_b;
    private Texture2D startTexture;
    private Material matErosion;
    private Material matRain;
    private Texture2D raindropTexture;
    private Texture2D globalRainTexture;
    private int globalRemainStep = 0;
    private int paintDelayStep = 0;

    public void Awake()
    {
        matErosion = new Material(Shader.Find("Hidden/Erosion"));
        matRain = new Material(Shader.Find("Hidden/Rain"));
        raindropTexture = CreateCircleTexture(64, new Color(0, 1, 0, 0), new Color(0, 0, 0, 0),true);
        brushPreviewTexture = CreateCircleTexture(64, new Color(0, 0.2f, 1, 0.8f), new Color(0, 0.2f, 1, 0),false);
        globalRainTexture = CreateColorTexture(2, new Color(0, 1, 0, 0));

        terrain = GetComponent<Terrain>();
        int size = terrain.terrainData.heightmapResolution;
        startTexture = new Texture2D(size, size, TextureFormat.RGBAFloat, false);
    }

    private RenderTexture CreateRenderTexture(int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBFloat);
        rt.generateMips = false;
        rt.useMipMap = false;
        rt.filterMode = FilterMode.Bilinear;
        rt.wrapMode = TextureWrapMode.Clamp;
        return rt;
    }

    private Texture2D CreateColorTexture(int size,Color color)
    {
        Color[] colors = new Color[size * size];
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                colors[j * size + i] = color;
            }
        }
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBAFloat, true);
        tex.SetPixels(colors);
        tex.Apply(true);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    private Texture2D CreateCircleTexture(int size, Color centerColor, Color sideColor,bool bMipmap)
    {
        Color[] colors = new Color[size * size];
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                float radius = size * 0.5f;
                float dx = (i - radius) / radius;
                float dy = (j - radius) / radius;
                float dist = (dx*dx + dy*dy) * 1.1f;
                colors[j * size + i] = Color.Lerp(centerColor, sideColor, dist);
            }
        }
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBAFloat, bMipmap);
        tex.SetPixels(colors);
        tex.Apply(bMipmap);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    private float[,] GetHardness(int width, int height)
    {
        LibNoise.Unity.Generator.Perlin generator = new LibNoise.Unity.Generator.Perlin();
        generator.Frequency = 1.0 / 6;
        generator.OctaveCount = 4;
        generator.Seed = 123498765;
        generator.Quality = LibNoise.Unity.QualityMode.Low;

        float[,] values = new float[width, height];
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                values[i, j] = (float)generator.GetValue(i, j, 0) * 0.4f + 0.5f;
            }
        }
        return values;
    }

    void Start()
    {

    }

    public void OnEnable()
    {

    }

    public void OnDisable()
    {

    }

    public void EditorUpdate(Action completeCallback)
    {
        if (isPainting) {
            paintDelayStep = 20;
            matErosion.SetFloat("_EvaporateSpeed", evaporateSpeed);
            ApplyBrush();
        }

        if (paintDelayStep > 0) {
            SimulateStep();
            paintDelayStep--;
            if (paintDelayStep == 0) {
                UpdateTerrain();
                if (completeCallback != null) {
                    completeCallback();
                }
            }
        }

        if (globalRemainStep > 0) {
            matErosion.SetFloat("_EvaporateSpeed", evaporateSpeed);
            matErosion.SetFloat("_RainSpeed", 0);

            ApplyGlobal();
            SimulateStep();

            globalRemainStep -= 1;
            if (globalRemainStep == 0) {
                UpdateTerrain();
                if (completeCallback != null) {
                    completeCallback();
                }
            }
        }
    }

    public void StartGlobalRain()
    {
        globalRemainStep = simulateStep;
    }

    public void StartErosion()
    {
        int size = terrain.terrainData.heightmapResolution;
        if (height_a == null || height_a.width != size) {
            height_a = CreateRenderTexture(size, size);
            outflow_a = CreateRenderTexture(size, size);
            height_b = CreateRenderTexture(size, size);
            outflow_b = CreateRenderTexture(size, size);
        }

        Clear(height_a);
        Clear(outflow_a);
        Clear(height_b);
        Clear(outflow_b);

        if(startTexture == null || startTexture.width != size) {
            startTexture = new Texture2D(size, size, TextureFormat.RGBAFloat, false);
        }
        
        float[,] pixels = terrain.terrainData.GetHeights(0, 0, size, size);
        Color[] colors = new Color[size * size];
       // float[,] hardness = GetHardness(size, size);
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                colors[j * size + i] = new Color(pixels[j, i], 0, 0, 0);// hardness[i, j] * 0.1f);
            }
        }
        startTexture.SetPixels(colors);
        startTexture.Apply(false);

        Draw(startTexture, outflow_b, outflow_a, 0);
        Draw(startTexture, outflow_a, height_a, 1);
    }

    public void StopErosion()
    {
        globalRemainStep = 1;
    }

    void SimulateStep()
    {
        Draw(height_a, outflow_a, outflow_b, 0);
        Draw(height_a, outflow_b, height_b, 1);

        Draw(height_b, outflow_b, outflow_a, 0);
        Draw(height_b, outflow_a, height_a, 1);
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
        pos -= Vector2.one * size * 0.5f;
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

    void ApplyBrush()
    {
        if (randomRaindrop) {
            matRain.SetFloat("_Height", GetRainHeight());
            float probabilityOfRain = raindropDensity * GetRealBrushSize() * GetRealBrushSize() * Mathf.PI;//画雨点的概率
            while (UnityEngine.Random.Range(0.0f, 1f) < probabilityOfRain) {
                float randomAngle = UnityEngine.Random.Range(0, Mathf.PI * 2);
                Vector2 randomDir = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
                float randomLenght = UnityEngine.Random.Range(0f, 1f);
                DrawRain(raindropTexture, height_a, brushPreviewUV + randomDir * randomLenght * GetRealBrushSize()/terrain.terrainData.heightmapResolution , rainPointSize/terrain.terrainData.heightmapResolution);
                probabilityOfRain -= 1;
            }
        }
        else {
            matRain.SetFloat("_Height", rainSpeed);
            DrawRain(raindropTexture, height_a, brushPreviewUV, GetRealBrushSize()*2/(float)terrain.terrainData.heightmapResolution);
        }
    }

    void ApplyGlobal()
    {
        if (randomRaindrop) {
            matRain.SetFloat("_Height", GetRainHeight());
            float probabilityOfRain = raindropDensity * terrain.terrainData.heightmapResolution * terrain.terrainData.heightmapResolution * Mathf.PI;//画雨点的概率
            while (UnityEngine.Random.Range(0.0f, 1f) < probabilityOfRain) {
                DrawRain(raindropTexture, height_a, new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)), rainPointSize / terrain.terrainData.heightmapResolution);
                probabilityOfRain -= 1;
            }
        }
        else {
            matRain.SetFloat("_Height", rainSpeed);
            DrawRain(globalRainTexture, height_a, new Vector2(0.5f,0.5f), 1);
        }
    }

    public float GetViewWaterHeight()
    {
        return Mathf.Pow(10, viewWaterDensity * 3);
    }
    public int GetRealBrushSize()
    {
        return (int)Mathf.Pow(2, brushSizeFactor * 10);
    }
    public int GetRemainStep()
    {
        return globalRemainStep;
    }
    public int GetPaintDelayStep()
    {
        return paintDelayStep;
    }
    private float GetRainHeight()
    {
        return rainSpeed / (raindropDensity * rainPointSize * rainPointSize * Mathf.PI / 3);
    }
#endif
}
