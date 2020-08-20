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


        #endregion

        #region Initialization


        public HoneycombRush()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);


            graphics.IsFullScreen = true;

            // Create a new instance of the Screen Manager
            screenManager = new ScreenManager(this);
            screenManager.AddScreen(new GameplayScreen(DifficultyMode.Easy), null);

            Components.Add(screenManager);
        }        


        #endregion
    }
}
