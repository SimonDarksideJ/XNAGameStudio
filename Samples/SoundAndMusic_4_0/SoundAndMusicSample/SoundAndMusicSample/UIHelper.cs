#region File Information
//-----------------------------------------------------------------------------
// UIHelper.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
#endregion Using Statements

namespace SoundAndMusicSample
{
    class UIHelper
    {
        #region Fields
        SpriteBatch spriteBatch;
        SpriteFont gameFont;
        Texture2D background;
        Texture2D sliderStrip;

        Button buttonPlayFireForget;
        Button buttonPlayStoredSoundEffect;
        Button buttonPlaySong;
        Button buttonPauseStoredSoundEffect;
        Button buttonPauseSong;
        Button buttonStopStoredSoundEffect;
        Button buttonStopSong;
        Button handleVolumeSong;
        Button handleVolumeSound;
        Button handlePitchSound;
        Button handlePanSound;
        #endregion

        /// <summary>
        /// Creating and adding the button controls and slider numbs
        /// to the game's Components collection
        /// </summary>
        public void CreateUIComponents(Game game,
                                        out Button handleVolumeSong,
                                        out Button handleVolumeSound,
                                        out Button handlePitchSound,
                                        out Button handlePanSound)
        {
            buttonPlayFireForget = new Button(@"Images\playButton", game);
            game.Components.Add(buttonPlayFireForget);

            buttonPlayStoredSoundEffect = new Button(@"Images\playButton", game);
            game.Components.Add(buttonPlayStoredSoundEffect);

            buttonPlaySong = new Button(@"Images\playButton", game);
            game.Components.Add(buttonPlaySong);

            buttonPauseStoredSoundEffect = new Button(@"Images\pauseButton", game);
            game.Components.Add(buttonPauseStoredSoundEffect);

            buttonPauseSong = new Button(@"Images\pauseButton", game);
            game.Components.Add(buttonPauseSong);

            buttonStopStoredSoundEffect = new Button(@"Images\stopButton", game);
            game.Components.Add(buttonStopStoredSoundEffect);

            buttonStopSong = new Button(@"Images\stopButton", game);
            game.Components.Add(buttonStopSong);

            handleVolumeSong = new Button(@"Images\sliderHandle", game);
            this.handleVolumeSong = handleVolumeSong;
            game.Components.Add(handleVolumeSong);

            handleVolumeSound = new Button(@"Images\sliderHandle", game);
            this.handleVolumeSound = handleVolumeSound;
            game.Components.Add(handleVolumeSound);

            handlePitchSound = new Button(@"Images\sliderHandle", game);
            this.handlePitchSound = handlePitchSound;
            game.Components.Add(handlePitchSound);

            handlePanSound = new Button(@"Images\sliderHandle", game);
            this.handlePanSound = handlePanSound;
            game.Components.Add(handlePanSound);
        }

