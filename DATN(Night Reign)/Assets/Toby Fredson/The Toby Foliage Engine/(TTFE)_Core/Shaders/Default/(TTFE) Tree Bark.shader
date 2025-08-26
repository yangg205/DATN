Shader "TTFE/URP/TreeBark"
{
    Properties
    {
        [Header(Textures)]
        _AlbedoMap("Albedo Map", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _MaskMap("Mask Map", 2D) = "white" {}

        [Header(Texture Settings)]
        _Tiling("Tiling", Vector) = (1,1,0,0)
        _Offset("Offset", Vector) = (0,0,0,0)
        _NormalIntensity("Normal Intensity", Range(0,3)) = 1
        _Smoothnessintensity("Smoothness Intensity", Range(0,1)) = 1
        _Aointensity("AO Intensity", Range(0,1)) = 1

        [Header(Wind Settings)]
        _GlobalWindStrength("Global Wind Strength", Range(0,1)) = 1
        _BranchWindLarge("Branch Wind Large", Range(0,20)) = 1
        _BranchWindSmall("Branch Wind Small", Range(0,20)) = 1
        [Toggle]_PivotSway("Pivot Sway", Float) = 0
        _PivotSwayPower("Pivot Sway Power", Float) = 1
    }

    SubShader
    {
        Tags{"RenderType"="Opaque" "Queue"="Geometry"}
        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode"="UniversalForward"}
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _LIGHTMAP
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            sampler2D _AlbedoMap;
            sampler2D _NormalMap;
            sampler2D _MaskMap;

            float4 _Tiling;
            float4 _Offset;
            float _NormalIntensity;
            float _Smoothnessintensity;
            float _Aointensity;

            float _GlobalWindStrength;
            float _BranchWindLarge;
            float _BranchWindSmall;
            float _PivotSway;
            float _PivotSwayPower;

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 pos = v.positionOS.xyz;

                // Simple sway
                float sway = sin(_Time.y + pos.x * 0.1 + pos.y * 0.05) * _GlobalWindStrength;
                pos.x += sway * _BranchWindLarge * 0.01;
                pos.z += sway * _BranchWindSmall * 0.01;

                if(_PivotSway > 0.5)
                {
                    pos.x += sin(_Time.y * 0.5) * _PivotSwayPower * 0.05;
                }

                o.positionWS = TransformObjectToWorld(pos);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.uv = v.uv * _Tiling.xy + _Offset.xy;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Albedo
                half4 albedo = SAMPLE_TEXTURE2D(_AlbedoMap, sampler_AlbedoMap, i.uv);

                // Normal
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));
                normalTS *= _NormalIntensity;
                half3 normalWS = normalize(TransformTangentToWorld(normalTS, half3x3(1,0,0, 0,1,0, 0,0,1), i.normalWS));

                // Mask map
                half4 mask = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, i.uv);
                half metallic   = mask.r;
                half ao         = pow(mask.g, _Aointensity);
                half smoothness = mask.a * _Smoothnessintensity;

                // Lighting
                InputData lightingInput;
                lightingInput.positionWS = i.positionWS;
                lightingInput.normalWS = normalWS;
                lightingInput.viewDirectionWS = GetWorldSpaceViewDir(i.positionWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                lightingInput.fogCoord = ComputeFogFactor(i.positionCS.z);

                SurfaceData surface;
                surface.albedo = albedo.rgb;
                surface.normalWS = normalWS;
                surface.metallic = metallic;
                surface.specular = 0.5;
                surface.smoothness = smoothness;
                surface.occlusion = ao;
                surface.emission = 0;
                surface.alpha = 1;

                half4 color = UniversalFragmentPBR(lightingInput, surface);
                color.rgb = MixFog(color.rgb, lightingInput.fogCoord);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
