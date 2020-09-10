#region File Description
//-----------------------------------------------------------------------------
// Catapult.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
#endregion

namespace CatapultMiniGame
{
    /// <summary>
    /// States for the Catapult
    /// </summary>
    public enum CatapultState 
    { 
        Rolling, 
        Firing, 
        Crash, 
        ProjectileFlying, 
        ProjectileHit,
        Reset
    }

    /// <summary>
    /// Class to manage the catapult and the projectile
    /// </summary>
    public class Catapult : DrawableGameComponent
    {
        #region Fields

        // Hold what game I belong to
        CatapultGame curGame = null;

        // Current state of the Catapult
        CatapultState currentState;
        public CatapultState CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }

        // Texture used for base of Catapult
        Texture2D baseTexture;
        Texture2D baseTextureBack;

        // Texture for the arm
        Texture2D armTexture;

        // Textures for pumpkins
        Texture2D pumpkinTexture;
        Texture2D pumpkinSmashTexture;

        // Position and speed of catapult base
        Vector2 basePosition = Vector2.Zero;
        float baseSpeed;

        // Position and rotation of catapult arm
        Vector2 armCenter = new Vector2(200, 27);
        Vector2 armOffset = new Vector2(280, 100);
        float armRotation;

        // Position, speed, and rotation of pumpkin
        Vector2 pumpkinPosition = Vector2.Zero;
        public Vector2 PumpkinPosition
        {
            get { return pumpkinPosition; }
            set { pumpkinPosition = value; }
        }

        Vector2 pumpkinVelocity = Vector2.Zero;
        Vector2 pumpkinAcceleration = new Vector2(0, 0.001f);
        Vector2 pumpkinRotationPosition = Vector2.Zero;
        float pumpkinLaunchPosition;
        public float PumpkinLaunchPosition
        {
            get { return pumpkinLaunchPosition; }
            set { pumpkinLaunchPosition = value; }
        }
        float pumpkinRotation;

        // Level of boost power
        int boostPower;
        public int BoostPower
        {
            get { return boostPower; }
            set { boostPower = value; }
        }

        // Are we playing the crash sound
        Cue playingCue;
        Cue crashCue;

        #endregion

        #region Initialization

        public Catapult(Game game) : base(game)
        {
            curGame = (CatapultGame)game;

            ResetCatapult();
        }

        public override void  Initialize()
        {
            baseTexture = 
                curGame.Content.Load<Texture2D>("Textures/body_front");
            baseTextureBack = 
                curGame.Content.Load<Texture2D>("Textures/body_back");
            pumpkinTexture = 
                curGame.Content.Load<Texture2D>("Textures/pumpkin");
            pumpkinSmashTexture = 
                curGame.Content.Load<Texture2D>("Textures/pumpkinsmash");
            armTexture = curGame.Content.Load<Texture2D>("Textures/arm");

            base.Initialize();
        }

        #endregion

        #region Update and Draw


        public override void Update(GameTime gameTime)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            // Do we need to reset
            if (currentState == CatapultState.Reset)
                ResetCatapult();

