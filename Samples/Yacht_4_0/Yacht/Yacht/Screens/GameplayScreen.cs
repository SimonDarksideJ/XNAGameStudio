#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
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
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Phone.Shell;
using YachtServices;
using System.Threading;


#endregion

namespace Yacht
{
    /// <summary>
    /// The main gameplay screen.
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields


        Texture2D background;
        GameStateHandler gameStateHandler;
        DiceHandler diceHandler;
        string name;
        Timer timer = null;
        Random random = new Random();
        GameTypes gameType;


        #endregion

        #region Initializations


        /// <summary>
        /// Initialize a new game screen.
        /// </summary>
        /// <param name="gameType">The type of game for which this screen is created.</param>
        public GameplayScreen(GameTypes gameType)
        {
            this.gameType = gameType;
            EnabledGestures = GestureType.Tap | GestureType.VerticalDrag | GestureType.DragComplete;
        }

        /// <summary>
        /// Initialize a new game screen.
        /// </summary>
        /// <param name="name">The name of the human player participating in the game.</param>
        /// <param name="gameType">The type of game for which this screen is created.</param>
        public GameplayScreen(string name, GameTypes gameType)
            : this(gameType)
        {
            this.name = name;
        }


        #endregion

        #region Loading


        /// <summary>
        /// Load all the game content.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();

            Dice.LoadAssets(ScreenManager.Game.Content);

            ContentManager content = ScreenManager.Game.Content;

            background = content.Load<Texture2D>(@"Images\bg");

            // When reaching a gameplay screen, we know that there is a yacht state in the current state object
            YachtState yachtState = (YachtState)PhoneApplicationService.Current.State[Constants.YachtStateKey];

            InitializeDiceHandler(yachtState.PlayerDiceState, gameType);
            yachtState.PlayerDiceState = diceHandler.DiceState;
            diceHandler.PositionDice();

