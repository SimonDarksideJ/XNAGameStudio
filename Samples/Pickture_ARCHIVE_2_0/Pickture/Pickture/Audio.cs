#region File Description
//-----------------------------------------------------------------------------
// Audio.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Pickture
{
    /// <summary>
    /// Manages all audio content and playback.
    /// </summary>
    static class Audio
    {
        static AudioEngine engine = new AudioEngine("Content/Audio/Pickture.xgs");
        static WaveBank waveBank = new WaveBank(engine, "Content/Audio/Wave Bank.xwb");
        static SoundBank soundBank =
            new SoundBank(engine, "Content/Audio/Sound Bank.xsb");

        /// <summary>
        /// Performs periodic work required by the audio engine.
        /// </summary>
        public static void Update()
        {
            engine.Update();
        }

        /// <summary>
        /// Plays a cue.
        /// </summary>
        /// <param name="cueName">Name of cue to play.</param>
        public static void Play(string cueName)
        {
            soundBank.PlayCue(cueName);            
        }
    }
}
