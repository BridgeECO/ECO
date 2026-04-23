Shader "Custom/DynamicVectorGrass"
{
    Properties
    {
        [Header(Color Settings)]
        _BaseColor ("Root Color", Color) = (0.1, 0.6, 0.3, 1)
        _TopColor ("Tip Color", Color) = (0.4, 1.0, 0.6, 1)
        
        [Header(Shape Settings)]
        _GrassCount ("Grass Count", Int) = 7
        _Thickness ("Line Thickness", Range(0.01, 0.5)) = 0.1
        _BaseCurvature ("S-Curve Intensity", Range(-1, 1)) = 0.3
        _RandomSeed ("Random Seed", Float) = 12.34
        
        [Header(Animation Settings)]
        _WindSpeed ("Wind Speed", Range(0, 5)) = 1.5
        _WindStrength ("Wind Strength", Range(0, 0.5)) = 0.1
        
        [Header(Interaction)]
        _CharacterX ("Character X Position", Range(-10, 10)) = 0.5
        _PushStrength ("Push Strength", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "GrassPass"
            HLSLPROGRAM
            // 1. 프래그마 선언은 반드시 줄바꿈을 해야 합니다.
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
            };

            half4 _BaseColor, _TopColor;
            int _GrassCount;
            float _Thickness, _BaseCurvature, _RandomSeed;
            float _WindSpeed, _WindStrength;
            float _CharacterX, _PushStrength;

            float rand(float n) { return frac(sin(n) * 43758.5453123); }

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float sdCapsule(float2 p, float2 a, float2 b, float r)
            {
                float2 pa = p - a, ba = b - a;
                float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
                return length(pa - ba * h) - r;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float3 combinedColor = float3(0, 0, 0);
                float combinedAlpha = 0;

                for (int i = 0; i < _GrassCount; i++)
                {
                    float id = float(i) + _RandomSeed;
                    float xAnchor = rand(id) * 0.8 + 0.1;
                    float height = rand(id + 1.2) * 0.5 + 0.4;
                    float individualSpeed = _WindSpeed * (0.8 + rand(id + 2.3) * 0.4);
                    
                    float y = input.uv.y;
                    float curveEffect = pow(y, 2.0);
                    float sCurve = sin(y * 4.0 + _Time.y * individualSpeed) * _BaseCurvature * _WindStrength;
                    
                    float distToChar = input.uv.x - _CharacterX;
                    float push = exp(-abs(distToChar) * 10.0) * sign(distToChar) * _PushStrength;
                    float deformedX = xAnchor + (sCurve + push) * curveEffect;

                    float2 p = input.uv;
                    // 2. 시작점을 0.0에서 -0.1로 내려서 바닥 잘림 방지
                    float d = sdCapsule(p, float2(deformedX, -0.1), float2(deformedX, height), _Thickness);
                    
                    // 3. 선이 더 잘 보이도록 외곽선 경계를 약간 부드럽게 보정 (0.01 -> 0.02)
                    float mask = smoothstep(0.02, 0.0, d);

                    float vProgress = clamp(y / height, 0.0, 1.0);
                    float3 color = lerp(_BaseColor.rgb, _TopColor.rgb, vProgress);
                    
                    // 4. 알파값이 너무 빨리 빠지지 않게 보정
                    float alpha = mask * clamp(1.2 - vProgress, 0, 1);

                    combinedColor = lerp(combinedColor, color, alpha);
                    combinedAlpha = max(combinedAlpha, alpha);
                }
                return half4(combinedColor, combinedAlpha);
            }
            ENDHLSL
        }
    }
}