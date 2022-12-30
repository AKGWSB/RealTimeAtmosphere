Shader "CasualAtmosphere/Skybox"
{
    Properties
    {
        _SourceHdrTexture ("Source HDR Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Helper.hlsl"
            #include "Scattering.hlsl"
            #include "AtmosphereParameter.hlsl"
            #include "Raymarching.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : SEMANTIC_HELLO_WORLD;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                return o;
            }

            SAMPLER(sampler_LinearClamp);
            Texture2D _skyViewLut;
            Texture2D _transmittanceLut;
            Texture2D _SourceHdrTexture;

            float3 GetSunDisk(in AtmosphereParameter param, float3 eyePos, float3 viewDir, float3 lightDir)
            {
                // 计算入射光照
                float cosine_theta = dot(viewDir, -lightDir);
                float theta = acos(cosine_theta) * (180.0 / PI);
                float3 sunLuminance = param.SunLightColor * param.SunLightIntensity;

                // 判断光线是否被星球阻挡
                float disToPlanet = RayIntersectSphere(float3(0,0,0), param.PlanetRadius, eyePos, viewDir);
                if(disToPlanet >= 0) return float3(0,0,0);

                // 和大气层求交
                float disToAtmosphere = RayIntersectSphere(float3(0,0,0), param.PlanetRadius + param.AtmosphereHeight, eyePos, viewDir);
                if(disToAtmosphere < 0) return float3(0,0,0);

                // 计算衰减
                //float3 hitPoint = eyePos + viewDir * disToAtmosphere;
                //sunLuminance *= Transmittance(param, hitPoint, eyePos);
                sunLuminance *= TransmittanceToAtmosphere(param, eyePos, viewDir, _transmittanceLut, sampler_LinearClamp);

                if(theta < param.SunDiskAngle) return sunLuminance;
                return float3(0,0,0);
            }

            float4 frag (v2f i) : SV_Target
            {
                AtmosphereParameter param = GetAtmosphereParameter();

                float4 color = float4(0, 0, 0, 1);
                float3 viewDir = normalize(i.worldPos);

                Light mainLight = GetMainLight();
                float3 lightDir = -mainLight.direction;

                float h = _WorldSpaceCameraPos.y - param.SeaLevel + param.PlanetRadius;
                float3 eyePos = float3(0, h, 0);
                
                color.rgb += SAMPLE_TEXTURE2D_X(_skyViewLut, sampler_LinearClamp, ViewDirToUV(viewDir)).rgb;
                color.rgb += GetSunDisk(param, eyePos, viewDir, lightDir);

                return color;
            }
            ENDHLSL
        }
    }
}
