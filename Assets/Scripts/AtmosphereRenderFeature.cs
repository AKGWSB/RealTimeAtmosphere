using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AtmosphereRenderFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        RenderTexture m_skyViewLut;
        RenderTexture m_transmittanceLut;
        RenderTexture m_multiScatteringLut;
        RenderTexture m_aerialPerspectiveLut;
        public Texture2D m_aerialPerspectiveLutReadBackBuffer;

        public Material skyViewLutMaterial;
        public Material transmittanceLutMaterial;
        public Material multiScatteringLutMaterial;
        public Material aerialPerspectiveLutMaterial;

        public AtmosphereSettings atmosphereSettings;

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            m_skyViewLut = RenderTexture.GetTemporary(256, 128, 0, RenderTextureFormat.ARGBFloat);
            m_transmittanceLut = RenderTexture.GetTemporary(256, 64, 0, RenderTextureFormat.ARGBFloat);
            m_multiScatteringLut = RenderTexture.GetTemporary(32, 32, 0, RenderTextureFormat.ARGBFloat);
            m_aerialPerspectiveLut = RenderTexture.GetTemporary(32 * 32, 32, 0, RenderTextureFormat.ARGBFloat);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            cmd.SetGlobalTexture("_skyViewLut", m_skyViewLut);
            cmd.SetGlobalTexture("_transmittanceLut", m_transmittanceLut);
            cmd.SetGlobalTexture("_multiScatteringLut", m_multiScatteringLut);
            cmd.SetGlobalTexture("_aerialPerspectiveLut", m_aerialPerspectiveLut);
            cmd.SetGlobalFloat("_SeaLevel", atmosphereSettings.SeaLevel);
            cmd.SetGlobalFloat("_PlanetRadius", atmosphereSettings.PlanetRadius);
            cmd.SetGlobalFloat("_AtmosphereHeight", atmosphereSettings.AtmosphereHeight);
            cmd.SetGlobalFloat("_SunLightIntensity", atmosphereSettings.SunLightIntensity);
            cmd.SetGlobalColor("_SunLightColor", atmosphereSettings.SunLightColor);
            cmd.SetGlobalFloat("_SunDiskAngle", atmosphereSettings.SunDiskAngle);
            cmd.SetGlobalFloat("_RayleighScatteringScale", atmosphereSettings.RayleighScatteringScale);
            cmd.SetGlobalFloat("_RayleighScatteringScalarHeight", atmosphereSettings.RayleighScatteringScalarHeight);
            cmd.SetGlobalFloat("_MieScatteringScale", atmosphereSettings.MieScatteringScale);
            cmd.SetGlobalFloat("_MieAnisotropy", atmosphereSettings.MieAnisotropy);
            cmd.SetGlobalFloat("_MieScatteringScalarHeight", atmosphereSettings.MieScatteringScalarHeight);
            cmd.SetGlobalFloat("_OzoneAbsorptionScale", atmosphereSettings.OzoneAbsorptionScale);
            cmd.SetGlobalFloat("_OzoneLevelCenterHeight", atmosphereSettings.OzoneLevelCenterHeight);
            cmd.SetGlobalFloat("_OzoneLevelWidth", atmosphereSettings.OzoneLevelWidth);
            cmd.SetGlobalFloat("_AerialPerspectiveDistance", atmosphereSettings.AerialPerspectiveDistance);
            cmd.SetGlobalVector("_AerialPerspectiveVoxelSize", new Vector4(32, 32, 32, 0));

            cmd.Blit(null, m_transmittanceLut, transmittanceLutMaterial);
            cmd.Blit(null, m_multiScatteringLut, multiScatteringLutMaterial);
            cmd.Blit(null, m_skyViewLut, skyViewLutMaterial);
            cmd.Blit(null, m_aerialPerspectiveLut, aerialPerspectiveLutMaterial);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);

            // for debug
            if(renderingData.cameraData.isSceneViewCamera)
            {
                Camera camera = renderingData.cameraData.camera;
                var cameraPos = camera.transform.position;

                var oldRT = RenderTexture.active;
                RenderTexture.active = m_aerialPerspectiveLut;
                m_aerialPerspectiveLutReadBackBuffer.ReadPixels(new Rect(0, 0, 32, 32), 0, 0);
                m_aerialPerspectiveLutReadBackBuffer.Apply();
                RenderTexture.active = oldRT;

                var data = m_aerialPerspectiveLutReadBackBuffer.GetPixelData<Vector4>(0);
                int index = 0;
                for(int i=0; i<32; i++)
                {
                    for(int j=0; j<32; j++)
                    {
                        var d4 = data[index];
                        //Debug.Log(d4);
                        Vector3 dir = new Vector3(d4.x, d4.y, d4.z);
                        Debug.DrawLine(cameraPos, cameraPos + dir * 100.0f);
                    }
                }
            }
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            RenderTexture.ReleaseTemporary(m_skyViewLut);
            RenderTexture.ReleaseTemporary(m_transmittanceLut);
            RenderTexture.ReleaseTemporary(m_multiScatteringLut);
            RenderTexture.ReleaseTemporary(m_aerialPerspectiveLut);
        }
    }

    CustomRenderPass m_ScriptablePass;
    public Material skyViewLutMaterial;
    public Material transmittanceLutMaterial;
    public Material multiScatteringLutMaterial;
    public Material aerialPerspectiveLutMaterial;
    //public ComputeShader aerialPerspectiveLutCS;
    public AtmosphereSettings atmosphereSettings;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRendering;
        m_ScriptablePass.skyViewLutMaterial = skyViewLutMaterial;
        m_ScriptablePass.transmittanceLutMaterial = transmittanceLutMaterial;
        m_ScriptablePass.multiScatteringLutMaterial = multiScatteringLutMaterial;
        m_ScriptablePass.atmosphereSettings = atmosphereSettings;
        m_ScriptablePass.aerialPerspectiveLutMaterial = aerialPerspectiveLutMaterial;
        m_ScriptablePass.m_aerialPerspectiveLutReadBackBuffer = new Texture2D(32, 32, TextureFormat.ARGB32, false);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


