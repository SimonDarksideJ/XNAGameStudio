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
        // Drag variables to hold first and last gesture samples
        GestureSample? prevSample;
        GestureSample? firstSample;
        public bool isDragging { get; set; }
        // Constant for longest distance possible between drag points
        readonly float maxDragDelta = (new Vector2(480, 800)).Length();
        // Textures & position & spriteEffects used for Catapult
        Texture2D arrow;
        float arrowScale;

        Vector2 catapultPosition = new Vector2(140, 332);
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

            Catapult.Initialize();

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
                    Vector2 delta = prevSample.Value.Position - firstSample.Value.Position;
                    Catapult.ShotStrength = delta.Length() / maxDragDelta;
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
                        Catapult.ShotVelocity = MinShotStrength + Catapult.ShotStrength *
                            (MaxShotStrength - MinShotStrength);
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
                DrawDragArrow(arrowScale);

            base.Draw(gameTime);
        }

        public void DrawDragArrow(float arrowScale)
        {
            spriteBatch.Draw(arrow, catapultPosition + new Vector2(0, -40),
              null, Color.Blue, 0,
              Vector2.Zero, new Vector2(arrowScale, 0.1f), SpriteEffects.None, 0);
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
