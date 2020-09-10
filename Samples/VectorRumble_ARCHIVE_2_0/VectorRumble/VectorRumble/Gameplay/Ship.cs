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
#endregion

namespace VectorRumble
{
    /// <summary>
    /// The ship, which is the primary playing-piece in the game.
    /// </summary>
    class Ship : Actor
    {
        #region Constants
        /// <summary>
        /// The value of the spawn timer set when the ship dies.
        /// </summary>
        const float respawnTimerOnDeath = 5f;

        /// <summary>
        /// How long, in seconds, for the ship to fade in.
        /// </summary>
        const float fadeInTimerMaximum = 0.5f;

        /// <summary>
        /// The maximum value of the "safe" timer.
        /// </summary>
        const float safeTimerMaximum = 4f;

        /// <summary>
        /// The amount of drag applied to velocity per second, 
        /// as a percentage of velocity.
        /// </summary>
        const float dragPerSecond = 0.9f;

        /// <summary>
        /// The amount that the right-stick must be pressed to fire, squared so that
        /// we can use LengthSquared instead of Length, which has a square-root in it.
        /// </summary>
        const float fireThresholdSquared = 0.25f;

        /// <summary>
        /// The number of radians that the ship can turn in a second at full left-stick.
        /// </summary>
        const float rotationRadiansPerSecond = 6f;

        /// <summary>
        /// The maximum length of the velocity vector on a ship.
        /// </summary>
        const float velocityLengthMaximum = 320f;

        /// <summary>
        /// The maximum strength of the shield.
        /// </summary>
        const float shieldMaximum = 100f;

        /// <summary>
        /// How much the shield recharges per second.
        /// </summary>
        const float shieldRechargePerSecond = 50f;

        /// <summary>
        /// The duration of the shield-recharge timer when the ship is hit.
        /// </summary>
        const float shieldRechargeTimerOnDamage = 2.5f;

        /// <summary>
        /// The amount at which to vibrate the large motor if the timer is active.
        /// </summary>
        const float largeMotorSpeed = 0.5f;

        /// <summary>
        /// The amount at which to vibrate the small motor if the timer is active.
        /// </summary>
        const float smallMotorSpeed = 0.5f;

        /// <summary>
        /// The amount of time that the A button must be held to join the game.
        /// </summary>
        const float aButtonHeldToPlay = 2f;

        /// <summary>
        /// The amount of time that the B button must be held to leave the game.
        /// </summary>
        const float bButtonHeldToLeave = 2f;

        /// <summary>
        /// The number of radians that the shield rotates per second.
        /// </summary>
        const float shieldRotationPeriodPerSecond = 2f;

        /// <summary>
        /// The relationship between the shield rotation and it's scale.
        /// </summary>
        const float shieldRotationToScaleScalar = 0.025f;

        /// <summary>
        /// The relationship between the shield rotation and it's scale period.
        /// </summary>
        const float shieldRotationToScalePeriodScalar = 4f;

        /// <summary>
        /// The colors used for each ship, given it's player-index.
        /// </summary>
        static readonly Color[] shipColorsByPlayerIndex = 
            {
                Color.Lime, Color.CornflowerBlue, Color.Fuchsia, Color.Red
            };

        /// <summary>
        /// Particle system colors for the ship-explosion effect.
        /// </summary>
        static readonly Color[] explosionColors = 
            { 
                Color.Red, Color.Red, Color.Silver, Color.Gray, Color.Orange, 
                Color.Yellow 
            };
        #endregion

        #region Fields
        /// <summary>
        /// If true, this ship is active in-game.
        /// </summary>
        private bool playing = false;

        /// <summary>
        /// The current score for this ship.
        /// </summary>
        private int score = 0;

        /// <summary>
        /// The speed at which the ship moves.
        /// </summary>
        private float speed = 480f;

        /// <summary>
        /// The strength of the shield.
        /// </summary>
        private float shield = 0f;

        /// <summary>
        /// The rotation of the shield effect.
        /// </summary>
        private float shieldRotation = 0f;

        /// <summary>
        /// The polygon used to render the shield effect
        /// </summary>
        private VectorPolygon shieldPolygon = null;

        /// <summary>
        /// The ship's current weapon.
        /// </summary>
        private Weapon weapon = null;

        /// <summary>
        /// The ship's additional mine-laying weapon.
        /// </summary>
        private MineWeapon mineWeapon = null;
        
        /// <summary>
        /// The Gamepad player index that is controlling this ship.
        /// </summary>
        private PlayerIndex playerIndex;

        /// <summary>
        /// The current state of the Gamepad that is controlling this ship.
        /// </summary>
        private GamePadState currentGamePadState;

