#region File Description
//-----------------------------------------------------------------------------
// Projectile.cs
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
    /// Base class for all projectiles that exist in the game.
    /// </summary>
    abstract class Projectile : Actor
    {
        #region Fields
        /// <summary>
        /// The player who fired this projectile.
        /// </summary>
        protected Ship owner;

        /// <summary>
        /// The speed that the projectile will move at.
        /// </summary>
        protected float speed = 0f;

        /// <summary>
        /// The amount that this projectile hurts it's target and those around it.
        /// </summary>
        protected float damageAmount = 0f;

        /// <summary>
        /// The radius at which this projectile hurts other actors when it explodes.
        /// </summary>
        protected float damageRadius = 0f;

        /// <summary>
        /// The amount of time before this projectile dies on it's own.
        /// </summary>
        protected float duration = 0f;

        /// <summary>
        /// If true, this object will damage it's owner if it hits it
        /// </summary>
        protected bool damageOwner = true;

        /// <summary>
        /// If true, this object explodes - calling Explode() - when it dies.
        /// </summary>
        protected bool explodes = false;

        /// <summary>
        /// The colors used in the particle system shown when this projectile hits.
        /// </summary>
        protected Color[] explosionColors;
        #endregion

        #region Properties
        public Ship Owner
        {
            get { return owner; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new projectile.
        /// </summary>
        /// <param name="world">The world that this projectile belongs to.</param>
        /// <param name="owner">The ship that fired this projectile, if any.</param>
        /// <param name="direction">The initial direction for this projectile.</param>
        public Projectile(World world, Ship owner, Vector2 direction)
            : base(world) 
        {
            this.owner = owner;
            this.position = owner.Position;
            this.velocity = direction;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the projectile.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public override void Update(float elapsedTime)
        {
            // projectiles can "time out"
            if (duration > 0f)
            {
                duration -= elapsedTime;
                if (duration < 0f)
                {
                    Die(null);
                }
            }

            base.Update(elapsedTime);
        }
        #endregion

        #region Interaction
        /// <summary>
        /// Damages all actors in a radius around the projectile.
        /// </summary>
        /// <param name="touchedActor">The actor that was originally hit.</param>
        public virtual void Explode(Actor touchedActor)
        {
            // if there is no radius, then don't bother
            if (damageRadius <= 0f)
            {
                return;
            }
            // check each actor for damage
            foreach (Actor actor in world.Actors)
            {
                // don't bother if it's already dead
                if (actor.Dead == true)
                {
                    continue;
                }
                // don't hurt the actor that the projectile hit, it's already hurt
                if (actor == touchedActor)
                {
                    continue;
                }
                // don't hit the owner if the damageOwner flag is off
                if ((actor == owner) && (damageOwner == false))
                {
                    continue;
                }
                // measure the distance to the actor and see if it's in range
                float distance = (actor.Position - this.Position).Length();
                if (distance <= damageRadius)
                {
                    // adjust the amount of damage based on the distance
                    // -- note that damageRadius <= 0 is accounted for earlier
                    float adjustedDamage = damageAmount *
                        (damageRadius - distance) / damageRadius;
                    // if we're still damaging the actor, then go ahead and apply it
                    if (adjustedDamage > 0f)
                    {
                        actor.Damage(this, adjustedDamage);
                    }
                }
            }
        }

        
        /// <summary>
        /// Defines the interaction between this projectile and a target actor
        /// when they touch.
        /// </summary>
        /// <param name="target">The actor that is touching this object.</param>
        /// <returns>True if the objects meaningfully interacted.</returns>
        public override bool Touch(Actor target)
        {
            // check the target, if we have one
            if (target != null)
            {
                // don't bother hitting any power-ups
                if (target is PowerUp)
                {
                    return false;
                }
                // don't hit the owner if the damageOwner flag isn't set
                if ((target == owner) && (this.damageOwner == false))
                {
                    return false;
                }
                // don't hit other projectiles from the same ship
                Projectile projectile = target as Projectile;
                if ((projectile != null) && (projectile.Owner == this.Owner))
                {
                    return false;
                }
                // damage the target
                target.Damage(this, this.damageAmount);
            }

            // either we hit something or the target is null - in either case, die
            Die(target);
            
            return base.Touch(target);
        }
        

        /// <summary>
        /// Kills this projectile, in response to the given actor.
        /// </summary>
        /// <param name="source">The actor responsible for the kill.</param>
        public override void Die(Actor source)
        {
            if (dead == false)
            {
                base.Die(source);
                if (dead && explodes)
                {
                    Explode(source);
                }
            }
        }

        /// <summary>
        /// Place this projectile in the world.
        /// </summary>
        /// <param name="findSpawnPoint">
        /// If true, the actor's position is changed to a valid, non-colliding point.
        /// </param>
        public override void Spawn(bool findSpawnPoint)
        {
            Vector2 newVelocity = speed * Vector2.Normalize(velocity);
            base.Spawn(findSpawnPoint);
            // reset the velocity to the speed times the current direction;
            velocity = newVelocity;
        }
        #endregion
    }
}
