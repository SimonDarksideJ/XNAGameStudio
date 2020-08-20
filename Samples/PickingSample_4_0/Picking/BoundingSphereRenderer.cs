#region File Description
//-----------------------------------------------------------------------------
// BoundingSphereRenderer.cs
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

namespace BoundingVolumeRendering
{
    /// <summary>
    /// Provides a set of methods for rendering BoundingSpheres.
    /// </summary>
    public static class BoundingSphereRenderer
    {
        private static VertexBuffer vertBuffer;
        private static BasicEffect effect;
        private static int lineCount;

        /// <summary>
        /// Initializes the graphics objects for rendering BoundingSpheres.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use when rendering.</param>
        /// <param name="sphereResolution">The number of line segments to use for each of the three circles.</param>
        public static void Initialize(GraphicsDevice graphicsDevice, int sphereResolution)
        {
            // create our effect
            effect = new BasicEffect(graphicsDevice);
            effect.LightingEnabled = false;
            effect.VertexColorEnabled = true;

            // calculate the number of lines to draw for all circles
            lineCount = (sphereResolution + 1) * 3;

            // we need two vertices per line, so we can allocate our vertices
            VertexPositionColor[] verts = new VertexPositionColor[lineCount * 2];

            // compute our step around each circle
            float step = MathHelper.TwoPi / sphereResolution;

            // used to track the index into our vertex array
            int index = 0;

            //create the loop on the XY plane first
            for (float a = 0f; a < MathHelper.TwoPi; a += step)
            {
                verts[index++] = new VertexPositionColor(
                    new Vector3((float)Math.Cos(a), (float)Math.Sin(a), 0f),
                    Color.Blue);
                verts[index++] = new VertexPositionColor(
                    new Vector3((float)Math.Cos(a + step), (float)Math.Sin(a + step), 0f),
                    Color.Blue);
            }

            //next on the XZ plane
            for (float a = 0f; a < MathHelper.TwoPi; a += step)
            {
                verts[index++] = new VertexPositionColor(
                    new Vector3((float)Math.Cos(a), 0f, (float)Math.Sin(a)),
                    Color.Red);
                verts[index++] = new VertexPositionColor(
                    new Vector3((float)Math.Cos(a + step), 0f, (float)Math.Sin(a + step)),
                    Color.Red);
            }

            //finally on the YZ plane
            for (float a = 0f; a < MathHelper.TwoPi; a += step)
            {
                verts[index++] = new VertexPositionColor(
                    new Vector3(0f, (float)Math.Cos(a), (float)Math.Sin(a)),
                    Color.Green);
                verts[index++] = new VertexPositionColor(
                    new Vector3(0f, (float)Math.Cos(a + step), (float)Math.Sin(a + step)),
                    Color.Green);
            }

            // now we create the vertex buffer and put the vertices in it
            vertBuffer = new VertexBuffer(
                graphicsDevice, typeof(VertexPositionColor), verts.Length, BufferUsage.WriteOnly);
            vertBuffer.SetData(verts);
        }

        /// <summary>
        /// Draws a BoundingSphere.
        /// </summary>
        /// <remarks>
        /// This method is an extension method (note the 'this' keyword in front of our BoundingSphere parameter).
        /// This allows us to either call this method like a static method or like an instance method:
        /// 
        /// BoundingSphereRenderer.Draw(sphere, view, projection);
        /// sphere.Draw(view, projection); 
        /// </remarks>
        /// <param name="sphere">The sphere to render.</param>
        /// <param name="view">The current view matrix.</param>
        /// <param name="projection">The current projection matrix.</param>
        public static void Draw(this BoundingSphere sphere, Matrix view, Matrix projection)
        {
            if (effect == null)
                throw new InvalidOperationException("You must call Initialize before you can render any spheres.");

            // set the vertex buffer
            effect.GraphicsDevice.SetVertexBuffer(vertBuffer);

            // update our effect matrices
            effect.World = Matrix.CreateScale(sphere.Radius) * Matrix.CreateTranslation(sphere.Center);
            effect.View = view;
            effect.Projection = projection;

            // draw the primitives with our effect
            effect.CurrentTechnique.Passes[0].Apply();
            effect.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, lineCount);
        }
    }
}
