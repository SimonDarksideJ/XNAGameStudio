#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Phone.Shell;


#endregion

namespace NinjAcademy
{
    enum ElementState
    {
        Invisible,
        Appearing,
        Visible
    }

    class MainMenuScreen : MenuScreen
    {
        #region Fields


        bool isExiting = false;
        bool isMovingToLoading = false;

        float maxEntryWidth;

        TimeSpan ninjaAppearDelay = TimeSpan.FromSeconds(3);
        TimeSpan titleAppearDelay = TimeSpan.FromSeconds(2);
        TimeSpan ninjaAppearDuration = TimeSpan.FromMilliseconds(500);
        TimeSpan titleAppearDuration = TimeSpan.FromMilliseconds(500);

        TimeSpan ninjaTimer = TimeSpan.Zero;
        TimeSpan titleTimer = TimeSpan.Zero;

        ElementState ninjaState = ElementState.Invisible;
        ElementState titleState = ElementState.Invisible;

        Texture2D instructionsTexture;
        Texture2D loadingTexture;

        Texture2D ninjaTexture;
        Texture2D titleTexture;

        Vector2 ninjaPosition = new Vector2(30, 40);
        Vector2 titlePosition = new Vector2(265, 20);

        Vector2 ninjaInitialOffset = new Vector2(-400, 50);
        Vector2 titleInitialOffset = new Vector2(0, -280);

        Vector2 ninjaOffset;
        Vector2 titleOffset; 


        #endregion

        #region Initializations


        /// <summary>
        /// Creates a new menu screen instance.
        /// </summary>
        public MainMenuScreen()
            : base("")
        {
            // Create our menu entries.
            MenuEntry startGameMenuEntry = new MenuEntry("Start");
            MenuEntry highScoreMenuEntry = new MenuEntry("High Score");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            startGameMenuEntry.Selected += StartSelected;
            highScoreMenuEntry.Selected += HighScoreSelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(startGameMenuEntry);
            MenuEntries.Add(highScoreMenuEntry);
            MenuEntries.Add(exitMenuEntry);

            ninjaOffset = ninjaInitialOffset;
            titleOffset = titleInitialOffset;
        }

        /// <summary>
        /// Arranges all menu at the lower right side of the screen.
        /// </summary>
        protected override void UpdateMenuEntryLocations()
        {
            int menuEntryHorizontalPlacement;
            int menuEntryVerticalPlacement = GameConstants.MainMenuTop;

            // update each menu entry's location in turn
            for (int i = 0; i < MenuEntries.Count; i++)
            {
                float entryWidth = ScreenManager.Font.MeasureString(MenuEntries[i].Text).X;
                menuEntryHorizontalPlacement = (int)(GameConstants.MainMenuLeft + maxEntryWidth / 2 - entryWidth / 2);
                MenuEntries[i].Position = new Vector2(menuEntryHorizontalPlacement, menuEntryVerticalPlacement);
                menuEntryVerticalPlacement += GameConstants.MainMenuEntryGap;
            }
        }

        public override void LoadContent()
        {
            instructionsTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/Backgrounds/Instructions");
            loadingTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/Backgrounds/loading");

            ninjaTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/ninja");
            titleTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/title");

            // If necessary, load the high-score data
            if (!HighScoreScreen.HighscoreLoaded)
            {
                HighScoreScreen.LoadHighscores();
            }

            AudioManager.LoadSounds();
            AudioManager.LoadMusic();            

            // We want to find the widest menu entry
            for (int i = 0; i < MenuEntries.Count; i++)
            {
                float entryWidth = ScreenManager.Font.MeasureString(MenuEntries[i].Text).X;
                if (maxEntryWidth < entryWidth)
                {
                    maxEntryWidth = entryWidth;
                }
            }

            base.LoadContent();
        }


        #endregion

        #region Update


        /// <summary>
        /// Performs necessary update logic.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="otherScreenHasFocus">Whether another screen has the focus.</param>
        /// <param name="coveredByOtherScreen">Whether this screen is covered by another.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Save the high-score when exiting the game
            if (isExiting)
            {
                if (!HighScoreScreen.HighscoreSaved)
                {
                    HighScoreScreen.SaveHighscore();
                }
                else
                {
                    isExiting = false;
                    // When exiting intentionally, clear the saved game data
                    NinjAcademyGame.CleanIsolatedStorage();
                    ScreenManager.Game.Exit();
                    return;
                }
            }

