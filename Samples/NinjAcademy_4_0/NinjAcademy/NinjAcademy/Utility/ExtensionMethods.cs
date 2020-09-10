#region File Description
//-----------------------------------------------------------------------------
// ExtensionMethods.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace NinjAcademy
{
    /// <summary>
    /// A class containing extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns a vector pointing to the specified point.
        /// </summary>
        /// <param name="point">The point for which to produce the vector.</param>
        /// <returns>A vector pointing to the specified point.</returns>
        public static Vector2 GetVector(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        /// <summary>
        /// Returns the height of a bounding box.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns>The height of the bounding box.</returns>
        public static float Height(this BoundingBox box)
        {
            return box.Max.Y - box.Min.Y;
        }

        /// <summary>
        /// Returns the width of a bounding box.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns>The width of the bounding box.</returns>
        public static float Width(this BoundingBox box)
        {
            return box.Max.X - box.Min.X;
        }
    }
}
