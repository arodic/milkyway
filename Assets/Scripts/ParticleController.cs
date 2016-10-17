using UnityEngine;
using System.Collections;


public class ParticleController : MonoBehaviour
{
  #region variables
  public ComputeShader ParticleCalculation;
  public Material ParticleMaterial;
  public int NumParticles = 500000;
  public float Radius = 10.0f;
  public float StartSpeed = 4.0f;
  public Texture2D HueTexture;

  private const int c_groupSize = 128;
  private int m_updateParticlesKernel;
  #endregion

  #region particleStruct
  //Notice that this struct has to match the on in the compute shader exactly.
  struct Particle
  {
    public Vector3 position;
    public Vector3 velocity;

    public Vector3 color; //Color is a float3 in the compute shader, we need a Vector3 to match that layout, not a Color!
  };
  #endregion

  #region buffers
  private ComputeBuffer m_particlesBuffer;
  private const int c_particleStride = 36;

  private ComputeBuffer m_quadPoints;
  private const int c_quadStride = 12;

  #endregion



  #region setup
  // Use this for initialization
  void Start()
  {
    //Find compute kernel
    m_updateParticlesKernel = ParticleCalculation.FindKernel("UpdateParticles");

    //Create particle buffer
    m_particlesBuffer = new ComputeBuffer(NumParticles, c_particleStride);

    Particle[] particles = new Particle[NumParticles];

    for (int i = 0; i < NumParticles; ++i)
    {
      particles[i].position = Random.insideUnitSphere * Radius;
      particles[i].velocity = Random.insideUnitSphere * StartSpeed;
      particles[i].color = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
    }

    m_particlesBuffer.SetData(particles);

    //Create quad buffer
    m_quadPoints = new ComputeBuffer(6, c_quadStride);

    m_quadPoints.SetData(new[] {
      new Vector3(-1.0f, 1.0f, 0.0f),
      new Vector3(1.0f, 1.0f, 0.0f),
      new Vector3(1.0f, -1.0f, 0.0f),
      new Vector3(1.0f, -1.0f, 0.0f),
      new Vector3(-1.0f, -1.0f, 0.0f),
      new Vector3(-1.0f, 1.0f, 0.0f)
    });
  }
  #endregion

  #region Compute update
  // Update is called once per frame
  void Update()
  {
    //Bind resources to compute shader
    ParticleCalculation.SetBuffer(m_updateParticlesKernel, "particles", m_particlesBuffer);
    ParticleCalculation.SetFloat("deltaTime", Time.deltaTime);
    ParticleCalculation.SetTexture(m_updateParticlesKernel, "HueTexture", HueTexture);

    //Dispatch, launch threads on GPU
    int numberOfGroups = Mathf.CeilToInt((float)NumParticles / c_groupSize);
    ParticleCalculation.Dispatch(m_updateParticlesKernel, numberOfGroups, 1, 1);
  }
  #endregion

  #region rendering
  void OnRenderObject()
  {
    //Bind resources to material
    ParticleMaterial.SetBuffer("particles", m_particlesBuffer);
    ParticleMaterial.SetBuffer("quadPoints", m_quadPoints);

    //Set the pass
    ParticleMaterial.SetPass(0);

    //Draw
    Graphics.DrawProcedural(MeshTopology.Triangles, 6, NumParticles);
  }
  #endregion

  #region cleanup
  void OnDestroy()
  {
    m_particlesBuffer.Release();
    m_quadPoints.Release();
  }
  #endregion
}