        /// <summary>
        /// The previous state of the Gamepad that is controlling this ship.
        /// </summary>
        private GamePadState lastGamePadState;

        /// <summary>
        /// Timer for how long the player has been holding the A button (to join).
        /// </summary>
        private float aButtonTimer = 0f;

        /// <summary>
        /// Timer for how long the player has been holding the B button (to leave).
        /// </summary>
        private float bButtonTimer = 0f;

        /// <summary>
        /// Timer for how much longer to vibrate the small motor.
        /// </summary>
        private float smallMotorTimer = 0f;

        /// <summary>
        /// Timer for how much longer to vibrate the large motor.
        /// </summary>
        private float largeMotorTimer = 0f;

        /// <summary>
        /// Timer for how much longer the player has to wait for the ship to respawn.
        /// </summary>
        private float respawnTimer = 0f;

        /// <summary>
        /// Timer for how long until the shield starts to recharge.
        /// </summary>
        private float shieldRechargeTimer = 0f;

        /// <summary>
        /// Timer for how long the player is safe after spawning.
        /// </summary>
        private float safeTimer = 0f;

        /// <summary>
        /// Timer for how long the player has been spawned for, to fade in
        /// </summary>
        private float fadeInTimer = 0f;
        #endregion

        #region Properties
        public bool Playing
        {
            get { return playing; }
        }

        public bool Safe
        {
            get { return (safeTimer > 0f); }
            set
            {
                if (value)
                {
                    safeTimer = safeTimerMaximum;
                }
                else
                {
                    safeTimer = 0f;
                }
            }
        }

