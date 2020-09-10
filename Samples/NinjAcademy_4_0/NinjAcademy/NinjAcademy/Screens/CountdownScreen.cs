#region File Description
//-----------------------------------------------------------------------------
// CountdownScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


#endregion

namespace NinjAcademy
{
    /// <summary>
    /// Displayed before the game actually begins to present the user with a countdown. Once the countdown is over,
    /// the screen will exit.
    /// </summary>
    /// <remarks>Using a separate screen for the countdown allows us to remove unnecessary checks from the
    /// actual gameplay screen.</remarks>
    class CountdownScreen : GameScreen
    {
        #region Fields and Properties


        bool isUpdating = true;

        Texture2D backgroundTexture;
        Texture2D roomTexture;

        SpriteFont countdownFont;

        Rectangle viewport;
        Vector2 screenCenter;

        TimeSpan countdownInterval = TimeSpan.FromSeconds(1);
        int countdownValue = 3;
        TimeSpan intervalTimer = TimeSpan.Zero;

        GameplayScreen gameplayScreen;


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new countdown screen instance, specifying the gameplay screen to appear once countdown is over.
        /// </summary>
        /// <param name="gameplayScreen">The gameplay screen which will appear once countdown is over.</param>
        public CountdownScreen(GameplayScreen gameplayScreen)
            : base()
        {
            this.gameplayScreen = gameplayScreen;
        }

        /// <summary>
        /// Loads assets which are required by this screen.
        /// </summary>
        public override void LoadContent()
        {
            // Assuming we
            backgroundTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/Backgrounds/gameplayBG");
            roomTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/Backgrounds/room");

            countdownFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameScreenFont36px");

            viewport = ScreenManager.GraphicsDevice.Viewport.Bounds;
            screenCenter = viewport.Center.GetVector();

            base.LoadContent();
        }


        #endregion

        #region Update and Render


        /// <summary>
        /// Keeps track of the countdown's progress, exiting the screen when the countdown is over.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="otherScreenHasFocus">Whether this screen currently has the focus.</param>
        /// <param name="coveredByOtherScreen">Whether this screen is covered by another screen.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (!isUpdating)
            {
                return;
            }

            intervalTimer += gameTime.ElapsedGameTime;

            if (intervalTimer >= countdownInterval)
            {
                intervalTimer = TimeSpan.Zero;
                countdownValue--;
            }

            if (countdownValue <= 0)
            {
                gameplayScreen.PreDisplayInitialization();
                ExitScreen();
            }
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);

            if (input.IsPauseGame(null))
            {
                PauseCountdown();

                ScreenManager.AddScreen(new BackgroundScreen("titlescreenBG"), null);
                ScreenManager.AddScreen(new PauseScreen(this), null);
            }
        }

        /// <summary>
        /// Renders the background and countdown progress.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            string countdownString = countdownValue.ToString();
            Vector2 stringSize = countdownFont.MeasureString(countdownString);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, viewport, Color.White);
            spriteBatch.Draw(roomTexture, viewport, Color.White);
            spriteBatch.DrawString(countdownFont, countdownString, screenCenter - (stringSize / 2),
                Color.White);

            spriteBatch.End();
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Pauses the countdown operation.
        /// </summary>
        public void PauseCountdown()
        {
            isUpdating = false;
        }

        /// <summary>
        /// Resumes the countdown operation.
        /// </summary>
        public void ResumeCountdown()
        {
            isUpdating = true;
        }


        #endregion
    }
}
