Shader "Custom/URP_EvolutionFog"
{
    Properties
    {
        [HDR] _BaseColor ("Fog Color", Color) = (0.5, 0.8, 0.5, 1) // 안개 색상 (HDR 지원)
        _NoiseScale ("Noise Scale (노이즈 크기)", Float) = 5.0
        _DistortAmount ("Distortion Strength (구기는 강도)", Range(0, 1)) = 0.2
        _Speed1 ("Evolution Speed (일렁임 속도)", Float) = 0.1
        _Speed2 ("Flow Speed (흐르는 속도)", Float) = -0.05
        _Edge1 ("Softness Min (Levels Low)", Range(0, 1)) = 0.2
        _Edge2 ("Softness Max (Levels High)", Range(0, 1)) = 0.7
    }

    SubShader
    {
        // 2D 투명 처리를 위한 URP 태그
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1; // 월드 좌표 추가
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _NoiseScale;
                float _DistortAmount;
                float _Speed1;
                float _Speed2;
                float _Edge1;
                float _Edge2;
            CBUFFER_END

            // 간단한 노이즈 함수 (AE의 프랙탈 노이즈 기초 수식)
            float hash(float2 p) {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 x) {
                float2 i = floor(x);
                float2 f = frac(x);
                float2 u = f * f * (3.0 - 2.0 * f); 
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), u.x),
                            lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), u.x), u.y);
            }

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // [Step 1] 변위 맵(Displacement Map) 생성 
                // 작은 노이즈가 천천히 흐르며 공간을 뒤틉니다.
                float2 distortUV = input.uv * (_NoiseScale * 2.0) + (_Time.y * _Speed1);
                float d = noise(distortUV) * _DistortAmount;

                // [Step 2] 메인 안개 생성 (Distortion 적용)
                // 에펙의 Turbulent Displace처럼 위에서 만든 'd' 값을 UV에 더해줍니다.
                float2 mainUV = input.uv * _NoiseScale + (_Time.y * _Speed2) + d;
                float fogNoise = noise(mainUV);

                // [Step 3] 레벨 조절 (Levels / Curves)
                // Smoothstep을 이용해 안개의 밀도를 깎아내어 부드러운 덩어리로 만듭니다.
                float alphaMap = smoothstep(_Edge1, _Edge2, fogNoise);

                // [Step 4] 최종 결과 (색상 + 알파)
                return half4(_BaseColor.rgb, alphaMap * _BaseColor.a);
            }
            ENDHLSL
        }
    }
}