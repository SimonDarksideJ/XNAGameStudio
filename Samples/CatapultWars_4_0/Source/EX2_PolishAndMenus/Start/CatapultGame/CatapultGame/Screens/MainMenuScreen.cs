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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;
using Microsoft.Xna.Framework;
#endregion

namespace CatapultGame
{
    class MainMenuScreen : MenuScreen
    {
        public MainMenuScreen()
            : base(String.Empty)
        {
            IsPopup = true;

            // Create our menu entries.
            MenuEntry startGameMenuEntry = new MenuEntry("Play");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            startGameMenuEntry.Selected += StartGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(startGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }

        // Handles "Play" menu item selection
        void StartGameMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new InstructionsScreen(), null);
        }

        // Handles "Exit" menu item selection		
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Exit();
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
    }
}