            if (gameType == GameTypes.Offline)
            {
                InitializeGameStateHandler(yachtState.YachGameState);
                yachtState.YachGameState = gameStateHandler.State;            
            }
            else
            {
                // Register for network notifications
                NetworkManager.Instance.GameStateArrived += ServerGameStateArrived;
                NetworkManager.Instance.GameOver += GameOverEnded;
                NetworkManager.Instance.Banned += Banned;
                NetworkManager.Instance.GameUnavailable += GameUnavailable;
                NetworkManager.Instance.ServiceError += ServerErrorOccurred;

                // Get the updated game state instead of initializing from the state object
                NetworkManager.Instance.GetGameState();
            }            
        }

        /// <summary>
        /// Handler called when the server declares that the game is over.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GameOverEnded(object sender, YachtGameOverEventArgs e)
        {
            gameStateHandler.ShowGameOver(e.EndGameState);

            if (gameStateHandler.WinnerPlayer is HumanPlayer)
            {
                AudioManager.PlaySound("Winner");
            }
            else
            {
                AudioManager.PlaySound("Loss");
            }
        }

        /// <summary>
        /// Initializes the dice handler used to handle the player's dice, and loads resources used for displaying
        /// it.
        /// </summary>
        /// <param name="diceState">State to initialize the dice handler according to, or null to use the default 
        /// initial state.</param>
        /// <param name="gameType">The game type for which the dice are initialized.</param>
        private void InitializeDiceHandler(DiceState diceState, GameTypes gameType)
        {
            diceHandler = new DiceHandler(ScreenManager.Game.GraphicsDevice, diceState);
            diceHandler.LoadAssets(ScreenManager.Game.Content);            
        }

        /// <summary>
        /// Initializes the game state handler used to manage the game state, and loads resources used for displaying
        /// it.
        /// </summary>
        /// <param name="state">State to initialize the game handler according to, or null to use the default 
        /// initial state.</param>
        private void InitializeGameStateHandler(GameState state)
        {            
            gameStateHandler = new GameStateHandler(diceHandler, ScreenManager.input, name, state,
                ScreenManager.Game.GraphicsDevice.Viewport.Bounds, ScreenManager.Game.Content);
        }


        #endregion

        #region Update and Render


        /// <summary>
        /// Handle user input.
        /// </summary>
        /// <param name="input">User input information.</param>
        public override void HandleInput(InputState input)
        {
            if (!Guide.IsVisible)
            {
                if (input.IsPauseGame(null))
                {
                    QuiteGame();
                }

                if (gameStateHandler != null && gameStateHandler.IsGameOver && input.Gestures.Count > 0 &&
                    input.Gestures[0].GestureType == GestureType.Tap)
                {
                    // We exit an online game after it has finished, so remove the state data.
                    PhoneApplicationService.Current.State.Remove(Constants.YachtStateKey);

                    ExitScreen();
                    Dispose();
                    if (gameStateHandler.State.GameType == GameTypes.Offline)
                    {
                        ScreenManager.AddScreen(new MainMenuScreen(), null);
                    }
                    else
                    {
                        ScreenManager.AddScreen(new SelectOnlineGameScreen(NetworkManager.Instance.name), null);
                    }


                }

                for (int i = 0; i < input.Gestures.Count; i++)
                {
                    if (gameStateHandler != null && gameStateHandler.IsInitialized)
                    {
                        gameStateHandler.HandleInput(input.Gestures[i]);
                    }
                }
            }

            base.HandleInput(input);
        }

        /// <summary>
        /// Pause the game.
        /// </summary>
        private void QuiteGame()
        {
            if (gameStateHandler.State.GameType == GameTypes.Offline)
            {

                // Give the user a chance to save his current progress
                Guide.BeginShowMessageBox("Save Game", "Do you want to save your progress?",
                    new String[] { "Yes", "No" }, 0, MessageBoxIcon.Warning,
                    ShowSaveDialogEnded, null);
            }
            else
            {
                if (gameStateHandler.IsGameOver)
                {
                    HandleExitScreen();
                }
                else
                {
                    // Give the user a chance to abort exiting the game
                    Guide.BeginShowMessageBox("Are you sure you want to leave the game?", " ",
                        new String[] { "Yes", "No" }, 0, MessageBoxIcon.Warning,
                        AbortExitDialogEnded, null);
                }
            }
        }

        /// <summary>
        /// Handler for the warning box displayed when the user tries to exit the game.
        /// </summary>
        /// <param name="result">The popup messagebox result.</param>
        private void AbortExitDialogEnded(IAsyncResult result)
        {
            int? res = Guide.EndShowMessageBox(result);

            if (res == 0)
            {
                HandleExitScreen();
            }
        }

        /// <summary>
        /// Exits the gameplay screen.
        /// </summary>
        private void HandleExitScreen()
        {
            ExitScreen();
            Dispose();

            // We voluntarily exit an online game, so remove the state data.
            PhoneApplicationService.Current.State.Remove(Constants.YachtStateKey);

            NetworkManager.Instance.Unregister();

            ScreenManager.AddScreen(new MainMenuScreen(), null);
        }

        /// <summary>
        /// Handler for the dialog box which offers to save the game state.
        /// </summary>
        /// <param name="result">The popup messagebox result.</param>
        private void ShowSaveDialogEnded(IAsyncResult result)
        {
            int? res = Guide.EndShowMessageBox(result);

            if (res.HasValue)
            {
                // Store the user's progress
                if (res.Value == 0)
                {
                    YachtGame.SaveGameState();
                }                

                // Remove state information before exiting the game
                PhoneApplicationService.Current.State.Remove(Constants.YachtStateKey);

                ExitScreen();
                Dispose();

                ScreenManager.AddScreen(new MainMenuScreen(), null);
            }
        }

        /// <summary>
        /// Perform the game's update logic.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="otherScreenHasFocus">Whether another screen has the focus currently.</param>
        /// <param name="coveredByOtherScreen">Whether this screen is covered by another screen.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!Guide.IsVisible && gameStateHandler != null && !gameStateHandler.IsGameOver)
            {
                gameStateHandler.SetScoreDice(diceHandler.GetHoldingDice());

                diceHandler.Update();

                if (gameStateHandler.IsInitialized && gameStateHandler.CurrentPlayer != null && 
                    !gameStateHandler.IsWaitingForPlayer)
                {
                    if (!(gameStateHandler.CurrentPlayer is AIPlayer))
                    {
                        gameStateHandler.CurrentPlayer.PerformPlayerLogic();
                    }
                    else
                    {
                        if (timer == null)
                        {
                            timer = new Timer(MakeAIPlay, gameStateHandler.CurrentPlayer, random.Next(300, 600), -1);
                        }
                    }
                }

            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Draws the game.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);

            ScreenManager.SpriteBatch.Begin();

            // Draw all game component
            ScreenManager.SpriteBatch.Draw(background, Vector2.Zero, Color.White);

            if (!Guide.IsVisible)
            {
                if (gameStateHandler != null && gameStateHandler.IsInitialized)
                {
                    if (diceHandler != null && !gameStateHandler.IsGameOver)
                    {
                        diceHandler.Draw(ScreenManager.SpriteBatch);
                        if (!gameStateHandler.IsWaitingForPlayer)
                        {
                            gameStateHandler.CurrentPlayer.Draw(ScreenManager.SpriteBatch);
                        }
                    }
                    gameStateHandler.Draw(ScreenManager.SpriteBatch);
                }

                DrawGameOver();
            }

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws information about the winning player once the game is over.
        /// </summary>
        private void DrawGameOver()
        {
            if (gameStateHandler != null && gameStateHandler.IsGameOver)
            {
                Rectangle screenBounds = ScreenManager.Game.GraphicsDevice.Viewport.Bounds;
                string winnerText = string.Format("{0} is the winner!", gameStateHandler.WinnerPlayer.Name);
                Vector2 measure = YachtGame.Font.MeasureString(winnerText);
                Vector2 position = new Vector2(screenBounds.Center.X - measure.X / 2,
                    screenBounds.Bottom - 100);
                ScreenManager.SpriteBatch.DrawString(YachtGame.Font, winnerText, position, Color.White);
            }
        }


        #endregion

        /// <summary>
        /// Handler called once the server reports a specific game is unavailable.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GameUnavailable(object sender, EventArgs e)
        {
            while (Guide.IsVisible) { };

            Guide.BeginShowMessageBox("The game " + NetworkManager.Instance.gameName + " is unavailable",
                "Do you want to create a new game with same name, or join another game?",
                new[] { "Create", "Join" }, 0, MessageBoxIcon.None, UnavailableGameDialogEnded, null);
        }

        /// <summary>
        /// Called after the player dismisses the dialog stating a game is unavailable.
        /// </summary>
        /// <param name="result">The messagebox selection result.</param>
        private void UnavailableGameDialogEnded(IAsyncResult result)
        {
            int? res = Guide.EndShowMessageBox(result);

            if (res == 0)
            {
                // Create a new game with the same name.
                NetworkManager.Instance.NewGameCreated += Instance_NewGameCreated;
                NetworkManager.Instance.NewGame(NetworkManager.Instance.gameName);
            }
            else
            {
                // Return to the game selection screen.                
                PhoneApplicationService.Current.State.Remove(Constants.YachtStateKey);

                ExitScreen();
                Dispose();

                if (res == 1)
                {
                    ScreenManager.AddScreen(new SelectOnlineGameScreen(NetworkManager.Instance.name), null);
                }
                else
                {
                    ScreenManager.AddScreen(new MainMenuScreen(), null);
                }
            }
        }

        /// <summary>
        /// Handler called once a new game instance has been created.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Instance_NewGameCreated(object sender, BooleanEventArgs e)
        {
            NetworkManager.Instance.NewGameCreated -= Instance_NewGameCreated;
            if (e.Answer)
            {
                NetworkManager.Instance.GetGameState();
            }
            else
            {
                Guide.BeginShowMessageBox("Cannot create the game with same name the name is in use", "",
                    new String[] { "OK" }, 0, MessageBoxIcon.Alert, null, null);
                ExitScreen();
                Dispose();

                ScreenManager.AddScreen(new SelectOnlineGameScreen(NetworkManager.Instance.name), null);
            }
        }

        /// <summary>
        /// Handler called when an error occurs communicating with the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ServerErrorOccurred(object sender, ExceptionEventArgs e)
        {
            Guide.BeginShowMessageBox("There was a server error. Please try to connect again.", " ",
                new[] { "OK" }, 0, MessageBoxIcon.Error, ErrorDialogEnded, null);
        }

        /// <summary>
        /// Called once the server error dialog is dismissed.
        /// </summary>
        /// <param name="result">The messagebox selection result.</param>
        void ErrorDialogEnded(IAsyncResult result)
        {
            ExitScreen();
            Dispose();

            ScreenManager.AddScreen(new SelectOnlineGameScreen(NetworkManager.Instance.name), null);
        }

        /// <summary>
        /// Called when the server kicks the player from the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Banned(object sender, EventArgs e)
        {
            ExitScreen();
            Dispose();

            ScreenManager.AddScreen(new MainMenuScreen(), null);
        }

        /// <summary>
        /// Handler called when a game state is received from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ServerGameStateArrived(object sender, YachtGameStateEventArgs e)
        {
            if (gameStateHandler == null)
            {
                InitializeGameStateHandler(e.GameState);

                YachtState yachtState = (YachtState)PhoneApplicationService.Current.State[Constants.YachtStateKey];
                yachtState.NetworkManagerState = NetworkManager.Instance;

                // Update state object with the new game state
                yachtState.YachGameState = gameStateHandler.State;

                // Reset the dice state if it is not valid for this turn
                if (diceHandler.DiceState.ValidForTurn != e.GameState.StepsMade)
                {
                    diceHandler.Reset(false);
                }

                NetworkManager.Instance.ScoreCardArrived += ServerScoreCardArrived;
            }
            else
            {
                if (e.GameState.StepsMade != gameStateHandler.State.StepsMade)
                {
                    diceHandler.Reset(false);
                }                

                gameStateHandler.SetState(e.GameState);
            }

            diceHandler.DiceState.ValidForTurn = gameStateHandler.State.StepsMade;

            AudioManager.PlaySoundRandom("TurnChange", 2);

            if (e.GameState.Players[e.GameState.CurrentPlayer].PlayerID == NetworkManager.Instance.playerID)
            {
                NetworkManager.Instance.GetScoreCard();
            }
        }

        /// <summary>
        /// Handler called when score card information arrives from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ServerScoreCardArrived(object sender, YachtScoreCardEventArgs e)
        {

            gameStateHandler.UpdateScoreCard(e.ScoreCard);
        }

        /// <summary>
        /// Causes local AI players to play.
        /// </summary>
        /// <param name="obj">Additional user data.</param>
        private void MakeAIPlay(object obj)
        {
            timer = null;
            (obj as AIPlayer).PerformPlayerLogic();
        }

        /// <summary>
        /// Performs necessary cleanup before disposing of the screen.
        /// </summary>
        public void Dispose()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.GameStateArrived -= ServerGameStateArrived;
                NetworkManager.Instance.ServiceError -= ServerErrorOccurred;
                NetworkManager.Instance.Banned -= Banned;
                NetworkManager.Instance.GameUnavailable -= GameUnavailable;
                NetworkManager.Instance.ScoreCardArrived -= ServerScoreCardArrived;
                NetworkManager.Instance.GameOver -= GameOverEnded;
            }
        }
    }
}
