#region File Description
//-----------------------------------------------------------------------------
// Lighting.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace Pickture
{
    /// <summary>
    /// Animated lighting rig used to render a board.
    /// </summary>
    class Lighting
    {
        #region Fields

        const float LightDistance = 300.0f;

        float previousXZ;
        float previousY;

        Vector3 position;

        float targetXZ;
        float targetY;

        float movementTime;
        float movementDuration;

        #endregion

        /// <summary>
        /// Position of camera in world space.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
        }

        /// <summary>
        /// Constructs a new instance of the Lighting class.
        /// </summary>
        public Lighting()
        {
            // Kick off an inital animation
            ChooseTargetPosition();
        }

        /// <summary>
        /// Animates the lighting rig.
        /// </summary>
        /// <param name="camera">The light's position is derived from this camera's
        /// current position.</param>
        public void Update(GameTime gameTime, Camera camera)
        {
            movementTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Has the current animation ended?
            if (movementTime >= movementDuration)
            {
                // Begin the next animation
                ChooseTargetPosition();
            }

            // Interpolate between the position from the end of the last animation to
            // the current animation target position
            float fraction = movementTime / movementDuration;
            float currentXZ = camera.CurrentXZ +
                MathHelper.SmoothStep(previousXZ, targetXZ, fraction);
            float currentY = camera.CurrentY +
                MathHelper.SmoothStep(previousY, targetY, fraction);

            // Calculate light position in world space
            position.X = (float)Math.Sin(currentXZ);
            position.Y = (float)Math.Sin(currentY);
            position.Z = (float)Math.Cos(currentXZ);
            position *= LightDistance;
        }

        /// <summary>
        /// Randomly select a new animation target and duration.
        /// </summary>
        void ChooseTargetPosition()
        {
            previousXZ = targetXZ;
            previousY = targetY;
            targetXZ = RandomHelper.Next(-MathHelper.Pi / 5.0f, MathHelper.Pi / 5.0f);
            targetY = RandomHelper.Next(-MathHelper.Pi / 5.0f, MathHelper.Pi / 5.0f);
            movementTime = 0.0f;
            movementDuration = RandomHelper.Next(4.0f, 8.0f);
        }
    }
}
