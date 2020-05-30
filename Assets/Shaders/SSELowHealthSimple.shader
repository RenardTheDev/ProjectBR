Shader "SSE/LowHealth simple"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" { }
        _Mask("Mask", 2D) = "white" { }
        _Tint("Tint", Color) = (1.0, 1.0, 1.0, 1.0)

        _Amount("Amount", Range(0,1)) = 1.0
        _Power("Power", float) = 2.0
        _Clip("Clip", float) = 0.2
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
            sampler2D _Mask;
            fixed4 _Tint;
            float _Amount;
            float _Power;
            float _Clip;

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

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 pix = tex2D(_MainTex, i.uv);
                fixed4 col = pix;

                fixed4 mask = tex2D(_Mask, i.uv);

                //by texure mask 
                //float power = pow(mask.a * _Amount, _Power);

                //by uv position
                float2 motion = (0.5 - (normalize(0.5 - i.uv) * _Clip)) - i.uv;
                float power = pow(length(motion), _Power);

                if (_Amount * power <= 0.01)
                {
                    return col;
                }

                col = lerp(pix, lerp(pix, pix * _Tint, power), _Amount);

                return col;

            }
            ENDCG
        }
    }
}
