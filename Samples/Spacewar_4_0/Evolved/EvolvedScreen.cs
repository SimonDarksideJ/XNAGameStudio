#region File Description
//-----------------------------------------------------------------------------
// EvolvedScreen.cs
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
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
#endregion

namespace Spacewar
{
    /// <summary>
    /// SpacewarScreen is the main game screen
    /// </summary>
    public class EvolvedScreen : SpacewarScreen
    {
        private double sunScale = 1000.0;
        private Asteroid[] asteroids;

        private float levelTime;
        private int lastLevelTime;
        private bool ended;
        private double endTime;

        private static string[] scoreLookup = new string[] { "000", "001", "011", "111" };

        private static Vector3[] asteroidStarts = new Vector3[]
        {
            new Vector3(390, 240, 0),
            new Vector3(-390, -240, 0),
            new Vector3(390, -240, 0),
            new Vector3(-390, 240,0),
            new Vector3(-390, 240,0),
            new Vector3(390, 240,0),
            new Vector3(-390, -240,0)
        };

        private Random random = new Random();

        /// <summary>
        /// Creates a new SpacewarScreen
        /// </summary>
        public EvolvedScreen(Game game)
            : base(game)
        {
            backdrop = new SceneItem(game, new EvolvedBackdrop(game));
            const float factor = 46;
            backdrop.Center = new Vector3(.5f, .5f, 0);
            backdrop.Scale = new Vector3(16f * factor, 9f * factor, 1f);
            backdrop.Position = new Vector3(-.5f, -.5f, 0);
            scene.Add(backdrop);

            bullets = new Projectiles(game);
            particles = new Particles(game);

            ship1 = new Ship(game, PlayerIndex.One, SpacewarGame.Players[0].ShipClass, SpacewarGame.Players[0].Skin, new Vector3(SpacewarGame.Settings.Ships[0].StartPosition, 0.0f), bullets, particles);
            ship1.Paused = true;
            ship1.Radius = 15f;
            if (SpacewarGame.Players[0].ShipClass == ShipClass.Pencil)
            {
                ship1.ExtendedExtent[0] = new Vector3(0.0f, 25.0f, 0.0f);
                ship1.ExtendedExtent[1] = new Vector3(0.0f, -25.0f, 0.0f);
            }

            ship2 = new Ship(game, PlayerIndex.Two, SpacewarGame.Players[1].ShipClass, SpacewarGame.Players[1].Skin, new Vector3(SpacewarGame.Settings.Ships[1].StartPosition, 0.0f), bullets, particles);
            ship2.Paused = true;
            ship2.Radius = 15f;
            if (SpacewarGame.Players[1].ShipClass == ShipClass.Pencil)
            {
                ship2.ExtendedExtent[0] = new Vector3(0.0f, 25f, 0.0f);
                ship2.ExtendedExtent[1] = new Vector3(0.0f, -25f, 0.0f);
            }

            scene.Add(bullets);

            asteroids = new Asteroid[SpacewarGame.GameLevel + 2];

            for (int i = 0; i < SpacewarGame.GameLevel + 2; i++)
            {

                asteroids[i] = new Asteroid(game, random.NextDouble() > .5 ? AsteroidType.Large : AsteroidType.Small, asteroidStarts[i]);
                asteroids[i].Scale = new Vector3(SpacewarGame.Settings.AsteroidScale, SpacewarGame.Settings.AsteroidScale, SpacewarGame.Settings.AsteroidScale);
                asteroids[i].Paused = true;
                asteroids[i].Velocity = (float)random.Next(100) * Vector3.Normalize(new Vector3((float)(random.NextDouble() - .5), (float)(random.NextDouble() - .5), 0));

                scene.Add(asteroids[i]);
            }

            scene.Add(ship1);
            scene.Add(ship2);
            //Added after other objects so they draw over the top
            scene.Add(particles);


            //Sun last so its on top
            sun = new Sun(game, new EvolvedSun(game), new Vector3(-.5f, -.5f, 0));
            scene.Add(sun);

            //Reset health meters.
            SpacewarGame.Players[0].Health = 5;
            SpacewarGame.Players[1].Health = 5;
        }

