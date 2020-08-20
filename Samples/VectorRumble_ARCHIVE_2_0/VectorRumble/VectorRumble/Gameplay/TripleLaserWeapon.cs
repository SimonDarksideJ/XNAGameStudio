#region File Description
//-----------------------------------------------------------------------------
// TripleLaserWeapon.cs
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
    /// A weapon that shoots a triple stream of laser projectiles.
    /// </summary>
    class TripleLaserWeapon : LaserWeapon
    {
        #region Constants
        /// <summary>
        /// The spread of the second and third laser projectiles' directions, in radians
        /// </summary>
        static readonly float laserSpreadRadians = MathHelper.ToRadians(2.5f);
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new triple-laser weapon.
        /// </summary>
        /// <param name="owner">The ship that owns this weapon.</param>
        public TripleLaserWeapon(Ship owner)
            : base(owner) { }
        #endregion

        #region Interaction
        /// <summary>
        /// Create and spawn the projectile(s) from a firing from this weapon.
        /// </summary>
        /// <param name="direction">The direction that the projectile will move.</param>
        protected override void CreateProjectiles(Vector2 direction)
        {
            // calculate the direction vectors for the second and third projectiles
            float rotation = (float)Math.Acos(Vector2.Dot(new Vector2(0f, -1f), 
                direction));
            rotation *= (Vector2.Dot(new Vector2(0f, -1f), 
                new Vector2(direction.Y, -direction.X)) > 0f) ? 1f : -1f;
            Vector2 direction2 = new Vector2(
                 (float)Math.Sin(rotation - laserSpreadRadians), 
                -(float)Math.Cos(rotation - laserSpreadRadians));
            Vector2 direction3 = new Vector2(
                 (float)Math.Sin(rotation + laserSpreadRadians), 
                -(float)Math.Cos(rotation + laserSpreadRadians));

            // create the first projectile
            LaserProjectile projectile = new LaserProjectile(owner.World, owner,
                direction);
            // spawn the projectile
            projectile.Spawn(false);

            // create the second projectile
            projectile = new LaserProjectile(owner.World, owner,
                direction2);
            // spawn the projectile
            projectile.Spawn(false);

            // create the third projectile
            projectile = new LaserProjectile(owner.World, owner,
                direction3);
            // spawn the projectile
            projectile.Spawn(false);
        }
        #endregion
    }
}
