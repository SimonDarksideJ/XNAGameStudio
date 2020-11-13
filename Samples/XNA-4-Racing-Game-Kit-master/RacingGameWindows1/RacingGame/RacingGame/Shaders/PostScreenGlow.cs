#region File Description
//-----------------------------------------------------------------------------
// PostScreenGlow.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections;
using System.Text;
using System.IO;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Texture = RacingGame.Graphics.Texture;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
#endregion

namespace RacingGame.Shaders
{
    /// <summary>
    /// Post screen glow shader based on PostScreenGlow.fx.
    /// Derive from PostScreenMenu, this way we can save duplicating
    /// the effect parameters and use the same RenderToTextures.
    /// </summary>
    public class PostScreenGlow : PostScreenMenu
    {
        static readonly BlendState BlendStateAlphaWrite = new BlendState() {
                ColorSourceBlend = Blend.One,
                AlphaSourceBlend = Blend.Zero,

                ColorDestinationBlend = Blend.Zero,
                AlphaDestinationBlend = Blend.One
            };

        #region Variables
        /// <summary>
        /// The shader effect filename for this shader.
        /// </summary>
        private const string Filename = "PostScreenGlow.fx";

        /// <summary>
        /// Effect handles for window size and scene map.
        /// </summary>
        private EffectParameter radialSceneMap,
            radialBlurScaleFactor,
            screenBorderFadeoutMap;

        /// <summary>
        /// Links to the passTextures, easier to write code this way.
        /// This are just reference copies.
        /// </summary>
        private RenderToTexture radialSceneMapTexture;

        /// <summary>
        /// Helper texture for the screen border (darken the borders).
        /// </summary>
        private Texture screenBorderFadeoutMapTexture = null;
        #endregion

        #region Properties
        /// <summary>
        /// Last used radial blur scale factor
        /// </summary>
        private float lastUsedRadialBlurScaleFactor = 0;
        /// <summary>
        /// Radial blur scale factor
        /// </summary>
        public float RadialBlurScaleFactor
        {
            get
            {
                return lastUsedRadialBlurScaleFactor;
            }
            set
            {
                if (radialBlurScaleFactor != null &&
                    lastUsedRadialBlurScaleFactor != value)
                {
                    lastUsedRadialBlurScaleFactor = value;
                    radialBlurScaleFactor.SetValue(value);
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create post screen glow
        /// </summary>
        public PostScreenGlow()
            : base(Filename)
        {
            // Final map for glow, used to perform radial blur next step
            radialSceneMapTexture = new RenderToTexture(
                RenderToTexture.SizeType.FullScreen);
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

            // We need both windowSize and sceneMap.
            if (windowSize == null ||
                sceneMap == null)
                throw new NotSupportedException("windowSize and sceneMap must be " +
                    "valid in PostScreenShader=" + Filename);

            // Init additional stuff
            downsampleMap = effect.Parameters["downsampleMap"];
            blurMap1 = effect.Parameters["blurMap1"];
            blurMap2 = effect.Parameters["blurMap2"];
            radialSceneMap = effect.Parameters["radialSceneMap"];

            // Load screen border texture
            screenBorderFadeoutMap = effect.Parameters["screenBorderFadeoutMap"];
            screenBorderFadeoutMapTexture = new Texture("ScreenBorderFadeout.dds");
            // Set texture
            screenBorderFadeoutMap.SetValue(
                screenBorderFadeoutMapTexture.XnaTexture);

            radialBlurScaleFactor = effect.Parameters["radialBlurScaleFactor"];
        }
        #endregion

        #region Show
        /// <summary>
        /// Execute shaders and show result on screen, Start(..) must have been
        /// called before and the scene should be rendered to sceneMapTexture.
        /// </summary>
        public override void Show()
        {
            // Only apply post screen glow if texture is valid and effect is valid 
            if (sceneMapTexture == null ||
                effect == null ||
                started == false)
                return;

            started = false;

            // Resolve sceneMapTexture render target for Xbox360 support
            sceneMapTexture.Resolve();

            // Don't use or write to the z buffer
            BaseGame.Device.DepthStencilState = DepthStencilState.None;
            // Also don't use any kind of blending.
            //Update: allow writing to alpha!
            BaseGame.Device.BlendState = BlendStateAlphaWrite;

            if (windowSize != null)
                windowSize.SetValue(
                    new float[] { sceneMapTexture.Width, sceneMapTexture.Height });
            if (sceneMap != null)
                sceneMap.SetValue(sceneMapTexture.XnaTexture);

            RadialBlurScaleFactor =
                // Warning: To big values will make the motion blur look to
                // stepy (we see each step and thats not good). -0.02 should be max.
                -(0.0025f + RacingGameManager.Player.Speed * 0.005f /
                Player.DefaultMaxSpeed);

            effect.CurrentTechnique = effect.Techniques["ScreenGlow20"];

            // We must have exactly 5 passes!
            if (effect.CurrentTechnique.Passes.Count != 5)
                throw new InvalidOperationException(
                    "This shader should have exactly 5 passes!");

            try
            {
                for (int pass = 0; pass < effect.CurrentTechnique.Passes.Count;
                    pass++)
                {
                    if (pass == 0)
                        radialSceneMapTexture.SetRenderTarget();
                    else if (pass == 1)
                        downsampleMapTexture.SetRenderTarget();
                    else if (pass == 2)
                        blurMap1Texture.SetRenderTarget();
                    else if (pass == 3)
                        blurMap2Texture.SetRenderTarget();
                    else
                    {
                        BaseGame.ResetRenderTarget(true);
                    }

                    EffectPass effectPass = effect.CurrentTechnique.Passes[pass];
                    effectPass.Apply();
                    // For first effect we use radial blur, draw it with a grid
                    // to get cooler results (more blur at borders than in middle).
                    if (pass == 0)
                        VBScreenHelper.Render10x10Grid();
                    else
                        VBScreenHelper.Render();

                    if (pass == 0)
                    {
                        radialSceneMapTexture.Resolve();
                        if (radialSceneMap != null)
                            radialSceneMap.SetValue(radialSceneMapTexture.XnaTexture);
                        effectPass.Apply();
                    }
                    else if (pass == 1)
                    {
                        downsampleMapTexture.Resolve();
                        if (downsampleMap != null)
                            downsampleMap.SetValue(downsampleMapTexture.XnaTexture);
                        effectPass.Apply();
                    }
                    else if (pass == 2)
                    {
                        blurMap1Texture.Resolve();
                        if (blurMap1 != null)
                            blurMap1.SetValue(blurMap1Texture.XnaTexture);
                        effectPass.Apply();
                    }
                    else if (pass == 3)
                    {
                        blurMap2Texture.Resolve();
                        if (blurMap2 != null)
                            blurMap2.SetValue(blurMap2Texture.XnaTexture);
                        effectPass.Apply();
                    }
                }
            }
            finally
            {
                // Restore z buffer state
                BaseGame.Device.DepthStencilState = DepthStencilState.Default;
            }
        }
        #endregion
    }
}
