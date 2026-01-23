Shader "Hidden/ShockwaveChromaticURP"
{
    Properties
    {
        _Centre ("Centre (UV)", Vector) = (0.5,0.5,0,0)
        _T ("T", Float) = 0

        // min(screenWidth, screenHeight) 기준 비율 (1보다 크게 써도 됨)
        _MaxRadius ("Max Radius (min-dim ratio, can be >1)", Float) = 2.0
        _RippleWidth ("Ripple Width (min-dim ratio)", Float) = 0.06

        // 왜곡 세기(검정 튐/과한 늘어남 방지용)
        _DistortAmp ("Distortion Amount", Float) = 0.35

        // RGB 시간 오프셋(색수차 세기)
        _ChromaticAmp ("Chromatic Time Offset", Float) = 0.05

        // 링 주변 밝기(검정 띠 방지 위해 "밝게만" 더함)
        _ShadingAmp ("Shading Amp (adds only)", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Fullscreen"
            ZTest Always ZWrite Off Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Full Screen Pass input (camera color)
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float4 _BlitTexture_TexelSize; // x=1/w, y=1/h, z=w, w=h

            // UV scale/bias for blit targets (필수: 잘못 샘플링/흑백 방지)
            float4 _BlitScaleBias; // xy = scale, zw = bias

            float4 _Centre;
            float _T;
            float _MaxRadius;
            float _RippleWidth;
            float _DistortAmp;
            float _ChromaticAmp;
            float _ShadingAmp;

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = GetFullScreenTriangleVertexPosition(v.vertexID);
                o.uv = GetFullScreenTriangleTexCoord(v.vertexID);
                return o;
            }

            // dirPx: centre-uv를 픽셀로 변환한 벡터
            // 반환값 단위: "minDim 기준 정규화"
            float GetOffsetStrength(float t, float2 dirPx, float minDim)
            {
                float distN = length(dirPx) / max(minDim, 1.0);
                float d = distN - t * _MaxRadius;

                // 링(띠)만 남기기
                d *= 1.0 - smoothstep(0.0, _RippleWidth, abs(d));

                // intro/outro
                d *= smoothstep(0.0, _RippleWidth, t);
                d *= 1.0 - smoothstep(0.5, 1.0, t);
                return d;
            }

            float2 Clamp01(float2 x) { return clamp(x, 0.0, 1.0); }

            float4 Frag (Varyings i) : SV_Target
            {
                // ✅ 올바른 Blit UV
                float2 uv = i.uv * _BlitScaleBias.xy + _BlitScaleBias.zw;

                float w = _BlitTexture_TexelSize.z;
                float h = _BlitTexture_TexelSize.w;
                float minDim = min(w, h);

                float2 centre = _Centre.xy;

                // ✅ 픽셀 공간에서 원형 거리 계산 (화면 비율에 상관없이 원 유지)
                float2 dirUv = centre - uv;
                float2 dirPx = dirUv * float2(w, h);

                // 픽셀 단위 방향 -> UV 방향
                float2 ndirPx = normalize(dirPx + 1e-6);
                float2 ndirUv = ndirPx / float2(w, h);

                float tOffset = _ChromaticAmp * sin(_T * 3.14159265);

                float rN = GetOffsetStrength(_T + tOffset, dirPx, minDim);
                float gN = GetOffsetStrength(_T,           dirPx, minDim);
                float bN = GetOffsetStrength(_T - tOffset, dirPx, minDim);

                // 변위량: minDim 기준 정규화 -> 픽셀 -> UV 오프셋
                float rPx = rN * minDim * _DistortAmp;
                float gPx = gN * minDim * _DistortAmp;
                float bPx = bN * minDim * _DistortAmp;

                // ✅ 화면 밖 샘플링 방지(검정 튐 방지)
                float2 rUV = Clamp01(uv + ndirUv * rPx);
                float2 gUV = Clamp01(uv + ndirUv * gPx);
                float2 bUV = Clamp01(uv + ndirUv * bPx);

                float r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, rUV).r;
                float g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, gUV).g;
                float b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, bUV).b;

                // ✅ 검정 띠 방지: 밝게만 더하기(음수 제거)
                float shading = max(0.0, gN) * _ShadingAmp;

                float3 col = float3(r, g, b) + shading;
                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}



