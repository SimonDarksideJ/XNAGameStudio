#region File Description
//-----------------------------------------------------------------------------
// AudioManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Minjie
{
    /// <summary>
    /// Component that manages audio playback for all cues.
    /// </summary>
    /// <remarks>Based on a similar class found in the NetRumble starter kit.</remarks>
    public class AudioManager : GameComponent
    {
        #region Singleton


        /// <summary>
        /// The singleton for this type.
        /// </summary>
        private static AudioManager audioManager = null;


        #endregion


        #region Audio Data


        /// <summary>
        /// The audio engine used to play all cues.
        /// </summary>
        private AudioEngine audioEngine;
        /// <summary>
        /// The soundbank that contains all cues.
        /// </summary>
        private SoundBank soundBank;
        /// <summary>
        /// The wavebank with all wave files for this game.
        /// </summary>
        private WaveBank waveBank;
        /// <summary>
        /// The cue for the current music.
        /// </summary>
        private Cue musicCue;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Constructs the manager for audio playback of all cues.
        /// </summary>
        /// <param name="game">The game that this component will be attached to.</param>
        /// <param name="settingsFile">The filename of the XACT settings file.</param>
        /// <param name="waveBankFile">The filename of the XACT wavebank file.</param>
        /// <param name="soundBankFile">The filename of the XACT soundbank file.</param>
        private AudioManager(Game game, string settingsFile, string waveBankFile,
            string soundBankFile)
            : base(game)
        {
            try
            {
                audioEngine = new AudioEngine(settingsFile);
                waveBank = new WaveBank(audioEngine, waveBankFile);
                soundBank = new SoundBank(audioEngine, soundBankFile);
            }
            catch (NoAudioHardwareException)
            {
                // silently fall back to silence
                audioEngine = null;
                waveBank = null;
                soundBank = null;
            }
        }


        /// <summary>
        /// Initialize the static AudioManager functionality.
        /// </summary>
        /// <param name="game">The game that this component will be attached to.</param>
        /// <param name="settingsFile">The filename of the XACT settings file.</param>
        /// <param name="waveBankFile">The filename of the XACT wavebank file.</param>
        /// <param name="soundBankFile">The filename of the XACT soundbank file.</param>
        public static void Initialize(Game game, string settingsFile, 
            string waveBankFile, string soundBankFile)
        {
            audioManager = new AudioManager(game, settingsFile, waveBankFile,
                soundBankFile);
            if (game != null)
            {
                game.Components.Add(audioManager);
            }
        }


        #endregion


        #region Cue Methods


        /// <summary>
        /// Retrieve a cue by name.
        /// </summary>
        /// <param name="cueName">The name of the cue requested.</param>
        /// <returns>The cue corresponding to the name provided.</returns>
        public static Cue GetCue(string cueName)
        {
            if ((audioManager == null) || (audioManager.audioEngine == null) ||
                (audioManager.soundBank == null) || (audioManager.waveBank == null))
            {
                return null;
            }
            return audioManager.soundBank.GetCue(cueName);
        }


        /// <summary>
        /// Plays a cue by name.
        /// </summary>
        /// <param name="cueName">The name of the cue to play.</param>
        public static void PlayCue(string cueName)
        {
            if ((audioManager != null) && (audioManager.audioEngine != null) &&
                (audioManager.soundBank != null) && (audioManager.waveBank != null))
            {
                audioManager.soundBank.PlayCue(cueName);
            }
        }


        /// <summary>
        /// Play music by cue name.
        /// </summary>
        /// <param name="musicCueName">The name of the music cue.</param>
        public static void PlayMusic(string musicCueName)
        {
            if ((audioManager == null) || (audioManager.audioEngine == null) ||
                (audioManager.soundBank == null) || (audioManager.waveBank == null))
            {
                return;
            }
            // stop the old music cue
            if (audioManager.musicCue != null)
            {
                audioManager.musicCue.Stop(AudioStopOptions.AsAuthored);
            }
            // get the new music cue, if any
            audioManager.musicCue = GetCue(musicCueName);
            // start the new music cue, if any
            if (audioManager.musicCue != null)
            {
                audioManager.musicCue.Play();
            }
        }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Update the audio manager, particularly the engine.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // update the audio engine
            if (audioEngine != null)
            {
                audioEngine.Update();
            }

            base.Update(gameTime);
        }


        #endregion


        #region Instance Disposal Methods


        /// <summary>
        /// Clean up the component when it is disposing.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (soundBank != null)
                    {
                        soundBank.Dispose();
                        soundBank = null;
                    }
                    if (waveBank != null)
                    {
                        waveBank.Dispose();
                        waveBank = null;
                    }
                    if (audioEngine != null)
                    {
                        audioEngine.Dispose();
                        audioEngine = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }


        #endregion
    }
}
