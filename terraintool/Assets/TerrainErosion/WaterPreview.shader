// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

Shader "Hidden/WaterPreview" {
	Properties {
		_MainTex ("MainTex", 2D) = "gray" {}
		_Scale("Scale",float) = 1
	}
	Subshader {
		Tags {"Queue"="Transparent"}
		Pass {
			ZWrite Off
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			
			struct v2f {
				float4 uvShadow : TEXCOORD0;
				UNITY_FOG_COORDS(2)
				float4 pos : SV_POSITION;
			};
			
			float4x4 unity_Projector;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (vertex);
				o.uvShadow = mul (unity_Projector, vertex);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			sampler2D _MainTex;
			float _Scale;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2Dproj (_MainTex, UNITY_PROJ_COORD(i.uvShadow));
				fixed4 res = fixed4(0, 0.2, 1, min(0.7,col.g * _Scale));
				UNITY_APPLY_FOG_COLOR(i.fogCoord, res, unity_FogColor);
				return res;
			}
			ENDCG
		}
	}
}
