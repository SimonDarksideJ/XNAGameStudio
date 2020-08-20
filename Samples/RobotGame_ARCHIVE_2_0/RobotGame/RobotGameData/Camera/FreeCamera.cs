#region File Description
//-----------------------------------------------------------------------------
// FreeCamera.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Text;
#endregion

namespace RobotGameData.Camera
{
    /// <summary>
    /// This camera can rotate and move freely.
    /// </summary>
    public class FreeCamera : CameraBase
    {
        #region Fields

        /// <summary>
        /// Rotation of the camera matrix
        /// </summary>
        protected Matrix rotateMatrix = Matrix.Identity;

        #endregion

        #region Properties

        public Matrix RotateMatrix
        {
            get { return rotateMatrix; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public FreeCamera()
            : base()
        {
            rotateMatrix = Matrix.Identity;
        }

        /// <summary>
        /// Update the camera
        /// </summary>
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
        }

        /// <summary>
        /// Rotate the camera
        /// </summary>
        public Matrix Rotate(Vector3 rotationAmount)
        {   
            // Add rotation amount per second
            rotationAmount *= (float)FrameworkCore.ElapsedDeltaTime.TotalSeconds;

            //  To radians amount
            float rotationAmountX = MathHelper.ToRadians(rotationAmount.X);
            float rotationAmountY = MathHelper.ToRadians(rotationAmount.Y);

            // Create rotation matrix from rotation amount
            rotateMatrix = Matrix.CreateFromAxisAngle(Right, rotationAmountY) *
                                Matrix.CreateFromAxisAngle(Vector3.Up, rotationAmountX);

            // Rotate orientation vectors
            Vector3 dir = Vector3.TransformNormal(this.Direction, rotateMatrix);
            Vector3 up = Vector3.TransformNormal(this.Up, rotateMatrix);

            // Re-normalize orientation vectors
            dir.Normalize();
            up.Normalize();

            // Re-calculate Right
            Vector3 right = Vector3.Cross(dir, up);

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            up = Vector3.Cross(right, dir);

            Vector3 tar = this.Position + dir;

            SetView(this.Position, tar, up);

            return rotateMatrix;
        }     

        /// <summary>
        /// if the velocity is a positive number, it moves forward.  
        /// If negative, it moves backward.
        /// Until velocity is set to 0, the camera will keep moving.
        /// </summary>
        /// <param name="velocity">movement per second</param>
        public void MoveForward(float velocity)
        {
            float vel = velocity * (float)FrameworkCore.ElapsedDeltaTime.TotalSeconds;

            //  Moves to forward or backward 
            Vector3 pos = this.Position + (this.Direction * vel);

            Vector3 tar = pos + this.Direction;

            SetView(pos, tar, this.Up);
        }

        /// <summary>
        /// If the velocity is positive, it moves to the right.  
        /// If negative, to the left.
        /// Until the velocity is set to 0 again, the camera will keep moving.
        /// </summary>
        /// <param name="velocity">move amount per second</param>
        public void MoveSide(float velocity)
        {
            float vel = velocity * (float)FrameworkCore.ElapsedDeltaTime.TotalSeconds;

            //  Moves to left or right
            Vector3 pos = this.Position + (this.Right * vel);

            Vector3 tar = pos + this.Direction;

            SetView(pos, tar, this.Up);
        }

        /// <summary>
        /// If the velocity is positive, it moves to the upward.  
        /// If negative, to the downward.
        /// Until the velocity is set to 0 again, the camera will keep moving.
        /// </summary>
        /// <param name="velocity">move amount per second</param>
        public void MoveUp(float velocity)
        {
            float vel = velocity * (float)FrameworkCore.ElapsedDeltaTime.TotalSeconds;

            //  Moves to left or right
            Vector3 pos = this.Position + (this.Up * vel);

            Vector3 tar = pos + this.Direction;

            SetView(pos, tar, this.Up);
        }
    }
}
