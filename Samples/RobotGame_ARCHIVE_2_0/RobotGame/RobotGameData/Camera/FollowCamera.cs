#region File Description
//-----------------------------------------------------------------------------
// FollowCamera.cs
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
    /// This camera moves by following the target matrix at a constant distance.
    /// The camera always positions itself from the target matrix 
    /// at a specific offset distance.
    /// </summary>
    public class FollowCamera : GameCamera
    {
        #region Fields

        /// <summary>
        /// It's a camera's target matrix
        /// </summary>
        protected Matrix targetMatrix = Matrix.Identity;

        /// <summary>
        /// It's a offset position of the camera look at target position
        /// </summary>
        protected Vector3 targetOffset = Vector3.Zero;

        /// <summary>
        /// It's a offset position of the camera's position.
        /// </summary>
        protected Vector3 positionOffset = Vector3.Zero;
        
        #endregion

        #region Properties

        public Matrix TargetMatrix             
        {
            set { targetMatrix = value; } 
            get { return targetMatrix; } 
        }
        public Vector3 TargetOffset            
        {
            set { targetOffset = value; }
            get { return targetOffset; } 
        }
        public Vector3 PositionOffset
        {
            set { positionOffset = value; }
            get { return positionOffset; } 
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public FollowCamera()
            : base()
        {
            targetMatrix = Matrix.Identity;
            targetOffset = Vector3.Zero;
            positionOffset = Vector3.Zero;
        }

        /// <summary>
        /// This configures a new view matrix by using target matrix and offset position.
        /// </summary>
        protected override void OnUpdate(GameTime gameTime)
        {
            //  Make a camera's target position using offset position
            Vector3 target = TargetMatrix.Translation +
                        Vector3.Multiply(TargetMatrix.Right, targetOffset.X) +
                        Vector3.Multiply(TargetMatrix.Up, targetOffset.Y) +
                        Vector3.Multiply(TargetMatrix.Forward, targetOffset.Z);

            Vector3 position = target + this.TrembleOffset +
                        Vector3.Multiply(TargetMatrix.Right, positionOffset.X) +
                        Vector3.Multiply(TargetMatrix.Up, positionOffset.Y) +
                        Vector3.Multiply(TargetMatrix.Forward, positionOffset.Z);

            //  Recalculate new view matrix
            SetView(position, target, Up);

            base.OnUpdate(gameTime);
        }

        /// <summary>
        /// Set the target matrix
        /// </summary>
        public void SetFollow(Matrix targetMatrix)
        {
            //  Set to following target
            this.targetMatrix = targetMatrix;
        }

        /// <summary>
        /// Set the target matrix and camera's offset position
        /// </summary>
        public void SetFollow(Matrix targetMatrix, 
                              Vector3 targetOffset, 
                              Vector3 cameraOffset)
        {
            //  Set to following target
            this.targetMatrix = targetMatrix;

            //  Set to target offset value
            this.TargetOffset = targetOffset;

            //  Set to position offset value
            this.PositionOffset = cameraOffset;
        }
    }
}
