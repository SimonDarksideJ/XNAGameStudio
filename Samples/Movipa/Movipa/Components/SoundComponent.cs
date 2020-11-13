#region File Description
//-----------------------------------------------------------------------------
// SoundComponent.cs
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
#endregion

namespace Movipa.Components
{
    /// <summary>
    /// Component that plays sound. 
    /// If nothing is specified in the Constructor, it loads the file 
    /// created with the default name. 
    /// The PlaySoundEffect method should be used for SoundEffect playback, 
    /// and the PlayBackgroundMusic method for BackgroundMusic playback.
    /// Cue is specified as the return value for the PlayBackgroundMusic method.
    /// "Volume" and "Pitch" must be provided in the Cue variables on the 
    /// XACT side for the SetVolume and SetPitch methods.
    ///
    /// ƒTƒEƒ“ƒh‚ğ–Â‚ç‚·ƒRƒ“ƒ|[ƒlƒ“ƒg‚Å‚·B
    /// ƒRƒ“ƒXƒgƒ‰ƒNƒ^‚Å“Á‚É‰½‚àw’è‚µ‚È‚¯‚ê‚ÎAƒfƒtƒHƒ‹ƒg‚Åì¬‚³‚ê‚é–¼‘O‚Ì
    /// ƒtƒ@ƒCƒ‹‚ğ“Ç‚İ‚Ş‚æ‚¤‚É‚È‚Á‚Ä‚¢‚Ü‚·B
    /// SoundEffect‚ğÄ¶‚·‚é‚Æ‚«‚ÍPlaySoundEffectƒƒ\ƒbƒh‚ğg—p‚µABackgroundMusic‚
    /// ğÄ¶‚·‚é‚ÍPlayBackgroundMusicƒƒ\ƒbƒh‚ğ
    /// g—p‚µ‚Ä‚­‚¾‚³‚¢B
    /// PlayBackgroundMusicƒƒ\ƒbƒh‚Í–ß‚è’l‚Écue‚ªw’è‚³‚ê‚Ä‚¢‚Ü‚·B
    /// SetVolume‚¨‚æ‚ÑSetPitchƒƒ\ƒbƒh‚ÉŠÖ‚µ‚Ä‚ÍAXACT‘¤‚ÅCue‚ÌVariable‚É
    /// "Volume"‚Æ"Pitch"‚ª—pˆÓ‚³‚ê‚Ä‚¢‚é•K—v‚ª‚ ‚è‚Ü‚·B
    /// </summary>
    public class SoundComponent : GameComponent
    {
        #region Fields
        private const string DefaultAudioEnginePath = "Content/Audio/Movipa.xgs";
        private const string DefaultWaveBankPath = "Content/Audio/Movipa.xwb";
        private const string DefaultSoundBankPath = "Content/Audio/Movipa.xsb";
        private const string VolumeVariableName = "Volume";
        private const string PitchVariableName = "Pitch";

        private string audioEnginePath;
        private string waveBankPath;
        private string soundBankPath;

        private AudioEngine audioEngine;
        private WaveBank waveBank;
        private SoundBank soundBank;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the audio engine path.
        /// 
        /// ƒI[ƒfƒBƒIƒGƒ“ƒWƒ“‚ÌƒpƒX‚ğæ“¾‚µ‚Ü‚·B
        /// </summary>
        public string AudioEnginePath
        {
            get { return audioEnginePath; }
        }


        /// <summary>
        /// Obtains the wave bank path.
        /// 
        /// ƒEƒF[ƒuƒoƒ“ƒN‚ÌƒpƒX‚ğæ“¾‚µ‚Ü‚·B
        /// </summary>
        public string WaveBankPath
        {
            get { return waveBankPath; }
        }


        /// <summary>
        /// Obtains the sound bank path.
        ///
        /// ƒTƒEƒ“ƒhƒoƒ“ƒN‚ÌƒpƒX‚ğæ“¾‚µ‚Ü‚·B
        /// </summary>
        public string SoundBankPath
        {
            get { return soundBankPath; }
        }


