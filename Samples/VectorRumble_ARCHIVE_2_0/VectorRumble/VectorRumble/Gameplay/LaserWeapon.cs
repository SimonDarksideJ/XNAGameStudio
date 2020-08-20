#region File Description
//-----------------------------------------------------------------------------
// LaserWeapon.cs
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
    /// A weapon that shoots a single stream of laser projectiles.
    /// </summary>
    class LaserWeapon : Weapon
    {
        #region Initialization
        /// <summary>
        /// Constructs a new laser weapon.
        /// </summary>
        /// <param name="owner">The ship that owns this weapon.</param>
        public LaserWeapon(Ship owner)
            : base(owner)
        {
            fireDelay = 0.15f;
            fireCueName = "laserBlaster";            
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
            LaserProjectile projectile = new LaserProjectile(owner.World, owner,
                direction);
            // spawn the projectile
            projectile.Spawn(false);
        }
        #endregion
    }
}