        /// <summary>
        /// Update for SpacewarScreen uses the base Update method and adds Sun zoom animation
        /// and detects the end of the level time
        /// </summary>
        /// <param name="time">Game time</param>
        /// <param name="elapsedTime">Elapsed time since last update</param>
        /// <returns>
        /// The next gamestate to transition to. Default is the return value of an overlay or NONE. Override Update if you want to change this behaviour
        /// </returns>
        public override GameState Update(TimeSpan time, TimeSpan elapsedTime)
        {
            if (sunScale > 1)
            {
                //1st frame always has a big elapsed time so use that to set the start pos
                if (sunScale > 20.0)
                {
                    sunScale = 20.0;
                }
                else
                {
                    sunScale -= elapsedTime.TotalSeconds * 15.0;
                }

                if (sunScale < 1)
                {
                    sunScale = 1;
                    paused = false;

                    ship1.Paused = false;
                    ship2.Paused = false;
                    foreach (Asteroid asteroid in asteroids)
                    {
                        asteroid.Paused = false;
                    }

                    //Kick off level timer
                    levelTime = SpacewarGame.Settings.LevelTime;
                }

                //Zoom the Sun
                sun.Scale = new Vector3(SpacewarGame.Settings.Size * (float)sunScale, SpacewarGame.Settings.Size * (float)sunScale, 1f);
            }

            base.Update(time, elapsedTime);

            handleCollisions(time);

            //Update screentimer
            if (!paused)
            {
                levelTime -= (float)elapsedTime.TotalSeconds;

                if ((levelTime - lastLevelTime) < 0.0f)
                {
                    //We have ticked down
                    if (levelTime < 0.0f)
                    {
                        Sound.PlayCue(Sounds.CountDownExpire);
                    }
                    else if (levelTime < 6)
                    {
                        Sound.PlayCue(Sounds.CountDownWarning);
                    }
                }

                lastLevelTime = (int)levelTime;
            }

            //When we run out of time show the totals
            if (!paused && (levelTime <= 0.0f))
            {
                //Stop everything while we count up the $$$
                paused = true;
                ended = true;
                ship1.Paused = true;
                ship2.Paused = true;
                ship1.Silence();
                ship2.Silence();

                foreach (Asteroid asteroid in asteroids)
                {
                    asteroid.Paused = true;
                }

                bullets.Clear(); //No more bullets
                particles.Clear(); //No more particles
                endTime = time.TotalSeconds + 5; //Show scores for 5 seconds
            }

            if (ended && time.TotalSeconds > endTime)
            {
                //Don't need the screen anymore, shut it down!
                Shutdown();

                //Update scores and round
                SpacewarGame.Players[0].Cash += player1Score * 1000;
                SpacewarGame.Players[1].Cash += player2Score * 1000;

                if (player1Score > player2Score)
                {
                    SpacewarGame.Players[0].Score++;
                    SpacewarGame.GameLevel++;
                }
                else if (player2Score > player1Score)
                {
                    SpacewarGame.Players[1].Score++;
                    SpacewarGame.GameLevel++;
                }
                else
                {
                    //nobody won - tie game
                    //replay that level
                }

                if (SpacewarGame.Players[0].Score == 3 || SpacewarGame.Players[1].Score == 3)
                {
                    return GameState.Victory;
                }
                else
                {
                    return GameState.ShipUpgrade;
                }
            }
            else
            {
                return GameState.None;
            }
        }

        /// <summary>
        /// Renders the screen using the base class and adds the HUD
        /// </summary>
        public override void Render()
        {
            //Render scene stuff
            base.Render();

            //If this is the end of level then show the overlays
            if (ended)
            {
                Texture2D overlayTexture = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"textures\In-game_score_overlay");

                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);

                SpriteBatch.Draw(overlayTexture, new Vector2(70, 200), null, Color.White);
                SpriteBatch.Draw(overlayTexture, new Vector2(900, 200), null, Color.White);

                SpriteBatch.End();
            }

            //HUD stuff
            Font.Begin();

            //If this is the end of level then show the scores
            if (ended)
            {
                Font.Draw(FontStyle.WeaponLarge, 100, 240, String.Format("{0} pts x $1,000", player1Score));
                Font.Draw(FontStyle.WeaponLarge, 220, 280, "=");
                Font.Draw(FontStyle.WeaponLarge, 180, 320, String.Format("{0:$##,##0}", player1Score * 1000));

                Font.Draw(FontStyle.WeaponLarge, 930, 240, String.Format("{0} pts x $1,000", player2Score));
                Font.Draw(FontStyle.WeaponLarge, 1050, 280, "=");
                Font.Draw(FontStyle.WeaponLarge, 1010, 320, String.Format("{0:$##,##0}", player2Score * 1000));
            }

