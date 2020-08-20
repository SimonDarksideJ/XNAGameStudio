#region File Description
//-----------------------------------------------------------------------------
// RocketProjectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// A rocket projectile.
    /// </summary>
    class RocketProjectile : Projectile
    {
        #region Fields
        /// <summary>
        /// The sound effect of the rocket as it flies.
        /// </summary>
        protected Cue rocketCue = null;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new rocket projectile.
        /// </summary>
        /// <param name="world">The world that this projectile belongs to.</param>
        /// <param name="owner">The ship that fired this projectile, if any.</param>
        /// <param name="direction">The initial direction for this projectile.</param>
        public RocketProjectile(World world, Ship owner, Vector2 direction)
            : base(world, owner, direction)
        {
            this.radius = 8f;
            this.life = 80f;
            this.mass = 3f;
            this.speed = 520f;
            this.duration = 4f;
            this.damageAmount = 100f;
            this.damageOwner = false;
            this.damageRadius = 128f;
            this.explodes = true;
            this.explosionColors = new Color[] 
                { Color.Orange, Color.Gray, Color.Gray, Color.Silver };
            this.polygon = VectorPolygon.CreateRocket();
            this.color = Color.Orange;
        }
        #endregion

        #region Interaction
        /// <summary>
        /// Damages all actors in a radius around the rocket.
        /// </summary>
        /// <param name="touchedActor">The actor that was originally hit.</param>
        public override void Explode(Actor touchedActor)
        {
            // stop the rocket-flying cue
            if (rocketCue != null)
            {
                rocketCue.Stop(AudioStopOptions.Immediate);
                rocketCue.Dispose();
                rocketCue = null;
            }

            // play the explosion cue
            world.AudioManager.PlayCue("explosionMedium");

            // add a double-particle system effect
            world.ParticleSystems.Add(new ParticleSystem(this.position,
                Vector2.Zero, 64, 32f, 64f, 3f, 0.05f, explosionColors));
            world.ParticleSystems.Add(new ParticleSystem(this.position,
                Vector2.Zero, 16, 128f, 256f, 4f, 0.1f, explosionColors));

            base.Explode(touchedActor);
        }


        /// <summary>
        /// Place this rocket in the world.
        /// </summary>
        /// <param name="findSpawnPoint">
        /// If true, the rocket's position is changed to a valid, non-colliding point.
        /// </param>
        public override void Spawn(bool findSpawnPoint)
        {
            base.Spawn(findSpawnPoint);

            // get and play the rocket-flying cue
            rocketCue = world.AudioManager.GetCue("rocket");
            if (rocketCue != null)
            {
                rocketCue.Play();
            }
            // twitch the motors of the launching ship
            owner.FireGamepadMotors(0f, 0.25f);
        }
        #endregion
    }
}
