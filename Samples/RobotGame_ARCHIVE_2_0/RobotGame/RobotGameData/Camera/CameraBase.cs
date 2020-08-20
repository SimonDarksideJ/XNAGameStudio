#region File Description
//-----------------------------------------------------------------------------
// CameraBase.cs
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
using RobotGameData.GameInterface;
#endregion

namespace RobotGameData.Camera
{
    /// <summary>
    /// This is the base class of every camera.
    /// It can configure View matrix and Projection matrix.
    /// </summary>
    public class CameraBase : GameNode
    {
        #region Fields

        /// <summary>
        /// Field of View
        /// </summary>
        float fov = MathHelper.PiOver4;

        float screenWidth = 0;
        float screenHeight = 0;
        float aspectRatio = 0;

        /// <summary>
        /// The Camera's near distance
        /// </summary>
        float near = 1.0f;

        /// <summary>
        /// The Camera's far distance
        /// </summary>
        float far = 1000.0f;

        /// <summary>
        /// The Camera position
        /// </summary>
        Vector3 position = Vector3.Zero;

        /// <summary>
        /// The Camera old position
        /// </summary>
        Vector3 oldPosition = Vector3.Zero;

        /// <summary>
        /// The Camera up vector
        /// </summary>
        Vector3 up = Vector3.Up;

        /// <summary>
        /// The Camera right vector
        /// </summary>
        Vector3 right = Vector3.Right;

        /// <summary>
        /// The Camera direction
        /// </summary>
        Vector3 direction = Vector3.Forward;

        /// <summary>
        /// The camera looks at the target position.
        /// </summary>
        Vector3 target = Vector3.Forward;

        /// <summary>
        /// The Camera velocity
        /// </summary>
        Vector3 velocity = Vector3.Zero;
        
        /// <summary>
        /// The projection matrix
        /// </summary>
        Matrix projectionMatrix = Matrix.Identity;

        /// <summary>
        /// The view matrix
        /// </summary>
        Matrix viewMatrix = Matrix.Identity;        

        #endregion

        #region Properties

        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
            protected set { projectionMatrix = value; }
        }

        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
            protected set { viewMatrix = value; }
        }

        public Vector3 Position
        {
            get { return position; }
        }

        public Vector3 Direction
        {
            get { return direction; }
        }

        public Vector3 Target
        {
            get { return target; }
        }

        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public Vector3 Up
        {
            get { return up; }
        }

        public Vector3 Right
        {
            get { return right; }
        }

        public float AspectRatio
        {
            get { return aspectRatio;  }
        }

        public float FieldOfView
        {
            get { return fov;    } 
        }

        public float Near
        {
            get { return near; }
        }

        public float Far
        {
            get { return far; }
        }

        public float Width
        {
            get { return screenWidth;    }
        }

        public float Height
        {
            get { return screenHeight;    }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public CameraBase()
            : base()
        {
            near = 1.0f;
            far = 1000.0f;
            position = oldPosition = Vector3.Zero;
            up = Vector3.Up;
            right = Vector3.Right;
            direction = Vector3.Forward;
            target = Vector3.Forward;
            velocity = Vector3.Zero;
            projectionMatrix = Matrix.Identity;
            viewMatrix = Matrix.Identity;
        }

        /// <summary>
        /// Update a velocity
        /// </summary>
        protected override void OnUpdate(GameTime gameTime)
        {
            if (oldPosition == this.Position)
            {
                this.velocity = Vector3.Zero;
            }
            else
            {
                this.velocity = this.position - oldPosition;

                oldPosition = this.position;
            }

            base.OnUpdate(gameTime);
        }

        /// <summary>
        /// Set the view matrix of the camera
        /// </summary>
        public Matrix SetView(Vector3 position, Vector3 target, Vector3 up)
        {
            //  Make a camera's direction
            this.direction = target - position;
            this.direction.Normalize();

            this.position = position;
            this.target = target;
            this.up = up;

            //  Make a camera's right vector
            this.right = Vector3.Cross(direction, up);
            this.right.Normalize();

            //  Make a camera's view matrix
            ViewMatrix = Matrix.CreateLookAt(this.Position, this.target, this.Up);
                        
            return ViewMatrix;
        }

        /// <summary>
        /// Set the projection matrix of the camera
        /// </summary>
        public Matrix SetPespective( float fov, float width, float height, 
                                     float near, float far)
        {
            this.screenWidth = width;
            this.screenHeight = height;
            this.fov = fov;
            this.near = near;
            this.far = far;

            aspectRatio = screenWidth / screenHeight;

            //  Make a camera's projection matrix
            projectionMatrix = 
                    Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far);

            return projectionMatrix;
        }

        /// <summary>
        /// It changes the width and height of screen.
        /// </summary>
        public void Resize(float width, float height)
        {
            SetPespective(this.fov, width, height, this.Near, this.Far);
        }

        /// <summary>
        /// Reset the camera
        /// </summary>
        public void Reset()
        {
            this.position = Vector3.Zero;
            this.up = Vector3.Up;
            this.right = Vector3.Right;
            this.direction = Vector3.Forward;
            this.target = position + direction;

            this.ViewMatrix = Matrix.Identity;            
        }
    }
}
