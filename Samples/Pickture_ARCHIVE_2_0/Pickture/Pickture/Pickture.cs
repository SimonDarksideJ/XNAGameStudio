#region File Description
//-----------------------------------------------------------------------------
// Pickture.cs
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
using System.Diagnostics;
#endregion

namespace Pickture
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class Pickture : Microsoft.Xna.Framework.Game
    {
        #region Singleton instance

        static Pickture instance;

        public static Pickture Instance
        {
            get { return instance; }
        }

        #endregion

        #region Fields

        GraphicsDeviceManager graphics;

        #endregion

        #region Properties

        /// <summary>
        /// Common game screen trasition time.
        /// </summary>
        public static readonly TimeSpan TransitionTime = TimeSpan.FromSeconds(2.5);

        #endregion

        #region Methods

        /// <summary>
        /// Construct a new instance of Pickture. This method will enforce that it is
        /// only ever called once because the game class itself is a singleton.
        /// </summary>
        public Pickture()
        {
            // Enforce singleton
            if (instance != null)
            {
                throw new InvalidOperationException(
                    "Only one instance of Pickture may be created.");
            }
            instance = this;

            // Initalize graphics
            graphics = new GraphicsDeviceManager(this);
            graphics.MinimumVertexShaderProfile = ShaderProfile.VS_2_0;
            graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;

#if XBOX360
            // On Xbox360, always use the user's preferred resolution and multisampling
            // The other approach is to always use the same resolution (typically
            // 1920x1080). The hardware will automatically down scale and letterbox as
            // needed. However, Pickture is already resolution independant for Windows
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            graphics.PreferMultiSampling = true;
            graphics.PreparingDeviceSettings +=
                new EventHandler<PreparingDeviceSettingsEventArgs>(
                    graphics_PreparingDeviceSettings);

#else
            // On Windows, just use a small and simple starting size, but allow
            // the user to resize the Window however they like
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            Window.AllowUserResizing = true;
            graphics.IsFullScreen = false;
#endif


            // Initalzie content
            Content.RootDirectory = "Content";

            // Initalize screen management
            ScreenManager screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            // Initalize picture database
            PictureDatabase.Initialize();

            // If enough pictures are available,
            if (PictureDatabase.Count >= 2)
            {
                // start up the game
                screenManager.AddScreen(new MainMenuScreen());
            }
            else
            {
                // Otherwise, show error and quit
                MessageBoxScreen messageBox = new MessageBoxScreen(
                    "Unable to find enough pictures to play.", false);
                messageBox.Accepted += new EventHandler<EventArgs>(messageBox_Accepted);
                screenManager.AddScreen(messageBox);
            }
        }

#if XBOX360
        // Callback used to force device settings on the 360
        void graphics_PreparingDeviceSettings(object sender,
                                              PreparingDeviceSettingsEventArgs e)
        {
            // Always use 4X antialiasing on the 360
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType =
                MultiSampleType.FourSamples;
        }
#endif

        // Callback used by error message box
        void messageBox_Accepted(object sender, EventArgs e)
        {
            Exit();
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Only the audio must be updated globally,
            Audio.Update();

            // the base call will allow the ScreenManager to update everything else
            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // The screen manager will handle the rest of the 
            base.Draw(gameTime);
        }

        #endregion
    }
}