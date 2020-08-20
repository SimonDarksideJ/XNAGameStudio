#region File Information
//-----------------------------------------------------------------------------
// TriangleSphereCollisionDetection.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using System.Collections.Generic;
#endregion Using Statements

namespace MarbleMazeGame
{
    /// <summary>
    /// Represents a simple triangle by the vertices at each corner.
    /// </summary>
    public class Triangle
    {
        #region Fields
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;
        #endregion

        #region Initialization
        public Triangle()
        {
            A = Vector3.Zero;
            B = Vector3.Zero;
            C = Vector3.Zero;
        }

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            A = v0;
            B = v1;
            C = v2;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Get a normal that faces away from the point specified (faces in)
        /// </summary>
        public void InverseNormal(ref Vector3 point, out Vector3 inverseNormal)
        {
            Normal(out inverseNormal);
            // The direction from any corner of the triangle to the point
            Vector3 inverseDirection = point - A; ;
            // Roughly facing the same way
            if (Vector3.Dot(inverseNormal, inverseDirection) > 0)
            {
                // Same direction therefore invert the normal to face away from the direction
                // to face the point
                Vector3.Multiply(ref inverseNormal, -1.0f, out inverseNormal);
            }
        }

        /// <summary>
        /// A unit length vector at right angles to the plane of the triangle
        /// </summary>
        public void Normal(out Vector3 normal)
        {
            normal = Vector3.Zero;
            Vector3 side1 = B - A;
            Vector3 side2 = C - A;
            normal = Vector3.Normalize(Vector3.Cross(side1, side2));
        }
        #endregion
    }

    /// <summary>
    /// Triangle-Sphere based collision test
    /// </summary>
    public static class TriangleSphereCollisionDetection
    {
        const float epsilon = float.Epsilon;

        #region Collision Detection
        /// <summary>
        ///  Shoot a ray into the triangle and get the distance from the point
        ///  it hits and if the distance is smaller than the radius we hit the triangle 
        /// </summary>
        /// <param name="sphere">Sphere to check collision with triangles</param>
        /// <param name="triangle">List of triangles</param>
        /// <returns></returns>
        public static bool LightSphereTriangleCollision(ref BoundingSphere sphere, ref Triangle triangle)
        {
            Ray ray = new Ray();
            ray.Position = sphere.Center;

            // Create a vector facing towards the triangle from the 
            // ray starting point.
            Vector3 inverseNormal;
            TriangleInverseNormal(ref ray.Position, out inverseNormal, ref triangle);
            ray.Direction = inverseNormal;

            // Check if the ray hits the triangle
            float? distance = RayTriangleIntersects(ref ray, ref triangle);
            if (distance != null && distance > 0 && distance <= sphere.Radius)
            {
                // Hit the surface of the triangle               
                return true;
            }

            return false;
        }

