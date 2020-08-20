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


#endregion

namespace HoneycombRush
{
    class MainMenuScreen : MenuScreen
    {
        #region Initializations


        public MainMenuScreen()
            : base("")
        {
            // Create our menu entries.
            MenuEntry startGameMenuEntry = new MenuEntry("Start");
            startGameMenuEntry.Position = new Vector2(173,364);
            MenuEntry exitMenuEntry = new MenuEntry("Exit");
            exitMenuEntry.Position = new Vector2(425, 364);

            // Hook up menu event handlers.
            startGameMenuEntry.Selected += StartGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(startGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);            
        }

        public override void LoadContent()
        {
            AudioManager.LoadSounds();
            AudioManager.LoadMusic();

            AudioManager.PlayMusic("MenuMusic_Loop");

            base.LoadContent();
        }


        #endregion

        #region Update


        /// <summary>
        /// Respond to "Play" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartGameMenuEntrySelected(object sender, EventArgs e)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
                screen.ExitScreen();

            ScreenManager.AddScreen(new BackgroundScreen("Instructions"), null);
            ScreenManager.AddScreen(new LoadingAndInstructionScreen(), null);



            AudioManager.StopSound("MenuMusic_Loop");
        }

        /// <summary>
        /// Respond to "Exit" Item Selection
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            HighScoreScreen.SaveHighscore();

            ScreenManager.Game.Exit();

            AudioManager.StopSound("MenuMusic_Loop");
        }


        #endregion
    }
}
