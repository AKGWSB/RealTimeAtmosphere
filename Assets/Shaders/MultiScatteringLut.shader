Shader "CasualAtmosphere/MultiScatteringLut"
{
    Properties
    {

    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            SAMPLER(sampler_LinearClamp);
            Texture2D _transmittanceLut;

            float4 frag (v2f i) : SV_Target
            {
                AtmosphereParameter param = GetAtmosphereParameter();

                float4 color = float4(0, 0, 0, 1);
                float2 uv = i.uv;

                float mu_s = uv.x * 2.0 - 1.0;
                float r = uv.y * param.AtmosphereHeight + param.PlanetRadius;

                float cos_theta = mu_s;
                float sin_theta = sqrt(1.0 - cos_theta * cos_theta);
                float3 lightDir = float3(sin_theta, cos_theta, 0);
                float3 p = float3(0, r, 0);

                color.rgb = IntegralMultiScattering(param, p, lightDir, _transmittanceLut, sampler_LinearClamp);
                //color.rg = uv;
                return color;
            }
            ENDHLSL
        }
    }
}
