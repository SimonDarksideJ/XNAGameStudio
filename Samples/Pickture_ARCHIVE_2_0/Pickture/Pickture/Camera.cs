#region File Description
//-----------------------------------------------------------------------------
// Camera.cs
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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Pickture
{
    /// <summary>
    /// Animated camera used to render a board.
    /// </summary>
    class Camera
    {
        public enum BoardSide
        {
            Front,
            Back,
        };

        public enum FlipDirection
        {
            Left = -1,
            Right = 1
        }

        #region Fields

        bool isFlipping = false;

        BoardSide side = BoardSide.Front;

        Vector3 position;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        float baseXZ;
        float previousXZ;
        float previousY;
        float targetXZ;
        float targetY;
        float currentXZ;
        float currentY;
        float cameraMovementCurrentTime;
        float cameraMovementTotalTime;

        const float MaxDeviation = MathHelper.Pi / 8.0f;

        float cameraDistance;

        #endregion

        #region Properties

        public bool IsFlipping
        {
            get { return isFlipping; }
        }

        public float CurrentXZ
        {
            get { return currentXZ; }
        }

        public float CurrentY
        {
            get { return currentY; }
        }

        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
        }

        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
        }

        public Vector3 Position
        {
            get { return position; }
        }

        public BoardSide Side
        {
            get { return side; }
        }

        #endregion

        /// <summary>
        /// Constructs a new instance of the Camera class.
        /// </summary>
        public Camera(float cameraDistance)
        {
            this.cameraDistance = cameraDistance;

            position = new Vector3(0.0f, 0.0f, cameraDistance);
            viewMatrix =
                Matrix.CreateLookAt(position, Vector3.Zero, Vector3.Up);

            // Kick off an inital animation
            ChooseTargetPosition();
        }
        
        /// <summary>
        /// Animates the camera.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            cameraMovementCurrentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Has the current animation ended?
            if (cameraMovementCurrentTime >= cameraMovementTotalTime)
            {
                ChooseTargetPosition();

                // In case the previous animation was a flip, we are no longer flipping
                isFlipping = false;
            }

            // Interpolate between the position from the end of the last animation to
            // the current animation target position
            float fraction = cameraMovementCurrentTime / cameraMovementTotalTime;
            currentXZ = MathHelper.SmoothStep(previousXZ, targetXZ, fraction);
            currentY = MathHelper.SmoothStep(previousY, targetY, fraction);

            // Calculate the camera position in world space
            position.X = (float)Math.Sin(currentXZ);
            position.Y = (float)Math.Sin(currentY);
            position.Z = (float)Math.Cos(currentXZ);
            position *= cameraDistance;

            // Recalculate the view matrix since the position has changed
            viewMatrix = Matrix.CreateLookAt(position, Vector3.Zero, Vector3.UnitY);

            // Recalculate aspect ratio
            Viewport viewport = Pickture.Instance.GraphicsDevice.Viewport;
            float aspectRatio = (float)viewport.Width / viewport.Height;

            // Recalculate the projection matrix in case the aspect ratio has changed
            projectionMatrix =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                    aspectRatio, 1.0f, 10000.0f);
        }

        /// <summary>
        /// Randomly select a new animation target and duration.
        /// </summary>
        void ChooseTargetPosition()
        {
            previousXZ = targetXZ;
            previousY = targetY;
            targetXZ = baseXZ + RandomHelper.Next(-MaxDeviation, MaxDeviation);
            targetY = RandomHelper.Next(-MaxDeviation, MaxDeviation);
            cameraMovementCurrentTime = 0.0f;
            cameraMovementTotalTime = RandomHelper.Next(10.0f, 20.0f);
        }

        /// <summary>
        /// Start a new animation to swing around to the other side of the puzzle board.
        /// </summary>
        /// <param name="direction">Direction to move the camera.</param>
        public void Flip(FlipDirection direction)
        {
            side = (side == BoardSide.Front) ? BoardSide.Back : BoardSide.Front;
            baseXZ += (float)Math.PI * (int)direction;

            previousXZ = currentXZ;
            targetXZ = baseXZ;
            cameraMovementCurrentTime = 0.0f;
            cameraMovementTotalTime = 0.6f;

            Audio.Play("Flip Board");
            isFlipping = true;
        }
    }
}
