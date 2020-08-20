#region File Description
//-----------------------------------------------------------------------------
// NewGameSubMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using GameStateManagement;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#endregion

namespace Yacht
{
    /// <summary>
    /// A screen which allows the player a chance to load his saved game.
    /// </summary>
    class NewGameSubMenuScreen : MenuScreen
    {
        Texture2D background;

        #region Initializations


        /// <summary>
        /// Creates a new screen instance.
        /// </summary>
        public NewGameSubMenuScreen()
            : base("")
        {
  
        }


        #endregion

        #region Loading        


        /// <summary>
        /// Loads the screen contents.
        /// </summary>
        public override void LoadContent()
        {
            background = ScreenManager.Game.Content.Load<Texture2D>(@"Images\bg");
            // Create our menu entries.
            MenuEntry newGameMenuEntry = new MenuEntry("New Game");
            MenuEntry loadGameMenuEntry = new MenuEntry("Load");

            float screenWidth = ScreenManager.GraphicsDevice.Viewport.Width;
            float screenHeight = ScreenManager.GraphicsDevice.Viewport.Height;

            newGameMenuEntry.Destination = 
                new Rectangle((int)screenWidth / 2 - 75, (int)screenHeight / 2 - 40, 150, 40);
            loadGameMenuEntry.Destination = 
                new Rectangle((int)screenWidth / 2 - 75, (int)screenHeight / 2 + 40, 150, 40);

            // Hook up menu event handlers.
            newGameMenuEntry.Selected += NewGameMenuEntrySelected;
            loadGameMenuEntry.Selected += LoadGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(newGameMenuEntry);
            MenuEntries.Add(loadGameMenuEntry);
            base.LoadContent();
        }


        #endregion

        #region Update and Render        


        /// <summary>
        /// Renders the screen.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();

            ScreenManager.SpriteBatch.Draw(background, Vector2.Zero, Color.White);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Menu Handlers


        /// <summary>
        /// Respond to "Load Game" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LoadGameMenuEntrySelected(object sender, EventArgs e)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
                screen.ExitScreen();

            ScreenManager.AddScreen(new InstructionScreen(false), null);
        }

        /// <summary>
        /// Respond to "New Game" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NewGameMenuEntrySelected(object sender, EventArgs e)
        {
            if (PhoneApplicationService.Current.State.ContainsKey("ScoreCard"))
            {
                PhoneApplicationService.Current.State.Remove("ScoreCard");
                PhoneApplicationService.Current.State.Remove("DiceHandler");
            }
            else if (PhoneApplicationService.Current.State.ContainsKey("GameState"))
            {
                PhoneApplicationService.Current.State.Remove("GameState");
            }

            foreach (GameScreen screen in ScreenManager.GetScreens())
                screen.ExitScreen();

            ScreenManager.AddScreen(new InstructionScreen(true), null);
        }

        /// <summary>
        /// Handle the back button and return to the main menu.
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
                screen.ExitScreen();

            ScreenManager.AddScreen(new MainMenuScreen(), null);
        }


        #endregion
    }
}