            //Timer
            Font.Draw(FontStyle.GameCountDown, 592, 40, String.Format("{0:0}:{1:00}", (int)(levelTime / 60), levelTime % 60));

            //Player1/2 labels
            Font.Draw(FontStyle.GamePlayerNames, 50, 40, "1");
            Font.Draw(FontStyle.GamePlayerNames, 1110, 40, "2");

            //Weapon icons 01234 is player1 weapons, 56789 is player2 weapons
            Font.Draw(FontStyle.WeaponIcons, 50, 560, ((int)SpacewarGame.Players[0].ProjectileType));
            Font.Draw(FontStyle.WeaponIcons, 1090, 560, ((int)(SpacewarGame.Players[1].ProjectileType) + 5));

            //Score buttons
            Font.Draw(FontStyle.ScoreButtons, 60, 70, scoreLookup[SpacewarGame.Players[0].Score]);
            Font.Draw(FontStyle.ScoreButtons, 1140, 70, scoreLookup[SpacewarGame.Players[1].Score]);

            //Health
            //Fudge factor for BFG icon
            int xOffset;
            xOffset = (SpacewarGame.Players[0].ProjectileType == ProjectileType.BFG) ? 12 : 0;
            Font.Draw(FontStyle.HealthBar, 130 + xOffset, 625, SpacewarGame.Players[0].Health);
            xOffset = (SpacewarGame.Players[1].ProjectileType == ProjectileType.BFG) ? 16 : 0;
            Font.Draw(FontStyle.HealthBar, 1170 + xOffset, 625, SpacewarGame.Players[1].Health);

            //Scores
            Font.Draw(FontStyle.Score, 300, 15, player1Score);
            Font.Draw(FontStyle.Score, 940, 15, player2Score);
            Font.End();
        }

        protected override void handleCollisions(TimeSpan gameTime)
        {
            base.handleCollisions(gameTime);

            bool asteroidHitShip1 = false;
            bool asteroidHitShip2 = false;

            //Asteroids do 3 damage and bounce you
            foreach (Asteroid asteroid in asteroids)
            {
                if (!asteroid.Destroyed)
                {
                    if (asteroid.Collide(ship1) && !ship1.Uncollidable)
                    {
                        asteroidHitShip1 = true;
                        if (!ship1.Invulnerable)
                        {
                            float ship1Speed = ship1.Velocity.Length();
                            float asteroidSpeed = asteroid.Velocity.Length();

                            Vector3 tmp = ship1.Velocity;
                            Vector3 vel = asteroid.Velocity;
                            vel.Normalize();
                            ship1.Velocity = vel * ship1Speed;

                            tmp.Normalize();
                            asteroid.Velocity = tmp * asteroidSpeed;

                            HitPlayer1(gameTime, 1);

                            ship1.Invulnerable = true;
                        }
                    }

                    if (asteroid.Collide(ship2) && !ship2.Uncollidable)
                    {
                        asteroidHitShip2 = true;
                        if (!ship2.Invulnerable)
                        {
                            float ship2Speed = ship2.Velocity.Length();
                            float asteroidSpeed = asteroid.Velocity.Length();

                            Vector3 tmp = ship2.Velocity;
                            Vector3 vel = asteroid.Velocity;
                            vel.Normalize();
                            ship2.Velocity = vel * ship2Speed;

                            tmp.Normalize();
                            asteroid.Velocity = tmp * asteroidSpeed;

                            HitPlayer2(gameTime, 1);

                            ship2.Invulnerable = true;
                        }
                    }

                    foreach (Projectile bullet in bullets)
                    {
                        if (bullet.Collide(asteroid))
                        {
                            particles.AddExplosion(asteroid.Position);
                            asteroid.Delete = true;
                            asteroid.Destroyed = true;
                            bullet.DeleteProjectile();
                            Sound.PlayCue(Sounds.Explosion);
                        }
                    }
                }
            }

            if (!asteroidHitShip1 && ship1.Invulnerable)
            {
                ship1.Invulnerable = false;
            }

            if (!asteroidHitShip2 && ship2.Invulnerable)
            {
                ship2.Invulnerable = false;
            }
        }

        public override void OnCreateDevice()
        {
            base.OnCreateDevice();

            foreach (Asteroid asteroid in asteroids)
            {
                asteroid.ShapeItem.OnCreateDevice();
            }

            foreach (Projectile bullet in bullets)
            {
                bullet.ShapeItem.OnCreateDevice();
            }

            particles.OnCreateDevice();
        }
    }
}
