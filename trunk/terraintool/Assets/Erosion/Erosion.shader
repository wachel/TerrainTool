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
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _Outflow;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _Outflow_TexelSize;

			float4 frag (v2f i):COLOR
			{
				//current
			    float4 height = tex2D(_MainTex, i.uv);
				float4 outflow = tex2D(_Outflow, i.uv);
				float totalHeight = height.x + height.y;
				float waterHeight = height.y;

				//neighbour
				float4 heightL = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(-1,0));
				float4 heightR = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2( 1,0));
				float4 heightB = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0,-1));
				float4 heightT = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0, 1));
				float4 totalHeightN = float4(heightL.x + heightL.y, heightR.x + heightR.y, heightB.x + heightB.y, heightT.x + heightT.y);

				//outflowN
				float4 outflowL = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(-1,0));
				float4 outflowR = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2( 1,0));
				float4 outflowB = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(0,-1));
				float4 outflowT = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(0, 1));

				//计算水面高度差
				float4 diffHeight = totalHeight.xxxx - totalHeightN;

				//水深的地方流速快，衰减低
				float x = 1 - (1 / (waterHeight * 100 + 1));//水越深x越趋近于1
				float flowdamp = lerp(0.85, 1, x);

				float x2 = 1 - (1 / (waterHeight * 30 + 1));//水越深x越趋近于1
				float flowSpeed = lerp(0.05, 0.2, x2);

				//流速
				outflow *= flowdamp;
				outflow += diffHeight * flowSpeed;
				outflow = max(outflow, (0.0000001).xxxx);

				//防止负数
				float outflowScale = waterHeight / (outflow.x + outflow.y + outflow.z + outflow.w);
				outflowScale = min(1, outflowScale);
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
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _Outflow;
			uniform float _EvaporateSpeed;
			uniform float _RainSpeed;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _Outflow_TexelSize;

			//float3 getNormal(float4 terrainN)
			//{
			//	float3 left		= float3(-1,  0, terrainN.x);
			//	float3 right	= float3( 1,  0, terrainN.y);
			//	float3 bottom	= float3( 0, -1, terrainN.z);
			//	float3 top		= float3( 0,  1, terrainN.w);
			//	float3 normal0 = normalize(cross(top - bottom, left - top));
			//	float3 normal1 = normalize(cross(bottom - top,right - bottom));
			//	return (normal0 + normal1) * 0.5;
			//}

			float4 frag (v2f i) :COLOR
			{
				//current
			    float4 height = tex2D(_MainTex, i.uv);
				float4 outflow = tex2D(_Outflow, i.uv);
				float terrainHeight = height.x;
				float waterHeight = height.y;
				float totalHeight = terrainHeight + waterHeight;

				//neighbour
				float4 heightL = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(-1,0));
				float4 heightR = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2( 1,0));
				float4 heightB = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0,-1));
				float4 heightT = tex2D(_MainTex,i.uv + _MainTex_TexelSize.xy * half2(0, 1));
				float4 terrainN = float4(heightL.x, heightR.x, heightB.x, heightT.x);
				float4 waterN = float4(heightL.y, heightR.y, heightB.y, heightT.y);
				float4 capacityN = float4(heightL.z, heightR.z, heightB.z, heightT.z);
				
				//inflow
				float4 outflowL = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(-1,0));
				float4 outflowR = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2( 1,0));
				float4 outflowB = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(0,-1));
				float4 outflowT = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(0, 1));
				float4 inflow = float4(outflowL.y, outflowR.x, outflowB.w, outflowT.z);

				////悬浮物数量转移
				//float4 suspendedSolidIn = inflow  * capacityN / (waterN + 0.0000001);
				//float4 suspendedSolidOut = outflow * height.z / (height.y + 0.0000001);
				//float suspendedSolidChange = dot(suspendedSolidIn, (1).xxxx) - dot(suspendedSolidOut, (1).xxxx);
				//float suspendedSolid = height.z + suspendedSolidChange;//当前悬浮物数量

				////悬浮物携带能力
				float2 fluxOutflow = float2(outflow.y - outflow.x, outflow.w - outflow.z);
				float2 fluxInflow = float2(inflow.x - inflow.y, inflow.z - inflow.w);
				float2 flux = (fluxInflow) ;//通量
				float2 velocity = flux / (height.y + 0.00001);
				//float newCapacity = length(velocity) * 0.8;

				float4 srcHeight = tex2D(_MainTex, i.uv - velocity * _MainTex_TexelSize.xy * 5);
				float suspendedSolid = srcHeight.z;

				float4 forwardHeight = tex2D(_MainTex, i.uv + normalize(flux) * 1 * _MainTex_TexelSize.xy);
				float abrupt = height.x - forwardHeight.x;
				float newCapacity = (abrupt * length(flux)) * 1;
				newCapacity = max(0, newCapacity);

				//水面更新
				float4 diffFlow = inflow - outflow;
				waterHeight += dot(diffFlow,(1).xxxx);

				//蒸发下雨
				float x = 1 - (1 / (waterHeight * 10 + 1));//水越深x越趋近于1
				waterHeight -= _EvaporateSpeed * x * 3;	//蒸发
				waterHeight += _RainSpeed;			//下雨
				waterHeight = max(waterHeight, 0);

				//修改地形高度
				float newTerrainHeight = height.x + suspendedSolid - newCapacity;
				newTerrainHeight = max(0, newTerrainHeight);
				float newSuspendedSolid = suspendedSolid + height.x - newTerrainHeight;
			
				return float4(newTerrainHeight, waterHeight, 0, abrupt);
			}
			ENDCG
		}


		////transform
		//Pass
		//{
		//	CGPROGRAM
		//	#pragma vertex vert
		//	#pragma fragment frag
		//
		//	#include "UnityCG.cginc"
		//
		//	struct appdata
		//	{
		//		float4 vertex : POSITION;
		//		float2 uv : TEXCOORD0;
		//	};
		//
		//	struct v2f
		//	{
		//		float2 uv : TEXCOORD0;
		//		float4 vertex : SV_POSITION;
		//	};
		//
		//	v2f vert (appdata v)
		//	{
		//		v2f o;
		//		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		//		o.uv = v.uv;
		//		return o;
		//	}
		//	
		//	sampler2D _MainTex;
		//	sampler2D _Outflow;
		//	uniform half4 _MainTex_TexelSize;
		//	uniform half4 _Outflow_TexelSize;
		//
		//
		//	float4 frag (v2f i) :COLOR
		//	{
		//		//current
		//	    float4 height = tex2D(_MainTex, i.uv);
		//		float4 outflow = tex2D(_Outflow, i.uv);
		//		
		//		float4 outflowL = tex2D(_Outflow, i.uv + _Outflow_TexelSize.xy * half2(-1, 0));
		//		float4 outflowR = tex2D(_Outflow, i.uv + _Outflow_TexelSize.xy * half2(1, 0));
		//		float4 outflowB = tex2D(_Outflow, i.uv + _Outflow_TexelSize.xy * half2(0, -1));
		//		float4 outflowT = tex2D(_Outflow, i.uv + _Outflow_TexelSize.xy * half2(0, 1));
		//		float4 inflow = float4(outflowL.y, outflowR.x, outflowB.w, outflowT.z);
		//
		//		float4 heightL = tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(-1, 0));
		//		float4 heightR = tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(1, 0));
		//		float4 heightB = tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(0, -1));
		//		float4 heightT = tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(0, 1));
		//		float4 waterN = float4(heightL.y, heightR.y, heightB.y, heightT.y);
		//		float4 capacityN = float4(heightL.z, heightR.z, heightB.z, heightT.z);
		//
		//		//悬浮物转移
		//		//float2 velocity = float2(outflow.y - outflow.x, outflow.w - outflow.z);//流速
		//		//float4 srcHeight = tex2D(_MainTex, i.uv - velocity * _MainTex_TexelSize.xy * 0.5);
		//		//return float4(height.x, height.y, srcHeight.z, height.w);
		//		//return float4((outflow * height.z / height.y));
		//
		//
		//		float4 inCapacity =  inflow  * capacityN / (waterN + 0.00001);
		//		float4 outCapacity = outflow * height.z / (height.y + 0.00001);
		//		float diff = dot(inCapacity, (1).xxxx) - dot(outCapacity, (1).xxxx);
		//		float newCapacity = height.z + diff;
		//		newCapacity = max(0, newCapacity);
		//		return float4(height.x, height.y, height.z, height.w);
		//
		//	}
		//	ENDCG
		//}
	}
}
