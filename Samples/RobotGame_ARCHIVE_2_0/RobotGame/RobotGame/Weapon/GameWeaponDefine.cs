#region File Description
//-----------------------------------------------------------------------------
// GameWeaponDefine.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace RobotGame
{
    #region Enum

    public enum WeaponType
    {
        Unknown = 0,

        CameleerGun,
        MaomingGun,
        DuskmasCannon,
        HammerCannon,
        TigerCannon,
        PhantomMelee,

        PlayerMachineGun,
        PlayerHandgun,
        PlayerShotgun,
    }

    #endregion

    /// <summary>
    /// Weapon specifications contain graphical and gameplay data about each weapon.
    /// The class reads from a weapon spec file (Content/Data/Players or 
    /// Enemies/<*>.spec) and stores the values.
    /// The weapon spec file is in XML format and the values can be easily changed.
    /// </summary>
    [Serializable]
    public class GameWeaponSpec : GameDataSpec
    {
        public WeaponType Type = WeaponType.Unknown;

        public int Damage = 0;        
        public int FireCount = 0;

        public float ModelRadius = 0.0f;
        public float FireRange = 0.0f;

        public int TotalBulletCount = 0;
        public int ReloadBulletCount = 0;

        public float FireIntervalTime = 0.0f;
        public float ReloadIntervalTime = 0.0f;
        public float FireDelayTimeTillFirst = 0.0f;        
       
        public int FireHorizontalTiltAngle = 0;
        public int FireVerticalTiltAngle = 0;     

        public bool CriticalDamagedFire = false;

        public bool ModelAlone = false;
        public int ModelCount = 1;
        public string ModelFilePath = String.Empty;
        public string AttachBone = String.Empty;
        public string MuzzleBone = String.Empty;

        public bool TracerBulletFire = false;
        public float TracerBulletSpeed = 0.0f;
        public float TracerBulletLength = 0.0f;
        public float TracerBulletThickness = 0.0f;
    }
}