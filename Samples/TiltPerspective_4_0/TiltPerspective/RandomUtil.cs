//-----------------------------------------------------------------------------
// RandomUtil.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TiltPerspectiveSample
{
    /// <remarks>
    /// Contains utilities and extension methods for random number generators.
    /// </remarks>
    public static class RandomUtil
    {
        [ThreadStatic]
        private static Random threadStaticRandom;

        /// <summary>
        /// Return a new random number generator initialized with a reasonably high-quality seed,
        /// so callers can be confident they will not get identical generators from different
        /// calls.
        /// 
        /// Initializing from the time is not adequate as there are no sufficiently
        /// high-resolution timers. NewGuid() is guaranteed to be unique each time,
        /// so we initialize from a hash of a NewGuid().
        /// </summary>
        /// <returns>A new instance of System.Random.</returns>
        public static Random NewRandom()
        {
            return new Random(Guid.NewGuid().GetHashCode());
        }

        /// <summary>
        /// Returns a shared random number generator initialized with a reasonably high-quality
        /// seed which is local to the current thread.
        ///
        /// This is faster than creating a new generator whenever you need one, and safer in
        /// potentially multithreaded code than having a global RNG, as long as callers do
        /// not pass the returned generator between threads. For a good discussion of the
        /// issue see
        /// http://blogs.msdn.com/pfxteam/archive/2009/02/19/9434171.aspx
        /// </summary>
        public static Random SharedRandom
        {
            get
            {
                if (threadStaticRandom == null)
                {
                    // Init this thread's RNG with a nicely random source.
                    threadStaticRandom = new Random(Guid.NewGuid().GetHashCode());
                }
                return threadStaticRandom;
            }
        }

        /// <summary>
        /// Return a random float in the range [0,1]. Roughly the same as (float)rng.NextDouble(),
        /// but avoids double-precision math which is expensive on some platforms.
        /// </summary>
        public static float NextFloat(this Random rng)
        {
            const float scale = 1.0f / 2147483648;
            return (float)(rng.Next() & 0x7fffffff) * scale;
        }

        /// <summary>
        /// Return a random float in the range [0,max]. Roughly the same as (float)rng.NextDouble(),
        /// but avoids double-precision math which is expensive on some platforms.
        /// </summary>
        public static float NextFloat(this Random rng, float max)
        {
            return NextFloat(rng) * max;
        }

        /// <summary>
        /// Return a random float in the range [min,max]. Roughly the same as (float)rng.NextDouble(),
        /// but avoids double-precision math which is expensive on some platforms.
        /// </summary>
        public static float NextFloat(this Random rng, float min, float max)
        {
            return NextFloat(rng) * (max - min) + min;
        }
    }
}
