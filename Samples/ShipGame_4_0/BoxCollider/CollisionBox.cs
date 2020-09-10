#region File Description
//-----------------------------------------------------------------------------
// CollisionBox.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion



namespace BoxCollider
{
    public class CollisionBox : IDisposable
    {
        // the bounding box minimum point
        public Vector3 min;
        // the bounding box maximum point
        public Vector3 max;

        const float COS45 = 0.70710678f;
        const float INVSQRT3 = 0.57735027f;

        // vertex buffer and declaration for drawing debug box
        VertexBuffer vertexBuffer;
        VertexDeclaration vertexDecl;

        // normals for each vertex
        public static Vector3[] vertexNormals = new Vector3[8]
        {
            new Vector3(-INVSQRT3,-INVSQRT3,-INVSQRT3),
            new Vector3( INVSQRT3, INVSQRT3, INVSQRT3),
            new Vector3( INVSQRT3,-INVSQRT3,-INVSQRT3),
            new Vector3(-INVSQRT3, INVSQRT3, INVSQRT3),
            new Vector3( INVSQRT3, INVSQRT3,-INVSQRT3),
            new Vector3(-INVSQRT3,-INVSQRT3, INVSQRT3),
            new Vector3(-INVSQRT3, INVSQRT3,-INVSQRT3),
            new Vector3( INVSQRT3,-INVSQRT3, INVSQRT3)
        };

        // normals for each edge
        public static Vector3[] edgeNormals = new Vector3[12]
        {
            new Vector3(-COS45, 0, -COS45),
            new Vector3(0, COS45, -COS45),
            new Vector3(COS45, 0, -COS45),
            new Vector3(0, -COS45, -COS45),
            new Vector3(0, COS45, COS45),
            new Vector3(-COS45, 0, COS45),
            new Vector3(0, -COS45, COS45),
            new Vector3( COS45, 0, COS45),
            new Vector3(-COS45,-COS45, 0),
            new Vector3(-COS45, COS45, 0),
            new Vector3( COS45, COS45, 0),
            new Vector3( COS45,-COS45, 0)
        };

        // normals for each face
        public static Vector3[] faceNormals = new Vector3[6]
        {
            new Vector3(1,0,0),
            new Vector3(0,1,0),
            new Vector3(0,0,1),
            new Vector3(-1,0,0),
            new Vector3(0,-1,0),
            new Vector3(0,0,-1)
        };

        // constructor from min/max floats
        public CollisionBox(float min, float max)
        {
            this.min = new Vector3(min, min, min);
            this.max = new Vector3(max, max, max);
        }

