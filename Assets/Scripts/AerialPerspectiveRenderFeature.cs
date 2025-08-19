using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class AerialPerspectiveRenderFeature : ScriptableRendererFeature
{
	class PassData
	{
		public Material material;
		public TextureHandle src;
		public TextureHandle dst;
	}

	CustomRenderPass m_ScriptablePass;
	public Material m_AerialPerspectiveMaterial;

	class CustomRenderPass : ScriptableRenderPass
    {
		public RTHandle tempRTHandle;
		public RTHandle blitSrc;
		public Material material;

		public CustomRenderPass m_ScriptablePass { get; private set; }

		[Obsolete]
		public override void OnCameraSetup( CommandBuffer cmd, ref RenderingData renderingData )
        {
            RenderTextureDescriptor rtDesc = renderingData.cameraData.cameraTargetDescriptor;
            tempRTHandle = RTHandles.Alloc( tempRTHandle.GetInstanceID( ), name: "_TempAerialPerspectiveRT" );

            ConfigureTarget(tempRTHandle);
            blitSrc = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }
        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            RenderTargetIdentifier tempRT = tempRTHandle.rt;

            Blitter.BlitCameraTexture(cmd, blitSrc, tempRTHandle, m_ScriptablePass.material, 0);
            Blitter.BlitCameraTexture(cmd, tempRTHandle, blitSrc);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			if( m_ScriptablePass == null )
			{
                return;
			}

			// Import the source and destination RTHandles into the RenderGraph
			var src = renderGraph.ImportTexture(blitSrc);
            var dst = renderGraph.ImportTexture(tempRTHandle);

			// Add a render pass to the graph
			PassData passData = new PassData
            {
				material = m_ScriptablePass.material,
                src = src,
                dst = dst
            };

            RenderGraphUtils.BlitMaterialParameters blitParamsTemp = new RenderGraphUtils.BlitMaterialParameters
            {
                material = passData.material,
                shaderPass = 0
            };
			renderGraph.AddBlitPass( blitParamsTemp, "TempAerialPerspectivePass" );

			RenderGraphUtils.BlitMaterialParameters blitParamsFinal = new RenderGraphUtils.BlitMaterialParameters
			{
				material = passData.material,
				shaderPass = 1
			};
			renderGraph.AddBlitPass( blitParamsFinal, "AerialPerspectivePass" );
		}

		public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (tempRTHandle != null)
            {
                RTHandles.Release(tempRTHandle);
                tempRTHandle = null;
            }
        }
    }

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        m_ScriptablePass.material = m_AerialPerspectiveMaterial;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
    