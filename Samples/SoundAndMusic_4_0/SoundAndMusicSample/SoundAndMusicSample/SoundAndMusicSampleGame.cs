#region File Information
//-----------------------------------------------------------------------------
// SoundAndMusicSampleGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion Using Statements

namespace SoundAndMusicSample
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SoundAndMusicSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Fields
        public TouchLocation? touchLocation;

        // UI Elements
        GraphicsDeviceManager graphics;
        Button handleVolumeSong;
        Button handleVolumeSound;
        Button handlePitchSound;
        Button handlePanSound;

        //Sound variables
        SoundEffect laserSoundEffect;
        SoundEffect loopedSoundEffect;
        SoundEffectInstance soundEffectInstance;
        Song song;

        // Helper class to handle all the UI in the game
        UIHelper uiHelper;
        #endregion Fields

        #region Initialization
        /// <summary>
        /// Construction
        /// </summary>
        public SoundAndMusicSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Setup game orientation to Portrait and enable Full Screen
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            uiHelper = new UIHelper();
            uiHelper.CreateUIComponents(this, out handleVolumeSong,
                                            out handleVolumeSound,
                                            out handlePitchSound,
                                            out handlePanSound);
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

            uiHelper.InitializeUIComponents(ButtonPlayFireForgetTouchDown,
                                            ButtonPlayStoredSoundEffectTouchDown,
                                            ButtonPauseStoredSoundEffectTouchDown,
                                            ButtonStopStoredSoundEffectTouchDown,
                                            SliderHandlePositionChanged,
                                            ButtonPlaySongTouchDown,
                                            ButtonPauseSongTouchDown,
                                            ButtonStopSongTouchDown);
        }
        #endregion

        #region Loading
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            uiHelper.LoadAssets(this, 
                                out laserSoundEffect, out loopedSoundEffect, 
                                out soundEffectInstance, out song);
            base.LoadContent();
        }
        #endregion

        #region Update
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //Get the touch data - to be used from UI elements
            TouchCollection touches = TouchPanel.GetState();
            if (touches.Count == 1)
            {
                // Use only the single (first) touch point 
                touchLocation = touches[0];
            }
            else
                touchLocation = null;

            base.Update(gameTime);
        }
        #endregion

        #region Rendering
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            uiHelper.RenderUI(GraphicsDevice);

            base.Draw(gameTime);
        }
        #endregion

        #region Event Handlers - Handles UI events and perfroms the Sound-related logic
        /// <summary>
        /// Handles the PositionChanged event of all the slider handles
        /// by extracting the scaledValue from the handle's position relative to it's dragging bounds
        /// and acting according to the handle the was moved.
        /// </summary>
        /// <param name="sender">The handle</param>
        /// <param name="e"></param>
        private void SliderHandlePositionChanged(object sender, EventArgs e)
        {
            Button handle = sender as Button;
            float scaledValue = (handle.PositionOfOrigin.X - (float)handle.DragRestrictions.Left) /
                (float)handle.DragRestrictions.Width;

            // Sound pan handle
            if (handle == handlePanSound)
            {
                // Rescale a 0->1 value to -1->1 value
                // -1 is panning left and 1 is panning right
                scaledValue = (scaledValue - 0.5f) * 2;

                soundEffectInstance.Pan = scaledValue;
            }
            // Sound pitch handle
            else if (handle == handlePitchSound)
            {
                // Rescale a 0->1 value to -1->1 value
                // -1 one octave down and 1 is one octave up
                scaledValue = (scaledValue - 0.5f) * 2;

                soundEffectInstance.Pitch = scaledValue;
            }
            // Sound volume handle
            else if (handle == handleVolumeSound)
            {
                soundEffectInstance.Volume = scaledValue;
            }
            // Song volume handle
            else if (handle == handleVolumeSong)
            {
                // Note: Because the bug in emulator, setting MediaPlayer.Volume to 0 will reset the 
                // volume to 1. This is not occurs when deployed to the real device. As a workaround
                // this sample checks the runtime environment and behaves correspondingly
                // Note: Volume adjustment is based on a decibel, not multiplicative, scale. 
                // For more information please refer to documentation online:
                // http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.media.mediaplayer.volume.aspx
                if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Device)
                    MediaPlayer.Volume = scaledValue;
                else
                    MediaPlayer.Volume = MathHelper.Clamp(scaledValue, 0.000001f, 1);
            }
        }

        /// <summary>
        /// Acting upon PlayFireForget button press
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPlayFireForgetTouchDown(object sender, EventArgs e)
        {
            laserSoundEffect.Play();
        }

        /// <summary>
        /// Acting upon StopStoredSoundEffect button press
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStopStoredSoundEffectTouchDown(object sender, EventArgs e)
        {
            if (soundEffectInstance.State != SoundState.Stopped)
            {
                soundEffectInstance.Stop();
            }
        }

        /// <summary>
        /// Acting upon PauseStoredSoundEffect button press
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPauseStoredSoundEffectTouchDown(object sender, EventArgs e)
        {
            if (soundEffectInstance.State == SoundState.Playing)
            {
                soundEffectInstance.Pause();
            }
        }

        /// <summary>
        /// Acting upon PlayStoredSoundEffect button press
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPlayStoredSoundEffectTouchDown(object sender, EventArgs e)
        {
            if (soundEffectInstance.State == SoundState.Paused)
            {
                soundEffectInstance.Resume();
            }
            else if (soundEffectInstance.State == SoundState.Stopped)
            {
                soundEffectInstance.Play();
            }
        }

        /// <summary>
        /// Acting upon StopSong button press
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStopSongTouchDown(object sender, EventArgs e)
        {
            if (MediaPlayer.State != MediaState.Stopped)
            {
                MediaPlayer.Stop();
            }
        }

        /// <summary>
        /// Acting upon PauseSong button press
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPauseSongTouchDown(object sender, EventArgs e)
        {
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Pause();
            }
        }

        /// <summary>
        /// Acting upon PlaySong button press
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ButtonPlaySongTouchDown(object sender, EventArgs e)
        {
            if (MediaPlayer.State == MediaState.Paused)
            {
                MediaPlayer.Resume();
            }
            else if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(song);
            }
        }
        #endregion
    }
}
