#region File Description
//-----------------------------------------------------------------------------
// SampleGrid.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace VertexLightingSample
{
    public class SampleGrid : IDisposable
    {
        #region Fields
        private int gridSize;
        private float gridScale;
        private Color gridColor;
        private bool isDisposed;

        // Rendering
        private VertexBuffer vertexBuffer;
        private int vertexCount;
        private int primitiveCount;
        private BasicEffect effect;
        private Matrix projection, view, world;
        private GraphicsDevice device;
        #endregion

        #region Public Properties
        public Color GridColor
        {
            get { return gridColor; }
            set { gridColor = value; }
        }
        public int GridSize
        {
            get { return gridSize; }
            set { gridSize = value; }
        }
        public float GridScale
        {
            get { return gridScale; }
            set { gridScale = value; }
        }
        public Matrix ProjectionMatrix
        {
            get { return projection; }
            set { projection = value; }
        }
        public Matrix WorldMatrix
        {
            get { return world; }
            set { world = value; }
        }
        public Matrix ViewMatrix
        {
            get { return view; }
            set { view = value; }
        }
        #endregion

        #region Constructors and Loading
        public SampleGrid()
        {
            gridSize = 16;
            gridScale = 32f;
            gridColor = new Color(0xFF, 0xFF, 0xFF, 0xFF);
            world = Matrix.Identity;
            view = Matrix.Identity;
            projection = Matrix.Identity;
        }
        public void UnloadGraphicsContent()
        {
            if (this.vertexBuffer != null)
            {
                vertexBuffer.Dispose();
                vertexBuffer = null;
            }
            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }
        }
        public void LoadGraphicsContent(GraphicsDevice graphicsDevice)
        {
            device = graphicsDevice;

            effect = new BasicEffect(device);
            int gridSize1 = this.gridSize + 1;
            this.primitiveCount = gridSize1 * 2;
            this.vertexCount = this.primitiveCount * 2;

            VertexPositionColor[] vertices = new VertexPositionColor[this.vertexCount];

            float length = (float)gridSize * gridScale;
            float halfLength = length * 0.5f;

            int index = 0;

            for (int i = 0; i < gridSize1; ++i)
            {
                vertices[index++] = new VertexPositionColor(new Vector3(
                    -halfLength, 0.0f, i * this.gridScale - halfLength), this.gridColor);
                vertices[index++] = new VertexPositionColor(new Vector3(
                    halfLength, 0.0f, i * this.gridScale - halfLength), this.gridColor);
                vertices[index++] = new VertexPositionColor(new Vector3(
                    i * this.gridScale - halfLength, 0.0f, -halfLength), this.gridColor);
                vertices[index++] = new VertexPositionColor(new Vector3(
                    i * this.gridScale - halfLength, 0.0f, halfLength), this.gridColor);
            }

            this.vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor),
                                                 this.vertexCount,
                                                 BufferUsage.WriteOnly);
            this.vertexBuffer.SetData<VertexPositionColor>(vertices);
        }
        ~SampleGrid()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    //if we're manually disposing,
                    //then managed content should be unloaded
                    UnloadGraphicsContent();
                }
                isDisposed = true;
            }
        }

        #endregion

        #region Drawing
        public void Draw()
        {
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;
            effect.VertexColorEnabled = true;
            effect.LightingEnabled = false;

            device.SetVertexBuffer(this.vertexBuffer);

            for (int i = 0; i < this.effect.CurrentTechnique.Passes.Count; ++i)
            {
                this.effect.CurrentTechnique.Passes[i].Apply();
                device.DrawPrimitives(PrimitiveType.LineList, 0, this.primitiveCount);
            }
        }
        #endregion
    }
}
