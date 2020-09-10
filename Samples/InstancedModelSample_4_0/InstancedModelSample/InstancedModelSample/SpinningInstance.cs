#region File Description
//-----------------------------------------------------------------------------
// SpinningInstance.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace InstancedModelSample
{
    /// <summary>
    /// Helper class animates a single instance that spins in a randomized
    /// spiral pattern. Large numbers of these instances are then rendered
    /// in a single batch using the InstancedModel class.
    /// </summary>
    class SpinningInstance
    {
        #region Fields

        float size;
        float spiralSpeed;
        float spinSpeed;
        Vector3 spinAxis;

        #endregion


        /// <summary>
        /// Constructor randomly chooses different
        /// movement parameters for each instance.
        /// </summary>
        public SpinningInstance()
        {
            size = RandomNumberBetween(0, 1);
            spiralSpeed = RandomNumberBetween(-1, 1);
            spinSpeed = RandomNumberBetween(-2, 2);

            // Choose a random axis for this instance to rotate around.
            spinAxis.X = RandomNumberBetween(-1, 1);
            spinAxis.Y = RandomNumberBetween(-1, 1);
            spinAxis.Z = RandomNumberBetween(-1, 1);

            if (spinAxis.LengthSquared() > 0.001f)
                spinAxis.Normalize();
            else
                spinAxis = Vector3.Up;
        }


        /// <summary>
        /// Gets a transform matrix describing the
        /// current position of this instance.
        /// </summary>
        public Matrix Transform
        {
            get { return transform; }
        }

        Matrix transform;


        /// <summary>
        /// Updates the position of the instance,
        /// moving it along a randomized spiral.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            // Combine scale and rotation transforms.
            Matrix scale, rotation;

            Matrix.CreateScale(size, out scale);
            Matrix.CreateFromAxisAngle(ref spinAxis, spinSpeed * time, out rotation);

            Matrix.Multiply(ref scale, ref rotation, out transform);

            // Compute our position along the spiral.
            float spiralTime = time * spiralSpeed;

            float spiralSize = (float)Math.Sin(spiralTime / 4) * 4;

            transform.M41 = (float)Math.Cos(spiralTime) * spiralSize;
            transform.M42 = (float)Math.Sin(spiralTime) * spiralSize;
            transform.M43 = (float)Math.Sin(spiralTime / 3) * 6;
        }


        /// <summary>
        /// Helper for picking a random number inside the specified range.
        /// </summary>
        static float RandomNumberBetween(float min, float max)
        {
            return MathHelper.Lerp(min, max, (float)random.NextDouble());
        }

        static Random random = new Random();
    }
}
