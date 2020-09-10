//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;


namespace UserInterfaceSample
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("Main Menu")
        {
            // Create our menu entries.
            MenuEntry levelSelect = new MenuEntry("Select level");
            levelSelect.Selected += SelectLevelPressed;
            MenuEntries.Add(levelSelect);

            MenuEntry highScores = new MenuEntry("High scores");
            highScores.Selected += HighScoresPressed;
            MenuEntries.Add(highScores);
        }

        /// <summary>
        /// Event handler for our Select Level button.
        /// </summary>
        private void SelectLevelPressed(object sender, PlayerIndexEventArgs e)
        {
            // We use the loading screen to move to our level selection screen because the
            // level selection screen needs to load in a decent amount of level art. The Load
            // method will cause all current screens to exit, so to enable us to be able to
            // easily come back from the level select screen, we must also pass down the
            // background and main menu screens.
            LoadingScreen.Load(
                ScreenManager, 
                true, 
                e.PlayerIndex, 
                new BackgroundScreen(), new MainMenuScreen(), new LevelSelectScreen());
        }

        /// <summary>
        /// Event handler for our High Scores button.
        /// </summary>
        private void HighScoresPressed(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new HighScoreScreen(), e.PlayerIndex);
        }

        /// <summary>
        /// When the user cancels the main menu, we exit the game.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Exit();
        }
    }
}
