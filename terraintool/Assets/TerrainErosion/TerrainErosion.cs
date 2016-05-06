using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
public class TerrainErosion : MonoBehaviour
{
#if UNITY_EDITOR
    public int simulateStep = 100;

    public float rainPointSpeed = 0;
    public float rainPointSize = 0.05f;
    public float rainHeight = 0.02f;

    public float evaporateSpeed = 0.0001f;
    public float globalRainSpeed = 0.00002f;

    [HideInInspector]
    public Terrain terrain;
    public RenderTexture height_a;
    private RenderTexture outflow_a;
    private RenderTexture height_b;
    private RenderTexture outflow_b;
    private Material matErosion;
    private Material matRain;
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

    void Start()
    {
        matErosion = new Material(Shader.Find("Hidden/Erosion"));
        matRain = new Material(Shader.Find("Hidden/Rain"));
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

        Texture2D terrainHeight = new Texture2D(size, size, TextureFormat.RGBAFloat, false);
        float[,] pixels = terrain.terrainData.GetHeights(0, 0, size, size);
        Color[] colors = new Color[size * size];
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                colors[j * size + i] = new Color(pixels[j, i], 0, 0);
            }
        }
        terrainHeight.SetPixels(colors);
        terrainHeight.Apply(false);

        Draw(terrainHeight, outflow_b, outflow_a, 0);
        Draw(terrainHeight, outflow_a, height_a, 1);

        matErosion.SetFloat("_EvaporateSpeed", evaporateSpeed);
        matErosion.SetFloat("_RainSpeed", globalRainSpeed);
        matRain.SetFloat("_Height", rainHeight);

        remainStep = simulateStep;
    }

    void SimulateStep()
    {
        Debug.Log("step remain " + remainStep);
        Draw(height_a, outflow_a, outflow_b, 0);
        Draw(height_a, outflow_b, height_b, 1);

        Draw(height_b, outflow_b, outflow_a, 0);
        Draw(height_b, outflow_a, height_a, 1);

        //Draw(height_c, outflow, height, 2);

        //float probabilityOfRain = rainPointSpeed * Time.deltaTime;//画雨点的概率
        //while (Random.Range(0.0f, 1f) < probabilityOfRain) {
        //    DrawRain(rainTexture, height_a, new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f)), rainPointSize);
        //    probabilityOfRain -= 1;
        //}
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
                heights[j, i] = pixels[j * height_b.width + i].r;
            }
        }
        terrain.terrainData.SetHeights(0, 0, heights);
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
