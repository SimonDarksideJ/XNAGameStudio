#region File Description
//-----------------------------------------------------------------------------
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
#endregion

namespace SpaceShooter
{
    public class ChaseCamera : Camera
    {
        private float upScale = 10.0f;
        private Vector3 targetPosition;
        private Quaternion targetRotation;

        private Vector3 targetCameraOffset = new Vector3(0, 10, 10);
        private Vector3 targetlookAtOffset = Vector3.Zero;
        private float approachSpeed = 5.0f;
        private float offsetApproachSpeed = 5.0f;

        // Current Chase Camera State
        Vector3 cameraPosition;
        Vector3 cameraLookAt;
        Vector3 cameraUpPos;
        Vector3 cameraUp;

        Vector3 cameraOffset;
        Vector3 cameraLookAtOffset;

        Matrix viewProj;

        public float UpScale
        {
            get { return upScale; }
            set { upScale = value; }
        }

        public Vector3 TargetPosition
        {
            get { return targetPosition; }
            set { targetPosition = value; }
        }

        public Quaternion TargetRotation
        {
            get { return targetRotation; }
            set { targetRotation = value; }
        }

        public Vector3 TargetCameraOffset
        {
            get { return targetCameraOffset; }
            set { targetCameraOffset = value; }
        }

        public Vector3 TargetLookAtOffset
        {
            get { return targetlookAtOffset; }
            set { targetlookAtOffset = value; }
        }

        public float ApproachSpeed
        {
            get { return approachSpeed; }
            set { approachSpeed = value; }
        }

        public float OffsetApproachSpeed
        {
            get { return offsetApproachSpeed; }
            set { offsetApproachSpeed = value; }
        }

        public ChaseCamera(Game game) :
            base(game)
        {
        }

        public Vector3 CameraLookAt
        {
            get { return cameraLookAt; }
        }

        public Vector3 CameraUp
        {
            get { return cameraUp; }
        }

        public Matrix ViewProj
        {
            get { return viewProj; }
        }

        /// <summary>
        /// Starts up the chase camera
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            ApproachSpeed = 5.0f;
            OffsetApproachSpeed = 5.0f;
            TargetCameraOffset = new Vector3(0, 2, 225);
            TargetLookAtOffset = new Vector3(0, -4, -80);

            Matrix targetMatrix = Matrix.CreateFromQuaternion(targetRotation);

            position = targetPosition;
            cameraPosition = Vector3.Zero;
            cameraLookAt = Vector3.Transform(targetlookAtOffset, targetMatrix);
            cameraOffset = targetCameraOffset;
            cameraLookAtOffset = targetlookAtOffset;
            cameraUpPos = Vector3.Transform(Vector3.Up * upScale, targetMatrix);

            cameraUp = cameraUp - cameraPosition;
            cameraUp.Normalize();

            View = Matrix.CreateLookAt(cameraPosition, cameraLookAt, cameraUp);
        }

        public override void Update(GameTime gameTime)
        {
            // Currently, we fix the chase camera parameters every frame
            {
                ApproachSpeed = 5.0f;
                OffsetApproachSpeed = 5.0f;
                TargetCameraOffset = new Vector3(0, 2, 25);
                TargetLookAtOffset = new Vector3(0, -4, -8);
            }

            // Update offsets.
            float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (time <= 0.0f)
                time = .001f;

            float t = offsetApproachSpeed * time;

            cameraOffset = Vector3.Lerp(cameraOffset, targetCameraOffset, t);
            cameraLookAtOffset = Vector3.Lerp(cameraLookAtOffset, targetlookAtOffset, t);

            Matrix targetMatrix = Matrix.CreateFromQuaternion(targetRotation);
            targetMatrix.Translation = targetPosition;

            // Update positions.
            Vector3 pos = Vector3.Transform(cameraOffset, targetMatrix);
            Vector3 lookAt = Vector3.Transform(cameraLookAtOffset, targetMatrix);
            Vector3 upPos = Vector3.Transform(Vector3.Up * upScale, targetMatrix);

            cameraPosition = Vector3.Lerp(cameraPosition, pos, t);
            cameraLookAt = Vector3.Lerp(cameraLookAt, lookAt, t);
            cameraUpPos = Vector3.Lerp(cameraUpPos, upPos, t);

            Vector3 up = cameraUpPos - cameraPosition;
            if (up.Length() > 0.001f)
            {
                cameraUp = Vector3.Normalize(up);
            }

            position = targetPosition;
            rotation = targetRotation;

            View = Matrix.CreateLookAt(cameraPosition, cameraLookAt, cameraUp);

            viewProj = view * proj;
            bf.Matrix = viewProj;
        }
    }
}