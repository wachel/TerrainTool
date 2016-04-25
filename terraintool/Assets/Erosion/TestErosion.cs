using UnityEngine;
using System.Collections;

public class TestErosion : MonoBehaviour
{
    public Texture2D startTexture;
    public RenderTexture[] rt0 = new RenderTexture[3];
    private RenderTexture[] rt1 = new RenderTexture[3];

    public Material mat;
    int num = 0;

    private RenderBuffer[] rb0 = new RenderBuffer[3];
    private RenderBuffer[] rb1 = new RenderBuffer[3];
    private RenderBuffer depthBuffer;

    void Start()
    {
        for (int i = 0; i<3; i++) {
            rb0[i] = rt0[i].colorBuffer;
            rt0[i].filterMode = FilterMode.Point;

            rt1[i] = new RenderTexture(rt0[0].width, rt0[0].height, 24, RenderTextureFormat.ARGBFloat);
            rt1[i].generateMips = false;
            rt1[i].useMipMap = false;
            rt1[i].filterMode = FilterMode.Point;
            rb1[i] = rt1[i].colorBuffer;
        }
        depthBuffer = rt0[0].depthBuffer;

        Clear(rt0);
        Clear(rt1);

        mat.SetTexture("_MainTex", startTexture);
        Graphics.SetRenderTarget(rb0, depthBuffer);
        Graphics.Blit(startTexture, mat);
        Graphics.SetRenderTarget(null);
    }

    public void OnDestroy()
    {
        Clear(rt0);
        Clear(rt1);
    }

    void Clear(RenderTexture[] rts)
    {
        for(int i =0; i<rts.Length; i++) {
            Graphics.SetRenderTarget(rts[i]);
            GL.Clear(true, true, Color.black);
        }
        Graphics.SetRenderTarget(null);
    }

    void Draw(RenderTexture[] srcTextuers, RenderBuffer[] targetBuffers)
    {
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, 1, 1, 0);

        mat.SetTexture("_Outflow", srcTextuers[1]);
        mat.SetTexture("_Velocity", srcTextuers[2]);
        Graphics.SetRenderTarget(targetBuffers, depthBuffer);
        Graphics.DrawTexture(new Rect(0, 0, 1, 1), srcTextuers[0], mat);

        GL.PopMatrix();
        Graphics.SetRenderTarget(null);
    }

    void Update()
    {
        Draw(rt0, rb1);
        Draw(rt1, rb0);
    }
}

//public class TestErosion : MonoBehaviour
//{
//    private RenderBuffer[] mrtRB = new RenderBuffer[3];
//    private Camera cam = null;
//
//    public Material mat;
//    public Texture2D start;
//    public RenderTexture target0;
//    public RenderTexture target1;
//    public RenderTexture target2;
//    int count = 0;
//    void Start()
//    {
//        cam = GetComponent<Camera>();
//        mrtRB[0] = target0.colorBuffer;
//        mrtRB[1] = target1.colorBuffer;
//        mrtRB[2] = target2.colorBuffer;
//        Graphics.SetRenderTarget(target0);
//        Graphics.DrawTexture(new Rect(0, 0, target0.width, target0.height), target0, mat);
//    }
//
//    //void OnPreRender()
//    //{
//    //    //cam.Render();
//    //}
//    //
//    //void OnRenderImage(RenderTexture src, RenderTexture dest)
//    //{
//    //    if (count == 0) {
//    //        Graphics.Blit(start, dest, mat);
//    //    }
//    //    else {
//    //        Graphics.Blit(src, dest, mat);
//    //    }
//    //    //count++;
//    //}
//}


//public class TestErosion : MonoBehaviour
//{
//    public Material mat;
//    public Texture2D start;
//    private RenderBuffer[] colorBuffers = new RenderBuffer[3];
//    private RenderBuffer[] depthBuffer = new RenderBuffer();
//    int count = 0;
//    Camera cam;
//
//    public void Start()
//    {
//        bool rlt = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat);
//        cam = GetComponent<Camera>();
//        cam.SetTargetBuffers(targets,depth);
//    }
//
//
//
//
//    //public void OnRenderObject()
//    //{
//    //    RenderTexture old = RenderTexture.active;
//    //    if (count == 0) {
//    //        Graphics.SetRenderTarget(target0);
//    //        mat.mainTexture = start;
//    //        Graphics.DrawTexture(new Rect(0, 0, target0.width, target0.height), start, mat);
//    //    }
//    //    else {
//    //        if (count % 2 == 1) {
//    //            Graphics.SetRenderTarget(target1);
//    //            mat.mainTexture = target0;
//    //            Graphics.DrawTexture(new Rect(0, 0, target1.width, target1.height), target0, mat);
//    //        }
//    //        else {
//    //            Graphics.SetRenderTarget(target0);
//    //            mat.mainTexture = target1;
//    //            Graphics.DrawTexture(new Rect(0, 0, target0.width, target0.height), target1, mat);
//    //        }
//    //    }
//    //    count++;
//    //    Graphics.SetRenderTarget(old);
//    //}
//    //public Material mat;
//    void OnRenderImage(RenderTexture src, RenderTexture dest)
//    {
//        if (count == 0) {
//            Graphics.Blit(start, dest, mat);
//        }
//        else {
//            Graphics.Blit(src, dest, mat);
//        }
//        count++;
//    }
//
//}
