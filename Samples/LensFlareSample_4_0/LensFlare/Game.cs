#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace LensFlare
{
    /// <summary>
    /// Sample showing how to implement a lensflare effect, using occlusion
    /// queries to hide the flares when the sun is hidden behind the landscape.
    /// </summary>
    public class LensFlareGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        
        Vector3 cameraPosition = new Vector3(-200, 30, 30);
        Vector3 cameraFront = new Vector3(1, 0, 0);

        Model terrain;

        LensFlareComponent lensFlare;

        #endregion

        #region Initialization


        public LensFlareGame()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";

            // Create and add the lensflare component.
            lensFlare = new LensFlareComponent(this);

            Components.Add(lensFlare);
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            terrain = Content.Load<Model>("terrain");
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            UpdateCamera(gameTime);

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Compute camera matrices.
            Matrix view = Matrix.CreateLookAt(cameraPosition,
                                              cameraPosition + cameraFront,
                                              Vector3.Up);

            float aspectRatio = GraphicsDevice.Viewport.AspectRatio;

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    aspectRatio,
                                                                    0.1f, 500);

            // Draw the terrain.
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (ModelMesh mesh in terrain.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.Identity;
                    effect.View = view;
                    effect.Projection = projection;

                    effect.LightingEnabled = true;
                    effect.DiffuseColor = new Vector3(1f);
                    effect.AmbientLightColor = new Vector3(0.5f);
                    
                    effect.DirectionalLight0.Enabled = true;
                    effect.DirectionalLight0.DiffuseColor = Vector3.One;
                    effect.DirectionalLight0.Direction = lensFlare.LightDirection;

                    effect.FogEnabled = true;
                    effect.FogStart = 200;
                    effect.FogEnd = 500;
                    effect.FogColor = Color.CornflowerBlue.ToVector3();
                }

                mesh.Draw();
            }

            // Tell the lensflare component where our camera is positioned.
            lensFlare.View = view;
            lensFlare.Projection = projection;

            base.Draw(gameTime);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        private void HandleInput()
        {
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }
        }


        /// <summary>
        /// Handles camera input.
        /// </summary>
        private void UpdateCamera(GameTime gameTime)
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

            Vector3 cameraRight = Vector3.Cross(Vector3.Up, cameraFront);
            Vector3 flatFront = Vector3.Cross(cameraRight, Vector3.Up);

            Matrix pitchMatrix = Matrix.CreateFromAxisAngle(cameraRight, pitch);
            Matrix turnMatrix = Matrix.CreateFromAxisAngle(Vector3.Up, turn);

            Vector3 tiltedFront = Vector3.TransformNormal(cameraFront, pitchMatrix * 
                                                          turnMatrix);

            // Check angle so we can't flip over.
            if (Vector3.Dot(tiltedFront, flatFront) > 0.001f)
            {
                cameraFront = Vector3.Normalize(tiltedFront);
            }

            // Check for input to move the camera around.
            if (currentKeyboardState.IsKeyDown(Keys.W))
                cameraPosition += cameraFront * time * 0.1f;
            
            if (currentKeyboardState.IsKeyDown(Keys.S))
                cameraPosition -= cameraFront * time * 0.1f;

            if (currentKeyboardState.IsKeyDown(Keys.A))
                cameraPosition += cameraRight * time * 0.1f;

            if (currentKeyboardState.IsKeyDown(Keys.D))
                cameraPosition -= cameraRight * time * 0.1f;

            cameraPosition += cameraFront *
                              currentGamePadState.ThumbSticks.Left.Y * time * 0.1f;

            cameraPosition -= cameraRight *
                              currentGamePadState.ThumbSticks.Left.X * time * 0.1f;

            if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.R))
            {
                cameraPosition = new Vector3(-200, 30, 30);
                cameraFront = new Vector3(1, 0, 0);
            }
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (LensFlareGame game = new LensFlareGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
