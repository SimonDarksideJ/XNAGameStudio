//-----------------------------------------------------------------------------
// TriangleTest.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using Microsoft.Xna.Framework;


namespace CollisionSample
{
    /// <summary>
    /// Contains miscellaneous utilities augmenting the framework math library.
    /// </summary>
    public static class GeomUtil
    {
        /// <summary>
        /// Do a full perspective transform of the given vector by the given matrix,
        /// dividing out the w coordinate to return a Vector3 result.
        /// </summary>
        /// <param name="position">Vector3 of a point in space</param>
        /// <param name="matrix">4x4 matrix</param>
        /// <param name="result">Transformed vector after perspective divide</param>
        public static void PerspectiveTransform(ref Vector3 position, ref Matrix matrix, out Vector3 result)
        {
            float w = position.X * matrix.M14 + position.Y * matrix.M24 + position.Z * matrix.M34 + matrix.M44;
            float winv = 1.0f / w;

            float x = position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41;
            float y = position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42;
            float z = position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43;

            result = new Vector3();
            result.X = x * winv;
            result.Y = y * winv;
            result.Z = z * winv;
        }
    }
}
