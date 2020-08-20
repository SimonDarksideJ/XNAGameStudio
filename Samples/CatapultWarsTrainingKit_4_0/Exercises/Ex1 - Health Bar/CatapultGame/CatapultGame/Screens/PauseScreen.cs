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
        Player human;
        Player computer;
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

            this.human = human;
            this.computer = computer;

            // Preserve the old state of the game
            prevHumanIsActive = this.human.Catapult.IsActive;
            prevCompuerIsActive = this.computer.Catapult.IsActive;

            // Pause the game logic progress
            this.human.Catapult.IsActive = false;
            this.computer.Catapult.IsActive = false;

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
            human.Catapult.IsActive = prevHumanIsActive;
            computer.Catapult.IsActive = prevCompuerIsActive;

            if (!(human as Human).isDragging)
                AudioManager.PauseResumeSounds(true);
            else
            {
                (human as Human).ResetDragState();
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