        public int Score
        {
            get { return score; }
            set { score = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Construct a new ship, for the given player.
        /// </summary>
        /// <param name="world">The world that this ship belongs to.</param>
        /// <param name="playerIndex">
        /// The Gamepad player index that controls this ship.
        /// </param>
        public Ship(World world, PlayerIndex playerIndex)
            : base(world)
        {
            this.playerIndex = playerIndex;

            this.radius = 20f;
            this.mass = 32f;
            this.color = shipColorsByPlayerIndex[(int)this.playerIndex];
            this.polygon = VectorPolygon.CreatePlayer();
            this.shieldPolygon = VectorPolygon.CreateCircle(Vector2.Zero, 20f, 16);
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the ship.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public override void Update(float elapsedTime)
        {
            // process all input
            ProcessInput(elapsedTime, false);

            // if this player isn't in the game, then quit now
            if (playing == false)
            {
                return;
            }

            if (dead == true)
            {
            // if we've died, then we're counting down to respawning
                if (respawnTimer > 0f)
                {
                    respawnTimer = Math.Max(respawnTimer - elapsedTime, 0f);
                }
                if (respawnTimer <= 0f)
                {
                    Spawn(true);
                }
            }
            else
            {
                // apply drag to the velocity
                velocity -= velocity * (elapsedTime * dragPerSecond);
                if (velocity.LengthSquared() <= 0f)
                {
                    velocity = Vector2.Zero;
                }
                // decrement the heal timer if necessary
                if (shieldRechargeTimer > 0f)
                {
                    shieldRechargeTimer = Math.Max(shieldRechargeTimer - elapsedTime, 
                        0f);
                }
                // recharge the shields if the timer has come up
                if (shieldRechargeTimer <= 0f)
                {
                    if (shield < 100f)
                    {
                        shield = Math.Min(100f,
                            shield + shieldRechargePerSecond * elapsedTime);
                    }
                }
            }

            // update the weapons
            if (weapon != null)
            {
                weapon.Update(elapsedTime);
            }
            if (mineWeapon != null)
            {
                mineWeapon.Update(elapsedTime);
            }

            // decrement the safe timer
            if (safeTimer > 0f)
            {
                safeTimer = Math.Max(safeTimer - elapsedTime, 0f);
            }

            // update the radius based on the shield
            radius = (shield > 0f) ? 20f : 14f;

            // update the spawn-in timer
            if (fadeInTimer < fadeInTimerMaximum)
            {
                fadeInTimer = Math.Min(fadeInTimer + elapsedTime,
                    fadeInTimerMaximum);
            }

            // update and apply the vibration
            smallMotorTimer -= elapsedTime;
            largeMotorTimer -= elapsedTime;
            GamePad.SetVibration(playerIndex,
                (largeMotorTimer > 0f) ? largeMotorSpeed : 0f,
                (smallMotorTimer > 0f) ? smallMotorSpeed : 0f);
            
            base.Update(elapsedTime);
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Render the ship.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="lineBatch">The LineBatch to render to.</param>
        public override void Draw(float elapsedTime, LineBatch lineBatch)
        {
            // if the ship isn't in the game, or it's dead, don't draw
            if ((playing == false) || (dead == true))
            {
                return;
            }
            // update the shield rotation
            shieldRotation += elapsedTime * shieldRotationPeriodPerSecond;
            // calculate the current color
            color = new Color(color.R, color.G, color.B, (byte)(255f * fadeInTimer / 
                fadeInTimerMaximum));
            // transform the shield polygon
            Matrix translationMatrix = Matrix.CreateTranslation(position.X, 
                position.Y, 0f);
            shieldPolygon.Transform(Matrix.CreateScale(1f + shieldRotationToScaleScalar 
                * (float)Math.Cos(shieldRotation * shieldRotationToScalePeriodScalar)) *
                Matrix.CreateRotationZ(shieldRotation) * translationMatrix);
            // draw the shield
            if (Safe)
            {
                lineBatch.DrawPolygon(shieldPolygon, color);
            }
            else if (shield > 0f)
            {
                lineBatch.DrawPolygon(shieldPolygon, new Color(color.R, color.G, 
                    color.B, (byte)(255f * shield / shieldMaximum)), true);
            }
            base.Draw(elapsedTime, lineBatch);
        }
        #endregion

        #region Interaction
        /// <summary>
        /// Set the ship up to join the game, if it's not in it already.
        /// </summary>
        public void JoinGame()
        {
            if (playing == false)
            {
                playing = true;
                score = 0;
                Spawn(true);
            }
        }


        /// <summary>
        /// Remove the ship from the game, if it's in it.
        /// </summary>
        public void LeaveGame()
        {
            if (playing == true)
            {
                playing = false;
                Die(null);
            }
        }


        /// <summary>
        /// Assigns the new weapon to the ship.
        /// </summary>
        /// <param name="weapon">The new weapon.</param>
        public void SetWeapon(Weapon weapon)
        {
            if (weapon != null)
            {
                this.weapon = weapon;
            }
        }


        /// <summary>
        /// Damage this ship by the amount provided.
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
            // if the safe timer hasn't yet gone off, then the ship can't be hurt
            if (safeTimer > 0f)
            {
                return false;
            }

            // tickle the gamepad vibration motors
            FireGamepadMotors(0f, 0.25f);

            // once you're hit, the shield-recharge timer starts over
            shieldRechargeTimer = 2.5f;

            // damage the shield first, then life
            if (shield <= 0f)
            {
                life -= damageAmount;
            }
            else
            {
                shield -= damageAmount;
                if (shield < 0f)
                {
                    // shield has the overflow value as a negative value, just add it
                    life += shield;
                    shield = 0f;
                }
            }

            // if the ship is out of life, it dies
            if (life < 0f)
            {
                Die(source);
            }

            return true;
        }


        /// <summary>
        /// Kills this ship, in response to the given actor.
        /// </summary>
        /// <param name="source">The actor responsible for the kill.</param>
        public override void Die(Actor source)
        {
            if (dead == false)
            {
                // hit the gamepad vibration motors
                FireGamepadMotors(0.75f, 0.25f);
                // play several explosion cues
                world.AudioManager.PlayCue("playerDeath");
                // add several particle systems for effect
                world.ParticleSystems.Add(new ParticleSystem(this.position,
                    Vector2.Zero, 128, 64f, 256f, 3f, 0.05f, explosionColors));
                world.ParticleSystems.Add(new ParticleSystem(this.position,
                    Vector2.Zero, 64, 256f, 1024f, 3f, 0.05f, explosionColors));
                // reset the respawning timer
                respawnTimer = respawnTimerOnDeath;

                // change the score
                Ship ship = source as Ship;
                if (ship == null)
                {
                    Projectile projectile = source as Projectile;
                    if (projectile != null)
                    {
                        ship = projectile.Owner;
                    }
                }
                if (ship != null)
                {
                    if (ship == this)
                    {
                        // reduce the score, since i blew myself up
                        ship.Score--;
                    }
                    else
                    {
                        // add score to the ship who shot this object
                        ship.Score++;
                    }
                }
                else
                {
                    // if it wasn't a ship, then this object loses score
                    this.Score--;
                }

                // ships should not be added to the garbage list, so just set dead
                dead = true;
            }
        }


        /// <summary>
        /// Place this ship in the world.
        /// </summary>
        /// <param name="findSpawnPoint">
        /// If true, the actor's position is changed to a valid, non-colliding point.
        /// </param>
        public override void Spawn(bool findSpawnPoint)
        {
            // do not call the base Spawn, as the actor never is added or removed when
            // dying or respawning, because we always need to be processing input
            // respawn this actor
            if (dead == true)
            {
                // I LIVE
                dead = false;
                // find a new spawn point if requested
                if (findSpawnPoint)
                {
                    position = world.FindSpawnPoint(this);
                }
                // reset the velocity
                velocity = Vector2.Zero;
                // reset the shield and life values
                life = 25f;
                shield = shieldMaximum;
                // reset the safety timers
                safeTimer = safeTimerMaximum;
                // create the default weapons
                weapon = new LaserWeapon(this);
                mineWeapon = new MineWeapon(this);
                // play the ship-spawn cue
                world.AudioManager.PlayCue("playerSpawn");
                // add a particle effect at the ship's new location
                world.ParticleSystems.Add(new ParticleSystem(this.position, 
                    Vector2.Zero, 32, 32f, 64f, 2f, 0.1f, new Color[] { this.color }));
                // remind the player that we're spawning
                FireGamepadMotors(0.25f, 0f);
            }
        }
        #endregion

        #region Input Methods
        /// <summary>
        /// Vibrate the gamepad motors for the given period of time.
        /// </summary>
        /// <param name="largeMotorTime">The time to run the large motor.</param>
        /// <param name="smallMotorTime">The time to run the small motor.</param>
        public void FireGamepadMotors(float largeMotorTime, float smallMotorTime)
        {
            // use the maximum timer value
            this.largeMotorTimer = Math.Max(this.largeMotorTimer, largeMotorTime);
            this.smallMotorTimer = Math.Max(this.smallMotorTimer, smallMotorTime);
        }


        /// <summary>
        /// Process the input for this ship, from the gamepad assigned to it.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <para
        public virtual void ProcessInput(float elapsedTime, bool overlayPresent)
        {
            currentGamePadState = GamePad.GetState(playerIndex);
            if (overlayPresent == false)
            {
                if (playing == false)
                {
                    // trying to join - update the a-button timer
                    if (currentGamePadState.Buttons.A == ButtonState.Pressed)
                    {
                        aButtonTimer += elapsedTime;
                    }
                    else
                    {
                        aButtonTimer = 0f;
                    }

                    // if the timer has exceeded the expected value, join the game
                    if (aButtonTimer > aButtonHeldToPlay)
                    {
                        JoinGame();
                    }
                }
                else
                {
                    // check if we're trying to leave
                    if (currentGamePadState.Buttons.B == ButtonState.Pressed)
                    {
                        bButtonTimer += elapsedTime;
                    }
                    else
                    {
                        bButtonTimer = 0f;
                    }
                    // if the timer has exceeded the expected value, leave the game
                    if (bButtonTimer > bButtonHeldToLeave)
                    {
                        LeaveGame();
                    }
                    else if (dead == false)
                    {
                        //
                        // the ship is alive, so process movement and firing
                        //
                        // calculate the current forward vector
                        Vector2 forward = new Vector2((float)Math.Sin(Rotation),
                            -(float)Math.Cos(Rotation));
                        Vector2 right = new Vector2(-forward.Y, forward.X);
                        // calculate the current left stick value
                        Vector2 leftStick = currentGamePadState.ThumbSticks.Left;
                        leftStick.Y *= -1f;
                        if (leftStick.LengthSquared() > 0f)
                        {
                            Vector2 wantedForward = Vector2.Normalize(leftStick);
                            float angleDiff = (float)Math.Acos(
                                Vector2.Dot(wantedForward, forward));
                            float facing = (Vector2.Dot(wantedForward, right) > 0f) ?
                                1f : -1f;
                            if (angleDiff > 0f)
                            {
                                Rotation += Math.Min(angleDiff, facing * elapsedTime *
                                    rotationRadiansPerSecond);
                            }
                            // add velocity
                            Velocity += leftStick * (elapsedTime * speed);
                            if (Velocity.Length() > velocityLengthMaximum)
                            {
                                Velocity = Vector2.Normalize(Velocity) *
                                    velocityLengthMaximum;
                            }

                        }
                        // check for firing with the right stick
                        Vector2 rightStick = currentGamePadState.ThumbSticks.Right;
                        rightStick.Y *= -1f;
                        if (rightStick.LengthSquared() > fireThresholdSquared)
                        {
                            weapon.Fire(Vector2.Normalize(rightStick));
                        }
                        // check for laying mines
                        if ((currentGamePadState.Buttons.B == ButtonState.Pressed) &&
                            (lastGamePadState.Buttons.B == ButtonState.Released))
                        {
                            // fire behind the ship
                            mineWeapon.Fire(-forward);
                        }
                    }
                }
            }

            // update the gamepad state
            lastGamePadState = currentGamePadState;
            return;
        }
        #endregion
    }
}
