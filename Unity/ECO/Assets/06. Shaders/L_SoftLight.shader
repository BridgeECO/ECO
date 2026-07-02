Shader "FX/CustomBlend_Hardware_URP"
{
    Properties
    {
        [Header(Basic Settings)]
        [HDR] _Color ("Main Color", Color) = (1, 1, 1, 1)
        _Radius ("Radius (원 크기)", Range(0.0, 0.5)) = 0.45
        _Feather ("Feather (페더값)", Range(0.001, 0.5)) = 0.05
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
            "IgnoreProjector"="True" 
        }

        // 🚨 고정 블렌드 모드 사용: 인스펙터에서 바꾸고 싶다면 이 줄을 
        // 셰이더 변형(Shader Variant)으로 처리해야 하나, 
        // 가장 가볍고 확실한 DstColor + One (소프트 라이트 느낌)으로 고정합니다.
        Blend DstColor One
        ZWrite Off
        Cull Off

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
                float4 color : COLOR; 
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Radius;
                half _Feather;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = distance(input.uv, center);
                half circleMask = 1.0 - smoothstep(_Radius - _Feather, _Radius, dist);

                half4 baseColor = _Color * input.color;
                
                // 마스크 적용 (알파 채널에 마스크 값을 곱함)
                half3 finalRGB = baseColor.rgb * baseColor.a * circleMask;

                return half4(finalRGB, 1.0);
            }
            ENDHLSL
        }
    }
}