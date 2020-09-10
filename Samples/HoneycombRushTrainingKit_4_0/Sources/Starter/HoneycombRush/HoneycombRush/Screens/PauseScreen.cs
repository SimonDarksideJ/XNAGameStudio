#region File Description
//-----------------------------------------------------------------------------
// pauseBackground.cs
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
    class PauseScreen : MenuScreen
    {
        #region Initializations


        public PauseScreen()
            : base(string.Empty)
        {
            IsPopup = true;
            // Create our menu entries.
            MenuEntry returnGameMenuEntry = new MenuEntry("Resume");
            returnGameMenuEntry.Position = new Vector2(173, 364);
            returnGameMenuEntry.Scale = 0.7f;

            MenuEntry exitMenuEntry = new MenuEntry("Exit");
            exitMenuEntry.Position = new Vector2(425, 364);

            // Hook up menu event handlers.
            returnGameMenuEntry.Selected += ReturnGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            //// Add entries to the menu.
            MenuEntries.Add(returnGameMenuEntry);
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

            foreach (GameScreen screen in ScreenManager.GetScreens())
            {
                if (!(screen is GameplayScreen))
                {
                    screen.ExitScreen();
                }
            }
        }

        /// <summary>
        /// Respond to "Quit Game" Item Selection
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
                screen.ExitScreen();

            ScreenManager.AddScreen(new BackgroundScreen("titleScreen"), null);
            ScreenManager.AddScreen(new MainMenuScreen(), null);
        }


        #endregion
    }
}



