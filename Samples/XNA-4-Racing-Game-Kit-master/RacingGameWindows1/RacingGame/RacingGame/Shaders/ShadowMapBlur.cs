#region File Description
//-----------------------------------------------------------------------------
// ShadowMapBlur.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using RacingGame.Graphics;
using RacingGame.Helpers;
#endregion

namespace RacingGame.Shaders
{
    /// <summary>
    /// ShadowMapBlur based on PostScreenBlur to blur the final shadow map
    /// output for a more smooth view on the screen.
    /// </summary>
    public class ShadowMapBlur : ShaderEffect
    {
        static readonly BlendState ZeroSourceBlend = new BlendState()
        {
            AlphaBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.Zero,
            ColorSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.SourceColor
        };

        #region Variables
        /// <summary>
        /// The shader effect filename for this shader.
        /// </summary>
        private const string Filename = "PostScreenShadowBlur.fx";

        /// <summary>
        /// Effect handles for window size and scene map.
        /// </summary>
        private EffectParameter windowSize,
            sceneMap,
            blurMap;

        /// <summary>
        /// Links to the render to texture instances.
        /// </summary>
        private RenderToTexture sceneMapTexture,
            blurMapTexture;

        /// <summary>
        /// Scene map texture
        /// </summary>
        /// <returns>Render to texture</returns>
        public RenderToTexture SceneMapTexture
        {
            get
            {
                return sceneMapTexture;
            }
        }

        /// <summary>
        /// Blur map texture
        /// </summary>
        /// <returns>Render to texture</returns>
        public RenderToTexture BlurMapTexture
        {
            get
            {
                return blurMapTexture;
            }
        }
        #endregion

        #region Create
        /// <summary>
        /// Create shadow map screen blur shader.
        /// obs, using full size again: But only use 1/4 of the screen!
        /// </summary>
        public ShadowMapBlur()
            : base(Filename)
        {
            // Scene map texture
            sceneMapTexture = new RenderToTexture(
                //RenderToTexture.SizeType.FullScreen);
                //improve performance:
                RenderToTexture.SizeType.HalfScreen);
            blurMapTexture = new RenderToTexture(
                //RenderToTexture.SizeType.FullScreen);
                //improve performance:
                RenderToTexture.SizeType.HalfScreen);
        }
        #endregion

        #region Get parameters
        /// <summary>
        /// Reload
        /// </summary>
        protected override void GetParameters()
        {
            // Can't get parameters if loading failed!
            if (effect == null)
                return;

            windowSize = effect.Parameters["windowSize"];
            sceneMap = effect.Parameters["sceneMap"];
            blurMap = effect.Parameters["blurMap"];

            // We need both windowSize and sceneMap.
            if (windowSize == null ||
                sceneMap == null)
                throw new NotSupportedException("windowSize and sceneMap must be " +
                    "valid in PostScreenShader=" + Filename);
        }
        #endregion

        #region RenderShadows
        /// <summary>
        /// Render shadows
        /// </summary>
        /// <param name="renderCode">Render code</param>
        public void RenderShadows(BaseGame.RenderHandler renderCode)
        {
            if (renderCode == null)
                throw new ArgumentNullException("renderCode");

            // Render into our scene map texture
            sceneMapTexture.SetRenderTarget();

            // Clear render target
            sceneMapTexture.Clear(Color.White);

            // Render everything
            renderCode();

            // Resolve render target
            sceneMapTexture.Resolve();

            // Restore back buffer as render target
            BaseGame.ResetRenderTarget(true);
        }
        #endregion

        #region ShowShadows
        /// <summary>
        /// Show shadows with help of our blur map shader
        /// </summary>
        public void RenderShadows()
        {
            // Only apply post screen blur if texture is valid and effect are valid
            if (sceneMapTexture == null ||
                Valid == false ||
                // If the shadow scene map is not yet filled, there is no point
                // continuing here ...
                sceneMapTexture.XnaTexture == null)
                return;

            // Don't use or write to the z buffer
            BaseGame.Device.DepthStencilState = DepthStencilState.None;
            // Disable alpha for the first pass
            BaseGame.Device.BlendState = BlendState.Opaque;

            if (windowSize != null)
                windowSize.SetValue(
                    new float[] { sceneMapTexture.Width, sceneMapTexture.Height });
            if (sceneMap != null)
                sceneMap.SetValue(sceneMapTexture.XnaTexture);

            effect.CurrentTechnique = effect.Techniques["ScreenAdvancedBlur20"];

            // We must have exactly 2 passes!
            if (effect.CurrentTechnique.Passes.Count != 2)
                throw new InvalidOperationException(
                    "This shader should have exactly 2 passes!");

            // Just start pass 0
            
                blurMapTexture.SetRenderTarget();

                EffectPass effectPass = effect.CurrentTechnique.Passes[0];
                effectPass.Apply();
                VBScreenHelper.Render();


            blurMapTexture.Resolve();
            BaseGame.ResetRenderTarget(false);

            // Restore z buffer state
            BaseGame.Device.DepthStencilState = DepthStencilState.Default;
            // Set u/v addressing back to wrap
            BaseGame.Device.SamplerStates[0] = SamplerState.LinearWrap;
            // Restore normal alpha blending
            //BaseGame.Device.RenderState.BlendFunction = BlendFunction.Add;
            BaseGame.SetCurrentAlphaMode(BaseGame.AlphaMode.Default);
        }

        /// <summary>
        /// Show shadows with help of our blur map shader
        /// </summary>
        public void ShowShadows()
        {
            // Only apply post screen blur if texture is valid and effect are valid
            if (blurMapTexture == null ||
                Valid == false ||
                // If the shadow scene map is not yet filled, there is no point
                // continuing here ...
                blurMapTexture.XnaTexture == null)
                return;

            // Don't use or write to the z buffer
            BaseGame.Device.DepthStencilState = DepthStencilState.None;

            // Make sure we clamp everything to 0-1
            BaseGame.Device.SamplerStates[0] = SamplerState.LinearClamp;

            // Restore back buffer as render target
            //not required: BaseGame.ResetRenderTarget(false);

            if (blurMap != null)
                blurMap.SetValue(blurMapTexture.XnaTexture);

            effect.CurrentTechnique = effect.Techniques["ScreenAdvancedBlur20"];

            // We must have exactly 2 passes!
            if (effect.CurrentTechnique.Passes.Count != 2)
                throw new InvalidOperationException(
                    "This shader should have exactly 2 passes!");

            // Render second pass

            // Use ZeroSourceBlend alpha mode for the final result
            BaseGame.Device.BlendState = ZeroSourceBlend;
                /*BaseGame.Device.RenderState.AlphaBlendEnable = true;
                BaseGame.Device.RenderState.AlphaBlendOperation = BlendFunction.Add;
                BaseGame.Device.RenderState.SourceBlend = Blend.Zero;
                BaseGame.Device.RenderState.DestinationBlend = Blend.SourceColor;*/

                EffectPass effectPass = effect.CurrentTechnique.Passes[1];
                effectPass.Apply();
                VBScreenHelper.Render();

            // Restore z buffer state
            BaseGame.Device.DepthStencilState = DepthStencilState.Default;
            // Set u/v addressing back to wrap
            BaseGame.Device.SamplerStates[0] = SamplerState.LinearWrap;
            // Restore normal alpha blending
            //BaseGame.Device.RenderState.BlendFunction = BlendFunction.Add;
            BaseGame.SetCurrentAlphaMode(BaseGame.AlphaMode.Default);
        }
        #endregion
    }
}
