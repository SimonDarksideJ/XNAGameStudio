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
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
#endregion

namespace Spacewar
{
    /// <summary>
    /// Represents a projectile in SpacewarGame - retro or evolved
    /// </summary>
    public class Projectile : SpacewarSceneItem
    {
        private static int[] projectileCount = new int[3];

        /// <summary>
        /// Which player this projectile came from 
        /// </summary>
        private PlayerIndex player;

        /// <summary>
        /// The damage that this bullet does
        /// </summary>
        private int damage;

        private double endTime;

        private int projectileType;

        private bool exploded = false;

        private Particles particles;

        private bool projectileArmed;

        private Vector3 thrust;

        #region Properties
        public static int[] ProjectileCount
        {
            get
            {
                return projectileCount;
            }
        }

        public int Damage
        {
            get
            {
                return damage;
            }
        }
        #endregion
        /// <summary>
        /// Cretes a new projectile
        /// </summary>
        /// <param name="game">Instance of the game</param>
        /// <param name="player">Which player it came from</param>
        /// <param name="position">The start position</param>
        /// <param name="velocity">The start velocity</param>
        /// <param name="angle">The direction its facing</param>
        /// <param name="time">The time the projectile was fired</param>
        public Projectile(Game game, PlayerIndex player, Vector3 position, Vector3 velocity, float angle, TimeSpan time, Particles particles)
            : base(game)
        {
            this.player = player;
            this.velocity = velocity;
            this.position = position;

            projectileCount[(int)player]++;

            if (SpacewarGame.GameState == GameState.PlayEvolved)
            {
                projectileType = (int)SpacewarGame.Players[(int)player].ProjectileType;

                this.particles = particles;

                endTime = time.TotalSeconds + SpacewarGame.Settings.Weapons[projectileType].Lifetime;
                thrust = Vector3.Multiply(Vector3.Normalize(velocity), SpacewarGame.Settings.Weapons[projectileType].Acceleration);
                radius = 2;

                damage = SpacewarGame.Settings.Weapons[projectileType].Damage;

                if (SpacewarGame.GameState == GameState.PlayEvolved)
                {
                    shape = new BasicEffectShape(GameInstance, BasicEffectShapes.Projectile, projectileType, LightingType.InGame);

                    //Evolved needs scaling
                    scale.X =
                    scale.Y =
                    scale.Z = SpacewarGame.Settings.BulletScale;

                    rotation.X = MathHelper.ToRadians(90);
                    rotation.Y = 0;
                    rotation.Z = (float)angle;
                }
            }
            else
            {
                //Build up a retro weapon
                damage = 5; //One shot kill
                projectileType = (int)ProjectileType.Peashooter;
                radius = 1;

                endTime = time.TotalSeconds + 2.0;
                acceleration = Vector3.Zero;
            }

            //Play 'shoot' sound
            switch (projectileType)
            {
                case (int)ProjectileType.Peashooter:
                    Sound.PlayCue(Sounds.PeashooterFire);
                    break;

                case (int)ProjectileType.MachineGun:
                    Sound.PlayCue(Sounds.MachineGunFire);
                    break;

                case (int)ProjectileType.DoubleMachineGun:
                    Sound.PlayCue(Sounds.DoubleMachineGunFire);
                    break;

                case (int)ProjectileType.Rocket:
                    Sound.PlayCue(Sounds.RocketExplode);
                    break;

                case (int)ProjectileType.BFG:
                    Sound.PlayCue(Sounds.BFGFire);
                    break;
            }
        }

        /// <summary>
        /// Updates the bullet. Removes it from scene when item is timed out
        /// </summary>
        /// <param name="time">Current game time</param>
        /// <param name="elapsedTime">Elapsed time since last update</param>
        public override void Update(TimeSpan time, TimeSpan elapsedTime)
        {
            //See if this bullets lifespan has expired
            if (time.TotalSeconds > endTime)
            {
                //BFG explodes and has a blast radius
                if (SpacewarGame.GameState == GameState.PlayEvolved && projectileType == 4 && !exploded)
                {
                    Sound.PlayCue(Sounds.Explosion);
                    particles.AddExplosion(Position);

                    //We don't delete it this frame but we change the radius and the damage
                    exploded = true;
                    radius = 30;
                    damage = 3;
                }
                else
                {
                    DeleteProjectile();
                }
            }

            acceleration = thrust;

            //For the rocket we need particles
            if (projectileType == 3)
            {
                particles.AddRocketTrail(shape.World, new Vector2(acceleration.X, -acceleration.Y));
            }

            base.Update(time, elapsedTime);
        }

        public void DeleteProjectile()
        {
            if (!delete)
                projectileCount[(int)player]--;
            delete = true;
        }

        /// <summary>
        /// Checks if there is a collision between the this and the passed in item
        /// </summary>
        /// <param name="item">A scene item to check</param>
        /// <returns>True if there is a collision</returns>
        public override bool Collide(SceneItem item)
        {
            // Until we get collision meshes sorted just do a simple sphere (well circle!) check
            float currentDistance = (Position - item.Position).Length();
            bool colliding = base.Collide(item);

            // For projectiles, do not allow them to destroy the ship that just fired them!
            Ship shipItem = item as Ship;
            if ((shipItem != null) && (shipItem.Player == player))
            {
                if (colliding && !projectileArmed)
                {
                    colliding = false;
                }
                else if (!colliding && !projectileArmed)
                {
                    // Once projectile is at least 2 ship radii away, it can arm!
                    // This is becuase the bolt launches with your velocity at the time
                    // and the ship can "catch up" to the bolt pretty easily if you are
                    // thrusting in the direction you are firing.
                    if (currentDistance > item.Radius * 2.0f)
                        projectileArmed = true;
                }
            }

            return colliding;
        }
    }
}
