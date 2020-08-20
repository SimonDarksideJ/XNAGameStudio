#region File Description
//-----------------------------------------------------------------------------
// SkyProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace GeneratedGeometryPipeline
{
    /// <summary>
    /// Custom content processor for creating skydome meshes. Given an
    /// input sky texture, this processor uses the MeshBuilder class to
    /// programatically generate skydome geometry. It creates a cylinder,
    /// texture maps the sky image around it, and assigns a custom effect
    /// that will be used to render the sky.
    /// </summary>
    [ContentProcessor]
    public class SkyProcessor : ContentProcessor<Texture2DContent, SkyContent>
    {
        const float cylinderSize = 100;

        const int cylinderSegments = 32;
        
        const float texCoordTop = 0.1f;
        const float texCoordBottom = 0.9f;

        
        /// <summary>
        /// Generates skydome geometry for an input sky texture.
        /// </summary>
        public override SkyContent Process(Texture2DContent input,
                                           ContentProcessorContext context)
        {
            MeshBuilder builder = MeshBuilder.StartMesh("sky");

            // Create two rings of vertices around the top and bottom of the cylinder.
            List<int> topVertices = new List<int>();
            List<int> bottomVertices = new List<int>();

            for (int i = 0; i < cylinderSegments; i++)
            {
                float angle = MathHelper.TwoPi * i / cylinderSegments;

                float x = (float)Math.Cos(angle) * cylinderSize;
                float z = (float)Math.Sin(angle) * cylinderSize;

                topVertices.Add(builder.CreatePosition(x, cylinderSize, z));
                bottomVertices.Add(builder.CreatePosition(x, -cylinderSize, z));
            }

            // Create two center vertices, used for closing the top and bottom.
            int topCenterVertex = builder.CreatePosition(0, cylinderSize * 2, 0);
            int bottomCenterVertex = builder.CreatePosition(0, -cylinderSize * 2, 0);

            // Create a vertex channel for holding texture coordinates.
            int texCoordId = builder.CreateVertexChannel<Vector2>(
                                            VertexChannelNames.TextureCoordinate(0));

            builder.SetMaterial(new BasicMaterialContent());

            // Create the individual triangles that make up our skydome.
            for (int i = 0; i < cylinderSegments; i++)
            {
                int j = (i + 1) % cylinderSegments;

                // Calculate texture coordinates for this segment of the cylinder.
                float u1 = (float)i / (float)cylinderSegments;
                float u2 = (float)(i + 1) / (float)cylinderSegments;

                // Two triangles form a quad, one side segment of the cylinder.
                AddVertex(builder, topVertices[i], texCoordId, u1, texCoordTop);
                AddVertex(builder, topVertices[j], texCoordId, u2, texCoordTop);
                AddVertex(builder, bottomVertices[i], texCoordId, u1, texCoordBottom);

                AddVertex(builder, topVertices[j], texCoordId, u2, texCoordTop);
                AddVertex(builder, bottomVertices[j], texCoordId, u2, texCoordBottom);
                AddVertex(builder, bottomVertices[i], texCoordId, u1, texCoordBottom);

                // Triangle fanning inward to fill the top above this segment.
                AddVertex(builder, topCenterVertex, texCoordId, u1, 0);
                AddVertex(builder, topVertices[j], texCoordId, u2, texCoordTop);
                AddVertex(builder, topVertices[i], texCoordId, u1, texCoordTop);

                // Triangle fanning inward to fill the bottom below this segment.
                AddVertex(builder, bottomCenterVertex, texCoordId, u1, 1);
                AddVertex(builder, bottomVertices[i], texCoordId, u1, texCoordBottom);
                AddVertex(builder, bottomVertices[j], texCoordId, u2, texCoordBottom);
            }

            // Create the output object.
            SkyContent sky = new SkyContent();

            // Chain to the ModelProcessor so it can convert the mesh we just generated.
            MeshContent skyMesh = builder.FinishMesh();

            sky.Model = context.Convert<MeshContent, ModelContent>(skyMesh,
                                                                   "ModelProcessor");
            
            // Chain to the TextureProcessor so it can convert the sky
            // texture. We don't use the default ModelTextureProcessor
            // here, because that would apply DXT compression, which
            // doesn't usually look very good with sky images.

            // Note: This could also be accomplished by creating a custom ModelProcessor
            // that would process its textures with the default TextureProcessor,
            // and adding the sky texture to the materials Textures dictionary.
            // For simplicity, the approach below is used instead.
            sky.Texture = context.Convert<TextureContent, TextureContent>(input,
                                                                    "TextureProcessor");

            return sky;
        }


        /// <summary>
        /// Helper for adding a new triangle vertex to a MeshBuilder,
        /// along with an associated texture coordinate value.
        /// </summary>
        static void AddVertex(MeshBuilder builder, int vertex,
                              int texCoordId, float u, float v)
        {
            builder.SetVertexChannelData(texCoordId, new Vector2(u, v));
            
            builder.AddTriangleVertex(vertex);
        }
    }
}
