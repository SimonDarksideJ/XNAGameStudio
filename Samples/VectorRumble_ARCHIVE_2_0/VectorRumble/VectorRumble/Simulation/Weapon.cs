#region File Description
//-----------------------------------------------------------------------------
// Weapon.cs
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
    /// Base class for all weapons that exist in the game.
    /// </summary>
    abstract class Weapon
    {
        #region Fields
        /// <summary>
        /// The ship that owns this weapon.
        /// </summary>
        protected Ship owner = null;

        /// <summary>
        /// The amount of time remaining before this weapon can fire again.
        /// </summary>
        protected float timeToNextFire = 0f;

        /// <summary>
        /// The minimum amount of time between each firing of this weapon.
        /// </summary>
        protected float fireDelay = 0f;

        /// <summary>
        /// The name of the audio cue played when this weapon fires.
        /// </summary>
        protected string fireCueName = String.Empty;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new weapon.
        /// </summary>
        /// <param name="owner">The ship that owns this weapon.</param>
        public Weapon(Ship owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this.owner = owner;
        }
        #endregion

        #region Update
        public virtual void Update(float elapsedTime)
        {
            // count down to when the weapon can fire again
            if (timeToNextFire > 0f)
            {
                timeToNextFire = MathHelper.Max(timeToNextFire - elapsedTime, 0f);
            }
        }
        #endregion

        #region Interaction
        /// <summary>
        /// Fire the weapon in the direction given.
        /// </summary>
        /// <param name="direction">The direction that the weapon is firing in.</param>
        public virtual void Fire(Vector2 direction)
        {
            // if we can't fire yet, then we're done
            if (timeToNextFire > 0f)
            {
                return;
            }

            // the owner is no longer safe from damage
            owner.Safe = false;

            // set the timer
            timeToNextFire = fireDelay;

            // create and spawn the projectile
            CreateProjectiles(direction);

            // play the audio cue for firing
            if (String.IsNullOrEmpty(fireCueName) == false)
            {
                this.owner.World.AudioManager.PlayCue(fireCueName);
            }
        }


        /// <summary>
        /// Create and spawn the projectile(s) from a firing from this weapon.
        /// </summary>
        /// <param name="direction">The direction that the projectile will move.</param>
        protected abstract void CreateProjectiles(Vector2 direction);
        #endregion
    }
}
