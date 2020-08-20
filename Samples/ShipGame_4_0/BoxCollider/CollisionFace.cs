#region File Description
//-----------------------------------------------------------------------------
// CollisionFace.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion



namespace BoxCollider
{
    public class CollisionFace : CollisionTreeElem, IDisposable
    {
        // indices for the three face vertices
        int[] indices;

        // face constructor
        public CollisionFace(
            int offset, 
            int[] indexBuffer, 
            int vertexOffset, 
            Vector3[] vertexBuffer)
        {
            indices = new int[3];
            box = new CollisionBox(float.MaxValue, -float.MaxValue);
            for (int i = 0; i < 3; i++)
            {
                indices[i] = indexBuffer[i + offset] + vertexOffset;
                box.AddPoint(vertexBuffer[indices[i]]);
            }
        }

        public CollisionFace(
            int offset, 
            short[] indexBuffer, 
            int vertexOffset, 
            Vector3[] vertexBuffer)
        {
            indices = new int[3];
            box = new CollisionBox(float.MaxValue, -float.MaxValue);
            for (int i = 0; i < 3; i++)
            {
                indices[i] = (int)indexBuffer[i + offset] + vertexOffset;
                box.AddPoint(vertexBuffer[indices[i]]);
            }
        }
        
        // remove vector component (vector3 to vector2)
        public static Vector2 Vector3RemoveComponent(Vector3 vec, uint i)
        {
            switch (i)
            {
                case 0: return new Vector2(vec.Y, vec.Z);
                case 1: return new Vector2(vec.X, vec.Z);
                case 2: return new Vector2(vec.X, vec.Y);
                default: return Vector2.Zero;
            }
        }

        // intersect edge (p1,p2) moving in direction (dir) colliding with edge (p3,p4) 
        // return true on a collision with collision distance (dist) 
        // and intersection point (ip)
        public static bool EdgeIntersect(Vector3 p1, Vector3 p2, Vector3 dir, 
            Vector3 p3, Vector3 p4, out float dist, out Vector3 ip)
        {
            dist = 0;
            ip = Vector3.Zero;

            // edge vectors
            Vector3 v1 = p2 - p1;
            Vector3 v2 = p4 - p3;

            // build plane based on edge (p1,p2) and move direction (dir)
            Vector3 planeDir;
            float planeW;
            planeDir = Vector3.Cross(v1, dir);
            planeDir.Normalize();
            planeW = Vector3.Dot(planeDir, p1);

            // if colliding edge (p3,p4) does not cross plane return no collision
            // same as if p3 and p4 on same side of plane return 0
            float temp = (Vector3.Dot(planeDir, p3) - planeW) * 
                (Vector3.Dot(planeDir, p4) - planeW);
            if (temp > 0)
                return false;

            // if colliding edge (p3,p4) and plane are paralell return no collision
            v2.Normalize();
            temp = Vector3.Dot(planeDir, v2);
            if (temp == 0)
                return false;

            // compute intersection point of plane and colliding edge (p3,p4)
            ip = p3 + v2 * ((planeW - Vector3.Dot(planeDir, p3)) / temp);

            // get largest 2D plane projection
            planeDir.X = Math.Abs(planeDir.X);
            planeDir.Y = Math.Abs(planeDir.Y);
            planeDir.Z = Math.Abs(planeDir.Z);
            uint i;
            if (planeDir.X > planeDir.Y)
            {
                i = 0;
                if (planeDir.X < planeDir.Z)
                    i = 2;
            }
            else
            {
                i = 1;
                if (planeDir.Y < planeDir.Z)
                    i = 2;
            }

            // remove component with largest absolute value 
            Vector2 p12d = CollisionFace.Vector3RemoveComponent(p1, i);
            Vector2 v12d = CollisionFace.Vector3RemoveComponent(v1, i);
            Vector2 ip2d = CollisionFace.Vector3RemoveComponent(ip, i);
            Vector2 dir2d = CollisionFace.Vector3RemoveComponent(dir, i);

            // compute distance of intersection from line (ip,-dir) to line (p1,p2)
            dist = (v12d.X * (ip2d.Y - p12d.Y) - v12d.Y * (ip2d.X - p12d.X)) /
                   (v12d.X * dir2d.Y - v12d.Y * dir2d.X);
            if (dist < 0)
                return false;

            // compute intesection point on edge (p1,p2)
            ip -= dist * dir;

            // check if intersection point (ip) is between egde (p1,p2) vertices
            temp = Vector3.Dot(p1 - ip, p2 - ip);
            if (temp >= 0)
                return false; // no collision

            return true; // collision found!
        }

        // triangle intersect from http://www.graphics.cornell.edu/pubs/1997/MT97.pdf
        public static bool RayTriangleIntersect(
                Vector3 rayOrigin, 
                Vector3 rayDirection,
                Vector3 vert0, Vector3 vert1, Vector3 vert2,
                out float t, out float u, out float v)
        {
            t = 0; u = 0; v = 0;

            Vector3 edge1 = vert1 - vert0;
            Vector3 edge2 = vert2 - vert0;

            Vector3 tvec, pvec, qvec;
            float det, inv_det;

            pvec = Vector3.Cross(rayDirection, edge2);

            det = Vector3.Dot(edge1, pvec);

            if (det > -0.00001f)
                return false;

            inv_det = 1.0f / det;

            tvec = rayOrigin - vert0;

            u = Vector3.Dot(tvec, pvec) * inv_det;
            if (u < -0.0001f || u > 1.0001f)
                return false;

            qvec = Vector3.Cross(tvec, edge1);

            v = Vector3.Dot(rayDirection, qvec) * inv_det;
            if (v < -0.0001f || u + v > 1.0001f)
                return false;

            t = Vector3.Dot(edge2, qvec) * inv_det;

            if (t <= 0)
                return false;

            return true;
        }

