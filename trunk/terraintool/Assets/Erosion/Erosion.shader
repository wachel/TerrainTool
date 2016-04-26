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
				
				//计算水面高度差
				float4 diffHeight = totalHeight.xxxx - totalHeightN;

				//水深的地方流速快，衰减低
				float x = 1 - (1 / (waterHeight * 50 + 1));//水越深x越趋近于1
				float flowdamp = lerp(0.8, 1, x);
				float flowSpeed = lerp(0.05, 0.15, x);

				outflow *= flowdamp;
				outflow = max(outflow + diffHeight * flowSpeed, (0.00001).xxxx);

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

			float4 frag (v2f i) :COLOR
			{
				//current
			    float4 height = tex2D(_MainTex, i.uv);
				float4 outflow = tex2D(_Outflow, i.uv);
				float terrainHeight = height.x;
				float waterHeight = height.y;
				float totalHeight = terrainHeight + waterHeight;

				//inflow
				float4 inflowL = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(-1,0));
				float4 inflowR = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2( 1,0));
				float4 inflowB = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(0,-1));
				float4 inflowT = tex2D(_Outflow,i.uv + _Outflow_TexelSize.xy * half2(0, 1));
				float4 inflow = float4(inflowL.y, inflowR.x, inflowB.w, inflowT.z);

				//
				float4 diffFlow = inflow - outflow;

				waterHeight += dot(diffFlow,(1).xxxx);

				float x = 1 - (1 / (waterHeight * 10 + 1));//水越深x越趋近于1

				waterHeight -= _EvaporateSpeed * x;	//蒸发
				waterHeight += _RainSpeed;			//下雨
				waterHeight = max(waterHeight, 0);

				//velocity
				//float speed = length(diffFlow.x - diffFlow.y, diffFlow.z - diffFlow.w);

			
				return float4(terrainHeight, waterHeight, 0, 0);
			}
			ENDCG
		}
	}
}
