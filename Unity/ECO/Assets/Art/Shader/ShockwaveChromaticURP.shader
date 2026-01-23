Shader "Hidden/ShockwaveChromaticURP"
{
    Properties
    {
        _Centre ("Centre (UV)", Vector) = (0.5,0.5,0,0)
        _T ("T", Float) = 0
        _MaxRadius ("Max Radius", Float) = 0.5
        _RippleWidth ("Ripple Width", Float) = 0.05
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

            // ✅ Full Screen Pass가 넘겨주는 화면 컬러 텍스처
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float4 _BlitTexture_TexelSize; // x=1/w, y=1/h, z=w, w=h

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

            // ✅ 메시 없이도 그려지는 Fullscreen triangle
            Varyings Vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = GetFullScreenTriangleVertexPosition(v.vertexID);
                o.uv = GetFullScreenTriangleTexCoord(v.vertexID);
                return o;
            }

            float GetOffsetStrength(float t, float2 dir, float2 aspect)
            {
                float d = length(dir / aspect) - t * _MaxRadius;

                d *= 1.0 - smoothstep(0.0, _RippleWidth, abs(d));
                d *= smoothstep(0.0, _RippleWidth, t);
                d *= 1.0 - smoothstep(0.5, 1.0, t);
                return d;
            }

            float4 Frag (Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                float2 aspect = float2(_BlitTexture_TexelSize.z / _BlitTexture_TexelSize.w, 1.0);

                float2 centre = _Centre.xy;
                float2 dir = centre - uv;

                float tOffset = _ChromaticAmp * sin(_T * 3.14159265);

                float rD = GetOffsetStrength(_T + tOffset, dir, aspect);
                float gD = GetOffsetStrength(_T,           dir, aspect);
                float bD = GetOffsetStrength(_T - tOffset, dir, aspect);

                float2 ndir = normalize(dir + 1e-6);

                float r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv + ndir * rD).r;
                float g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv + ndir * gD).g;
                float b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv + ndir * bD).b;

                float shading = gD * _ShadingAmp;
                float3 col = float3(r, g, b) + shading;

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}