            // Are we currently rolling?
            if (currentState == CatapultState.Rolling)
            {
                // Add to current speed
                float speedAmt = curGame.CurrentGamePadState.Triggers.Left;
                if (curGame.CurrentKeyboardState.IsKeyDown(Keys.Right))
                    speedAmt = 1.0f;

                baseSpeed += speedAmt * 
                             gameTime.ElapsedGameTime.Milliseconds * 0.001f;

                // Move catapult based on speed
                basePosition.X += baseSpeed * gameTime.ElapsedGameTime.Milliseconds;

                // Move pumpkin to match catapult
                pumpkinPosition.X = pumpkinLaunchPosition = basePosition.X + 120;
                pumpkinPosition.Y = basePosition.Y + 80;

                // Play moving sound
                if (playingCue == null && baseSpeed > 0)
                {
                    playingCue = curGame.SoundBank.GetCue("Move");
                    playingCue.Play();
                }

                // Check to see if we fire the pumpkin
                if ((curGame.CurrentGamePadState.Buttons.A == ButtonState.Pressed &&
                    curGame.LastGamePadState.Buttons.A != ButtonState.Pressed) ||
                    (curGame.CurrentKeyboardState.IsKeyDown(Keys.Space) &&
                    curGame.LastKeyboardState.IsKeyUp(Keys.Space)))
                {
                    Fire();
                    if (playingCue != null && playingCue.IsPlaying)
                    {
                        playingCue.Stop(AudioStopOptions.Immediate);
                        playingCue.Dispose();
                        playingCue = curGame.SoundBank.GetCue("Flying");
                        playingCue.Play();
                    }
                }
            }
            // Are we in the firing state
            else if (currentState == CatapultState.Firing)
            {
                // Rotate the arm
                if (armRotation < MathHelper.ToRadians(81))
                {
                    armRotation += 
                        MathHelper.ToRadians(gameTime.ElapsedGameTime.Milliseconds);

                    Matrix matTranslate, matTranslateBack, matRotate, matFinal;
                    matTranslate = Matrix.CreateTranslation((-pumpkinRotationPosition.X)
                                   - 170, -pumpkinRotationPosition.Y, 0);
                    matTranslateBack = 
                        Matrix.CreateTranslation(pumpkinRotationPosition.X + 170, 
                                                 pumpkinRotationPosition.Y, 0);
                    matRotate = Matrix.CreateRotationZ(armRotation);
                    matFinal = matTranslate * matRotate * matTranslateBack;

                    Vector2.Transform(ref pumpkinRotationPosition, ref matFinal, 
                                      out pumpkinPosition);
                    pumpkinLaunchPosition = pumpkinPosition.X;

                    pumpkinRotation += MathHelper.ToRadians(
                                        gameTime.ElapsedGameTime.Milliseconds / 10.0f);
                }
                // We are done rotating so send the pumpkin flying
                else
                {
                    currentState = CatapultState.ProjectileFlying;

                    pumpkinVelocity.X = baseSpeed * 2.0f + 1;
                    pumpkinVelocity.Y = -baseSpeed * 0.75f;

                    // Add extra velocity for Right trigger 
                    float rightTriggerAmt = curGame.CurrentGamePadState.Triggers.Right;

                    if (rightTriggerAmt > 0.5f)
                        rightTriggerAmt = 1.0f - rightTriggerAmt;

                    if (curGame.CurrentKeyboardState.IsKeyDown(Keys.B))
                        rightTriggerAmt = 0.5f;

                    rightTriggerAmt *= 2;

                    pumpkinVelocity *= 1.0f + rightTriggerAmt;

                    // Check for extra boost power
                    if (basePosition.X > 620)
                    {
                        boostPower = 3;
                        pumpkinVelocity *= 2.0f;
                        curGame.SoundBank.PlayCue("Boost");
                    }
                    else if (basePosition.X > 600)
                    {
                        boostPower = 2;
                        pumpkinVelocity *= 1.6f;
                        curGame.SoundBank.PlayCue("Boost");
                    }
                    else if (basePosition.X > 580)
                    {
                        boostPower = 1;
                        pumpkinVelocity *= 1.3f;
                        curGame.SoundBank.PlayCue("Boost");
                    }
                }
            }
            // Pumpkin is in the flying state
            else if (currentState == CatapultState.ProjectileFlying)
            {
                // Update the position of the pumpkin
                pumpkinPosition += pumpkinVelocity * 
                                   gameTime.ElapsedGameTime.Milliseconds;
                pumpkinVelocity += pumpkinAcceleration * 
                                   gameTime.ElapsedGameTime.Milliseconds;

                // Move the catapult away from the pumpkin
                basePosition.X -= pumpkinVelocity.X * 
                                  gameTime.ElapsedGameTime.Milliseconds;

                // Rotate the pumpkin as it flys
                pumpkinRotation += MathHelper.ToRadians(pumpkinVelocity.X * 3.5f);

                // Is the pumpkin hitting the ground
                if (pumpkinPosition.Y > 630)
                {
                    // Stop playing any sounds
                    if (playingCue != null && playingCue.IsPlaying)
                    {
                        playingCue.Stop(AudioStopOptions.Immediate);
                        playingCue.Dispose();
                        playingCue = null;
                    }

                    // Play the bounce sound
                    curGame.SoundBank.PlayCue("Bounce");

                    // Move the pumpkin out of the ground and Change the pumkin velocity
                    pumpkinPosition.Y = 630;
                    pumpkinVelocity.Y *= -0.8f;
                    pumpkinVelocity.X *= 0.7f;

                    // Stop the pumpkin if the speed is too low
                    if (pumpkinVelocity.X < 0.1f)
                    {
                        currentState = CatapultState.ProjectileHit;
                        curGame.SoundBank.PlayCue("Hit");

                        if (curGame.HighScore == (int)curGame.PumpkinDistance && 
                            curGame.HighScore > 1000)
                                curGame.SoundBank.PlayCue("HighScore");
                    }
                }
            }
            // Did we crash into the log
            if (basePosition.X > 650)
            {
                currentState = CatapultState.Crash;

                if (playingCue != null && playingCue.IsPlaying)
                {
                    playingCue.Stop(AudioStopOptions.Immediate);
                    playingCue.Dispose();
                    playingCue = null;

                    if (crashCue != null)
                    {
                        crashCue.Stop(AudioStopOptions.Immediate);
                        crashCue.Dispose();
                        crashCue = null;
                    }

                    crashCue = curGame.SoundBank.GetCue("Crash");
                    crashCue.Play();
                }
            }

