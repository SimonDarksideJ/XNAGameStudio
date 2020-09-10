#region File Description
//-----------------------------------------------------------------------------
// CpuSkinnedModelPart.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CpuSkinningDataTypes
{
    public class CpuSkinnedModelPart
    {
        private readonly int triangleCount;
        private readonly int vertexCount;
        private readonly CpuVertex[] cpuVertices;
        private readonly VertexPositionNormalTexture[] gpuVertices;

        private readonly DynamicVertexBuffer vertexBuffer;
        private readonly IndexBuffer indexBuffer;

        public BasicEffect Effect { get; internal set; }
        
        internal CpuSkinnedModelPart(int triangleCount, CpuVertex[] vertices, IndexBuffer indexBuffer)
        {
            this.triangleCount = triangleCount;
            this.vertexCount = vertices.Length;
            this.cpuVertices = vertices;
            this.indexBuffer = indexBuffer;
       
            // create our GPU resources
            gpuVertices = new VertexPositionNormalTexture[cpuVertices.Length];
            vertexBuffer = new DynamicVertexBuffer(indexBuffer.GraphicsDevice, typeof(VertexPositionNormalTexture), cpuVertices.Length, BufferUsage.WriteOnly);

            // copy texture coordinates once since they don't change with skinnning
            for (int i = 0; i < cpuVertices.Length; i++)
            {
                gpuVertices[i].TextureCoordinate = cpuVertices[i].TextureCoordinate;
            }
        }

        public void SetBones(Matrix[] bones)
        {
            // skin all of the vertices
            for (int i = 0; i < vertexCount; i++)
            {
                CpuSkinningHelpers.SkinVertex(
                    bones,
                    ref cpuVertices[i].Position,
                    ref cpuVertices[i].Normal,
                    ref cpuVertices[i].BlendIndices,
                    ref cpuVertices[i].BlendWeights,
                    out gpuVertices[i].Position,
                    out gpuVertices[i].Normal);
            }

            // put the vertices into our vertex buffer
            vertexBuffer.SetData(gpuVertices, 0, vertexCount, SetDataOptions.Discard);
        }

        public void Draw()
        {
            GraphicsDevice graphics = Effect.GraphicsDevice;

            // set our buffers on the device
            graphics.Indices = indexBuffer;
            graphics.SetVertexBuffer(vertexBuffer);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, triangleCount);
            }

            graphics.Indices = null;
            graphics.SetVertexBuffer(null);
        }
    }
}
