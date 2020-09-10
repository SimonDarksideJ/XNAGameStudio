#region File Description
//-----------------------------------------------------------------------------
// KeyFrameTable.cs
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

namespace RobotGameData.ParticleSystem
{
    /// <summary>
    /// this structure has a key value which varies along time.
    /// </summary>
    [Serializable]
    public class KeyFrameTable
    {
        #region Enum Interpolation
        public enum Interpolation
        {
            /// <summary>
            /// no interpolate
            /// </summary>
            None,
            
            /// <summary>
            /// interpolate using Lerp
            /// </summary>
            Lerp,

            /// <summary>
            /// interpolate using Slerp
            /// </summary>
            Slerp,

            /// <summary>
            /// interpolate using Spline
            /// </summary>
            Spline,
        };
        #endregion

        #region Fields

        public int Count = 0;
        public bool IsFixedInterval = true;
        public List<float> Table = null;
        public List<float> Time = null;

        // Volatile Members
        float step = 0.0f;

        #endregion

        /// <summary>
        /// Initialize members
        /// </summary>
        public void Initialize()
        {
            if (Count == 0)
                step = 0.0f;
            else
                step = 1.0f / (float) Count;
        }

        /// <summary>
        /// gets a key value of defined time using interpolation method.
        /// </summary>
        /// <param name="t">defined time</param>
        /// <param name="mode">interpolation method</param>
        /// <returns>key value</returns>
        public float GetKeyFrame(float t, Interpolation mode)
        {
            int idx = -1;
            float s = 0.0f;

            if (Time != null && Time.Count > 0)
                idx = GetTimeIndex(t);
            else
                idx = GetIndex(t);

            if (idx >= Count)
                idx = Count - 1;

            switch (mode)
            {
                case Interpolation.Lerp:
                    {
                        if (idx < (Count - 1))
                        {
                            if( Time != null && Time.Count > 0)
                                s = (t - Time[idx]) / (Time[idx + 1] - Time[idx]);
                            else
                                s = (t - ((float)idx * step)) * (float)Count;

                            return Table[idx] + ((Table[idx + 1] - Table[idx]) * s);
                        }

                        return Table[idx];
                    }
                default:
                    {
                        return Table[idx];
                    }
            }
        }

        /// <summary>
        /// gets a key index of defined time.
        /// </summary>
        /// <param name="t">defined time</param>
        /// <returns>key index</returns>
        protected int GetIndex(float t)
        {
            return (int)(t * Count);
        }

        /// <summary>
        /// gets a time index of defined time.
        /// </summary>
        /// <param name="t">defined time</param>
        /// <returns>time index</returns>
        protected int GetTimeIndex(float t)
        {
            int si = 0;
            int ei = Count - 1;
            int mi = 0;

            if (t >= Time[ei])
            {
                return ei;
            }

            do
            {
                mi = (si + ei) / 2;

                if ((ei - si) <= 1)
                {
                    break;
                }
                else if (Time[mi] < t)
                {
                    si = mi;
                }
                else if (Time[mi] > t)
                {
                    ei = mi;
                }
                else
                {
                    si = mi;
                    break;
                }
            } while ((ei - si) > 1);

            return si;
        }
    }
}
