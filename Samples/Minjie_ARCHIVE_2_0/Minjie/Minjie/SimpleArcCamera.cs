#region File Description
//-----------------------------------------------------------------------------
// SimpleArcCamera.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Minjie
{
    static class SimpleArcCamera
    {
        private static InputState inputState;
        public static InputState InputState
        {
            get { return inputState; }
            set { inputState = value; }
        }

        private static Matrix projection;
        public static Matrix ProjectionMatrix
        {
            get { return projection; }
        }

        private static Matrix view;
        public static Matrix ViewMatrix
        {
            get { return view; }
        }

        private const float fieldOfView = MathHelper.PiOver4;
        private const float nearPlaneDistance = 1f;
        private const float farPlaneDistance = 10000f;
        private static float aspectRatio = 0.75f;
        private static float cameraArc = -45f;
        private static float cameraDistance = 800f;
        private static float cameraRotation = 180f;
        public static float Rotation
        {
            get { return cameraRotation; }
        }


        /// <summary>
        /// Reset the camera.
        /// </summary>
        /// <param name="aspectRatio">The new aspect ratio.</param>
        public static void Reset(float aspectRatio)
        {
            SimpleArcCamera.aspectRatio = aspectRatio;
            SimpleArcCamera.cameraArc = -45f;
            SimpleArcCamera.cameraRotation = 180f;
            SimpleArcCamera.cameraDistance = 800f;

            SimpleArcCamera.projection =
                Matrix.CreatePerspectiveFieldOfView(fieldOfView,
                aspectRatio, nearPlaneDistance, farPlaneDistance);

            SimpleArcCamera.view = Matrix.CreateRotationY(
                MathHelper.ToRadians(cameraRotation)) *
                Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                Matrix.CreateLookAt(new Vector3(0, 0, -cameraDistance),
                                    Vector3.Zero, Vector3.Up);
        }


        /// <summary>
        /// Update the camera.
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            // if we don't have any input, don't bother
            if (inputState == null)
            {
                return;
            }

            // Calculate the camera's current position.
            if (inputState.ResetCamera)
            {
                Reset(aspectRatio);
            }

            float totalMilliseconds = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // check for input to rotate the camera up and down around the model
            if (inputState.IsKeyPress(Keys.Up))
            {
                cameraArc += totalMilliseconds * 0.1f;
            }
            else if (inputState.IsKeyPress(Keys.Down))
            {
                cameraArc -= totalMilliseconds * 0.1f;
            }
            cameraArc += inputState.CurrentGamePadStates[0].ThumbSticks.Right.Y * 
                totalMilliseconds * 0.25f;
            // limit the arc movement
            if (cameraArc > 90.0f)
            {
                cameraArc = 90.0f;
            }
            else if (cameraArc < -90.0f)
            {
                cameraArc = -90.0f;
            }

            // check for input to rotate the camera around the model
            if (inputState.IsKeyPress(Keys.Right))
            {
                cameraRotation += totalMilliseconds * 0.1f;
            }
            else if (inputState.IsKeyPress(Keys.Left))
            {
                cameraRotation -= totalMilliseconds * 0.1f;
            }
            cameraRotation += inputState.CurrentGamePadStates[0].ThumbSticks.Right.X *
                totalMilliseconds * 0.25f;
            // limit camera to one rotation
            while (cameraRotation > 360f)
            {
                cameraRotation -= 360f;
            }
            while (cameraRotation < 0f)
            {
                cameraRotation += 360f;
            }


            // Check for input to zoom camera in and out
            if (inputState.IsKeyPress(Keys.Z))
            {
                cameraDistance += totalMilliseconds;
            }
            else if (inputState.IsKeyPress(Keys.X))
            {
                cameraDistance -= totalMilliseconds;
            }
            cameraDistance += inputState.CurrentGamePadStates[0].Triggers.Left *
                totalMilliseconds;
            cameraDistance -= inputState.CurrentGamePadStates[0].Triggers.Right *
                totalMilliseconds;
            // limit the camera distance.
            if (cameraDistance > 5000.0f)
            {
                cameraDistance = 5000.0f;
            }
            else if (cameraDistance < 350.0f)
            {
                cameraDistance = 350.0f;
            }

            // recreate the view matrix
            view = Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                Matrix.CreateLookAt(new Vector3(0, 0, -cameraDistance),
                                    Vector3.Zero, Vector3.Up);
        }
    }
}
