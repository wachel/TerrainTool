using UnityEngine;
using System.Collections;

public class TestErosion : MonoBehaviour
{
    public Texture2D startTexture;

    public RenderTexture height_a;
    public RenderTexture outflow_a;
    public RenderTexture height_b;
    public RenderTexture outflow_b;

    public Texture2D rainTexture;
    public Material rainMaterial;
    public float rainPointSpeed = 0;
    public float rainPointSize = 0.05f;
    public float rainHeight = 0.02f;

    public float evaporateSpeed = 0.0001f;
    public float globalRainSpeed = 0.00002f;


    public Material mat;
    int num = 0;

    RenderTexture createTexture(int width,int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBFloat);
        rt.autoGenerateMips = false;
        rt.useMipMap = false;
        rt.filterMode = FilterMode.Point;
        rt.wrapMode = TextureWrapMode.Clamp;
        return rt;
    }

    void Start()
    {
        //height_b = createTexture(height.width, height.height);
        //height_c = createTexture(height.width, height.height);
        //outflow_b = createTexture(height.width, height.height);
        

        Clear(height_a);
        Clear(outflow_a);
        Clear(height_b);
        Clear(outflow_b);

        Draw(startTexture, outflow_b, outflow_a, 0);
        Draw(startTexture, outflow_a, height_a, 1);
    }

    public void OnDestroy()
    {
        Clear(height_a);
        Clear(outflow_a);
        Clear(height_b);
        Clear(outflow_b);
    }

    void Clear(RenderTexture rt)
    {
        Graphics.SetRenderTarget(rt);
        GL.Clear(true, true, new Color(0,0,0,0));
        Graphics.SetRenderTarget(null);
    }

    void Draw(Texture srcHeight,Texture srcOutflow, RenderTexture renderTarget,int passIndex)
    {
        mat.SetTexture("_Outflow", srcOutflow);
        Graphics.SetRenderTarget(renderTarget);
        Graphics.Blit(srcHeight, mat, passIndex);
        Graphics.SetRenderTarget(null);
    }

    void DrawRain(Texture2D rainTexture, RenderTexture target,Vector2 pos,float size)
    {
        GL.PushMatrix();
        float w = 1 / size;
        float h = 1 / size;
        float x = -pos.x / size;
        float y = -pos.y / size;
        GL.LoadPixelMatrix(x,x + w,y,y+w);
        Graphics.SetRenderTarget(target);
        Graphics.DrawTexture(new Rect(0,0,1,1), rainTexture, rainMaterial);
        Graphics.SetRenderTarget(null);
        GL.PopMatrix();
    }

    void Update()
    {
        mat.SetFloat("_EvaporateSpeed", evaporateSpeed);
        mat.SetFloat("_RainSpeed", globalRainSpeed);
        rainMaterial.SetFloat("_Height", rainHeight);
        for (int i = 0; i < 1; i++) {
            Draw(height_a, outflow_a, outflow_b, 0);
            Draw(height_a, outflow_b, height_b, 1);

            Draw(height_b, outflow_b, outflow_a, 0);
            Draw(height_b, outflow_a, height_a, 1);

            //Draw(height_c, outflow, height, 2);

            float probabilityOfRain = rainPointSpeed * Time.deltaTime;//画雨点的概率
            while (Random.Range(0.0f, 1f) < probabilityOfRain) {
                DrawRain(rainTexture, height_a, new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f)), rainPointSize);
                probabilityOfRain -= 1;
            }
        }
    }
}
