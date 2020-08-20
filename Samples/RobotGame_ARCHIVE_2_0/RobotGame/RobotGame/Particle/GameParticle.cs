#region File Description
//-----------------------------------------------------------------------------
// GameParticle.cs
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
using RobotGameData.ParticleSystem;
#endregion

namespace RobotGame
{
    #region Enum

    public enum ParticleType
    {
        EnvironmentSmoke = 0,
        EnvironmentFire,

        DestroyLightMech1,
        DestroyLightMech2,
        DestroyHeavyMech1,
        DestroyHeavyMech2,
        DestroyTank1,
        DestroyTank2,

        PlayerMachineGunFire,
        PlayerMachineGunUnitHit,
        PlayerMachineGunWorldHit,
        PlayerMachineGunReload,

        PlayerShotgunFire,
        PlayerShotgunUnitHit,
        PlayerShotgunWorldHit,

        PlayerHandgunFire,
        PlayerHandgunUnitHit,
        PlayerHandgunWorldHit,
        PlayerHandgunReload,

        EnemyGunFire,
        EnemyGunUnitHit, 
        EnemyGunWorldHit,

        EnemyCannonFire,
        EnemyCannonUnitHit,
        EnemyCannonWorldHit,

        EnemyMeleeUnitHit,

        BoosterOn,
        BoosterPrepare,
        BoosterGround,

        Count
    }

    #endregion

    /// <summary>
    /// It sets up particles which are used in the game and 
    /// provides an interface for playing back of particles.
    /// The particles that are used in the game are defined 
    /// in particle list (Content/Particles/<*>.ParticleList ) XML file.
    /// Particle List(.ParticleList) file specifies the number 
    /// of particle instances and the names of every particles 
    /// that are to be used in one stage.
    /// </summary>
    public static class GameParticle
    {
        #region Fields

        //  Particle indices
        static int[] index = new int[(int)ParticleType.Count];

        //  Particle names
        static string[] szName = new string[]
        {
            ParticleType.EnvironmentSmoke.ToString(),
            ParticleType.EnvironmentFire.ToString(),

            ParticleType.DestroyLightMech1.ToString(),
            ParticleType.DestroyLightMech2.ToString(),
            ParticleType.DestroyHeavyMech1.ToString(),
            ParticleType.DestroyHeavyMech2.ToString(),
            ParticleType.DestroyTank1.ToString(),
            ParticleType.DestroyTank2.ToString(),

            ParticleType.PlayerMachineGunFire.ToString(),
            ParticleType.PlayerMachineGunUnitHit.ToString(),
            ParticleType.PlayerMachineGunWorldHit.ToString(),
            ParticleType.PlayerMachineGunReload.ToString(),

            ParticleType.PlayerShotgunFire.ToString(),
            ParticleType.PlayerShotgunUnitHit.ToString(),
            ParticleType.PlayerShotgunWorldHit.ToString(),

            ParticleType.PlayerHandgunFire.ToString(),
            ParticleType.PlayerHandgunUnitHit.ToString(),
            ParticleType.PlayerHandgunWorldHit.ToString(),
            ParticleType.PlayerHandgunReload.ToString(),

            ParticleType.EnemyGunFire.ToString(),
            ParticleType.EnemyGunUnitHit.ToString(),
            ParticleType.EnemyGunWorldHit.ToString(),

            ParticleType.EnemyCannonFire.ToString(),
            ParticleType.EnemyCannonUnitHit.ToString(),
            ParticleType.EnemyCannonWorldHit.ToString(),

            ParticleType.EnemyMeleeUnitHit.ToString(),

            ParticleType.BoosterOn.ToString(),
            ParticleType.BoosterPrepare.ToString(),
            ParticleType.BoosterGround.ToString(),
        };

        #endregion

        public static void LoadParticleList(string fileName, NodeBase sceneParent)
        {
            FrameworkCore.ParticleManager.Clear();

            //  load all particles from list file.
            FrameworkCore.ParticleManager.LoadParticleList(fileName, sceneParent);

            //  find particle indices.
            for (int i = 0; i < (int)ParticleType.Count; i++)
            {
                index[i] = 
                    FrameworkCore.ParticleManager.FindParticleIndexByName(szName[i]);
            }
        }

        public static ParticleSequence PlayParticle(ParticleType type, Vector3 position,
                                                    Vector3 normal, Matrix axis)
        {
            return FrameworkCore.ParticleManager.PlayParticle(index[(int)type],
                                                              position, normal, axis);
        }

        public static ParticleSequence PlayParticle(ParticleType type, 
                                                    Matrix world, Matrix axis)
        {
            return FrameworkCore.ParticleManager.PlayParticle(index[(int)type], 
                                                              world, axis);
        }
    }
}
