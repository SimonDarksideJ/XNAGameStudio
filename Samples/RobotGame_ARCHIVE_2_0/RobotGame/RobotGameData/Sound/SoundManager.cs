#region File Description
//-----------------------------------------------------------------------------
// SoundManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using RobotGameData.Camera;
#endregion

namespace RobotGameData.Sound
{
    #region SoundElement

    /// <summary>
    /// a sound structure with sound cue and emitter for 3D playback.
    /// </summary>
    public class SoundElement
    {
        public Cue cue = null;
        public AudioEmitter emitter = null;

        public string Name
        {
            get { return cue.Name; }
        }
    }
    #endregion

    /// <summary>
    /// It reads sound data and plays sound and supports 3D positional sound.
    /// When playing sound, it creates SoundElement class internally and 
    /// manages a sound pool.
    /// In order to load sound data, the files, which have been created by 
    /// Microsoft Cross-Platform Audio Creation Tool (XACT), such as .xgs,
    /// .xwb, and .xsb, are needed. 
    /// </summary>
    public class SoundManager
    {
        #region Fields

        /// <summary>
        /// If set to false, all of the related functions get turned off.
        /// </summary>
        bool soundOn = true;

        static AudioEngine audioEngine = null;
        static WaveBank waveBank = null;
        static SoundBank soundBank = null;
        static Dictionary<string, AudioCategory> audioCategories = 
                                        new Dictionary<string, AudioCategory>();

        // Keep track of all the 3D sounds that are currently playing.
        static List<SoundElement> activeSounds = new List<SoundElement>();

        // Keep track of spare SoundObject instances, so we can reuse them.
        // Otherwise we would have to allocate new instances each time
        // a sound was played, which would create unnecessary garbage.
        static Stack<SoundElement> soundPool = new Stack<SoundElement>();

        // The listener describes the ear which is hearing 3D sounds.
        // This is usually set to match the camera.
        AudioListener listener = new AudioListener();

        // The emitter describes an entity which is making a 3D sound.
        AudioEmitter emitter = new AudioEmitter();
        
        static bool pauseAll = false;

        #endregion

        #region Properties

        public static AudioEngine AudioEngine
        {
            get { return audioEngine; }
        }

        public static WaveBank WaveBank
        {
            get { return waveBank; }
        }

        public static SoundBank SoundBank
        {
            get { return soundBank; }
        }