        /// <summary>
        ///  Shoot a ray into the triangle and get the distance from the point
        ///  it hits and if the distance is smaller than the radius we hit the triangle
        ///  and check collision with the edges
        /// </summary>
        /// <param name="sphere">Sphere to check collision with triangles</param>
        /// <param name="triangle">List of triangles</param>
        /// <returns></returns>
        public static bool SphereTriangleCollision(ref BoundingSphere sphere, ref Triangle triangle)
        {
            // First check if any corner point is inside the sphere
            // This is necessary because the other tests can easily miss
            // small triangles that are fully inside the sphere.
            if (sphere.Contains(triangle.A) != ContainmentType.Disjoint ||
                sphere.Contains(triangle.B) != ContainmentType.Disjoint ||
                sphere.Contains(triangle.C) != ContainmentType.Disjoint)
            {
                // A point is inside the sphere
                return true;
            }

            // Test the edges of the triangle using a ray
            // If any hit then check the distance to the hit is less than the length of the side
            // The distance from a point of a small triangle inside the sphere coule be longer
            // than the edge of the small triangle, hence the test for points inside above.
            Vector3 side = triangle.B - triangle.A;
            // Important:  The direction of the ray MUST
            // be normalised otherwise the resulting length 
            // of any intersect is wrong!
            Ray ray = new Ray(triangle.A, Vector3.Normalize(side));
            float distSq = 0;
            float? length = null;
            sphere.Intersects(ref ray, out length);
            if (length != null)
            {
                distSq = (float)length * (float)length;
                if (length > 0 && distSq < side.LengthSquared())
                {
                    // Hit edge
                    return true;
                }
            }
            // Stay at A and change the direction to C
            side = triangle.C - triangle.A;
            ray.Direction = Vector3.Normalize(side);
            length = null;
            sphere.Intersects(ref ray, out length);
            if (length != null)
            {
                distSq = (float)length * (float)length;
                if (length > 0 && distSq < side.LengthSquared())
                {
                    // Hit edge
                    return true;
                }
            }
            // Change to corner B and edge to C
            side = triangle.C - triangle.B;
            ray.Position = triangle.B;
            ray.Direction = Vector3.Normalize(side);
            length = null;
            sphere.Intersects(ref ray, out length);
            if (length != null)
            {
                distSq = (float)length * (float)length;
                if (length > 0 && distSq < side.LengthSquared())
                {
                    // Hit edge
                    return true;
                }
            }
            // If we get this far we are not touching the edges of the triangle

            // Calculate the InverseNormal of the triangle from the centre of the sphere
            // Do a ray intersection from the centre of the sphere to the triangle.
            // If the triangle is too small the ray could miss a small triangle inside
            // the sphere hence why the points were tested above.
            ray.Position = sphere.Center;
            // This will always create a vector facing towards the triangle from the 
            // ray starting point.
            TriangleInverseNormal(ref ray.Position, out side, ref triangle);
            ray.Direction = side;
            length = RayTriangleIntersects(ref ray, ref triangle);
            if (length != null && length > 0 && length < sphere.Radius)
            {
                // Hit the surface of the triangle
                return true;
            }
            // Only if we get this far have we missed the triangle
            return false;
        }

        /// <summary>
        ///  Check if sphere collide with triangles
        /// </summary>
        /// <param name="vertices">List of triangles vertices</param>
        /// <param name="boundingSphere">Sphere</param>
        /// <param name="triangle">Return which triangle collide with the sphere</param>
        /// <param name="light">Use light or full detection</param>
        /// <returns>If sphere collide with triangle</returns>
        public static bool IsSphereCollideWithTringles(List<Vector3> vertices,
            BoundingSphere boundingSphere, out Triangle triangle, bool light)
        {
            bool res = false;
            triangle = null;

            for (int i = 0; i < vertices.Count; i += 3)
            {
                // Create triangle from the tree vertices
                Triangle t = new Triangle(vertices[i], vertices[i + 1], vertices[i + 2]);

                // Check if the sphere collide with the triangle
                if (light)
                {
                    res = TriangleSphereCollisionDetection.LightSphereTriangleCollision(ref boundingSphere, ref t);
                }
                else
                {
                    res = TriangleSphereCollisionDetection.SphereTriangleCollision(ref boundingSphere, ref t);
                }

                if (res)
                {
                    triangle = t;
                    return res;
                }
            }
            return res;
        }

        /// <summary>
        /// Check if bounding box Collide with triangles by checking the bounding box
        /// contains any vertices
        /// </summary>
        /// <param name="vertices">List of triangles vertices</param>
        /// <param name="boundingBox">Box</param>
        /// <returns>If box collide with triangle</returns>
        public static bool IsBoxCollideWithTringles(List<Vector3> vertices,
            BoundingBox boundingBox, out Triangle triangle)
        {
            bool res = false;
            triangle = null;

            for (int i = 0; i < vertices.Count; i += 3)
            {
                // Create triangle from the tree vertices
                Triangle t = new Triangle(vertices[i], vertices[i + 1], vertices[i + 2]);

                // Check if the box collide with the triangle
                res = (boundingBox.Contains(t.A) != ContainmentType.Disjoint) ||
                    (boundingBox.Contains(t.B) != ContainmentType.Disjoint) ||
                    (boundingBox.Contains(t.C) != ContainmentType.Disjoint);

                if (res)
                {
                    triangle = t;
                    return res;
                }
            }
            return res;
        }

