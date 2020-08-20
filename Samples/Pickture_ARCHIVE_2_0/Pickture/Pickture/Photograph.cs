#region File Description
//-----------------------------------------------------------------------------
// Photograph.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace Pickture
{
    /// <summary>
    /// An animated falling photo used in the CompletedScreen.
    /// </summary>
    class Photograph
    {
        #region Fields

        // Texture used as the picture on this photograph
        Texture2D texture;

        // Transform is derived from the other fields each time Update is called
        Vector3 position;
        Matrix worldTransform;

        // Given the other control parameters, age can be used to calculate all
        // transforms at a given time in the animation
        float age;

        // Initial position and velocity
        float initialX;
        float initialY;
        const float InitialZ = 1000.0f;
        float fallSpeed;

        // Describes the pendulum used for the swaying away
        float maxHorizontalPendulumAngle;
        float horizontalPendulumLength;
        float maxVerticalPendulumAngle;
        float verticalPendulumLength;

        #endregion

        #region Properties

        public Matrix WorldTransform
        {
            get { return worldTransform; }
        }

        public Vector3 Position
        {
            get { return position; }
        }

        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        #endregion

        #region Reset and Update

        /// <summary>
        /// Selects a new random starting position, velocity, and pendulum. Resets age
        /// to 0 to start the a new animation from the top of the fall again.
        /// </summary>
        public void Reset()
        {
            float maxDeviation = CompletedScreen.ResetZValue *
                (float)Math.Sin(MathHelper.ToRadians(22.5f));
            initialX = RandomHelper.Next(-maxDeviation, maxDeviation);
            initialY = RandomHelper.Next(-maxDeviation, maxDeviation);

            age = 0.0f;

            position = new Vector3(initialX, initialY, InitialZ);
            fallSpeed = RandomHelper.Next(100.0f, 400.0f);

            maxHorizontalPendulumAngle = (float)RandomHelper.Random.NextDouble() *
                MathHelper.PiOver4 / 2.0f;
            horizontalPendulumLength = RandomHelper.Next(2.0f, 4.0f);
            maxVerticalPendulumAngle = (float)RandomHelper.Random.NextDouble() *
                MathHelper.PiOver4 / 2.0f;
            verticalPendulumLength = RandomHelper.Next(2.0f, 4.0f);
        }

        public void Update(GameTime gameTime)
        {
            age += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Determine distance into the fall. +Z is down, away from the camera.
            float baseZPosition = InitialZ - (age * fallSpeed);

            // The pendulum angles follow a sin curve to get some nice variations
            // from swing to swing
            float horizontalPendulumAngle = maxHorizontalPendulumAngle *
                (float)Math.Sin(Math.Sqrt(9.81f / horizontalPendulumLength) * age);
            float verticalPendulumAngle = maxVerticalPendulumAngle *
                (float)Math.Sin(Math.Sqrt(9.81f / verticalPendulumLength) * age);

            // Calculate a more accurate Z value which accounts for the pendulum swings
            float positionModificationMultiplier = 75.0f;
            float zPositionModification =
                -((float)Math.Cos(horizontalPendulumAngle) * horizontalPendulumLength) -
                ((float)Math.Cos(verticalPendulumAngle) * verticalPendulumLength);
            zPositionModification *= positionModificationMultiplier;

            // Determine the X and Y values from the pendulum swing
            position = new Vector3(
                initialX + ((float)Math.Sin(verticalPendulumAngle) *
                    verticalPendulumLength * positionModificationMultiplier),
                initialY + ((float)Math.Sin(horizontalPendulumAngle) *
                    horizontalPendulumLength * positionModificationMultiplier),
                baseZPosition + zPositionModification);

            // Rebuild the world transform
            worldTransform =
                Matrix.CreateRotationX(horizontalPendulumAngle) *
                Matrix.CreateRotationY(verticalPendulumAngle) *
                Matrix.CreateTranslation(Position);
        }

        #endregion
    }
}
