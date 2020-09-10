//-----------------------------------------------------------------------------
// DebugDraw.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace CollisionSample
{
    /// <summary>
    /// Debug drawing routines for common collision shapes. These are not designed to be the most
    /// efficent way to submit geometry to the graphics device as they are intended for use in
    /// visualizing collision for debugging purposes.
    /// </summary>
    public class DebugDraw : IDisposable
    {
        #region Constants

        public const int MAX_VERTS = 2000;
        public const int MAX_INDICES = 2000;

        // Indices for drawing the edges of a cube, given the vertex ordering
        // used by Bounding(Frustum|Box|OrientedBox).GetCorners()
        static ushort[] cubeIndices = new ushort[] { 0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7 };
        
        #endregion

        #region Fields

        BasicEffect basicEffect;
        DynamicVertexBuffer vertexBuffer;
        DynamicIndexBuffer indexBuffer;

        ushort[] Indices = new ushort[MAX_INDICES];
        VertexPositionColor[] Vertices = new VertexPositionColor[MAX_VERTS];
        int IndexCount;
        int VertexCount;

        #endregion

        #region Initialization

        public DebugDraw(GraphicsDevice device)
        {
            vertexBuffer = new DynamicVertexBuffer(device, typeof(VertexPositionColor), MAX_VERTS, BufferUsage.WriteOnly);
            indexBuffer = new DynamicIndexBuffer(device, typeof(ushort), MAX_INDICES, BufferUsage.WriteOnly);

            basicEffect = new BasicEffect(device); //(device, null);
            basicEffect.LightingEnabled = false;
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = false;
        }

        #endregion

        #region Dispose

        ~DebugDraw()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (vertexBuffer != null)
                    vertexBuffer.Dispose();

                if (indexBuffer != null)
                    indexBuffer.Dispose();

                if (basicEffect != null)
                    basicEffect.Dispose();
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Starts debug drawing by setting the required render states and camera information
        /// </summary>
        public void Begin(Matrix view, Matrix projection)
        {
            basicEffect.World = Matrix.Identity;
            basicEffect.View = view;
            basicEffect.Projection = projection;

            VertexCount = 0;
            IndexCount = 0;
        }

        /// <summary>
        /// Ends debug drawing and restores standard render states
        /// </summary>
        public void End()
        {
            FlushDrawing();
        }

        public void DrawWireShape(Vector3[] positionArray, ushort[] indexArray, Color color)
        {
            if (Reserve(positionArray.Length, indexArray.Length))
            {
                for (int i = 0; i < indexArray.Length; i++)
                    Indices[IndexCount++] = (ushort)(VertexCount + indexArray[i]);

                for (int i = 0; i < positionArray.Length; i++)
                    Vertices[VertexCount++] = new VertexPositionColor(positionArray[i], color);
            }
        }

        // Draw any queued objects and reset our line buffers
        private void FlushDrawing()
        {
            if (IndexCount > 0)
            {
                vertexBuffer.SetData(Vertices, 0, VertexCount, SetDataOptions.Discard);
                indexBuffer.SetData(Indices, 0, IndexCount, SetDataOptions.Discard);

                GraphicsDevice device = basicEffect.GraphicsDevice;
                device.SetVertexBuffer(vertexBuffer);
                device.Indices = indexBuffer;

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, VertexCount, 0, IndexCount / 2);
                }

                device.SetVertexBuffer(null);
                device.Indices = null;
            }
            IndexCount = 0;
            VertexCount = 0;
        }

        // Check if there's enough space to draw an object with the given vertex/index counts.
        // If necessary, call FlushDrawing() to make room.
        private bool Reserve(int numVerts, int numIndices)
        {
            if(numVerts > MAX_VERTS || numIndices > MAX_INDICES)
            {
                // Whatever it is, we can't draw it
                return false;
            }
            if (VertexCount + numVerts > MAX_VERTS || IndexCount + numIndices >= MAX_INDICES)
            {
                // We can draw it, but we need to make room first
                FlushDrawing();
            }
            return true;
        }

        /// <summary>
        /// Renders a 2D grid (must be called within a Begin/End pair)
        /// </summary>
        /// <param name="xAxis">Vector direction for the local X-axis direction of the grid</param>
        /// <param name="yAxis">Vector direction for the local Y-axis of the grid</param>
        /// <param name="origin">3D starting anchor point for the grid</param>
        /// <param name="iXDivisions">Number of divisions in the local X-axis direction</param>
        /// <param name="iYDivisions">Number of divisions in the local Y-axis direction</param>
        /// <param name="color">Color of the grid lines</param>
        public void DrawWireGrid(Vector3 xAxis, Vector3 yAxis, Vector3 origin, int iXDivisions, int iYDivisions, Color color)
        {
            Vector3 pos, step;

            pos = origin;
            step = xAxis / iXDivisions;
            for (int i = 0; i <= iXDivisions; i++)
            {
                DrawLine(pos, pos + yAxis, color);
                pos += step;
            }

            pos = origin;
            step = yAxis / iYDivisions;
            for (int i = 0; i <= iYDivisions; i++)
            {
                DrawLine(pos, pos + xAxis, color);
                pos += step;
            }
        }

        /// <summary>
        /// Renders the outline of a bounding frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum to render</param>
        /// <param name="color">Color of the frustum lines</param>
        public void DrawWireFrustum(BoundingFrustum frustum, Color color)
        {
            DrawWireShape(frustum.GetCorners(), cubeIndices, color);
        }

        /// <summary>
        /// Renders the outline of an axis-aligned bounding box
        /// </summary>
        /// <param name="box">Bounding box to render</param>
        /// <param name="color">Color of the box lines</param>
        public void DrawWireBox(BoundingBox box, Color color)
        {
            DrawWireShape(box.GetCorners(), cubeIndices, color);
        }

        /// <summary>
        /// Renders the outline of an oriented bounding box
        /// </summary>
        /// <param name="box">Oriented bounding box to render</param>
        /// <param name="color">Color of the box lines</param>
        public void DrawWireBox(BoundingOrientedBox box, Color color)
        {
            DrawWireShape(box.GetCorners(), cubeIndices, color);
        }

        /// <summary>
        /// Renders a circular ring (tessellated circle)
        /// </summary>
        /// <param name="origin">Center point for the ring</param>
        /// <param name="majorAxis">Direction of the major-axis of the circle</param>
        /// <param name="minorAxis">Direction of hte minor-axis of the circle</param>
        /// <param name="color">Color of the ring lines</param>
        public void DrawRing(Vector3 origin, Vector3 majorAxis, Vector3 minorAxis, Color color)
        {
            const int RING_SEGMENTS = 32;
            const float fAngleDelta = 2.0F * (float)Math.PI / RING_SEGMENTS;

            if (Reserve(RING_SEGMENTS, RING_SEGMENTS * 2))
            {
                for (int i = 0; i < RING_SEGMENTS; i++)
                {
                    Indices[IndexCount++] = (ushort)(VertexCount + i);
                    Indices[IndexCount++] = (ushort)(VertexCount + (i + 1) % RING_SEGMENTS);
                }
                float cosDelta = (float)Math.Cos(fAngleDelta);
                float sinDelta = (float)Math.Sin(fAngleDelta);

                float cosAcc = 1;
                float sinAcc = 0;

                for (int i = 0; i < RING_SEGMENTS; ++i)
                {
                    Vector3 pos = new Vector3(majorAxis.X * cosAcc + minorAxis.X * sinAcc + origin.X,
                                              majorAxis.Y * cosAcc + minorAxis.Y * sinAcc + origin.Y,
                                              majorAxis.Z * cosAcc + minorAxis.Z * sinAcc + origin.Z);

                    Vertices[VertexCount++] = new VertexPositionColor(pos, color);

                    float newCos = cosAcc * cosDelta - sinAcc * sinDelta;
                    float newSin = cosAcc * sinDelta + sinAcc * cosDelta;

                    cosAcc = newCos;
                    sinAcc = newSin;
                }
            }
        }

        /// <summary>
        /// Renders the outline of a bounding sphere.
        /// 
        /// This code assumes that the model and view matrices contain only rigid motion.
        /// </summary>
        /// <param name="sphere">Bounding sphere to render</param>
        /// <param name="color">Color of the outline lines</param>
        public void DrawWireSphere(BoundingSphere sphere, Color color)
        {
            // Invert the modelview matrix to get direction vectors
            // in screen space, so we can draw a circle that always
            // faces the camera.
            Matrix view = basicEffect.World * basicEffect.View;
            Matrix.Transpose(ref view, out view);
            DrawRing(sphere.Center, view.Right * sphere.Radius, view.Up * sphere.Radius, color);
        }

        /// <summary>
        /// Draw a ray of the given length
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="color"></param>
        /// <param name="length"></param>
        public void DrawRay(Ray ray, Color color, float length)
        {
            DrawLine(ray.Position, ray.Position + ray.Direction * length, color);
        }

        public void DrawLine(Vector3 v0, Vector3 v1, Color color)
        {
            if(Reserve(2, 2))
            {
                Indices[IndexCount++] = (ushort)VertexCount;
                Indices[IndexCount++] = (ushort)(VertexCount+1);
                Vertices[VertexCount++] = new VertexPositionColor(v0, color);
                Vertices[VertexCount++] = new VertexPositionColor(v1, color);
            }
        }

        public void DrawWireTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Color color)
        {
            if(Reserve(3, 6))
            {
                Indices[IndexCount++] = (ushort)(VertexCount+0);
                Indices[IndexCount++] = (ushort)(VertexCount+1);
                Indices[IndexCount++] = (ushort)(VertexCount+1);
                Indices[IndexCount++] = (ushort)(VertexCount+2);
                Indices[IndexCount++] = (ushort)(VertexCount+2);
                Indices[IndexCount++] = (ushort)(VertexCount+0);

                Vertices[VertexCount++] = new VertexPositionColor(v0, color);
                Vertices[VertexCount++] = new VertexPositionColor(v1, color);
                Vertices[VertexCount++] = new VertexPositionColor(v2, color);
            }
        }

        public void DrawWireTriangle(Triangle t, Color color)
        {
            DrawWireTriangle(t.V0, t.V1, t.V2, color);
        }

        #endregion
    }
}
