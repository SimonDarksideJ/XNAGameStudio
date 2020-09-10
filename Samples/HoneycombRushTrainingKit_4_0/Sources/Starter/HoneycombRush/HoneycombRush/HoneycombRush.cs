#region File Description
//-----------------------------------------------------------------------------
// HoneycombRush.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using Microsoft.Xna.Framework;
using HoneycombRush.GameDebugTools;


#endregion

namespace HoneycombRush
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class HoneycombRush : Game
    {
        #region Fields


        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

        DebugSystem debugSystem;


        #endregion

        #region Initialization


        public HoneycombRush()
        {
            // Initialize sound system
            AudioManager.Initialize(this);            
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);


            graphics.IsFullScreen = true;

            // Create a new instance of the Screen Manager
            screenManager = new ScreenManager(this);

            screenManager.AddScreen(new BackgroundScreen("titleScreen"), null);
            screenManager.AddScreen(new MainMenuScreen(), PlayerIndex.One);
            Components.Add(screenManager);
        }

        protected override void Initialize()
        {
            // Initialize the debug system with the game and the name of the font 
            // we want to use for the debugging
            debugSystem = DebugSystem.Initialize(this, @"Fonts\GameScreenFont16px");

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            HighScoreScreen.LoadHighscores();

            base.LoadContent();
        }


        #endregion

        #region Update and Draw
        

        protected override void Update(GameTime gameTime)
        {
            // Tell the TimeRuler that we're starting a new frame. you always want
            // to call this at the start of Update
            debugSystem.TimeRuler.StartFrame();

            // Start measuring time for "Update".
            debugSystem.TimeRuler.BeginMark("Update", Color.Blue);

            base.Update(gameTime);

            // Stop measuring time for "Update".
            debugSystem.TimeRuler.EndMark("Update");
        }

        protected override void Draw(GameTime gameTime)
        {
            // Start measuring time for "Draw".
            debugSystem.TimeRuler.BeginMark("Draw", Color.Yellow);

            base.Draw(gameTime);

            // Stop measuring time for "Draw".
            debugSystem.TimeRuler.EndMark("Draw");
        }


        #endregion
    }
}
