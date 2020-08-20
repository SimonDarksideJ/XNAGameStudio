#region File Description
//-----------------------------------------------------------------------------
// GameSound.cs
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
using RobotGameData;
using RobotGameData.Sound;
#endregion

namespace RobotGame
{
    #region Enums

    enum SoundCategory
    {
        Default = 0,
        Effect,
        Music,

        Count
    }

    /// <summary>
    /// sound names in the game.
    /// </summary>
    public enum SoundTrack
    {
        MainTitle = 0,
        ReadyRoom,
        FirstStage,
        SecondStage,
        MissionClear,
        MissionFail,

        MenuOpen,
        MenuClose,
        MenuFocus,
        MenuSelected,
        MenuError,

        PlayerEmptyBullet,
        PlayerMachineGunFire,
        PlayerMachineGunMarkReload,
        PlayerMachineGunGrundReload,
        PlayerShotgunFire,
        PlayerShotgunReload,
        PlayerHandgunFire,
        PlayerHandgunReload,
        PlayerWalk,
        PlayerRun,
        PlayerDestroy1,
        PlayerDestroy2,
        PlayerBooster,
        PlayerPrepareBooster,
        PlayerSwapWeapon,

        CameleerFire,
        CameleerWalk,

        MaomingFire,
        MaomingWalk,

        DuskmasFire,
        DuskmasWalk,

        HammerFire,
        HammerMelee,        
        HammerWalk,

        TankFire,
        TankMove,

        BossMelee,        
        BossWalk,

        HitGrenade,
        HitGun,
        HitCannon,
        HitDuskmasMelee,
        HitHammerMelee,
        HitBossMelee,
        
        DestroyLightMech1,
        DestroyLightMech2,
        DestroyHeavyMech1,
        DestroyHeavyMech2,

        PickupRemedyBox,
        PickupMagazine,
        PickupWeapon,

        Count
    }

    #endregion

    /// <summary>
    /// It provides an interface which allows the sound playback during gameplay.
    /// Initialize  function initializes the RobotGameData’s sound engine and 
    /// sets up the sound category.
    /// The names of the sound files which can be played back
    /// are stored as strings in a table.
    /// </summary>
    public static class GameSound
    {
        #region Fields

        static string[] szSoundName = new string[]
        {
            //  BGM music
            "TitleMenu",
            "MainMenu",
            "BGM_France",
            "BGM_AircraftCarrier",
            "MissionSuccess",
            "MissionFail",

            //  Menu sound
            "MenuOpen",
            "MenuClose",
            "Point_Up",
            "BClick_Menu",
            "Error",

            //  Player sound
            "EmptyMagazine",
            "Fire_PM42",
            "Reload_SMG",
            "Reload_AR3",
            "Fire_Shotgun",
            "Reload_Shotgun",
            "Fire_Handgun",
            "Reload_Handgun",
            "HERO_Move",
            "HERO_Run",
            "Explosion_PC_1",
            "Explosion_PC_2",
            "HERO_Booster",
            "HERO_PreBooster",
            "SwapWeapon",

            //  Cameleer sound
            "Fire_CameleerT1_Gun",
            "Step_Cameleer",

            //  Maoming sound
            "Fire_MaomingT3_Gun",
            "Step_Maoming",

            //  Duskmas sound
            "Fire_Duskmas_Gun",
            "Step_Duskmas",

            //  Hammer sound
            "Fire_Hammer_Gun",
            "Fire_HammerMelee",            
            "Step_Hammer",    
        
            //  Tank
            "Fire_Tiger12_Gun",
            "Step_Tiger12",

            //  Boss
            "Fire_Phantom_Melee",            
            "Step_Phantom_Boss",

            //  global sound
            "HIT_FM_Grenade",
            "HIT_MachineGun",
            "HIT_60mm",
            "HIT_Axis6M",
            "HIT_HammerMelee",
            "HIT_Phantom_Melee",

            "Explosion_LightMech_1",
            "Explosion_LightMech_2",
            "Explosion_HeavyMech_1",
            "Explosion_HeavyMech_2",

            "ITEM_HP",
            "ITEM_BULLET",
            "ITEM_WEAPON",
        };

