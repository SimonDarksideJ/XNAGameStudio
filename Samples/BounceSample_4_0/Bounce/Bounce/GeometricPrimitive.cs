#region File Description
//-----------------------------------------------------------------------------
// GeometricPrimitive.cs
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

namespace Bounce
{
    /// <summary>
    /// Base class for simple geometric primitive models. This provides a vertex
    /// buffer, an index buffer, plus methods for drawing the model. Classes for
    /// specific types of primitive (CubePrimitive, SpherePrimitive, etc.) are
    /// derived from this common base, and use the AddVertex and AddIndex methods
    /// to specify their geometry.
    /// </summary>
    public abstract class GeometricPrimitive : IDisposable
    {
        #region Fields


        // During the process of constructing a primitive model, vertex
        // and index data is stored on the CPU in these managed lists.
        List<VertexPositionNormal> vertices = new List<VertexPositionNormal>();
        List<ushort> indices = new List<ushort>();


        // Once all the geometry has been specified, the InitializePrimitive
        // method copies the vertex and index data into these buffers, which
        // store it on the GPU ready for efficient rendering.
        VertexDeclaration vertexDeclaration;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        BasicEffect basicEffect;
        BasicEffect basicEffectShadow;


        #endregion

        #region Initialization


        /// <summary>
        /// Adds a new vertex to the primitive model. This should only be called
        /// during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddVertex(Vector3 position, Vector3 normal)
        {
            vertices.Add(new VertexPositionNormal(position, normal));
        }


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
        /// Queries the index of the current vertex. This starts at
        /// zero, and increments every time AddVertex is called.
        /// </summary>
        protected int CurrentVertex
        {
            get { return vertices.Count; }
        }


        /// <summary>
        /// Once all the geometry has been specified by calling AddVertex and AddIndex,
        /// this method copies the vertex and index data into GPU format buffers, ready
        /// for efficient rendering.
        protected void InitializePrimitive(GraphicsDevice graphicsDevice)
        {
            // Create a vertex declaration, describing the format of our vertex data.
            vertexDeclaration = new VertexDeclaration(VertexPositionNormal.VertexElements);

            // Create a vertex buffer, and copy our vertex data into it.
            vertexBuffer = new VertexBuffer(graphicsDevice,
                                            vertexDeclaration,
                                            vertices.Count, BufferUsage.None);

            vertexBuffer.SetData(vertices.ToArray());

            // Create an index buffer, and copy our index data into it.
            indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort),
                                          indices.Count, BufferUsage.None);

            indexBuffer.SetData(indices.ToArray());

            // Create a BasicEffect, which will be used to render the primitive.
            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.LightingEnabled = true;

            // Set up light 0 for per pixel lighting, 1 directoinal light
            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.DirectionalLight0.Direction = new Vector3(0.25f, -1.0f, -1.0f);
            basicEffect.DirectionalLight0.Direction.Normalize();

            basicEffect.SpecularColor = Vector3.One;
            basicEffect.DirectionalLight0.SpecularColor = Vector3.One;
            basicEffect.SpecularPower = 32;
            basicEffect.PreferPerPixelLighting = false;

            // Shadow 
            basicEffectShadow = new BasicEffect(graphicsDevice);
            basicEffectShadow.LightingEnabled = false;
            basicEffectShadow.PreferPerPixelLighting = false;
        }


        /// <summary>
        /// Finalizer.
        /// </summary>
        ~GeometricPrimitive()
        {
            Dispose(false);
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (vertexDeclaration != null)
                    vertexDeclaration.Dispose();

                if (vertexBuffer != null)
                    vertexBuffer.Dispose();

                if (indexBuffer != null)
                    indexBuffer.Dispose();

                if (basicEffect != null)
                    basicEffect.Dispose();

                if (basicEffectShadow != null)
                    basicEffectShadow.Dispose();
            }
        }


        #endregion

        #region Draw

        /// <summary>
        /// Draws the primitive model, using the specified effect. Unlike the other
        /// Draw overload where you just specify the world/view/projection matrices
        /// and color, this method does not set any renderstates, so you must make
        /// sure all states are set to sensible values before you call it.
        /// </summary>
        public void Draw(BasicEffect effect)
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
        public void Draw(Matrix world, Matrix view, Matrix projection, Color color, Boolean drawShadow)
        {
            BasicEffect drawBasicEffect = (drawShadow ? basicEffectShadow : basicEffect);

            // Set BasicEffect parameters.
            drawBasicEffect.World = world;
            drawBasicEffect.View = view;
            drawBasicEffect.Projection = projection;
            drawBasicEffect.DiffuseColor = color.ToVector3();
            drawBasicEffect.Alpha = color.A / 255.0f;

            // Draw the model, using BasicEffect.
            Draw(drawBasicEffect);
        }


        #endregion
    }
}
