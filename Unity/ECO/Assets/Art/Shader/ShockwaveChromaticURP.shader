Shader "Hidden/ShockwaveChromaticURP"
{
    Properties
    {
        _Centre ("Centre (UV)", Vector) = (0.5,0.5,0,0)
        _T ("T", Float) = 0
        _MaxRadius ("Max Radius (0..1 of min screen dim)", Float) = 0.5
        _RippleWidth ("Ripple Width (0..1 of min screen dim)", Float) = 0.05
        _ChromaticAmp ("Chromatic Amp", Float) = 0.05
        _ShadingAmp ("Shading Amp", Float) = 8
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

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float4 _BlitTexture_TexelSize; // x=1/w, y=1/h, z=w, w=h
            float4 _BlitScaleBias;         // xy=scale, zw=bias

            float4 _Centre;
            float _T;
            float _MaxRadius;
            float _RippleWidth;
            float _ChromaticAmp;
            float _ShadingAmp;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = GetFullScreenTriangleVertexPosition(v.vertexID);
                o.uv = GetFullScreenTriangleTexCoord(v.vertexID);
                return o;
            }

            // t: 0..1, dirPx: 픽셀 기준 방향(centre->uv), minDim: min(w,h)
            float GetOffsetStrength(float t, float2 dirPx, float minDim)
            {
                // 픽셀 거리 → minDim으로 정규화해서 "화면비 무관" 거리로 만듦
                float distN = length(dirPx) / max(minDim, 1.0);
                float d = distN - t * _MaxRadius;

                d *= 1.0 - smoothstep(0.0, _RippleWidth, abs(d)); // ripple mask
                d *= smoothstep(0.0, _RippleWidth, t);            // intro
                d *= 1.0 - smoothstep(0.5, 1.0, t);               // outro
                return d; // "minDim 기준 정규화" 단위
            }

            float4 Frag (Varyings i) : SV_Target
            {
                // ✅ 올바른 Blit UV
                float2 uv = i.uv * _BlitScaleBias.xy + _BlitScaleBias.zw;

                float2 centre = _Centre.xy;

                // 화면 크기(픽셀)
                float w = _BlitTexture_TexelSize.z;
                float h = _BlitTexture_TexelSize.w;
                float minDim = min(w, h);

                // ✅ UV → 픽셀 공간으로 변환해서 거리 계산 (원이 절대 안 찌그러짐)
                float2 dirUv = centre - uv;
                float2 dirPx = dirUv * float2(w, h);

                // 픽셀 공간에서의 단위 방향 → UV 방향으로 다시 변환(샘플 오프셋용)
                float2 ndirPx = normalize(dirPx + 1e-6);
                float2 ndirUv = ndirPx / float2(w, h);

                float tOffset = _ChromaticAmp * sin(_T * 3.14159265);

                float rN = GetOffsetStrength(_T + tOffset, dirPx, minDim);
                float gN = GetOffsetStrength(_T,           dirPx, minDim);
                float bN = GetOffsetStrength(_T - tOffset, dirPx, minDim);

                // 정규화 단위(rN/gN/bN)를 "픽셀 변위"로 바꿔서 UV 오프셋으로 적용
                float rPx = rN * minDim;
                float gPx = gN * minDim;
                float bPx = bN * minDim;

                float r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv + ndirUv * rPx).r;
                float g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv + ndirUv * gPx).g;
                float b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv + ndirUv * bPx).b;

                float shading = gN * _ShadingAmp;

                float3 col = float3(r, g, b) + shading;
                return float4(saturate(col), 1);
            }
            ENDHLSL
        }
    }
}



