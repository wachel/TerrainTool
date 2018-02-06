// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Erosion"
{
	Properties
	{
		_MainTex ("_Main", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		//update outflow
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
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _Outflow;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _Outflow_TexelSize;

			float2 hash(float2 p)
			{
				p = float2(dot(p, float2(127.1, 311.7)),
					dot(p, float2(269.5, 183.3)));

				return -1.0 + 2.0*frac(sin(p)*(43758.5453123));
			}

			float4 frag (v2f i):COLOR
			{
				//current
				float4 outflow = tex2D(_Outflow, i.uv);//outflow        
				float4 height = tex2D(_MainTex, i.uv);//main haight
				float totalHeight = height.x + height.y;
				float waterHeight = height.y;

				//neighbour
				float4 heightL = tex2D(_MainTex,i.uv + (float2(-1.0,0.0)) * _MainTex_TexelSize.xy);
				float4 heightR = tex2D(_MainTex,i.uv + (float2( 1.0,0.0)) * _MainTex_TexelSize.xy);
				float4 heightB = tex2D(_MainTex,i.uv + (float2(0.0,-1.0)) * _MainTex_TexelSize.xy);
				float4 heightT = tex2D(_MainTex,i.uv + (float2(0.0, 1.0)) * _MainTex_TexelSize.xy);
				float4 totalHeightN = float4(
					heightL.x + heightL.y, 
					heightR.x + heightR.y, 
					heightB.x + heightB.y, 
					heightT.x + heightT.y
				);

				//outflowN
				//float4 outflowL = tex2D(_Outflow,i.uv + float2(-1.0,0.0) * _MainTex_TexelSize.xy);
				//float4 outflowR = tex2D(_Outflow,i.uv + float2( 1.0,0.0) * _MainTex_TexelSize.xy);
				//float4 outflowB = tex2D(_Outflow,i.uv + float2(0.0,-1.0) * _MainTex_TexelSize.xy);
				//float4 outflowT = tex2D(_Outflow,i.uv + float2(0.0, 1.0) * _MainTex_TexelSize.xy);

				//
				float4 diffHeight = (totalHeight.xxxx - totalHeightN);

				//damp
				float flowdamp = 0.95;

				float flowSpeed = 0.2;

				float4 distance = sqrt(diffHeight * diffHeight / _MainTex_TexelSize.x + 1.0 * 1.0);


				//
				outflow *= flowdamp;
				outflow += diffHeight * flowSpeed / distance;
				outflow = max(outflow, (0.00000000000001).xxxx);

				//
				float outflowScale = waterHeight / (outflow.x + outflow.y + outflow.z + outflow.w);
				outflowScale = min(1.0, outflowScale);
				outflow *= outflowScale;

				return outflow;
			}
			ENDCG
		}


		//update waterheight
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
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _Outflow;
			sampler2D _SolidOutflow;
			uniform float _EvaporateSpeed;
			uniform float _RainSpeed;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _Outflow_TexelSize;

			float4 frag (v2f i) :COLOR
			{
				//current
				float4 outflow = tex2D(_Outflow, i.uv);//outflow
				float4 solidOutflow = tex2D(_SolidOutflow, i.uv);
				float4 height = tex2D(_MainTex, i.uv);//main
				float terrainHeight = height.x;
				float waterHeight = height.y;
				float totalHeight = terrainHeight + waterHeight;

				//neighbour
				float4 heightL = tex2D(_MainTex, i.uv + float2(-1.0,0.0) * _MainTex_TexelSize.xy);
				float4 heightR = tex2D(_MainTex, i.uv + float2(1.0,0.0)  * _MainTex_TexelSize.xy);
				float4 heightB = tex2D(_MainTex, i.uv + float2(0.0,-1.0) * _MainTex_TexelSize.xy);
				float4 heightT = tex2D(_MainTex, i.uv + float2(0.0, 1.0) * _MainTex_TexelSize.xy);
				float4 terrainN = float4(heightL.x, heightR.x, heightB.x, heightT.x);
				float4 waterN = float4(heightL.y, heightR.y, heightB.y, heightT.y);
				float4 capacityN = float4(heightL.z, heightR.z, heightB.z, heightT.z);

				//inflow
				float4 outflowL = tex2D(_Outflow, i.uv + float2(-1.0,0.0) * _MainTex_TexelSize.xy);
				float4 outflowR = tex2D(_Outflow, i.uv + float2(1.0,0.0)  * _MainTex_TexelSize.xy);
				float4 outflowB = tex2D(_Outflow, i.uv + float2(0.0,-1.0) * _MainTex_TexelSize.xy);
				float4 outflowT = tex2D(_Outflow, i.uv + float2(0.0, 1.0) * _MainTex_TexelSize.xy);
				float4 inflow = float4(outflowL.y, outflowR.x, outflowB.w, outflowT.z);

				//solid inflow
				float4 solidOutflowL = tex2D(_SolidOutflow, i.uv + float2(-1.0,0.0) * _MainTex_TexelSize.xy);
				float4 solidOutflowR = tex2D(_SolidOutflow, i.uv + float2(1.0,0.0)  * _MainTex_TexelSize.xy);
				float4 solidOutflowB = tex2D(_SolidOutflow, i.uv + float2(0.0,-1.0) * _MainTex_TexelSize.xy);
				float4 solidOutflowT = tex2D(_SolidOutflow, i.uv + float2(0.0, 1.0) * _MainTex_TexelSize.xy);
				float4 solidInflow = float4(solidOutflowL.y, solidOutflowL.x, solidOutflowL.w, solidOutflowL.z);

				////suspended
				float2 fluxOutflow = float2(outflow.y - outflow.x, outflow.w - outflow.z);
				float2 fluxInflow = float2(inflow.x - inflow.y, inflow.z - inflow.w);
				float2 flux = (fluxInflow + fluxOutflow) * 0.5;//
				float2 velocity = flux / (height.y + 0.000000001);

				float suspendedSolid = height.z + dot(inflow * capacityN / (waterN + (0.000000001).xxxx), (1).xxxx);
				suspendedSolid -= dot(outflow,(1).xxxx) * height.z / (height.y + 0.000000001);
				suspendedSolid = max(0.000000001, suspendedSolid);

				//float4 srcHeight = tex2D(_MainTex, i.uv - flux * _MainTex_TexelSize.xy * 1000);
				//float4 srcHeight = tex2D(_MainTex, i.uv - velocity * _MainTex_TexelSize.xy * 1);
				//float suspendedSolid = height.z;

				float4 forwardHeight = tex2D(_MainTex, i.uv + normalize(flux) * 0.5 * _MainTex_TexelSize.xy);
				float abrupt = (height.x - forwardHeight.x) * (1.0 + height.w * 5.0);
				float newCapacity = (abrupt + 0.0005) * pow(length(flux),1.0) * 200.0;
				newCapacity = clamp(newCapacity,0.0,height.y * 0.5);

				//update water
				float4 diffFlow = inflow - outflow;
				float4 solidDiffFlow = solidInflow - solidOutflow;

				waterHeight += dot(diffFlow,(1.0).xxxx);
				
				//evaporate and rain
				float x = 1.0 - (1.0 / (waterHeight * 10.0 + 1.0));//
				waterHeight -= _EvaporateSpeed * x * 3.0;	//evaporate
				waterHeight += _RainSpeed;			//rain
				waterHeight = max(waterHeight, 0.0);

				//update terrain height
				float newTerrainHeight = height.x + (suspendedSolid - newCapacity);
				//newTerrainHeight = lerp(newTerrainHeight,dot(terrainN,(1).xxxx) * 0.25,0.01);

				//newTerrainHeight += dot(solidDiffFlow, (1).xxxx);
				newTerrainHeight = max(0.0, newTerrainHeight);
				float suspendedSolidChange = height.x - newTerrainHeight;
				float newSuspendedSolid = suspendedSolid + suspendedSolidChange;
				waterHeight += suspendedSolidChange;

				return float4(newTerrainHeight, waterHeight, newSuspendedSolid, lerp(height.w,height.x,0.01));

				//
				//float terrainHeight = height.x;
				//float waterHeight = height.y;
				//float totalHeight = terrainHeight + waterHeight;
				//
				////neighbour
				//float4 heightL = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(-1,0));
				//float4 heightR = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2( 1,0));
				//float4 heightB = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0,-1));
				//float4 heightT = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0, 1));
				//float4 terrainN = float4(heightL.x, heightR.x, heightB.x, heightT.x);
				//float4 waterN = float4(heightL.y, heightR.y, heightB.y, heightT.y);
				//float4 capacityN = float4(heightL.z, heightR.z, heightB.z, heightT.z);
				//
				////inflow
				//float4 outflowL = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(-1,0));
				//float4 outflowR = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2( 1,0));
				//float4 outflowB = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(0,-1));
				//float4 outflowT = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(0, 1));
				//float4 inflow = float4(outflowL.y, outflowR.x, outflowB.w, outflowT.z);
				//
				//////悬浮物携带能力
				//float2 fluxOutflow = float2(outflow.y - outflow.x, outflow.w - outflow.z);
				//float2 fluxInflow = float2(inflow.x - inflow.y, inflow.z - inflow.w);
				//float2 flux = (fluxInflow + fluxOutflow) * 0.5;//通量
				//float2 velocity = flux / (height.y + 0.000000001);
				//
				//float suspendedSolid = height.z + dot(inflow * capacityN / (waterN + (0.000000001).xxxx), (1).xxxx);
				//suspendedSolid -= dot(outflow,(1).xxxx) * height.z / (height.y + 0.000000001);
				//suspendedSolid = max(0.000000001, suspendedSolid);
				//
				////float4 srcHeight = tex2D(_MainTex, i.uv - flux * _MainTex_TexelSize.xy * 1000);
				////float4 srcHeight = tex2D(_MainTex, i.uv - velocity * _MainTex_TexelSize.xy * 1);
				////float suspendedSolid = height.z;
				//
				//float4 forwardHeight = tex2D(_MainTex, i.uv + normalize(flux) * 0.5 * _MainTex_TexelSize.xy);
				//float abrupt = (height.x - forwardHeight.x) * (1 + height.w * 5);
				//float newCapacity = (abrupt + 0.0005) * pow(length(flux),1.0) * 200;
				//newCapacity = clamp(newCapacity,0,height.y * 0.5);
				//
				////水面更新
				//float4 diffFlow = inflow - outflow;
				//waterHeight += dot(diffFlow,(1).xxxx);
				//
				////蒸发下雨
				//float x = 1 - (1 / (waterHeight * 10 + 1));//水越深x越趋近于1
				//waterHeight -= _EvaporateSpeed * x * 3;	//蒸发
				//waterHeight += _RainSpeed;			//下雨
				//waterHeight = max(waterHeight, 0);
				//
				////修改地形高度
				//float newTerrainHeight = height.x + (suspendedSolid - newCapacity);
				////newTerrainHeight = lerp(newTerrainHeight,dot(terrainN,(1).xxxx) * 0.25,0.01);
				//newTerrainHeight = max(0, newTerrainHeight);
				//float suspendedSolidChange = height.x - newTerrainHeight;
				//float newSuspendedSolid = suspendedSolid + suspendedSolidChange;
				//waterHeight += suspendedSolidChange;
				//
				//return float4(newTerrainHeight, waterHeight, newSuspendedSolid, height.w);
			}
			ENDCG
		}
	}
}
