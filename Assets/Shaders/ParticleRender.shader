Shader "Custom/ParticleRender"
{
  Properties {
    _MainTex("Base (RGB)", 2D) = "white" {}
    _Color("Color", Color) = (1, 0.5, 0.5, 1)
    _Size("Size", Float) = 100.0
    _MinSize("Min Size", Float) = 1.0
  }

  SubShader {
    Pass {
      Tags { "RenderType" = "Transparant" }
      ZTest On
      ZWrite Off
      Cull Off
      Blend SrcAlpha One

      CGPROGRAM
        #pragma vertex particle_vertex
        #pragma fragment frag

        #pragma target 5.0
        #include "UnityCG.cginc"

        struct Particle
        {
          float3 position;
          float3 velocity;
          float3 color;
        };

        StructuredBuffer<Particle> particles;
        StructuredBuffer<float3> quadPoints;

        sampler2D _MainTex;
        float4 _Color;
        float _Size;
        float _MinSize;

        struct v2f
        {
          float4 pos : SV_POSITION;
          float2 uv : TEXCOORD0;
          float4 color : COLOR;
          float size : PSIZE;
        };

        v2f particle_vertex(uint id : SV_VertexID, uint inst : SV_InstanceID)
        {
          v2f o;

          float3 quadPoint = quadPoints[id];
          float3 worldPosition = particles[inst].position;

          float2 pixelScale = float2(1.0f / _ScreenParams.x, 1.0f / _ScreenParams.y);
          float2 size = quadPoint.xy * (float2)_Size;
          float2 minSize = quadPoint.xy * pixelScale.xy * (float2)_MinSize;
          float minRadius = length(minSize);

          float4 viewPos = mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(size, 0.0f, 0.0f);
          float4 screenPos = mul(UNITY_MATRIX_P, viewPos);

          float4 viewPos2 = mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f));
          float4 screenPos2 = mul(UNITY_MATRIX_P, viewPos2);

          o.pos = screenPos;
          o.pos.xy += minSize * float2(o.pos.z, 1.0-o.pos.z);

          // Project the vertices to far clipping plane in "correct" coordinates.
          o.pos.xyz /= o.pos.w;
          o.pos.z = min(o.pos.z, 1.0);
          o.pos.xyz *= o.pos.w;

          o.uv = quadPoint / 2.0f + 0.5f;
          o.color = float4(particles[inst].color, 1.0f) * _Color;

          float dist = length(screenPos2.xy - screenPos.xy) / screenPos.z;
          if (dist < minRadius) {
            o.color = lerp(o.color, float4(o.color.rgb, 0.0f), (minRadius - dist) / minRadius);
          }

          return o;
        }

        float4 frag(v2f i) : COLOR
        {
          float4 texCol = tex2Dbias(_MainTex, float4(i.uv, 0.0f, -1.0f)).aaaa;
          float4 partCol = i.color;
          /*return partCol;*/
          return float4(1.0f - (1.0f - texCol.rgb) * (1.0f - partCol.rgb), texCol.a * 2.0f); //screen blend mode
        }
      ENDCG

    }
  }
}
