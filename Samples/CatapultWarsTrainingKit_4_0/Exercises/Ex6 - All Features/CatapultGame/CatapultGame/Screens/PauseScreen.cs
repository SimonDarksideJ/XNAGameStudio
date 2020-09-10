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
        #region Fields
        GameScreen backgroundScreen;
        Player player1;
        Player player2;
        bool prevHumanIsActive;
        bool prevCompuerIsActive;
        #endregion

        #region Initialization
        public PauseScreen(GameScreen backgroundScreen, Player human, Player computer)
            : base(String.Empty)
        {
            IsPopup = true;

            this.backgroundScreen = backgroundScreen;

            // Create our menu entries.
            MenuEntry startGameMenuEntry = new MenuEntry("Return");
            MenuEntry exitMenuEntry = new MenuEntry("Quit Game");

            // Hook up menu event handlers.
            startGameMenuEntry.Selected += StartGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(startGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);

            this.player1 = human;
            this.player2 = computer;

            // Preserve the old state of the game
            prevHumanIsActive = this.player1.Catapult.IsActive;
            prevCompuerIsActive = this.player2.Catapult.IsActive;

            // Pause the game logic progress
            this.player1.Catapult.IsActive = false;
            this.player2.Catapult.IsActive = false;

            AudioManager.PauseResumeSounds(false);
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
        /// Handles "Return" menu item selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartGameMenuEntrySelected(object sender, EventArgs e)
        {
            player1.Catapult.IsActive = prevHumanIsActive;
            player2.Catapult.IsActive = prevCompuerIsActive;

            if (!(player1 as Human).isDragging)
                AudioManager.PauseResumeSounds(true);
            else
            {
                (player1 as Human).ResetDragState();
                AudioManager.StopSounds();
            }

            if (player2 is Human && (player2 as Human).isDragging)
            {
                (player2 as Human).ResetDragState();
                AudioManager.StopSounds();
            }

            backgroundScreen.ExitScreen();
            ExitScreen();
        }

        /// <summary>
        /// Handles "Exit" menu item selection
        /// </summary>
        /// 
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            AudioManager.StopSounds();
            ScreenManager.AddScreen(new MainMenuScreen(), null);
            ExitScreen();


        }
        #endregion
    }
}
