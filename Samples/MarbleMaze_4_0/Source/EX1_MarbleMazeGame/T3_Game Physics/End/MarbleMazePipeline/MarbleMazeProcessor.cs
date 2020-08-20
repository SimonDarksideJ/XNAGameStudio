#region File Description
//-----------------------------------------------------------------------------
// MazeProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace MarbleMazePipeline
{
    /// <summary>
    /// This custom processor attaches vertex position data to a model's tag property
    /// for later use. The vertices are arranged so that each consecutive triplet
    /// defines a triangle on the mesh.
    /// </summary>
    [ContentProcessor]
    public class MarbleMazeProcessor : ModelProcessor
    {
        #region Fields
        Dictionary<string, List<Vector3>> tagData = new Dictionary<string, List<Vector3>>();
        #endregion

        #region Intialization
        /// <summary>
        /// The main method in charge of processing the content.
        /// </summary>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            FindVertices(input);

            ModelContent model = base.Process(input, context);

            model.Tag = tagData;

            return model;
        }
        #endregion

        #region Private functionality
        /// <summary>
        /// Helper for extracting a list of all the vertex positions in a model.
        /// </summary>
        void FindVertices(NodeContent node)
        {
            // Is this node a mesh?
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                string meshName = mesh.Name;
                List<Vector3> meshVertices = new List<Vector3>();
                // Look up the absolute transform of the mesh.
                Matrix absoluteTransform = mesh.AbsoluteTransform;
                // Loop over all the pieces of geometry in the mesh.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    // Loop over all the indices in this piece of geometry.
                    // Every group of three indices represents one triangle.
                    foreach (int index in geometry.Indices)
                    {
                        // Look up the position of this vertex.
                        Vector3 vertex = geometry.Vertices.Positions[index];

                        // Transform from local into world space.
                        vertex = Vector3.Transform(vertex, absoluteTransform);

                        // Store this vertex.
                        meshVertices.Add(vertex);
                    }
                }

                tagData.Add(meshName, meshVertices);
            }

            // Recursively scan over the children of this node.
            foreach (NodeContent child in node.Children)
            {
                FindVertices(child);
            }
        }
        #endregion
    }
}
