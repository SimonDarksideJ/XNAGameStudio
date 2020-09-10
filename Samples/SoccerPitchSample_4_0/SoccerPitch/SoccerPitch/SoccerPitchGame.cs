#region File Description
//-----------------------------------------------------------------------------
// SoccerPitchGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using FrameRateCounterComponent;
#endregion

namespace SoccerPitch
{
    /// <summary>
    /// This sample demonstrates use of the DualTextured Effect, combined with multi-pass rendering
    /// to produce a detail textured sports field with white paint lines. 
    /// The white lines can be either alpha blended or drawn using alpha test.
    /// All of the geometry used here is procedurally created using a generic procedural primitive class.
    /// Custom vertex formats have been created for single and dual texture UV channels which 
    /// are computed at primitive creation time.
    /// </summary>
    public class SoccerPitchGame : Microsoft.Xna.Framework.Game
    {
        // 3D  CONSTANTS - Our Worlds near far clip is 150 units.
        const float FAR_CLIP = 150.0f;
        const float PLANE_SIZE = 100.0f;
        const float PLANE_TILING = 30.0f;
        const float SOCCERBALL_DIAMETER = 2.0f;
        const float SOCCERBALL_RADIUS = SOCCERBALL_DIAMETER * 0.5f;
        const float SOCCERBALL_DEPTH_OFFSET = 0.0001f;

        GraphicsDeviceManager     graphics;
        SpriteBatch               spriteBatch;
        SpriteFont                spriteFont;

        Color                     transparentWhite;
        bool                      useAlphaBlend = true;
        const string              alphaTestText = "Alpha-Test\n";
        const string              alphaBlendText = "Alpha-Blend\n";

        // Geometry for textured pitch - plane
        PlanePrimitiveDualTextured pitchPrimitive;
        PlanePrimitiveTextured pitchStripePrimitive;
        SpherePrimitiveTextured spherePrimitive;

        // Textures for the Multi-Texture Pitch
        Texture2D pitchBaseTexture, pitchDetailTexture, pitchStripeTexture, soccerballTexture;
        
        // Effects for Multi-Texture Pitch Rendering
        DualTextureEffect pitchDualTextureEffect;
        BasicEffect pitchBasicEffect;
        AlphaTestEffect pitchStripeEffect;

        RasterizerState shadowRasterizerState;        

        // Camera and shadows Vector / Matrix Storage         
        Matrix view;
        Vector3 eyeAtStart; // Initial position of the eye 
        Vector3 eyeAtBall; // Position of the eye when looking at the soccerball
        Vector3 camera; // current camera position which is computed by lerping between eye and eyeBall

        Matrix projection; // projection matrix for rendering
        Matrix shadowMatrix; // Flattening matrix for a Shadow

        // Touch input store
        TouchCollection currentTouches;

        public SoccerPitchGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            Components.Add(new FrameRateCounter(this)); 

            // Ensure the framework is not blocking progress with vsync or timing as this should run as fast as possible
            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

#if WINDOWS_PHONE
            graphics.IsFullScreen = true;
            Guide.IsScreenSaverEnabled = false;
#endif
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a SpriteBatch and Font for Text rendering
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Font");

            // Create a BasicEffect, which will be used to render the primitive.
            pitchBasicEffect = new BasicEffect(GraphicsDevice);
            pitchBasicEffect.LightingEnabled = false;
            pitchBasicEffect.PreferPerPixelLighting = false;
            pitchBasicEffect.FogEnabled = false;
            pitchBasicEffect.VertexColorEnabled = false;

            // With a tiling stripe texture, we can tile in any direction 
            Vector2 Tiling1 = new Vector2(PLANE_TILING/3.0f, PLANE_TILING/3.0f); 
            Vector2 Tiling2 = new Vector2( PLANE_TILING, PLANE_TILING );

            pitchPrimitive = new PlanePrimitiveDualTextured(GraphicsDevice, PLANE_SIZE, Tiling1, Tiling2 );
            pitchStripePrimitive = new PlanePrimitiveTextured(GraphicsDevice, PLANE_SIZE);
            pitchDualTextureEffect = new DualTextureEffect(GraphicsDevice);
            pitchStripeEffect = new AlphaTestEffect(GraphicsDevice);
            spherePrimitive = new SpherePrimitiveTextured(GraphicsDevice, SOCCERBALL_DIAMETER, 6);

            // Acquire the textures for the pitch
            // using the content manager (this.Content) to load game content here
            pitchBaseTexture = Content.Load<Texture2D>("Base");
            pitchDetailTexture = Content.Load<Texture2D>("Detail");
            pitchStripeTexture = Content.Load<Texture2D>("Stripe2");
            soccerballTexture = Content.Load<Texture2D>("Soccerball");