        #endregion

        /// <summary>
        /// initializes the sound manager.
        /// </summary>
        public static void Initialize()
        {
            //  initializes audio settings.
            FrameworkCore.SoundManager.Initialize(
                        "Content/Audio/RobotGame.xgs",
                        "Content/Audio/RobotGame.xwb",
                        "Content/Audio/RobotGame.xsb");

            //  gets the categories needed to change volume and pitching
            FrameworkCore.SoundManager.AddSoundCategory(
                SoundCategory.Default.ToString());
            FrameworkCore.SoundManager.AddSoundCategory(
                SoundCategory.Effect.ToString());
            FrameworkCore.SoundManager.AddSoundCategory(
                SoundCategory.Music.ToString());

            //  set the sounds default volume
            FrameworkCore.SoundManager.SetVolume(
                SoundCategory.Default.ToString(), 1.0f);
            FrameworkCore.SoundManager.SetVolume(
                SoundCategory.Effect.ToString(), 1.0f);
            FrameworkCore.SoundManager.SetVolume(
                SoundCategory.Music.ToString(), 1.0f);
        }

        /// <summary>
        /// plays the sound by index.
        /// </summary>
        /// <param name="index">an index of game sound</param>
        /// <returns>playing sound cue</returns>
        public static Cue Play(SoundTrack index)
        {
            return SoundManager.PlaySound( szSoundName[(int)index]);
        }

        /// <summary>
        /// plays back the 3D positional sound
        /// </summary>
        /// <param name="index">an index of game sound</param>
        /// <param name="owner">
        /// the object to which the 3D position will be applied.
        /// </param>
        /// <returns></returns>
        public static Cue Play3D(SoundTrack index, GameUnit owner)
        {
            return FrameworkCore.SoundManager.PlaySound3D(szSoundName[(int)index], 
                                                          owner.Emitter);
        }

        /// <summary>
        /// plays back the 3D positional sound
        /// </summary>
        /// <param name="index">an index of game sound</param>
        /// <param name="emitter">
        /// the emitter to which the 3D position will be applied.
        /// </param>
        /// <returns></returns>
        public static Cue Play3D(SoundTrack index, AudioEmitter emitter)
        {
            return FrameworkCore.SoundManager.PlaySound3D(szSoundName[(int)index],
                                                          emitter);
        }
        
        /// <summary>
        /// whether the specified sound is being played.
        /// </summary>
        /// <param name="cue">a sound cue of the target</param>
        /// <returns></returns>
        public static bool IsPlaying(Cue cue)
        {
            return FrameworkCore.SoundManager.IsPlaying(cue);
        }

        /// <summary>
        /// whether the specified sound is being paused.
        /// </summary>
        /// <param name="cue">a sound cue of the target</param>
        /// <returns></returns>
        public static bool IsPause(Cue cue)
        {
            return FrameworkCore.SoundManager.IsPause(cue);
        }

        /// <summary>
        /// stops playing sound.
        /// </summary>
        /// <param name="cue">a sound cue of the target</param>
        /// <returns></returns>
        public static bool Stop(Cue cue)
        {
            return FrameworkCore.SoundManager.StopSound(cue);
        }

        /// <summary>
        /// stop every sounds.
        /// </summary>
        public static void StopAll()
        {
            FrameworkCore.SoundManager.StopSound();
        }

        /// <summary>
        /// whether every sound is being paused.
        /// </summary>
        /// <returns></returns>
        public static bool IsPauseAll()
        {
            return SoundManager.PauseAll;
        }

        /// <summary>
        /// pause every sounds.
        /// </summary>
        public static void PauseAll()
        {
            if( !IsPauseAll())
                FrameworkCore.SoundManager.PauseSound();
        }

        /// <summary>
        /// resume every sounds.
        /// </summary>
        public static void ResumeAll()
        {
            if (IsPauseAll())
                FrameworkCore.SoundManager.ResumeSound();
        }
    }
}
