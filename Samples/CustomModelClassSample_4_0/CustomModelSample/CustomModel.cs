#region File Description
//-----------------------------------------------------------------------------
// CustomModel.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace CustomModelSample
{
    /// <summary>
    /// Custom class that can be used as a replacement for the built-in Model type.
    /// This provides functionality roughly similar to Model, but simplified as far
    /// as possible while still being able to correctly render data from arbitrary
    /// X or FBX files. This can be used as a starting point for building up your
    /// own more sophisticated Model replacements.
    /// </summary>
    public class CustomModel
    {
        #region Fields

        // Disable compiler warning that we never initialize these fields.
        // That's ok, because the XNB deserializer initialises them for us!
        #pragma warning disable 649


        // Internally our custom model is made up from a list of model parts.
        [ContentSerializer]
        List<ModelPart> modelParts;


        // Each model part represents a piece of geometry that uses one
        // single effect. Multiple parts are needed for models that use
        // more than one effect.
        class ModelPart
        {
            public int TriangleCount;
            public int VertexCount;

            public VertexBuffer VertexBuffer;
            public IndexBuffer IndexBuffer;

            [ContentSerializer(SharedResource = true)]
            public Effect Effect;
        }


        #pragma warning restore 649

        #endregion


        /// <summary>
        /// Private constructor, for use by the XNB deserializer.
        /// </summary>
        private CustomModel()
        {
        }


        /// <summary>
        /// Draws the model using the specified camera matrices.
        /// </summary>
        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelPart modelPart in modelParts)
            {
                // Look up the effect, and set effect parameters on it. This sample
                // assumes the model will only be using BasicEffect, but a more robust
                // implementation would probably want to handle custom effects as well.
                BasicEffect effect = (BasicEffect)modelPart.Effect;

                effect.EnableDefaultLighting();

                effect.World = world;
                effect.View = view;
                effect.Projection = projection;

                // Set the graphics device to use our vertex declaration,
                // vertex buffer, and index buffer.
                GraphicsDevice device = effect.GraphicsDevice;

                device.SetVertexBuffer(modelPart.VertexBuffer);
                
                device.Indices = modelPart.IndexBuffer;

                // Loop over all the effect passes.
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    // Draw the geometry.
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                 0, 0, modelPart.VertexCount,
                                                 0, modelPart.TriangleCount);
                }
            }
        }
    }
}