        /// <summary>
        /// Check if sphere collide with triangles
        /// </summary>
        /// <param name="vertices">List of triangles vertices</param>
        /// <param name="boundingSphere">Sphere</param>
        /// <param name="triangles">Return which triangles collide with the sphere</param>
        /// /// <param name="light">Use light or full detection</param>
        /// <returns>If sphere collide with triangle</returns>
        public static bool IsSphereCollideWithTringles(List<Vector3> vertices,
            BoundingSphere boundingSphere, out IEnumerable<Triangle> triangles, bool light)
        {
            bool res = false;
            List<Triangle> resualt = new List<Triangle>();

            for (int i = 0; i < vertices.Count; i += 3)
            {
                // Create triangle from the tree vertices
                Triangle t = new Triangle(vertices[i], vertices[i + 1], vertices[i + 2]);

                // Check if the shpere collide with the triangle
                bool tmp;
                if (light)
                {
                    tmp = TriangleSphereCollisionDetection.LightSphereTriangleCollision(ref boundingSphere, ref t);
                }
                else
                {
                    tmp = TriangleSphereCollisionDetection.SphereTriangleCollision(ref boundingSphere, ref t);
                }

                if (tmp)
                {
                    resualt.Add(t);
                    res = true;
                }
            }
            triangles = resualt;
            return res;
        }

        /// <summary>
        /// Check if bounding box Collide with triangles by checking the bounding box
        /// contains any vertices
        /// </summary>
        /// <param name="vertices">List of triangles vertices</param>
        /// <param name="boundingBox">Box</param>
        /// <param name="triangles">Return which triangles collide with the sphere</param>
        /// <returns>If box collide with triangle</returns>
        public static bool IsBoxCollideWithTringles(List<Vector3> vertices, BoundingBox boundingBox,
            out IEnumerable<Triangle> triangles)
        {
            bool res = false;
            List<Triangle> resualt = new List<Triangle>();

            for (int i = 0; i < vertices.Count; i += 3)
            {
                // Create triangle from the tree vertices
                Triangle t = new Triangle(vertices[i], vertices[i + 1], vertices[i + 2]);

                // Check if the box collide with the triangle
                if ((boundingBox.Contains(t.A) != ContainmentType.Disjoint) ||
                    (boundingBox.Contains(t.B) != ContainmentType.Disjoint) ||
                    (boundingBox.Contains(t.C) != ContainmentType.Disjoint))
                {
                    resualt.Add(t);
                    res = true;
                }
            }
            triangles = resualt;
            return res;
        }
        #endregion

        #region Triangle-Ray

        /// <summary>
        /// Determine whether the triangle (v0,v1,v2) intersects the given ray. If there is intersection,
        /// returns the parametric value of the intersection point on the ray. Otherwise returns null.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static float? RayTriangleIntersects(ref Ray ray, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            // The algorithm is based on Moller, Tomas and Trumbore, "Fast, Minimum Storage 
            // Ray-Triangle Intersection", Journal of Graphics Tools, vol. 2, no. 1, 
            // pp 21-28, 1997.

            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;

            Vector3 p = Vector3.Cross(ray.Direction, e2);

            float det = Vector3.Dot(e1, p);

            float t;
            if (det >= epsilon)
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
            else if (det <= -epsilon)
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
        /// <param name="ray"></param>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public static float? RayTriangleIntersects(ref Ray ray, ref Triangle triangle)
        {
            return RayTriangleIntersects(ref ray, ref triangle.A, ref triangle.B, ref triangle.C);
        }

        /// <summary>
        /// A unit length vector at a right angle to the plane of the triangle
        /// </summary>
        public static void TriangleNormal(out Vector3 normal, ref Triangle triangle)
        {
            normal = Vector3.Zero;

            // Get the two sides of the triangle
            Vector3 side1 = triangle.B - triangle.A;
            Vector3 side2 = triangle.C - triangle.A;

            // Then do a cross product between the two sides  
            // to get a vector perpendicular to the triangle
            normal = Vector3.Cross(side1, side2);

            // Normalize the vector so we get it at unit length
            normal = Vector3.Normalize(normal);
        }

        /// <summary>
        /// Get a normal that faces towards the triangle from the point given
        /// </summary>
        public static void TriangleInverseNormal(ref Vector3 point, out Vector3 inverseNormal, ref Triangle triangle)
        {
            // Get the normal of the triangle
            TriangleNormal(out inverseNormal, ref triangle);

            // The direction from 1 corner of the triangle to the
            // to the given point
            Vector3 inverseDirection = point - triangle.A;

            // Check if the inverseNormal and inverseDirection is pointing in the same direction
            if (Vector3.Dot(inverseNormal, inverseDirection) > 0)
            {
                // Same direction therefore invert the normal to face away from the direction
                // to face the point
                Vector3.Multiply(ref inverseNormal, -1.0f, out inverseNormal);
            }
        }

        #endregion
    }
}

