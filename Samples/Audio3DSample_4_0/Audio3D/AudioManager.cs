#region File Description
//-----------------------------------------------------------------------------
// AudioManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Audio3D
{
    /// <summary>
    /// Audio manager keeps track of what 3D sounds are playing, updating
    /// their settings as the camera and entities move around the world, and
    /// automatically disposing sound effect instances after they finish playing.
    /// </summary>
    public class AudioManager : Microsoft.Xna.Framework.GameComponent
    {
        #region Fields


        // List of all the sound effects that will be loaded into this manager.
        static string[] soundNames =
        {
            "CatSound0",
            "CatSound1",
            "CatSound2",
            "DogSound",
        };


        // The listener describes the ear which is hearing 3D sounds.
        // This is usually set to match the camera.
        public AudioListener Listener
        {
            get { return listener; }
        }

        AudioListener listener = new AudioListener();


        // The emitter describes an entity which is making a 3D sound.
        AudioEmitter emitter = new AudioEmitter();


        // Store all the sound effects that are available to be played.
        Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();

        
        // Keep track of all the 3D sounds that are currently playing.
        List<ActiveSound> activeSounds = new List<ActiveSound>();


        #endregion


        public AudioManager(Game game)
            : base(game)
        { }


        /// <summary>
        /// Initializes the audio manager.
        /// </summary>
        public override void Initialize()
        {
            // Set the scale for 3D audio so it matches the scale of our game world.
            // DistanceScale controls how much sounds change volume as you move further away.
            // DopplerScale controls how much sounds change pitch as you move past them.
            SoundEffect.DistanceScale = 2000;
            SoundEffect.DopplerScale = 0.1f;

            // Load all the sound effects.
            foreach (string soundName in soundNames)
            {
                soundEffects.Add(soundName, Game.Content.Load<SoundEffect>(soundName));
            }

            base.Initialize();
        }


        /// <summary>
        /// Unloads the sound effect data.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    foreach (SoundEffect soundEffect in soundEffects.Values)
                    {
                        soundEffect.Dispose();
                    }

                    soundEffects.Clear();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        
        /// <summary>
        /// Updates the state of the 3D audio system.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Loop over all the currently playing 3D sounds.
            int index = 0;

            while (index < activeSounds.Count)
            {
                ActiveSound activeSound = activeSounds[index];

                if (activeSound.Instance.State == SoundState.Stopped)
                {
                    // If the sound has stopped playing, dispose it.
                    activeSound.Instance.Dispose();

                    // Remove it from the active list.
                    activeSounds.RemoveAt(index);
                }
                else
                {
                    // If the sound is still playing, update its 3D settings.
                    Apply3D(activeSound);

                    index++;
                }
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Triggers a new 3D sound.
        /// </summary>
        public SoundEffectInstance Play3DSound(string soundName, bool isLooped, IAudioEmitter emitter)
        {
            ActiveSound activeSound = new ActiveSound();

            // Fill in the instance and emitter fields.
            activeSound.Instance = soundEffects[soundName].CreateInstance();
            activeSound.Instance.IsLooped = isLooped;

            activeSound.Emitter = emitter;

            // Set the 3D position of this sound, and then play it.
            Apply3D(activeSound);

            activeSound.Instance.Play();

            // Remember that this sound is now active.
            activeSounds.Add(activeSound);

            return activeSound.Instance;
        }


        /// <summary>
        /// Updates the position and velocity settings of a 3D sound.
        /// </summary>
        private void Apply3D(ActiveSound activeSound)
        {
            emitter.Position = activeSound.Emitter.Position;
            emitter.Forward = activeSound.Emitter.Forward;
            emitter.Up = activeSound.Emitter.Up;
            emitter.Velocity = activeSound.Emitter.Velocity;

            activeSound.Instance.Apply3D(listener, emitter);
        }


        /// <summary>
        /// Internal helper class for keeping track of an active 3D sound,
        /// and remembering which emitter object it is attached to.
        /// </summary>
        private class ActiveSound
        {
            public SoundEffectInstance Instance;
            public IAudioEmitter Emitter;
        }
    }
}
