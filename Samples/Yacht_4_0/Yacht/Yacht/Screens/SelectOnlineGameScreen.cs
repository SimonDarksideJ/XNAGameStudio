#region File Description
//-----------------------------------------------------------------------------
// SelectOnlineGameScreen.cs
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;
using YachtServices;
using Microsoft.Phone.Shell;


#endregion

namespace Yacht
{
    /// <summary>
    /// Screen where the player can select a game to join.
    /// </summary>
    class SelectOnlineGameScreen : GameScreen, IDisposable
    {
        #region Fields


        Texture2D background;
        Texture2D line;
        Texture2D scrollThumb;
        AvailableGames availableGames = null;
        Button searchAgain;
        Button newGame;
        Button connect;

        bool isConnecting = true;
        bool isConnected;
        bool isSearching;
        string error;
        string name;
        int frame = 300;
        string text = "Connecting...";
        Vector2 offset = Vector2.Zero;
        const int maxShowGames = 10;
        int? selectedLine;


        #endregion

        #region Initialization
        

        /// <summary>
        /// Creates a new instance of the screen.
        /// </summary>
        /// <param name="name">The name of the player.</param>
        public SelectOnlineGameScreen(string name)
        {
            this.name = name;
            NetworkManager.Instance.Registered += Instance_Registered;
            NetworkManager.Instance.ServiceError += Instance_Error;
            NetworkManager.Instance.AvailableGamesArrived += Instance_AvailableGamesArrived;
            NetworkManager.Instance.NewGameCreated += Instance_NewGameCreated;
            NetworkManager.Instance.JoinedGame += Instance_JoinedGame;
            NetworkManager.Instance.Connect(name);
            EnabledGestures = GestureType.Tap | GestureType.VerticalDrag | GestureType.DragComplete;
        }


        #endregion

        #region Loading


        /// <summary>
        /// Loads screen resources.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;
            background = content.Load<Texture2D>(@"Images\online_game_selectionBG");
            line = content.Load<Texture2D>(@"Images\button");
            scrollThumb = content.Load<Texture2D>(@"Images\ScrollThumb");

            connect = new Button(content.Load<Texture2D>(@"Images\button"), 
                new Rectangle(5, 700,140,60),YachtGame.Font,"Connect");
            connect.Click += connect_Click;

            searchAgain = new Button(content.Load<Texture2D>(@"Images\button"),
                new Rectangle(170, 700, 140, 60), YachtGame.Font, "Search");
            searchAgain.Click += searchAgain_Click;

            newGame = new Button(content.Load<Texture2D>(@"Images\button"),
                new Rectangle(340, 700, 140, 60), YachtGame.Font, "New Game");
            newGame.Click += newGame_Click;

            base.LoadContent();
        }


        #endregion

        #region Update and Render


        /// <summary>
        /// Handle player input.
        /// </summary>
        /// <param name="input">Input information.</param>
        public override void HandleInput(InputState input)
        {
            if (input.IsPauseGame(null))
            {
                ExitScreen();
                Dispose();
                NetworkManager.Instance.Unregister();
                ScreenManager.AddScreen(new MainMenuScreen(), null);
            }

            for (int i = 0; i < input.Gestures.Count; i++)
            {
                connect.HandleInput(input.Gestures[i]);
                searchAgain.HandleInput(input.Gestures[i]);
                newGame.HandleInput(input.Gestures[i]);

                HandleDragList(input.Gestures[i]);
                HandleSelectedGameInput(input.Gestures[i]);
            }

            base.HandleInput(input);
        }

        /// <summary>
        /// Handles drag input to allow the player to browse through the available games.
        /// </summary>
        /// <param name="sample">Gesture information.</param>
        private void HandleDragList(GestureSample sample)
        {
            if (availableGames != null && sample.GestureType == GestureType.VerticalDrag)
            {
                Rectangle bounds = new Rectangle(10, 100, line.Width, line.Height * maxShowGames);
                Rectangle touchRect = new Rectangle((int)sample.Position.X - 5, (int)sample.Position.Y - 5, 10, 10);
                if (bounds.Intersects(touchRect))
                {
                    offset.Y -= sample.Delta.Y;

                    offset.Y = MathHelper.Clamp(offset.Y, 0, line.Height * (availableGames.Games.Count - maxShowGames));
                }
            }
        }

