#region File Description
//-----------------------------------------------------------------------------
// TicTacToe.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Phone.Notification;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO.IsolatedStorage;
using TicTacToeServices;
using System.Xml;
using TicTacToeGame.GameService;
using System.Threading;
using Microsoft.Phone.Shell;
using System.ComponentModel;


#endregion

namespace TicTacToe
{
    /// <summary>
    /// Possible game states.
    /// </summary>
    enum TicTacToeState
    {
        GameInitialize,
        WaitingForService,
        ServiceNotAvailable,
        PlayerTurn,
        AITurn,
        GameOver
    }

    /// <summary>
    /// Main game type.
    /// </summary>
    public class TicTacToe : Game
    {
        #region Fields


        Random random = new Random();

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Fonts
        SpriteFont textFont;
        SpriteFont buttonFont;

        Texture2D blankTexture, circleTexture, xTexture;
        Rectangle boardBounds;
        Rectangle[,] squareBounds;

        // Send button
        Rectangle sendMoveButtonPosition = new Rectangle(40, 290, 160, 40);
        const string sendMoveButtonText = "Send Move";
        Button sendMoveButton;

        // New game button
        Rectangle newGameButtonPosition = new Rectangle(40, 340, 160, 40);
        const string newGameButtonText = "New Game";
        Button newGameButton;

        // Exit button
        Rectangle exitButtonPosition = new Rectangle(40, 390, 160, 40);
        const string exitButtonText = "Exit";
        Button exitButtonButton;

        string text = "Retrieving Game State";

        bool canUserInput = false;

        // Game state
        TicTacToeState state = TicTacToeState.GameInitialize;
        TicTacToeMove currentMove;
        List<TicTacToeMove> moves = new List<TicTacToeMove>();

        // Constant strings
        private const string ChannelName = "MyChannel";
        private const string ServiceName = "TicTacToeServices.TicTacToeService";
        private const string SendingMoveText = "Sending Move...";
        private const string SendingAIMoveText = "Sending AI Move...";
        private const string MakeYourStepText = "Place your marker";
        private const string sessionFileName = "sessions.txt";
        private const string ServiceNotAvailableText = "Service Not Available";

        TicTacToeServiceClient proxy = new TicTacToeServiceClient();

        Guid playerID = Guid.NewGuid();
        Guid aiID = Guid.NewGuid();

        int gameLapsToDraw = 8;
        int gameLoopsCounter;
        bool needToDrawStep;

        const int rowCount = 3;
        const int columnCount = 3;
        const int squareWidth = 80;
        const int squareHeight = 80;
        const int boardWidth = 300;
        const int boardHeight = 300;
    

        HttpNotificationChannel channel = null;

        #endregion

        #region Initializations


        /// <summary>
        /// Creates a new game instance.
        /// </summary>
        public TicTacToe()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.IsFullScreen = true;

            // Enable tap gestures
            TouchPanel.EnabledGestures = GestureType.Tap;
        }

        

        /// <summary>
        /// Initialize the game.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Initialize the board position and size
            boardBounds = new Rectangle(GraphicsDevice.Viewport.Bounds.Center.X - boardWidth / 2,
                GraphicsDevice.Viewport.Bounds.Center.Y - boardHeight /2, boardWidth, boardHeight);

