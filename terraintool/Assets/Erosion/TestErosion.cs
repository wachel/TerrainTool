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
    void Start()
    {
        rb0[0] = rt0[0].colorBuffer;
        rb0[1] = rt0[1].colorBuffer;
        rb0[2] = rt0[2].colorBuffer;

        for (int i = 0; i<3; i++) {
            rt1[i] = new RenderTexture(rt0[0].width, rt0[0].height, 24, RenderTextureFormat.ARGBFloat);
            rb1[i] = rt1[i].colorBuffer;
        }
        mat.SetTexture("_MainTex", startTexture);
        mat.SetTexture("_OutFlow", startTexture);
        mat.SetTexture("_Velocity", startTexture);

        Graphics.SetRenderTarget(rb0, rt0[0].depthBuffer);
        Graphics.Blit(startTexture, mat);
        Graphics.SetRenderTarget(null);
    }

    public void OnDestroy()
    {
        Graphics.SetRenderTarget(rt0[0]);
        GL.Clear(true, true, Color.black);
        Graphics.SetRenderTarget(rt0[1]);
        GL.Clear(true, true, Color.black);
        Graphics.SetRenderTarget(rt0[2]);
        GL.Clear(true, true, Color.black);
    }


    void Update()
    {
        GL.PushMatrix();                             
        GL.LoadPixelMatrix(0, 512, 512, 0);

        if (num % 2 == 0) {
            mat.SetTexture("_OutFlow", rt0[1]);
            mat.SetTexture("_Velocity", rt0[2]);
            Graphics.SetRenderTarget(rb1,rt1[0].depthBuffer);
            Graphics.DrawTexture(new Rect(0, 0, 512, 512), rt0[0], mat);
        }
        else {
            mat.SetTexture("_OutFlow", rt1[1]);
            mat.SetTexture("_Velocity", rt1[2]);
            Graphics.SetRenderTarget(rb0, rt0[0].depthBuffer);
            Graphics.DrawTexture(new Rect(0, 0, 512, 512), rt1[0], mat);
        }

        GL.PopMatrix();
        Graphics.SetRenderTarget(null);
    
        num++;                                                             
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
