using UnityEngine;
using System.Collections;

public class ViewHeight3D : MonoBehaviour {
    public int gridNumX = 10;
    public int gridNumY = 10;

	// Use this for initialization
	void Start () {
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = new Mesh();
        Vector3[] vertices = new Vector3[gridNumX * gridNumY];
        Vector2[] uv = new Vector2[gridNumX * gridNumY];
        for (int i = 0; i<gridNumX; i++) {
            for(int j = 0;j<gridNumY; j++) {
                vertices[j * gridNumX + i] = new Vector3(i / (float)gridNumX, 0, j / (float)gridNumY);
                uv[j * gridNumX + i] = new Vector2(i / (float)gridNumX, j / (float)gridNumY);
            }
        }
        int[] triangles = new int[(gridNumX - 1) * (gridNumY - 1) * 6];
        for(int i = 0; i<gridNumX - 1; i++) {
            for(int j = 0;j<gridNumY - 1; j++) {
                int start = (j * (gridNumX - 1) + i) * 6;
                int w = gridNumX;
                triangles[start + 0] = (j + 0) * w + (i + 0);
                triangles[start + 1] = (j + 1) * w + (i + 0);
                triangles[start + 2] = (j + 0) * w + (i + 1);
                triangles[start + 3] = (j + 0) * w + (i + 1);
                triangles[start + 4] = (j + 1) * w + (i + 0);
                triangles[start + 5] = (j + 1) * w + (i + 1);
            }
        }
        mf.mesh.vertices = vertices;
        mf.mesh.uv = uv;
        mf.mesh.triangles = triangles;
        mf.mesh.UploadMeshData(true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
