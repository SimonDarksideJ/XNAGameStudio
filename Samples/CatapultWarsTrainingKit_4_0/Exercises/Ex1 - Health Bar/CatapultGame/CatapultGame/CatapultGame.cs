#region File Description
//-----------------------------------------------------------------------------
// CatapultGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using CatapultGame;
using GameStateManagement;
#endregion

namespace CatapultGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CatapultGame : Game
    {
        #region Fields
        GraphicsDeviceManager graphics;
        ScreenManager screenManager;
        #endregion

        #region Initialization Methods
        public CatapultGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            //Create a new instance of the Screen Manager
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            //Switch to full screen for best game experience
            graphics.IsFullScreen = true;

            //Add two new screens
            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new MainMenuScreen(), null);

            AudioManager.Initialize(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }
        #endregion

        #region Loading
        protected override void LoadContent()
        {
            AudioManager.LoadSounds();
            base.LoadContent();
        }
        #endregion
    }
}
