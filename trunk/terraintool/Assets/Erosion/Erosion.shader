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
			    float4 height = tex2D(_MainTex, i.uv);
				float4 flow = tex2D(_Outflow, i.uv);//xy记录当前格子右方和上方两个管道的水流量，zw记录右方和上方两个管道的泥流量
				float4 flowL = tex2D(_Outflow, i.uv + _Outflow_TexelSize.xy * half2(-1, 0));//与左边格子相连管道的流量
				float4 flowB = tex2D(_Outflow, i.uv + _Outflow_TexelSize.xy * half2(0, -1));//与下面格子相连管道的流量
				float4 heightR = tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(1, 0));
				float4 heightT = tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(0, 1));
				float4 heightL = tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(-1, 0));
				float4 heightB = tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(0, -1));

				float diffTerrainR = heightR.x - height.x;//与右边地形高度差
				float diffTotalR = (heightR.x + heightR.y + heightR.z) - (height.x + height.y + height.z);//与右边水面高度差
				float diffTerrainT = heightT.x - height.x;//与上边地形高度差
				float diffTotalT = (heightT.x + heightT.y + heightT.z) - (height.x + height.y + height.z);//与上边水面高度差


				//流量改变,流入为+，流出为-
				flow.xy *= 0.95;//damp
				flow.xy += float2(diffTotalR, diffTotalT) * 0.1;				
				flow.xy = -min(-flow.xy, (height.yy + height.zz) * 0.25);//流出
				flow.xy = min(flow.xy, float2(heightR.y + heightR.z, heightT.y + heightT.z) * 0.25);//流入

				float2 randPoint = hash(i.uv + height.xy * 123);
				randPoint = normalize(randPoint) * pow(length(randPoint),2);
				float4 randHeight = tex2D(_MainTex, i.uv +_MainTex_TexelSize.xy * 2 * randPoint);

				flow.zw *= 0.92;//damp
				float2 waterHeight = (height.yy + height.zz + float2(heightR.y + heightR.z,heightT.y + heightT.z)) * 0.5;
				flow.zw += float2(diffTerrainR, diffTerrainT) * pow(randHeight.y + randHeight.z,0.2) * 0.055;

				//float2 randPoint = hash(i.uv + height.xy*123);
				//randPoint = normalize(randPoint) * pow(length(randPoint),2);
				//float4 randHeight = tex2D(_MainTex, i.uv);// +_MainTex_TexelSize.xy * 2 * randPoint);

				flow.zw += (flow.xy) * 0.15;// *((1).xx + randPoint * 0.000000000000001);

				return flow;
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
			uniform float _EvaporateSpeed;
			uniform float _RainSpeed;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _Outflow_TexelSize;

			float4 frag (v2f i) :COLOR
			{
				//current
			    float4 height = tex2D(_MainTex, i.uv);//x:地 y:水 z:悬浮泥沙
				float4 flow = tex2D(_Outflow, i.uv);

				float4 heightL = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(-1,0));
				float4 heightR = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2( 1,0));
				float4 heightB = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0,-1));
				float4 heightT = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0, 1));

				float4 flowL = tex2D(_Outflow, i.uv + _Outflow_TexelSize.xy * half2(-1, 0));//与左边格子相连管道的流量
				float4 flowB = tex2D(_Outflow, i.uv + _Outflow_TexelSize.xy * half2(0, -1));//与下面格子相连管道的流量

				float4 waterAndSolidN = float4(heightL.x + heightL.y, heightR.x + heightR.y, heightB.x + heightB.y, heightT.x + heightT.y);//四周格子的泥加水量
				float4 solidProportionN = float4(	heightL.z / (heightL.y + heightL.z + 1e-20), 
													heightR.z / (heightR.y + heightR.z + 1e-20),
													heightB.z / (heightB.y + heightB.z + 1e-20),
													heightT.z / (heightT.y + heightT.z + 1e-20));//四周格子泥浆占比
				float4 waterProportionN = float4(	heightL.y / (heightL.y + heightL.z + 1e-20), 
													heightR.y / (heightR.y + heightR.z + 1e-20),
													heightB.y / (heightB.y + heightB.z + 1e-20),
													heightT.y / (heightT.y + heightT.z + 1e-20));//四周格子泥浆占比
				
				float4 waterFlow = float4(flow.xy, -flowL.x, -flowB.y);//当前格子同周围格子的流量关系，流入+，流出-
				float newWaterHeight = height.y + dot(waterFlow * waterProportionN, (1).xxxx);
				newWaterHeight = max(1e-20, newWaterHeight);

				float x = 1 - (1 / (newWaterHeight * 10 + 1));//水越深x越趋近于1
				newWaterHeight -= _EvaporateSpeed * x * 3;	//蒸发
				newWaterHeight += _RainSpeed;			//下雨
				newWaterHeight = max(newWaterHeight, 0);
				
				float suspendedSolid = height.z + dot(waterFlow * solidProportionN, (1).xxxx);//当前泥量+流入的泥的量
				suspendedSolid = max(1e-20, suspendedSolid);
				
				float2 flux = float2(waterFlow.xy - waterFlow.zw) * 0.5;//通量,用来计算速度
				float4 forwardHeight = tex2D(_MainTex, i.uv + normalize(flux) * 0.5 * _MainTex_TexelSize.xy);
				float4 backwardHeight = tex2D(_MainTex, i.uv - normalize(flux) * 1 * _MainTex_TexelSize.xy);
				//float abrupt = (forwardHeight.x - height.x);
				float abrupt = (height.x - backwardHeight.x);

				float newCapacity = length(flux) * 0.25;//*(1 - abrupt * 50);
				//newCapacity += height.y * 0.2f;
				newCapacity = clamp(newCapacity, 0, height.y * 0.5);
				
				float capacityDiff = (newCapacity - suspendedSolid);

				float newSuspendSolid = suspendedSolid + capacityDiff;
				float newTerrainHeight = height.x - capacityDiff;


				float4 solidFlow = float4(flow.z, flow.w, -flowL.z, -flowB.w);
				newTerrainHeight = newTerrainHeight + dot(solidFlow, (1).xxxx);

				return float4(newTerrainHeight, newWaterHeight, newCapacity, 0);
				
				
				//float4 waterN = float4(heightL.y, heightR.y, heightB.y, heightT.y);
				//float4 capacityN = float4(heightL.z, heightR.z, heightB.z, heightT.z);
				//float suspendedSolid = height.z + dot(waterFlow * capacityN / (waterN + (0.000000001).xxxx), (1).xxxx);
				//suspendedSolid = max(0.000000001, suspendedSolid);
				//float4 forwardHeight = tex2D(_MainTex, i.uv + normalize(flux) * 0.5 * _MainTex_TexelSize.xy);
				//float abrupt = (height.x - forwardHeight.x) * (1 + height.w * 5);
				//float newCapacity = (abrupt + 0.0005) * pow(length(flux),1.0) * 200;
				//newCapacity = clamp(newCapacity,0,height.y * 0.5);

				//float waterHeight = height.y + dot(waterFlow, (1).xxxx);
				//float x = 1 - (1 / (waterHeight * 10 + 1));//水越深x越趋近于1
				//waterHeight -= _EvaporateSpeed * x * 3;	//蒸发
				//waterHeight += _RainSpeed;			//下雨
				//waterHeight = max(waterHeight, 0);

				////修改地形高度
				//float newTerrainHeight = height.x + (suspendedSolid - newCapacity);
				//newTerrainHeight = max(0, newTerrainHeight);
				//float suspendedSolidChange = height.x - newTerrainHeight;
				//float newSuspendedSolid = suspendedSolid + suspendedSolidChange;
				//waterHeight += suspendedSolidChange;

				//return float4(newTerrainHeight, waterHeight, newSuspendedSolid, height.w);

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
