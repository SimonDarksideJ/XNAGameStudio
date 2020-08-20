#region File Description
//-----------------------------------------------------------------------------
// ShadowMapping.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace ShadowMapping
{
    /// <summary>
    /// Sample showing how to implement a simple shadow mapping technique where
    /// the shadow map always contains the contents of the viewing frustum
    /// </summary>
    public class ShadowMappingGame : Microsoft.Xna.Framework.Game
    {
        #region Constants

        // The size of the shadow map
        // The larger the size the more detail we will have for our entire scene
        const int shadowMapWidthHeight = 2048;

        const int windowWidth = 800;
        const int windowHeight = 480;

        #endregion

        #region Fields

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Starting position and direction of our camera
        Vector3 cameraPosition = new Vector3(0, 70, 100);
        Vector3 cameraForward = new Vector3(0, -0.4472136f, -0.8944272f);
        BoundingFrustum cameraFrustum = new BoundingFrustum(Matrix.Identity);

        // Light direction
        Vector3 lightDir = new Vector3(-0.3333333f, 0.6666667f, 0.6666667f);

        KeyboardState currentKeyboardState;
        GamePadState currentGamePadState;

        // Our two models in the scene
        Model gridModel;
        Model dudeModel;

        float rotateDude = 0.0f;

        // The shadow map render target
        RenderTarget2D shadowRenderTarget;

        // Transform matrices
        Matrix world;
        Matrix view;
        Matrix projection;

        // ViewProjection matrix from the lights perspective
        Matrix lightViewProjection;

        #endregion

        #region Initialization

        public ShadowMappingGame()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;

            float aspectRatio = (float)windowWidth / (float)windowHeight;
  
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 
                                                             aspectRatio, 
                                                             1.0f, 1000.0f);
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the two models we will be using in the sample
            gridModel = Content.Load<Model>("grid");
            dudeModel = Content.Load<Model>("dude");

            // Create floating point render target
            shadowRenderTarget = new RenderTarget2D(graphics.GraphicsDevice,
                                                    shadowMapWidthHeight,
                                                    shadowMapWidthHeight,
                                                    false,
                                                    SurfaceFormat.Single,
                                                    DepthFormat.Depth24);
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);

            UpdateCamera(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Update the lights ViewProjection matrix based on the 
            // current camera frustum
            lightViewProjection = CreateLightViewProjectionMatrix();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Render the scene to the shadow map
            CreateShadowMap();

            // Draw the scene using the shadow map
            DrawWithShadowMap();

            // Display the shadow map to the screen
            DrawShadowMapToScreen();

            base.Draw(gameTime);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the WorldViewProjection matrix from the perspective of the 
        /// light using the cameras bounding frustum to determine what is visible 
        /// in the scene.
        /// </summary>
        /// <returns>The WorldViewProjection for the light</returns>
        Matrix CreateLightViewProjectionMatrix()
        {
            // Matrix with that will rotate in points the direction of the light
            Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero, 
                                                       -lightDir, 
                                                       Vector3.Up);

            // Get the corners of the frustum
            Vector3[] frustumCorners = cameraFrustum.GetCorners();

            // Transform the positions of the corners into the direction of the light
            for (int i = 0; i < frustumCorners.Length; i++)
            {
                frustumCorners[i] = Vector3.Transform(frustumCorners[i], lightRotation);
            }

            // Find the smallest box around the points
            BoundingBox lightBox = BoundingBox.CreateFromPoints(frustumCorners);

            Vector3 boxSize = lightBox.Max - lightBox.Min;
            Vector3 halfBoxSize = boxSize * 0.5f;

            // The position of the light should be in the center of the back
            // pannel of the box. 
            Vector3 lightPosition = lightBox.Min + halfBoxSize;
            lightPosition.Z = lightBox.Min.Z;

            // We need the position back in world coordinates so we transform 
            // the light position by the inverse of the lights rotation
            lightPosition = Vector3.Transform(lightPosition, 
                                              Matrix.Invert(lightRotation));

            // Create the view matrix for the light
            Matrix lightView = Matrix.CreateLookAt(lightPosition, 
                                                   lightPosition - lightDir, 
                                                   Vector3.Up);

            // Create the projection matrix for the light
            // The projection is orthographic since we are using a directional light
            Matrix lightProjection = Matrix.CreateOrthographic(boxSize.X, boxSize.Y, 
                                                               -boxSize.Z, boxSize.Z);

            return lightView * lightProjection;
        }

        /// <summary>
        /// Renders the scene to the floating point render target then 
        /// sets the texture for use when drawing the scene.
        /// </summary>
        void CreateShadowMap()
        {
            // Set our render target to our floating point render target
            GraphicsDevice.SetRenderTarget(shadowRenderTarget);

            // Clear the render target to white or all 1's
            // We set the clear to white since that represents the 
            // furthest the object could be away
            GraphicsDevice.Clear(Color.White);

            // Draw any occluders in our case that is just the dude model

            // Set the models world matrix so it will rotate
            world = Matrix.CreateRotationY(MathHelper.ToRadians(rotateDude));
            // Draw the dude model
            DrawModel(dudeModel, true);

            // Set render target back to the back buffer
            GraphicsDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Renders the scene using the shadow map to darken the shadow areas
        /// </summary>
        void DrawWithShadowMap()
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

            // Draw the grid
            world = Matrix.Identity;
            DrawModel(gridModel, false);

            // Draw the dude model
            world = Matrix.CreateRotationY(MathHelper.ToRadians(rotateDude));
            DrawModel(dudeModel, false);
        }

        /// <summary>
        /// Helper function to draw a model
        /// </summary>
        /// <param name="model">The model to draw</param>
        /// <param name="technique">The technique to use</param>
        void DrawModel(Model model, bool createShadowMap)
        {
            string techniqueName = createShadowMap ? "CreateShadowMap" : "DrawWithShadowMap";

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            // Loop over meshs in the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                // Loop over effects in the mesh
                foreach (Effect effect in mesh.Effects)
                {
                    // Set the currest values for the effect
                    effect.CurrentTechnique = effect.Techniques[techniqueName];
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["LightDirection"].SetValue(lightDir);
                    effect.Parameters["LightViewProj"].SetValue(lightViewProjection);

                    if (!createShadowMap)
                        effect.Parameters["ShadowMap"].SetValue(shadowRenderTarget);
                }
                // Draw the mesh
                mesh.Draw();
            }
        }

        /// <summary>
        /// Render the shadow map texture to the screen
        /// </summary>
        void DrawShadowMapToScreen()
        {
            spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(shadowRenderTarget, new Rectangle(0, 0, 128, 128), Color.White);
            spriteBatch.End();

            GraphicsDevice.Textures[0] = null;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        void HandleInput(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Rotate the dude model
            rotateDude += currentGamePadState.Triggers.Right * time * 0.2f;
            rotateDude -= currentGamePadState.Triggers.Left * time * 0.2f;
            
            if (currentKeyboardState.IsKeyDown(Keys.Q))
                rotateDude -= time * 0.2f;
            if (currentKeyboardState.IsKeyDown(Keys.E))
                rotateDude += time * 0.2f;

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }
        }


        /// <summary>
        /// Handles input for moving the camera.
        /// </summary>
        void UpdateCamera(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check for input to rotate the camera.
            float pitch = -currentGamePadState.ThumbSticks.Right.Y * time * 0.001f;
            float turn = -currentGamePadState.ThumbSticks.Right.X * time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.Up))
                pitch += time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.Down))
                pitch -= time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.Left))
                turn += time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.Right))
                turn -= time * 0.001f;

            Vector3 cameraRight = Vector3.Cross(Vector3.Up, cameraForward);
            Vector3 flatFront = Vector3.Cross(cameraRight, Vector3.Up);

            Matrix pitchMatrix = Matrix.CreateFromAxisAngle(cameraRight, pitch);
            Matrix turnMatrix = Matrix.CreateFromAxisAngle(Vector3.Up, turn);

            Vector3 tiltedFront = Vector3.TransformNormal(cameraForward, pitchMatrix *
                                                          turnMatrix);

            // Check angle so we cant flip over
            if (Vector3.Dot(tiltedFront, flatFront) > 0.001f)
            {
                cameraForward = Vector3.Normalize(tiltedFront);
            }

            // Check for input to move the camera around.
            if (currentKeyboardState.IsKeyDown(Keys.W))
                cameraPosition += cameraForward * time * 0.1f;

            if (currentKeyboardState.IsKeyDown(Keys.S))
                cameraPosition -= cameraForward * time * 0.1f;

            if (currentKeyboardState.IsKeyDown(Keys.A))
                cameraPosition += cameraRight * time * 0.1f;

            if (currentKeyboardState.IsKeyDown(Keys.D))
                cameraPosition -= cameraRight * time * 0.1f;

            cameraPosition += cameraForward *
                              currentGamePadState.ThumbSticks.Left.Y * time * 0.1f;

            cameraPosition -= cameraRight *
                              currentGamePadState.ThumbSticks.Left.X * time * 0.1f;

            if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.R))
            {
                cameraPosition = new Vector3(0, 50, 50);
                cameraForward = new Vector3(0, 0, -1);
            }

            cameraForward.Normalize();

            // Create the new view matrix
            view = Matrix.CreateLookAt(cameraPosition, 
                                       cameraPosition + cameraForward, 
                                       Vector3.Up);

            // Set the new frustum value
            cameraFrustum.Matrix = view * projection;
        }

        #endregion
    }

    #region Entry Point

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            using (ShadowMappingGame game = new ShadowMappingGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
