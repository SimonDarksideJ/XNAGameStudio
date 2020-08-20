#region File Description
//-----------------------------------------------------------------------------
// SpacewarScreen.cs
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
    public class SpacewarScreen : Screen
    {
        protected Projectiles bullets;
        protected Ship ship1;
        protected Ship ship2;
        protected SceneItem sun;
        protected bool paused = true;
        protected int player1Score;
        protected int player2Score;
        protected Particles particles;
        protected SceneItem backdrop;

        public SpacewarScreen(Game game)
            : base(game)
        {
        }


        /// <summary>
        /// Checks for collisions between objects
        /// </summary>
        protected virtual void handleCollisions(TimeSpan gameTime)
        {
            if (!paused)
            {
                //Sun always does 5 damage i.e kills you
                if (sun.Collide(ship1) && !ship1.Uncollidable)
                    HitPlayer1(gameTime, 5);

                if (sun.Collide(ship2) && !ship2.Uncollidable)
                    HitPlayer2(gameTime, 5);

                //Ship collisions do 2 damage and bounce you
                if (ship1.Collide(ship2) && !ship1.Uncollidable && !ship2.Uncollidable)
                {
                    if (SpacewarGame.GameState == GameState.PlayEvolved)
                    {
                        HitPlayer1(gameTime, 2);
                        HitPlayer2(gameTime, 2);

                        float ship1Speed = ship1.Velocity.Length();
                        float ship2Speed = ship2.Velocity.Length();

                        Vector3 tmp = ship1.Velocity;
                        Vector3 vel = ship2.Velocity;
                        if (vel.LengthSquared() > 0.0f)
                            vel.Normalize();
                        ship1.Velocity = vel * ship1Speed;

                        if (tmp.LengthSquared() > 0.0f)
                            tmp.Normalize();
                        ship2.Velocity = tmp * ship2Speed;
                    }
                    else
                    {
                        //Retro is instant kill
                        HitPlayer1(gameTime, 5);
                        HitPlayer2(gameTime, 5);
                    }
                }

                foreach (Projectile bullet in bullets)
                {
                    if (bullet.Collide(sun))
                        bullet.DeleteProjectile();

                    //Bullets to differing amounts of damage depending on the bullet type
                    if (bullet.Collide(ship1) && !ship1.Uncollidable)
                    {
                        bullet.DeleteProjectile();
                        HitPlayer1(gameTime, bullet.Damage);
                    }

                    if (bullet.Collide(ship2) && !ship2.Uncollidable)
                    {
                        bullet.DeleteProjectile();
                        HitPlayer2(gameTime, bullet.Damage);
                    }
                }
            }
        }

        protected void HitPlayer2(TimeSpan gameTime, int damage)
        {
            SpacewarGame.Players[1].Health -= damage;
            if (SpacewarGame.Players[1].Health <= 0)
            {
                SpacewarGame.Players[1].Health = 5;
                if (SpacewarGame.GameState == GameState.PlayEvolved)
                    particles.AddExplosion(ship2.Position);

                player1Score++;
                Sound.PlayCue(Sounds.ExplodeShip);

                resetShips(gameTime);
            }
            else
            {
                Sound.PlayCue(Sounds.DamageShip);
            }
        }


        protected void HitPlayer1(TimeSpan gameTime, int damage)
        {
            SpacewarGame.Players[0].Health -= damage;
            if (SpacewarGame.Players[0].Health <= 0)
            {
                SpacewarGame.Players[0].Health = 5;
                if (SpacewarGame.GameState == GameState.PlayEvolved)
                    particles.AddExplosion(ship1.Position);

                player2Score++;
                Sound.PlayCue(Sounds.ExplodeShip);

                resetShips(gameTime);
            }
            else
            {
                Sound.PlayCue(Sounds.DamageShip);
            }
        }

        private void resetShips(TimeSpan gameTime)
        {
            //When either player dies reset both ships to their start positions
            if (SpacewarGame.GameState == GameState.PlayEvolved)
            {
                ship2.ResetShip(gameTime, new Vector3(SpacewarGame.Settings.Ships[1].StartPosition, 0.0f));
                ship1.ResetShip(gameTime, new Vector3(SpacewarGame.Settings.Ships[0].StartPosition, 0.0f));
            }
            else
            {
                ship2.ResetShip(gameTime, new Vector3(250, 0, 0));
                ship1.ResetShip(gameTime, new Vector3(-250, 0, 0));
            }

            ship1.Silence();
            ship2.Silence();

            //and remove any bullets
            bullets.Clear();
        }

        /// <summary>
        /// Tidies up anything that may need tidying up
        /// </summary>
        public override void Shutdown()
        {
            //Quiet any ship noises
            ship1.Silence();
            ship2.Silence();

            base.Shutdown();
        }

        /// <summary>
        /// OnCreateDevice is called when the device is created
        /// </summary>
        public override void OnCreateDevice()
        {
            base.OnCreateDevice();

            ship1.OnCreateDevice();
            ship2.OnCreateDevice();
            ship1.ShapeItem.OnCreateDevice();
            ship2.ShapeItem.OnCreateDevice();
            sun.ShapeItem.OnCreateDevice();
            backdrop.ShapeItem.OnCreateDevice();
        }
    }
}
