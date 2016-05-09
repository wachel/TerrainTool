Shader "Hidden/ErosionBrushPreview" {
	Properties {
		_MainTex ("MainTex", 2D) = "gray" {}
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
			
			float4x4 _Projector;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, vertex);
				o.uvShadow = mul (_Projector, vertex);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			sampler2D _MainTex;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2Dproj (_MainTex, UNITY_PROJ_COORD(i.uvShadow));
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, unity_FogColor);
				return col;
			}
			ENDCG
		}
	}
}
