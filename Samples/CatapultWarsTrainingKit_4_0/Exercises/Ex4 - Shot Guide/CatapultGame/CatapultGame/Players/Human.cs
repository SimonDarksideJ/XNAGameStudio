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
    class Human : Player
    {
        #region Fields/Constants
        //Drag variables to hold first and last gesture samples
        GestureSample? prevSample;
        GestureSample? firstSample; 
        public bool isDragging { get; set; }
        // Constant for longest distance possible between drag points
        readonly float maxDragDelta = (new Vector2(480, 800)).Length();
        // Textures & position & spriteEffects used for Catapult
        Texture2D arrow;
        float arrowScale;
        Texture2D guideDot;

        Vector2 catapultPosition = new Vector2(140, 332);

        // A projectile which we will use to draw guide lines
        Projectile guideProjectile;
        #endregion

        #region Initialization
        public Human(Game game)
            : base(game)
        {
        }

        public Human(Game game, SpriteBatch screenSpriteBatch)
            : base(game, screenSpriteBatch)
        {
            Catapult = new Catapult(game, screenSpriteBatch,
                                    "Textures/Catapults/Blue/blueIdle/blueIdle",
                                    catapultPosition, SpriteEffects.None, false);            
        }

        public override void Initialize()
        {
            arrow = curGame.Content.Load<Texture2D>("Textures/HUD/arrow");
            guideDot = curGame.Content.Load<Texture2D>("Textures/HUD/guideDot");

            Catapult.Initialize();

            guideProjectile = new Projectile(Game, spriteBatch, 
                "Textures/Ammo/rock_ammo", Catapult.ProjectileStartPosition, 
                Catapult.GroundHitOffset, false, Catapult.Gravity);

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
                    Catapult.ShotVelocity = MinShotStrength +
                            Catapult.ShotStrength * (MaxShotStrength - MinShotStrength);
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
                        Vector2 delta = prevSample.Value.Position -
                            firstSample.Value.Position;
                        Catapult.Fire(Catapult.ShotVelocity);
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
                DrawGuide();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws a guide line which shows the course of the shot
        /// </summary>
        public void DrawGuide()
        {
            bool guideDone = false;
            guideProjectile.ProjectilePosition = Catapult.ProjectileStartPosition;
            guideProjectile.Fire(Catapult.ShotVelocity, Catapult.ShotVelocity);

            while (guideDone == false)
            {
                guideProjectile.UpdateProjectileFlightData(0.1f, Catapult.Wind, 
                    Catapult.Gravity, out guideDone);

                spriteBatch.Draw(guideDot, guideProjectile.ProjectilePosition, 
                    Color.Blue);
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
        }
    }
}
