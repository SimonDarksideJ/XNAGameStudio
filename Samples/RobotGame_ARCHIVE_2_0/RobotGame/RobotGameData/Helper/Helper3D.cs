#region File Description
//-----------------------------------------------------------------------------
// Helper3D.cs
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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RobotGameData.Helper
{
    /// <summary>
    /// a triangle structure
    /// </summary>
    public struct Triangle
    {
        public Vector3 v1;
        public Vector3 v2;
        public Vector3 v3;

        public Triangle(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            v1 = point1;
            v2 = point2;
            v3 = point3;
        }

        public void Clear()
        {
            v1 = Vector3.Zero;
            v2 = Vector3.Zero;
            v3 = Vector3.Zero;
        }
    }

    /// <summary>
    /// Useful functions about 3D dimension.
    /// </summary>
    public static class Helper3D
    {
        public const float Epsilon = 1E-5f; //for numerical imprecision
        
        /// <summary>
        /// Checks whether a ray intersects a triangle
        /// </summary>
        /// <returns>closest intersection point</returns>
        public static float? RayIntersectTriangle(Ray ray, Vector3[] vertices, 
                                                Matrix transform, out Triangle triangle)
        {
            triangle.v1 = triangle.v2 = triangle.v3 = Vector3.Zero;

            // Keep track of the closest triangle we found so far,
            // so we can always return the closest one.
            float? closestIntersection = null;

            Vector3 v1 = Vector3.Zero;
            Vector3 v2 = Vector3.Zero;
            Vector3 v3 = Vector3.Zero;

            // Loop over the vertex data, 3 at a time (3 vertices = 1 triangle).
            for (int i = 0; i < vertices.Length; i += 3)
            {
                // Perform a ray to triangle intersection test.
                float? intersection;

                // Transform the three vertex positions into world space
                v1 = Vector3.Transform(vertices[i], transform);
                v2 = Vector3.Transform(vertices[i + 1], transform);
                v3 = Vector3.Transform(vertices[i + 2], transform);

                RayIntersectsTriangle(ray, v1, v2, v3, out intersection);

                // Does the ray intersect this triangle?
                if (intersection != null)
                {
                    // If so, is it closer than any other previous triangle?
                    if ((closestIntersection == null) ||
                        (intersection < closestIntersection))
                    {
                        // Store the distance to this triangle.
                        closestIntersection = intersection;

                        // Store the three vertex positions into the vertex parameters.
                        triangle.v1 = v1;
                        triangle.v2 = v2;
                        triangle.v3 = v3;
                    }
                }
            }

            return closestIntersection;
        }

        /// <summary>
        /// Checks whether a ray intersects a model. This method needs to access
        /// the model vertex data, 
        /// Returns the distance along the ray to the point of intersection, or null
        /// if there is no intersection.
        /// </summary>
        /// <returns>closest intersection point</returns> 
        public static float? RayIntersectModel(Ray ray, Model model, 
            Matrix worldTransform, out bool insideBoundingSphere, out Triangle triangle)
        {
            triangle.v1 = triangle.v2 = triangle.v3 = Vector3.Zero;

            // Look up our custom collision data from the Tag property of the model.
            Dictionary<string, object> tagData = (Dictionary<string, object>)model.Tag;

            if (tagData == null)
            {
                throw new InvalidOperationException(
                    "Model.Tag is not set correctly. Make sure your model " +
                    "was built using the custom CollideProcessor.");
            }

            // Start off with a fast bounding sphere test.
            BoundingSphere boundingSphere = (BoundingSphere)tagData["BoundingSphere"];

            if (boundingSphere.Intersects(ray) == null)
            {
                // If the ray does not intersect the bounding sphere, we cannot
                // possibly have picked this model, so there is no need to even
                // bother looking at the individual triangle data.
                insideBoundingSphere = false;

                return null;
            }
            else
            {
                // The bounding sphere test passed, so we need to do a full
                // triangle picking test.
                insideBoundingSphere = true;

                // Keep track of the closest triangle we found so far,
                // so we can always return the closest one.
                float? closestIntersection = null;

                // Loop over the vertex data, 3 at a time (3 vertices = 1 triangle).
                Vector3[] vertices = (Vector3[])tagData["Vertices"];

                for (int i = 0; i < vertices.Length; i += 3)
                {
                    // Perform a ray to triangle intersection test.
                    float? intersection;

                    RayIntersectsTriangle(ray,
                                          vertices[i],
                                          vertices[i + 1],
                                          vertices[i + 2],
                                          out intersection);

                    // Does the ray intersect this triangle?
                    if (intersection != null)
                    {
                        // If so, is it closer than any other previous triangle?
                        if ((closestIntersection == null) ||
                            (intersection < closestIntersection))
                        {
                            // Store the distance to this triangle.
                            closestIntersection = intersection;

                            // Transform the three vertex positions into world space,
                            // and store them into the output vertex parameters.
                            triangle.v1 
                                = Vector3.Transform(vertices[i], worldTransform);

                            triangle.v2 
                                = Vector3.Transform(vertices[i + 1], worldTransform);

                            triangle.v3 
                                = Vector3.Transform(vertices[i + 2], worldTransform);
                        }
                    }
                }

                return closestIntersection;
            }
        }

        /// <summary>
        /// Checks whether a ray intersects a triangle.
        /// This method is implemented using the pass-by-reference versions of the
        /// XNA math functions. Using these overloads is generally not recommended,
        /// because they make the code less readable than the normal pass-by-value
        /// versions. This method can be called very frequently in a tight inner loop,
        /// however, so in this particular case the performance benefits from passing
        /// everything by reference outweigh the loss of readability.
        /// </summary>
        public static void RayIntersectsTriangle( Ray ray,
                                                  Vector3 vertex1,
                                                  Vector3 vertex2,
                                                  Vector3 vertex3, 
                                                  out float? result)
        {
            // Compute vectors along two edges of the triangle.
            Vector3 edge1 = Vector3.Subtract(vertex2, vertex1); 
            Vector3 edge2 = Vector3.Subtract(vertex3, vertex1);

            // Compute the determinant.
            Vector3 directionCrossEdge2 = Vector3.Cross(ray.Direction, edge2);

            float determinant = Vector3.Dot(edge1, directionCrossEdge2);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                result = null;
                return;
            }

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector = Vector3.Subtract(ray.Position, vertex1);

            float triangleU = Vector3.Dot(distanceVector, directionCrossEdge2);
            
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                result = null;
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1 = Vector3.Cross(distanceVector, edge1);

            float triangleV = Vector3.Dot(ray.Direction, distanceCrossEdge1);
            
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                result = null;
                return;
            }

            // Compute the distance along the ray to the triangle.
            float rayDistance = Vector3.Dot(edge2, distanceCrossEdge1);
            rayDistance *= inverseDeterminant;

            // Is the triangle behind the ray origin?
            if (rayDistance < 0)
            {
                result = null;
                return;
            }

            result = rayDistance;
        }     

        public static Plane TransformPlane(ref Plane plane, ref Matrix matrix)
        {
            Vector3 normal = plane.Normal;
            normal.Normalize();

            Vector3 planeCenter = Vector3.Zero + plane.D * normal;

            Vector3 newCenter = Vector3.Transform(planeCenter, matrix);

            Vector3 centerToOrigin = Vector3.Zero - newCenter;

            float newDistance = Math.Abs(centerToOrigin.Length());

            Vector3 newNormal = Vector3.TransformNormal(normal, matrix);
            newNormal.Normalize();

            return new Plane(newNormal, newDistance);
        }

        /// <summary>
        /// Intersect point inside triangle.
        /// </summary>
        /// <param name="vA">vertex 1</param>
        /// <param name="vB">vertex 2</param>
        /// <param name="vC">vertex 3</param>
        /// <param name="p">point</param>
        /// <returns></returns>
        public static bool PointInsidePoly(Vector3 vectorA, Vector3 vectorB, 
                                            Vector3 vectorC, Vector3 point)
        {
            int positives = 0;
            int negatives = 0;
            Vector3[] verts = { vectorA, vectorB, vectorC };
            Plane plane = new Plane(vectorA, vectorB, vectorC);

            uint v0 = 3 - 1;
            for (uint v1 = 0; v1 < 3; v0 = v1, ++v1)
            {
                Vector3 point0 = verts[v0];
                Vector3 point1 = verts[v1];

                // Generate a normal for this edge
                Vector3 normal = Vector3.Cross(point1 - point0, plane.Normal);

                // Which side of this edge-plane is the point?

                float halfPlane = Vector3.Dot(point, normal) - 
                    Vector3.Dot(point0, normal);

                // Keep track of positives and negatives 
                //(but not zeros -- which means it's on the edge)

                if (halfPlane > Epsilon) positives++;
                else if (halfPlane < -Epsilon) negatives++;

                // Early-out

                if ((positives | negatives) != 0) return false;
            }

            // If they're ALL positive, or ALL negative, then it's inside

            if ((positives | negatives) == 0) return true;

            // Must not be inside, because some were pos and some were neg

            return false;
        }

        /// <summary>
        /// It calculates the perpendicular point of the specified point and the line.
        /// </summary>
        public static bool GetPerpendicularPoint(Vector3 vector1, Vector3 vector2, 
                                                Vector3 point, 
                                                out Vector3 perpendicular, out float t)
        {
            Vector3 vector12 = vector2 - vector1;
            Vector3 vector10 = point - vector1;
            Vector3 norm = Vector3.Normalize(vector12);

            perpendicular = Vector3.Zero;
            t = 0.0f;

            float num = Vector3.Dot(norm, vector10);
            float den = Vector3.Dot(norm, vector12);

            if (den * den < 1E-15f * num * num)
                return false;

            t = num / den;

            perpendicular = vector1 + (vector12 * t);

            if (t > 0.0f && t < 1.0f) return true;

            return false;
        }

        /// <summary>
        /// Calculates whether a line intersects a triangle
        /// </summary>
        public static bool LineIntersectSphere(Vector3 vector1, Vector3 vector2,
            Vector3 center, float radius, out Vector3 intersect, out float t)
        {
            Vector3 len = Vector3.Zero;

            if (GetPerpendicularPoint(vector1, vector2, center, out intersect, out t))
            {
                len = intersect - center;

                if (len.Length() < radius) return true;

                return false;
            }

            len = vector2 - center;
            if (len.Length() < radius)
            {
                intersect = vector2;
                t = 1.0f;

                return true;
            }

            len = vector1 - center;
            if (len.Length() < radius)
            {
                intersect = vector1;
                t = 0.0f;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates whether a sphere intersects a triangle
        /// </summary>
        public static bool SphereIntersectTriangle(Vector3 center, float radius,
                                                    Vector3 vector1, Vector3 vector2, 
                                                    Vector3 vector3, 
                                                    out Vector3 interset, out float t)
        {
            Vector3 normal = 
                Vector3.Normalize(Vector3.Cross(vector3 - vector1, vector2 - vector1));

            float dot = Math.Abs(Vector3.Dot(center - vector1, normal));

            interset = Vector3.Zero;
            t = dot;

            if (dot < radius)
            {
                Vector3 projCenter = normal * -dot;
                projCenter += center;

                //  Colliding center
                if (PointInsidePoly(vector1, vector2, vector3, center))
                {
                    interset = projCenter;
                    t = dot - radius;

                    return true;
                }

                // Colliding edge
                Vector3 intersect1 = Vector3.Zero;
                Vector3 intersect2 = Vector3.Zero;
                Vector3 intersect3 = Vector3.Zero;
                float t1 = 0.0f;
                float t2 = 0.0f;
                float t3 = 0.0f;

                bool bHit1 = LineIntersectSphere(vector1, vector2, center, radius, 
                                                out intersect1, out t1);

                bool bHit2 = LineIntersectSphere(vector1, vector3, center, radius, 
                                                out intersect2, out t2);

                bool bHit3 = LineIntersectSphere(vector2, vector3, center, radius, 
                                                out intersect3, out t3);

                if (bHit1 || bHit2 || bHit3)
                {
                    float min = (t1 < t2 ? (t1 < t3 ? t1 : t3) : (t2 < t3 ? t2 : t3));
                    if (t1 == min)
                        interset = intersect1;
                    else if (t2 == min)
                        interset = intersect2;
                    else if (t3 == min)
                        interset = intersect3;

                    Vector3 len = center - interset;
                    t = len.Length();

                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Calculates whether a ray intersects a triangle
        /// </summary>
        public static bool RayTriangleIntersect(Vector3 rayOrigin, Vector3 rayDirection,
                    Vector3 vertex0, Vector3 vertex1, Vector3 vertex2,
                    out float t, out float u, out float v)
        {
            t = 0; u = 0; v = 0;

            Vector3 edge1 = vertex1 - vertex0;
            Vector3 edge2 = vertex2 - vertex0;

            Vector3 tvec, pvec, qvec;
            float det, inv_det;

            Vector3.Cross(ref rayDirection, ref edge2, out pvec);

            det = Vector3.Dot(edge1, pvec);

            if (det > -0.00001f)
                return false;

            inv_det = 1.0f / det;

            tvec = rayOrigin - vertex0;

            u = Vector3.Dot(tvec, pvec) * inv_det;
            if (u < -0.001f || u > 1.001f)
                return false;

            qvec = Vector3.Cross(tvec, edge1);

            v = Vector3.Dot(rayDirection, qvec) * inv_det;
            if (v < -0.001f || u + v > 1.001f)
                return false;

            t = Vector3.Dot(edge2, qvec) * inv_det;

            if (t <= 0)
                return false;

            return true;
        }

        /// <summary>
        /// Calculates whether a ray intersects a triangle
        /// </summary>
        public static bool RayTriangleIntersect(Vector3 origin, Vector3 direction, 
                                            Vector3 vertex0, Vector3 vertex1, 
                                            Vector3 vertex2,
                                            out float t, out float u, out float v, 
                                            bool testCull)
        {
            // Make sure the "out" params are set.
            t = 0; u = 0; v = 0;

            // Get vectors for the two edges that share vert0
            Vector3 edge1 = vertex1 - vertex0;
            Vector3 edge2 = vertex2 - vertex0;

            Vector3 tvec, pvec, qvec;
            float det, inv_det;

            // Begin calculating determinant
            pvec = Vector3.Cross(direction, edge2);

            // If the determinant is near zero, ray lies in plane of triangle
            det = Vector3.Dot(edge1, pvec);

            if (testCull)
            {
                if (det < Helper3D.Epsilon)
                    return false;

                tvec = origin - vertex0;

                u = Vector3.Dot(tvec, pvec);
                if (u < 0.0 || u > det)
                    return false;

                qvec = Vector3.Cross(tvec, edge1);

                v = Vector3.Dot(direction, qvec);
                if (v < 0.0f || u + v > det)
                    return false;

                t = Vector3.Dot(edge2, qvec);
                inv_det = 1.0f / det;
                t *= inv_det;
                u *= inv_det;
                v *= inv_det;
            }
            else
            {
                // Account for Float rounding errors / inaccuracies.
                if (det > -Helper3D.Epsilon && det < Helper3D.Epsilon)
                    return false;

                // Get the inverse determinant
                inv_det = 1.0f / det;

                // Calculate distance from vert0 to ray origin
                tvec = origin - vertex0;

                // Calculate U parameter and test bounds
                u = Vector3.Dot(tvec, pvec) * inv_det;
                if (u < 0.0f || u > 1.0f)
                    return false;

                // Prepare for v
                qvec = Vector3.Cross(tvec, edge1);

                // Calculate V parameter and test bounds
                v = Vector3.Dot(direction, qvec) * inv_det;
                if (v < 0.0f || u + v > 1.0f)
                    return false;

                // Calculate t, ray intersects triangle.
                t = Vector3.Dot(edge2, qvec) * inv_det;
            }

            return true;
        }

        /// <summary>
        /// ray intersect face and return intersection distance, point and normal.
        /// </summary>
        public static bool PointIntersect(Vector3 rayOrigin, Vector3 rayDirection,
                            Triangle triangle, out float intersectDistance,
                            out Vector3 intersectPosition, out Vector3 intersectNormal)
        {
            intersectDistance = 0.0f;
            intersectPosition = rayOrigin;
            intersectNormal = Vector3.Zero;

            Vector3 uvt = Vector3.Zero;

            if (RayTriangleIntersect(rayOrigin, rayDirection,
                                    triangle.v1, triangle.v2, triangle.v3, 
                                    out uvt.Z, out uvt.X, out uvt.Y))
            {
                intersectDistance = uvt.Z;
                intersectPosition = (1.0f - uvt.X - uvt.Y) * triangle.v1 + uvt.X *
                                    triangle.v2 + uvt.Y * triangle.v3;
                intersectNormal = Vector3.Normalize(
                    Vector3.Cross(triangle.v3 - triangle.v1, triangle.v2 - triangle.v1));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates a world space ray starting at the camera's
        /// "eye" and pointing in the direction of 2D screen position. 
        /// Viewport.Unproject is used to accomplish this. 
        /// </summary>
        public static Ray ScreenPositionToRay(Vector2 screenPosition, 
                                            Matrix projectionMatrix, Matrix viewMatrix, 
                                            GraphicsDevice device)
        {
            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearSource = new Vector3(screenPosition, 0f);
            Vector3 farSource = new Vector3(screenPosition, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = device.Viewport.Unproject(nearSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 farPoint = device.Viewport.Unproject(farSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }

        static Vector3[] axis3x3 = new Vector3[3] 
        {
            new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
        };

        public static Matrix MakeMatrixWithUp(Vector3 up, Vector3 at)
        {
            Matrix matrix = Matrix.Identity;

            matrix.Up = Vector3.Normalize(up);

            float dot = Vector3.Dot(matrix.Up, at);
            if (Math.Abs(dot) >= 1.0f - Epsilon)
            {        
                for (int i = 0; (i < 3) && (Math.Abs(dot) >= 1.0f - Epsilon); i++)
                {
                    dot = Vector3.Dot(matrix.Up, axis3x3[i]);

                    if (Math.Abs(dot) < 1.0f - Epsilon)
                    {
                        matrix.Right = Vector3.Cross(axis3x3[i], matrix.Up);
                        matrix.Right.Normalize();
                        break;
                    }
                }
            }
            else
            {
                matrix.Right = Vector3.Cross(at, matrix.Up);
                matrix.Right.Normalize();
            }

            matrix.Forward = Vector3.Cross(matrix.Right, matrix.Up);
            matrix.Forward.Normalize();

            matrix.Translation = Vector3.Zero;

            return matrix;
        }

        public static Matrix MakeMatrixWithAt(Vector3 at, Vector3 up)
        {
            Matrix matrix = Matrix.Identity;

            matrix.Forward = Vector3.Normalize(at);

            float dot = Vector3.Dot(matrix.Forward, up);
            if (Math.Abs(dot) >= 1.0f - Epsilon)
            {
                for (int i = 0; (i < 3) && (Math.Abs(dot) >= 1.0f - Epsilon); i++)
                {
                    dot = Vector3.Dot(matrix.Forward, axis3x3[i]);

                    if (Math.Abs(dot) < 1.0f - Epsilon)
                    {
                        matrix.Right = Vector3.Cross(matrix.Forward, axis3x3[i]);
                        matrix.Right.Normalize();
                        break;
                    }
                }
            }
            else
            {
                matrix.Right = Vector3.Cross(matrix.Forward, up);
                matrix.Right.Normalize();
            }

            matrix.Up = Vector3.Cross(matrix.Forward, matrix.Right);
            matrix.Up.Normalize();

            matrix.Translation = Vector3.Zero;

            return matrix;
        }

        public static Vector3 TransformCoord(Vector3 vector, Matrix matrix)
        {
            float x = 0.0f, y = 0.0f, z = 0.0f, w = 0.0f;

            x = (matrix.M11 * vector.X) + (matrix.M21 * vector.Y) + 
                (matrix.M31 * vector.Z) + matrix.M41;

            y = (matrix.M12 * vector.X) + (matrix.M22 * vector.Y) + 
                (matrix.M32 * vector.Z) + matrix.M42;

            z = (matrix.M13 * vector.X) + (matrix.M23 * vector.Y) + 
                (matrix.M33 * vector.Z) + matrix.M43;

            w = (matrix.M14 * vector.X) + (matrix.M24 * vector.Y) + 
                (matrix.M34 * vector.Z) + matrix.M44;

            return new Vector3((x / w), (y / w), (z / w));
        }

        /// <summary>
        /// Transposes the rows and columns of a matrix.
        /// It only has the rotation value.
        /// </summary>
        public static Matrix Transpose(Matrix matrix)
        {
            Matrix temp = matrix;
            Matrix transpose = Matrix.Transpose(matrix);

            temp.Right = transpose.Right;
            temp.Up = transpose.Up;
            temp.Forward = transpose.Forward;

            return temp;
        }

        public static Matrix PreScale(Matrix matrix, Vector3 vector)
        {
            matrix.M11 = matrix.M11 * vector.X;
            matrix.M12 = matrix.M12 * vector.X;
            matrix.M13 = matrix.M13 * vector.X;
            matrix.M14 = matrix.M14 * vector.X;

            matrix.M21 = matrix.M21 * vector.Y;
            matrix.M22 = matrix.M22 * vector.Y;
            matrix.M23 = matrix.M23 * vector.Y;
            matrix.M24 = matrix.M24 * vector.Y;

            matrix.M31 = matrix.M31 * vector.Z;
            matrix.M32 = matrix.M32 * vector.Z;
            matrix.M33 = matrix.M33 * vector.Z;
            matrix.M34 = matrix.M34 * vector.Z;

            return matrix;
        }

        public static Matrix PostScale(Matrix matrix, Vector3 vector)
        {
            matrix.M11 = matrix.M11 * vector.X;
            matrix.M21 = matrix.M21 * vector.X;
            matrix.M31 = matrix.M31 * vector.X;
            matrix.M41 = matrix.M41 * vector.X;

            matrix.M12 = matrix.M12 * vector.Y;
            matrix.M22 = matrix.M22 * vector.Y;
            matrix.M32 = matrix.M32 * vector.Y;
            matrix.M42 = matrix.M42 * vector.Y;

            matrix.M13 = matrix.M13 * vector.Z;
            matrix.M23 = matrix.M23 * vector.Z;
            matrix.M33 = matrix.M33 * vector.Z;
            matrix.M43 = matrix.M43 * vector.Z;

            return matrix;
        }
    }
}
