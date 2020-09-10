#region File Description
//-----------------------------------------------------------------------------
// CpuSkinningHelpers.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace CpuSkinningDataTypes
{
    public static class CpuSkinningHelpers
    {
        /// <summary>
        /// Skins an individual vertex.
        /// </summary>
        public static void SkinVertex(
            Matrix[] bones,
            ref Vector3 position,
            ref Vector3 normal,
            ref Vector4 blendIndices,
            ref Vector4 blendWeights,
            out Vector3 outPosition,
            out Vector3 outNormal)
        {
            int b0 = (int)blendIndices.X;
            int b1 = (int)blendIndices.Y;
            int b2 = (int)blendIndices.Z;
            int b3 = (int)blendIndices.W;

            Matrix skinnedTransformSum;
            Blend4x3Matrix(ref bones[b0], ref bones[b1], ref bones[b2], ref bones[b3], ref blendWeights, out skinnedTransformSum);

            // Support the 4 Bone Influences - Position then Normal
            Vector3.Transform(ref position, ref skinnedTransformSum, out outPosition);
            Vector3.TransformNormal(ref normal, ref skinnedTransformSum, out outNormal);
        }

        /// <summary>
        /// This method blends the 4 input matrices using the 4 weights provided
        /// </summary>
        /// <param name="m1">1st input matrix for blending.</param>
        /// <param name="m2">2nd input matrix for blending.</param>
        /// <param name="m3">3rd input matrix for blending.</param>
        /// <param name="m4">4th input matrix for blending.</param>
        /// <param name="weights">4 Blend weights encoded in a Vector4 type.</param>
        /// <param name="blended">Output Blended result matrix.</param>
        private static void Blend4x3Matrix(ref Matrix m1, ref Matrix m2, ref Matrix m3, ref Matrix m4, ref Vector4 weights, out Matrix blended)
        {
            float w1 = weights.X;
            float w2 = weights.Y;
            float w3 = weights.Z;
            float w4 = weights.W;

            float num11 = (m1.M11 * w1) + (m2.M11 * w2) + (m3.M11 * w3) + (m4.M11 * w4);
            float num12 = (m1.M12 * w1) + (m2.M12 * w2) + (m3.M12 * w3) + (m4.M12 * w4);
            float num13 = (m1.M13 * w1) + (m2.M13 * w2) + (m3.M13 * w3) + (m4.M13 * w4);
            float num21 = (m1.M21 * w1) + (m2.M21 * w2) + (m3.M21 * w3) + (m4.M21 * w4);
            float num22 = (m1.M22 * w1) + (m2.M22 * w2) + (m3.M22 * w3) + (m4.M22 * w4);
            float num23 = (m1.M23 * w1) + (m2.M23 * w2) + (m3.M23 * w3) + (m4.M23 * w4);
            float num31 = (m1.M31 * w1) + (m2.M31 * w2) + (m3.M31 * w3) + (m4.M31 * w4);
            float num32 = (m1.M32 * w1) + (m2.M32 * w2) + (m3.M32 * w3) + (m4.M32 * w4);
            float num33 = (m1.M33 * w1) + (m2.M33 * w2) + (m3.M33 * w3) + (m4.M33 * w4);
            float num41 = (m1.M41 * w1) + (m2.M41 * w2) + (m3.M41 * w3) + (m4.M41 * w4);
            float num42 = (m1.M42 * w1) + (m2.M42 * w2) + (m3.M42 * w3) + (m4.M42 * w4);
            float num43 = (m1.M43 * w1) + (m2.M43 * w2) + (m3.M43 * w3) + (m4.M43 * w4);

            blended = new Matrix();
            blended.M11 = num11;
            blended.M12 = num12;
            blended.M13 = num13;
            blended.M14 = 0.0f;
            blended.M21 = num21;
            blended.M22 = num22;
            blended.M23 = num23;
            blended.M24 = 0.0f;
            blended.M31 = num31;
            blended.M32 = num32;
            blended.M33 = num33;
            blended.M34 = 0.0f;
            blended.M41 = num41;
            blended.M42 = num42;
            blended.M43 = num43;
            blended.M44 = 1.0f;
        }
    }
}
