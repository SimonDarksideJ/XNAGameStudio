#region File Description
//-----------------------------------------------------------------------------
// LightingProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Usings
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.Diagnostics;
#endregion

namespace Pickture.Pipeline
{
    /// <summary>
    /// Prepares a model to be rendered using the lighting shader.
    /// </summary>
    [ContentProcessor(DisplayName="Lighting Processor")]
    public class LightingProcessor : ModelProcessor
    {
        public override ModelContent Process(NodeContent input,
                                             ContentProcessorContext context)
        {
            CalculateTangentFrames(input, context);
            // The base processor will include the tagent frames in the resulting model
            ModelContent modelContent = base.Process(input, context);

            // Copy each mesh part's material name to it's tag. Chip rendering uses the
            // material name to determine which texture goes on which side.
            foreach (ModelMeshContent modelMesh in modelContent.Meshes)
            {
                foreach (ModelMeshPartContent modelMeshPart in modelMesh.MeshParts)
                    modelMeshPart.Tag = modelMeshPart.Material.Name;
            }

            return modelContent;
        }

        /// <summary>
        /// Recursively adds calculated tangent frames to all meshes.
        /// </summary>
        void CalculateTangentFrames(NodeContent input, ContentProcessorContext context)
        {
            MeshContent inputMesh = input as MeshContent;
            if (inputMesh != null)
            {
                MeshHelper.CalculateTangentFrames(inputMesh,
                    VertexChannelNames.TextureCoordinate(0),
                    VertexChannelNames.Tangent(0), null);
            }

            foreach (NodeContent childNode in input.Children)
                CalculateTangentFrames(childNode, context);
        }
    }
}
