#region File Description
//-----------------------------------------------------------------------------
// YachtService.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ComponentModel.Composition;
using WindowsPhone.Recipes.Push.Messasges;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using Yacht;

#endregion

namespace YachtServices
{
    /// <summary>
    /// Implements a Yacht game server.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class YachtService : IYachtService
    {
        #region Fields

        static int PlayerIDCounter = 100;        
        private const string GameName = "Yacht";

        private readonly Dictionary<int, Subscription> subscribers = new Dictionary<int, Subscription>();
        private Dictionary<Guid, GameState> gameStates = new Dictionary<Guid, GameState>();
        private readonly object SubscribersSync = new object();
        public delegate void CheckNextPlayerAsync(Guid gameID, int playerIndex);

        #endregion


        #region Public Methods
        /// <summary>
        /// Register new player with the server.
        /// </summary>        
        /// <param name="clientURI">The Uri to use to send push notifications to the client.</param>
        /// <param name="name">The player's name.</param>
        /// <param name="playerID">The client identifier.</param>
        /// <returns>True if the client is successfully registered with the sever and false otherwise.</returns>
        public int Register(Uri clientURI, string name, int playerID)
        {
            int playerIDByserver = -1;
            if (subscribers.ContainsKey(playerID) && subscribers[playerID].Name == name)
            {
                subscribers[playerID].ChannelUri = clientURI;
                playerIDByserver = playerID;
            }
            else if (clientURI != null)
            {
                Subscription subscription = new Subscription(PlayerIDCounter++, clientURI, name);

                lock (SubscribersSync)
                {
                    subscribers[subscription.SessionID] = subscription;
                }

                playerIDByserver = subscription.SessionID;
            }

            return playerIDByserver;
        }

        /// <summary>
        /// Unregister a client from the server.
        /// </summary>
        /// <param name="sessionID">The client's session ID.</param>
        public void Unregister(int sessionID)
        {
            if (subscribers.ContainsKey(sessionID))
            {
                LeaveGame(sessionID);

                lock (SubscribersSync)
                {
                    subscribers.Remove(sessionID);
                }
            }
        }

        /// <summary>
        /// Join to an existing game on the server.
        /// </summary>
        /// <param name="sessionID">The session id to identify the player joining the game.</param>
        /// <param name="gameID">The ID of the game to join.</param>
        /// <returns>True if the game was successfully joined and false otherwise.</returns>
        public bool JoinGame(Guid gameID, int sessionID)
        {
            if (gameStates.ContainsKey(gameID))
            {
                // Get the game's state
                GameState game = gameStates[gameID];

                if (subscribers[sessionID].GameID != gameID)
                {
                    // Try to find an empty slot for the player to join (the player will replace an AI player)
                    for (int i = 0; i < gameStates[gameID].Players.Count; i++)
                    {
                        if (game.Players[i].AIPlayer != null)
                        {
                            // Join to game
                            subscribers[sessionID].GameID = gameID;
                            game.Players[i].Name = subscribers[sessionID].Name;
                            game.Players[i].AIPlayer = null;
                            game.Players[i].PlayerID = sessionID;

                            byte[] bytes = GetBytes<GameState>(game);

                            // Notify all clients that a new player joined the game
                            NotifyUpdate(gameID, sessionID, bytes, ServiceConstants.JoinMessageString);

                            return true;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Remove a player from the game he is currently part of.
        /// </summary>
        /// <param name="sessionID">The session id to identify the player joining the game.</param>
        /// <returns>True if the player was successfully removed from his game and false otherwise.</returns>
        public bool LeaveGame(int sessionID)
        {
            if (gameStates.ContainsKey(subscribers[sessionID].GameID))
            {
                // Get the game state
                GameState game = gameStates[subscribers[sessionID].GameID];
                subscribers[sessionID].GameID = Guid.Empty;
                for (int i = 0; i < game.Players.Count; i++)
                {
                    if (game.Players[i].PlayerID == sessionID)
                    {
                        // Replace the player with an AI player
                        game.Players[i].Name = ServiceConstants.AIMessageString + i.ToString();
                        game.Players[i].PlayerID = -1;
                        game.Players[i].AIPlayer = new Yacht.AIPlayerBehavior();

                        if (!IsHumanExists(game))
                        {
                            gameStates.Remove(game.GameID);
                        }
                        else
                        {
                            byte[] bytes = GetBytes<GameState>(game);

                            // Notify all other clients that the player has left the game
                            NotifyUpdate(game.GameID, sessionID, bytes, ServiceConstants.LeftMessageString);
                            HandlePlayerStep(game.GameID, i);
                            return true;
                        }

                        break;
                    }
                }
            }
            else
            {
                subscribers.Remove(sessionID);
            }
            return false;
        }

        /// <summary>
        /// The method gives the server information about a step that the client has done.
        /// </summary>
        /// <param name="gameID">The game identifier.</param>
        /// <param name="sessionID">the client's identifier.</param>
        /// <param name="scoreIndex">The cell index of the score card.</param>
        /// <param name="scoreValue">The value of the score.</param>
        /// <param name="playerIndex">The index of player in the players list.</param>
        /// <param name="step">How many step have been made.</param>
        public void GameStep(Guid gameID, int sessionID, int scoreIndex, byte scoreValue, int playerIndex, int step)
        {
            if (gameStates.ContainsKey(gameID))
            {
                // Get the game state.
                GameState game = gameStates[gameID];

                // Get the player information
                PlayerInformation player = game.Players[playerIndex];


                if (subscribers[sessionID].GameID == game.GameID)
                {
                    YachtStep currentStep = new YachtStep(scoreIndex, scoreValue, playerIndex, step);

                    MakeStep(gameID, currentStep);
                }

                if (gameStates[gameID].TimerToDelete != null)
                {
                    gameStates[gameID].TimerToDelete.Dispose();
                    gameStates[gameID].TimerToDelete = null;
                }
            }
        }



        /// <summary>
        /// Returns the current state of a specified game.
        /// </summary>
        /// <param name="sessionID">The session ID of the player requesting the game state.</param>
        /// <param name="gameID">The ID of the game for which to return the state.</param>
        /// <returns>A byte array containing the serialized game state.</returns>
        public byte[] GetGameState(Guid gameID, int sessionID)
        {
            if (subscribers.ContainsKey(sessionID) && subscribers[sessionID].GameID == gameID &&
                gameStates.ContainsKey(gameID))
            {
                GameState game = gameStates[gameID];
                PlayerInformation player = game.Players[game.CurrentPlayer];
                if (player.PlayerID == sessionID && game.IsStarted)
                {
                    ResetTimeout(gameID, sessionID);
                }

                byte[] bytes = GetBytes<GameState>(game);
                return bytes;
            }

            return null;
        }

        /// <summary>
        /// Create a new game on the server.
        /// </summary>
        /// <param name="sessionID">The session ID of the player creating the game.</param>
        /// <param name="name">The name of the new game.</param>
        /// <returns>Returns the state of the new game or null if game creation failed.</returns>
        public byte[] NewGame(int sessionID, string name)
        {
            if (subscribers.ContainsKey(sessionID) && CheckUniqueGameName(name))
            {
                Guid gameID = Guid.NewGuid();
                gameStates.Add(gameID, new GameState() { Name = name, GameID = gameID, GameType = GameTypes.Online,
                    SequenceNumber = 0});

                subscribers[sessionID].GameID = gameID;

                // Creates the human player (the player who created the game)
                gameStates[gameID].Players.Add(new PlayerInformation()
                {
                    Name = subscribers[sessionID].Name,
                    ScoreCard = NewScoreCard(),
                    PlayerID = sessionID,
                    Timer = new Timer(PlayAI, new object[] { gameID, 0 }, Timeout.Infinite, Timeout.Infinite)
                });

                // Creates additional AI players to fill the game.
                for (int i = 1; i < 4; i++)
                {
                    gameStates[gameID].Players.Add(new PlayerInformation()
                    {
                        Name = ServiceConstants.AIMessageString + i.ToString(),
                        ScoreCard = NewScoreCard(),
                        AIPlayer = new Yacht.AIPlayerBehavior(),
                    });
                }

                gameStates[gameID].TimerToDelete = new Timer(DeleteGame, gameID, 120000, -1);

                return gameID.ToByteArray();
            }

            return null;
        }

        /// <summary>
        /// Returns all games available on the server.        
        /// </summary>
        /// <param name="sessionID">Session ID of the player requesting the current list of games.</param>
        /// <returns>A byte array containing the serialized game list.</returns>
        public byte[] GetAvailableGames(int sessionID)
        {
            byte[] bytes = GetBytes<AvailableGames>(new AvailableGames());

            if (subscribers.ContainsKey(sessionID) && gameStates.Count > 0)
            {
                AvailableGames availableGames = new AvailableGames();

                foreach (var game in gameStates)
                {
                    availableGames.Games.Add(new GameInformation()
                    {
                        GameID = game.Key,
                        Name = game.Value.Name
                    });
                }

                bytes = GetBytes<AvailableGames>(availableGames);
            }

            return bytes;
        }

        /// <summary>
        /// Reset the timeout used to deem a player inactive.
        /// </summary>
        /// <param name="sessionID">Session ID of the player notifying the server of activity.</param>


        /// <param name="gameID">The ID of the game to update with player activity.</param>
        public void ResetTimeout(Guid gameID, int sessionID)
        {
            if (gameStates.ContainsKey(gameID))
            {
                GameState game = gameStates[gameID];
                PlayerInformation player = game.Players[game.CurrentPlayer];
                if (subscribers.ContainsKey(sessionID) &&
                    player.PlayerID == sessionID)
                {
                    if (!game.IsStarted)
                    {
                        game.IsStarted = true;
                    }
                    if (player.Timer != null)
                    {
                        player.ToastMessageSent = false;
                        player.Timer.Change(25000, -1);
                    }
                }
            }
        }




        /// <summary>
        /// Gets a specific player's score card.
        /// </summary>
        /// <param name="sessionID">Session ID of the player for which to get the score card.</param>
        /// <returns>The requested player score card.</returns>
        public byte[] GetScoreCard(int sessionID)
        {
            Guid gameID = subscribers[sessionID].GameID;
            if (subscribers.ContainsKey(sessionID) && gameStates.ContainsKey(gameID))
            {
                for (int i = 0; i < gameStates[gameID].Players.Count; i++)
                {
                    PlayerInformation playerInfo = gameStates[gameID].Players[i];
                    if (playerInfo.PlayerID == sessionID)
                    {
                        return playerInfo.ScoreCard;
                    }
                }
            }

            return null;
        }


        #endregion

        #region Private Methods


        /// <summary>
        /// Remove the game from the games state
        /// </summary>
        /// <param name="gameID">the game id to remove</param>
        private void DeleteGame(object gameID)
        {
            Guid gameIdentifier = (Guid)gameID;
            if (gameStates.ContainsKey(gameIdentifier))
            {
                byte[] bytes = GetBytes<SimpleType>(new SimpleType()
                {
                    Name = ServiceConstants.BannedMessageString
                });

                foreach (Subscription client in subscribers.Values)
                {
                    if (client.GameID == gameIdentifier)
                    {
                        SendRawMessage(client.ChannelUri, bytes);
                    }
                }

                
                gameStates.Remove(gameIdentifier);
            }
        }

        /// <summary>
        /// Checks if a game name is already in use.
        /// </summary>
        /// <param name="name">The game name to check.</param>
        /// <returns>True if the name is not in use and false otherwise.</returns>
        private bool CheckUniqueGameName(string name)
        {
            foreach (var game in gameStates)
            {
                if (game.Value.Name == name)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if all players in a specific game are AI players.
        /// </summary>
        /// <param name="game">The game state to check.</param>
        /// <returns>True if all the players are AI players and false otherwise.</returns>
        private bool IsHumanExists(GameState game)
        {
            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i].AIPlayer == null)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Send a notification message to all connected client, optionally skipping a specific client.
        /// </summary>
        /// <param name="gameID">The ID of the game from which the message originates.</param>
        /// <param name="sessionID">Session ID of a player to which the message should not be sent, or null to send
        /// the message to all players.</param>
        /// <param name="raw">Byte array containing the XML serialized message to send.</param>
        /// <param name="toast">The title of the toast message to be displayed at the client side.</param>
        private void NotifyUpdate(Guid gameID, int? sessionID, byte[] raw, string toast)
        {
            lock (SubscribersSync)
            {
                foreach (var subscriber in subscribers)
                {
                    if (subscriber.Value.GameID == gameID &&
                        (sessionID == null || subscriber.Value.SessionID != sessionID))
                    {
                        PlayerInformation player = GetPlayer(subscriber.Value);
                        SendRawMessage(subscriber.Value.ChannelUri, raw);
                    }
                }
            }
        }


        private PlayerInformation GetPlayer(Subscription sub)
        {
            if (gameStates.ContainsKey(sub.GameID))
            {
                for (int i = 0; i < gameStates[sub.GameID].Players.Count; i++)
                {
                    if (gameStates[sub.GameID].Players[i].PlayerID == sub.SessionID)
                    {
                        return gameStates[sub.GameID].Players[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a specific game has ended.
        /// </summary>
        /// <param name="gameID">The ID of the game to check.</param>
        private void CheckIfGameEnded(Guid gameID)
        {
            if (gameStates.ContainsKey(gameID))
            {
                GameState game = gameStates[gameID];

                // When all players perform twelve steps then the game ends
                if (game.StepsMade == 12 * game.Players.Count)
                {
                    PlayerInformation player = GetWinner(game);
                    byte[] bytes = GetBytes<EndGameInformation>(new EndGameInformation()
                    {
                        PlayerID = player.PlayerID,
                        ScoreCard = player.ScoreCard
                    });
                    NotifyUpdate(gameID, null, bytes, ServiceConstants.GameOverMessageString);
                    gameStates.Remove(gameID);
                }
            }
        }

        /// <summary>
        /// Updates a game according to a step performed.
        /// </summary>
        /// <param name="gameID">ID of the game to update.</param>
        /// <param name="step">The step to perform in the game.</param>
        private void MakeStep(Guid gameID, YachtStep step)
        {
            if (gameStates.ContainsKey(gameID))
            {
                GameState game = gameStates[gameID];
                PlayerInformation player = game.Players[step.PlayerIndex];

                // Check if the step is valid
                if (IsStepValid(game, step.PlayerIndex, step.StepNumber))
                {
                    // Reset the timer for deeming a player inactive
                    if (player.Timer != null)
                    {
                        player.Timer.Change(Timeout.Infinite, Timeout.Infinite);
                        if (player.AIPlayer == null)
                        {
                            player.TimeOutsCounter = 0;
                        }
                    }

                    MakeMove(gameID, step);
                    byte[] bytes = GetBytes<GameState>(gameStates[gameID]);

                    NotifyUpdate(gameID, null, bytes, ServiceConstants.GameStateMessageString);

                    PlayerInformation previousPlayer;

                    if (step.PlayerIndex == 0)
                    {
                        previousPlayer = game.Players[3];
                    }
                    else
                    {
                        previousPlayer = game.Players[step.PlayerIndex - 1];
                    }

                    if (previousPlayer.TimeOutsCounter == 1)
                    {
                        sendToastMessage(subscribers[previousPlayer.PlayerID].ChannelUri,
                            "Yacht - Network player made his step");
                    }

                    CheckIfGameEnded(gameID);

                    CheckNextPlayerAsync checkNextPlayerDelegate = HandlePlayerStep;
                    checkNextPlayerDelegate.BeginInvoke(gameID, game.CurrentPlayer, null, null);
                }
            }
        }

        /// <summary>
        /// Handle game step for both AI and client
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="playerIndex"></param>
        private void HandlePlayerStep(Guid gameID, int playerIndex)
        {
            if (gameStates.ContainsKey(gameID))
            {
                GameState game = gameStates[gameID];
                PlayerInformation player = game.Players[playerIndex];

                if (game.CurrentPlayer == playerIndex && game.StepsMade < 12 * game.Players.Count)
                {
                    // If ai player makes a move
                    if (player.AIPlayer != null)
                    {
                        YachtStep step = player.AIPlayer.Play(player.ScoreCard);
                        step.PlayerIndex = playerIndex;
                        step.StepNumber = game.StepsMade;
                        MakeStep(gameID, step);
                    }
                    else
                    {
                        // If client reset or create the timer
                        if (player.Timer != null)
                        {
                            player.Timer.Change(25000, -1);
                        }
                        else
                        {
                            player.Timer = new Timer(PlayAI, new object[] { gameID, playerIndex }, 25000, -1);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void PlayAI(object obj)
        {

            object[] param = (object[])obj;
            Guid gameID = (Guid)param[0];
            int playerIndex = (int)param[1];

            if (gameStates.ContainsKey(gameID))
            {
                GameState game = gameStates[gameID];
                PlayerInformation player = game.Players[playerIndex];

                if (game.StepsMade < 12 * game.Players.Count)
                {
                    player.TimeOutsCounter++;
                    player.AIPlayer = new Yacht.AIPlayerBehavior();

                    YachtStep step = player.AIPlayer.Play(player.ScoreCard);
                    step.PlayerIndex = playerIndex;
                    step.StepNumber = game.StepsMade;

                    MakeStep(gameID, step);

                    player.AIPlayer = null;

                    CheckForInactivePlayer(gameID, playerIndex, player);
                }
            }
        }

        private  PlayerInformation GetWinner(GameState game)
        {
            int winnerPlayerIndex = 0;
            for (int i = 1; i < game.Players.Count; i++)
            {
                if (game.Players[i].TotalScore > game.Players[winnerPlayerIndex].TotalScore)
                {
                    winnerPlayerIndex = i;
                }
            }

            return game.Players[winnerPlayerIndex];
        }

        /// <summary>
        /// Checks if a player needs to be kicked from the game forcefully.
        /// </summary>
        /// <param name="gameID">The game identifier.</param>
        /// <param name="playerIndex">Index of the player to possibly ban.</param>
        /// <param name="playerInformation">Information belonging to the player who will possibly be banned.</param>
        private void CheckForInactivePlayer(Guid gameID, int playerIndex, PlayerInformation playerInformation)
        {
            if (playerInformation.TimeOutsCounter == 3)
            {
                byte[] bytes = GetBytes<SimpleType>(new SimpleType()
                {
                    Name = ServiceConstants.BannedMessageString
                });

                SendRawMessage(subscribers[playerInformation.PlayerID].ChannelUri, bytes);

                playerInformation.Name = ServiceConstants.AIMessageString + playerIndex.ToString();
                playerInformation.TimeOutsCounter = 0;
                playerInformation.AIPlayer = new Yacht.AIPlayerBehavior();

                lock (SubscribersSync)
                {
                    subscribers.Remove(playerInformation.PlayerID);
                }

                if (!IsHumanExists(gameStates[gameID]))
                {
                    gameStates.Remove(gameID);
                }
                else
                {
                    bytes = GetBytes<GameState>(gameStates[gameID]);

                    NotifyUpdate(gameID, playerInformation.PlayerID, bytes, ServiceConstants.BannedMessageString);
                }

                playerInformation.PlayerID = -1;
            }
        }



        /// <summary>
        /// Makes a move in the game.
        /// </summary>
        /// <param name="gameID">The game identifier.</param>
        /// <param name="move">The move to apply on the game</param>
        private void MakeMove(Guid gameID, YachtStep move)
        {
            if (gameStates.ContainsKey(gameID))
            {
                GameState game = gameStates[gameID];
                PlayerInformation player = game.Players[move.PlayerIndex];

                player.ScoreCard[move.ScoreLine] = move.Score;
                player.TotalScore += move.Score;

                game.CurrentPlayer = (game.CurrentPlayer + 1) % game.Players.Count;

                game.StepsMade++;
            }
        }

        /// <summary>
        /// Takes a serializable object and returns a message which has the object as its body.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A <see cref="Yacht.Message"/> containing <paramref name="obj"/> as its body.</returns>
        private byte[] GetBytes<T>(T obj) where T : IXmlSerializable
        {
            XmlSerializer messageSerializer = new XmlSerializer(typeof(Message));

            // Creates the message
            Message message = new Message();
            message.ContentType = (MessageContentType)Enum.Parse(typeof(MessageContentType), obj.GetType().Name);
            if (obj is GameState)
            {
                message.SequenceNumber = (obj as GameState).SequenceNumber++;
            }
            else
            {
                message.SequenceNumber = -1;
            }
            message.Body = obj;

            using (MemoryStream outputStream = new MemoryStream())
            {
                // Serializes the message
                using (XmlWriter writer =
                    XmlWriter.Create(outputStream, new XmlWriterSettings() { Encoding = Encoding.UTF8 }))
                {
                    messageSerializer.Serialize(writer, message);

                    return outputStream.ToArray();
                }
            }
        }



        /// <summary>
        /// Send a raw notification message to the client.
        /// </summary>
        /// <param name="clientURI">Client Uri.</param>
        /// <param name="rawData">The data to send.</param>
        /// <returns>The message send operation result.</returns>
        private static void SendRawMessage(Uri clientURI, byte[] rawData)
        {
            RawPushNotificationMessage message = new RawPushNotificationMessage(MessageSendPriority.High);
            message.RawData = rawData;
            message.SendAsync(clientURI);
        }


        private static void sendToastMessage(Uri clientURI, string toastTitle)
        {
            ToastPushNotificationMessage toastMessage = new ToastPushNotificationMessage(MessageSendPriority.High);
            toastMessage.Title = toastTitle;
            toastMessage.SendAsync(clientURI);
        }


        /// <summary>
        /// Creates a new score card.
        /// </summary>
        /// <returns>A byte array which represents an empty score card.</returns>
        private byte[] NewScoreCard()
        {
            byte[] score = new byte[12];
            for (int i = 0; i < score.Length; i++)
            {
                score[i] = ServiceConstants.NullScore;
            }
            return score;
        }

        /// <summary>
        /// Check if a the game step is valid.
        /// </summary>
        /// <param name="game">The game state that the step is preformed on.</param>
        /// <param name="playerIndex">The index of the play who made the step.</param>
        /// <param name="step">The amount of game steps made</param>
        /// <returns>True if the step is valid, otherwise returns false.</returns>
        private bool IsStepValid(GameState game, int playerIndex, int step)
        {
            return game.CurrentPlayer == playerIndex &&
                    game.StepsMade < 12 * game.Players.Count &&
                    game.StepsMade == step;
        }
        #endregion
    }
}
