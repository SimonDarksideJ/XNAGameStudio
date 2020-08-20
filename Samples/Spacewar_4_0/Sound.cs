#region File Description
//-----------------------------------------------------------------------------
// Sound.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Spacewar
{
    /// <summary>
    /// An enum for all of the spaceWar sounds
    /// </summary>
    public enum Sounds
    {
        /// <summary>
        /// Peashoot gun shot sound
        /// </summary>
        PeashooterFire,
        /// <summary>
        /// Entering hyperspace
        /// </summary>
        HyperspaceActivate,
        /// <summary>
        /// Returning from hyperspace
        /// </summary>
        HyperspaceReturn,
        /// <summary>
        /// Rocket weapon sound
        /// </summary>
        RocketFire,
        /// <summary>
        /// Rocket weapon final explosion sound
        /// </summary>
        RocketExplode,
        /// <summary>
        /// BFG weapon sound
        /// </summary>
        BFGFire,
        /// <summary>
        /// Menu Selection Sound
        /// </summary>
        MenuSelect,
        /// <summary>
        /// Menu Advance sound
        /// </summary>
        MenuAdvance,
        /// <summary>
        /// Menu Back Sound
        /// </summary>
        MenuBack,
        /// <summary>
        /// Bad Menu selection Sound
        /// </summary>
        MenuBadSelect,
        /// <summary>
        /// Menu Scroll sound
        /// </summary>
        MenuScroll,
        /// <summary>
        /// Alternate Menu selection sound
        /// </summary>
        MenuSelect2,
        /// <summary>
        /// Another Alternate Menu selection Sound
        /// </summary>
        MenuSelect3,
        /// <summary>
        /// Bonus weapon pickup sound
        /// </summary>
        WeaponPickup,
        /// <summary>
        /// Timer expiry sound
        /// </summary>
        CountDownExpire,
        /// <summary>
        /// Beep sound for last few seconds of a countdown
        /// </summary>
        CountDownWarning,
        /// <summary>
        /// Phase Activation sound
        /// </summary>
        PhaseActivate,
        /// <summary>
        /// Phase Expiration sound
        /// </summary>
        PhaseExpire,
        /// <summary>
        /// Engine sound for player1
        /// </summary>
        ThrustPlayer1,
        /// <summary>
        /// Engine sound for player2
        /// </summary>
        ThrustPlayer2,
        /// <summary>
        /// Title screen music
        /// </summary>
        TitleMusic,
        /// <summary>
        /// Ambient music for menu backgrounds
        /// </summary>
        MenuMusic,
        /// <summary>
        /// Counter when points or money ar tallying
        /// </summary>
        PointsTally,
        /// <summary>
        /// General explosion
        /// </summary>
        Explosion,
        /// <summary>
        /// Machine gun weapon sound
        /// </summary>
        MachineGunFire,
        /// <summary>
        /// Double machine gun weapon sound
        /// </summary>
        DoubleMachineGunFire,
        /// <summary>
        /// BFG Explosion sound
        /// </summary>
        BFGExplode,

        /// <summary>
        /// When ship is damaged
        /// </summary>
        DamageShip,

        /// <summary>
        /// When ship is destroyed
        /// </summary>
        ExplodeShip,
    }

    /// <summary>
    /// Abstracts away the sounds for a simple interface using the Sounds enum
    /// </summary>
    public static class Sound
    {
        private static AudioEngine engine;
        private static WaveBank wavebank;
        private static SoundBank soundbank;

        private static string[] cueNames = new string[]
        {
            "tx0_fire",             // 
            "hyperspace_activate",  //
            "hyperspace_return",    //
            "pdp3_fire",
            "pdp3_explode",
            "hax2_fire",
            "menu_select",
            "menu_advance",         //Used all over - probbly need to choose where is back/select/advance etc
            "menu_back",
            "menu_bad_select",
            "menu_scroll",
            "menu_select2",
            "menu_select3",
            "weapon_pickup",
            "countdown_expire",     //
            "countdown_warning",    //
            "phase_activate",
            "phase_expire",
            "accel_player1",                //
            "accel_player2",                //
            "title_music",          //sort of - not working!
            "menu_music",           //sort of - not working
            "points_tally",         //used on cash right now
            "explosion_generic",
            "pdp1_fire",
            "pdp2_fire",
            "hax2_impact",
            "damage_ship",
            "explosion_ship",
        };

        /// <summary>
        /// Plays a sound
        /// </summary>
        /// <param name="sound">Which sound to play</param>
        /// <returns>XACT cue to be used if you want to stop this particular looped sound. Can NOT be ignored.  If the cue returned goes out of scope, the sound stops!!</returns>
        public static Cue Play(Sounds sound)
        {
            Cue returnValue = soundbank.GetCue(cueNames[(int)sound]);
            returnValue.Play();
            return returnValue;
        }

        /// <summary>
        /// Plays a sound
        /// </summary>
        /// <param name="sound">Which sound to play</param>
        /// <returns>Nothing!  This cue will play through to completion and then free itself.</returns>
        public static void PlayCue(Sounds sound)
        {
            soundbank.PlayCue(cueNames[(int)sound]);
        }

        /// <summary>
        /// Pumps the AudioEngine to help it clean itself up
        /// </summary>
        public static void Update()
        {
            engine.Update();
        }

        /// <summary>
        /// Stops a previously playing cue
        /// </summary>
        /// <param name="cue">The cue to stop that you got returned from Play(sound)</param>
        public static void Stop(Cue cue)
        {
            cue.Stop(AudioStopOptions.Immediate);
        }

        /// <summary>
        /// Starts up the sound code
        /// </summary>
        public static void Initialize()
        {
            engine = new AudioEngine(SpacewarGame.Settings.MediaPath + @"audio\spacewar.xgs");
            wavebank = new WaveBank(engine, SpacewarGame.Settings.MediaPath + @"audio\spacewar.xwb");
            soundbank = new SoundBank(engine, SpacewarGame.Settings.MediaPath + @"audio\spacewar.xsb");
        }

        /// <summary>
        /// Shuts down the sound code tidily
        /// </summary>
        public static void Shutdown()
        {
            soundbank.Dispose();
            wavebank.Dispose();
            engine.Dispose();
        }
    }
}
