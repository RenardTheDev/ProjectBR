Shader "Unlit/Fire"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Distortion("Distortion", Range(0,1)) = 0.25

        _GradUVScale("Gradient noise uv scale", Float) = 5.0
        _GradOffset("Gradient noise offset", Vector) = (0, 0, 0, 0)

        _VoronUVScale("Voronoi uv scale", Float) = 5.0
        _VoronOffset("Voronoi offset", Vector) = (0, 0, 0, 0)

        _Disolve("Disolve", Float) = 1.2

        _ColorPower("Power", Float) = 1.2
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        //Cull Front
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
            float4 _MainTex_ST;
            float4 _Color;
            float _Distortion;

            float _GradUVScale;
            float2 _GradOffset;
            float _VoronUVScale;
            float2 _VoronOffset;

            float _Disolve;
            float _ColorPower;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float2 random2(float2 p)
            {
                return frac(sin(float2(dot(p, float2(117.12, 341.7)), dot(p, float2(269.5, 123.3)))) * 43458.5453);
            }

            fixed4 Voronoi(float2 uv)
            {
                fixed4 col = fixed4(0, 0, 0, 1);

                //uv *= 6.0; //Scaling amount (larger number more cells can be seen)
                float2 iuv = floor(uv); //gets integer values no floating point
                float2 fuv = frac(uv); // gets only the fractional part
                float minDist = 1.0;  // minimun distance
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        // Position of neighbour on the grid
                        float2 neighbour = float2(float(x), float(y));
                        // Random position from current + neighbour place in the grid
                        float2 pointv = random2(iuv + neighbour);
                        // Move the point with time
                        pointv = 0.5 + 0.5 * sin(_Time.z + 6.2236 * pointv);//each point moves in a certain way
                                                                        // Vector between the pixel and the point
                        float2 diff = neighbour + pointv - fuv;
                        // Distance to the point
                        float dist = length(diff);
                        // Keep the closer distance
                        minDist = min(minDist, dist);
                    }
                }
                // Draw the min distance (distance field)
                col.r += minDist * minDist; // squared it to to make edges look sharper

                return col;
            }

            float2 unity_gradientNoise_dir(float2 p)
            {
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float unity_gradientNoise(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(unity_gradientNoise_dir(ip), fp);
                float d01 = dot(unity_gradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(unity_gradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(unity_gradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //return Voronoi(i.uv).r * col.a;

                // gradient noise setup
                float2 tiling = i.uv * _GradUVScale;
                tiling.x = tiling.x - _Time.z * _GradOffset.x;
                tiling.y = tiling.y - _Time.z  * _GradOffset.y;

                float2 noise = unity_gradientNoise(tiling) + 0.5;
                float2 distortion = lerp(i.uv, noise, _Distortion);

                // voronoi setup
                tiling = i.uv * _VoronUVScale;
                tiling.x = tiling.x - _Time.z  * _VoronOffset.x;
                tiling.y = tiling.y - _Time.z  * _VoronOffset.y;

                fixed4 voronoi = Voronoi(tiling) * 2;

                // result
                float noiseMix = noise * pow(voronoi.r, _Disolve);
                fixed4 tex = tex2D(_MainTex, distortion);
                fixed4 col = tex * (_Color * _ColorPower) * noiseMix;

                col = clamp(col, 0, 1);

                return col;
                //return (noise * pow(voronoi.r, _Disolve)).r;
            }
            ENDCG
        }
    }
}