        /// <summary>
        /// An equivalent to WinForms "InitializeComponent" method.
        /// Used to layout and initialize the game's UI controls
        /// </summary>
        public void InitializeUIComponents(EventHandler buttonPlayFireForgetTouchDown,
            EventHandler buttonPlayStoredSoundEffectTouchDown,
            EventHandler buttonPauseStoredSoundEffectTouchDown,
            EventHandler buttonStopStoredSoundEffectTouchDown,
            EventHandler sliderHandlePositionChanged,
            EventHandler buttonPlaySongTouchDown,
            EventHandler buttonPauseSongTouchDown,
            EventHandler buttonStopSongTouchDown)
        {
            // "Fire and Forget" button
            buttonPlayFireForget.PositionOrigin = buttonPlayFireForget.TextureCenter;
            buttonPlayFireForget.PositionOfOrigin = new Vector2(239, 112);
            buttonPlayFireForget.TouchDown += buttonPlayFireForgetTouchDown;

            //  "StoredSoundEffect" controls
            buttonPlayStoredSoundEffect.PositionOrigin = buttonPlayStoredSoundEffect.TextureCenter;
            buttonPlayStoredSoundEffect.PositionOfOrigin = new Vector2(152, 286);
            buttonPlayStoredSoundEffect.TouchDown += buttonPlayStoredSoundEffectTouchDown;

            buttonPauseStoredSoundEffect.PositionOrigin = buttonPauseStoredSoundEffect.TextureCenter;
            buttonPauseStoredSoundEffect.PositionOfOrigin = new Vector2(240, 286);
            buttonPauseStoredSoundEffect.TouchDown += buttonPauseStoredSoundEffectTouchDown;

            buttonStopStoredSoundEffect.PositionOrigin = buttonStopStoredSoundEffect.TextureCenter;
            buttonStopStoredSoundEffect.PositionOfOrigin = new Vector2(327, 286);
            buttonStopStoredSoundEffect.TouchDown += buttonStopStoredSoundEffectTouchDown;

            handlePitchSound.PositionOrigin = handlePitchSound.TextureCenter;
            handlePitchSound.PositionOfOrigin = new Vector2(280, 434);
            handlePitchSound.DragRestrictions = new Rectangle(120, 
                (int)handlePitchSound.PositionOfOrigin.Y, 300, 0);
            handlePitchSound.PositionChanged += sliderHandlePositionChanged;
            handlePitchSound.AllowDrag = true;

            handleVolumeSound.PositionOrigin = handleVolumeSound.TextureCenter;
            handleVolumeSound.PositionOfOrigin = new Vector2(360, 505);
            handleVolumeSound.DragRestrictions = new Rectangle(120, 
                (int)handleVolumeSound.PositionOfOrigin.Y, 300, 0);
            handleVolumeSound.PositionChanged += sliderHandlePositionChanged;
            handleVolumeSound.AllowDrag = true;

            handlePanSound.PositionOrigin = handlePanSound.TextureCenter;
            handlePanSound.PositionOfOrigin = new Vector2(280, 364);
            handlePanSound.DragRestrictions = new Rectangle(120, 
                (int)handlePanSound.PositionOfOrigin.Y, 300, 0);
            handlePanSound.PositionChanged += sliderHandlePositionChanged;
            handlePanSound.AllowDrag = true;

            // "Song" controls
            buttonPlaySong.PositionOrigin = buttonPlaySong.TextureCenter;
            buttonPlaySong.PositionOfOrigin = new Vector2(112, 660);
            buttonPlaySong.TouchDown += buttonPlaySongTouchDown;

            buttonPauseSong.PositionOrigin = buttonPauseSong.TextureCenter;
            buttonPauseSong.PositionOfOrigin = new Vector2(240, 660);
            buttonPauseSong.TouchDown += buttonPauseSongTouchDown;

            buttonStopSong.PositionOrigin = buttonStopSong.TextureCenter;
            buttonStopSong.PositionOfOrigin = new Vector2(367, 660);
            buttonStopSong.TouchDown += buttonStopSongTouchDown;

            handleVolumeSong.PositionOrigin = handleVolumeSong.TextureCenter;
            handleVolumeSong.PositionOfOrigin = new Vector2(300, 743);
            handleVolumeSong.DragRestrictions = new Rectangle(120, 
                (int)handleVolumeSong.PositionOfOrigin.Y, 300, 0);
            handleVolumeSong.PositionChanged += sliderHandlePositionChanged;
            handleVolumeSong.AllowDrag = true;
        }

        /// <summary>
        /// Draws the labels and static textures that assembles the background/UI of the game
        /// </summary>
        public void RenderUI(GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();

            // Background:
            spriteBatch.Draw(background, graphicsDevice.Viewport.Bounds, Color.White);

            // Slider strips:
            spriteBatch.Draw(sliderStrip, new Vector2(96, 364), Color.White);
            spriteBatch.Draw(sliderStrip, new Vector2(96, 434), Color.White);
            spriteBatch.Draw(sliderStrip, new Vector2(96, 504), Color.White);
            spriteBatch.Draw(sliderStrip, new Vector2(96, 742), Color.White);

            // Labels:
            spriteBatch.DrawString(gameFont, "Fire and Forget SoundEffect", new Vector2(94, 6),
                new Color(0, 168, 255));
            spriteBatch.DrawString(gameFont, "Stored SoundEffectInstance", new Vector2(96, 189),
                new Color(0, 168, 255));
            spriteBatch.DrawString(gameFont, "Song", new Vector2(213, 565), new Color(0, 168, 255));

            spriteBatch.DrawString(gameFont, "Pan", new Vector2(50, 346), Color.White);
            spriteBatch.DrawString(gameFont, "Pitch", new Vector2(37, 416), Color.White);
            spriteBatch.DrawString(gameFont, "Volume", new Vector2(8, 486), Color.White);

            spriteBatch.DrawString(gameFont, "Volume", new Vector2(8, 725), Color.White);

            spriteBatch.End();
        }

        /// <summary>
        /// Loads game assets and initialize the textures, soundEffects, etc.
        /// </summary>
        public void LoadAssets(Game game, out SoundEffect laserSoundEffect,
            out SoundEffect loopedSoundEffect, out SoundEffectInstance soundEffectInstance,
            out Song song)
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);

            gameFont = game.Content.Load<SpriteFont>(@"Fonts\GameFont");

            background = game.Content.Load<Texture2D>(@"Images\bg");
            sliderStrip = game.Content.Load<Texture2D>(@"Images\sliderStrip");

            song = game.Content.Load<Song>(@"Sounds\Music");

            laserSoundEffect = game.Content.Load<SoundEffect>(@"Sounds\Laser");
            loopedSoundEffect = game.Content.Load<SoundEffect>(@"Sounds\EngineLoop");
            soundEffectInstance = loopedSoundEffect.CreateInstance();
            soundEffectInstance.IsLooped = true;
        }
    }
}