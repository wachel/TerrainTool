Shader "Hidden/Erosion"
{
	Properties
	{
		_MainTex ("TerrainStart", 2D) = "white" {}
		_InFlow("Temp0",2D) = "black" {}
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
			sampler2D _InFlow;
			sampler2D _Velocity;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _InFlow_TexelSize;
			uniform half4 _Velocity_TexelSize;

			struct PixelOutput
			{
				float4 height: COLOR0;//x:terrain,y:water,z:suspended sediment
				float4 outflow : COLOR1;//x:Left,y:Bottom,z:Right,w:Top
				float4 velocity : COLOR2;
			};

			float GetOutFlow(float terrainHeight, float waterHeight, float targetHeight, float speedOut)
			{
				float outflowFactor = 0.65;
				float diff = (terrainHeight + waterHeight) + speedOut  - targetHeight;
				float outflow = clamp(diff * outflowFactor, 0, waterHeight) * 0.25;
				return outflow;
			}

			PixelOutput frag (v2f i)
			{
				PixelOutput dest;

				//current
			    float4 height = tex2D(_MainTex, i.uv);
				float2 velocity = tex2D(_Velocity, i.uv).xy;

				//neighbour
				float4 heightL = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(-1,0));
				float4 heightR = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2( 1,0));
				float4 heightB = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0,-1));
				float4 heightT = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0, 1));

				//inflow
				float4 inflowL = tex2D(_InFlow,i.uv + _InFlow_TexelSize.xy * half2(-1,0));
				float4 inflowR = tex2D(_InFlow,i.uv + _InFlow_TexelSize.xy * half2( 1,0));
				float4 inflowB = tex2D(_InFlow,i.uv + _InFlow_TexelSize.xy * half2(0,-1));
				float4 inflowT = tex2D(_InFlow,i.uv + _InFlow_TexelSize.xy * half2(0, 1));

				//velocity
				float2 velocityL = tex2D(_Velocity, i.uv + _Velocity_TexelSize.xy * half2(-1, 0));
				float2 velocityR = tex2D(_Velocity, i.uv + _Velocity_TexelSize.xy * half2( 1, 0));
				float2 velocityB = tex2D(_Velocity, i.uv + _Velocity_TexelSize.xy * half2(0, -1));
				float2 velocityT = tex2D(_Velocity, i.uv + _Velocity_TexelSize.xy * half2(0,  1));

				//name
				float terrainHeight = height.x;
				float waterHeight = height.y;
				float sufaceHeight = terrainHeight + waterHeight;

				float speed = length(velocity);
				float2 dir = float2(sign(velocity.x),sign(velocity.y));

				//deltaTop
				float diffSufaceHeightL = sufaceHeight - (heightL.x + heightL.y) + speed * speed * (-dir.x);
				float diffSufaceHeightR = sufaceHeight - (heightR.x + heightR.y) + speed * speed * ( dir.x);
				float diffSufaceHeightB = sufaceHeight - (heightB.x + heightB.y) + speed * speed * (-dir.y);
				float diffSufaceHeightT = sufaceHeight - (heightT.x + heightT.y) + speed * speed * ( dir.y);

				//add by inflow
				float waterHeightAfterInflow = waterHeight + inflowL.z + inflowR.x + inflowB.w + inflowT.y;//add by inflow

				//outflow
				float outflowL = clamp(diffSufaceHeightL * 0.5, 0, waterHeight) * 0.25;
				float outflowR = clamp(diffSufaceHeightR * 0.5, 0, waterHeight) * 0.25; 
				float outflowB = clamp(diffSufaceHeightB * 0.5, 0, waterHeight) * 0.25;
				float outflowT = clamp(diffSufaceHeightT * 0.5, 0, waterHeight) * 0.25; 

				//new water height
				float waterHeightAfterOutflow = waterHeightAfterInflow - (outflowL + outflowR + outflowB + outflowT);
				waterHeightAfterOutflow = max(waterHeightAfterOutflow, 0);
				
				//new velocity
				float velocityDamp = 0.99;
				//add velocity by inflow
				float2 addImply = float2( sqrt(inflowL.z)*(inflowL.z) - sqrt(inflowR.x)*(inflowR.x),
										  sqrt(inflowB.w)*(inflowB.w) - sqrt(inflowT.y)*(inflowT.y));
				float2 newImply = addImply + velocity * waterHeight;
				velocity = newImply / (waterHeightAfterInflow + 0.0000001);
				
				
				//float2 addImply = float2( (inflowL.z) - (inflowR.x),
				//						  (inflowB.w) - (inflowT.y)) * 0.8;
				//velocity += addImply;
				velocity *= velocityDamp;

				//drop
				float capacity = height.z;
				float drop = capacity * 0.2;
				float newCapacity = capacity - drop;
				//float terrainHeightAfterDrop = terrainHeight + drop;
				////capacity
				//float terrainHeightAfterErosion = max(0, terrainHeightAfterDrop - length(velocity) * 0.1);
				//newCapacity += terrainHeightAfterDrop - terrainHeightAfterErosion;
				
				dest.height = float4(terrainHeight, waterHeightAfterOutflow, newCapacity, 0);
				dest.outflow = float4(outflowL, outflowB, outflowR, outflowT);
				dest.velocity = float4(velocity,dir);
				return dest;
			}
			


			ENDCG
		}
	}
}
