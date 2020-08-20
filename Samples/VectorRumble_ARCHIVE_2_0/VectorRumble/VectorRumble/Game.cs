#region File Description
//-----------------------------------------------------------------------------
// Game.cs
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class VectorRumbleGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        ScreenManager screenManager;
        AudioManager audioManager;


        public VectorRumbleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;
            graphics.SynchronizeWithVerticalRetrace = true;

            // create the screen manager
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            // create the audio manager
            audioManager = new AudioManager(this, 
                "Content\\Audio\\VectorRumble.xgs", 
                "Content\\Audio\\VectorRumble.xwb",
                "Content\\Audio\\VectorRumble.xsb");
            Services.AddService(typeof(AudioManager), audioManager);
        }


        /// <summary>
        /// Overridden from Game.Initialize().  Sets up the ScreenManager.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // add the background screen to the screen manager
            screenManager.AddScreen(new BackgroundScreen());

            // add the main menu screen to the screen manager
            screenManager.AddScreen(new MainMenuScreen());
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // update the audio manager
            audioManager.Update(gameTime);

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // the screen manager owns the real drawing

            base.Draw(gameTime);
        }

        #region Entry Point
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            using (VectorRumbleGame game = new VectorRumbleGame())
            {
                game.Run();
            }
        }
        #endregion
    }
}
