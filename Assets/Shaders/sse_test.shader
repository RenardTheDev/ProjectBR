Shader "SSE/test"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Mapper("Map", 2D) = "white" {}
        _PickLength ("Length", float) = 0.0
        _PickSamples ("Samples", float) = 6.0
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
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _Mapper;
            float _PickLength;
            float _PickSamples;
            float2 pick = 0.0;

            fixed4 getBlurredPixel(sampler2D _sampler, float2 uv)
            {
                fixed4 total = tex2D(_sampler, uv);
                float PI = 3.14159265;

                for (float s = 0; s < _PickSamples; s++)
                {
                    for (float l = 0.1; l < _PickLength; l += _PickLength * 0.1)
                    {
                        pick.x = cos((360 / _PickSamples) * s * (PI / 180)) * _PickLength * 0.01 * l;
                        pick.y = sin((360 / _PickSamples) * s * (PI / 180)) * _PickLength * 0.01 * l;

                        total += tex2D(_sampler, uv + pick.xy);
                    }
                }

                total /= (_PickSamples * 9 + 1);

                return total;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 map = tex2D(_Mapper, i.uv);
                
                fixed4 pix = getBlurredPixel(_MainTex, i.uv);

                // just invert the colors
                //col.rgb = 1 - col.rgb;
                return lerp(col, pix, map.r);
            }
            ENDCG
        }
    }
}
