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
using GameStateManagement;
#endregion

namespace CatapultGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CatapultGame : Game
    {
        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

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

            // TODO: Start with menu screen
            screenManager.AddScreen(new GameplayScreen(), null);

            // AudioManager.Initialize(this);
        }

    }
}
