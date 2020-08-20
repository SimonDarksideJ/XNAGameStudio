#region File Description
//-----------------------------------------------------------------------------
// NetworkManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Phone.Notification;
using Yacht.YachtServiceReference;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;
using System.Globalization;
using YachtServices;
using System.ComponentModel;
using System.Xml.Schema;


#endregion

namespace Yacht
{

    public class NetworkManager : IXmlSerializable, IDisposable
    {
        #region Singleton


        /// <summary>
        /// The singleton NetworkManager instance.
        /// </summary>
        public static NetworkManager Instance { get; private set; }

        /// <summary>
        /// Create the singleton NetworkManager instance.
        /// </summary>
        static NetworkManager()
        {
            try
            {

                // Try to initialize new instance of NetworkManager
                Instance = new NetworkManager("OneTimePatternChannel", "YachtServices.YachtService");
            }
            catch (Exception)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Initialize a new network manager.
        /// </summary>
        /// <param name="channelName">The name the application uses to identify the notification channel 
        /// instance used to communicate with the game server.</param>
        /// <param name="serviceName">The name that the game's server uses to associate itself with the Push 
        /// Notification Service.</param>
        private NetworkManager(string channelName, string serviceName)
        {
            this.channelName = channelName;
            this.serviceName = serviceName;
        }


        #endregion

        #region Fields Properties and Events


        /// <summary>
        /// Address of the game's server.
        /// </summary>
        public string ServerAddress
        {
            get
            {
                return proxy != null ? proxy.Endpoint.Address.ToString() : null;
            }
        }

        HttpNotificationChannel channel;
        YachtServiceClient proxy;
        public int playerID = -1;
        public Guid gameID;
        string channelName;
        string serviceName;
        public string name;
        public string gameName;
        int lastMessageSequenceNumber;

        public event EventHandler<BooleanEventArgs> Registered;
        public event EventHandler<BooleanEventArgs> JoinedGame;
        public event EventHandler<BooleanEventArgs> LeftGame;
        public event EventHandler<BooleanEventArgs> NewGameCreated;
        public event EventHandler<YachtScoreCardEventArgs> ScoreCardArrived;
        public event EventHandler<YachtGameStateEventArgs> GameStateArrived;
        public event EventHandler<YachtAvailableGamesEventArgs> AvailableGamesArrived;
        public event EventHandler Banned;
        public event EventHandler GameUnavailable;
        public event EventHandler<YachtGameOverEventArgs> GameOver;

        /// <summary>
        /// Event fired when there is an error contacting the game server.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ServiceError;

        #endregion

        #region Public Methods
        /// <summary>
        /// Connect to the game server and open a push notification channel.
        /// </summary>
        /// <param name="name">The player name to use when connecting to the server.</param>
        public void Connect(string name)
        {
            this.name = name;
            
            // Create a proxy to the server
            proxy = new YachtServiceClient();

            // Register to server communication events
            proxy.RegisterCompleted += proxy_RegisterCompleted;
            proxy.JoinGameCompleted += proxy_JoinGameCompleted;
            proxy.LeaveGameCompleted += proxy_LeaveGameCompleted;
            proxy.GetGameStateCompleted += proxy_GetGameStateCompleted;
            proxy.GetAvailableGamesCompleted += proxy_GetAvailableGamesCompleted;
            proxy.NewGameCompleted += proxy_NewGameCompleted;
            proxy.GetScoreCardCompleted += proxy_GetScoreCardCompleted;
            proxy.UnregisterCompleted += proxy_ErrorChecked;
            proxy.ResetTimeoutCompleted += proxy_ErrorChecked;
            proxy.GameStepCompleted += proxy_ErrorChecked;

            // Create the push notification channel
            InitializePushNotification(channelName, serviceName);
        }

        /// <summary>
        /// Unregister from the server.
        /// </summary>
        public void Unregister()
        {
            gameID = Guid.Empty;
            name = null;
            gameName = null;
            if (proxy != null)
            {
                proxy.UnregisterAsync(playerID);
            }
        }

        /// <summary>
        /// Leave the current game.
        /// </summary>
        public void LeaveGame()
        {
            gameID = Guid.Empty;
            gameName = null;
            proxy.LeaveGameAsync(playerID);
        }

        /// <summary>
        /// Send a game step to the server.
        /// </summary>
        /// <param name="step">The step that was performed in the game.</param>
        public void GameStep(YachtStep step)
        {
            proxy.GameStepAsync(gameID, playerID, step.ScoreLine, step.Score, step.PlayerIndex, step.StepNumber);
        }

        /// <summary>
        /// Join a specific game.
        /// </summary>
        /// <param name="gameID">The ID of the game to join.</param>
        public void JoinGame(Guid gameID)
        {
            this.gameID = gameID;
            proxy.JoinGameAsync(gameID, playerID);
        }

        /// <summary>
        /// Get the full state of the game from the server.
        /// </summary>
        public void GetGameState()
        {
            proxy.GetGameStateAsync(gameID, playerID);
        }

        /// <summary>
        /// Create a new game on the server.
        /// </summary>
        /// <param name="name">The name to give the game on the server.</param>
        public void NewGame(string name)
        {
            gameName = name;
            proxy.NewGameAsync(playerID, name);
        }

        /// <summary>
        /// Get the available games from the server.
        /// </summary>
        public void GetAvailableGames()
        {
            proxy.GetAvailableGamesAsync(playerID);
        }

        /// <summary>
        /// Reset the time out for the current player turn.
        /// </summary>
        public void ResetTimeout()
        {
            if (proxy != null)
            {
                proxy.ResetTimeoutAsync(gameID, playerID);
            }
        }

        /// <summary>
        /// Get the score card from the server.
        /// </summary>
        public void GetScoreCard()
        {
            proxy.GetScoreCardAsync(playerID);
        }

        /// <summary>
        /// Closes the push notification channel.
        /// </summary>
        public void StopListeningToPushNotification()
        {
            if (channel != null && channel.ChannelUri != null)
            {
                channel.Close();
            }
        }

        /// <summary>
        /// Reads a serialized NetworkManager object. Unlike <see cref="ReadXml"/>, the reader is assumed to have
        /// already read the node which contains the start element of the network manager's representation. This will
        /// move the reader past the representation's end element.
        /// </summary>
        /// <param name="reader">The xml reader from which to read.</param>
        public void Deserialize(XmlReader reader)
        {
            gameID = new Guid(reader.GetAttribute("GameID"));
            int.TryParse(reader.GetAttribute("PlayerID"), out playerID);
            name = reader.GetAttribute("Name");
            gameName = reader.GetAttribute("GameName");

            // Read end element
            reader.Read();
        }

        #endregion

        #region Private Methods


        /// <summary>
        /// Reads a message received from the server.
        /// </summary>
        /// <param name="stream">Stream containing the message.</param>
        /// <returns>The message received from the server.</returns>
        private static Message ReadServerMessage(Stream stream)
        {
            // Create an XmlReader from the data supplied by the server
            using (XmlReader reader = XmlReader.Create(stream))
            {
                // Read past xml declaration
                reader.Read();

                // Read the wrapper node created by xml serialization
                reader.Read();

                // Read the data into an object
                Message message = new Message();
                message.ReadXml(reader);
                return message;
            }
        }

        /// <summary>
        /// Initialize the notification channel used to receive data from the game server.
        /// </summary>
        /// <param name="channelName">The name the application uses to identify the notification channel 
        /// instance used to communicate with the game server.</param>
        /// <param name="serviceName">The name that the game's server uses to associate itself with the Push 
        /// Notification Service.</param>
        private void InitializePushNotification(string channelName, string serviceName)
        {
            try
            {
                // Look for an already existing channel
                channel = HttpNotificationChannel.Find(channelName);
                if (channel == null)
                {
                    // Create a new channel and open it
                    channel = new HttpNotificationChannel(channelName, serviceName);
                    channel.ChannelUriUpdated += channel_ChannelUriUpdated;
                    channel.HttpNotificationReceived += channel_HttpNotificationReceived;
                    channel.Open();
                }
                else
                {
                    // Register the client using the existing channel
                    channel.ChannelUriUpdated += channel_ChannelUriUpdated;
                    channel.HttpNotificationReceived += channel_HttpNotificationReceived;
                    RegisterForPushNotifications();
                }
            }
            catch (Exception e)
            {
                if (ServiceError != null)
                {
                    ServiceError(this, new ExceptionEventArgs() { Error = e });
                }
            }
        }

        /// <summary>
        /// Parse a simple type message.
        /// </summary>
        /// <param name="simpleType">The simple type message container.</param>
        private void NewSimpleType(SimpleType simpleType)
        {
            // Parse the message according to its name
            switch (simpleType.Name)
            {
                case ServiceConstants.BannedMessageString:
                    NewBanned();
                    break;
                default:

                    break;
            }
        }

        /// <summary>
        /// Raise the "Banned" event.
        /// </summary>
        /// <param name="name">The name of the player that was banned.</param>
        private void NewBanned()
        {
            if (Banned != null)
            {
                Banned(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raise the "AvailableGamesArrived" event.
        /// </summary>
        /// <param name="availableGames">The available games as received from the server.</param>
        private void NewAvailableGames(AvailableGames availableGames)
        {
            if (AvailableGamesArrived != null)
            {
                AvailableGamesArrived(this, new YachtAvailableGamesEventArgs() { AvailableGames = availableGames });
            }
        }

        /// <summary>
        /// Raise the "GameStateArrived" event.
        /// </summary>
        /// <param name="gameState">The game state data that arrived.</param>
        private void NewGameState(GameState gameState)
        {
            if (GameStateArrived != null)
            {
                GameStateArrived(this, new YachtGameStateEventArgs() { GameState = gameState });
            }
        }

        /// <summary>
        /// Register for push notifications with the server.
        /// </summary>
        private void RegisterForPushNotifications()
        {
            proxy.RegisterAsync(channel.ChannelUri, name, playerID);
        }

        /// <summary>
        /// Raise the "GameOver" event.
        /// </summary>
        /// <param name="endGameState">The winning player's details.</param>
        private void NewEndGameState(EndGameInformation endGameState)
        {
            if (GameOver != null)
            {
                GameOver(this, new YachtGameOverEventArgs() { EndGameState = endGameState });
            }
        }


        #endregion

        #region Event Handlers


        /// <summary>
        /// Handler for push notification channel Uri updates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void channel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            RegisterForPushNotifications();

            if (!(sender as HttpNotificationChannel).IsShellToastBound)
            {
                (sender as HttpNotificationChannel).BindToShellToast();
            }
        }

        /// <summary>
        /// Handle push notification sent from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void channel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {                        
            // Reads the stream into an object
            Message message = ReadServerMessage(e.Notification.Body);

            if (message.SequenceNumber > lastMessageSequenceNumber)
            {
                lastMessageSequenceNumber = message.SequenceNumber;
            }            
            else if (message.SequenceNumber != -1)
            {
                return;
            }

            // Parse the message body according to its type
            switch (message.ContentType)
            {
                case MessageContentType.GameState:
                    NewGameState((message.Body as GameState));
                    break;
                case MessageContentType.SimpleType:
                    NewSimpleType((message.Body as SimpleType));
                    break;
                case MessageContentType.EndGameInformation:
                    NewEndGameState((message.Body as EndGameInformation));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handler called after receiving the list of available games from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void proxy_GetAvailableGamesCompleted(object sender, GetAvailableGamesCompletedEventArgs e)
        {
            if (e.Error != null && ServiceError != null)
            {
                // Raise an error notification
                ServiceError(this, new ExceptionEventArgs() { Error = e.Error });
            }
            else if (e.Result != null)
            {
                MemoryStream memoryStream = new MemoryStream(e.Result);

                Message message = ReadServerMessage(memoryStream);

                NewAvailableGames((message.Body as AvailableGames));
            }
        }        

        /// <summary>
        /// Handler called after receiving a requested score card from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void proxy_GetScoreCardCompleted(object sender, GetScoreCardCompletedEventArgs e)
        {
            if (e.Error != null && ServiceError != null)
            {
                // Raise an error notification
                ServiceError(this, new ExceptionEventArgs() { Error = e.Error });
            }
            else if (ScoreCardArrived != null)
            {
                ScoreCardArrived(this, new YachtScoreCardEventArgs() { ScoreCard = e.Result });
            }
        }

        /// <summary>
        /// Handler called after receiving the current game state from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void proxy_GetGameStateCompleted(object sender, GetGameStateCompletedEventArgs e)
        {
            if (e.Error != null && ServiceError != null)
            {
                // Raise an error notification
                ServiceError(this, new ExceptionEventArgs() { Error = e.Error });
            }
            else if (e.Result != null)
            {
                MemoryStream memoryStream = new MemoryStream(e.Result);

                Message message = ReadServerMessage(memoryStream);

                NewGameState((message.Body as GameState));
            }
            else if (GameUnavailable != null)
            {
                GameUnavailable(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handler called after creating a new game on the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void proxy_NewGameCompleted(object sender, NewGameCompletedEventArgs e)
        {
            if (e.Error != null && ServiceError != null)
            {
                // Raise an error notification
                ServiceError(this, new ExceptionEventArgs() { Error = e.Error });
            }
            else if (NewGameCreated != null)
            {
                if (e.Result != null)
                {
                    gameID = new Guid(e.Result);                    
                }

                lastMessageSequenceNumber = -1;

                NewGameCreated(this, new BooleanEventArgs() { Answer = e.Result != null });
            }
        }

        /// <summary>
        /// Handler called after registering with the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void proxy_RegisterCompleted(object sender, RegisterCompletedEventArgs e)
        {
            if (e.Error != null && ServiceError != null)
            {
                // Raise an error notification
                ServiceError(this, new ExceptionEventArgs() { Error = e.Error });
            }
            else if (Registered != null)
            {
                playerID = e.Result;
                // Raise a notification about the registration
                Registered(this, new BooleanEventArgs() { Answer = e.Result != -1 });
            }
        }

        /// <summary>
        /// Handler called after telling the server that we wish to leave the current game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void proxy_LeaveGameCompleted(object sender, LeaveGameCompletedEventArgs e)
        {
            if (e.Error != null && ServiceError != null)
            {
                // Raise an error notification
                ServiceError(this, new ExceptionEventArgs() { Error = e.Error });
            }
            else if (LeftGame != null)
            {
                LeftGame(this, new BooleanEventArgs() { Answer = e.Result });
            }
        }

        /// <summary>
        /// Handler called after joining one of the games on the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void proxy_JoinGameCompleted(object sender, JoinGameCompletedEventArgs e)
        {
            if (e.Error != null && ServiceError != null)
            {
                // Raise an error notification
                ServiceError(this, new ExceptionEventArgs() { Error = e.Error });
            }
            else if (JoinedGame != null)
            {
                JoinedGame(this, new BooleanEventArgs() { Answer = e.Result });
            }
        }

        /// <summary>
        /// Notify of errors encountered during an asynchronous service call.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void proxy_ErrorChecked(object sender, AsyncCompletedEventArgs e)
        {
            if (ServiceError != null && e.Error != null)
            {
                // Raise an error notification
                ServiceError(this, new ExceptionEventArgs() { Error = e.Error });
            }
        }


        #endregion

        #region IXmlSerializable Methods


        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a serialized NetworkManager object.
        /// </summary>
        /// <param name="reader">The xml reader from which to read.</param>
        public void ReadXml(XmlReader reader)
        {
            // Read start element
            reader.Read();

            Deserialize(reader);
        }

        /// <summary>
        /// Serializes the NetworkManager object.
        /// </summary>
        /// <param name="writer">The xml writer to use when writing.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetType().Name);

            writer.WriteAttributeString("GameID", gameID.ToString());
            writer.WriteAttributeString("PlayerID", playerID.ToString());
            writer.WriteAttributeString("Name", name);
            writer.WriteAttributeString("GameName", gameName);

            writer.WriteEndElement();
        }


        #endregion

        /// <summary>
        /// Performs necessary cleanup.
        /// </summary>
        public void Dispose()
        {
            if (channel != null)
            {
                // Dispose the push notification channel
                channel.Dispose();
            }
        }
    }
}
