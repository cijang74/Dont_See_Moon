Shader "Custom/URPSpriteOutlineLit"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color ("Tint", Color) = (1,1,1,1)
        [HDR] _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.1 // 그림자 모양을 결정하는 임계값
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }

        // 빛과 그림자를 제대로 받기 위해 Culling Off, ZWrite On 설정
        Cull Off
        ZWrite On

        // 1. 빛을 받고 렌더링하는 메인 패스 (Universal Forward)
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // URP 광원 및 그림자 매크로
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD1;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
                float3 normalWS     : NORMAL;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineWidth;
                float _Cutoff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 w = _MainTex_TexelSize.xy * _OutlineWidth;

                // 원본 이미지 샘플링
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * input.color;

                // 상하좌우 픽셀 검사로 외곽선 알파 누적
                float outline = 0;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(w.x, 0)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-w.x, 0)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, w.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -w.y)).a;

                outline = saturate(outline);

                half4 finalColor = c;
                // 원본은 투명한데 주변에 알파가 있다면 외곽선 색상 칠하기
                if (c.a == 0 && outline > 0)
                {
                    finalColor = _OutlineColor;
                }

                // 알파 테스트: 투명한 부분은 렌더링하지 않음 (3D 그림자를 위해 필수)
                clip(finalColor.a - _Cutoff);

                // --- URP 라이팅 계산 ---
                // 노멀 및 방향 벡터
                float3 normalWS = normalize(input.normalWS);
                
                // 메인 광원 데이터 가져오기 (그림자 포함)
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                
                // 빛의 각도에 따른 감쇠 (Lambert)
                half NdotL = max(dot(normalWS, mainLight.direction), 0.0001);
                
                // 빛 색상 * 거리 감쇠 * 그림자 감쇠 * 표면 각도
                half3 diffuseLight = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation * NdotL;

                // 환경광 (Global Illumination / Ambient)
                half3 ambient = SampleSH(normalWS);

                // 최종 색상에 빛과 환경광 곱하기
                finalColor.rgb *= (diffuseLight + ambient);

                return finalColor;
            }
            ENDHLSL
        }

        // 2. 그림자를 드리우기 위한 패스 (Shadow Caster)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                float _Cutoff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                // 그림자 클립 스페이스로 변환
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 w = _MainTex_TexelSize.xy * _OutlineWidth;

                half a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;

                float outline = 0;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(w.x, 0)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-w.x, 0)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, w.y)).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -w.y)).a;

                // 원본 알파와 외곽선 알파 중 더 큰 값을 최종 알파로 사용
                half finalAlpha = max(a, saturate(outline));

                // 컷오프를 통해 투명한 부분은 그림자 생성에서 제외
                clip(finalAlpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
}