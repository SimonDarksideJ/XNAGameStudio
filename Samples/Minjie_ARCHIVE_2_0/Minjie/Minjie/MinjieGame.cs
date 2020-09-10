#region File Description
//-----------------------------------------------------------------------------
// MinjieGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace Minjie
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MinjieGame : Microsoft.Xna.Framework.Game
    {
        #region Fields


        GraphicsDeviceManager graphics;
        ScreenManager screenManager;


        #endregion


        #region Initialization


        /// <summary>
        /// Construct a new MinjieGame object.
        /// </summary>
        public MinjieGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            Content.RootDirectory = "Content";

            // start the audio manager
            AudioManager.Initialize(this, @"Content\Minjie.xgs", 
                @"Content\Minjie.xwb", @"Content\Minjie.xsb");

            // initialize the screen manager
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting.
        /// This is where it can query for any required services and load any
        /// non-graphic related content.  Calling base.Initialize will enumerate through
        /// any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            screenManager.AddScreen(new TitleScreen());

            base.Initialize();
        }


        #endregion


        #region Entry Point


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            using (MinjieGame game = new MinjieGame())
            {
                game.Run();
            }
        }


        #endregion
    }
}
