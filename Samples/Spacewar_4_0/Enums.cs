#region File Description
//-----------------------------------------------------------------------------
// Enums.cs
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

namespace Spacewar
{
    /// <summary>
    /// This enum if for the state transitions.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Default value - means no state is set
        /// </summary>
        None,

        /// <summary>
        /// Nothing visible, game has just been run and nothing is initialized
        /// </summary>
        Started,

        /// <summary>
        /// Logo Screen is being displayed
        /// </summary>
        LogoSplash,

        /// <summary>
        /// Currently playing a version of the Evolved game
        /// </summary>
        PlayEvolved,

        /// <summary>
        /// Currently playing a version of the Evolved game
        /// </summary>
        PlayRetro,

        /// <summary>
        /// Choosing the ship
        /// </summary>
        ShipSelection,

        /// <summary>
        /// UpgradingWeapons
        /// </summary>
        ShipUpgrade,

        /// <summary>
        /// In the victory screen
        /// </summary>
        Victory,
    }

    /// <summary>
    /// The 5 types of projectiles
    /// </summary>
    public enum ProjectileType
    {
        /// <summary>
        /// Basic low damage default projectiles
        /// </summary>
        Peashooter,
        /// <summary>
        /// Fires multiple bullets per burst
        /// </summary>
        MachineGun,
        /// <summary>
        /// 2 streams of multiple bullets
        /// </summary>
        DoubleMachineGun,
        /// <summary>
        /// Rockets are single kill
        /// </summary>
        Rocket,
        /// <summary>
        /// BFG is single kill plus explosive damage
        /// </summary>
        BFG,
    }

    /// <summary>
    /// The types of ship available
    /// </summary>
    public enum ShipClass
    {
        /// <summary>
        /// Long and thin
        /// </summary>
        Pencil,

        /// <summary>
        /// Round ship
        /// </summary>
        Saucer,

        /// <summary>
        /// Square shape
        /// </summary>
        Wedge,
    }

    /// <summary>
    /// Spacewar has 2 groups of light settings for the ship.fx shader
    /// </summary>
    public enum LightingType
    {
        /// <summary>
        /// Use the lighting parameters setup for in game
        /// </summary>
        InGame,
        /// <summary>
        /// Use the lighting parameters setup for menus
        /// </summary>
        Menu,
    }
}