        /// <summary>
        /// Handles tap input which selects a game out of the game list.
        /// </summary>
        /// <param name="sample">Gesture information.</param>
        private void HandleSelectedGameInput(GestureSample sample)
        {
            if (availableGames != null && sample.GestureType == GestureType.Tap)
            {
                int heightInteval = 60;
                Rectangle screenBounds = ScreenManager.GraphicsDevice.Viewport.Bounds;
                Rectangle touchRect = new Rectangle((int)sample.Position.X - 5, 
                    (int)sample.Position.Y - 5, 10, 10);
                int from = (int)offset.Y / heightInteval;
                int to = (int)MathHelper.Clamp(from + maxShowGames, 0, availableGames.Games.Count);
                for (int i = from; i < to; i++)
                {
                    Vector2 sizeOfText = YachtGame.Font.MeasureString(availableGames.Games[i].Name);
                    Rectangle gameRect = new Rectangle(0,
                        200 + (i - from) * heightInteval, screenBounds.Width, line.Height);
                    if (gameRect.Intersects(touchRect))
                    {
                        selectedLine = i;
                    }
                }
            }
        }

        /// <summary>
        /// Performs the screens update logic.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="otherScreenHasFocus">Whether another screen has the focus currently.</param>
        /// <param name="coveredByOtherScreen">Whether this screen is covered by another screen.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            connect.Enabled = isConnected && selectedLine.HasValue;
            searchAgain.Enabled = !isConnecting && !isSearching;
            newGame.Enabled = isConnected;

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Renders the screen.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.Draw(background, Vector2.Zero, Color.White);

            searchAgain.Draw(spriteBatch);
            newGame.Draw(spriteBatch);
            connect.Draw(spriteBatch);

