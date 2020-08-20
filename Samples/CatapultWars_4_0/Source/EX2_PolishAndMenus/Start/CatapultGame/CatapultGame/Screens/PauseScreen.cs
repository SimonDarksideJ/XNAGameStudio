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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;
using Microsoft.Xna.Framework;

#endregion



namespace CatapultGame
{
    class PauseScreen : MenuScreen
    {
        GameScreen backgroundScreen;

        public PauseScreen(GameScreen backgroundScreen)
            : base(String.Empty)
        {
            IsPopup = true;

            this.backgroundScreen = backgroundScreen;

            // Create our menu entries.
            MenuEntry startGameMenuEntry = new MenuEntry("Return");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            startGameMenuEntry.Selected += StartGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(startGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }

        protected override void UpdateMenuEntryLocations()
        {
            base.UpdateMenuEntryLocations();

            foreach (var entry in MenuEntries)
            {
                var position = entry.Position;

                position.Y += 60;

                entry.Position = position;
            }
        }

        /// <summary>
        /// Handles "Return" menu item selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartGameMenuEntrySelected(object sender, EventArgs e)
        {
            backgroundScreen.ExitScreen();
            ExitScreen();
        }

        /// <summary>
        /// Handles "Exit" menu item selection
        /// </summary>
        /// 
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Exit();
        }
    }
}
