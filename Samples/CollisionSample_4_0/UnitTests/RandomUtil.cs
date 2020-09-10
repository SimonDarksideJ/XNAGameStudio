//---------------------------------------------------------------------------------------------------------------------
// UnitTests.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//---------------------------------------------------------------------------------------------------------------------
using System;
using Microsoft.Xna.Framework;


namespace UnitTests
{
    /// <summary>
    /// Utility functions for creating uniform random vectors and orientations.
    /// </summary>
    public static class RandomUtil
    {
        public static float NextFloat(this Random random, float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }

        public static Vector3 PointInCube(this Random random)
        {
            return new Vector3
            {
                X = random.NextFloat(-1, 1),
                Y = random.NextFloat(-1, 1),
                Z = random.NextFloat(-1, 1)
            };
        }

        public static Vector3 PointInSphere(this Random random)
        {
            for (; ; )
            {
                Vector3 p = random.PointInCube();
                if (p.LengthSquared() <= 1.0f)
                {
                    p.Normalize();
                    return p;
                }
            }
        }

        public static Quaternion Orientation(this Random random)
        {
            for (; ; )
            {
                // Pick a random point uniformly inside the 4-sphere, then normalize
                Quaternion q = new Quaternion
                {
                    X = random.NextFloat(-1,1),
                    Y = random.NextFloat(-1,1),
                    Z = random.NextFloat(-1,1),
                    W = random.NextFloat(-1,1)
                };
                if (q.LengthSquared() <= 1.0f)
                {
                    q.Normalize();
                    return q;
                }
            }
        }
    }
}
