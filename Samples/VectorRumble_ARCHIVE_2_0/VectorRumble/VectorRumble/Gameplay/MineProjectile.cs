#region File Description
//-----------------------------------------------------------------------------
// MineProjectile.cs
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
    /// A mine projectile.
    /// </summary>
    class MineProjectile : Projectile
    {
        #region Constants
        /// <summary>
        /// The amount of drag applied to velocity per second, 
        /// as a percentage of velocity.
        /// </summary>
        const float dragPerSecond = 0.9f;

        /// <summary>
        /// The radians-per-second that this object rotates at.
        /// </summary>
        const float rotationRadiansPerSecond = 1f;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new mine projectile.
        /// </summary>
        /// <param name="world">The world that this projectile belongs to.</param>
        /// <param name="owner">The ship that fired this projectile, if any.</param>
        /// <param name="direction">The initial direction for this projectile.</param>
        public MineProjectile(World world, Ship owner, Vector2 direction)
            : base(world, owner, direction)
        {
            this.radius = 16f;
            this.life = 15f;
            this.speed = 64f;
            this.duration = 15f;
            this.mass = 5f;
            this.damageAmount = 200f;
            this.damageOwner = true;
            this.damageRadius = 80f;
            this.explodes = true;
            this.explosionColors = new Color[] 
                { Color.Red, Color.Maroon, Color.White, Color.Silver };
            this.polygon = VectorPolygon.CreateMine();
            this.color = Color.Red;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the mine.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public override void Update(float elapsedTime)
        {
            base.Update(elapsedTime);

            this.velocity -= velocity * (elapsedTime * dragPerSecond);
            this.rotation += elapsedTime * rotationRadiansPerSecond;
        }
        #endregion

        #region Interaction
        /// <summary>
        /// Damages all actors in a radius around the mine.
        /// </summary>
        /// <param name="touchedActor">The actor that was originally hit.</param>
        public override void Explode(Actor touchedActor)
        {
            // play the explosion cue
            world.AudioManager.PlayCue("explosionLarge");

            // add a double-particle system effect
            world.ParticleSystems.Add(new ParticleSystem(this.position,
                Vector2.Zero, 64, 32f, 64f, 3f, 0.05f, explosionColors));
            world.ParticleSystems.Add(new ParticleSystem(this.position,
                Vector2.Zero, 16, 128f, 256f, 4f, 0.1f, explosionColors));

            base.Explode(touchedActor);
        }
        #endregion
    }
}
