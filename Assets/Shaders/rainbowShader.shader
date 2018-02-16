Shader "wnRainbow/Rainbow"
{
  Properties {
    _Rainbow ("Rainbow Texture", 2D) = "white" {}
    _TintOpacity ("Tint/Opacity", Color) = (1,1,1,0)
    _Fresnel ("Fresnel", Range(0.001, 10)) = 0.5
    _FresnelColor ("Fresnel Color", Color) = (0.5,0.5,0.5,1)
  }
  SubShader {
    Tags {
        "Queue"="Transparent+1"
        "RenderType"="Transparent"
    }
    Pass {
      Name "FORWARD"
      Blend One OneMinusSrcAlpha
      Cull Off
      ZWrite Off
      CGPROGRAM

      #pragma vertex vert
      #pragma fragment frag
      #pragma target 3.0

      #include "UnityCG.cginc"

      uniform sampler2D _Rainbow;
      uniform float4 _TintOpacity;
      uniform float _Fresnel;
      uniform float4 _FresnelColor;

      struct Vertex {
        float3 position;
        float3 velocity;
        float3 color;
        float3 normal;
        float2 uv;
        float life;
        float debug;
      };
      struct Index {
        int id0;
        int id1;
        int id2;
        int id3;
        int id4;
        int id5;
        int id6;
        int id7;
      };

      StructuredBuffer<Vertex> vertices;
      StructuredBuffer<Index> indices;

      struct v2f {
        float4 position: SV_POSITION;
        float3 color: TEXCOORD0;
        float3 normalDir: TEXCOORD1;
        float2 uv: TEXCOORD2;
        float4 posWorld : TEXCOORD3;
        float2 viewNormal : TEXCOORD4;
      };

      v2f vert(uint id : SV_VertexID) {
        v2f o;
        Index i = indices[id];
        o.posWorld = float4(vertices[i.id0].position, 1.0);
        o.position = mul(UNITY_MATRIX_VP, o.posWorld);
        o.color = vertices[i.id0].color;
        o.normalDir = vertices[i.id0].normal;
        o.uv = vertices[i.id0].uv;
        o.viewNormal = normalize(mul((float3x3)UNITY_MATRIX_MV, o.normalDir));
        return o;
      }

      float4 frag(v2f i, fixed facing : VFACE) : COLOR {
        float3 normalDirection = normalize(i.normalDir);
        if ( facing < 0 ) normalDirection = -normalDirection;
        float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
        fixed4 rainbow = tex2D(_Rainbow, i.uv.yx);
        // return float4(rainbow.rgb, 0.5f);
        // return float4(i.normalDir, 0.5f);

        half3 finalColor = (
          rainbow.rgb * _TintOpacity.rgb +
          rainbow.rgb * _TintOpacity.a +
          rainbow.rgb * _FresnelColor.rgb * pow(1.0 - max(0.0, dot(normalDirection, viewDirection)), _Fresnel)
        );

        // finalColor.rgb = max(0, dot(normalDirection, viewDirection));

        return fixed4(finalColor, 0.0);

      }

      ENDCG
    }
  }
}
