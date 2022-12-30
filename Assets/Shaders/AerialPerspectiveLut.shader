Shader "CasualAtmosphere/AerialPerspectiveLut"
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"
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

            float _AerialPerspectiveDistance;
            float4 _AerialPerspectiveVoxelSize;

            SAMPLER(sampler_LinearClamp);
            Texture2D _transmittanceLut;
            Texture2D _multiScatteringLut;

            float4 frag (v2f i) : SV_Target
            {
                AtmosphereParameter param = GetAtmosphereParameter();

                float4 color = float4(0, 0, 0, 1);
                float3 uv = float3(i.uv, 0);
                uv.x *= _AerialPerspectiveVoxelSize.x * _AerialPerspectiveVoxelSize.z;  // X * Z
                uv.z = int(uv.x / _AerialPerspectiveVoxelSize.z) / _AerialPerspectiveVoxelSize.x;
                uv.x = fmod(uv.x, _AerialPerspectiveVoxelSize.z) / _AerialPerspectiveVoxelSize.x;
                uv.xyz += 0.5 / _AerialPerspectiveVoxelSize.xyz;

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float3 viewDir = normalize(mul(unity_CameraToWorld, float4(
                    (uv.x * 2.0 - 1.0) * 1.0, 
                    (uv.y * 2.0 - 1.0) / aspect, 
                    1.0, 0.0
                )).xyz);
                //return float4(viewDir, 1.0);

                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
                
                float h = _WorldSpaceCameraPos.y - param.SeaLevel + param.PlanetRadius;
                float3 eyePos = float3(0, h, 0);

                float maxDis = uv.z * _AerialPerspectiveDistance;

                // inScattering
                color.rgb = GetSkyView(
                    param, eyePos, viewDir, lightDir, maxDis,
                    _transmittanceLut, _multiScatteringLut, sampler_LinearClamp
                );

                // transmittance
                float3 voxelPos = eyePos + viewDir * maxDis;
                float3 t1 = TransmittanceToAtmosphere(param, eyePos, viewDir, _transmittanceLut, sampler_LinearClamp);
                float3 t2 = TransmittanceToAtmosphere(param, voxelPos, viewDir, _transmittanceLut, sampler_LinearClamp);
                float3 t = t1 / t2;
                color.a = dot(t, float3(1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0));

                return color;
            }
            ENDHLSL
        }
    }
}
