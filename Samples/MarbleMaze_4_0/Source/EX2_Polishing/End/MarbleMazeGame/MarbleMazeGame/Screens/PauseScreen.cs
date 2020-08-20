#region File Description
//-----------------------------------------------------------------------------
// PauseScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using GameStateManagement;
#endregion

namespace MarbleMazeGame
{
    class PauseScreen : MenuScreen
    {
        #region Initializations
        public PauseScreen()
            : base("Game Paused")
        {
            // Create our menu entries.
            MenuEntry returnGameMenuEntry = new MenuEntry("Return");
            MenuEntry restartGameMenuEntry = new MenuEntry("Restart");
            MenuEntry exitMenuEntry = new MenuEntry("Quit Game");

            // Hook up menu event handlers.
            returnGameMenuEntry.Selected += ReturnGameMenuEntrySelected;
            restartGameMenuEntry.Selected += RestartGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(returnGameMenuEntry);
            MenuEntries.Add(restartGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }
        #endregion
       
        #region Update
        /// <summary>
        /// Respond to "Return" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ReturnGameMenuEntrySelected(object sender, EventArgs e)
        {
            AudioManager.PauseResumeSounds(true);

            var res = from screen in ScreenManager.GetScreens()
                      where screen.GetType() != typeof(GameplayScreen)
                      select screen;

            foreach (GameScreen screen in res)
                screen.ExitScreen();

            (ScreenManager.GetScreens()[0] as GameplayScreen).IsActive = true;
        }

        /// <summary>
        /// Respond to "Restart" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RestartGameMenuEntrySelected(object sender, EventArgs e)
        {
            AudioManager.PauseResumeSounds(true);

            var res = from screen in ScreenManager.GetScreens()
                      where screen.GetType() != typeof(GameplayScreen)
                      select screen;

            foreach (GameScreen screen in res)
                screen.ExitScreen();

            (ScreenManager.GetScreens()[0] as GameplayScreen).IsActive = true;

            (ScreenManager.GetScreens()[0] as GameplayScreen).Restart();
        }

        /// <summary>
        /// Respond to "Quit Game" Item Selection
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
                screen.ExitScreen();

            ScreenManager.AddScreen(new BackgroundScreen(), null);
            ScreenManager.AddScreen(new MainMenuScreen(), null);
        }
        #endregion
    }
}
