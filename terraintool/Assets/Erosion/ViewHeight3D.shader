Shader "Unlit/ViewHeight3D"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Height("Height", 2D) = "black" {}
		_Size("Size", Vector) = (1,0.2,1,0)
		_Scale("Scale",float) = 1
		_WaterDensity("WaterDensity",float) = 5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
            #include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				fixed4 diff : COLOR0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform half4 _MainTex_TexelSize;
			sampler2D _Height;
			float _Scale;
			float4 _Size;
			float _WaterDensity;
			
			v2f vert (appdata v)
			{
				v.uv *= _Scale;
				float2 texelSize = _MainTex_TexelSize.xy * _Scale;
				float4 color = tex2Dlod(_Height, float4(v.uv, 0, 0));

				float y = color.r;
				float yRight = tex2Dlod(_Height, float4(v.uv + float2(1, 0) * texelSize, 0, 0)).r;
				float yDown = tex2Dlod(_Height, float4(v.uv + float2(0, -1) * texelSize, 0, 0)).r;

				float4 pos = (v.vertex + float4(0, y, 0, 0)) * _Size;
				float3 posRight = (v.vertex + float3(_MainTex_TexelSize.x, yRight, 0)) * _Size.xyz;
				float3 posDown = (v.vertex + float3(0 , yRight, -_MainTex_TexelSize.y)) * _Size.xyz;

				float3 normal = cross(posRight - pos.xyz,posDown - pos.xyz);
				normal = normalize(normal);

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, pos);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				fixed3 worldNormal = UnityObjectToWorldNormal(normal);
				half nl = max(0, dot(worldNormal, -_WorldSpaceLightPos0.xyz));
				o.diff = fixed4(nl * _LightColor0.rgb, color.g);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * float4(i.diff.xyz,1);
				col = lerp(col,fixed4(0,0,1,1),i.diff.a * _WaterDensity);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