        // constructor from min/max vectors
        public CollisionBox(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        // constructor from another collision box
        public CollisionBox(CollisionBox bb)
        {
            min = bb.min;
            max = bb.max;
        }

        // add a point to the bounding box extending it if needed
        public void AddPoint(Vector3 p)
        {
            if (p.X >= max.X)
                max.X = p.X;
            if (p.Y >= max.Y)
                max.Y = p.Y;
            if (p.Z >= max.Z)
                max.Z = p.Z;

            if (p.X <= min.X)
                min.X = p.X;
            if (p.Y <= min.Y)
                min.Y = p.Y;
            if (p.Z <= min.Z)
                min.Z = p.Z;
        }

        // get the bounding box center point
        public Vector3 Center
        {
            get { return 0.5f * (min + max); }
        }

        // check if two bounding boxes have any intersection
        public bool BoxIntersect(CollisionBox bb)
        {
            if (max.X >= bb.min.X && min.X <= bb.max.X &&
                max.Y >= bb.min.Y && min.Y <= bb.max.Y &&
                max.Z >= bb.min.Z && min.Z <= bb.max.Z)
                return true;
            return false;
        }

        // check if a point in inside the bounding box
        public bool PointInside(Vector3 p)
        {
            return p.X > min.X && p.X <= max.X &&
                    p.Y > min.Y && p.Y <= max.Y &&
                    p.Z > min.Z && p.Z <= max.Z;
        }

        // split in middle point creating 8 children
        public CollisionBox[] GetChildren()
        {
            Vector3 center = 0.5f * (min + max);

            CollisionBox[] children = new CollisionBox[8];

            children[0] = new CollisionBox(min, center);
            children[1] = new CollisionBox(new Vector3(center.X, min.Y, min.Z), 
                                new Vector3(max.X, center.Y, center.Z));
            children[2] = new CollisionBox(new Vector3(min.X, center.Y, min.Z), 
                                new Vector3(center.X, max.Y, center.Z));
            children[3] = new CollisionBox(new Vector3(center.X, center.Y, min.Z), 
                                new Vector3(max.X, max.Y, center.Z));
            children[4] = new CollisionBox(new Vector3(min.X, min.Y, center.Z), 
                                new Vector3(center.X, center.Y, max.Z));
            children[5] = new CollisionBox(new Vector3(center.X, min.Y, center.Z), 
                                new Vector3(max.X, center.Y, max.Z));
            children[6] = new CollisionBox(new Vector3(min.X, center.Y, center.Z), 
                                new Vector3(center.X, max.Y, max.Z));
            children[7] = new CollisionBox(center, max);

            return children;
        }

        // get the 8 bounding box vertices
        public Vector3[] GetVertices()
        {
            Vector3[] vertices = new Vector3[8];

            vertices[0] = min;
            vertices[1] = max;
            vertices[2] = new Vector3(max.X, min.Y, min.Z);
            vertices[3] = new Vector3(min.X, max.Y, max.Z);
            vertices[4] = new Vector3(max.X, max.Y, min.Z);
            vertices[5] = new Vector3(min.X, min.Y, max.Z);
            vertices[6] = new Vector3(min.X, max.Y, min.Z);
            vertices[7] = new Vector3(max.X, min.Y, max.Z);

            return vertices;
        }

        // get the 12 edges (each edge in list made of two 3D points)
        public Vector3[] GetEdges()
        {
            Vector3[] vertices = GetVertices();

            Vector3[] edges = new Vector3[24];

            edges[0] = vertices[0]; edges[1] = vertices[6];
            edges[2] = vertices[6]; edges[3] = vertices[4];
            edges[4] = vertices[4]; edges[5] = vertices[2];
            edges[6] = vertices[2]; edges[7] = vertices[0];
            edges[8] = vertices[1]; edges[9] = vertices[3];
            edges[10] = vertices[3]; edges[11] = vertices[5];
            edges[12] = vertices[5]; edges[13] = vertices[7];
            edges[14] = vertices[7]; edges[15] = vertices[1];
            edges[16] = vertices[0]; edges[17] = vertices[5];
            edges[18] = vertices[3]; edges[19] = vertices[6];
            edges[20] = vertices[4]; edges[21] = vertices[1];
            edges[22] = vertices[7]; edges[23] = vertices[2];

            return edges;
        }

        // collide ray defined by ray origin (ro) and ray direction (rd) with the box. 
        // returns -1 on no collision and the face index (0 to 5) if collision is found 
        // together with the distances to the collision points
        public int RayIntersect(Vector3 rayOrigin, Vector3 rayDirection, 
            out float tnear, out float tfar)
        {
            float t1, t2, t;

            tnear = -float.MaxValue;
            tfar = float.MaxValue;

            int face, i = -1, j = -1;

            // intersect in X
            if (rayDirection.X > -0.00001f && rayDirection.X < -0.00001f)
            {
                if (rayOrigin.X < min.X || rayOrigin.X > max.X)
                    return -1;
            }
            else
            {
                t = 1.0f / rayDirection.X;
                t1 = (min.X - rayOrigin.X) * t;
                t2 = (max.X - rayOrigin.X) * t;

                if (t1 > t2)
                {
                    t = t1; t1 = t2; t2 = t;
                    face = 0;
                }
                else
                    face = 3;

                if (t1 > tnear)
                {
                    tnear = t1;
                    i = face;
                }
                if (t2 < tfar)
                {
                    tfar = t2;
                    if (face > 2)
                        j = face - 3;
                    else
                        j = face + 3;
                }

                if (tnear > tfar || tfar < 0.00001f)
                    return -1;
            }

            // intersect in Y
            if (rayDirection.Y > -0.00001f && rayDirection.Y < -0.00001f)
            {
                if (rayOrigin.Y < min.Y || rayOrigin.Y > max.Y)
                    return -1;
            }
            else
            {
                t = 1.0f / rayDirection.Y;
                t1 = (min.Y - rayOrigin.Y) * t;
                t2 = (max.Y - rayOrigin.Y) * t;

                if (t1 > t2)
                {
                    t = t1; t1 = t2; t2 = t;
                    face = 1;
                }
                else
                    face = 4;

                if (t1 > tnear)
                {
                    tnear = t1;
                    i = face;
                }
                if (t2 < tfar)
                {
                    tfar = t2;
                    if (face > 2)
                        j = face - 3;
                    else
                        j = face + 3;
                }

                if (tnear > tfar || tfar < 0.00001f)
                    return -1;
            }

            // intersect in Z
            if (rayDirection.Z > -0.00001f && rayDirection.Z < -0.00001f)
            {
                if (rayOrigin.Z < min.Z || rayOrigin.Z > max.Z)
                    return -1;
            }
            else
            {
                t = 1.0f / rayDirection.Z;
                t1 = (min.Z - rayOrigin.Z) * t;
                t2 = (max.Z - rayOrigin.Z) * t;

                if (t1 > t2)
                {
                    t = t1; t1 = t2; t2 = t;
                    face = 2;
                }
                else
                    face = 5;

                if (t1 > tnear)
                {
                    tnear = t1;
                    i = face;
                }
                if (t2 < tfar)
                {
                    tfar = t2;
                    if (face > 2)
                        j = face - 3;
                    else
                        j = face + 3;
                }
            }

            if (tnear > tfar || tfar < 0.00001f)
                return -1;

            if (tnear < 0.0f)
                return j;
            else
                return i;
        }

        // render the bounding box as wireframe
        public void Draw(GraphicsDevice gd)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            Vector3[] edges = GetEdges();
            if (vertexBuffer == null)
            {
                vertexBuffer = new VertexBuffer(gd, 
                    typeof(VertexPositionColor), 24, BufferUsage.WriteOnly);
            }
            if (vertexDecl == null)
            {
                vertexDecl = new VertexDeclaration(
                    VertexPositionColor.VertexDeclaration.GetVertexElements());
            }

            VertexPositionColor[] verts = new VertexPositionColor[24];
            for (int i = 0; i < 24; i += 2)
            {
                verts[i].Position = edges[i];
                verts[i].Color = Color.Red;
                verts[i + 1].Position = edges[i + 1];
                verts[i + 1].Color = Color.Red;
            }
            vertexBuffer.SetData<VertexPositionColor>(verts);

            gd.RasterizerState.DepthBias = -0.1f;

            gd.SetVertexBuffer(vertexBuffer);
            gd.DrawPrimitives(PrimitiveType.LineList, 0, 12);

            gd.RasterizerState.DepthBias = 0.0f;
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
                if (vertexDecl != null)
                {
                    vertexDecl.Dispose();
                    vertexDecl = null;
                }
                if (vertexBuffer != null)
                {
                    vertexBuffer.Dispose();
                    vertexBuffer = null;
                }
            }
        }

        #endregion
    }
}
