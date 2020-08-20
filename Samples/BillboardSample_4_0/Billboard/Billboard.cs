#region File Description
//-----------------------------------------------------------------------------
// Billboard.cs
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
using Microsoft.Xna.Framework.Input;
#endregion

namespace Billboard
{
    /// <summary>
    /// Sample showing how to efficiently render billboard sprites.
    /// </summary>
    public class BillboardGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        
        Vector3 cameraPosition = new Vector3(0, 50, 50);
        Vector3 cameraFront = new Vector3(0, 0, -1);

        Model landscape;

        #endregion

        #region Initialization


        public BillboardGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            landscape = Content.Load<Model>("landscape");
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
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.CornflowerBlue);

            // Compute camera matrices.
            Matrix view = Matrix.CreateLookAt(cameraPosition,
                                              cameraPosition + cameraFront,
                                              Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    device.Viewport.AspectRatio,
                                                                    1, 10000);

            Vector3 lightDirection = Vector3.Normalize(new Vector3(3, -1, 1));
            Vector3 lightColor = new Vector3(0.3f, 0.4f, 0.2f);

            // Time is scaled down to make things wave in the wind more slowly.
            float time = (float)gameTime.TotalGameTime.TotalSeconds * 0.333f;

            // First we draw the ground geometry using BasicEffect.
            foreach (ModelMesh mesh in landscape.Meshes)
            {
                if (mesh.Name != "Billboards")
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.View = view;
                        effect.Projection = projection;

                        effect.LightingEnabled = true;

                        effect.DirectionalLight0.Enabled = true;
                        effect.DirectionalLight0.Direction = lightDirection;
                        effect.DirectionalLight0.DiffuseColor = lightColor;

                        effect.AmbientLightColor = new Vector3(0.1f, 0.2f, 0.1f);
                    }

                    device.BlendState = BlendState.Opaque;
                    device.DepthStencilState = DepthStencilState.Default;
                    device.RasterizerState = RasterizerState.CullCounterClockwise;

                    mesh.Draw();
                }
            }

            // Then we use a two-pass technique to render alpha blended billboards with
            // almost-correct depth sorting. The only way to make blending truly proper for
            // alpha objects is to draw everything in sorted order, but manually sorting all
            // our billboards would be very expensive. Instead, we draw in two passes.
            //
            // The first pass has alpha blending turned off, alpha testing set to only accept
            // ~95% or more opaque pixels, and the depth buffer turned on. Because this is only
            // rendering the solid parts of each billboard, the depth buffer works as
            // normal to give correct sorting, but obviously only part of each billboard will
            // be rendered.
            //
            // Then in the second pass we enable alpha blending, set alpha test to only accept
            // pixels with fractional alpha values, and set the depth buffer to test against
            // the existing data but not to write new depth values. This means the translucent
            // areas of each billboard will be sorted correctly against the depth buffer
            // information that was previously written while drawing the opaque parts, although
            // there can still be sorting errors between the translucent areas of different
            // billboards.
            //
            // In practice, sorting errors between translucent pixels tend not to be too
            // noticable as long as the opaque pixels are sorted correctly, so this technique
            // often looks ok, and is much faster than trying to sort everything 100%
            // correctly. It is particularly effective for organic textures like grass and
            // trees.
            foreach (ModelMesh mesh in landscape.Meshes)
            {
                if (mesh.Name == "Billboards")
                {
                    // First pass renders opaque pixels.
                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["LightDirection"].SetValue(lightDirection);
                        effect.Parameters["WindTime"].SetValue(time);
                        effect.Parameters["AlphaTestDirection"].SetValue(1f);
                    }

                    device.BlendState = BlendState.Opaque;
                    device.DepthStencilState = DepthStencilState.Default;
                    device.RasterizerState = RasterizerState.CullNone;
                    device.SamplerStates[0] = SamplerState.LinearClamp;

                    mesh.Draw();

                    // Second pass renders the alpha blended fringe pixels.
                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.Parameters["AlphaTestDirection"].SetValue(-1f);
                    }

                    device.BlendState = BlendState.NonPremultiplied;
                    device.DepthStencilState = DepthStencilState.DepthRead;

                    mesh.Draw();
                }
            }

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

            // Check angle so we cant flip over
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
                cameraPosition = new Vector3(0, 50, 50);
                cameraFront = new Vector3(0, 0, -1);
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
            using (BillboardGame game = new BillboardGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
