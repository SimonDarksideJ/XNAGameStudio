#region File Description
//-----------------------------------------------------------------------------
// LoadingAndInstructionScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GameStateManagement;
using System.Threading;
#endregion

namespace MarbleMazeGame
{
    class LoadingAndInstructionScreen : GameScreen
    {
        #region Fields
        Texture2D background;
        SpriteFont font;
        bool isLoading;
        GameplayScreen gameplayScreen;
        Thread thread;
        #endregion

        #region Initialization
        public LoadingAndInstructionScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            EnabledGestures = Microsoft.Xna.Framework.Input.Touch.GestureType.Tap;
        }
        #endregion

        #region Loading
        /// <summary>
        /// Load the screen resources
        /// </summary>
        public override void LoadContent()
        {
            background = Load<Texture2D>(@"Textures\instructions");
            font = Load<SpriteFont>(@"Fonts\MenuFont");

            // Create a new instance of the gameplay screen
            gameplayScreen = new GameplayScreen();
            gameplayScreen.ScreenManager = ScreenManager;
        }
        #endregion

        #region Update
        /// <summary>
        /// Exit the screen after a tap gesture
        /// </summary>
        /// <param name="input"></param>
        public override void HandleInput(InputState input)
        {
            if (!isLoading)
            {
                if (input.Gestures.Count > 0)
                {
                    if (input.Gestures[0].GestureType == 
                        Microsoft.Xna.Framework.Input.Touch.GestureType.Tap)
                    {
                        // Start loading the resources in an additional thread
                        thread = new Thread(
                            new ThreadStart(gameplayScreen.LoadAssets));

                        isLoading = true;
                        thread.Start();
                    }
                }
            }
            base.HandleInput(input);
        }

        /// <summary>
        /// Screen update logic
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // If additional thread is running, do nothing
            if (null != thread)
            {
                // If additional thread finished loading and the screen is not exiting
                if (thread.ThreadState == ThreadState.Stopped && !IsExiting)
                {
                    // Move on to the gameplay screen
                    foreach (GameScreen screen in ScreenManager.GetScreens())
                        screen.ExitScreen();

                    ScreenManager.AddScreen(gameplayScreen, null);
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
        #endregion

        #region Render
        /// <summary>
        /// Render screen 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw Background
            spriteBatch.Draw(background, new Vector2(0, 0),
                 Color.White * TransitionAlpha);

            // If loading gameplay screen resource in the 
            // background show "Loading..." text
            if (isLoading)
            {
                string text = "Loading...";
                Vector2 size = font.MeasureString(text);
                Vector2 position = new Vector2(
                    (ScreenManager.GraphicsDevice.Viewport.Width - size.X) / 2,
                    (ScreenManager.GraphicsDevice.Viewport.Height - size.Y) / 2);
                spriteBatch.DrawString(font, text, position, Color.White);
            }

            spriteBatch.End();
        }
        #endregion
    }
}
