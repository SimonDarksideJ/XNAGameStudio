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


namespace TiltPerspectiveSample
{
    /// <summary>
    /// Class for creating and rendering textured shapes.
    /// </summary>
    public class DebugDraw : IDisposable
    {
        #region Fields

        private int VertexCount;
        private int PrimitiveCount;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        public BasicEffect BasicEffect;
        
        #endregion

        #region Initialization

        public DebugDraw(GraphicsDevice device, VertexPositionNormalTexture[] vertices, ushort[] indices)
        {
            PrimitiveCount = indices.Length / 3;
            indexBuffer = new IndexBuffer(device, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices, 0, indices.Length);

            VertexCount = vertices.Length;
            vertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices, 0, vertices.Length);

            BasicEffect = new BasicEffect(device);
        }

        static void AppendQuad(Vector3 origin, Vector3 dx, Vector3 dy, List<VertexPositionNormalTexture> vertices, List<ushort> indices)
        {
            Vector3 norm = Vector3.Cross(dx, dy);
            norm.Normalize();
            int i = vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(origin, norm, new Vector2(0, 0)));
            vertices.Add(new VertexPositionNormalTexture(origin + dx, norm, new Vector2(1, 0)));
            vertices.Add(new VertexPositionNormalTexture(origin + dy, norm, new Vector2(0, 1)));
            vertices.Add(new VertexPositionNormalTexture(origin + dx + dy, norm, new Vector2(1, 1)));

            indices.Add((ushort)(i + 0));
            indices.Add((ushort)(i + 2));
            indices.Add((ushort)(i + 1));
            indices.Add((ushort)(i + 1));
            indices.Add((ushort)(i + 2));
            indices.Add((ushort)(i + 3));
        }

        // Draw a box with faces oriented inward
        public static DebugDraw CreateBoxInterior(GraphicsDevice device, BoundingBox box)
        {
            Vector3 size = box.Max - box.Min;
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            AppendQuad(new Vector3(box.Min.X, box.Min.Y, box.Min.Z),
                       new Vector3(size.X, 0, 0), new Vector3(0, size.Y, 0),
                       vertices, indices);

            AppendQuad(new Vector3(box.Min.X, box.Min.Y, box.Min.Z),
                       new Vector3(0, size.Y, 0), new Vector3(0, 0, size.Z),
                       vertices, indices);
            
            AppendQuad(new Vector3(box.Min.X, box.Min.Y, box.Min.Z),
                       new Vector3(0, 0, size.Z), new Vector3(size.X, 0, 0),
                       vertices, indices);

            AppendQuad(new Vector3(box.Max.X, box.Max.Y, box.Min.Z),
                       new Vector3(0, -size.Y, 0), new Vector3(0, 0, size.Z),
                       vertices, indices);

            AppendQuad(new Vector3(box.Max.X, box.Max.Y, box.Min.Z),
                       new Vector3(0, 0, size.Z), new Vector3(-size.X, 0, 0),
                       vertices, indices);

            return new DebugDraw(device, vertices.ToArray(), indices.ToArray());
        }
        #endregion

        #region Dispose

        public void Dispose()
        {
            if (vertexBuffer != null)
            {
                vertexBuffer.Dispose();
                vertexBuffer = null;
            }

            if (indexBuffer != null)
            {
                indexBuffer.Dispose();
                indexBuffer = null;
            }

            if (BasicEffect != null)
            {
                BasicEffect.Dispose();
                BasicEffect = null;
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Starts debug drawing by setting the required render states and camera information
        /// </summary>
        public void Draw(ref Matrix world, ref Matrix view, ref Matrix projection, Texture2D texture)
        {
            BasicEffect.World = world;
            BasicEffect.View = view;
            BasicEffect.Projection = projection;

            BasicEffect.Texture = texture;
            BasicEffect.TextureEnabled = true;

            BasicEffect.LightingEnabled = true;
            BasicEffect.DirectionalLight0.Direction = -Vector3.UnitZ;
            BasicEffect.AmbientLightColor = new Vector3(.5f, .5f, .5f);

            BasicEffect.VertexColorEnabled = false;

            GraphicsDevice device = BasicEffect.GraphicsDevice;
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;

            foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, 0, PrimitiveCount);
            }

            device.SetVertexBuffer(null);
            device.Indices = null;
        }

        #endregion
    }
}
