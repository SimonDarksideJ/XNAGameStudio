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
using System.Threading;


#endregion

namespace NinjAcademy
{
    class PauseScreen : MenuScreen
    {
        #region Fields


        float maxEntryWidth;

        // The screen from which the game was paused
        GameScreen screenToRestore;


        #endregion

        #region Initializations


        /// <summary>
        /// Creates a new pause screen instance.
        /// </summary>
        /// <param name="returnToScreen">The screen which initiated the pause operation.</param>
        public PauseScreen(GameScreen returnToScreen)
            : base("")
        {
            // Create our menu entries.
            MenuEntry resumeMenuEntry = new MenuEntry("Resume");
            MenuEntry exitMenuEntry = new MenuEntry("Quit");

            // Hook up menu event handlers.
            resumeMenuEntry.Selected += ResumeSelected;
            exitMenuEntry.Selected += ExitSelected;

            // Add entries to the menu.
            MenuEntries.Add(resumeMenuEntry);
            MenuEntries.Add(exitMenuEntry);

            screenToRestore = returnToScreen;
        }

        /// <summary>
        /// Arranges the pause screen menu items.
        /// </summary>
        protected override void UpdateMenuEntryLocations()
        {
            int menuEntryHorizontalPlacement;
            int menuEntryVerticalPlacement = GameConstants.PauseMenuTop;

            // update each menu entry's location in turn
            for (int i = 0; i < MenuEntries.Count; i++)
            {
                float entryWidth = ScreenManager.Font.MeasureString(MenuEntries[i].Text).X;
                menuEntryHorizontalPlacement = (int)(GameConstants.PauseMenuLeft + maxEntryWidth / 2 - entryWidth / 2);
                MenuEntries[i].Position = new Vector2(menuEntryHorizontalPlacement, menuEntryVerticalPlacement);
                menuEntryVerticalPlacement += GameConstants.MainMenuEntryGap;
            }
        }

        public override void LoadContent()
        {
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

        #region Menu handlers


        /// <summary>
        /// Respond to the "Resume" item selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ResumeSelected(object sender, EventArgs e)
        {
            // If we simply paused the game, then resume it
            if (screenToRestore != null)
            {
                // Exit all background screens and the current pause screen
                foreach (GameScreen screen in ScreenManager.GetScreens())
                {
                    if (screen is BackgroundScreen)
                    {
                        screen.ExitScreen();
                    }
                }

                ExitScreen();

                // Cause the previous screen to resume
                if (screenToRestore is GameplayScreen)
                {
                    (screenToRestore as GameplayScreen).ResumeGame();
                }
                else if (screenToRestore is CountdownScreen)
                {
                    (screenToRestore as CountdownScreen).ResumeCountdown();
                }

                return;
            }
        }

        /// <summary>
        /// Responds to the "Exit" item selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ExitSelected(object sender, EventArgs e)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
            {
                screen.ExitScreen();
            }

            ScreenManager.AddScreen(new BackgroundScreen("titlescreenBG"), null);
            ScreenManager.AddScreen(new MainMenuScreen(), null);

            AudioManager.PlayMusic("Menu Music");
        }


        #endregion
    }
}
