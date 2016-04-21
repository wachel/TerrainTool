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
			uniform half4 _MainTex_TexelSize;  

			struct PixelOutput
			{
				float4 height: COLOR0;//x:terrain,y:water,z:suspended sediment
				float4 outflow : COLOR1;
				float4 velocity : COLOR2;
			};

			//PixelOutput frag (v2f i)
			//{
			//	PixelOutput old;
			//    old.height = tex2D(_MainTex, i.uv);
			//	old.outflow = tex2D(_OutFlow, i.uv);
			//	old.velocity = tex2D(_Velocity, i.uv);
			//	return old;
			//}

			PixelOutput frag (v2f i)
			{
				PixelOutput dest;

			    float4 height = tex2D(_MainTex, i.uv);
				float4 velocity = tex2D(_Velocity, i.uv);

				float4 heightL = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(-1,0));
				float4 heightR = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(1,0));
				float4 heightB = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0,-1));
				float4 heightT = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0,1));

				float4 outflowL = tex2D(_OutFlow,i.uv + _OutFlow_TexelSize.xy * half2(-1,0));
				float4 outflowR = tex2D(_OutFlow,i.uv + _OutFlow_TexelSize.xy * half2(1,0));
				float4 outflowB = tex2D(_OutFlow,i.uv + _OutFlow_TexelSize.xy * half2(0,-1));
				float4 outflowT = tex2D(_OutFlow,i.uv + _OutFlow_TexelSize.xy * half2(0,1));

				float terrainHeight = src.height.x;
				float waterHeight = src.height.y;

				dest.height =  tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0,1));    
				dest.height += tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0,-1));    
				dest.height += tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(1,0));    
				dest.height += tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(-1,0));
				dest.height *= 0.25f;
				dest.outflow = src.outflow;
				dest.velocity = src.velocity;
				return dest;
			}
			

			ENDCG
		}
	}
}
