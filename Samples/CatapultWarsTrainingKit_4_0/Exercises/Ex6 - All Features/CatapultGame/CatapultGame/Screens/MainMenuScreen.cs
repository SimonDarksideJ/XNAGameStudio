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
        #region Initialization
        public MainMenuScreen()
            : base(String.Empty)
        {
            IsPopup = true;

            // Create our menu entries.
            MenuEntry startSinglePlayerGameMenuEntry = new MenuEntry("Play VS Phone");
            MenuEntry startTwoPlayersGameMenuEntry = new MenuEntry("Play VS Human");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            startSinglePlayerGameMenuEntry.Selected += StartSinglePlayerGameMenuEntrySelected;
            startTwoPlayersGameMenuEntry.Selected += StartTwoPlayerGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(startSinglePlayerGameMenuEntry);
            MenuEntries.Add(startTwoPlayersGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }
        #endregion

        #region Overrides
        protected override void UpdateMenuEntryLocations()
        {
            base.UpdateMenuEntryLocations();

            foreach (var entry in MenuEntries)
            {
                Vector2 position = entry.Position;

                position.Y += 60;

                entry.Position = position;
            }
        }
        #endregion

        #region Event Handlers for Menu Items
        /// <summary>
        /// Handles "Play VS Phone" menu item selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartSinglePlayerGameMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new InstructionsScreen(false), null);
        }

        /// <summary>
        /// Handles "Play VS Human" menu item selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartTwoPlayerGameMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new InstructionsScreen(true), null);
        }

        /// <summary>
        /// Handles "Exit" menu item selection
        /// </summary>
        /// 
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Exit();
        }
        #endregion
    }
}