            // If the projectile hit or we crashed reset the catapult
            if ((currentState == CatapultState.Crash || 
                      currentState == CatapultState.ProjectileHit) && 
                      (curGame.CurrentGamePadState.Buttons.A == ButtonState.Pressed || 
                      curGame.CurrentKeyboardState.IsKeyDown(Keys.Space)) &&
                      curGame.CurrentGamePadState.Triggers.Left == 0 &&
                      curGame.CurrentKeyboardState.IsKeyUp(Keys.Right))
            {
                currentState = CatapultState.Reset;
            }

            base.Update(gameTime);
        }

        // Draw the catapult and pumpkin
        public override void Draw(GameTime gameTime)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            if (currentState == CatapultState.Crash)
            {
                curGame.SpriteBatch.Draw(baseTextureBack, basePosition, null, 
                    Color.White, MathHelper.ToRadians(-5), 
                    Vector2.Zero, 1.0f, SpriteEffects.None, 0);
                curGame.SpriteBatch.Draw(armTexture, basePosition + armOffset, null, 
                   Color.White, armRotation, armCenter, 1.0f, SpriteEffects.None, 0.0f);
                curGame.SpriteBatch.Draw(baseTexture, basePosition, null, Color.White,
                    MathHelper.ToRadians(5), Vector2.Zero, 1.0f, SpriteEffects.None, 0);
            }
            else
            {
                curGame.SpriteBatch.Draw(baseTextureBack, basePosition, Color.White);
                curGame.SpriteBatch.Draw(armTexture, basePosition + armOffset, null, 
                   Color.White, armRotation, armCenter, 1.0f, SpriteEffects.None, 0.0f);
                curGame.SpriteBatch.Draw(baseTexture, basePosition, Color.White);
            }

            if (currentState != CatapultState.ProjectileHit && 
                currentState != CatapultState.Crash)
                curGame.SpriteBatch.Draw(pumpkinTexture, 
                                new Vector2(pumpkinLaunchPosition, pumpkinPosition.Y), 
                                null, Color.White, pumpkinRotation,
                                new Vector2(32, 32), 1.0f, SpriteEffects.None, 0.0f);
            else
                curGame.SpriteBatch.Draw(pumpkinSmashTexture, 
                                new Vector2(pumpkinLaunchPosition, pumpkinPosition.Y), 
                                null, Color.White, 0,
                                new Vector2(50, 32), 1.0f, SpriteEffects.None, 0.0f);

            base.Draw(gameTime);
        }

        #endregion

        #region Actions

        // Reset the catapult and pumpkin to default positions
        private void ResetCatapult()
        {
            basePosition.X = -100;
            basePosition.Y = 430;
            baseSpeed = 0;

            pumpkinPosition = Vector2.Zero;
            armRotation = MathHelper.ToRadians(0);

            currentState = CatapultState.Rolling;

            pumpkinPosition = Vector2.Zero;
            pumpkinVelocity = Vector2.Zero;
            pumpkinPosition.X = pumpkinLaunchPosition = basePosition.X + 120;
            pumpkinPosition.Y = basePosition.Y + 80;
            pumpkinRotation = 0;

            boostPower = 0;
        }

        // Change state to firing and play fire sound
        private void Fire()
        {
            currentState = CatapultState.Firing;
            pumpkinRotationPosition = pumpkinPosition;
            curGame.SoundBank.PlayCue("ThrowSound");
        }

        #endregion
    }
}