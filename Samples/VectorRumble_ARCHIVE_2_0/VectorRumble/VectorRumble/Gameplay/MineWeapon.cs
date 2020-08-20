#region File Description
//-----------------------------------------------------------------------------
// MineWeapon.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// A weapon that fires a single mine on a long timer.
    /// </summary>
    class MineWeapon : Weapon
    {
        #region Constants
        /// <summary>
        /// The distance that the mine spawns behind the ship.
        /// </summary>
        const float mineSpawnDistance = 8f;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new mine-laying weapon.
        /// </summary>
        /// <param name="owner">The ship that owns this weapon.</param>
        public MineWeapon(Ship owner)
            : base(owner)
        {
            fireDelay = 2f;         
        }
        #endregion

        #region Interaction
        /// <summary>
        /// Create and spawn the projectile(s) from a firing from this weapon.
        /// </summary>
        /// <param name="direction">The direction that the projectile will move.</param>
        protected override void CreateProjectiles(Vector2 direction)
        {
            // create the new projectile
            MineProjectile projectile = new MineProjectile(owner.World, owner,
                direction);
            // move the mine out from the ship
            projectile.Position = owner.Position + 
                direction * (owner.Radius + projectile.Radius + mineSpawnDistance);
            // spawn the projectile
            projectile.Spawn(false);
        }
        #endregion
    }
}
