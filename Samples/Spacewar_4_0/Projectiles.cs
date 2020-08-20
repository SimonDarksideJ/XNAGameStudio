#region File Description
//-----------------------------------------------------------------------------
// Projectiles.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
#endregion

namespace Spacewar
{
    /// <summary>
    /// Projectiles handles collections of projectiles shot from the retro or evolved ship.
    /// Its useful to have a a parent group because in Retro mode we want to batch render them
    /// </summary>
    public class Projectiles : SceneItem
    {
        public Projectiles(Game game)
            : base(game)
        {
            Projectile.ProjectileCount[0] = 0;
            Projectile.ProjectileCount[1] = 0;
            Projectile.ProjectileCount[2] = 0;
        }

        /// <summary>
        /// Creates a group of projectiles
        /// </summary>
        /// <param name="player">Which player shot the bullet</param>
        /// <param name="position">Start position of projectile</param>
        /// <param name="velocity">Initial velocity of projectile</param>
        /// <param name="angle">Direction projectile is facing</param>
        /// <param name="time">Game time that this projectile was shot</param>
        /// <param name="particles">The particles to add to for effects</param>
        public virtual void Add(PlayerIndex player, Vector3 position, Vector3 velocity, float angle, TimeSpan time, Particles particles)
        {
            ProjectileType projectileType = SpacewarGame.Players[(int)player].ProjectileType;

            Vector3 offset = Vector3.Zero;

            if (SpacewarGame.Players[(int)player].ProjectileType == ProjectileType.DoubleMachineGun)
            {
                //Get a perpendicular vector to the direction of fire to offset the double shot
                offset.X = -velocity.Y;
                offset.Y = velocity.X;
                offset.Normalize();
                offset *= 10.0f;
            }

            for (int i = 0; i < SpacewarGame.Settings.Weapons[(int)projectileType].Burst; i++)
            {
                //If we are not up to max then we can add bullets
                if (Projectile.ProjectileCount[(int)player] < SpacewarGame.Settings.Weapons[(int)projectileType].Max)
                {
                    Add(new Projectile(GameInstance, player, position + velocity * i * .1f + offset, velocity, angle, time, particles));
                    if (offset != Vector3.Zero)
                    {
                        Add(new Projectile(GameInstance, player, position + velocity * i * .1f - offset, velocity, angle, time, particles));
                    }
                }
            }
        }

        /// <summary>
        /// Removes all elements from the <see cref="T:System.Collections.Generic.List`1"></see>.
        /// This method hides the real method and just marks the items as ready to be deleted
        /// to avoid updating the collection while in a loop and to allow us to maintain the correct
        /// bullet count
        /// </summary>
        public new void Clear()
        {
            foreach (Projectile projectile in this)
            {
                projectile.DeleteProjectile();
            }
        }
    }
}
