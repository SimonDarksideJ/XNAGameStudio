#region File Description
//-----------------------------------------------------------------------------
// DistortionComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
#endregion

namespace DistortionSample
{
    public class DistortionComponent : DrawableGameComponent
    {
        #region Enumerations
        public enum DistortionTechnique
        {
            DisplacementMapped,
            HeatHaze,
            PullIn,
            ZeroDisplacement,
        }
        #endregion

        #region Constant Data
        private const float blurAmount = 2f;

        private readonly static string[] distortionTechniqueFriendlyNames = new string[]
        {
            "Displacement-Mapped",
            "Heat-Haze",
            "Pull-In",
            "Zero Displacement",
        };

        public static string GetDistortionTechniqueFriendlyName(
            DistortionTechnique technique)
        {
            return distortionTechniqueFriendlyNames[(int)technique];
        }
        #endregion

        #region Fields
        SpriteBatch spriteBatch;
        RenderTarget2D sceneMap;
        RenderTarget2D distortionMap;

        Effect distortEffect;
        EffectTechnique distortTechnique;
        EffectTechnique distortBlurTechnique;

        public Matrix View;
        public Matrix Projection;

        public Distorter Distorter;
        public bool ShowDistortionMap = false;
        #endregion

        #region Initialization
        public DistortionComponent(Game game)
            : base(game) { }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            distortEffect = Game.Content.Load<Effect>("Distort");
            distortTechnique = distortEffect.Techniques["Distort"];
            distortBlurTechnique = distortEffect.Techniques["DistortBlur"];

            // update the projection matrix
            Projection = Matrix.CreatePerspectiveFieldOfView(1f, GraphicsDevice.Viewport.AspectRatio, 1f, 10000f);

            // look up the resolution and format of our main backbuffer
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;
            SurfaceFormat format = pp.BackBufferFormat;
            DepthFormat depthFormat = pp.DepthStencilFormat;

            // create textures for reading back the backbuffer contents
            sceneMap = new RenderTarget2D(GraphicsDevice, width, height, false, format, depthFormat);
            distortionMap = new RenderTarget2D(GraphicsDevice, width, height, false, format, depthFormat);

            // set the blur parameters for the current viewport
            SetBlurEffectParameters(1f / (float)width, 1f / (float)height);
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            spriteBatch = null;
            distortEffect = null;
            distortTechnique = null;
            distortBlurTechnique = null;

            if (sceneMap != null)
            {
                sceneMap.Dispose();
                sceneMap = null;
            }
            if (distortionMap != null)
            {
                distortionMap.Dispose();
                distortionMap = null;
            }
        }
        #endregion

        #region Draw

        /// <summary>
        /// This should be called at the very start of scene rendering. The distortion
        /// component uses it to redirect drawing into its custom rendertarget, so it
        /// can capture the scene image ready to apply the distortion postprocess.
        /// </summary>
        public void BeginDraw()
        {
            if (Visible)
            {
                GraphicsDevice.SetRenderTarget(sceneMap);
            }
        }
        
        /// <summary>
        /// Grab a scene that has already been rendered, 
        /// and add a distortion effect over the top of it.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // now draw the distortion map
            GraphicsDevice.SetRenderTarget(ShowDistortionMap ? null : distortionMap);
            GraphicsDevice.Clear(Color.Transparent);

            // draw the distorter
            if (Distorter != null)
            {
                Matrix worldView = Distorter.World * View;
                Matrix[] transforms = new Matrix[Distorter.Model.Bones.Count];
                Distorter.Model.CopyAbsoluteBoneTransformsTo(transforms);

                // make sure the depth buffering is on, so only parts of the scene
                // behind the distortion effect are affected
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                foreach (ModelMesh mesh in Distorter.Model.Meshes)
                {
                    Matrix meshWorldView = transforms[mesh.ParentBone.Index] * 
                        worldView;                    
                    foreach (Effect effect in mesh.Effects)
                    {                        
                        effect.CurrentTechnique = 
                            effect.Techniques[Distorter.Technique.ToString()];
                        effect.Parameters["WorldView"].SetValue(meshWorldView);
                        effect.Parameters["WorldViewProjection"].SetValue(
                            meshWorldView * Projection);
                        effect.Parameters["DistortionScale"].SetValue(
                            Distorter.DistortionScale);
                        effect.Parameters["Time"].SetValue(
                            (float)gameTime.TotalGameTime.TotalSeconds);
                    }
                    mesh.Draw();
                }
            }

            // if we want to show the distortion map, then the backbuffer is done.
            // if we want to render the scene distorted, then we need to resolve the
            // backbuffer as the distortion map and use it to distort the scene
            if (!ShowDistortionMap)
            {
                GraphicsDevice.SetRenderTarget(null);

                // draw the scene image again, distorting it with the distortion map
                GraphicsDevice.Textures[1] = distortionMap;
                GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
                Viewport viewport = GraphicsDevice.Viewport;
                distortEffect.CurrentTechnique = Distorter.DistortionBlur ?
                    distortBlurTechnique : distortTechnique;
                DrawFullscreenQuad(sceneMap, viewport.Width, viewport.Height,
                    distortEffect);
            }
        } 

        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFullscreenQuad(Texture2D texture, int width, int height, Effect effect)
        {
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();
        }
        #endregion

        #region Blur Calculation
        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        /// <remarks>
        /// This function was originally provided in the BloomComponent class in the 
        /// Bloom Postprocess sample.
        /// </remarks>
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = distortEffect.Parameters["SampleWeights"];
            offsetsParameter = distortEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }

        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        /// <remarks>
        /// This function was originally provided in the BloomComponent class in the 
        /// Bloom Postprocess sample.
        /// </remarks>
        static float ComputeGaussian(float n)
        {
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * blurAmount)) *
                           Math.Exp(-(n * n) / (2 * blurAmount * blurAmount)));
        }
        #endregion
    }
}
