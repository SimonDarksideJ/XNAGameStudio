#region File Description
//-----------------------------------------------------------------------------
// YachtGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using GameStateManagement;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using YachtServices;
using System.Xml.Serialization;


#endregion

namespace Yacht
{
    /// <summary>
    /// The main game type.
    /// </summary>
    /// 
    public class YachtGame : Game
    {
        #region Static Properties

        public static SpriteFont RegularFont { get; private set; }
        public static SpriteFont ScoreFont { get; private set; }
        public static SpriteFont ScoreFontBold { get; private set; }
        public static SpriteFont LeaderScoreFont { get; private set; }
        public static SpriteFont Font { get; private set; }
        #endregion

        #region Fields


        GraphicsDeviceManager graphics;
        ScreenManager screenManager;


        #endregion

        #region Initializations


        public YachtGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.IsFullScreen = true;

            screenManager = new ScreenManager(this);

            Components.Add(screenManager);

            // Set the orientation of the view to portrait.
            graphics.SupportedOrientations = DisplayOrientation.Portrait;
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 480;

            // Initialize the display orientation of the touch
            TouchPanel.DisplayOrientation = DisplayOrientation.Portrait;

            // Subscribe to the application's lifecycle events
            PhoneApplicationService.Current.Activated += GameActivated;
            PhoneApplicationService.Current.Deactivated += GameDeactivated;
            PhoneApplicationService.Current.Closing += GameClosed;
            PhoneApplicationService.Current.Launching += GameLaunched;
        }

        /// <summary>
        /// Initialize the game.
        /// </summary>
        protected override void Initialize()
        {
            // Initialize the accelerometer.
            Accelerometer.Initialize();

            // Initialize Audio
            AudioManager.Initialize(this);
            AudioManager.LoadSounds();

            base.Initialize();
        }


        #endregion

        #region Loading


        /// <summary>
        /// Load content which will be used by the game.
        /// </summary>
        protected override void LoadContent()
        {            
            RegularFont = Content.Load<SpriteFont>(@"Fonts\Regular");
            ScoreFont = Content.Load<SpriteFont>(@"Fonts\ScoreFont");
            ScoreFontBold = Content.Load<SpriteFont>(@"Fonts\ScoreFontBold");
            LeaderScoreFont = Content.Load<SpriteFont>(@"Fonts\LeaderScoreFont");
            Font = Content.Load<SpriteFont>(@"Fonts\MenuFont");

            base.LoadContent();
        }


        #endregion

        #region Tombstoning


        /// <summary>
        /// Saves necessary data to isolated storage before the game is deactivated.
        /// </summary>
        void GameDeactivated(object sender, DeactivatedEventArgs e)
        {
            if (PhoneApplicationService.Current.State.ContainsKey(Constants.YachtStateKey))
            {
                SaveGameState();
            }
        }
        
        /// <summary>
        /// Loads game state data from isolated storage once the game is activated. If an online game was in progress,
        /// reconnects to the server. If no stored data is available, starts the game normally.
        /// </summary>
        void GameActivated(object sender, ActivatedEventArgs e)
        {
            // Check if we were in the middle of an online game
            if (LoadGameState(GameTypes.Online))
            {
                // Remove stored online data
                DeleteIsolatedStorageFile(Constants.YachtStateFileNameOnline);

                // The network manager is updated according to the game state loaded so try to connect
                NetworkManager.Instance.Registered += RegisteredWithServer;
                NetworkManager.Instance.ServiceError += ServerErrorOccurred;
                NetworkManager.Instance.Connect(NetworkManager.Instance.name);

                return;
            }

            // Check if we were in the middle of an offline game
            if (LoadGameState(GameTypes.Offline))
            {
                // Remove stored offline data
                DeleteIsolatedStorageFile(Constants.YachtStateFileNameOffline);

                screenManager.AddScreen(new GameplayScreen(GameTypes.Offline), null);
                
                return;
            }

            // There is no game state data, so display the main menu
            screenManager.AddScreen(new MainMenuScreen(), null);
        }        
        
        /// <summary>
        /// Save the game state to isolated storage when closing the game.
        /// </summary>
        void GameClosed(object sender, ClosingEventArgs e)
        {            
            if (PhoneApplicationService.Current.State.ContainsKey(Constants.YachtStateKey))
            {
                SaveGameState();
            }
        }


