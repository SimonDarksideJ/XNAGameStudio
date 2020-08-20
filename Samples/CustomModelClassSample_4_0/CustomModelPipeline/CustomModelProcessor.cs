#region File Description
//-----------------------------------------------------------------------------
// CustomModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace CustomModelPipeline
{
    /// <summary>
    /// Content Pipeline processor converts incoming
    /// graphics data into our custom model format.
    /// </summary>
    [ContentProcessor(DisplayName = "Custom Model")]
    public class CustomModelProcessor : ContentProcessor<NodeContent,
                                                         CustomModelContent>
    {
        #region Fields

        ContentProcessorContext context;
        CustomModelContent outputModel;

        // A single material may be reused on more than one piece of geometry.
        // This dictionary keeps track of materials we have already converted,
        // to make sure we only bother processing each of them once.
        Dictionary<MaterialContent, MaterialContent> processedMaterials =
                            new Dictionary<MaterialContent, MaterialContent>();

        #endregion


        /// <summary>
        /// Converts incoming graphics data into our custom model format.
        /// </summary>
        public override CustomModelContent Process(NodeContent input,
                                                   ContentProcessorContext context)
        {
            this.context = context;

            outputModel = new CustomModelContent();

            ProcessNode(input);

            return outputModel;
        }


        /// <summary>
        /// Recursively processes a node from the input data tree.
        /// </summary>
        void ProcessNode(NodeContent node)
        {
            // Meshes can contain internal hierarchy (nested tranforms, joints, bones,
            // etc), but this sample isn't going to bother storing any of that data.
            // Instead we will just bake any node transforms into the geometry, after
            // which we can reset them to identity and forget all about them.
            MeshHelper.TransformScene(node, node.Transform);
            
            node.Transform = Matrix.Identity;

            // Is this node in fact a mesh?
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Reorder vertex and index data so triangles will render in
                // an order that makes efficient use of the GPU vertex cache.
                MeshHelper.OptimizeForCache(mesh);

                // Process all the geometry in the mesh.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    ProcessGeometry(geometry);
                }
            }

            // Recurse over any child nodes.
            foreach (NodeContent child in node.Children)
            {
                ProcessNode(child);
            }
        }


        /// <summary>
        /// Converts a single piece of input geometry into our custom format.
        /// </summary>
        void ProcessGeometry(GeometryContent geometry)
        {
            int triangleCount = geometry.Indices.Count / 3;
            int vertexCount = geometry.Vertices.VertexCount;

            // Flatten the flexible input vertex channel data into
            // a simple GPU style vertex buffer byte array.
            VertexBufferContent vertexBufferContent = geometry.Vertices.CreateVertexBuffer();

            // Convert the input material.
            MaterialContent material = ProcessMaterial(geometry.Material);

            // Add the new piece of geometry to our output model.
            outputModel.AddModelPart(triangleCount, vertexCount,
                                     vertexBufferContent, geometry.Indices, material);
        }


        /// <summary>
        /// Converts an input material by chaining to the built-in MaterialProcessor
        /// class. This will automatically go off and build any effects or textures
        /// that are referenced by the material. When you load the resulting material
        /// at runtime, you will get back an Effect instance that has the appropriate
        /// textures already loaded into it and ready to go.
        /// </summary>
        MaterialContent ProcessMaterial(MaterialContent material)
        {
            // Have we already processed this material?
            if (!processedMaterials.ContainsKey(material))
            {
                // If not, process it now.
                processedMaterials[material] =
                    context.Convert<MaterialContent,
                                    MaterialContent>(material, "MaterialProcessor");
            }

            return processedMaterials[material];
        }
    }
}
