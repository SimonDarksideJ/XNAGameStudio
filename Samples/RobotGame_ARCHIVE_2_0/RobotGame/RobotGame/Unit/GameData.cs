#region File Description
//-----------------------------------------------------------------------------
// GameData.cs
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
using RobotGameData.Helper;
#endregion

namespace RobotGame
{
    #region Enums

    public enum UnitClassId
    {
        Unknown = 0,
        LightMech,
        HeavyMech,
        Tank,
        Boss,

        Count
    }

    public enum UnitTypeId
    {
        Unknown = 0,

        //  Players
        Grund,
        Kiev,
        Mark,
        Yager,

        //  Enemies
        Cameleer,
        Maoming,
        Duskmas,
        Hammer,
        Tiger,
        PhantomBoss,

        Count
    }

    #endregion

    #region GameDataSpec

    /// <summary>
    /// game data's base structure.
    /// </summary>
    [Serializable]
    public class GameDataSpec
    {
        public string sourceFilePath = String.Empty;
    }

    #endregion

    #region GamePlayerSpec

    /// <summary>
    /// It has a number that shows the player’s capabilities 
    /// and is used as reference during game play. 
    /// The class reads from a player spec file(Content/Data/Players/<*>.spec) 
    /// and stores the values.
    /// The Player spec file is in XML format and the values can be easily changed.
    /// </summary>
    [Serializable]
    public class GamePlayerSpec : GameDataSpec
    {
        public UnitTypeId UnitType = UnitTypeId.Unknown;
        public UnitClassId UnitClass = UnitClassId.Unknown;

        public int Life = 0;

        public float MechRadius = 0.0f;
        public float RunSpeed = 0.0f;
        public float WalkSpeed = 0.0f;
        public float WalkBackwardSpeed = 0.0f;
        public float TurnAngle = 0.0f;
        public float CriticalDamagedTime = 0.0f;
        public float BoosterSpeed = 0.0f;
        public float BoosterActiveTime = 0.0f;
        public float BoosterPrepareTime = 0.0f;
        public float BoosterCoolTime = 0.0f;
        public float BoosterTurnAngle = 0.0f;

        public string ModelFilePath = String.Empty;
        public string AnimationFolderPath = String.Empty;
        public string DefaultWeaponFilePath = String.Empty;

        public Vector3 CameraTargetOffset = Vector3.Zero;
        public Vector3 CameraPositionOffset = Vector3.Zero;
    }

    #endregion

    #region GameEnemySpec

    /// <summary>
    /// It has a number that shows the enemy’s capabilities and 
    /// is used as reference during game play. 
    /// The class reads from a enemy spec file(Content/Data/Enemies/<*>.spec)
    /// and stores the values.
    /// The enemy spec file is in XML format and the values can be easily changed.
    /// </summary>
    [Serializable]
    public class GameEnemySpec : GameDataSpec
    {
        public UnitTypeId UnitType = UnitTypeId.Unknown;
        public UnitClassId UnitClass = UnitClassId.Unknown;

        public int Life = 0;

        public float MechRadius = 0.0f;
        public float MoveSpeed = 0.0f;
        public float TurnAngle = 0.0f;

        public string ModelFilePath = String.Empty;
        public string AnimationFolderPath = String.Empty;
        public string DefaultWeaponFilePath = String.Empty;
    }

    #endregion

    /// <summary>
    /// a manager which manages the game data.
    /// Provides an interface to loading game data and 
    /// the loaded game data is stored in the list.
    /// </summary>
    public static class GameDataSpecManager
    {
        #region Fields

        static Dictionary<string, GameDataSpec> specList =
                                    new Dictionary<string, GameDataSpec>();

        #endregion

        /// <summary>
        /// loads a game data from file.
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="type">object type</param>
        /// <returns></returns>
        public static GameDataSpec Load(string fileName, Type type)
        {
            GameDataSpec spec = FindSpec(fileName);
            if (spec == null)
            {
                spec = (GameDataSpec)HelperFile.LoadData(fileName, type);
                spec.sourceFilePath = fileName;

                specList.Add(fileName, spec);
            }

            return spec;
        }

        /// <summary>
        /// seaches a game data in the list.
        /// </summary>
        /// <param name="fileName">game data file name</param>
        /// <returns>gama data structure</returns>
        public static GameDataSpec FindSpec(string fileName)
        {
            if (specList.ContainsKey(fileName) )
            {
                return specList[fileName];
            }

            return null;
        }

        /// <summary>
        /// clear all game datas.
        /// </summary>
        public static void Clear()
        {
            specList.Clear();
        }
    }
}