        // ray intersect face and return intersection distance, point and normal
        public override bool PointIntersect(
            Vector3 rayOrigin, 
            Vector3 rayDirection, 
            Vector3[] vertices,
            out float intersectDistance, 
            out Vector3 intersectPosition, 
            out Vector3 intersectNormal)
        {
            intersectDistance = 0.0f;
            intersectPosition = rayOrigin;
            intersectNormal = Vector3.Zero;

            Vector3 v1 = vertices[indices[0]];
            Vector3 v2 = vertices[indices[1]];
            Vector3 v3 = vertices[indices[2]];

            Vector3 uvt;
            if (CollisionFace.RayTriangleIntersect(rayOrigin, rayDirection, 
                                v1, v2, v3, out uvt.Z, out uvt.X, out uvt.Y))
            {
                intersectDistance = uvt.Z;
                intersectPosition = (1.0f - uvt.X - uvt.Y) * v1 + 
                                    uvt.X * v2 + uvt.Y * v3;
                intersectNormal = Vector3.Normalize(Vector3.Cross(v3 - v1, v2 - v1));
                return true;
            }
            return false;
        }

        // box intersect face and return intersection distance, point and normal
        public override bool BoxIntersect(
            CollisionBox rayBox, 
            Vector3 rayOrigin, 
            Vector3 rayDirection, 
            Vector3[] vertices,
            out float intersectDistance, 
            out Vector3 intersectPosition, 
            out Vector3 intersectNormal)
        {
            intersectDistance = float.MaxValue;
            intersectPosition = rayOrigin;
            intersectNormal = Vector3.Zero;

            bool intersected = false;
            Vector3 p1, p2, p3, p4;
            uint i, j;

            CollisionBox worldBox = new CollisionBox(rayBox.min + rayOrigin, 
                                            rayBox.max + rayOrigin);

            Vector3[] boxVerts = worldBox.GetVertices();
            Vector3[] boxEdges = worldBox.GetEdges();

            float distance;
            Vector3 position;

            // intersect box edges to face edges
            for (i = 0; i < 12; i++)
            {
                // cull edges with normal more than 135 degree from moving direction
                float dot = Vector3.Dot(CollisionBox.edgeNormals[i], rayDirection);
                if (dot < -0.70710678)
                    continue;

                p1 = boxEdges[i * 2];
                p2 = boxEdges[i * 2 + 1];
                p4 = vertices[indices[0]];
                for (j = 0; j < indices.Length; j++)
                {
                    p3 = p4;
                    p4 = vertices[indices[(j + 1) % indices.Length]];

                    if (CollisionFace.EdgeIntersect(p1, p2, rayDirection, 
                                        p3, p4, out distance, out position))
                    {
                        if (distance < intersectDistance)
                        {
                            intersectDistance = distance;
                            intersectPosition = position;
                            intersectNormal = Vector3.Cross(p2 - p1, p3 - p4);
                            intersectNormal = Vector3.Normalize(intersectNormal);
                            if (Vector3.Dot(rayDirection, intersectNormal) > 0)
                                intersectNormal = Vector3.Negate(intersectNormal);
                            intersected = true;
                        }
                    }
                }
            }
            
            // intersect from face vertices to box
            for (i = 0; i < 3; i++)
            {
                float tnear, tfar;
                p1 = vertices[indices[i]];
                int box_face_id = worldBox.RayIntersect(p1, -rayDirection, 
                                                    out tnear, out tfar);
                if (box_face_id > -1)
                {
                    if (tnear < intersectDistance)
                    {
                        intersectDistance = tnear;
                        intersectPosition = p1;
                        intersectNormal = -CollisionBox.faceNormals[box_face_id];
                        intersected = true;
                    }
                }
            }
            
            // intersect from box vertices to face polygon
            Vector3 v1 = vertices[indices[0]];
            Vector3 v2 = vertices[indices[1]];
            Vector3 v3 = vertices[indices[2]];
            Vector3 uvt;
            for (i = 0; i < 8; i++)
            {
                // cull vertices with normal more than 135 degree from moving direction
                float dot = Vector3.Dot(CollisionBox.vertexNormals[i], rayDirection);
                if (dot < -0.70710678)
                    continue;

                if (CollisionFace.RayTriangleIntersect(boxVerts[i], rayDirection, 
                                        v1, v2, v3, out uvt.Z, out uvt.X, out uvt.Y))
                {
                    if (uvt.Z < intersectDistance)
                    {
                        intersectDistance = uvt.Z;
                        intersectPosition = (1.0f - uvt.X - uvt.Y) * 
                                        v1 + uvt.X * v2 + uvt.Y * v3;
                        intersectNormal = Vector3.Cross(v3 - v1, v2 - v1);
                        intersectNormal = Vector3.Normalize(intersectNormal);
                        intersected = true;
                    }
                }
            }
            
            return intersected;
        }

        #region IDisposable Members

        bool isDisposed = false;
        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                if (box != null)
                {
                    box.Dispose();
                    box = null;
                }
            }
        }

        #endregion
    }
}