            // Initialize the board contents
            squareBounds = new Rectangle[rowCount, columnCount];
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    squareBounds[i, j] = new Rectangle(boardBounds.X + 10 + 100 * j, boardBounds.Y + 10 + 100 * i,
                        squareWidth, squareHeight);
                }
            }

            // Initialize the push notification channel
            InitializePushNotification(ChannelName, ServiceName);

            proxy.RestartGameCompleted += new EventHandler<AsyncCompletedEventArgs>(proxy_RestartGameCompleted);
            proxy.GetGameStateCompleted += new EventHandler<AsyncCompletedEventArgs>(proxy_GetGameStateCompleted);
            proxy.GameStepCompleted += new EventHandler<AsyncCompletedEventArgs>(proxy_GameStepCompleted);
        }




        #endregion

        #region Loading


        /// <summary>
        /// Load the game content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Loads assets
            blankTexture = Content.Load<Texture2D>("blank");
            circleTexture = Content.Load<Texture2D>("circle");
            xTexture = Content.Load<Texture2D>("x");
            textFont = Content.Load<SpriteFont>("TextFont");
            buttonFont = Content.Load<SpriteFont>("ButtonFont");

            // Initialize send button
            sendMoveButton = new Button(sendMoveButtonText, Color.White, buttonFont, sendMoveButtonPosition,
                Color.Black, blankTexture, spriteBatch);
            sendMoveButton.Click += new EventHandler(sendMoveButton_Click);

            // Initialize new game button
            newGameButton = new Button(newGameButtonText, Color.White, buttonFont, newGameButtonPosition,
                Color.Black, blankTexture, spriteBatch);
            newGameButton.Click += new EventHandler(newGameButton_Click);

            // Initialize exit button
            exitButtonButton = new Button(exitButtonText, Color.White, buttonFont, exitButtonPosition,
                Color.Black, blankTexture, spriteBatch);
            exitButtonButton.Click += new EventHandler(exitButtonButton_Click);
        }


        #endregion

        #region Update and Render


        /// <summary>
        /// Perform logic based on the current game state.
        /// </summary>
        /// <param name="gameTime">Time since this method was last called.</param>
        protected override void Update(GameTime gameTime)
        {

            // Allows the game to be closed using the back button
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }


            // Reads a gesture if one is available
            Rectangle? touchRect = null;
            GestureSample gs = new GestureSample();
            while (TouchPanel.IsGestureAvailable && canUserInput)
            {
                gs = TouchPanel.ReadGesture();
                touchRect = new Rectangle((int)gs.Position.X - 1, (int)gs.Position.Y - 1, 2, 2);
            }

            switch (state)
            {
                case TicTacToeState.GameInitialize:
                    if (random.Next(0, 10) > 5)
                    {
                        state = TicTacToeState.PlayerTurn;
                    }
                    else
                    {
                        state = TicTacToeState.AITurn;
                    }
                    break;
                case TicTacToeState.ServiceNotAvailable:
                    text = ServiceNotAvailableText;
                    break;
                case TicTacToeState.WaitingForService:

                    if (currentMove.Player == ConstData.XString)
                    {
                        text = SendingMoveText;
                    }
                    else
                    {
                        text = SendingAIMoveText;
                    }
                    break;
                case TicTacToeState.PlayerTurn:
                    // If a tap was performed, try to perform a corresponding move
                    if (touchRect.HasValue )
                    {
                        HandlePickMoveInput(touchRect.Value);
                        if(currentMove != null)
                        {
                            sendMoveButton.HandleInput(gs);
                        }
                    }
                    break;
                case TicTacToeState.AITurn:
                    text = "Waiting for AI Player...";
                    AIPlay();
                    break;
                case TicTacToeState.GameOver:
                    // Checks if one of the two buttons available at this state have been clicked
                    newGameButton.HandleInput(gs);
                    exitButtonButton.HandleInput(gs);
                    break;
                default:
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the game.
        /// </summary>
        /// <param name="gameTime">Time since this method was last called.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            DrawBoard();

            DrawMoves();

            DrawStateText();

            if (currentMove != null && state != TicTacToeState.ServiceNotAvailable)
            {
                // Draw the user's proposed move
                DrawCurrentMove(gameTime);

                if (state == TicTacToeState.PlayerTurn)
                {
                    // Draws the send move button
                    sendMoveButton.Draw();
                }
            }

            if (state == TicTacToeState.GameOver)
            {
                // Draws the new game and the exit buttons
                newGameButton.Draw();
                exitButtonButton.Draw();
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Helper Methods

        /// <summary>
        /// Simulate AI player by performing a random move.
        /// </summary>
        private void AIPlay()
        {            
            // Generate a random move
            TicTacToeMove move = new TicTacToeMove()
            {
                X = random.Next(0, rowCount),
                Y = random.Next(0, columnCount),
                Player = ConstData.OString
            };

            // Try generating another move if the first one was not legal
            while (!IsLegalMove(move))
            {
                move = new TicTacToeMove()
                {
                    X = random.Next(0, rowCount),
                    Y = random.Next(0, columnCount),
                    Player = ConstData.OString
                };
            }

            // Send the move to the server
            currentMove = move;
            try
            {
                proxy.GameStepAsync(playerID, currentMove.X, currentMove.Y, currentMove.Player);
                state = TicTacToeState.WaitingForService;
            }
            catch (Exception)
            {
                state = TicTacToeState.ServiceNotAvailable;
            }


        }

        /// <summary>
        /// Check if a move is legal.
        /// </summary>
        /// <param name="move">Move to validate.</param>
        /// <returns>True if the move is legal and false otherwise.</returns>
        private bool IsLegalMove(TicTacToeMove move)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                // Check if the square is occupied
                if (move.X == moves[i].X && move.Y == moves[i].Y)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Select a move to designate as the current move by the position of the user's tap.
        /// </summary>
        /// <param name="touchBounds">Rectangle representing the user's touch input location.</param>
        private void HandlePickMoveInput(Rectangle touchBounds)
        {
            // Get the square that was tapped
            Point? square = GetIntersectedSquare(touchBounds);

            if (square.HasValue)
            {
                // Create a move for the square that was tapped
                TicTacToeMove move = new TicTacToeMove()
                {
                    Player = ConstData.XString,
                    X = square.Value.X,
                    Y = square.Value.Y
                };

                // If the move is legal then set it as the current move
                if (IsLegalMove(move))
                {
                    currentMove = move;
                }
            }
        }

        /// <summary>
        /// Check which square was tapped on the board.
        /// </summary>
        /// <param name="touchBounds">Rectangle representing the user's touch input location.</param>
        /// <returns>The position of the square tapped, or null if none are tapped.</returns>
        private Point? GetIntersectedSquare(Rectangle touchBounds)
        {
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    if (squareBounds[i, j].Intersects(touchBounds))
                    {
                        return new Point(i, j);
                    }
                }
            }

            return null;
        }        

        /// <summary>
        /// Draw the state of the game at the bottom of the screen.
        /// </summary>
        private void DrawStateText()
        {
            Rectangle screenRectangle = GraphicsDevice.Viewport.Bounds;

            // Gets the size of the text
            Vector2 measure = textFont.MeasureString(text);

            // Calculate the proper position for the text at the bottom of the screen
            Vector2 textPosition = new Vector2(screenRectangle.Center.X - measure.X / 2f,
                screenRectangle.Bottom - measure.Y);

            spriteBatch.DrawString(textFont, text, textPosition, Color.Black);
        }

        /// <summary>
        /// Draw all move received from the server.
        /// </summary>
        private void DrawMoves()
        {
            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].Player == ConstData.XString)
                {
                    DrawX(moves[i].X, moves[i].Y);
                }
                else
                {
                    DrawO(moves[i].X, moves[i].Y);
                }
            }
        }

        /// <summary>
        /// Draw the current user move.
        /// </summary>
        private void DrawCurrentMove(GameTime gameTime)
        {
            // If the move was sent to the sever, the move should blink
            if (state == TicTacToeState.WaitingForService)
            {
                gameLoopsCounter++;

                // Every call we alternate whether or not the current move is drawn
                if (gameLoopsCounter == gameLapsToDraw)
                {
                    gameLoopsCounter = 0;
                    needToDrawStep = !needToDrawStep;
                }

                if (!needToDrawStep)
                {
                    return;
                }
            }

            if (currentMove.Player == ConstData.XString)
            {
                DrawX(currentMove.X, currentMove.Y);
            }
            else
            {
                DrawO(currentMove.X, currentMove.Y);
            }
        }

        /// <summary>
        /// Draw "O" in a specific position on the tic-tac-toe board.
        /// </summary>
        /// <param name="x">x-position on the board (0-2).</param>
        /// <param name="y">y-position on the board (0-2).</param>
        private void DrawO(int x, int y)
        {
            spriteBatch.Draw(circleTexture, squareBounds[x, y], Color.White);
        }

        /// <summary>
        /// Draw "X" in a specific position on the tic-tac-toe board.
        /// </summary>
        /// <param name="x">x-position on the board (0-2).</param>
        /// <param name="y">y-position on the board (0-2).</param>
        private void DrawX(int x, int y)
        {
            spriteBatch.Draw(xTexture, squareBounds[x, y], Color.Red);
        }

        /// <summary>
        /// Draw the tic-tac-toe board.
        /// </summary>
        private void DrawBoard()
        {
            // Draw the board's background
            spriteBatch.Draw(blankTexture, boardBounds, Color.Black);

            // Draw the board's squares
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    spriteBatch.Draw(blankTexture, squareBounds[i, j], Color.White);
                }
            }

        }

        /// <summary>
        /// Sets the client to a state equivalent to a supplied server state.
        /// </summary>
        /// <param name="serverState">Server state to set for the client.</param>
        private void ConvertServerStateToClientState(string serverState)
        {
            switch (serverState)
            {
                case "XPlayerTurn":
                    state = TicTacToeState.PlayerTurn;
                    text = MakeYourStepText;
                    break;
                case "OPlayerTurn":
                    state = TicTacToeState.AITurn;
                    text = "AI Player is playing";
                    break;
                case "XPlayerWin":
                    text = "Player X win";
                    state = TicTacToeState.GameOver;
                    break;
                case "OPlayerWin":
                    text = "Player O win";
                    state = TicTacToeState.GameOver;
                    break;
                case "Tie":
                    text = "Tie";
                    state = TicTacToeState.GameOver;
                    break;
            }
        }


        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs when the "exit" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void exitButtonButton_Click(object sender, EventArgs e)
        {
            this.Exit();
        }


        /// <summary>
        /// Occurs when the "new game" button clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void newGameButton_Click(object sender, EventArgs e)
        {
            if (state == TicTacToeState.GameOver)
            {
                // Instruct the server to restart the game
                try
                {
                    proxy.RestartGameAsync(playerID);

                    // Reset the local variables
                    moves = new List<TicTacToeMove>();
                    currentMove = null;
                    if (random.Next(0, 10) > 5)
                    {
                        state = TicTacToeState.PlayerTurn;
                    }
                    else
                    {
                        state = TicTacToeState.AITurn;
                    }
                    text = MakeYourStepText;
                }
                catch (Exception)
                {
                    state = TicTacToeState.ServiceNotAvailable;
                }
            }
        }


        /// <summary>
        /// Occurs when the "send move" button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sendMoveButton_Click(object sender, EventArgs e)
        {
            Guid guid = Guid.Empty;
            if (state == TicTacToeState.AITurn)
            {
                guid = aiID;
            }
            else
            {
                guid = playerID;
            }
            try
            {
                proxy.GameStepAsync(guid, currentMove.X, currentMove.Y, currentMove.Player);

                state = TicTacToeState.WaitingForService;
            }
            catch (Exception)
            {
                state = TicTacToeState.ServiceNotAvailable;
            }
        }

        #endregion

        #region Push Notifications


        /// <summary>
        /// Initialize the channel to the server and subscribe to events.
        /// </summary>
        /// <param name="channelName">The name of the channel used to access the service.</param>
        /// <param name="serviceName">The name of the service.</param>
        private void InitializePushNotification(string channelName, string serviceName)
        {
            // Check if the desired channel exists
            channel = HttpNotificationChannel.Find(channelName);
            if (channel == null)
            {
                // Create a new channel and open it
                channel = new HttpNotificationChannel(channelName, serviceName);
                channel.ChannelUriUpdated +=
                    new EventHandler<NotificationChannelUriEventArgs>(channel_ChannelUriUpdated);
                channel.HttpNotificationReceived +=
                    new EventHandler<HttpNotificationEventArgs>(channel_HttpNotificationReceived);
                channel.Open();
            }
            else
            {
                // Register the client using the existing channel
                channel.ChannelUriUpdated +=
                    new EventHandler<NotificationChannelUriEventArgs>(channel_ChannelUriUpdated);
                channel.HttpNotificationReceived +=
                    new EventHandler<HttpNotificationEventArgs>(channel_HttpNotificationReceived);
                RegisterClient(channel.ChannelUri);
            }

        }

        /// <summary>
        /// Handle the notifications from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void channel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            // Create an XmlReader from the stream supplied by the server
            XmlReader reader = XmlReader.Create(e.Notification.Body);

            // Read the stream into an object
            Message msg = new Message();
            msg.ReadXml(reader);

            // Handle the message according to its type
            switch (msg.ContentType)
            {
                case MessageContentType.GameState:
                    {
                        UpdateGameState(msg.Body as GameState);
                        break;
                    }
                case MessageContentType.TicTacToeMove:
                    {
                        NewMove(msg.Body as TicTacToeMove);
                        break;
                    }
            }

        }

        void proxy_GameStepCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                state = TicTacToeState.ServiceNotAvailable;
            }
        }

        void proxy_GetGameStateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                state = TicTacToeState.ServiceNotAvailable;
            }
        }

        void proxy_RestartGameCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                state = TicTacToeState.ServiceNotAvailable;
            }
        }



        /// <summary>
        /// Handle a message describing a move performed.
        /// </summary>
        /// <param name="ticTacToeMove">A move that was performed.</param>
        private void NewMove(TicTacToeMove ticTacToeMove)
        {
            moves.Add(ticTacToeMove);
            currentMove = null;
            ConvertServerStateToClientState(ticTacToeMove.GameFlow);
        }

        /// <summary>
        /// Handles messages describing the game state by reflecting it in the client.
        /// </summary>
        /// <param name="gameState">A GameState instance describing the game state.</param>
        private void UpdateGameState(GameState gameState)
        {
            // Goes over the entire board and creates a list of all the move. 
            // This list is used to render the board's current state.
            for (int i = 0; i < gameState.Board.Length; i++)
            {
                for (int j = 0; j < gameState.Board[i].Length; j++)
                {
                    string cellValue = gameState.Board[i][j];
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        moves.Add(new TicTacToeMove()
                        {
                            X = i,
                            Y = j,
                            Player = cellValue
                        });
                    }
                }
            }

            ConvertServerStateToClientState(gameState.CurrentState.ToString());

            text = MakeYourStepText;
            canUserInput = true;
        }

        /// <summary>
        /// When creating a new channel for push notification, its Uri updates.
        /// This event is raised when that occurs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void channel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            RegisterClient(e.ChannelUri);
            HttpNotificationChannel channel = (sender as HttpNotificationChannel);
            if (!channel.IsShellToastBound)
            {
                channel.BindToShellToast();
            }


        }

        /// <summary>
        /// Register the client to the server.
        /// </summary>
        /// <param name="channelUri">Uri supplied to the server for contacting the client.</param>
        private void RegisterClient(Uri channelUri)
        {
            proxy.RegisterCompleted += new EventHandler<RegisterCompletedEventArgs>(proxy_RegisterCompleted);
            try
            {
                // Register the client with the service and submit the callback Uri
                proxy.RegisterAsync(playerID, channelUri);
                proxy.RegisterAsync(aiID, channelUri);
            }
            catch (Exception)
            {
                state = TicTacToeState.ServiceNotAvailable;
            }
        }

        /// <summary>
        /// Called when the client successfully registers with the server. Retrieves the ongoing game data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void proxy_RegisterCompleted(object sender, RegisterCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                state = TicTacToeState.ServiceNotAvailable;
                return;
            }
            if (e.Result)
            {
                try
                {
                    proxy.GetGameStateAsync(playerID);
                }
                catch (Exception)
                {
                    state = TicTacToeState.ServiceNotAvailable;
                }
            }

        }


        #endregion
    }
}