            // The eye and eyeBall views are constant (we just lerp between them) so set them up here.
            eyeAtStart = new Vector3(0.0f, PLANE_SIZE / 10.0f, -PLANE_SIZE * 0.75f);
            eyeAtBall = new Vector3(0.0f, 3*SOCCERBALL_DIAMETER, -SOCCERBALL_DIAMETER * 0.75f);

            // Setup Transparent White for blending 
            transparentWhite = Color.White;
            transparentWhite.A = 250; // 250 provides a pleasing additive blend.

            // Create a RasterizerState object for Shadow rendering with depth biasing
            shadowRasterizerState = new RasterizerState();
            shadowRasterizerState.DepthBias = -SOCCERBALL_DEPTH_OFFSET;

            // Initialize the projection matrix here as it never changes
            float aspect = GraphicsDevice.Viewport.AspectRatio;
            projection = Matrix.CreatePerspectiveFieldOfView(
                                (float)(System.Math.PI) / 4.0F, aspect, 2.0F, FAR_CLIP);

            // Clean up any load time generated garbage
            GC.Collect();
        }

        #region Handle Input
        /// <summary>
        /// Handles Touch inputs for changing settings.
        /// </summary>
        void HandleTouchInput()
        {
            // Process Touch Based Inputs
            currentTouches = TouchPanel.GetState();

            foreach (TouchLocation location in currentTouches)
            {
                if (location.State == TouchLocationState.Released)
                {
                    // Toggle Alpha Test / Alpha Blend
                    useAlphaBlend = !useAlphaBlend;                    
                }
            }
        }
        #endregion

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            HandleTouchInput();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Create camera matrices, making the object spin.
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            Matrix primitiveOrientation = Matrix.CreateRotationY(time * 0.2f);
            float t = Math.Max( 0.1f, (float)Math.Sin((double)time * 0.1f));
            Vector3.Lerp(ref eyeAtStart, ref eyeAtBall, t, out camera);

            view = Matrix.CreateLookAt(camera, Vector3.Zero, Vector3.Up);

            // Draw a Multi-Textured Pitch Material using DualTexturedEffect plus multipass for lines
            pitchDualTextureEffect.Texture = pitchBaseTexture;
            pitchDualTextureEffect.Texture2 = pitchDetailTexture;
            pitchDualTextureEffect.VertexColorEnabled = false;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            pitchPrimitive.DrawDualTextured(pitchDualTextureEffect, 
                                    primitiveOrientation, view, projection, Color.White);

            // 2nd Pass - Add white stripe
            if (useAlphaBlend)
            {
                // Use Additive Alpha Blend 
                pitchBasicEffect.Texture = pitchStripeTexture;
                pitchBasicEffect.TextureEnabled = true;
                pitchBasicEffect.LightingEnabled = false;
                pitchStripePrimitive.Draw(pitchBasicEffect, 
                                primitiveOrientation, view, projection, transparentWhite);
            }
            else
            {
                // Draw the striping / pitch lines using AlphaTestEffect
                pitchStripeEffect.Texture = pitchStripeTexture;
                pitchStripePrimitive.DrawAlphaTest(pitchStripeEffect, 
                                        primitiveOrientation, view, projection, Color.White);
            }

            // Render a flattened ball as a Shadow
            // Setup our shadow flattening matrix
            shadowMatrix = Matrix.Identity;
            shadowMatrix.M12 = 0.0f;
            shadowMatrix.M22 = 0.0f;
            shadowMatrix.M23 = 0.0f;

            shadowMatrix = primitiveOrientation * shadowMatrix;
            shadowMatrix.M42 = 0.0f; 
            pitchBasicEffect.TextureEnabled = false;
            pitchBasicEffect.LightingEnabled = false;
            RasterizerState oldRasterizerState = GraphicsDevice.RasterizerState;
            GraphicsDevice.RasterizerState = shadowRasterizerState;
            spherePrimitive.Draw(pitchBasicEffect, shadowMatrix, view, projection, Color.Black);
            GraphicsDevice.RasterizerState = oldRasterizerState;

            // Render the ball on top of the lot
            primitiveOrientation.M42 -= -SOCCERBALL_RADIUS;
            pitchBasicEffect.Texture = soccerballTexture;
            pitchBasicEffect.TextureEnabled = true;
            pitchBasicEffect.EnableDefaultLighting();
            spherePrimitive.Draw(pitchBasicEffect, primitiveOrientation, view, projection, Color.White);

            // Draw Rendering status text.
            spriteBatch.Begin();
            if (useAlphaBlend)
            {
                spriteBatch.DrawString(spriteFont, alphaBlendText, new Vector2(320, 70), Color.White);
            }
            else
            {
                spriteBatch.DrawString(spriteFont, alphaTestText, new Vector2(320, 70), Color.White);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
