#region File Description
//-----------------------------------------------------------------------------
// RandomHelper.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace RacingGame.Helpers
{
    /// <summary>
    /// Random helper
    /// </summary>
    public static class RandomHelper
    {
        #region Variables
        /// <summary>
        /// Global random generator
        /// </summary>
        public static Random globalRandomGenerator =
            GenerateNewRandomGenerator();
        #endregion

        #region Generate a new random generator
        /// <summary>
        /// Generate a new random generator with help of
        /// WindowsHelper.GetPerformanceCounter.
        /// Also used for all GetRandom methods here.
        /// </summary>
        /// <returns>Random</returns>
        public static Random GenerateNewRandomGenerator()
        {
            globalRandomGenerator =
                new Random((int)DateTime.Now.Ticks);
            //needs Interop: (int)WindowsHelper.GetPerformanceCounter());
            return globalRandomGenerator;
        }
        #endregion

        #region Get random float and byte methods
        /// <summary>
        /// Get random int
        /// </summary>
        /// <param name="max">Maximum</param>
        /// <returns>Int</returns>
        public static int GetRandomInt(int max)
        {
            return globalRandomGenerator.Next(max);
        }

        /// <summary>
        /// Get random float between min and max
        /// </summary>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        /// <returns>Float</returns>
        public static float GetRandomFloat(float min, float max)
        {
            return (float)globalRandomGenerator.NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// Get random byte between min and max
        /// </summary>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        /// <returns>Byte</returns>
        public static byte GetRandomByte(byte min, byte max)
        {
            return (byte)(globalRandomGenerator.Next(min, max));
        }

        /// <summary>
        /// Get random Vector2
        /// </summary>
        /// <param name="min">Minimum for each component</param>
        /// <param name="max">Maximum for each component</param>
        /// <returns>Vector2</returns>
        public static Vector2 GetRandomVector2(float min, float max)
        {
            return new Vector2(
                GetRandomFloat(min, max),
                GetRandomFloat(min, max));
        }

        /// <summary>
        /// Get random Vector3
        /// </summary>
        /// <param name="min">Minimum for each component</param>
        /// <param name="max">Maximum for each component</param>
        /// <returns>Vector3</returns>
        public static Vector3 GetRandomVector3(float min, float max)
        {
            return new Vector3(
                GetRandomFloat(min, max),
                GetRandomFloat(min, max),
                GetRandomFloat(min, max));
        }

        /// <summary>
        /// Get random color
        /// </summary>
        /// <returns>Color</returns>
        public static Color RandomColor
        {
            get
            {
                return new Color(new Vector3(
                    GetRandomFloat(0.25f, 1.0f),
                    GetRandomFloat(0.25f, 1.0f),
                    GetRandomFloat(0.25f, 1.0f)));
            }
        }

        /// <summary>
        /// Get random normal Vector3
        /// </summary>
        /// <returns>Vector3</returns>
        public static Vector3 RandomNormalVector3
        {
            get
            {
                Vector3 randomNormalVector = new Vector3(
                    GetRandomFloat(-1.0f, 1.0f),
                    GetRandomFloat(-1.0f, 1.0f),
                    GetRandomFloat(-1.0f, 1.0f));
                randomNormalVector.Normalize();
                return randomNormalVector;
            }
        }
        #endregion
    }
}
