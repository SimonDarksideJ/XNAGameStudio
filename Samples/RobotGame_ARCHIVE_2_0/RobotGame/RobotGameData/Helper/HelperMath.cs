#region File Description
//-----------------------------------------------------------------------------
// HelperMath.cs
//
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

namespace RobotGameData.Helper
{
    /// <summary>
    /// Useful functions about math
    /// </summary>
    public static class HelperMath
    {
        #region Fields

        public static Random Random = new Random();
        public const float Epsilon = 1E-5f; //for numerical imprecision

        #endregion

        /// <summary>
        /// Return a random integer.
        /// </summary>
        /// <returns>Random integer</returns>
        public static int Randomi()
        {
            return Random.Next();
        }

        /// <summary>
        /// It specifies the maximum value and returns a random integer.
        /// </summary>
        /// <param name="max">Max integer</param>
        /// <returns>Random integer</returns>
        public static int Randomi(int max)
        {
            return Random.Next(max);
        }

        /// <summary>
        /// It specifies the maximum and the minimum and returns a random integer.
        /// </summary>
        /// <param name="min">Min integer</param>
        /// <param name="max">Max integer</param>
        /// <returns>Random integer</returns>
        public static int Randomi(int min, int max)
        {
            return Random.Next(min, max);
        }

        /// <summary>
        /// Return a random floating point between 0.0 to 1.0
        /// </summary>
        /// <returns>Random floating point</returns>
        public static float RandomNormal()
        {
            // randomed 0.0 to 1.0
            return (float)Random.NextDouble();
        }

        /// <summary>
        /// Return a random floating point between -1.0 to 1.0
        /// </summary>
        /// <returns>Random floating point</returns>
        public static float RandomNormal2()
        {
            // randomed -1.0 to 1.0
            return (float)Random.Next(-1, 2) * (float)Random.NextDouble();
        }

        /// <summary>
        /// Return a randomed vector3 between 0.0 to 1.0
        /// </summary>
        public static Vector3 RandomVector()
        {
            return new Vector3(RandomNormal(), RandomNormal(), RandomNormal());
        }

        /// <summary>
        /// Return a randomed vector3 between -1.0 to 1.0
        /// </summary>
        public static Vector3 RandomVector2()
        {
            return new Vector3(RandomNormal2(), RandomNormal2(), RandomNormal2());
        }

        /// <summary>
        /// It calculates the remainder of the division of value1 by value2.
        /// </summary>
        /// <param name="val1">value 1</param>
        /// <param name="val2">value 2</param>
        /// <returns></returns>
        public static float CalculateModulo(float value1, float value2)
        {
            while (value1 - value2 >= 0)
            {
                value1 -= value2;
            }

            return value1;
        }

        /// <summary>
        /// Vector value change to integer, and reduce 0.5 coordinate
        /// </summary>
        public static Vector3 Make2DCoord(Vector3 vec)
        {
            int x = (int)vec.X;
            int y = (int)vec.Y;
            int z = (int)vec.Z;

            return new Vector3((float)x - 0.5f,
                               (float)y - 0.5f,
                               (float)z - 0.5f);
        }

        /// <summary>
        /// Vector value change to integer, and reduce 0.5 coordinate
        /// </summary>
        public static Vector2 Make2DCoord(Vector2 vec)
        {
            int x = (int)vec.X;
            int y = (int)vec.Y;

            return new Vector2((float)x - 0.5f,
                               (float)y - 0.5f);
        }

    }
}