        /// <summary>
        /// Moves to the main menu screen.
        /// </summary>
        void GameLaunched(object sender, LaunchingEventArgs e)
        {
            // Check if we were in the middle of an online game
            if (LoadGameState(GameTypes.Online))
            { 
                // Remove stored online data
                DeleteIsolatedStorageFile(Constants.YachtStateFileNameOnline);

                // The network manager is updated according to the game state loaded so try to connect
                NetworkManager.Instance.Registered += RegisteredWithServer;
                NetworkManager.Instance.ServiceError += ServerErrorOccurred;
                NetworkManager.Instance.Connect(NetworkManager.Instance.name);

                return;
            }

            // Start the game normally, at the main menu
            screenManager.AddScreen(new MainMenuScreen(), null);
        }


        #endregion

        #region Server Communication Handlers


        /// <summary>
        /// Called when there is an error contacting the game server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ServerErrorOccurred(object sender, ExceptionEventArgs e)
        {
            // We no longer need to be notified of server events (the main menu screen will handle that)
            NetworkManager.Instance.Registered -= RegisteredWithServer;
            NetworkManager.Instance.ServiceError -= ServerErrorOccurred;

            screenManager.AddScreen(new MainMenuScreen(), null);

            Guide.BeginShowMessageBox("The server is unavailable", "  ", new String[] { "OK" }, 0,
                MessageBoxIcon.Alert, null, null);
        }

        /// <summary>
        /// Called once registration with the game server is successful.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RegisteredWithServer(object sender, BooleanEventArgs e)
        {
            // We no longer need to be notified of server events (the gameplay screen will handle that)
            NetworkManager.Instance.Registered -= RegisteredWithServer;
            NetworkManager.Instance.ServiceError -= ServerErrorOccurred;
            screenManager.AddScreen(new GameplayScreen(GameTypes.Online), null);
        }


        #endregion

        #region Private Methods


        /// <summary>
        /// Saves the game-state data from the game's state object in isolated storage. Assumes the game state object
        /// contains game-state data.
        /// </summary>        
        public static void SaveGameState()
        {
            if (PhoneApplicationService.Current.State.ContainsKey(Constants.YachtStateKey))
            {
                try
                {
                    using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        YachtState yachtState = 
                            (YachtState)PhoneApplicationService.Current.State[Constants.YachtStateKey];

                        string fileName = yachtState.YachGameState.GameType == GameTypes.Offline ?
                            Constants.YachtStateFileNameOffline : Constants.YachtStateFileNameOnline;

                        using (IsolatedStorageFileStream fileStream = isolatedStorageFile.CreateFile(fileName))
                        {                           
                            using (XmlWriter writer = XmlWriter.Create(fileStream))
                            {                                
                                yachtState.WriteXml(writer);
                            }
                        }
                    }
                }
                catch
                {
                    // There was an error saving data to isolated storage. Not much that we can do about it.
                }
            }
        }

        /// <summary>
        /// Loads a game-state object from isolated storage and places it in the game's state object.
        /// </summary>
        /// <param name="gameType">The type of game for which to load the data.</param>
        /// <returns>True if game data was successfully loaded from isolated storage and false otherwise.</returns>
        public static bool LoadGameState(GameTypes gameType)
        {
            string fileName;

            switch (gameType)
            {
                case GameTypes.Offline:
                    fileName = Constants.YachtStateFileNameOffline;
                    break;
                case GameTypes.Online:
                    fileName = Constants.YachtStateFileNameOnline;
                    break;
                default:
                    fileName = null;
                    break;
            }

            try
            {
                using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {                   
                    // Check whether or not the data file exists
                    if (isolatedStorageFile.FileExists(fileName))
                    {
                        // If the file exits, open it and read its contents
                        using (IsolatedStorageFileStream fileStream = 
                            isolatedStorageFile.OpenFile(fileName, FileMode.Open))
                        {
                            using (XmlReader reader = XmlReader.Create(fileStream))
                            {
                                YachtState yachtState = new YachtState();

                                // Read the xml declaration to get it out of the way
                                reader.Read();

                                yachtState.ReadXml(reader);

                                PhoneApplicationService.Current.State[Constants.YachtStateKey] = yachtState;
                            }
                        }

                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }                               

        /// <summary>
        /// Cleans a specific file from isolated storage.
        /// </summary>
        /// <param name="fileName">The name of the file to clean from isolated storage.</param>
        public static void DeleteIsolatedStorageFile(string fileName)
        {
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorageFile.FileExists(fileName))
                {
                    isolatedStorageFile.DeleteFile(fileName);
                }
            }
        }                

        #endregion
    }
}
