#region File Description
//-----------------------------------------------------------------------------
// ProceduralPrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SoccerPitch
{
    public abstract class ProceduralPrimitive<T> : IDisposable where T : struct
    {
        #region fields
        // Once all the geometry has been specified, the InitializePrimitive
        // method copies the vertex and index data into these buffers, which
        // store it on the GPU ready for efficient rendering.
        protected VertexDeclaration vertexDeclaration;
        protected VertexBuffer vertexBuffer;
        protected IndexBuffer indexBuffer;

        protected List<ushort> indices = new List<ushort>();
        protected List<T> vertices = new List<T>();
        #endregion

        #region Initialization
        /// <summary>
        /// Adds a new index to the primitive model. This should only be called
        /// during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddIndex(int index)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("index");

            indices.Add((ushort)index);
        }

        /// <summary>
        /// Adds a new vertex to the primitive model. This should only be called
        /// during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddVertex(T vertex)
        {
            vertices.Add(vertex);
        }
   
        /// <summary>
        /// Once all the geometry has been specified by calling AddVertex and AddIndex,
        /// this method copies the vertex and index data into GPU format buffers, ready
        /// for efficient rendering.
        protected void InitializePrimitive(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration)
        {
            // Create a vertex buffer, and copy our vertex data into it.
            vertexBuffer = new VertexBuffer(graphicsDevice,
                                            vertexDeclaration,
                                            vertices.Count, BufferUsage.None);
            
            vertexBuffer.SetData(vertices.ToArray());

            // Create an index buffer, and copy our index data into it.
            indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort),
                                          indices.Count, BufferUsage.None);

            indexBuffer.SetData(indices.ToArray());
        }
        #endregion

        #region Destruction
        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        public void Dispose()
        {
            if (vertexDeclaration != null)
                vertexDeclaration.Dispose();

            if (vertexBuffer != null)
                vertexBuffer.Dispose();

            if (indexBuffer != null)
                indexBuffer.Dispose();
        }
        #endregion

        /// <summary>
        /// Queries the index of the current vertex. This starts at
        /// zero, and increments every time AddVertex is called.
        /// </summary>
        protected int CurrentVertex
        {
            get { return vertices.Count; }
        }

        #region Draw
        /// <summary>
        /// Draws the primitive model, using the specified effect. Unlike the other
        /// Draw overload where you just specify the world/view/projection matrices
        /// and color, this method does not set any renderstates, so you must make
        /// sure all states are set to sensible values before you call it.
        /// </summary>
        public void Draw(Effect effect)
        {
            GraphicsDevice graphicsDevice = effect.GraphicsDevice;

            // Set vertex buffer, and index buffer.
            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            // Draw the model, using the specified effect.
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();

                int primitiveCount = indices.Count / 3;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Count, 0, primitiveCount);
            }
        }

        /// <summary>
        /// Draws the primitive model, using a BasicEffect shader with default
        /// lighting. Unlike the other Draw overload where you specify a custom
        /// effect, this method sets important renderstates to sensible values
        /// for 3D model rendering, so you do not need to set these states before
        /// you call it.
        /// </summary>
        public void Draw(BasicEffect basicEffect, Matrix world, Matrix view, Matrix projection, Color color)
        {
            // Set BasicEffect parameters.
            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.DiffuseColor = color.ToVector3();
            basicEffect.Alpha = color.A / 255.0f;

            if (color.A < 255)
            {
                basicEffect.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                basicEffect.GraphicsDevice.BlendState = BlendState.Additive;
            }
            else
            {
                basicEffect.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                basicEffect.GraphicsDevice.BlendState = BlendState.Opaque;
            }

            // Draw the model, using BasicEffect.
            Draw(basicEffect);
        }

        /// <summary>
        /// Draws the primitive model, using an AlphaTestEffect shader.
        /// </summary>
        public void DrawAlphaTest(AlphaTestEffect atEffect, Matrix world, Matrix view, Matrix projection, Color color)
        {
            // Set AlphaTest effect parameters.
            atEffect.World = world;
            atEffect.View = view;
            atEffect.Projection = projection;
            atEffect.DiffuseColor = color.ToVector3();
            atEffect.Alpha = color.A / 255.0f;

            if (color.A < 255)
            {
                atEffect.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                atEffect.GraphicsDevice.BlendState = BlendState.Additive;
            }
            else
            {
                atEffect.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                atEffect.GraphicsDevice.BlendState = BlendState.Opaque;
            }

            // Draw the model, using the AlphaTestEffect.
            Draw(atEffect);
        }

        /// <summary>
        /// Draws the primitive model, using a DualTextureEffect shader.
        /// </summary>
        public void DrawDualTextured(DualTextureEffect dtEffect, Matrix world, Matrix view, Matrix projection, Color color)
        {
            // Set DrawDualTextured effect parameters.
            dtEffect.World = world;
            dtEffect.View = view;
            dtEffect.Projection = projection;
            dtEffect.DiffuseColor = color.ToVector3();
            dtEffect.Alpha = color.A / 255.0f;

            if (color.A < 255)
            {
                dtEffect.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                dtEffect.GraphicsDevice.BlendState = BlendState.Additive;
            }
            else
            {
                dtEffect.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                dtEffect.GraphicsDevice.BlendState = BlendState.Opaque;
            }

            // Draw the model, using DualTextureEffect.
            Draw(dtEffect);
        }
        #endregion
    }

} // end Namespace

