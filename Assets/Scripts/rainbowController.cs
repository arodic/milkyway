using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class rainbowController : MonoBehaviour {

  [Range(1, 100)]
  public int xSize = 1;
  [Range(1, 100)]
	public int ySize = 1;
  [Range(1, 50)]
	public int resolution = 20;
  [Range(0, 1)]
	public float damping = 0.1f;
  [Range(0, 2)]
	public float curl = 1f;
  [Range(0, 1)]
	public float curlFreq = 1f;
  [Range(0, 2)]
	public float spring = 1f;
  [Range(0, 2)]
	public float smooth = 1f;

  public ComputeShader computeShader;
  public Material ParticleMaterial;

  [Range(0, 100)]
	public float life = 10f;
	public Vector3 force0 = new Vector3(1,0,0);
	public Vector3 force1 = new Vector3(0,1,0);
	public Vector3 force2 = new Vector3(-1,0,0);
	public Vector3 force3 = new Vector3(0,-1,0);

  private const int computeGroupSize = 128;
  private int computeKernel;
  private int numberOfGroups;
  private int resX;
  private int resY;
  private int resX1;
  private int resY1;


  private ComputeBuffer vertexBuffer;
  private const int vertexStride = 64;
  struct Vertex {
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 color;
    public Vector3 normal;
    public Vector2 uv;
    public float life;
    public int id;
  };
  private int vertexCount;

  private ComputeBuffer indexBuffer;
  private const int indexStride = 32;
  struct Index {
    public int id0;
    public int id1;
    public int id2;
    public int id3;
    public int id4;
    public int id5;
    public int id6;
    public int id7;
  };
  private int indexCount;

  void Start() {
    Generate();
  }

  void Generate() {
    Dispose();

    resX = xSize * resolution;
    resY = ySize * resolution;
    resX1 = xSize * resolution + 1;
    resY1 = ySize * resolution + 1;
    vertexCount = resX1 * resY1;
    indexCount = resX1 * resY1 * 6;

    computeKernel = computeShader.FindKernel("CSMain");
    vertexBuffer = new ComputeBuffer(vertexCount, vertexStride);
    indexBuffer = new ComputeBuffer(indexCount, indexStride);

    Vertex[] vertices = new Vertex[vertexCount];
    for (int z = 0; z < resY1; z++) {
      float yPos = ((float)z / resY - .5f) * (float)ySize;
      for (int x = 0; x < resX1; x++) {
        float xPos = ((float)x / resX - .5f) * (float)xSize;
        vertices[x + z * resX1].position = new Vector3( xPos, yPos, 0f );
        vertices[x + z * resX1].velocity = new Vector3( 0f, 0f, 0f );
        vertices[x + z * resX1].color = Random.insideUnitSphere * 10.0f;
        vertices[x + z * resX1].normal = Vector3.forward;
        vertices[x + z * resX1].uv = new Vector2( (float)x / resX, (float)z / resY );
        vertices[x + z * resX1].life = -(float)x / resX;
        vertices[x + z * resX1].id = (int)(x + resX1 * z) * 6;
      }
    }
    vertexBuffer.SetData(vertices);

    Index[] indices = new Index[indexCount];
    int t = 0;
    for (int v = 0; v <= resY; v++) {
      for (int u = 0; u <= resX; u++) {
        int vp  = Mathf.Min(( v + 1 ), resY);
        int vpp = Mathf.Min(( v + 2 ), resY);
        int vm  = Mathf.Max(( v - 1 ), 0);
        int up  = Mathf.Min(( u + 1 ), resX);
        int upp = Mathf.Min(( u + 2 ), resX);
        int um  = Mathf.Max(( u - 1 ), 0);

        int a =  u + resX1 * v;
        int b =  u + resX1 * vp;
        int c = up + resX1 * vp;
        int d = up + resX1 * v;

        int e =  u + resX1 * vm;
        int f = um + resX1 * v;
        int g = up + resX1 * vpp;
        int h = upp + resX1 * vp;
        int i = um + resX1 * vp;
        int j =  u + resX1 * vpp;
        int k = upp + resX1 * v;
        int l = up + resX1 * vm;

        indices[t].id0 = a;
        indices[t].id1 = f;
        indices[t].id2 = b;
        indices[t].id3 = d;
        indices[t].id4 = e;
        t++;
        indices[t].id0 = b;
        indices[t].id1 = i;
        indices[t].id2 = j;
        indices[t].id3 = c;
        indices[t].id4 = a;
        t++;
        indices[t].id0 = d;
        indices[t].id1 = a;
        indices[t].id2 = c;
        indices[t].id3 = k;
        indices[t].id4 = l;
        t++;
        indices[t].id0 = b;
        indices[t].id1 = i;
        indices[t].id2 = j;
        indices[t].id3 = c;
        indices[t].id4 = a;
        t++;
        indices[t].id0 = c;
        indices[t].id1 = b;
        indices[t].id2 = g;
        indices[t].id3 = h;
        indices[t].id4 = d;
        t++;
        indices[t].id0 = d;
        indices[t].id1 = a;
        indices[t].id2 = c;
        indices[t].id3 = k;
        indices[t].id4 = l;
        t++;

      }
    }
    indexBuffer.SetData(indices);
  }

  void Dispose() {
    if (vertexBuffer != null) vertexBuffer.Dispose();
    if (indexBuffer != null) indexBuffer.Dispose();
    vertexBuffer = null;
    indexBuffer = null;
  }

  void FixedUpdate() {
    if (vertexBuffer == null && indexBuffer == null) {
      Generate();
    } else if (resX != xSize * resolution || resY != ySize * resolution) {
      Generate();
    }
    computeShader.SetBuffer(computeKernel, "indices", indexBuffer);
    computeShader.SetBuffer(computeKernel, "vertices", vertexBuffer);
    computeShader.SetFloat("_Time", Time.time);
    computeShader.SetFloat("_Delta", Time.deltaTime);
    computeShader.SetFloat("_Resolution", resolution);
    computeShader.SetFloat("_Damping", damping);
    computeShader.SetFloat("_Curl", curl);
    computeShader.SetFloat("_CurlFreq", curlFreq);
    computeShader.SetFloat("_Spring", spring);
    computeShader.SetFloat("_Smooth", smooth);
    computeShader.SetFloat("_Life", life);
    computeShader.SetVector("_Force0", force0);
    computeShader.SetVector("_Force1", force1);
    computeShader.SetVector("_Force2", force2);
    computeShader.SetVector("_Force3", force3);
    // numberOfGroups = Mathf.CeilToInt((float)indexCount / computeGroupSize);
    numberOfGroups = Mathf.CeilToInt((float)vertexCount / computeGroupSize);
    computeShader.Dispatch(computeKernel, numberOfGroups, 1, 1);
  }

  void OnRenderObject() {
    ParticleMaterial.SetBuffer("indices", indexBuffer);
    ParticleMaterial.SetBuffer("vertices", vertexBuffer);
    ParticleMaterial.SetPass(0);
    Graphics.DrawProcedural(MeshTopology.Triangles, indexCount);
  }

  void OnApplicationQuit() {
    Dispose();
  }
  void OnDestroy() {
    Dispose();
  }
  void OnDisable() {
    Dispose();
  }
  void Reset() {
    Dispose();
  }

}
