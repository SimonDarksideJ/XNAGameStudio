#region File Description
//-----------------------------------------------------------------------------
// RocketWeapon.cs
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
    /// A weapon that shoots a rockets.
    /// </summary>
    class RocketWeapon : Weapon
    {
        #region Initialization
        /// <summary>
        /// Constructs a new rocket-launching weapon.
        /// </summary>
        /// <param name="owner">The ship that owns this weapon.</param>
        public RocketWeapon(Ship owner)
            : base(owner)
        {
            fireDelay = 0.75f;
            fireCueName = "rocketFire";            
        }
        #endregion

        #region Interaction
        /// <summary>
        /// Create and spawn the projectile(s) from a firing from this weapon.
        /// </summary>
        /// <param name="direction">The direction that the projectile will move.</param>
        protected override void CreateProjectiles(Vector2 direction)
        {
            // calculate the rocket's rotation
            float rotation = (float)Math.Acos(Vector2.Dot(new Vector2(0f, -1f),
                direction));
            rotation *= (Vector2.Dot(new Vector2(0f, -1f),
                new Vector2(direction.Y, -direction.X)) > 0f) ? 1f : -1f;

            // create the new projectile
            RocketProjectile projectile = new RocketProjectile(owner.World, owner,
                direction);
            projectile.Rotation = rotation;
            // spawn the projectile
            projectile.Spawn(false);
        }
        #endregion
    }
}
