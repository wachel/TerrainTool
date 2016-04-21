Shader "Hidden/Erosion"
{
	Properties
	{
		_MainTex ("TerrainStart", 2D) = "white" {}
		_OutFlow("Temp0",2D) = "white" {}
		_Velocity("Temp1",2D) = "white" {}
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
			#pragma target 3.0

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
			sampler2D _OutFlow;
			sampler2D _Velocity;

			struct PixelOutput
			{
				float4 height: COLOR0;//x:terrain,y:water,z:suspended sediment
				float4 outflow : COLOR1;
				float4 velocity : COLOR2;
			};

			PixelOutput frag (v2f i)
			{
				PixelOutput o;
			    o.height = tex2D(_MainTex, i.uv) * 0.93;
				o.outflow = tex2D(_OutFlow, i.uv) * 0.95;
				o.velocity = tex2D(_Velocity, i.uv) * 0.99;
				return o;
			}



			ENDCG
		}
	}
}
