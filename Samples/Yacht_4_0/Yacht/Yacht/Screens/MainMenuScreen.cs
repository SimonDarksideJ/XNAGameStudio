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
using System.Text;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.GamerServices;
using YachtServices;


#endregion

namespace Yacht
{
    class MainMenuScreen : MenuScreen
    {
        #region Fields
        

        Texture2D background;
        Texture2D titleTexture;

        Vector2 titlePosition;

        bool isInvalidName;
        string invalidName;


        #endregion

        #region Initializations


        /// <summary>
        /// Initializes a new instance of the screen.
        /// </summary>
        public MainMenuScreen()
            : base("")
        {
        }


        #endregion

        #region Loading


        /// <summary>
        /// Initializes the menu displayed on the screen.
        /// </summary>
        public override void LoadContent()
        {
            background = ScreenManager.Game.Content.Load<Texture2D>(@"Images\titlescreen");
            titleTexture = ScreenManager.Game.Content.Load<Texture2D>(@"Images\yachtTitle");

            titlePosition = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 -
                (titleTexture.Width / 2), 20);

            // Create our menu entries.
            MenuEntry offlineGameMenuEntry = new MenuEntry("Offline Game");
            MenuEntry onlineGameMenuEntry = new MenuEntry("Online Game");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            offlineGameMenuEntry.Destination = new Rectangle(30, (int)titlePosition.Y + titleTexture.Height
                + 20, 165, 55);
            onlineGameMenuEntry.Destination = new Rectangle(30, (int)titlePosition.Y + titleTexture.Height
                + 80, 165, 55);
            exitMenuEntry.Destination = new Rectangle(30, (int)titlePosition.Y + titleTexture.Height
                + 140, 165, 45);

            // Hook up menu event handlers.
            offlineGameMenuEntry.Selected += OfflineGameMenuEntrySelected;
            onlineGameMenuEntry.Selected += OnlineGameMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(offlineGameMenuEntry);
            MenuEntries.Add(onlineGameMenuEntry);
            MenuEntries.Add(exitMenuEntry);


            base.LoadContent();
        }


        #endregion

        #region Update


        /// <summary>
        /// Perform update logic.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="otherScreenHasFocus">Whether another screen has the focus.</param>
        /// <param name="coveredByOtherScreen">Whether this screen is covered by another.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (isInvalidName)
            {
                isInvalidName = false;

                Guide.BeginShowKeyboardInput(PlayerIndex.One, "Enter your name", "The name is not valid.", invalidName,
                    EnterNameDialogEnded, null);
            }            
        }

        /// <summary>
        /// Respond to "Offline Game" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OfflineGameMenuEntrySelected(object sender, EventArgs e)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
                screen.ExitScreen();

            if (YachtGame.LoadGameState(YachtServices.GameTypes.Offline))
            {
                // Remove stored offline data
                YachtGame.DeleteIsolatedStorageFile(Constants.YachtStateFileNameOffline);

                ScreenManager.AddScreen(new NewGameSubMenuScreen(), null);
            }
            else
            {
                PhoneApplicationService.Current.State[Constants.YachtStateKey] = new YachtState();
                

                ScreenManager.AddScreen(new InstructionScreen(true), null);
            }
        }

        /// <summary>
        /// Respond to "Online Game" Item Selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnlineGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            if (NetworkManager.Instance.name == null || NetworkManager.Instance.name == "")
            {
                Guide.BeginShowKeyboardInput(PlayerIndex.One, "Enter your name", String.Empty, "Player1",
                    EnterNameDialogEnded, null);
            }
            else
            {
                ExitScreen();
                ScreenManager.AddScreen(new SelectOnlineGameScreen(NetworkManager.Instance.name), null);
            }

        }

        /// <summary>
        /// Called once the player has selected a name for himself.
        /// </summary>
        /// <param name="result">Dialog result containing the text entered by the user.</param>

        private void EnterNameDialogEnded(IAsyncResult result)
        {
            string name = Guide.EndShowKeyboardInput(result);
            if (name != null)
            {
                if (StringUtility.IsNameValid(name))
                {
                    ExitScreen();
                    ScreenManager.AddScreen(new SelectOnlineGameScreen(name), null);
                }
                else
                {
                    isInvalidName = true;
                    invalidName = name;
                }
            }
        }

        /// <summary>
        /// Respond to "Exit" Item Selection
        /// </summary>
        /// <param name="playerIndex"></param>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Exit();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();

            ScreenManager.SpriteBatch.Draw(background, Vector2.Zero, Color.White);
            ScreenManager.SpriteBatch.Draw(titleTexture, titlePosition, Color.White);

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }


        #endregion
    }
}