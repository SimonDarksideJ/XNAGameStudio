//---------------------------------------------------------------------------------------------------------------------
// TriangleTest.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//---------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.Xna.Framework;

namespace CollisionSample
{
    /// <summary>
    /// Represents a simple triangle by the vertices at each corner.
    /// </summary>
    public struct Triangle
    {
        public Vector3 V0;
        public Vector3 V1;
        public Vector3 V2;

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }
    }

    /// <summary>
    /// Triangle-based collision tests
    /// </summary>
    public static class TriangleTest
    {
        const float EPSILON = 1e-20F;

        #region Triangle-BoundingBox

        /// <summary>
        /// Returns true if the given box intersects the triangle (v0,v1,v2).
        /// </summary>
        public static bool Intersects(ref BoundingBox box, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            Vector3 boxCenter = (box.Max + box.Min) * 0.5f;
            Vector3 boxHalfExtent = (box.Max - box.Min) * 0.5f;

            // Transform the triangle into the local space with the box center at the origin
            Triangle localTri = new Triangle();
            Vector3.Subtract(ref v0, ref boxCenter, out localTri.V0);
            Vector3.Subtract(ref v1, ref boxCenter, out localTri.V1);
            Vector3.Subtract(ref v2, ref boxCenter, out localTri.V2);

            return OriginBoxContains(ref boxHalfExtent, ref localTri) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether the given box contains, intersects, or is disjoint from the triangle (v0,v1,v2).
        /// </summary>
        public static ContainmentType Contains(ref BoundingBox box, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            Vector3 boxCenter = (box.Max + box.Min) * 0.5f;
            Vector3 boxHalfExtent = (box.Max - box.Min) * 0.5f;

            // Transform the triangle into the local space with the box center at the origin
            Triangle localTri;
            Vector3.Subtract(ref v0, ref boxCenter, out localTri.V0);
            Vector3.Subtract(ref v1, ref boxCenter, out localTri.V1);
            Vector3.Subtract(ref v2, ref boxCenter, out localTri.V2);

            return OriginBoxContains(ref boxHalfExtent, ref localTri);
        }

        /// <summary>
        /// Tests whether the given box contains, intersects, or is disjoint from the given triangle.
        /// </summary>
        public static ContainmentType Contains(ref BoundingBox box, ref Triangle triangle)
        {
            return Contains(ref box, ref triangle.V0, ref triangle.V1, ref triangle.V2);
        }
        #endregion

        #region Triangle-BoundingOrientedBox

        /// <summary>
        /// Returns true if the given BoundingOrientedBox intersects the triangle (v0,v1,v2)
        /// </summary>
        public static bool Intersects(ref BoundingOrientedBox obox, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            // Transform the triangle into the local space of the box, so we can use a
            // faster axis-aligned box test.
            // Note than when transforming more than one point, using an intermediate matrix
            // is faster than doing multiple quaternion transforms directly.
            Quaternion qinv;
            Quaternion.Conjugate(ref obox.Orientation, out qinv);

            Matrix minv = Matrix.CreateFromQuaternion(qinv);
            Triangle localTri = new Triangle();
            localTri.V0 = Vector3.TransformNormal(v0 - obox.Center, minv);
            localTri.V1 = Vector3.TransformNormal(v1 - obox.Center, minv);
            localTri.V2 = Vector3.TransformNormal(v2 - obox.Center, minv);

            return OriginBoxContains(ref obox.HalfExtent, ref localTri) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether the given BoundingOrientedBox contains/intersects/is disjoint from the triangle
        /// (v0,v1,v2)
        /// </summary>
        public static ContainmentType Contains(ref BoundingOrientedBox obox, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            // Transform the triangle into the local space of the box, so we can use a
            // faster axis-aligned box test.
            // Note than when transforming more than one point, using an intermediate matrix
            // is faster than doing multiple quaternion transforms directly.
            Quaternion qinv;
            Quaternion.Conjugate(ref obox.Orientation, out qinv);

            Matrix minv;
            Matrix.CreateFromQuaternion(ref qinv, out minv);

            Triangle localTri = new Triangle();
            localTri.V0 = Vector3.TransformNormal(v0 - obox.Center, minv);
            localTri.V1 = Vector3.TransformNormal(v1 - obox.Center, minv);
            localTri.V2 = Vector3.TransformNormal(v2 - obox.Center, minv);

            return OriginBoxContains(ref obox.HalfExtent, ref localTri);
        }

        /// <summary>
        /// Determines whether the given BoundingOrientedBox contains/intersects/is disjoint from the
        /// given triangle.
        /// </summary>
        public static ContainmentType Contains(ref BoundingOrientedBox obox, ref Triangle triangle)
        {
            return Contains(ref obox, ref triangle.V0, ref triangle.V1, ref triangle.V2);
        }
        #endregion

        #region Triangle-Sphere

        /// <summary>
        /// Returns true if the given sphere intersects the triangle (v0,v1,v2).
        /// </summary>
        public static bool Intersects(ref BoundingSphere sphere, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            Vector3 p = NearestPointOnTriangle(ref sphere.Center, ref v0, ref v1, ref v2);
            return Vector3.DistanceSquared(sphere.Center, p) < sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Returns true if the given sphere intersects the given triangle.
        /// </summary>
        public static bool Intersects(ref BoundingSphere sphere, ref Triangle t)
        {
            Vector3 p = NearestPointOnTriangle(ref sphere.Center, ref t.V0, ref t.V1, ref t.V2);
            return Vector3.DistanceSquared(sphere.Center, p) < sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Determines whether the given sphere contains/intersects/is disjoint from the triangle
        /// (v0,v1,v2)
        /// </summary>
        public static ContainmentType Contains(ref BoundingSphere sphere, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            float r2 = sphere.Radius * sphere.Radius;
            if (Vector3.DistanceSquared(v0, sphere.Center) <= r2 &&
                Vector3.DistanceSquared(v1, sphere.Center) <= r2 &&
                Vector3.DistanceSquared(v2, sphere.Center) <= r2)
                return ContainmentType.Contains;

            return Intersects(ref sphere, ref v0, ref v1, ref v2)
                   ? ContainmentType.Intersects : ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether the given sphere contains/intersects/is disjoint from the
        /// given triangle.
        /// </summary>
        public static ContainmentType Contains(ref BoundingSphere sphere, ref Triangle triangle)
        {
            return Contains(ref sphere, ref triangle.V0, ref triangle.V1, ref triangle.V2);
        }
        #endregion

        #region Triangle-Frustum

        /// <summary>
        /// Returns true if the given frustum intersects the triangle (v0,v1,v2).
        /// </summary>
        public static bool Intersects(BoundingFrustum frustum, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            // A BoundingFrustum is defined by a matrix that projects the frustum shape
            // into the box from (-1,-1,0) to (1,1,1). We will project the triangle
            // through this matrix, and then do a simpler box-triangle test.
            Matrix m = frustum.Matrix;
            Triangle localTri;
            GeomUtil.PerspectiveTransform(ref v0, ref m, out localTri.V0);
            GeomUtil.PerspectiveTransform(ref v1, ref m, out localTri.V1);
            GeomUtil.PerspectiveTransform(ref v2, ref m, out localTri.V2);

            BoundingBox box;
            box.Min = new Vector3(-1, -1, 0);
            box.Max = new Vector3(1, 1, 1);

            return Intersects(ref box, ref localTri.V0, ref localTri.V1, ref localTri.V2);
        }

        /// <summary>
        /// Determines whether the given frustum contains/intersects/is disjoint from the triangle
        /// (v0,v1,v2)
        /// </summary>
        public static ContainmentType Contains(BoundingFrustum frustum, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            // A BoundingFrustum is defined by a matrix that projects the frustum shape
            // into the box from (-1,-1,0) to (1,1,1). We will project the triangle
            // through this matrix, and then do a simpler box-triangle test.
            Matrix m = frustum.Matrix;
            Triangle localTri;
            GeomUtil.PerspectiveTransform(ref v0, ref m, out localTri.V0);
            GeomUtil.PerspectiveTransform(ref v1, ref m, out localTri.V1);
            GeomUtil.PerspectiveTransform(ref v2, ref m, out localTri.V2);

            // Center the projected box at the origin
            Vector3 halfExtent = new Vector3(1, 1, 0.5f);
            localTri.V0.Z -= 0.5f;
            localTri.V1.Z -= 0.5f;
            localTri.V2.Z -= 0.5f;
            return OriginBoxContains(ref halfExtent, ref localTri);
        }

        /// <summary>
        /// Determines whether the given frustum contains/intersects/is disjoint from the
        /// given triangle.
        /// </summary>
        public static ContainmentType Contains(BoundingFrustum frustum, ref Triangle triangle)
        {
            return Contains(frustum, ref triangle.V0, ref triangle.V1, ref triangle.V2);
        }
        #endregion

        #region Triangle-Plane

        /// <summary>
        /// Classify the triangle (v0,v1,v2) with respect to the given plane.
        /// </summary>
        public static PlaneIntersectionType Intersects(ref Plane plane, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            float dV0 = plane.DotCoordinate(v0);
            float dV1 = plane.DotCoordinate(v1);
            float dV2 = plane.DotCoordinate(v2);

            if (Math.Min(dV0, Math.Min(dV1, dV2)) >= 0)
            {
                return PlaneIntersectionType.Front;
            }
            if (Math.Max(dV0, Math.Max(dV1, dV2)) <= 0)
            {
                return PlaneIntersectionType.Back;
            }
            return PlaneIntersectionType.Intersecting;
        }
        #endregion

        #region Triangle-Ray

        /// <summary>
        /// Determine whether the triangle (v0,v1,v2) intersects the given ray. If there is intersection,
        /// returns the parametric value of the intersection point on the ray. Otherwise returns null.
        /// </summary>
        public static float? Intersects(ref Ray ray, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            // The algorithm is based on Moller, Tomas and Trumbore, "Fast, Minimum Storage 
            // Ray-Triangle Intersection", Journal of Graphics Tools, vol. 2, no. 1, 
            // pp 21-28, 1997.

            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;

            Vector3 p = Vector3.Cross(ray.Direction, e2);

            float det = Vector3.Dot(e1, p);

            float t;
            if (det >= EPSILON)
            {
                // Determinate is positive (front side of the triangle).
                Vector3 s = ray.Position - v0;
                float u = Vector3.Dot(s, p);
                if (u < 0 || u > det)
                    return null;

                Vector3 q = Vector3.Cross(s, e1);
                float v = Vector3.Dot(ray.Direction, q);
                if (v < 0 || ((u + v) > det))
                    return null;

                t = Vector3.Dot(e2, q);
                if (t < 0)
                    return null;
            }
            else if (det <= -EPSILON)
            {
                // Determinate is negative (back side of the triangle).
                Vector3 s = ray.Position - v0;
                float u = Vector3.Dot(s, p);
                if (u > 0 || u < det)
                    return null;

                Vector3 q = Vector3.Cross(s, e1);
                float v = Vector3.Dot(ray.Direction, q);
                if (v > 0 || ((u + v) < det))
                    return null;

                t = Vector3.Dot(e2, q);
                if (t > 0)
                    return null;
            }
            else
            {
                // Parallel ray.
                return null;
            }

            return t / det;
        }

        /// <summary>
        /// Determine whether the given triangle intersects the given ray. If there is intersection,
        /// returns the parametric value of the intersection point on the ray. Otherwise returns null.
        /// </summary>
        public static float? Intersects(ref Ray ray, ref Triangle tri)
        {
            return Intersects(ref ray, ref tri.V0, ref tri.V1, ref tri.V2);
        }

        #endregion

        #region Common utility methods

        /// <summary>
        /// Return the point on triangle (v0,v1,v2) closest to point p.
        /// </summary>
        public static Vector3 NearestPointOnTriangle(ref Vector3 p, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            // We'll work in a space where v0 is the origin.
            // Let D=p-v0 be the local position of p, E1=v1-v0 and E2=v2-v0 be the
            // local positions of v1 and v2.
            //
            // Points on the triangle are defined by
            //      P=v0 + s*E1 + t*E2 
            //      for s >= 0, t >= 0, s+t <= 1
            //
            // To compute (s,t) for p, note that s=the ratio of the components of d and e1 which
            // are perpendicular to e2 in the plane of the triangle.
            //
            // s = project(perp(D,E2),E1) / project(perp(E1,E2),E1)
            // where project(A,B) = B*(A . B)/(B . B)
            //       perp(A,B) = A - project(A,B)
            //
            // expanding and rearranging terms a bit gives:
            //
            //     (D . E1)*(E2 . E2) - (D . E2)*(E1 . E2)
            // s = ---------------------------------------
            //     (E1 . E1)*(E2 . E2) - (E1 . E2)^2
            //
            // t = [same thing with E1/E2 swapped]
            //
            // Note that the denominator is the same for s and t, so we only need to compute it
            // once, and that the denominator is never negative. So we can compute the numerator
            // and denominator separately, and only do the division in case we actually need to
            // produce s and/or t.
            //
            // We also need the parametric projections of p onto each edge:
            //      u1 onto E1, u2 onto E2, u12 onto the (v2-v1) edge.
            //      u1 = (D . E1)/(E1 . E1)
            //      u2 = (D . E2)/(E2 . E2)
            Vector3 D = p - v0;
            Vector3 E1 = (v1 - v0);
            Vector3 E2 = (v2 - v0);
            float dot11 = E1.LengthSquared();
            float dot12 = Vector3.Dot(E1, E2);
            float dot22 = E2.LengthSquared();
            float dot1d = Vector3.Dot(E1, D);
            float dot2d = Vector3.Dot(E2, D);
            float dotdd = D.LengthSquared();

            float s = dot1d * dot22 - dot2d * dot12;
            float t = dot2d * dot11 - dot1d * dot12;
            float d = dot11 * dot22 - dot12 * dot12;

            if (dot1d <= 0 && dot2d <= 0)
            {
                // nearest point is V0
                return v0;
            }
            if (s <= 0 && dot2d >= 0 && dot2d <= dot22)
            {
                // nearest point is on E2
                return v0 + E2 * (dot2d / dot22);
            }
            if (t <= 0 && dot1d >= 0 && dot1d <= dot11)
            {
                // nearest point is on E1
                return v0 + E1 * (dot1d / dot11);
            }
            if (s >= 0 && t >= 0 && s + t <= d)
            {
                // nearest point is inside the triangle
                float dr = 1.0f / d;
                return v0 + (s * dr) * E1 + (t * dr) * E2;
            }

            // we need to compute u12. This is hairier than
            // u1 or u2 because we're not in a convenient
            // basis any more.
            float u12_num = dot2d - dot1d - dot12 + dot11;
            float u12_den = dot22 + dot11 - 2 * dot12;
            if (u12_num <= 0)
            {
                return v1;
            }
            if (u12_num >= u12_den)
            {
                return v2;
            }
            return v1 + (v2 - v1) * (u12_num / u12_den);
        }

        /// <summary>
        /// Check if an origin-centered, axis-aligned box with the given half extents contains,
        /// intersects, or is disjoint from the given triangle. This is used for the box and
        /// frustum vs. triangle tests.
        /// </summary>
        public static ContainmentType OriginBoxContains(ref Vector3 halfExtent, ref Triangle tri)
        {
            BoundingBox triBounds = new BoundingBox(); // 'new' to work around NetCF bug
            triBounds.Min.X = Math.Min(tri.V0.X, Math.Min(tri.V1.X, tri.V2.X));
            triBounds.Min.Y = Math.Min(tri.V0.Y, Math.Min(tri.V1.Y, tri.V2.Y));
            triBounds.Min.Z = Math.Min(tri.V0.Z, Math.Min(tri.V1.Z, tri.V2.Z));

            triBounds.Max.X = Math.Max(tri.V0.X, Math.Max(tri.V1.X, tri.V2.X));
            triBounds.Max.Y = Math.Max(tri.V0.Y, Math.Max(tri.V1.Y, tri.V2.Y));
            triBounds.Max.Z = Math.Max(tri.V0.Z, Math.Max(tri.V1.Z, tri.V2.Z));

            Vector3 triBoundhalfExtent;
            triBoundhalfExtent.X = (triBounds.Max.X - triBounds.Min.X) * 0.5f;
            triBoundhalfExtent.Y = (triBounds.Max.Y - triBounds.Min.Y) * 0.5f;
            triBoundhalfExtent.Z = (triBounds.Max.Z - triBounds.Min.Z) * 0.5f;

            Vector3 triBoundCenter;
            triBoundCenter.X = (triBounds.Max.X + triBounds.Min.X) * 0.5f;
            triBoundCenter.Y = (triBounds.Max.Y + triBounds.Min.Y) * 0.5f;
            triBoundCenter.Z = (triBounds.Max.Z + triBounds.Min.Z) * 0.5f;

            if (triBoundhalfExtent.X + halfExtent.X <= Math.Abs(triBoundCenter.X) ||
                triBoundhalfExtent.Y + halfExtent.Y <= Math.Abs(triBoundCenter.Y) ||
                triBoundhalfExtent.Z + halfExtent.Z <= Math.Abs(triBoundCenter.Z))
            {
                return ContainmentType.Disjoint;
            }

            if (triBoundhalfExtent.X + Math.Abs(triBoundCenter.X) <= halfExtent.X &&
                triBoundhalfExtent.Y + Math.Abs(triBoundCenter.Y) <= halfExtent.Y &&
                triBoundhalfExtent.Z + Math.Abs(triBoundCenter.Z) <= halfExtent.Z)
            {
                return ContainmentType.Contains;
            }

            Vector3 edge1, edge2, edge3;
            Vector3.Subtract(ref tri.V1, ref tri.V0, out edge1);
            Vector3.Subtract(ref tri.V2, ref tri.V0, out edge2);

            Vector3 normal;
            Vector3.Cross(ref edge1, ref edge2, out normal);
            float triangleDist = Vector3.Dot(tri.V0, normal);
            if(Math.Abs(normal.X*halfExtent.X) + Math.Abs(normal.Y*halfExtent.Y) + Math.Abs(normal.Z*halfExtent.Z) <= Math.Abs(triangleDist))
            {
                return ContainmentType.Disjoint;
            }

            // Worst case: we need to check all 9 possible separating planes
            // defined by Cross(box edge,triangle edge)
            // Check for separation in plane containing an axis of box A and and axis of box B
            //
            // We need to compute all 9 cross products to find them, but a lot of terms drop out
            // since we're working in A's local space. Also, since each such plane is parallel
            // to the defining axis in each box, we know those dot products will be 0 and can
            // omit them.
            Vector3.Subtract(ref tri.V1, ref tri.V2, out edge3);
            float dv0, dv1, dv2, dhalf;

            // a.X ^ b.X = (1,0,0) ^ edge1
            // axis = Vector3(0, -edge1.Z, edge1.Y);
            dv0 = tri.V0.Z * edge1.Y - tri.V0.Y * edge1.Z;
            dv1 = tri.V1.Z * edge1.Y - tri.V1.Y * edge1.Z;
            dv2 = tri.V2.Z * edge1.Y - tri.V2.Y * edge1.Z;
            dhalf = Math.Abs(halfExtent.Y * edge1.Z) + Math.Abs(halfExtent.Z * edge1.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.X ^ b.Y = (1,0,0) ^ edge2
            // axis = Vector3(0, -edge2.Z, edge2.Y);
            dv0 = tri.V0.Z * edge2.Y - tri.V0.Y * edge2.Z;
            dv1 = tri.V1.Z * edge2.Y - tri.V1.Y * edge2.Z;
            dv2 = tri.V2.Z * edge2.Y - tri.V2.Y * edge2.Z;
            dhalf = Math.Abs(halfExtent.Y * edge2.Z) + Math.Abs(halfExtent.Z * edge2.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.X ^ b.Y = (1,0,0) ^ edge3
            // axis = Vector3(0, -edge3.Z, edge3.Y);
            dv0 = tri.V0.Z * edge3.Y - tri.V0.Y * edge3.Z;
            dv1 = tri.V1.Z * edge3.Y - tri.V1.Y * edge3.Z;
            dv2 = tri.V2.Z * edge3.Y - tri.V2.Y * edge3.Z;
            dhalf = Math.Abs(halfExtent.Y * edge3.Z) + Math.Abs(halfExtent.Z * edge3.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,1,0) ^ edge1
            // axis = Vector3(edge1.Z, 0, -edge1.X);
            dv0 = tri.V0.X * edge1.Z - tri.V0.Z * edge1.X;
            dv1 = tri.V1.X * edge1.Z - tri.V1.Z * edge1.X;
            dv2 = tri.V2.X * edge1.Z - tri.V2.Z * edge1.X;
            dhalf = Math.Abs(halfExtent.X * edge1.Z) + Math.Abs(halfExtent.Z * edge1.X);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,1,0) ^ edge2
            // axis = Vector3(edge2.Z, 0, -edge2.X);
            dv0 = tri.V0.X * edge2.Z - tri.V0.Z * edge2.X;
            dv1 = tri.V1.X * edge2.Z - tri.V1.Z * edge2.X;
            dv2 = tri.V2.X * edge2.Z - tri.V2.Z * edge2.X;
            dhalf = Math.Abs(halfExtent.X * edge2.Z) + Math.Abs(halfExtent.Z * edge2.X);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,1,0) ^ bX
            // axis = Vector3(edge3.Z, 0, -edge3.X);
            dv0 = tri.V0.X * edge3.Z - tri.V0.Z * edge3.X;
            dv1 = tri.V1.X * edge3.Z - tri.V1.Z * edge3.X;
            dv2 = tri.V2.X * edge3.Z - tri.V2.Z * edge3.X;
            dhalf = Math.Abs(halfExtent.X * edge3.Z) + Math.Abs(halfExtent.Z * edge3.X);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,0,1) ^ edge1
            // axis = Vector3(-edge1.Y, edge1.X, 0);
            dv0 = tri.V0.Y * edge1.X - tri.V0.X * edge1.Y;
            dv1 = tri.V1.Y * edge1.X - tri.V1.X * edge1.Y;
            dv2 = tri.V2.Y * edge1.X - tri.V2.X * edge1.Y;
            dhalf = Math.Abs(halfExtent.Y * edge1.X) + Math.Abs(halfExtent.X * edge1.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,0,1) ^ edge2
            // axis = Vector3(-edge2.Y, edge2.X, 0);
            dv0 = tri.V0.Y * edge2.X - tri.V0.X * edge2.Y;
            dv1 = tri.V1.Y * edge2.X - tri.V1.X * edge2.Y;
            dv2 = tri.V2.Y * edge2.X - tri.V2.X * edge2.Y;
            dhalf = Math.Abs(halfExtent.Y * edge2.X) + Math.Abs(halfExtent.X * edge2.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,0,1) ^ edge3
            // axis = Vector3(-edge3.Y, edge3.X, 0);
            dv0 = tri.V0.Y * edge3.X - tri.V0.X * edge3.Y;
            dv1 = tri.V1.Y * edge3.X - tri.V1.X * edge3.Y;
            dv2 = tri.V2.Y * edge3.X - tri.V2.X * edge3.Y;
            dhalf = Math.Abs(halfExtent.Y * edge3.X) + Math.Abs(halfExtent.X * edge3.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            return ContainmentType.Intersects;
        }

        #endregion
    }
}
