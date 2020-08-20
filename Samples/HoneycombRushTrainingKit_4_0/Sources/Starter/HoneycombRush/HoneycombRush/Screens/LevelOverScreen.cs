#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System.Threading;


#endregion

namespace HoneycombRush
{
    class LevelOverScreen : GameScreen
    {
        #region Fields


        SpriteFont font36px;
        SpriteFont font16px;

        string text;
        bool isLoading;
        Vector2 textSize;

        DifficultyMode? difficultyMode;

        Thread thread;
        GameplayScreen gameplayScreen;


        #endregion

        #region Initialization


        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="difficultyMode">The next level</param>
        public LevelOverScreen(string text, DifficultyMode? difficultyMode)
        {
            this.text = text;
            EnabledGestures = GestureType.Tap;
            this.difficultyMode = difficultyMode;
        }

        /// <summary>
        /// Load screen resources
        /// </summary>
        public override void LoadContent()
        {
            if (difficultyMode.HasValue)
            {
                gameplayScreen = new GameplayScreen(difficultyMode.Value);
                gameplayScreen.ScreenManager = ScreenManager;
            }
            font36px = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameScreenFont36px");
            font16px = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameScreenFont16px");
            textSize = font36px.MeasureString(text);

            base.LoadContent();
        }


        #endregion

        #region Update


        /// <summary>
        /// Update the screen
        /// </summary>
        /// <param name="gameTime">The game time</param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // If null don't do anything
            if (null != thread)
            {
                // If we finishedloading the assets, add the game play screen
                if (thread.ThreadState == ThreadState.Stopped)
                {
                    // Exit all the screen
                    foreach (GameScreen screen in ScreenManager.GetScreens())
                        screen.ExitScreen();

                    // Add the gameplay screen
                    if (difficultyMode.HasValue)
                    {
                        ScreenManager.AddScreen(gameplayScreen, null);
                    }
                }
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Handle any input from the user
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="input"></param>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Return to the main menu when a tap gesture is recognized
            if (input.Gestures.Count > 0)
            {
                GestureSample sample = input.Gestures[0];
                if (sample.GestureType == GestureType.Tap)
                {
                    StartNewLevelOrExit(input);
                    input.Gestures.Clear();
                }
            }
            base.HandleInput(gameTime, input);
        }


        #endregion

        #region Render


        /// <summary>
        /// Renders the screen
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw the footer text

            if (difficultyMode.HasValue)
            {
                string tapText = "Touch to start next level";
                spriteBatch.DrawString(font16px, tapText,
                new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - font16px.MeasureString(tapText).X / 2,
                                   ScreenManager.GraphicsDevice.Viewport.Height -
                                   font16px.MeasureString(tapText).Y - 4), Color.Black);
            }
            else
            {

                string tapText = "Touch to end game";
                spriteBatch.DrawString(font16px, tapText,
                new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - font16px.MeasureString(tapText).X / 2,
                                   ScreenManager.GraphicsDevice.Viewport.Height -
                                   font16px.MeasureString(tapText).Y - 4), Color.Black);
            }

            spriteBatch.End();
        }


        #endregion

        #region Private Function


        /// <summary>
        /// Starts new level or exit to High Score
        /// </summary>
        /// <param name="input"></param>
        private void StartNewLevelOrExit(InputState input)
        {
            // If there is no next level - go to high score screen
            if (!difficultyMode.HasValue)
            {
                ScreenManager.AddScreen(new BackgroundScreen("highScoreScreen"), null);
                ScreenManager.AddScreen(new HighScoreScreen(), null);
            }
            // If not already loading
            else if (!isLoading)
            {
                // Start loading the resources in an additional thread
                thread = new Thread(new ThreadStart(gameplayScreen.LoadAssets));

                isLoading = true;
                thread.Start();
            }
        }


        #endregion
    }
}
