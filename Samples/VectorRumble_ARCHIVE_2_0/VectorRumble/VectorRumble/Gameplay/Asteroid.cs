#region File Description
//-----------------------------------------------------------------------------
// Asteroid.cs
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
    /// Asteroids that fill the game simulation, blocking the player's 
    /// shots and movements.
    /// </summary>
    class Asteroid : Actor
    {
        #region Constants
        /// <summary>
        /// The ratio between the mass and the radius of an asteroid.
        /// </summary>
        const float massRadiusRatio = 4f;

        /// <summary>
        /// The amount of drag applied to velocity per second, 
        /// as a percentage of velocity.
        /// </summary>
        const float dragPerSecond = 0.20f;

        /// <summary>
        /// Scalar for calculated damage values that asteroids apply to players.
        /// </summary>
        const float damageScalar = 0.001f;

        /// <summary>
        /// Scalar to convert the velocity / mass ratio into a "nice" rotational value.
        /// </summary>
        const float velocityMassRatioToRotationScalar = 0.01f;
        #endregion

        #region Initialization
        /// <summary>
        /// Construct a new asteroid.
        /// </summary>
        /// <param name="world">The world that this asteroid belongs to.</param>
        /// <param name="radius">The size of the asteroid.</param>
        public Asteroid(World world, float radius)
            : base(world)
        {
            // all asteroids are gray
            this.color = Color.Gray;
            // create the polygon
            this.polygon = VectorPolygon.CreateAsteroid(radius);
            // the asteroid polygon might not be as big as the original radius, 
            // so find out how big it really is
            for (int i = 0; i < this.polygon.Points.Length; i++)
            {
                float length = this.polygon.Points[i].Length();
                if (length > this.radius)
                {
                    this.radius = length;
                }
            }
            // calculate the mass
            this.mass = radius * massRadiusRatio;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the actor.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public override void Update(float elapsedTime)
        {
            // spin the asteroid based on the size and velocity
            this.rotation += (this.velocity.LengthSquared() / this.mass) * elapsedTime *
                velocityMassRatioToRotationScalar;

            // apply some drag so the asteroids settle down
            velocity -= velocity * (elapsedTime * dragPerSecond);

            base.Update(elapsedTime);
        }
        #endregion

        #region Interaction
        /// <summary>
        /// Defines the interaction between the asteroid and a target actor
        /// when they touch.
        /// </summary>
        /// <param name="target">The actor that is touching this object.</param>
        /// <returns>True if the objects meaningfully interacted.</returns>
        public override bool Touch(Actor target)
        {
            // if the asteroid has touched a player, then damage it
            Ship player = target as Ship;
            if (player != null)
            {
                // calculate damage as a function of how much the two actor's
                // velocities were going towards one another
                Vector2 playerAsteroidVector = 
                    Vector2.Normalize(this.position - player.Position);
                float rammingSpeed = 
                    Vector2.Dot(playerAsteroidVector, player.Velocity) - 
                    Vector2.Dot(playerAsteroidVector, this.velocity);
                player.Damage(this, this.mass * rammingSpeed * damageScalar);

            }
            // if the asteroid didn't hit a projectile, play the asteroid-touch cue
            if ((target is Projectile) == false)
            {
                this.world.AudioManager.PlayCue("asteroidTouch");
            }
            return base.Touch(target);
        }

        
        /// <summary>
        /// Damage this asteroid by the amount provided.
        /// </summary>
        /// <remarks>
        /// This function is provided in lieu of a Life mutation property to allow 
        /// classes of objects to restrict which kinds of objects may damage them,
        /// and under what circumstances they may be damaged.
        /// </remarks>
        /// <param name="source">The actor responsible for the damage.</param>
        /// <param name="damageAmount">The amount of damage.</param>
        /// <returns>If true, this object was damaged.</returns>
        public override bool Damage(Actor source, float damageAmount)
        {
            // nothing hurst asteroids, nothing!
            return false;
        }
        #endregion
    }
}
