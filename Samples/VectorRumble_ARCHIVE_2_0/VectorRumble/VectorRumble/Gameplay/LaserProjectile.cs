#region File Description
//-----------------------------------------------------------------------------
// LaserProjectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// A laser bolt projectile.
    /// </summary>
    class LaserProjectile : Projectile
    {
        #region Constants
        /// <summary>
        /// The length of the laser-bolt line, expressed as a percentage of velocity.
        /// </summary>
        const float lineLengthVelocityPercent = 0.01f;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new laser projectile.
        /// </summary>
        /// <param name="world">The world that this projectile belongs to.</param>
        /// <param name="owner">The ship that fired this projectile, if any.</param>
        /// <param name="direction">The initial direction for this projectile.</param>
        public LaserProjectile(World world, Ship owner, Vector2 direction)
            : base(world, owner, direction)
        {
            this.radius = 0.5f;
            this.speed = 640f;
            this.duration = 5f;
            this.damageAmount = 20f;
            this.damageOwner = false;
            this.mass = 0.5f;
            this.explodes = false;
            this.explosionColors = new Color[] 
                { Color.White, Color.Gray, Color.Gray, Color.Silver, Color.Yellow };
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Render the actor.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="lineBatch">The LineBatch to render to.</param>
        public override void Draw(float elapsedTime, LineBatch lineBatch)
        {
            if (lineBatch == null)
            {
                throw new ArgumentNullException("lineBatch");
            }
            // draw a simple line
            lineBatch.DrawLine(position, 
                position - velocity * lineLengthVelocityPercent, Color.Yellow);
        }
        #endregion 

        #region Interaction
        /// <summary>
        /// Defines the interaction between this projectile and a target actor
        /// when they touch.
        /// </summary>
        /// <param name="target">The actor that is touching this object.</param>
        /// <returns>True if the objects meaningfully interacted.</returns>
        public override bool Touch(Actor target)
        {
            // add a particle effect if we touched anything
            if (base.Touch(target))
            {
                // make the particle effect slightly more significant if it was a ship
                if (target is Ship)
                {
                    world.ParticleSystems.Add(new ParticleSystem(this.position,
                        Vector2.Zero, 16, 32f, 64f, 1f, 0.1f, explosionColors));
                }
                else
                {
                    world.ParticleSystems.Add(new ParticleSystem(this.position,
                        Vector2.Zero, 4, 32f, 64f, 1f, 0.05f, explosionColors));
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}
