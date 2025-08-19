Shader "CasualAtmosphere/SkyViewLut"
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

            SAMPLER(sampler_skyViewLinearClamp);
            Texture2D _transmittanceLut;
            Texture2D _multiScatteringLut;

            float4 frag (v2f i) : SV_Target
            {
                AtmosphereParameter param = GetAtmosphereParameter();

                float4 color = float4(0, 0, 0, 1);
                float2 uv = i.uv;
                float3 viewDir = UVToViewDir(uv);

                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
                
                float h = _WorldSpaceCameraPos.y - param.SeaLevel + param.PlanetRadius;
                float3 eyePos = float3(0, h, 0);

                color.rgb = GetSkyView(
                    param, eyePos, viewDir, lightDir, -1.0f,
                    _transmittanceLut, _multiScatteringLut, sampler_LinearClamp
                );

                return color;
            }
            ENDHLSL
        }
    }
}
