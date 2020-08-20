#region File Description
//-----------------------------------------------------------------------------
// MemoryMadnessGame.cs
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
using GameStateManagement;
#endregion

namespace MemoryMadness
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MemoryMadnessGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ScreenManager screenManager;

        public MemoryMadnessGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            //Create a new instance of the Screen Manager
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            // Switch to full screen for best game experience
            graphics.IsFullScreen = true;

            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 480;

            graphics.SupportedOrientations = DisplayOrientation.Portrait;

            // Initialize sound system
            //AudioManager.Initialize(this);

            // TODO � start with main menu screen
            GameplayScreen gameplayScreen = new GameplayScreen(1);
            gameplayScreen.IsActive = true;
            screenManager.AddScreen(gameplayScreen, null);
        }
    }
}