            // Move on to the instruction screen if necessary
            if (isMovingToLoading)
            {
                foreach (GameScreen screen in ScreenManager.GetScreens())
                {
                    screen.ExitScreen();
                }

                ScreenManager.AddScreen(new BackgroundScreen("Instructions"), null);
                ScreenManager.AddScreen(new LoadingScreen(instructionsTexture, loadingTexture), null);

                AudioManager.StopMusic();
                return;
            }

            // Cause the ninja and title text to appear gradually
            ninjaTimer += gameTime.ElapsedGameTime;
            titleTimer += gameTime.ElapsedGameTime;

            switch (ninjaState)
            {
                case ElementState.Invisible:
                    if (ninjaTimer >= ninjaAppearDelay)
                    {
                        ninjaState = ElementState.Appearing;
                        ninjaTimer = TimeSpan.Zero;
                    }
                    break;
                case ElementState.Appearing:
                    ninjaOffset = ninjaInitialOffset *
                        (float)Math.Pow(1 - ninjaTimer.TotalMilliseconds / ninjaAppearDuration.TotalMilliseconds, 2);

                    if (ninjaTimer > ninjaAppearDuration)
                    {
                        ninjaState = ElementState.Visible;
                    }
                    break;
                case ElementState.Visible:
                    // Nothing to do in this state
                    break;
                default:
                    break;
            }

            switch (titleState)
            {
                case ElementState.Invisible:
                    if (titleTimer >= titleAppearDelay)
                    {
                        titleState = ElementState.Appearing;
                        titleTimer = TimeSpan.Zero;
                    }
                    break;
                case ElementState.Appearing:
                    titleOffset = titleInitialOffset *
                        (float)Math.Pow(1 - titleTimer.TotalMilliseconds / titleAppearDuration.TotalMilliseconds, 2);

                    if (titleTimer > titleAppearDuration)
                    {
                        titleState = ElementState.Visible;
                    }
                    break;
                case ElementState.Visible:
                    // Nothing to do in this state
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles user input.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="input">Input information.</param>
        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);

            if (isExiting)
            {
                return;
            }
        }


        #endregion

        #region Rendering


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.Draw(ninjaTexture, ninjaPosition + ninjaOffset, Color.White);
            spriteBatch.Draw(titleTexture, titlePosition + titleOffset, Color.White);

            spriteBatch.End();
        }


        #endregion

        #region Menu handlers


        /// <summary>
        /// Respond to the "Play" item selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartSelected(object sender, EventArgs e)
        {
            AudioManager.PlaySound("Menu Selection");

            // If there is no game state data, start a new game
            if (!PhoneApplicationService.Current.State.ContainsKey(NinjAcademyGame.GameStateKey))
            {
                foreach (GameScreen screen in ScreenManager.GetScreens())
                {
                    screen.ExitScreen();
                }

                ScreenManager.AddScreen(new LoadingScreen(instructionsTexture, loadingTexture), null);

                AudioManager.StopMusic();
            }
            else
            {
                Guide.BeginShowMessageBox("Load game", "Saved game data detected. Load it?", 
                    new string[] { "Yes", "No" }, 0, MessageBoxIcon.None, HandleGameLoadMessageBox, null);
            }
        }

        /// <summary>
        /// Responds to the "Hi-Score" item selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HighScoreSelected(object sender, EventArgs e)
        {
            AudioManager.PlaySound("Menu Selection");

            foreach (GameScreen screen in ScreenManager.GetScreens())
            {
                screen.ExitScreen();
            }

            ScreenManager.AddScreen(new BackgroundScreen("highScoreBG"), null);
            ScreenManager.AddScreen(new HighScoreScreen(), null);
        }

        /// <summary>
        /// Called when the user tries to exit the screen.
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            isExiting = true;

            AudioManager.StopMusic();
        }


        #endregion

        #region Private Methods


        /// <summary>
        /// Handler called when the user selects whether to load saved game data or not.
        /// </summary>
        /// <param name="result"></param>
        private void HandleGameLoadMessageBox(IAsyncResult result)
        {
            int? selection = Guide.EndShowMessageBox(result);

            if (!selection.HasValue)
            {
                // The user did not decide, so do nothing
                return;
            }

            if (selection.Value == 1)
            {
                // Remove the game state data
                PhoneApplicationService.Current.State.Remove(NinjAcademyGame.GameStateKey);
            }

            isMovingToLoading = true;            
        }


        #endregion
    }
}
