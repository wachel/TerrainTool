Shader "Hidden/Erosion"
{
	Properties
	{
		_MainTex ("TerrainStart", 2D) = "white" {}
		_Outflow("Temp0",2D) = "black" {}
		_Velocity("Temp1",2D) = "black" {}
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
			#pragma enable_d3d11_debug_symbols

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
			sampler2D _Outflow;
			sampler2D _Velocity;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _Outflow_TexelSize;
			uniform half4 _Velocity_TexelSize;

			struct PixelOutput
			{
				float4 height: COLOR0;//x:terrain,y:water,z:suspended sediment
				float4 outflow : COLOR1;//x:Left,y:Right,z:Bottom,w:Top
				float4 velocity : COLOR2;
			};


			PixelOutput frag (v2f i)
			{

				//current
			    float4 height = tex2D(_MainTex, i.uv);
				float2 velocity = tex2D(_Velocity, i.uv).xy;
				float4 outflow = tex2D(_Outflow, i.uv);

				//neighbour
				float4 heightL = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(-1,0));
				float4 heightR = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2( 1,0));
				float4 heightB = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0,-1));
				float4 heightT = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0, 1));
																							
				float4 inflowL = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(-1,0));
				float4 inflowR = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2( 1,0));
				float4 inflowB = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(0,-1));
				float4 inflowT = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(0, 1));

				//name
				float terrainHeight = height.x;
				float waterHeight = height.y;
				float totalHeight = terrainHeight + waterHeight;

				//add by inflow
				//inflow
				float totalInflow = inflowL.y + inflowR.x + inflowB.w + inflowT.z;
				float totalOutflow = outflow.x + outflow.y + outflow.z + outflow.w;
				float newWaterHeight = waterHeight + (totalInflow - totalOutflow);
				newWaterHeight = max(newWaterHeight, 0);

				//deltaTop
				float diffHeightL = totalHeight - (heightL.x + heightL.y);
				float diffHeightR = totalHeight - (heightR.x + heightR.y);
				float diffHeightB = totalHeight - (heightB.x + heightB.y);
				float diffHeightT = totalHeight - (heightT.x + heightT.y);

				float flowdamp = 0.9;

				outflow *= flowdamp;
				outflow.x = max(outflow.x + diffHeightL * 0.08, 0.000000001);
				outflow.y = max(outflow.y + diffHeightR * 0.08, 0.000000001);
				outflow.z = max(outflow.z + diffHeightB * 0.08, 0.000000001);
				outflow.w = max(outflow.w + diffHeightT * 0.08, 0.000000001);

				float outflowScale = newWaterHeight / (outflow.x + outflow.y + outflow.z + outflow.w);
				outflowScale = min(1, outflowScale);
				outflow *= outflowScale;
			
				PixelOutput dest;
				dest.height = float4(terrainHeight, newWaterHeight, 0, 0);
				dest.outflow = outflow;
				dest.velocity = float4(velocity, inflowL.y + inflowR.x + inflowB.w + inflowT.z, 0);
				return dest;
			}
			ENDCG
		}
	}
}
