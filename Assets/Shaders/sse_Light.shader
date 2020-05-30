Shader "SSE/Light"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_LightColor("Light color", Color) = (1, 1, 1, 1)
		_LightDir("Light direction", Vector) = (0, -1, 0, 0)
		_BackLightPower1("_BackLightPower1", Float) = 0.5
		_BackLightPower2("_BackLightPower2", Float) = 0.5
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
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;
				sampler2D _CameraDepthTexture;
				sampler2D _CameraGBufferTexture2;

				float3 _LightDir;
				float4 _LightColor;

				float _BackLightPower1;
				float _BackLightPower2;

				fixed4 frag(v2f i) : SV_Target
				{
					fixed3 lDir = normalize(_LightDir);
					fixed4 color = tex2D(_MainTex, i.uv);
					fixed4 depth = tex2D(_CameraDepthTexture, i.uv);
					fixed3 normals = UnpackNormal(tex2D(_CameraGBufferTexture2, i.uv));

					fixed4 res;

					float NdotL = dot(normals, lDir);
					float diff = NdotL * _BackLightPower1 + _BackLightPower2;
					res = color * _LightColor * diff;

					if (depth.r > 0) {
						return res;
					}
					else {
						return color;
					}
				}
				ENDCG
			}
		}
}