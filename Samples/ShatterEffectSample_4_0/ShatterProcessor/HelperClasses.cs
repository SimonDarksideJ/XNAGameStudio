#region File Description
//-----------------------------------------------------------------------------
// HelperClasses.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace ShatterEffectProcessor
{
    /// <summary>
    /// Enumerates each element 3 times, once for each vertex in a triangle
    /// </summary>    
    internal class ReplicateTriangleDataToEachVertex <T> : IEnumerable<T>
    {
        private IEnumerable<T> perTriangleData;

        public ReplicateTriangleDataToEachVertex (IEnumerable<T> perTriangleData)
        {
            this.perTriangleData = perTriangleData;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T item in perTriangleData)
            {
                for (int i = 0; i < 3; i++)
                {
                    // Return the same center value for every 3 vertices.
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Enumerates a set of random vectors such that each element of those vectors
    /// are in the range [-1,1]
    /// </summary>
    internal class RandomVectorEnumerable : IEnumerable<Vector3>
    {
        private Random random = new Random();
        private int count;

        public RandomVectorEnumerable(int count)
        {
            this.count = count;
        }

        public IEnumerator<Vector3> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 vector = new Vector3((float)random.NextDouble(),
                    (float)random.NextDouble(), (float)random.NextDouble());
                vector *= 2;
                vector -= Vector3.One;

                yield return vector;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
