#region File Description
//-----------------------------------------------------------------------------
// Line.cs
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

namespace NinjAcademy
{
    /// <summary>
    /// A finite line between two points.
    /// </summary>
    struct Line
    {
        #region Properties


        /// <summary>
        /// The line's starting point.
        /// </summary>
        public Vector2 Start { get; set; }

        /// <summary>
        /// The line's end point.
        /// </summary>
        public Vector2 End { get; set; }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new line.
        /// </summary>
        /// <param name="start">Line's start position.</param>
        /// <param name="end">Line's end position.</param>
        public Line (Vector2 start, Vector2 end) : this()
        {
            Start = start;
            End = end;
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Returns the intersection point with another line.
        /// </summary>
        /// <param name="otherLine">Another line to check for intersection.</param>
        /// <returns>The intersection point with the other line, or null if there is none.</returns>
        public Vector2? GetIntersection(Line otherLine)
        {
            // The code may be a little difficult to follow as it is based on mathematical formulas. The current
            // object is line A, while "otherLine" is line B.

            float uA;
            float uB;
            
            float numeratorA;
            float numeratorB;
            float denominator;

            numeratorA = (otherLine.End.X - otherLine.Start.X) * (Start.Y - otherLine.Start.Y) -
                (otherLine.End.Y - otherLine.Start.Y) * (Start.X - otherLine.Start.X);
            numeratorB = (End.X - Start.X) * (Start.Y - otherLine.Start.Y) -
                (End.Y - Start.Y) * (Start.X - otherLine.Start.X);
            denominator = (otherLine.End.Y - otherLine.Start.Y) * (End.X - Start.X) -
                (otherLine.End.X - otherLine.Start.X) * (End.Y - Start.Y);

            if (denominator == 0)
            {
                // Lines are parallel (and possibly coincide)
                return null;
            }

            uA = numeratorA / denominator;
            uB = numeratorB / denominator;

            if (uA < 0 || uA > 1 || uB < 0 || uB > 1)
            {
                // The intersection is outside one of the line segments
                return null;
            }

            // The line segments intersect so return the intersection point
            return new Vector2(Start.X + uA * (End.X - Start.X), Start.Y + uA * (End.Y - Start.Y));
        }


        #endregion
    }
}