            if (isConnecting)
            {
                frame++;
                spriteBatch.DrawString(YachtGame.Font, text.Substring(0, frame / 30),
                    new Vector2(10, 10), Color.White);
                if (frame / 30 == text.Length)
                {
                    frame = 300;
                }
            }
            else if (error != null)
            {
                spriteBatch.DrawString(YachtGame.Font, error, new Vector2(10, 10), Color.White);
            }
            else if (availableGames != null)
            {
                DrawAvailableGames(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the list of available games.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to use for rendering the list.</param>
        private void DrawAvailableGames(SpriteBatch spriteBatch)
        {
            Rectangle screenBounds = ScreenManager.GraphicsDevice.Viewport.Bounds;
            int heightInteval = 60;
            int from = (int)offset.Y / heightInteval;
            int to = (int)MathHelper.Clamp(from + maxShowGames, 0, availableGames.Games.Count);
            for (int i = from; i < to; i++)
            {
                //spriteBatch.Draw(line, new Vector2(screenBounds.Center.X - line.Width / 2,
                //  200 + (i - from) * heightInteval), Color.White);

                spriteBatch.Draw(line, new Rectangle(0,200 + (i - from) * heightInteval,
                    screenBounds.Width,line.Height), Color.White);

                Vector2 sizeOfText = YachtGame.Font.MeasureString(availableGames.Games[i].Name);

                spriteBatch.DrawString(YachtGame.Font, availableGames.Games[i].Name,
                    new Vector2(screenBounds.Center.X - sizeOfText.X / 2,
                        200 + (i - from) * heightInteval), i == selectedLine ? Color.Red : Color.White);
            }
            if (availableGames.Games.Count == 0)
            {
                spriteBatch.DrawString(YachtGame.Font, "No available games", new Vector2(10, 220), Color.White);
            }
           else if (availableGames.Games.Count > maxShowGames)
            {
                float scrollYPos = (heightInteval * maxShowGames - scrollThumb.Height) /
                    (float)(heightInteval * availableGames.Games.Count - heightInteval * maxShowGames) * offset.Y;
                spriteBatch.Draw(scrollThumb,
                    new Vector2(screenBounds.Center.X - line.Width / 2 - 10, 100 + scrollYPos), Color.White);
            }
        }


        #endregion

        #region Private Methods


        /// <summary>
        /// Launches a dialog for entering a name when creating a new game.
        /// </summary>
        /// <param name="description">Description to display for the dialog.</param>
        /// <param name="defaultName">Default name to display in the dialog.</param>

        private void EnterGameName(string description, string defaultName)
        {
            while (Guide.IsVisible) { }

            Guide.BeginShowKeyboardInput(PlayerIndex.One, "Enter game name", description,
                defaultName, EnterGameNameDialogEnded, null);
        }

        /// <summary>
        /// Called once the player has selected a name for the game he is creating.
        /// </summary>
        /// <param name="result">Dialog result containing the text entered by the user.</param>
        private void EnterGameNameDialogEnded(IAsyncResult result)
        {
            string res = Guide.EndShowKeyboardInput(result);
            if (res != null)
            {
                if (StringUtility.IsNameValid(res))
                {
                    NetworkManager.Instance.NewGame(res);
                }
                else
                {
                    EnterGameName("The name is not valid", res);
                }
            }
        }


        #endregion

        #region Event Handlers


        /// <summary>
        /// Called when there is an error contacting the game server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Instance_Error(object sender, ExceptionEventArgs e)
        {
            isConnecting = false;
            error = "Server unavailable";
        }

        /// <summary>
        /// Called once the server handles the client's registration request.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Instance_Registered(object sender, BooleanEventArgs e)
        {
            if (e.Answer)
            {
                isConnecting = false;
                isConnected = true;
                NetworkManager.Instance.GetAvailableGames();
            }
            else
            {
                isConnecting = false;
                error = "Register failed";
            }
        }

        /// <summary>
        /// Called when the list of available games arrives from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Instance_AvailableGamesArrived(object sender, YachtAvailableGamesEventArgs e)
        {
            isSearching = false;
            searchAgain.Enabled = !isSearching;
            availableGames = e.AvailableGames;
        }

        /// <summary>
        /// Handler for the "Connect" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void connect_Click(object sender, EventArgs e)
        {
            if (!isConnecting && selectedLine.HasValue)
            {
                NetworkManager.Instance.JoinGame(availableGames.Games[selectedLine.Value].GameID);
            }
        }

        /// <summary>
        /// Handler called once the server recognizes the player's request to join a game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Instance_JoinedGame(object sender, BooleanEventArgs e)
        {
            if (e.Answer)
            {
                PhoneApplicationService.Current.State[Constants.YachtStateKey] = new YachtState();

                ExitScreen();
                Dispose();

                ScreenManager.AddScreen(new GameplayScreen(GameTypes.Online), null);
            }
            else
            {
                Guide.BeginShowMessageBox("Cannot join this game", " ",
                    new String[] { "OK" }, 0, MessageBoxIcon.Alert, null, null);
            }
        }

        /// <summary>
        /// Called once the player clicks the button for creating a new game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void newGame_Click(object sender, EventArgs e)
        {
            EnterGameName("", "Game1");
        }

        /// <summary>
        /// Called once the server responds to the user's request to create a new game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Instance_NewGameCreated(object sender, BooleanEventArgs e)
        {
            if (e.Answer)
            {                                
                PhoneApplicationService.Current.State[Constants.YachtStateKey] = new YachtState();

                ExitScreen();
                Dispose();

                ScreenManager.AddScreen(new GameplayScreen(GameTypes.Online), null);
            }
            else
            {
                EnterGameName("The name is in use", NetworkManager.Instance.gameName);
            }
        }

        /// <summary>
        /// Called when the player clicks the button to retrieve the game list from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void searchAgain_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                NetworkManager.Instance.Connect(name);
                isConnecting = true;
                error = null;
            }
            else
            {
                isSearching = true;
                searchAgain.Enabled = !isSearching;
                NetworkManager.Instance.GetAvailableGames();
                selectedLine = null;
            }
        }


        #endregion

        /// <summary>
        /// Performs cleanup operations before removing the screen.
        /// </summary>
        public void Dispose()
        {
            NetworkManager.Instance.Registered -= Instance_Registered;
            NetworkManager.Instance.ServiceError -= Instance_Error;
            NetworkManager.Instance.AvailableGamesArrived -= Instance_AvailableGamesArrived;
            NetworkManager.Instance.NewGameCreated -= Instance_NewGameCreated;
            NetworkManager.Instance.JoinedGame -= Instance_JoinedGame;
        }
    }
}
