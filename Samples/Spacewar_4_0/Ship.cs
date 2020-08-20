#region File Description
//-----------------------------------------------------------------------------
// Ship.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Spacewar
{
    /// <summary>
    /// SpacewarGame ship
    /// </summary>
    public class Ship : SpacewarSceneItem
    {
        private Projectiles bullets;
        private Particles particles;
        private double thrustFrame;
        private bool showThrust;
        private bool evolved;

        private const float ninetyDegrees = (float)(Math.PI / 2.0);

        private PlayerIndex player;

        private bool playingThrustSound;
        private Cue cue;

        private const float shotOffset = 20.0f;
        private const float shotSpeed = 150.0f;

        private bool inHyperspace;
        private bool inRecovery;
        private double exitHyperspaceTime;
        private double exitRecoveryTime;
        private const double hyperspaceTime = 1.5;
        private bool playedReturn;
        private bool invulnerable;

        private static Random random = new Random();

        private Vector3 direction;

        private SpriteBatch batch;

        private Vector3[] extendedExtent;

        #region Properties
        public bool Invulnerable
        {
            get
            {
                return invulnerable;
            }

            set
            {
                invulnerable = value;
            }
        }

        public bool Uncollidable
        {
            get
            {
                return inHyperspace || inRecovery;
            }
        }

        public Vector3[] ExtendedExtent
        {
            get
            {
                return extendedExtent;
            }
        }
        #endregion

        #region Data for bullet and engine locations
        private static Vector3[,] bulletOffsets = new Vector3[,]
            {
                { 
                    new Vector3(0, 0, -2134f),
                    new Vector3(0,0, -782),
                    new Vector3(0, 0, -1068),
                },
                {
                    new Vector3(0, 0f, -2134f),
                    new Vector3(0, 0, -859),
                    new Vector3(0, 0, -866),
                }
            };

        private static Vector4[,][] engineOffsets = new Vector4[,][]
            {
                {
                    //Player 1 - 3 ships
                    new Vector4[] 
                    {
                        new Vector4(0, 640, 1146, 1),
                        new Vector4(-526, -262, 1146, 1),
                        new Vector4(526, -262, 1146, 1),
                    },
                    new Vector4[] 
                    {
                        new Vector4(-1125, 135, 432, 1),
                        new Vector4(1125, 135, 432, 1),
                        new Vector4(0, 0, 1020, 1),
                    },
                    new Vector4[] 
                    {
                        new Vector4(0, 500, 1140, 1),
                    },

                },
                {
                    //Player 2 - 3 ships
                    new Vector4[] 
                    {
                        new Vector4(-390, 100, 2013, 1),
                        new Vector4(390, 100, 2013, 1),
                        new Vector4(0, -240, 1915, 1),                        
                    },
                    new Vector4[] 
                    {
                        new Vector4(0, 270, 1013, 1),
                        new Vector4(-380, 635, 628, 1),
                        new Vector4(380, 635, 628, 1),                                                
                    },
                    new Vector4[] 
                    {
                        new Vector4(-270, 150, 656, 1),
                        new Vector4(270, 150, 656, 1),
                        new Vector4(0, 0, 740, 1),
                    },
               }
            };

        #endregion

        public Ship(Game game, PlayerIndex player, ShipClass shipNumber, int shipSkin, Vector3 initialPosition, Projectiles bullets, Particles particles)
            : base(game, new EvolvedShape(game, EvolvedShapes.Ship, player, (int)shipNumber, shipSkin, LightingType.InGame), initialPosition)
        {
            this.player = player;
            this.bullets = bullets;
            this.particles = particles;
            this.evolved = true;

            //Evolved needs scaling
            scale = new Vector3(SpacewarGame.Settings.ShipScale, SpacewarGame.Settings.ShipScale, SpacewarGame.Settings.ShipScale);
            rotation = new Vector3(MathHelper.ToRadians(90), 0, 0);
            direction = new Vector3((float)(-Math.Sin(Rotation.Z)), (float)(Math.Cos(Rotation.Z)), 0);

            if (game != null)
            {
                IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));
                batch = new SpriteBatch(graphicsService.GraphicsDevice);
            }

            if (shipNumber == ShipClass.Pencil)
                extendedExtent = new Vector3[2];
        }

        public Ship(Game game, PlayerIndex player, Vector3 initialPosition, Projectiles bullets)
            : base(game, new RetroShip(game), initialPosition)
        {
            this.player = player;
            this.bullets = bullets;
            this.evolved = false;

            //Scale the ship to match the screen
            scale = new Vector3(8, 8, 8);
        }

        public PlayerIndex Player
        {
            get
            {
                return player;
            }
        }

        public override void Update(TimeSpan time, TimeSpan elapsedTime)
        {
            acceleration = Vector3.Zero;

            if (!Paused)
            {
                if (!inHyperspace && !inRecovery)
                {
                    //HyperSpace
                    if (XInputHelper.GamePads[player].LeftTriggerPressed || XInputHelper.GamePads[player].BPressed)
                    {
                        inHyperspace = true;
                        playedReturn = false;
                        exitHyperspaceTime = time.TotalSeconds + hyperspaceTime;
                        Sound.PlayCue(Sounds.HyperspaceActivate);
                    }

                    //Fire
                    if (XInputHelper.GamePads[player].RightTriggerPressed || XInputHelper.GamePads[player].APressed)
                    {
                        direction.X = (float)(-Math.Sin(Rotation.Z));
                        direction.Y = (float)(Math.Cos(Rotation.Z));
                        direction.Z = 0.0f;

                        direction.Normalize();
                        if (evolved)
                        {
                            //Special case for rockets
                            if (SpacewarGame.Players[(int)player].ProjectileType == ProjectileType.Rocket)
                            {
                                //Don;t take ship velocity into account cos it looks odd
                                //rockets accelerte so little chance of rear ending
                                bullets.Add(player,
                                    Vector3.Transform(bulletOffsets[(int)player, (int)SpacewarGame.Players[(int)player].ShipClass], ShapeItem.World),
                                    Vector3.Multiply(direction, shotSpeed),
                                    Rotation.Z,
                                    time, particles);
                            }
                            else
                            {
                                bullets.Add(player,
                                    Vector3.Transform(bulletOffsets[(int)player, (int)SpacewarGame.Players[(int)player].ShipClass], ShapeItem.World),
                                    Velocity + Vector3.Multiply(direction, shotSpeed),
                                    Rotation.Z,
                                    time, particles);
                            }
                        }
                        else
                        {
                            bullets.Add(player,
                                Vector3.Transform(new Vector3(0, 1.1f, 0), ShapeItem.World),
                                Velocity + Vector3.Multiply(direction, shotSpeed),
                                Rotation.Z,
                                time, null);
                        }
                    }
                }

                //Move ship based on controller and gravity
                if (inHyperspace)
                {
                    //Play return sound 1.3s before
                    if (time.TotalSeconds > exitHyperspaceTime - 1.3 && !playedReturn)
                    {
                        playedReturn = true;
                        Sound.PlayCue(Sounds.HyperspaceReturn);
                    }

                    //Wait until the time is up then reappear in a random position
                    if (time.TotalSeconds > exitHyperspaceTime)
                    {
                        inHyperspace = false;

                        // Asteroids are not allowed to collide with us when we first emerge from Hyperspace
                        // When no asteroids are colliding with us, this reverts to false!
                        Invulnerable = true;
                        position = new Vector3((float)(random.NextDouble() * 800.0 - 400.0), (float)(random.NextDouble() * 500.0 - 250.0), 0);
                    }
                }
                else if (inRecovery)
                {
                    //Wait until the time is up then reappear
                    if (time.TotalSeconds > exitRecoveryTime)
                    {
                        inRecovery = false;

                        // Asteroids are not allowed to collide with us when we first emerge from Recovery
                        // When no asteroids are colliding with us, this reverts to false!
                        Invulnerable = true;
                    }
                }
                else
                {
                    //Only move ship if we are out of hyperspace and recovery from destruction
                    rotation.Z -= (float)((double)XInputHelper.GamePads[player].ThumbStickLeftX * elapsedTime.TotalSeconds * 3.0);

                    if (XInputHelper.GamePads[player].ThumbStickLeftY != 0)
                    {
                        if (!playingThrustSound)
                        {
                            GamePad.SetVibration(player, .8f, .2f);

                            if (player == PlayerIndex.One)
                            {

                                cue = Sound.Play(Sounds.ThrustPlayer1);
                            }
                            else
                            {
                                cue = Sound.Play(Sounds.ThrustPlayer2);

                            }

                            playingThrustSound = true;
                        }

                        //animate the thrust
                        showThrust = true;
                        thrustFrame = (thrustFrame + (elapsedTime.TotalSeconds * 12));
                        if (thrustFrame > 12)
                            thrustFrame = 12;

                        float factor = XInputHelper.GamePads[player].ThumbStickLeftY;
                        Vector2 thrustDirection = new Vector2((float)(-SpacewarGame.Settings.ThrustPower * factor * Math.Sin(Rotation.Z)), (float)(SpacewarGame.Settings.ThrustPower * factor * Math.Cos(Rotation.Z)));
                        acceleration += new Vector3(thrustDirection.X, thrustDirection.Y, 0);
                    }
                    else
                    {
                        //Shrink the thrust
                        thrustFrame += (elapsedTime.TotalSeconds * 12);
                        if (thrustFrame > 29)
                        {
                            showThrust = false;
                            thrustFrame = 0;
                        }

                        if (playingThrustSound)
                        {
                            GamePad.SetVibration(player, 0, 0);

                            Sound.Stop(cue);
                            playingThrustSound = false;
                        }
                    }

                    //Friction is a function of current velocity
                    if (evolved)
                        acceleration -= Velocity * SpacewarGame.Settings.FrictionFactor;
                }
            }

            //Calculate new positions, speeds and other base class stuff
            base.Update(time, elapsedTime);

            //Limit the speed
            if (velocity.Length() > SpacewarGame.Settings.MaxSpeed)
            {
                velocity = Vector3.Normalize(Velocity) * SpacewarGame.Settings.MaxSpeed;
            }
        }

        /// <summary>
        /// Moves the ship back to its starting position
        /// </summary>
        /// <param name="newPosition">Position to move the ship back to</param>
        public void ResetShip(TimeSpan gameTime, Vector3 newPosition)
        {
            position = newPosition;
            rotation.Z = 0f;
            velocity = Vector3.Zero;

            inRecovery = true;
            Invulnerable = true;
            inHyperspace = false;
            exitRecoveryTime = gameTime.TotalSeconds + SpacewarGame.Settings.ShipRecoveryTime;
        }

        /// <summary>
        /// Silence will shut down any looping sounds that are playing
        /// </summary>
        public void Silence()
        {
            if (playingThrustSound)
            {
                Sound.Stop(cue);
                playingThrustSound = false;
                GamePad.SetVibration(player, 0, 0);
            }
        }

        public override void OnCreateDevice()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));
            batch = new SpriteBatch(graphicsService.GraphicsDevice);
        }

        public override void Render()
        {
            //If we are in hyperspace render nothing
            if (!inHyperspace && !inRecovery)
            {
                base.Render();

                //Render engine thrust
                IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));
                GraphicsDevice device = graphicsService.GraphicsDevice;

                if (showThrust && evolved)
                {
                    Texture2D engine = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"textures\thrust_stripSmall");
                    batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                    foreach (Vector4 engineOffset in engineOffsets[(int)player, (int)SpacewarGame.Players[(int)player].ShipClass])
                    {
                        //Move into screen space
                        Vector4 source = Vector4.Transform(engineOffset, ShapeItem.World * SpacewarGame.Camera.View * SpacewarGame.Camera.Projection);
                        //and into pixels
                        Vector2 source2D = new Vector2((int)((source.X / source.W + 1f) / 2f * 1280), (int)((-source.Y / source.W + 1f) / 2f * 720));

                        batch.Draw(engine, source2D, new Rectangle(((int)thrustFrame) * 64, 0, 64, 16), Color.White, -Rotation.Z + ninetyDegrees, new Vector2(2, 8), 1f, SpriteEffects.None, 0.1f);
                    }

                    batch.End();
                }
            }
        }
    }
}
