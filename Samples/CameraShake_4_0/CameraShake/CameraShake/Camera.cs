#region File Description
//-----------------------------------------------------------------------------
// Camera.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;

namespace CameraShake
{
    /// <summary>
    /// A very basic camera that supports the ability to shake.
    /// </summary>
    public class Camera
    {
        // We only need one Random object no matter how many Cameras we have
        private static readonly Random random = new Random();

        // Are we shaking?
        private bool shaking;

        // The maximum magnitude of our shake offset
        private float shakeMagnitude;

        // The total duration of the current shake
        private float shakeDuration;

        // A timer that determines how far into our shake we are
        private float shakeTimer;

        // The shake offset vector
        private Vector3 shakeOffset;

        /// <summary>
        /// The position of the camera.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The target location the camera is looking at.
        /// </summary>
        public Vector3 Target = Vector3.Zero;

        /// <summary>
        /// The up vector of the camera.
        /// </summary>
        public Vector3 Up = Vector3.Up;

        /// <summary>
        /// Gets the View matrix from the camera
        /// </summary>
        public Matrix View
        {
            get
            {
                // Start with our regular position and target
                Vector3 position = Position;
                Vector3 target = Target;

                // If we're shaking, add our offset to our position and target
                if (shaking)
                {
                    position += shakeOffset;
                    target += shakeOffset;
                }

                // Return the matrix using our modified position and target
                return Matrix.CreateLookAt(position, target, Up);
            }
        }

        /// <summary>
        /// The projection matrix for the camera.
        /// </summary>
        public Matrix Projection;

        /// <summary>
        /// Shakes the camera with a specific magnitude and duration.
        /// </summary>
        /// <param name="magnitude">The largest magnitude to apply to the shake.</param>
        /// <param name="duration">The length of time (in seconds) for which the shake should occur.</param>
        public void Shake(float magnitude, float duration)
        {
            // We're now shaking
            shaking = true;

            // Store our magnitude and duration
            shakeMagnitude = magnitude;
            shakeDuration = duration;

            // Reset our timer
            shakeTimer = 0f;
        }

        /// <summary>
        /// Updates the Camera.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            // If we're shaking...
            if (shaking)
            {
                // Move our timer ahead based on the elapsed time
                shakeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // If we're at the max duration, we're not going to be shaking anymore
                if (shakeTimer >= shakeDuration)
                {
                    shaking = false;
                    shakeTimer = shakeDuration;
                }

                // Compute our progress in a [0, 1] range
                float progress = shakeTimer / shakeDuration;

                // Compute our magnitude based on our maximum value and our progress. This causes
                // the shake to reduce in magnitude as time moves on, giving us a smooth transition
                // back to being stationary. We use progress * progress to have a non-linear fall 
                // off of our magnitude. We could switch that with just progress if we want a linear 
                // fall off.
                float magnitude = shakeMagnitude * (1f - (progress * progress));

                // Generate a new offset vector with three random values and our magnitude
                shakeOffset = new Vector3(NextFloat(), NextFloat(), NextFloat()) * magnitude;
            }
        }

        /// <summary>
        /// Helper to generate a random float in the range of [-1, 1].
        /// </summary>
        private float NextFloat()
        {
            return (float)random.NextDouble() * 2f - 1f;
        }
    }
}
