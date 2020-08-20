#region File Description
//-----------------------------------------------------------------------------
// SimpleAnimation.cs
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

namespace SimpleAnimation
{
    /// <summary>
    /// Sample showing how to apply simple animation to a rigid body tank model.
    /// </summary>
    public class SimpleAnimationGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        Tank tank;

        #endregion

        #region Initialization


        public SimpleAnimationGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.IsFullScreen = true;
#endif
            
            tank = new Tank();
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            tank.Load(Content);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            // Update the animation properties on the tank object. In a real game
            // you would probably take this data from user inputs or the physics
            // system, rather than just making everything rotate like this!

            tank.WheelRotation = time * 5;
            tank.SteerRotation = (float)Math.Sin(time * 0.75f) * 0.5f;
            tank.TurretRotation = (float)Math.Sin(time * 0.333f) * 1.25f;
            tank.CannonRotation = (float)Math.Sin(time * 0.25f) * 0.333f - 0.333f;
            tank.HatchRotation = MathHelper.Clamp((float)Math.Sin(time * 2) * 2, -1, 0);

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.DarkGray);

            // Calculate the camera matrices.
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            Matrix rotation = Matrix.CreateRotationY(time * 0.1f);

            Matrix view = Matrix.CreateLookAt(new Vector3(1000, 500, 0),
                                              new Vector3(0, 150, 0),
                                              Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 
                                                                    device.Viewport.AspectRatio,
                                                                    10, 
                                                                    10000);

            // Draw the tank model.
            tank.Draw(rotation, view, projection);

            base.Draw(gameTime);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        private void HandleInput()
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
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
            using (SimpleAnimationGame game = new SimpleAnimationGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
