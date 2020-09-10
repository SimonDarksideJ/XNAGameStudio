#region File Description
//-----------------------------------------------------------------------------
// RandomHelper.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace Pickture
{
    /// <summary>
    /// Provides a single shared Random instance and several related helper methods.
    /// </summary>
    static class RandomHelper
    {
        static Random random = new Random();
        /// <summary>
        /// The single shared Random instance.
        /// </summary>
        public static Random Random
        {
            get { return random; }
        }

        /// <summary>
        /// Returns a random float within a specified range.
        /// </summary>
        /// <param name="min">
        /// The inclusive lower bound of the random number returned.
        /// </param>
        /// <param name="max">
        /// The exclusive upper bound of the random number returned. maxValue must be
        /// greater than or equal to minValue.
        /// </param>
        public static float Next(float min, float max)
        {             
            return min + (float)(Random.NextDouble() * (max - min));
        }        

        /// <summary>
        /// Returns a random boolean value.
        /// </summary>
        public static bool NextBool()
        {
            return Random.Next(2) == 0;
        }

        /// <summary>
        /// Randomizes the order of the elements in a list.
        /// </summary>
        /// <typeparam name="T">Type of the list elements</typeparam>
        /// <param name="list">The list to be randomized</param>
        public static void Randomize<T>(IList<T> list)
        {
            // Step through the array,
            for (int i = 0; i < list.Count - 1; i++)
            {
                // swapping each element with a random element from the remainder of
                // the list. An element can be swapped with itself so that it is
                // possible for an element to remain in the same place it started.

                int randomIndex = Random.Next(i, list.Count);

                T swap = list[randomIndex];
                list[randomIndex] = list[i];
                list[i] = swap;
            }
        }
    }
}