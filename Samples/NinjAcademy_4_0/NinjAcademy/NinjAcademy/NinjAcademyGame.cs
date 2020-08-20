#region File Description
//-----------------------------------------------------------------------------
// NinjAcademyGame.cs
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
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.IO;


#endregion

namespace NinjAcademy
{
    /// <summary>
    /// A structure containing game state information used in the game's tombstoning process.
    /// </summary>
    public struct GameState
    {
        public int Score;
        public int HitPoints;
        public int GamePhasesPassed;
        public TimeSpan ElapsedPhaseTime;
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class NinjAcademyGame : Game
    {
        #region Fields
        
        /// <summary>
        /// File name of the file used to persist the game's state.
        /// </summary>
        public const string SaveFileName = "State.txt";

        /// <summary>
        /// Name of the key used to access the game state information from the current state object.
        /// </summary>
        internal const string GameStateKey = "GameState";

        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

        Texture2D activatingTexture;
        Texture2D loadingTexture;


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new instance of the game class.
        /// </summary>
        public NinjAcademyGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";            

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.IsFullScreen = true;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            screenManager = new ScreenManager(this);

            Components.Add(screenManager);

            PhoneApplicationService.Current.Activated += GameActivated;
            PhoneApplicationService.Current.Deactivated += GameDeactivated;
            PhoneApplicationService.Current.Launching += GameLaunched;            
        }

        /// <summary>
        /// Performs initializations required by the game.
        /// </summary>
        protected override void Initialize()
        {
            AudioManager.Initialize(this);

            screenManager.AddScreen(new BackgroundScreen("titlescreenBG"), null);
            screenManager.AddScreen(new MainMenuScreen(), null);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            activatingTexture = Content.Load<Texture2D>("Textures/Backgrounds/Activating");
            loadingTexture = Content.Load<Texture2D>("Textures/Backgrounds/loading");
        }


        #endregion

        #region Tombstoning Handlers
        

        /// <summary>
        /// Handler for game activation event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void GameActivated(object sender, ActivatedEventArgs e)
        {
            LoadStateFromIsolatedStorage();

            if (!PhoneApplicationService.Current.State.ContainsKey(GameStateKey))
            {
                // There's no game state information, so proceed to the main menu as usual
                AudioManager.PlayMusic("Menu Music");
                return;
            }            

            // Start directly from a screen resuming the gameplay
            foreach (GameScreen screen in screenManager.GetScreens())
            {
                screen.ExitScreen();
            }

            screenManager.AddScreen(new BackgroundScreen("Resuming"), null);
            screenManager.AddScreen(new LoadingScreen(activatingTexture, loadingTexture), null);  
        }

        /// <summary>
        /// Handler for game deactivation event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void GameDeactivated(object sender, DeactivatedEventArgs e)
        {
            SaveStateToIsolatedStorage();
        }        

        /// <summary>
        /// Handler for game launch event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void GameLaunched(object sender, LaunchingEventArgs e)
        {
            AudioManager.PlayMusic("Menu Music");
            LoadStateFromIsolatedStorage();
        }

        
        #endregion

        #region Rendering
        

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Clean isolated storage from previously saved state information.
        /// </summary>
        public static void CleanIsolatedStorage()
        {
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                isolatedStorageFile.DeleteFile(SaveFileName);
            }
        }


        #endregion

        #region Private Methods


        /// <summary>
        /// Saves the current game state to isolated storage.
        /// </summary>
        private void SaveStateToIsolatedStorage()
        {
            GameplayScreen gameplayScreen = GetGameplayScreen();

            if (gameplayScreen == null)
            {
                // There's no game to save. Clean saved data that may relate to a previous save.
                NinjAcademyGame.CleanIsolatedStorage();
                return;
            }
            
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Save the game's current state (only a portion of the state is saved)
                using (IsolatedStorageFileStream fileStream = isolatedStorageFile.CreateFile(SaveFileName))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine(gameplayScreen.Score);
                        streamWriter.WriteLine(gameplayScreen.HitPoints);
                        streamWriter.WriteLine(gameplayScreen.GamePhasesPassed);
                        streamWriter.WriteLine(gameplayScreen.ElapsedPhaseTime);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the current game state from isolated storage and places it in the game state object.
        /// </summary>
        private void LoadStateFromIsolatedStorage()
        {
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isolatedStorageFile.FileExists(SaveFileName))
                {
                    return;
                }

                try
                {
                    // Load the game's state from the isolated storage file
                    using (IsolatedStorageFileStream fileStream =
                        isolatedStorageFile.OpenFile(SaveFileName, FileMode.Open))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            GameState result;

                            result.Score = int.Parse(streamReader.ReadLine());
                            result.HitPoints = int.Parse(streamReader.ReadLine());
                            result.GamePhasesPassed = int.Parse(streamReader.ReadLine());
                            result.ElapsedPhaseTime = TimeSpan.Parse(streamReader.ReadLine());

                            PhoneApplicationService.Current.State[GameStateKey] = result;
                        }
                    }
                }
                catch
                {
                    // We did not manage to get the game state
                }
            }
        }

        /// <summary>
        /// Finds a gameplay screen objects among all screens and returns it.
        /// </summary>
        /// <returns>A gameplay screen instance, or null if none are available.</returns>
        private GameplayScreen GetGameplayScreen()
        {
            GameScreen[] screens = screenManager.GetScreens();

            foreach (GameScreen screen in screens)
            {
                if (screen is GameplayScreen)
                {
                    return screen as GameplayScreen;
                }
            }

            return null;
        }


        #endregion
    }
}
