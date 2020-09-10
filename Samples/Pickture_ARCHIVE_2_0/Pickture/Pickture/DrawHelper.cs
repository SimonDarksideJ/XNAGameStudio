#region File Description
//-----------------------------------------------------------------------------
// DrawHelper.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Pickture
{
    /// <summary>
    /// Provides some helper methods for drawing.
    /// </summary>
    static class DrawHelper
    {
        /// <summary>
        /// Reset the rendering states to known values.
        /// </summary>
        public static void SetState()
        {
            RenderState state = Pickture.Instance.GraphicsDevice.RenderState;

            state.AlphaBlendEnable = true;
            state.SourceBlend = Blend.SourceAlpha;
            state.DestinationBlend = Blend.InverseSourceAlpha;
            state.AlphaSourceBlend = Blend.SourceAlpha;
            state.AlphaDestinationBlend = Blend.InverseSourceAlpha;

            state.DepthBufferEnable = true;
            state.DepthBufferWriteEnable = true;
        }

        /// <summary>
        /// Draws a ModelMeshPart with a custom effect.
        /// </summary>
        /// <param name="mesh">Mesh which owns the mesh part.</param>
        /// <param name="part">Mesh part to be drawn.</param>
        /// <param name="effect">Effect to draw the mesh part with.</param>
        public static void DrawMeshPart(ModelMesh mesh, ModelMeshPart part,Effect effect)
        {
            GraphicsDevice device = Pickture.Instance.GraphicsDevice;

            foreach(EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                
                device.VertexDeclaration = part.VertexDeclaration;
                device.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset,
                                             part.VertexStride);
                device.Indices = mesh.IndexBuffer;

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                             part.BaseVertex, 0, part.NumVertices,
                                             part.StartIndex, part.PrimitiveCount);

                pass.End();
            }
        }
    }
}
