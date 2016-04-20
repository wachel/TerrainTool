Shader "Hidden/Erosion"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;

			struct PixelOutput
			{
				float4 pos : COLOR0;
				float4 normal : COLOR1;
				float4 ambient : COLOR2;
			};

			float frag (v2f i) : SV_Target
			{
				//float4 col = tex2D(_MainTex, i.uv);
				//just invert the colors
				//col = 1 - col;
				//col *= 0.99; 
				//col.rgb *= 0.99;
				//PixelOutput o;
			    //o.pos = (1, 1, 1, 0);
				//o.normal = (1,0,0,1);
				//o.ambient = (1, 0, 1, 1);
				//return o;
				return (1,1,0,0);
			}
			ENDCG
		}
	}
}