        /// <summary>
        /// Obtains the audio engine.
        ///
        /// ƒI[ƒfƒBƒIƒGƒ“ƒWƒ“‚ğæ“¾‚µ‚Ü‚·B
        /// </summary>
        public AudioEngine AudioEngine
        {
            get { return audioEngine; }
        }


        /// <summary>
        /// Obtains the wave bank.
        ///
        /// ƒEƒF[ƒuƒoƒ“ƒN‚ğæ“¾‚µ‚Ü‚·B
        /// </summary>
        public WaveBank WaveBank
        {
            get { return waveBank; }
        }


        /// <summary>
        /// Obtains the sound bank.
        ///
        /// ƒTƒEƒ“ƒhƒoƒ“ƒN‚ğæ“¾‚µ‚Ü‚·B
        /// </summary>
        public SoundBank SoundBank
        {
            get { return soundBank; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// The default audio file is used.
        ///
        /// ƒCƒ“ƒXƒ^ƒ“ƒX‚ğ‰Šú‰»‚µ‚Ü‚·B
        /// ƒI[ƒfƒBƒIƒtƒ@ƒCƒ‹‚ÍƒfƒtƒHƒ‹ƒg‚Ì‚à‚Ì‚ğg—p‚µ‚Ü‚·B
        /// </summary>
        public SoundComponent(Game game)
            : this(game, 
            DefaultAudioEnginePath, DefaultWaveBankPath, DefaultSoundBankPath)
        {
        }


        /// <summary>
        /// Initializes the instance.
        /// 
        /// ƒCƒ“ƒXƒ^ƒ“ƒX‚ğ‰Šú‰»‚µ‚Ü‚·B
        /// </summary>
        public SoundComponent(Game game, 
            string audioEnginePath, string waveBankPath, string soundBankPath)
            : base(game)
        {
            this.audioEnginePath = audioEnginePath;
            this.waveBankPath = waveBankPath;
            this.soundBankPath = soundBankPath;
        }


        /// <summary>
        /// Initializes the sound.
        ///
        /// ƒTƒEƒ“ƒh‚Ì‰Šú‰»‚ğs‚¢‚Ü‚·B
        /// </summary>
        public override void Initialize()
        {
            audioEngine = new AudioEngine(AudioEnginePath);
            waveBank = new WaveBank(audioEngine, WaveBankPath);
            soundBank = new SoundBank(audioEngine, SoundBankPath);

            audioEngine.Update();

            base.Initialize();
        }


        /// <summary>
        /// Releases all sound resources.
        ///
        /// ƒTƒEƒ“ƒhƒŠƒ\[ƒX‚ğ‘S‚ÄŠJ•ú‚µ‚Ü‚·B
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (soundBank != null && !soundBank.IsDisposed)
                soundBank.Dispose();

            if (waveBank != null && !waveBank.IsDisposed)
                waveBank.Dispose();

            if (audioEngine != null && !audioEngine.IsDisposed)
                audioEngine.Dispose();

            base.Dispose(disposing);
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the sound resource.
        /// 
        /// ƒTƒEƒ“ƒhƒŠƒ\[ƒX‚ÌXVˆ—‚ğs‚¢‚Ü‚·B
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (audioEngine != null && !audioEngine.IsDisposed)
            {
                audioEngine.Update();
            }

            base.Update(gameTime);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Obtains the Cue.
        ///
        /// Cue‚ğæ“¾‚µ‚Ü‚·B
        /// </summary>
        /// <param name="sound">Sound name</param>
        ///  
        /// <param name="sound">ƒTƒEƒ“ƒh‚Ì–¼‘O</param>
        /// <returns>ƒTƒEƒ“ƒh‚ÌCue</returns>
        public Cue GetCue(string cueName)
        {
            return soundBank.GetCue(cueName);
        }


        /// <summary>
        /// Plays the sound.
        /// 
        /// ƒTƒEƒ“ƒh‚ÌÄ¶‚ğ‚µ‚Ü‚·B
        /// </summary>
        /// <param name="sound">Sound for playback</param>
        ///  
        /// <param name="sound">Ä¶‚·‚éƒTƒEƒ“ƒh</param>
        ///
        /// <returns>Played Cue</returns>
        ///  
        /// <returns>Ä¶‚³‚ê‚½Cue</returns>
        public Cue PlayBackgroundMusic(string cueName)
        {
            Cue cue = GetCue(cueName);

            if (cue != null && !cue.IsDisposed)
                cue.Play();

            return cue;
        }


        /// <summary>
        /// Plays the sound.
        /// 
        /// ƒTƒEƒ“ƒh‚ÌÄ¶‚ğ‚µ‚Ü‚·B
        /// </summary>
        /// <param name="sound">Sound for playback</param>
        ///  
        /// <param name="sound">Ä¶‚·‚éƒTƒEƒ“ƒh</param>
        public void PlaySoundEffect(string cueName)
        {
            if (soundBank != null && !soundBank.IsDisposed)
                soundBank.PlayCue(cueName);
        }


        /// <summary>
        /// Sets the Cue variable.
        /// 
        /// Cue‚Ì•Ï”‚ğİ’è‚µ‚Ü‚·B
        /// </summary>
        /// <param name="cue">Cue to be changed</param>
        ///  
        /// <param name="cue">•ÏX‚·‚éƒLƒ…[</param>
        ///
        /// <param name="value">Value to be changed</param>
        ///  
        /// <param name="value">•ÏX‚·‚é’l</param>
        public static void SetVariable(Cue cue, string name, float value)
        {
            if (cue == null || cue.IsDisposed)
                return;

            cue.SetVariable(name, value);
        }


        /// <summary>
        /// Changes the Cue volume.
        /// Specified as float type between 0 and 1.
        /// 
        /// ƒLƒ…[‚Ìƒ{ƒŠƒ…[ƒ€‚ğ•ÏX‚µ‚Ü‚·B
        /// 0`1‚Ü‚Å‚ÌfloatŒ^‚Åw’è‚µ‚Ü‚·B
        /// </summary>
        /// <param name="cue">Cue to be changed</param>
        ///  
        /// <param name="cue">•ÏX‚·‚éƒLƒ…[</param>
        ///
        /// <param name="value">Value to be changed</param>
        ///  
        /// <param name="value">•ÏX‚·‚é’l</param>
        public static void SetVolume(Cue cue, float value)
        {
            float volume = 100.0f * MathHelper.Clamp(value, 0.0f, 1.0f);
            SetVariable(cue, VolumeVariableName, volume);
        }


        /// <summary>
        /// Alters the Cue pitch.
        /// Alters the pitch by semitone in the range -12 to +12.
        /// 
        /// ƒLƒ…[‚Ìƒsƒbƒ`‚ğ•ÏX‚µ‚Ü‚·B
        /// -12`+12‚Ü‚ÅA”¼‰¹‚¸‚Â•Ï‰»‚µ‚Ü‚·B
        /// </summary>
        /// <param name="cue">Cue to be changed</param>
        ///  
        /// <param name="cue">•ÏX‚·‚éƒLƒ…[</param>
       ///
        /// <param name="value">Value to be changed</param>
        ///  
        /// <param name="value">•ÏX‚·‚é’l</param>
        public static void SetPitch(Cue cue, float value)
        {
            float pitch = 12.0f * MathHelper.Clamp(value, -1.0f, 1.0f);
            SetVariable(cue, PitchVariableName, pitch);
        }


        /// <summary>
        /// Stops the Cue.
        /// 
        /// Cue‚ğ’â~‚µ‚Ü‚·B
        /// </summary>
        /// <param name="cue">Cue to be stopped</param>
        ///  
        /// <param name="cue">’â~‚·‚éƒLƒ…[</param>
        public static void Stop(Cue cue)
        {
            if (cue != null && !cue.IsDisposed && cue.IsPlaying)
                cue.Stop(AudioStopOptions.Immediate);
        }
        #endregion
    }
}
