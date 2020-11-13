#region File Description
//-----------------------------------------------------------------------------
// ChaseCamera.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Properties;
using Color = Microsoft.Xna.Framework.Color;
using Model = RacingGame.Graphics.Model;
#endregion

namespace RacingGame.GameLogic
{
    /// <summary>
    /// Chase camera for our car. We are always close behind it.
    /// The camera rotation is not the same as the current car rotation,
    /// we interpolate the values a bit, allowing the user to do small changes
    /// without rotating the camera frantically. Also feels more realistic in
    /// curves. Derived from the CarController class, which controls the car
    /// by the user input. This camera class is not controlled by the user,
    /// its all automatic!
    /// </summary>
    public class ChaseCamera : CarPhysics
    {
        #region Variables
        /// <summary>
        /// Current camera position.
        /// </summary>
        protected Vector3 cameraPos;
        /// <summary>
        /// Distance of the camera to our car.
        /// </summary>
        private float cameraDistance;

        /// <summary>
        /// Look vector to the car. The car is our look at target. The up
        /// vector is the same as the one from the car, but the look vector is
        /// different because we slowly interpolate it.
        /// </summary>
        private Vector3 cameraLookVector;

        /// <summary>
        /// Camera modes
        /// </summary>
        public enum CameraMode
        {
            /// <summary>
            /// Default mode for game and menu, just chasing the car.
            /// </summary>
            Default,
            /// <summary>
            /// Free camera mode, allows to freely rotate and zoom around the car,
            /// much cooler than the Default mode for testing and stuff.
            /// Also used when we lose a game (fallen out of the track).
            /// </summary>
            FreeCamera,
        }

        /// <summary>
        /// Current camera mode.
        /// </summary>
        private CameraMode cameraMode = CameraMode.Default;//FreeCamera;

        /// <summary>
        /// Rotation matrix, used in UpdateViewMatrix.
        /// </summary>
        private Matrix rotMatrix = Matrix.Identity;
        /// <summary>
        /// Rotation matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public Matrix RotationMatrix
        {
            get
            {
                return rotMatrix;
            }
        }
        #endregion

        #region Camera wobbel
        /// <summary>
        /// Max. value for camera wobbel timeout.
        /// </summary>
        const int MaxCameraWobbelTimeoutMs = 700;

        /// <summary>
        /// Camera wobbel timeout.
        /// Used to shake camera after a collision.
        /// </summary>
        static float cameraWobbelTimeoutMs = 0;

        /// <summary>
        /// Camera wobbel factor.
        /// </summary>
        static float cameraWobbelFactor = 1.0f;

