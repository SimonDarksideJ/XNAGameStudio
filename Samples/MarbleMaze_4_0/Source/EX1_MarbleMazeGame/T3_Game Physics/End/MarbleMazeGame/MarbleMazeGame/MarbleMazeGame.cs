#region File Description
//-----------------------------------------------------------------------------
// MarbleMazeGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using GameStateManagement;
#endregion

namespace MarbleMazeGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MarbleMazeGame : Microsoft.Xna.Framework.Game
    {
        #region Fields
        GraphicsDeviceManager graphics;
        ScreenManager screenManager;
        #endregion

        #region Initializations
        public MarbleMazeGame()
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

            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;

            screenManager.AddScreen(new GameplayScreen(),null);
        }
        #endregion
    }
}
