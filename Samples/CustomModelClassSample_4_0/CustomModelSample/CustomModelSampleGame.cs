#region File Description
//-----------------------------------------------------------------------------
// CustomModelSampleGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace CustomModelSample
{
    /// <summary>
    /// Sample showing how to use a custom class to
    /// replace the built-in XNA Framework Model type.
    /// </summary>
    public class CustomModelSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        CustomModel model;

        Matrix world;
        Matrix view;
        Matrix projection;

        #endregion

        #region Initialization


        public CustomModelSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            model = Content.Load<CustomModel>("tank");

            // Calculate camera view and projection matrices.
            view = Matrix.CreateLookAt(new Vector3(1000, 500, 0),
                                       new Vector3(0, 150, 0), Vector3.Up);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                             GraphicsDevice.Viewport.AspectRatio, 10, 10000);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            // Update the world transform to make the model rotate.
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            world = Matrix.CreateRotationY(time * 0.1f);

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            model.Draw(world, view, projection);

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
            using (CustomModelSampleGame game = new CustomModelSampleGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