        /// <summary>
        /// Sets the camera to wobble which fades over time.
        /// </summary>
        /// <param name="factor">Factor</param>
        public static void WobbelCamera(float wobbelFactor)
        {
            cameraWobbelTimeoutMs = (int)
                //((0.75f + 0.5f * wobbelFactor) *
                (MaxCameraWobbelTimeoutMs);
            cameraWobbelFactor = wobbelFactor;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Camera position
        /// </summary>
        /// <returns>Vector 3</returns>
        public Vector3 CameraPosition
        {
            get
            {
                return cameraPos;
            }
        }

        /// <summary>
        /// Get current x axis with help of the current view matrix.
        /// </summary>
        /// <returns>Vector 3</returns>
        static public Vector3 XAxis
        {
            get
            {
                // Get x column
                return new Vector3(
                    BaseGame.ViewMatrix.M11,
                    BaseGame.ViewMatrix.M21,
                    BaseGame.ViewMatrix.M31);
            }
        }

        /// <summary>
        /// Get current y axis with help of the current view matrix.
        /// </summary>
        /// <returns>Vector 3</returns>
        static public Vector3 YAxis
        {
            get
            {
                // Get y column
                return new Vector3(
                    BaseGame.ViewMatrix.M12,
                    BaseGame.ViewMatrix.M22,
                    BaseGame.ViewMatrix.M32);
            }
        }

        /// <summary>
        /// Get current z axis with help of the current view matrix.
        /// </summary>
        /// <returns>Vector 3</returns>
        static public Vector3 ZAxis
        {
            get
            {
                // Get z column
                return new Vector3(
                    BaseGame.ViewMatrix.M13,
                    BaseGame.ViewMatrix.M23,
                    BaseGame.ViewMatrix.M33);
            }
        }

        /// <summary>
        /// Free camera
        /// </summary>
        /// <returns>Bool</returns>
        public bool FreeCamera
        {
            get
            {
                return cameraMode == CameraMode.FreeCamera;
            }
            set
            {
                if (value == true)
                    cameraMode = CameraMode.FreeCamera;
                else
                    cameraMode = CameraMode.Default;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create chase camera. Sets the car position and the camera position,
        /// which is then used to rotate around the car.
        /// </summary>
        /// <param name="setCarPosition">Set car position</param>
        /// <param name="setDirection">Set direction</param>
        /// <param name="setUp">Set up</param>
        /// <param name="setCameraPos">Set camera pos</param>
        public ChaseCamera(Vector3 setCarPosition, Vector3 setDirection,
            Vector3 setUp, Vector3 setCameraPos)
            : base(setCarPosition, setDirection, setUp)
        {
            // Set camera position and calculate rotation from look pos
            SetCameraPosition(setCameraPos);
        }

        /// <summary>
        /// Create chase camera. Sets the car position and the camera position,
        /// which is then used to rotate around the car.
        /// </summary>
        /// <param name="setCarPosition">Set car position</param>
        /// <param name="setCameraPos">Set camera pos</param>
        public ChaseCamera(Vector3 setCarPosition, Vector3 setCameraPos)
            : base(setCarPosition)
        {
            // Set camera position and calculate rotation from look pos
            SetCameraPosition(setCameraPos);
        }

        /// <summary>
        /// Create chase camera. Just sets the car position.
        /// The chase camera is set behind it.
        /// </summary>
        /// <param name="setCarPosition">Set car position</param>
        public ChaseCamera(Vector3 setCarPosition)
            : base(setCarPosition)
        {
            // Set camera position and calculate rotation from look pos
            SetCameraPosition(
                //setCarPosition - new Vector3(0, 0.5f, 1.0f) * carDir);
                setCarPosition + new Vector3(0, 10.0f, 25.0f));
        }
        #endregion

        #region Set position
        /// <summary>
        /// Set camera position
        /// </summary>
        /// <param name="setCameraPos">Set camera position</param>
        public void SetCameraPosition(Vector3 setCameraPos)
        {
            cameraPos = setCameraPos;
            cameraDistance = Vector3.Distance(LookAtPos, cameraPos);
            cameraLookVector = LookAtPos - cameraPos;
            wannaCameraDistance = cameraDistance;
            wannaCameraLookVector = cameraLookVector;

            // Build look at matrix
            rotMatrix = Matrix.CreateLookAt(cameraPos, LookAtPos, CarUpVector);
        }

        Vector3 wannaCameraLookVector = Vector3.Zero;
        float wannaCameraDistance = 0.0f;

        /// <summary>
        /// Interpolate camera position
        /// </summary>
        /// <param name="setInterpolatedCameraPos">Set interpolated camera
        /// position</param>
        public void InterpolateCameraPosition(Vector3 setInterpolatedCameraPos)
        {
            // Don't use for free camera
            if (FreeCamera)
                return;

            if (wannaCameraDistance == 0.0f)
                SetCameraPosition(setInterpolatedCameraPos);

            wannaCameraDistance =
                Vector3.Distance(LookAtPos, setInterpolatedCameraPos);
            wannaCameraLookVector = LookAtPos - setInterpolatedCameraPos;
        }
        #endregion

        #region Handle free camera
        /// <summary>
        /// Helper values to keep the free camera steady.
        /// </summary>
        private Vector3 freeCameraRot = new Vector3(
            MathHelper.Pi, 0, -MathHelper.Pi / 2);
        /// <summary>
        /// Wanna have camera rotation
        /// </summary>
        Vector3 wannaHaveCameraRotation = Vector3.Zero;
        /// <summary>
        /// Handle free camera, only used for unit tests.
        /// </summary>
        private void HandleFreeCamera()
        {
            // Don't control the camera in the menu or game, only in unit tests!
            if (cameraMode != CameraMode.FreeCamera)
                return;

            float rotationFactor = 0.0075f;
            float gamePadRotFactor = 5.0f * BaseGame.MoveFactorPerSecond;

            // We don't use lookDistance or cameraRotation here, so we have
            // to calculate this values here.
            cameraDistance = cameraLookVector.Length();

            if (wannaHaveCameraRotation.Equals(Vector3.Zero))
                wannaHaveCameraRotation = freeCameraRot;
            Vector3 rot = wannaHaveCameraRotation;

            float addRotX =
                // Allow mouse input
                -Input.MouseXMovement * rotationFactor +
                // And gamepad input
                Input.GamePad.ThumbSticks.Left.X * gamePadRotFactor;
            // Also allow gamepad and keyboard cursors
            if (addRotX == 0)
            {
                if (Input.GamePadLeftPressed ||
                    Input.KeyboardLeftPressed)
                    addRotX = -gamePadRotFactor;
                if (Input.GamePadRightPressed ||
                    Input.KeyboardRightPressed)
                    addRotX = +gamePadRotFactor;
            }
            float addRotY =
                // Allow mouse input
                -Input.MouseYMovement * rotationFactor +
                // And gamepad input
                Input.GamePad.ThumbSticks.Left.Y * gamePadRotFactor;
            // Also allow gamepad and keyboard cursors
            if (addRotY == 0)
            {
                if (Input.GamePadUpPressed ||
                    Input.KeyboardUpPressed)
                    addRotY = -gamePadRotFactor;
                if (Input.GamePadDownPressed ||
                    Input.KeyboardDownPressed)
                    addRotY = +gamePadRotFactor;
            }

            wannaHaveCameraRotation = new Vector3(
                rot.X,
                rot.Y + addRotY,
                rot.Z + addRotX);

            // Mix camera rotation slowly to wanna have rotation
            freeCameraRot = Vector3.Lerp(
                freeCameraRot, wannaHaveCameraRotation, 0.5f);

            #region fix the "up-rotaion" to 0-180 degrees //old: 180-360 degrees
            // Substract a very small value to make sure we never reach PI,
            // this causes the z rotation to mess everything else up ...
            float minRotationRange = BaseGame.Epsilon;
            float maxRotationRange = (float)Math.PI - BaseGame.Epsilon;
            if (freeCameraRot.X < minRotationRange)
            {
                freeCameraRot.X = minRotationRange;
            }
            else if (freeCameraRot.X > maxRotationRange)
            {
                freeCameraRot.X = maxRotationRange;
            }
            #endregion

            // Calculate cameraPos like in HandleLookPosCamera()
            cameraLookVector = new Vector3(0, 0, cameraDistance);
            cameraLookVector = Vector3.TransformNormal(cameraLookVector,
                Matrix.CreateRotationX(freeCameraRot.X) *
                Matrix.CreateRotationY(freeCameraRot.Y) *
                Matrix.CreateRotationZ(freeCameraRot.Z));

            float moveFactor =
                (Input.Keyboard.IsKeyDown(Keys.LeftShift) ? 20.0f : 40.0f) *
                BaseGame.MoveFactorPerSecond;
            float smallMoveFactor = moveFactor / 4.0f;

            float lookDistanceChange = 0.0f;
            // Page up/down or Home/End to zoom in and out.
            if (Input.Keyboard.IsKeyDown(Keys.PageUp))
                lookDistanceChange += moveFactor * 0.05f;
            if (Input.Keyboard.IsKeyDown(Keys.PageDown))
                lookDistanceChange -= moveFactor * 0.05f;
            if (Input.Keyboard.IsKeyDown(Keys.Home))
                lookDistanceChange += smallMoveFactor * 0.05f;
            if (Input.Keyboard.IsKeyDown(Keys.End))
                lookDistanceChange -= smallMoveFactor * 0.05f;

            // Also allow mouse wheel to zoom
            if (Input.MouseWheelDelta != 0)
            {
                lookDistanceChange =
                    Input.MouseWheelDelta * BaseGame.MoveFactorPerSecond / 16.0f;
            }

            // Also allow gamepad to zoom
            if (Input.GamePad.ThumbSticks.Right.Y != 0)
            {
                lookDistanceChange =
                    Input.GamePad.ThumbSticks.Right.Y * BaseGame.MoveFactorPerSecond;
            }

            if (lookDistanceChange != 0)
            {
                // Half zoom effect if shift is pressed
                if (Input.Keyboard.IsKeyDown(Keys.LeftShift))
                    lookDistanceChange /= 2.0f;

                cameraDistance *= 1.0f - lookDistanceChange;
                if (cameraDistance < 1.0f)
                    cameraDistance = 1.0f;

                // Calculate cameraPos like in HandleLookPosCamera()
                cameraLookVector = Vector3.TransformNormal(
                    new Vector3(0, 0, cameraDistance),
                    Matrix.CreateRotationX(freeCameraRot.X) *
                    Matrix.CreateRotationY(freeCameraRot.Y) *
                    Matrix.CreateRotationZ(freeCameraRot.Z));
            }

            // Make sure we use these new values and don't interpolate them back.
            wannaCameraDistance = cameraDistance;
            wannaCameraLookVector = cameraLookVector;
        }
        #endregion

        #region Update view matrix
        Vector3 lastCameraWobble = Vector3.Zero;
        /// <summary>
        /// Update view matrix
        /// </summary>
        private void UpdateViewMatrix()
        {
            cameraDistance = cameraDistance * 0.9f + wannaCameraDistance * 0.1f;

            // Better interpolation formula, not good for slow framerates,
            // but looks much better on high frame rates this way.
            cameraLookVector =
                (cameraLookVector * 0.9f) +
                (wannaCameraLookVector * 0.1f);

            // Update camera pos based on the current lookPos and cameraDistance
            cameraPos = LookAtPos + cameraLookVector;

            // Build look at matrix
            rotMatrix = Matrix.CreateLookAt(cameraPos, LookAtPos, CarUpVector);

            // Is camera wobbeling?
            if (cameraWobbelTimeoutMs > 0)
            {
                cameraWobbelTimeoutMs -= BaseGame.ElapsedTimeThisFrameInMilliseconds;
                if (cameraWobbelTimeoutMs < 0)
                    cameraWobbelTimeoutMs = 0;
            }

            // Add camera shake if camera wobbel effect is on
            if (cameraWobbelTimeoutMs > 0 &&
                // But only if not zooming in and if in game.
                ZoomInTime <= StartGameZoomTimeMilliseconds)
            {
                float effectStrength = 1.5f * cameraWobbelFactor *
                    (cameraWobbelTimeoutMs / (float)MaxCameraWobbelTimeoutMs);
                // Interpolate, make wobbleing more smoooth than in Rocket Commander
                lastCameraWobble =
                    lastCameraWobble * 0.9f +
                    RandomHelper.GetRandomVector3(
                    -effectStrength, +effectStrength) * 0.1f;
                rotMatrix *= Matrix.CreateTranslation(lastCameraWobble);
            }

            // Just set view matrix
            BaseGame.ViewMatrix = rotMatrix;
        }
        #endregion

        #region Reset
        /// <summary>
        /// Resets just the camera wobbel factor here.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            cameraWobbelFactor = 0;
        }

        /// <summary>
        /// Clear variables for game over
        /// </summary>
        public override void ClearVariablesForGameOver()
        {
            base.ClearVariablesForGameOver();
            cameraWobbelFactor = 0;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update camera, should be called every frame to handle all the input.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // Only allow control when zooming is finished.
            HandleFreeCamera();

            UpdateViewMatrix();
        }
        #endregion
    }
}
