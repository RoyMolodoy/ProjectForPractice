using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Cainos.InteractablePixelWater
{
    // URP Renderer Feature for rendering pixel water
    public class PixelWaterRendererFeature : ScriptableRendererFeature
    {
        public static PixelWaterRendererFeature instance;

        [Tooltip("Layer mask of objects that will rendered behind the water")]
        public LayerMask behindWaterMask;

        [Tooltip("The color to use for behind water content if nothing is there")]
        public Color backgroundColor = new (30, 30, 30, 255);

        [Tooltip("Downsample for capturing the behind water content to a texture. Increase this value will improve performance but lower visual quality"), Range(1, 8)]
        public int downsample = 1;

        private PixelWaterRenderPass pixelWaterRenderPass;

        //initializes the render feature and creates the render pass
        public override void Create()
        {
            instance = this;

            //create render pass and configure its execution timing
            pixelWaterRenderPass = new PixelWaterRenderPass();
            pixelWaterRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }

        //adds the render pass to the URP render pipeline
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(pixelWaterRenderPass);
        }
    }


    //RENDER PASS
    public class PixelWaterRenderPass : ScriptableRenderPass
    {
        //temporary render targets for blur processing
        private RenderTargetHandle rtBehindWater;

        //reference to parent feature
        private PixelWaterRendererFeature feature;

        public PixelWaterRenderPass() : base()
        {
            feature = PixelWaterRendererFeature.instance;

            //initialize temporary render target identifiers
            rtBehindWater.Init("_RT_BehindWater");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //setup render targets
            RenderTextureDescriptor camDesc = renderingData.cameraData.cameraTargetDescriptor;
            camDesc.colorFormat = RenderTextureFormat.ARGB32;
            camDesc.width = Mathf.CeilToInt((float)camDesc.width / feature.downsample);
            camDesc.height = Mathf.CeilToInt((float)camDesc.height / feature.downsample);

            cmd.GetTemporaryRT(rtBehindWater.id, camDesc, FilterMode.Bilinear);
        }

        //main rendering execution logic
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Pixel Water Renderer Feature");

            //render objects in behindWaterLayerMask to rtBehindWater
            var filteringSettings = new FilteringSettings(RenderQueueRange.all, feature.behindWaterMask);
            var shaderTags = new List<ShaderTagId>
            {
                new ShaderTagId("UniversalForward"),        // Regular 3D objects
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("SRPDefaultUnlit"),         // Unlit materials
                new ShaderTagId("Universal2D"),             // 2D URP Sprites
                new ShaderTagId("Sprite"),                  // Legacy Sprite shader
            };
            var drawingSettings = CreateDrawingSettings(shaderTags, ref renderingData, SortingCriteria.CommonTransparent);
            cmd.SetRenderTarget(rtBehindWater.Identifier());
            cmd.ClearRenderTarget(true, true, feature.backgroundColor);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            //set as global texture
            cmd.SetGlobalTexture("_BehindWaterTex", rtBehindWater.Identifier());
            context.ExecuteCommandBuffer(cmd);

            //DEBUG STEP: to directly display rtBehindWater to the screen, need to set renderPassEvent to RenderPassEvent.AfterRendering
            //var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
            //cmd.Blit(rtBehindWater.Identifier(), cameraColorTarget);
            //context.ExecuteCommandBuffer(cmd);

            //release cmd
            CommandBufferPool.Release(cmd);
        }

        //clean up temporary resources after rendering completes
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(rtBehindWater.id);
        }
    }
}


