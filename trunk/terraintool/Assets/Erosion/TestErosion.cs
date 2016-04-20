using UnityEngine;
using System.Collections;

public class TestErosion : MonoBehaviour
{
    private RenderBuffer[] mrtRB = new RenderBuffer[3];
    private Camera cam = null;

    public Material mat;
    public Texture2D start;
    public RenderTexture target0;
    public RenderTexture target1;
    public RenderTexture target2;
    int count = 0;
    void Start()
    {
        cam = GetComponent<Camera>();
        mrtRB[0] = target0.colorBuffer;
        mrtRB[1] = target1.colorBuffer;
        mrtRB[2] = target2.colorBuffer;
        Graphics.SetRenderTarget(target0);
        Graphics.DrawTexture(new Rect(0, 0, target0.width, target0.height), target0, mat);
    }

    //void OnPreRender()
    //{
    //    //cam.Render();
    //}
    //
    //void OnRenderImage(RenderTexture src, RenderTexture dest)
    //{
    //    if (count == 0) {
    //        Graphics.Blit(start, dest, mat);
    //    }
    //    else {
    //        Graphics.Blit(src, dest, mat);
    //    }
    //    //count++;
    //}
}


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
