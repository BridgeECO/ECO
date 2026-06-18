Shader "CustomUI/GlowNoiseMaskParticles_Hover_Final"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _HoverAlpha ("Hover Alpha (Script Driven)", Range(0, 1)) = 1.0 
        _BgAlpha ("Background Image Alpha", Range(0, 1)) = 0.0 

        // --- 1. Shape ---
        _LineRatio ("Line Width Ratio", Range(0.01, 1.0)) = 0.8
        _EllipseSquish ("Ellipse Squish (Y-axis)", Range(1.0, 3.0)) = 1.5 
        
        // --- 1a. Noise ---
        _NoiseScale ("Noise Scale", Range(1, 50)) = 20.0
        _NoiseSpeed ("Noise Scroll Speed", Vector) = (0.5, 0.2, 0, 0)
        _WaveringStrength ("Wavering Strength", Range(0, 1)) = 0.7

        // --- 2. Core Glow ---
        [HDR] _CoreColor ("Core Color", Color) = (1, 1, 1, 1)
        _CoreIntensity ("Core Brightness", Range(0, 10)) = 3.0
        _CoreSmoothness ("Core Smoothness", Range(0.0001, 0.5)) = 0.05 

        // --- 3 & 4. Edge Glow ---
        [HDR] _EdgeColor ("Edge Color", Color) = (0.2, 0.6, 1, 1)
        _EdgeSpread ("Edge Spread Size", Range(0.01, 0.5)) = 0.15
        _EdgeSmoothness ("Edge Smoothness", Range(0.001, 0.5)) = 0.1
        _EdgeOpacity ("Edge Opacity", Range(0, 1)) = 1.0 

        // --- 5. Particles ---
        [HDR] _ParticleColor ("Particle Color", Color) = (0.8, 0.9, 1, 1)
        _ParticleDensity ("Particle Density", Range(10, 100)) = 40.0
        _ParticleSpeed ("Particle Speed", Range(0, 2)) = 0.3
        _ParticleSize ("Particle Size", Range(0.01, 0.5)) = 0.15
        _ParticleRandomness ("Particle Randomness", Range(0, 1)) = 0.8
        _ParticleAlpha ("Particle Alpha", Range(0, 1)) = 0.8
        _ParticleSpreadRadius ("Particle Spread Radius", Range(0.01, 1.0)) = 0.4 

        // --- UI Mask Properties ---
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane" 
            "CanUseSpriteAtlas"="True" 
        }
        
        Stencil 
        { 
            Ref [_Stencil] 
            Comp [_StencilComp] 
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask] 
            WriteMask [_StencilWriteMask] 
        }
        
        Cull Off 
        Lighting Off 
        ZWrite Off 
        ZTest [unity_GUIZTestMode] 

        // [수정된 부분 1] 물감 섞기가 아닌 '빛 더하기(Premultiplied)' 방식의 블렌딩으로 변경합니다.
        Blend One OneMinusSrcAlpha 
        
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t 
            { 
                float4 vertex : POSITION; 
                float4 color : COLOR; 
                float2 texcoord : TEXCOORD0; 
            };
            
            struct v2f 
            { 
                float4 vertex : SV_POSITION; 
                fixed4 color : COLOR; 
                float2 texcoord : TEXCOORD0; 
                float2 localPos : TEXCOORD1; 
            };

            sampler2D _MainTex;
            fixed4 _Color;

            float _HoverAlpha;
            float _BgAlpha;
            
            float _CoreSmoothness, _EdgeOpacity; 
            
            float _LineRatio, _EllipseSquish, _NoiseScale, _WaveringStrength;
            float2 _NoiseSpeed;
            fixed4 _CoreColor, _EdgeColor, _ParticleColor;
            float _CoreIntensity, _EdgeSpread, _EdgeSmoothness;
            float _ParticleDensity, _ParticleSpeed, _ParticleSize, _ParticleRandomness, _ParticleAlpha, _ParticleSpreadRadius;

            float hash21(float2 p) 
            { 
                p = frac(p * float2(123.34, 456.21)); 
                p += dot(p, p + 45.32); 
                return frac(p.x * p.y); 
            }
            
            float noise2D(float2 p) 
            {
                float2 i = floor(p); 
                float2 f = frac(p); 
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i); 
                float b = hash21(i + float2(1.0, 0.0)); 
                float c = hash21(i + float2(0.0, 1.0)); 
                float d = hash21(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert(appdata_t v) 
            {
                v2f o; 
                o.vertex = UnityObjectToClipPos(v.vertex); 
                o.texcoord = v.texcoord; 
                o.localPos = v.vertex.xy; 
                o.color = v.color * _Color; 
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 bg = tex2D(_MainTex, i.texcoord) * i.color;
                bg.a *= _BgAlpha; 

                float2 dx_local = float2(ddx(i.localPos.x), ddy(i.localPos.x));
                float2 dx_uv = float2(ddx(i.texcoord.x), ddy(i.texcoord.x));
                float uiWidth = length(dx_local) / max(length(dx_uv), 0.0001);

                float2 dy_local = float2(ddx(i.localPos.y), ddy(i.localPos.y));
                float2 dy_uv = float2(ddx(i.texcoord.y), ddy(i.texcoord.y));
                float uiHeight = length(dy_local) / max(length(dy_uv), 0.0001);

                float aspect = uiWidth / max(uiHeight, 0.0001);

                float2 p = i.texcoord - float2(0.5, 0.5); 
                p.x *= aspect;

                float halfLength = (aspect * 0.5) * _LineRatio;
                float2 a = float2(-halfLength, 0.0); 
                float2 b = float2(halfLength, 0.0);
                float2 pa = p - a; 
                float2 ba = b - a;
                float h = clamp(dot(pa, ba) / max(dot(ba, ba), 0.0001), 0.0, 1.0);
                
                float2 p_mapped = pa - ba * h;
                p_mapped.y *= _EllipseSquish; 
                float glowDist = length(p_mapped); 

                float rawNoise = noise2D(i.texcoord * _NoiseScale + _Time.yy * _NoiseSpeed);
                float noiseMask = lerp(1.0, rawNoise, _WaveringStrength);

                float coreFactor = 1.0 - smoothstep(0.0, _CoreSmoothness, glowDist);
                float3 coreGlow = _CoreColor.rgb * coreFactor * noiseMask * _CoreIntensity * _CoreColor.a;

                float edgeFactor = 1.0 - smoothstep(_EdgeSpread - _EdgeSmoothness, _EdgeSpread + _EdgeSmoothness, glowDist);
                float edgeNoiseMask = lerp(1.0, rawNoise, _WaveringStrength * 0.5);
                float3 edgeGlow = _EdgeColor.rgb * edgeFactor * edgeNoiseMask * _EdgeColor.a * _EdgeOpacity;

                float2 partUV = p;
                partUV.y -= sign(p.y) * _Time.y * _ParticleSpeed; 
                partUV.x -= sign(p.x) * _Time.y * _ParticleSpeed * 0.3;
                
                float2 grid = partUV * _ParticleDensity; 
                float2 gridId = floor(grid); 
                float2 gridF = frac(grid) - 0.5;

                float pRand1 = hash21(gridId); 
                float pRand2 = hash21(gridId + float2(1.0, 1.0));
                gridF += (float2(pRand1, pRand2) - 0.5) * 0.7;

                float currentPSize = _ParticleSize * lerp(1.0, pRand1, _ParticleRandomness);
                float pDist = length(gridF);
                
                float pShape = 1.0 - smoothstep(currentPSize * 0.1, currentPSize, pDist);
                float pBlink = sin(_Time.y * 4.0 + pRand1 * 10.0) * 0.5 + 0.5;
                float pFade = 1.0 - smoothstep(_ParticleSpreadRadius * 0.1, _ParticleSpreadRadius, length(p));
                float pSpawn = step(0.7, pRand2); 

                float particleAlpha = pShape * pFade * pBlink * pSpawn * _ParticleAlpha * _ParticleColor.a;
                float3 particleGlow = _ParticleColor.rgb * particleAlpha;

                // --- 4. 최종 합성 ---
                float3 totalGlowRGB = (edgeGlow + coreGlow + particleGlow) * _HoverAlpha;

                fixed4 finalCol;
                
                // [수정된 부분 2] 이미 알파가 곱해진 배경색에, 순수한 빛(totalGlowRGB)을 그대로 더해줍니다.
                finalCol.rgb = (bg.rgb * bg.a) + totalGlowRGB;
                
                // [핵심 변경] 빛은 물리적으로 뒤에 있는 배경을 가리지 않습니다! 
                // 따라서 최종 알파값은 오직 '배경 이미지의 알파값(bg.a)'만 내보냅니다.
                // 이렇게 하면 글로우가 부드럽게 퍼질 때 뒤의 배경을 어둡게 만드는 현상이 완벽히 사라집니다.
                finalCol.a = bg.a;

                return finalCol;
            }
            ENDCG
        }
    }
}