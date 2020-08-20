#region File Description
//-----------------------------------------------------------------------------
// Human.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region File Information
//-----------------------------------------------------------------------------
// Human.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace CatapultGame
{
    enum PlayerSide
    {
        Left,
        Right
    }

    class Human : Player
    {
        #region Fields/Constants
        //Drag variables to hold first and last gesture samples
        GestureSample? prevSample;
        GestureSample? firstSample;
        Vector2 deltaSum = Vector2.Zero;
        public bool isDragging { get; set; }
        // Constant for longest distance possible between drag points
        readonly float maxDragDelta = (new Vector2(480, 800)).Length();
        // Textures & position & spriteEffects used for Catapult
        Texture2D arrow;
        Texture2D guideDot;

        float arrowScale;

        Vector2 catapultPosition;
        PlayerSide playerSide;
        SpriteEffects spriteEffect = SpriteEffects.None;

        // A projectile which we will use to draw guide lines
        Projectile guideProjectile;
        #endregion

        #region Initialization
        public Human(Game game)
            : base(game)
        {
        }

        public Human(Game game, SpriteBatch screenSpriteBatch, PlayerSide playerSide)
            : base(game, screenSpriteBatch)
        {
            string idleTextureName = "";
            this.playerSide = playerSide;

            if (playerSide == PlayerSide.Left)
            {
                catapultPosition = new Vector2(140, 332);
                idleTextureName = "Textures/Catapults/Blue/blueIdle/blueIdle";
            }
            else
            {
                catapultPosition = new Vector2(600, 332);
                spriteEffect = SpriteEffects.FlipHorizontally;
                idleTextureName = "Textures/Catapults/Red/redIdle/redIdle";
            }

            Catapult = new Catapult(game, screenSpriteBatch,
                                    idleTextureName, catapultPosition, spriteEffect, 
                                    playerSide == PlayerSide.Left ? false : true, true);
        }

        public override void Initialize()
        {
            arrow = curGame.Content.Load<Texture2D>("Textures/HUD/arrow");
            guideDot = curGame.Content.Load<Texture2D>("Textures/HUD/guideDot");

            Catapult.Initialize();

            guideProjectile =
                new Projectile(curGame, spriteBatch, null,
                "Textures/Ammo/rock_ammo", Catapult.ProjectileStartPosition,
                Catapult.GroundHitOffset, playerSide == PlayerSide.Right, 
                Catapult.Gravity);    

            base.Initialize();
        }
        #endregion

        #region Handle Input
        /// <summary>
        /// Function processes the user input
        /// </summary>
        /// <param name="gestureSample"></param>
        public void HandleInput(GestureSample gestureSample)
        {            
            // Process input only if in Human's turn
            if (IsActive)
            {
                // Process any Drag gesture
                if (gestureSample.GestureType == GestureType.FreeDrag)
                {
                    // If drag just began save the sample for future 
                    // calculations and start Aim "animation"
                    if (null == firstSample)
                    {
                        firstSample = gestureSample;
                        Catapult.CurrentState = CatapultState.Aiming;
                    }

                    // save the current gesture sample 
                    prevSample = gestureSample;

                    // calculate the delta between first sample and current
                    // sample to present visual sound on screen
                    Vector2 delta = prevSample.Value.Position -
                        firstSample.Value.Position;
                    Catapult.ShotStrength = delta.Length() / maxDragDelta;

                    Catapult.ShotVelocity = MinShotVelocity +
                        Catapult.ShotStrength * (MaxShotVelocity - MinShotVelocity);

                    if (delta.Length() > 0)
                        Catapult.ShotAngle =
                                MathHelper.Clamp((float)Math.Asin(-delta.Y / delta.Length()),
                                MinShotAngle, MaxShotAngle);
                    else
                        Catapult.ShotAngle = MinShotAngle;

                    float baseScale = 0.001f;
                    arrowScale = baseScale * delta.Length();

                    isDragging = true;
                }
                else if (gestureSample.GestureType == GestureType.DragComplete)
                {
                    // calc velocity based on delta between first and last
                    // gesture samples
                    if (null != firstSample)
                    {
                        Vector2 delta = prevSample.Value.Position - firstSample.Value.Position;
                        Catapult.ShotVelocity = MinShotVelocity +
                                                Catapult.ShotStrength * (MaxShotVelocity - MinShotVelocity);
                        if (delta.Length() > 0)
                            Catapult.ShotAngle =
                                    MathHelper.Clamp((float)Math.Asin(-delta.Y / delta.Length()),
                                    MinShotAngle, MaxShotAngle);
                        else
                            Catapult.ShotAngle = MinShotAngle;
                        Catapult.CurrentState = CatapultState.Firing;
                    }

                    // turn off dragging state
                    ResetDragState();
                }
            }
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            if (isDragging)
            {
                DrawGuide();
                DrawDragArrow(arrowScale);
            }

            base.Draw(gameTime);
        }

        public void DrawDragArrow(float arrowScale)
        {
            spriteBatch.Draw(arrow, catapultPosition + new Vector2(0, -40),
                null, playerSide == PlayerSide.Left ? Color.Blue : Color.Red , 
                playerSide == PlayerSide.Left ? -Catapult.ShotAngle : 
                Catapult.ShotAngle,
                playerSide == PlayerSide.Left ? new Vector2(34, arrow.Height / 2) :
                new Vector2(725, arrow.Height / 2),
                new Vector2(arrowScale, 0.1f),
                playerSide == PlayerSide.Left ? SpriteEffects.None : SpriteEffects.FlipHorizontally
                , 0);
        }

        /// <summary>
        /// Draws a guide line which shows the course of the shot
        /// </summary>
        public void DrawGuide()
        {
            guideProjectile.ProjectilePosition = Catapult.ProjectileStartPosition;

            Single direction = playerSide == PlayerSide.Left ? 1 : -1;

            guideProjectile.Fire(Catapult.ShotVelocity * (float)Math.Cos(Catapult.ShotAngle),
                                 Catapult.ShotVelocity * (float)Math.Sin(Catapult.ShotAngle));           

            while (guideProjectile.State == ProjectileState.InFlight)
            {
                guideProjectile.UpdateProjectileFlightData(0.1f, 
                    Catapult.Wind,
                    Catapult.Gravity);

                spriteBatch.Draw(guideDot, guideProjectile.ProjectilePosition, null,
                    playerSide == PlayerSide.Left ? Color.Blue : Color.Red, 0f, 
                    Vector2.Zero, 1f, spriteEffect, 0);
            }
        }
        #endregion

        /// <summary>
        /// Turn off dragging state and reset drag related variables
        /// </summary>
        public void ResetDragState()
        {
            firstSample = null;
            prevSample = null;
            isDragging = false;
            arrowScale = 0;
            Catapult.ShotStrength = 0;
            deltaSum = Vector2.Zero;
        }
    }
}
