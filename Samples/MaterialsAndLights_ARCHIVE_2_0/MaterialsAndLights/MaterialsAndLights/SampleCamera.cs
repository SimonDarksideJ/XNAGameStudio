#region File Description
//-----------------------------------------------------------------------------
// SampleCamera.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace MaterialsAndLightsSample
{
    public enum SampleArcBallCameraMode
    {
        /// <summary>
        /// A totally free-look arcball that orbits relative
        /// to its orientation
        /// </summary>
        Free = 0,

        /// <summary>
        /// A camera constrained by roll so that orbits only
        /// occur on latitude and longitude
        /// </summary>
        RollConstrained = 1

    }

    /// <summary>
    /// An example arc ball camera
    /// </summary>
    public class SampleArcBallCamera
    {
        #region Helper Functions
        /// <summary>
        /// Uses a pair of keys to simulate a positive or negative axis input.
        /// </summary>
        public static float ReadKeyboardAxis(KeyboardState keyState, Keys downKey,
            Keys upKey)
        {
            float value = 0;

            if (keyState.IsKeyDown(downKey))
                value -= 1.0f;

            if (keyState.IsKeyDown(upKey))
                value += 1.0f;

            return value;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The location of the look-at target
        /// </summary>
        private Vector3 targetValue;

        /// <summary>
        /// The distance between the camera and the target
        /// </summary>
        private float distanceValue;

        /// <summary>
        /// The orientation of the camera relative to the target
        /// </summary>
        private Quaternion orientation;

        private float inputDistanceRateValue;
        private const float InputTurnRate = 3.0f;
        private SampleArcBallCameraMode mode;
        private float yaw, pitch;
        #endregion

        #region Constructors
        /// <summary>
        /// Create an arcball camera that allows free orbit around a target point.
        /// </summary>
        public SampleArcBallCamera(SampleArcBallCameraMode controlMode)
        {
            //orientation quaternion assumes a Pi rotation so you're facing the "front"
            //of the model (looking down the +Z axis)
            orientation = Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.Pi);

            mode = controlMode;
            inputDistanceRateValue = 4.0f;
            yaw = MathHelper.Pi;
            pitch = 0;
        }
        #endregion

        #region Calculated Camera Properties
        /// <summary>
        /// Get the forward direction vector of the camera.
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                //R v R' where v = (0,0,-1,0)
                //The equation can be reduced because we know the following things:
                //  1.  We're using unit quaternions
                //  2.  The initial aspect does not change
                //The reduced form of the same equation follows
                Vector3 dir = Vector3.Zero;
                dir.X = -2.0f *
                    ((orientation.X * orientation.Z) + (orientation.W * orientation.Y));
                dir.Y = 2.0f *
                    ((orientation.W * orientation.X) - (orientation.Y * orientation.Z));
                dir.Z =
                    ((orientation.X * orientation.X) + (orientation.Y * orientation.Y)) -
                    ((orientation.Z * orientation.Z) + (orientation.W * orientation.W));
                Vector3.Normalize(ref dir, out dir);
                return dir;

            }
        }

        /// <summary>
        /// Get the right direction vector of the camera.
        /// </summary>
        public Vector3 Right
        {
            get
            {
                //R v R' where v = (1,0,0,0)
                //The equation can be reduced because we know the following things:
                //  1.  We're using unit quaternions
                //  2.  The initial aspect does not change
                //The reduced form of the same equation follows
                Vector3 right = Vector3.Zero;
                right.X =
                    ((orientation.X * orientation.X) + (orientation.W * orientation.W)) -
                    ((orientation.Z * orientation.Z) + (orientation.Y * orientation.Y));
                right.Y = 2.0f *
                    ((orientation.X * orientation.Y) + (orientation.Z * orientation.W));
                right.Z = 2.0f *
                    ((orientation.X * orientation.Z) - (orientation.Y * orientation.W));

                return right;

            }
        }

        /// <summary>
        /// Get the up direction vector of the camera.
        /// </summary>
        public Vector3 Up
        {
            get
            {
                //R v R' where v = (0,1,0,0)
                //The equation can be reduced because we know the following things:
                //  1.  We're using unit quaternions
                //  2.  The initial aspect does not change
                //The reduced form of the same equation follows
                Vector3 up = Vector3.Zero;
                up.X = 2.0f *
                    ((orientation.X * orientation.Y) - (orientation.Z * orientation.W));
                up.Y =
                    ((orientation.Y * orientation.Y) + (orientation.W * orientation.W)) -
                    ((orientation.Z * orientation.Z) + (orientation.X * orientation.X));
                up.Z = 2.0f *
                    ((orientation.Y * orientation.Z) + (orientation.X * orientation.W));
                return up;
            }
        }

        

        /// <summary>
        /// Get the View (look at) Matrix defined by the camera
        /// </summary>
        public Matrix ViewMatrix
        {
            get
            {
                return Matrix.CreateLookAt(targetValue -
                    (Direction * distanceValue), targetValue, Up);
            }
        }

        public SampleArcBallCameraMode ControlMode
        {
            get { return mode; }
            set
            {
                if (value != mode)
                {
                    mode = value;
                    SetCamera(targetValue - (Direction* distanceValue),
                              targetValue, Vector3.Up);
                }
            }
        }


        #endregion

        #region Position Controls
        /// <summary>
        /// Get or Set the current target of the camera
        /// </summary>
        public Vector3 Target
        {
            get
            { return targetValue; }
            set
            { targetValue = value; }
        }

        /// <summary>
        /// Get or Set the camera's distance to the target.
        /// </summary>
        public float Distance
        {
            get
            { return distanceValue; }
            set
            { distanceValue = value; }
        }

        /// <summary>
        /// Sets the rate of distance change 
        /// when automatically handling input
        /// </summary>
        public float InputDistanceRate
        {
            get
            { return inputDistanceRateValue; }
            set
            { inputDistanceRateValue = value; }
        }

        /// <summary>
        /// Get/Set the camera's current postion.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return targetValue - (Direction * Distance);
            }
            set
            {
                SetCamera(value, targetValue, Vector3.Up);
            }
        }
        #endregion

        #region Orbit Controls
        /// <summary>
        /// Orbit directly upwards in Free camera or on
        /// the longitude line when roll constrained
        /// </summary>
        public void OrbitUp(float angle)
        {

            switch (mode)
            {
                case SampleArcBallCameraMode.Free:
                    //rotate the aspect by the angle 
                    orientation = orientation *
                     Quaternion.CreateFromAxisAngle(Vector3.Right, -angle);

                    //normalize to reduce errors
                    Quaternion.Normalize(ref orientation, out orientation);
                    break;
                case SampleArcBallCameraMode.RollConstrained:
                    //update the yaw
                    pitch -= angle;

                    //constrain pitch to vertical to avoid confusion
                    pitch = MathHelper.Clamp(pitch, -(MathHelper.PiOver2) + .0001f,
                        (MathHelper.PiOver2) - .0001f);

                    //create a new aspect based on pitch and yaw
                    orientation = Quaternion.CreateFromAxisAngle(Vector3.Up, -yaw) *
                        Quaternion.CreateFromAxisAngle(Vector3.Right, pitch);
                    break;
            }
        }

        /// <summary>
        /// Orbit towards the Right vector in Free camera
        /// or on the latitude line when roll constrained
        /// </summary>
        public void OrbitRight(float angle)
        {
            switch (mode)
            {
                case SampleArcBallCameraMode.Free:
                    //rotate the aspect by the angle 
                    orientation = orientation *
                        Quaternion.CreateFromAxisAngle(Vector3.Up, angle);

                    //normalize to reduce errors
                    Quaternion.Normalize(ref orientation, out orientation);
                    break;
                case SampleArcBallCameraMode.RollConstrained:
                    //update the yaw
                    yaw -= angle;

                    //float mod yaw to avoid eventual precision errors
                    //as we move away from 0
                    yaw = yaw % MathHelper.TwoPi;

                    //create a new aspect based on pitch and yaw
                    orientation = Quaternion.CreateFromAxisAngle(Vector3.Up, -yaw) *
                        Quaternion.CreateFromAxisAngle(Vector3.Right, pitch);
                    
                    //normalize to reduce errors
                    Quaternion.Normalize(ref orientation, out orientation);
                    break;
            }
        }

        /// <summary>
        /// Rotate around the Forward vector 
        /// in free-look camera only
        /// </summary>
        public void RotateClockwise(float angle)
        {
           

            switch (mode)
            {
                case SampleArcBallCameraMode.Free:
                    //rotate the orientation around the direction vector
                    orientation = orientation *
                        Quaternion.CreateFromAxisAngle(Vector3.Forward, angle);
                    Quaternion.Normalize(ref orientation, out orientation);
                    break;
                case SampleArcBallCameraMode.RollConstrained:
                    //Do nothing, we don't want to roll at all to stay consistent
                    break;
            }
        }

        /// <summary>
        /// Sets up a quaternion & position from vector camera components
        /// and oriented the camera up
        /// </summary>
        /// <param name="eye">The camera position</param>
        /// <param name="lookAt">The camera's look-at point</param>
        /// <param name="up"></param>
        public void SetCamera(Vector3 position, Vector3 target, Vector3 up)
        {
            //Create a look at matrix, to simplify matters a bit
            Matrix temp = Matrix.CreateLookAt(position, target, up);

            //invert the matrix, since we're determining the
            //orientation from the rotation matrix in RH coords
            temp = Matrix.Invert(temp);

            //set the postion
            targetValue = target;

            //create the new aspect from the look-at matrix
            orientation = Quaternion.CreateFromRotationMatrix(temp);

            //When setting a new eye-view direction 
            //in one of the gimble-locked modes, the yaw and
            //pitch gimble must be calculated.
            if (mode != SampleArcBallCameraMode.Free)
            {
                //first, get the direction projected on the x/z plne
                Vector3 dir = Direction;
                dir.Y = 0;
                if (dir.Length() == 0f)
                {
                    dir = Vector3.Forward;
                }
                dir.Normalize();

                //find the yaw of the direction on the x/z plane
                //and use the sign of the x-component since we have 360 degrees
                //of freedom
                yaw = (float)(Math.Acos(-dir.Z) * Math.Sign(dir.X));

                //Get the pitch from the angle formed by the Up vector and the 
                //the forward direction, then subtracting Pi / 2, since 
                //we pitch is zero at Forward, not Up.
                pitch = (float)-(Math.Acos(Vector3.Dot(Vector3.Up, Direction))
                    - MathHelper.PiOver2);
            }
        }
        #endregion

        #region Input Handlers
        /// <summary>
        /// Handle default keyboard input for a camera
        /// </summary>
        public void HandleDefaultKeyboardControls(KeyboardState kbState,
            GameTime gameTime)
        {
            if (gameTime == null)
            {
                throw new ArgumentNullException("gameTime", 
                    "GameTime parameter cannot be null.");
            }

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float dX = elapsedTime * ReadKeyboardAxis(
                kbState, Keys.A, Keys.D) * InputTurnRate;
            float dY = elapsedTime * ReadKeyboardAxis(
                kbState, Keys.S, Keys.W) * InputTurnRate;

            if (dY != 0) OrbitUp(dY);
            if (dX != 0) OrbitRight(dX);


            distanceValue += ReadKeyboardAxis(kbState, Keys.Z, Keys.X)
                * inputDistanceRateValue * elapsedTime;
            if (distanceValue < .001f) distanceValue = .001f;

            if (mode != SampleArcBallCameraMode.Free)
            {
                float dR = elapsedTime * ReadKeyboardAxis(
                    kbState, Keys.Q, Keys.E) * InputTurnRate;
                if (dR != 0) RotateClockwise(dR);
            }
        }

        /// <summary>
        /// Handle default gamepad input for a camera
        /// </summary>
        public void HandleDefaultGamepadControls(GamePadState gpState, GameTime gameTime)
        {
            if (gameTime == null)
            {
                throw new ArgumentNullException("gameTime", 
                    "GameTime parameter cannot be null.");
            }

            if (gpState.IsConnected)
            {
                float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                float dX = gpState.ThumbSticks.Right.X * elapsedTime * InputTurnRate;
                float dY = gpState.ThumbSticks.Right.Y * elapsedTime * InputTurnRate;
                float dR = gpState.Triggers.Right * elapsedTime * InputTurnRate;
                dR-= gpState.Triggers.Left * elapsedTime * InputTurnRate;

                //change orientation if necessary
                if(dY != 0) OrbitUp(dY);
                if(dX != 0) OrbitRight(dX);
                if (dR != 0) RotateClockwise(dR);

                //decrease distance to target (move forward)
                if (gpState.Buttons.A == ButtonState.Pressed)
                {
                    distanceValue -= elapsedTime * inputDistanceRateValue;
                }
                //increase distance to target (move back)
                if (gpState.Buttons.B == ButtonState.Pressed)
                {
                    distanceValue += elapsedTime * inputDistanceRateValue;
                }
                if (distanceValue < .001f) distanceValue = .001f;
            }  
        }
        #endregion

        #region Misc Camera Controls
        /// <summary>
        /// Reset the camera to the defaults set in the constructor
        /// </summary>
        public void Reset()
        {
            //orientation quaternion assumes a Pi rotation so you're facing the "front"
            //of the model (looking down the +Z axis)
            orientation = Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.Pi);
            distanceValue = 3f;
            targetValue = Vector3.Zero;
            yaw = MathHelper.Pi;
            pitch = 0;
        }
        #endregion
    }
}
