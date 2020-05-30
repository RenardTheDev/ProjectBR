Shader "SSE/LowHealth"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _Tint("Tint", Color) = (1.0, 1.0, 1.0, 1.0)

        _MaxLength("Max Length", float) = 1.0
        _PickSamples("Samples", Range(1,128)) = 8.0

        _Clip("Clip", Range(0,1)) = 0.2
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

            sampler2D _MainTex;


            //---BLUR---
            float _MaxLength;
            float _PickSamples;

            float _PickLength = 0.1;

            fixed4 _Tint;

            float _Clip; 
            float picks;

            float2 pick = 0.0;
            float power;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float remap(float value, float low1, float high1, float low2, float high2)
            {
                float res;
                res = low2 + (value - low1) * (high2 - low2) / (high1 - low1);
                return res;
            }

            fixed4 getDirBlur(sampler2D _sampler, float2 uv)
            {
                float2 motion = (0.5 - (normalize(0.5 - uv) * _Clip)) - uv;
                if (length(motion) < _Clip) {
                    motion = 0;
                }
                else {
                    power = remap(length(motion), _Clip, 1.0, 0.0, 1.0);
                    motion = normalize(motion) * power;
                }

                fixed4 total = tex2D(_sampler, uv);
                picks = 1;

                _PickLength = _MaxLength / _PickSamples;

                for (float i = _PickLength; i < _MaxLength; i += _PickLength)
                {
                    total += tex2D(_sampler, uv + motion * _MaxLength * i);
                    picks++;
                }

                total = total / picks;

                return total;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 pix = getDirBlur(_MainTex, i.uv); 
                fixed4 col = lerp(pix, pix * _Tint, pow(remap(power, 0.0, 0.5, 0.0, 1.0), 2));
                return col;
            }
            ENDCG
        }
    }
}
