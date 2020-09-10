#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// The pause menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    /// <remarks>
    /// This class is somewhat similar to one of the same name in the 
    /// GameStateManagement sample.
    /// </remarks>
    class PauseMenuScreen : MenuScreen
    {
        #region Fields
        Texture2D titleTexture;
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public PauseMenuScreen()
        {
            MenuEntry resumeGameMenuEntry = new MenuEntry("Resume Game");
            MenuEntry quitGameEntry = new MenuEntry("Quit Game");

            resumeGameMenuEntry.Selected += ResumeGameMenuEntrySelected;
            quitGameEntry.Selected += QuitGameEntrySelected;

            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(quitGameEntry);

            // Flag that there is no need for the game to transition
            // off when the pause menu is on top of it.
            IsPopup = true;
        }

        public override void LoadContent()
        {
            titleTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/title");
            base.LoadContent();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// When the user cancels the pause menu, resume the game.
        /// </summary>
        protected override void OnCancel()
        {
            ExitScreen();
        }


        /// <summary>
        /// Event handler for when the Resume Game menu entry is selected.
        /// </summary>
        void ResumeGameMenuEntrySelected(object sender, EventArgs e)
        {
            ExitScreen();
        }


        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameEntrySelected(object sender, EventArgs e)
        {
            // Quit the game, after a confirmation message box.
            const string message = "Are you sure?";

            MessageBoxScreen messageBox = new MessageBoxScreen(message);
            messageBox.Accepted += QuitMessageBoxAccepted;
            ScreenManager.AddScreen(messageBox);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void QuitMessageBoxAccepted(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, LoadMainMenuScreen, false);
        }


        /// <summary>
        /// Loading screen callback for activating the main menu screen,
        /// used when quitting from the game.
        /// </summary>
        void LoadMainMenuScreen(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new BackgroundScreen());
            ScreenManager.AddScreen(new MainMenuScreen());
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the pause menu screen. This darkens down the gameplay screen
        /// that is underneath us, and then chains to the base MenuScreen.Draw.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);

            // title
            Vector2 titlePosition = new Vector2(
                (viewportSize.X - titleTexture.Width) / 2f,
                viewportSize.Y * 0.18f);
            titlePosition.Y -= (float)Math.Pow(TransitionPosition, 2) * titlePosition.Y;
            Color titleColor = Color.White;

            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(titleTexture, titlePosition,
                new Color(titleColor.R, titleColor.G, titleColor.B, TransitionAlpha));
            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion
    }
}