        public static bool PauseAll
        {
            get { return pauseAll; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public SoundManager()
        {
            audioCategories.Clear();

            pauseAll = false;
        }

        /// <summary>
        /// applies 3D sound to the listener.
        /// </summary>
        /// <param name="position">listener position</param>
        /// <param name="direction">listener direction</param>
        /// <param name="up">listener up vector</param>
        /// <param name="velocity">listener velocity</param>
        public void ApplyEmitter(Vector3 position, Vector3 direction, Vector3 up,
                                 Vector3 velocity)
        {
            //  Update listener by the current camera
            this.listener.Position = position;
            this.listener.Forward = direction;
            this.listener.Up = up;
            this.listener.Velocity = velocity;
        }

        /// <summary>
        /// processes the sound pool and the 3D sound playback.
        /// XNA's AudioEngine is updated here.
        /// </summary>
        public void Update()
        {
            if (soundOn == false)
                return;

            // Loop over all the currently playing 3D sounds.
            int index = 0;

            while (index < activeSounds.Count)
            {
                SoundElement sound = activeSounds[index];

                if (sound.cue.IsStopped)
                {
                    // If the cue has stopped playing, dispose it.
                    //sound.cue.Dispose();

                    sound.cue = null;
                    sound.emitter = null;

                    // Store the SoundElement instance for future reuse.
                    soundPool.Push(sound);

                    // Remove it from the active list.
                    activeSounds.RemoveAt(index);
                }
                else
                {
                    if (sound.emitter != null)
                    {
                        // If the cue is still playing, update its 3D settings.
                        Apply3D(sound);
                    }

                    index++;
                }
            }

            // Update the XACT engine.
            AudioEngine.Update();
        }

        /// <summary>
        /// stops every sound and removes every sound member.
        /// </summary>
        public void Dispose()
        {
            if (soundOn == false)
                return;

            StopSound();

            audioCategories.Clear();

            SoundBank.Dispose();
            WaveBank.Dispose();
            AudioEngine.Dispose();
        }

        /// <summary>
        /// initialize all sound members.
        /// creates and initializes the XNA's AudioEndgine.
        /// </summary>
        /// <param name="globalSettingsFileName"></param>
        /// <param name="waveBankFileName"></param>
        /// <param name="soundBankFileName"></param>
        public void Initialize( string globalSettingsFileName, 
                                string waveBankFileName, 
                                string soundBankFileName)
        {
            if (soundOn == false)
                return;

            //  Create audio engine
            audioEngine = new AudioEngine(globalSettingsFileName);

            if (audioEngine == null)
            {
                throw new ArgumentException(
                                    "The audio engine could not be created.");
            }

            //  Create wave bank
            waveBank = new WaveBank(audioEngine, waveBankFileName);

            if (waveBank == null)
            {
                throw new ArgumentException(
                                    "The wave bank could not be created.");
            }

            //  Create sound bank
            soundBank = new SoundBank(audioEngine, soundBankFileName);

            if (soundBank == null)
            {
                throw new ArgumentException(
                                    "The sound bank could not be created.");
            }
        }

        /// <summary>
        /// adds a sound category.
        /// </summary>
        /// <param name="categoryName">name</param>
        public void AddSoundCategory(string categoryName)
        {
            if (soundOn == false)
                return;

            AudioCategory audioCategory = AudioEngine.GetCategory(categoryName);
            
            audioCategories.Add(categoryName, audioCategory);
        }

        /// <summary>
        /// plays the sound for 3D. 
        /// </summary>
        /// <param name="soundName">entried sound name</param>
        /// <param name="emitter">3D emitter</param>
        /// <returns>playing sound cue</returns>
        public Cue PlaySound3D(string soundName, AudioEmitter emitter)
        {
            SoundElement sound;

            if (soundPool.Count > 0)
            {
                // If possible, reuse an existing Cue3D instance.
                sound = soundPool.Pop();
            }
            else
            {
                // Otherwise we have to allocate a new one.
                sound = new SoundElement();
            }

            // Fill in the cue and emitter fields.
            sound.cue = soundBank.GetCue(soundName);
            sound.emitter = emitter;

            // Set the 3D position of this cue, and then play it.
            Apply3D(sound);

            sound.cue.Play();

            // Remember that this cue is now active.
            activeSounds.Add(sound);

            return sound.cue;
        }

        /// <summary>
        /// plays the sound.
        /// </summary>
        /// <param name="soundName">entried sound name</param>
        /// <returns>playing sound cue</returns>
        public static Cue PlaySound(string soundName)
        {
            SoundElement sound = null;

            if (soundPool.Count > 0)
            {
                // If possible, reuse an existing Cue3D instance.
                sound = soundPool.Pop();
            }
            else
            {
                // Otherwise we have to allocate a new one.
                sound = new SoundElement();                
            }

            // Fill in the cue and emitter fields.
            sound.cue = soundBank.GetCue(soundName);
            sound.cue.Play();

            // Remember that this cue is now active.
            activeSounds.Add(sound);

            return sound.cue;
        }

        /// <summary>
        /// pauses the sound.
        /// </summary>
        public void PauseSound()
        {
            if (soundOn == false || pauseAll )
                return;

            for (int i = 0; i < activeSounds.Count; i++)
                PauseSound(activeSounds[i]);

            pauseAll = true;
        }

        /// <summary>
        /// resumes the sound
        /// </summary>
        public void ResumeSound()
        {
            if (soundOn == false || pauseAll == false)
                return;

            for (int i = 0; i < activeSounds.Count; i++)
                ResumeSound(activeSounds[i]);

            pauseAll = false;
        }

        /// <summary>
        /// stops the sound
        /// </summary>
        /// <param name="cue">playing sound</param>
        public bool StopSound(Cue cue)
        {
            if (soundOn == false || cue == null)
                return false;

            if (cue.IsPaused || cue.IsPlaying)
            {
                cue.Stop(AudioStopOptions.Immediate);
                return true;
            }

            return false;
        }

        /// <summary>
        /// stop all sounds.
        /// </summary>
        public void StopSound()
        {
            if (soundOn == false)
                return;

            for (int i = 0; i < activeSounds.Count; i++)
                StopSound(activeSounds[i]);
        }

        /// <summary>
        /// sets volume.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="volume"></param>
        public void SetVolume(string categoryName, float volume)
        {
            if (soundOn == false)
                return;

            if (!audioCategories.ContainsKey(categoryName))
            {
                throw new InvalidOperationException("Cannot find the category : " 
                                                    + categoryName);
            }

            audioCategories[categoryName].SetVolume(
                MathHelper.Clamp(volume, 0.0f, 1.0f));
        }

        /// <summary>
        /// determines whether a sound is being played.
        /// </summary>
        /// <param name="cue">playing sound cue</param>
        /// <returns>true or false</returns>
        public bool IsPlaying(Cue cue)
        {
            if (soundOn == false)
                return false;

            if (cue != null)
                return cue.IsPlaying;

            return false;
        }

        /// <summary>
        /// determines whether a sound is being played.
        /// </summary>
        /// <param name="cue">playing sound cue</param>
        /// <returns>true or false</returns>
        public bool IsPause(Cue cue)
        {
            if (soundOn == false)
                return false;

            if (cue != null)
                return cue.IsPaused;

            return false;
        }

        /// <summary>
        /// applies 3D position to sound element.
        /// </summary>
        /// <param name="element"></param>
        private void Apply3D(SoundElement element)
        {
            emitter.Position = element.emitter.Position;
            emitter.Forward = element.emitter.Forward;
            emitter.Up = element.emitter.Up;
            emitter.Velocity = element.emitter.Velocity;

            element.cue.Apply3D(listener, emitter);
        }

        /// <summary>
        /// pauses the sound.
        /// </summary>
        /// <param name="sound">playing sound element</param>
        private bool PauseSound(SoundElement sound)
        {
            if (soundOn == false || sound == null || sound.cue == null)
                return false;

            if (sound.cue.IsPlaying)
            {
                sound.cue.Pause();
                return true;
            }

            return false;
        }

        /// <summary>
        /// resumes the sound.
        /// </summary>
        /// <param name="sound">paused sound element</param>
        private bool ResumeSound(SoundElement sound)
        {
            if (soundOn == false || sound == null || sound.cue == null)
                return false;

            if (sound.cue.IsPaused)
            {
                sound.cue.Resume();
                return true;
            }

            return false;
        }

        /// <summary>
        /// stops the sound.
        /// </summary>
        /// <param name="sound">playing sound element</param>
        private bool StopSound(SoundElement sound)
        {
            if (soundOn == false || sound == null || sound.cue == null)
                return false;

            if (sound.cue.IsPaused || sound.cue.IsPlaying)
            {
                sound.cue.Stop(AudioStopOptions.Immediate);
                return true;
            }

            return false;
        }
    }
}
