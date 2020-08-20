#region File Description
//-----------------------------------------------------------------------------
// AudioManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// Component that manages audio playback for all cues.
    /// </summary>
    public class AudioManager : GameComponent
    {
        #region Fields
        /// <summary>
        /// The audio engine used to play all cues.
        /// </summary>
        private AudioEngine engine;
        /// <summary>
        /// The soundbank that contains all cues.
        /// </summary>
        private SoundBank sounds;
        /// <summary>
        /// The wavebank with all wave files for this game.
        /// </summary>
        private WaveBank waves;

        /// <summary>
        /// The music cue for the game
        /// </summary>
        private Cue musicCue;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs the manager for audio playback of all cues.
        /// </summary>
        /// <param name="game">The game that this component will be attached to.</param>
        /// <param name="settingsFile">The filename of the XACT settings file.</param>
        /// <param name="waveBankFile">The filename of the XACT wavebank file.</param>
        /// <param name="soundBankFile">The filename of the XACT soundbank file.</param>
        public AudioManager(Game game, string settingsFile, string waveBankFile,
            string soundBankFile)
            : base(game)
        {
            engine = new AudioEngine(settingsFile);
            waves = new WaveBank(engine, waveBankFile);
            sounds = new SoundBank(engine, soundBankFile);
        }
        #endregion

        #region Cues
        /// <summary>
        /// Retrieve a cue by name.
        /// </summary>
        /// <param name="cueName">The name of the cue requested.</param>
        /// <returns>The cue corresponding to the name provided.</returns>
        public Cue GetCue(string cueName)
        {
            return sounds.GetCue(cueName);
        }

        /// <summary>
        /// Plays a cue by name.
        /// </summary>
        /// <param name="cueName">The name of the cue to play.</param>
        public void PlayCue(string cueName)
        {
            sounds.PlayCue(cueName);
        }
        #endregion

        #region Music
        /// <summary>
        /// Plays the music for this game, by name.
        /// </summary>
        /// <param name="cueName">The name of the music cue to play.</param>
        public void PlayMusic(string cueName)
        {
            // if music is already playing, stop it
            StopMusic();
            // start the new music cue
            musicCue = GetCue(cueName);
            musicCue.Play();
        }

        /// <summary>
        /// Stop music playback.
        /// </summary>
        public void StopMusic()
        {
            if (musicCue != null)
            {
                musicCue.Stop(AudioStopOptions.AsAuthored);
                musicCue.Dispose();
                musicCue = null;
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the audio system.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            engine.Update();
        }
        #endregion

        #region Disposing
        /// <summary>
        /// Clean up the component when it is disposing.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    StopMusic();
                    if (sounds != null)
                    {
                        sounds.Dispose();
                        sounds = null;
                    }
                    if (waves != null)
                    {
                        waves.Dispose();
                        waves = null;
                    }
                    if (engine != null)
                    {
                        engine.Dispose();
                        engine = null;
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
