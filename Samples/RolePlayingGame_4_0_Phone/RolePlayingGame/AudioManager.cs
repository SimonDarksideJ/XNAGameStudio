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
using System.Collections.Generic;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Component that manages audio playback for all cues.
    /// </summary>
    /// <remarks>
    /// Similar to a class found in the Net Rumble starter kit on the 
    /// XNA Creators Club Online website (http://creators.xna.com).
    /// </remarks>
    public class AudioManager : GameComponent
    {
        #region Singleton


        /// <summary>
        /// The singleton for this type.
        /// </summary>
        private static AudioManager audioManager = null;
        public static AudioManager Instance
        {
            get { return audioManager; }
        }

        #endregion
              
        #region Audio Data

        Dictionary<string, SoundEffectInstance> soundBank;
        string[,] soundNames;

        #endregion
        
        #region Initialization Methods
        /// <summary>
        /// Constructs the manager for audio playback of all cues.
        /// </summary>
        /// <param name="game">The game that this component will be attached to.</param>
        /// <param name="settingsFile">The filename of the XACT settings file.</param>
        /// <param name="waveBankFile">The filename of the XACT wavebank file.</param>
        /// <param name="soundBankFile">The filename of the XACT soundbank file.</param>
        
        private AudioManager(Game game)
            : base(game) { }

        
        public static void Initialize(Game game)
        {
            audioManager = new AudioManager(game);

            game.Components.Add(audioManager);

            LoadSounds();
        }


         #endregion

        #region Loading Methodes
        /// <summary>
        /// Loads a sounds and organizes them for future usage.
        /// </summary>
        public static void LoadSounds()
        {

            audioManager.soundNames = new string[,] 
            { 
                {"BattleTheme","BattleTheme"},
                {"Continue","Continue"},
                {"Death","Death"},
                {"DungeonTheme","DungeonTheme"},
                {"FireballCreate","FireballCreate"},
                {"FireballHit","FireballHit"},
                {"FireballTravel","FireballTravel"},
                {"ForestTheme","ForestTheme"},
                {"HealCreate","HealCreate"},
                {"HealImpact","HealImpact"},
                {"HealPotion","HealPotion"},
                {"LevelUp","LevelUp"},
                {"LoseTheme","LoseTheme"},
                {"MainTheme","MainTheme"},
                {"MenuMove","MenuMove"},
                {"Money","Money"},
                {"QuestComplete","QuestComplete"},
                {"SpellBlock","SpellBlock"},
                {"SpellCreate","SpellCreate"},
                {"StaffBlock","StaffBlock"},
                {"StaffHit","StaffHit"},
                {"StaffSwing","StaffSwing"},               
                {"SwordBlock","SwordBlock"},
                {"SwordHit","SwordHit"},
                {"SwordSwing","SwordSwing"},
                {"VillageTheme","VillageTheme"},
                {"WinTheme","WinTheme"}
            };


            audioManager.soundBank = new Dictionary<string, SoundEffectInstance>();

        }

        #endregion

        #region Sounds Methods



        /// <summary>
        /// Retrieve a cue by name.
        /// </summary>
        /// <param name="cueName">The name of the cue requested.</param>
        /// <returns>The cue corresponding to the name provided.</returns>
        public static SoundEffectInstance GetCue(string assetName)
        {
            return LoadAsset<SoundEffect>(assetName).CreateInstance();
        }

        /// <summary>
        /// Plays a cue by name.
        /// </summary>
        /// <param name="assetName">The name of the cue to play.</param>
        public static void PlayCue(string assetName)
        {
            LoadAsset<SoundEffect>(assetName).Play();
        }


        #endregion
        
        #region Music


        /// <summary>
        /// The cue for the music currently playing, if any.
        /// </summary>
        private SoundEffectInstance musicSound;
        /// <summary>
        /// Stack of music cue names, for layered music playback.
        /// </summary>
        private Stack<string> musicSoundNameStack = new Stack<string>();


        /// <summary>
        /// Plays the desired music, clearing the stack of music cues.
        /// </summary>
        /// <param name="assetName">The name of the music cue to play.</param>
        public static void PlayMusic(string assetName,bool isLooped)
        {

            // Stop the old music sound
            if (audioManager.musicSound != null && !audioManager.musicSound.IsDisposed)
            {
                audioManager.musicSound.Stop(true);
                UnoadAsset(audioManager.musicSound);
            }


            audioManager.musicSound = LoadAsset<SoundEffect>(assetName).CreateInstance();
            audioManager.musicSound.IsLooped = isLooped;
            audioManager.musicSound.Play();
            audioManager.musicSoundNameStack.Push(assetName);
        }


        /// <summary>
        /// Plays the music for this game, adding it to the music stack.
        /// </summary>
        /// <param name="cueName">The name of the music cue to play.</param>
        public static void PushMusic(string cueName,bool isLooped)
        {
            // start the new music cue
            if ((audioManager != null) && (audioManager.soundBank != null))
            {
                audioManager.musicSoundNameStack.Push(cueName);
                if (audioManager.musicSound != null )
                {
                    audioManager.musicSound.Stop();
                    UnoadAsset(audioManager.musicSound);
                    audioManager.musicSound = null;
                }
                audioManager.musicSound = GetCue(cueName);
                if (audioManager.musicSound != null )
                {
                    audioManager.musicSound.IsLooped = isLooped;
                    audioManager.musicSound.Play();
                }
            }
        }

        /// <summary>
        /// Stops the current music and plays the previous music on the stack.
        /// </summary>
        public static void PopMusic()
        {
            // start the new music cue
            if ((audioManager != null) && (audioManager.soundBank != null))
            {
                string cueName = null;
                if (audioManager.musicSoundNameStack.Count > 0)
                {
                    audioManager.musicSoundNameStack.Pop();
                    if (audioManager.musicSoundNameStack.Count > 0)
                    {
                        cueName = audioManager.musicSoundNameStack.Peek();
                    }
                }
                if (audioManager.musicSound != null)
                {
                    audioManager.musicSound.Stop();
                    UnoadAsset(audioManager.musicSound);
                    audioManager.musicSound = null;
                }
                if (!String.IsNullOrEmpty(cueName))
                {
                    audioManager.musicSound = GetCue(cueName);
                    if (audioManager.musicSound != null)
                    {
                        if (!audioManager.musicSound.IsDisposed)
                        {
                            audioManager.musicSound.Play();
                        }
                    }
                }
          }
        }

        /// <summary>
        /// Stop music playback, clearing the cue.
        /// </summary>
        public static void StopMusic()
        {
            foreach (var sound in audioManager.soundBank.Values)
            {
                if (sound.State != SoundState.Stopped)
                {
                    sound.Stop();
                    UnoadAsset(sound);
                }
            }
        }

        private static T LoadAsset<T>(string assetName) where T : class
        {
            T sound = audioManager.Game.Content.Load<T>( "Audio/Waves/" + assetName);
            return sound;
        }

        private static void UnoadAsset(IDisposable asset)
        {
            asset.Dispose();
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
                    foreach (var item in soundBank)
                    {
                        item.Value.Dispose();
                    }
                    soundBank.Clear();
                    soundBank = null;
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
