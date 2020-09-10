#region File Description
//-----------------------------------------------------------------------------
// TrianglePickingProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace TrianglePickingPipeline
{
    /// <summary>
    /// Custom content pipeline processor attaches vertex position information to
    /// a model, which can be used at runtime to implement per-triangle picking.
    /// It derives from the built-in ModelProcessor, and overrides the Process
    /// method, using this to attach custom data to the model Tag property.
    /// </summary>
    [ContentProcessor]
    public class TrianglePickingProcessor : ModelProcessor
    {
        List<Vector3> vertices = new List<Vector3>();


        /// <summary>
        /// The main method in charge of processing the content.
        /// </summary>
        public override ModelContent Process(NodeContent input,
                                             ContentProcessorContext context)
        {
            // Chain to the base ModelProcessor class.
            ModelContent model = base.Process(input, context);

            // Look up the input vertex positions.
            FindVertices(input);

            // You can store any type of object in the model Tag property. This
            // sample only uses built-in types such as string, Vector3, BoundingSphere,
            // dictionaries, and arrays, which the content pipeline knows how to
            // serialize by default. We could also attach custom data types here, but
            // then we would have to provide a ContentTypeWriter and ContentTypeReader
            // implementation to tell the pipeline how to serialize our custom type.
            //
            // We are setting our model Tag to a dictionary that maps strings to
            // objects, and then storing two different kinds of custom data into that
            // dictionary. This is a useful pattern because it allows processors to
            // combine many different kinds of information inside the single Tag value.

            Dictionary<string, object> tagData = new Dictionary<string, object>();

            model.Tag = tagData;

            // Store vertex information in the tag data, as an array of Vector3.
            tagData.Add("Vertices", vertices.ToArray());

            // Also store a custom bounding sphere.
            tagData.Add("BoundingSphere", BoundingSphere.CreateFromPoints(vertices));
            
            return model;
        }


        /// <summary>
        /// Helper for extracting a list of all the vertex positions in a model.
        /// </summary>
        void FindVertices(NodeContent node)
        {
            // Is this node a mesh?
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
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
                        vertices.Add(vertex);
                    }
                }
            }

            // Recursively scan over the children of this node.
            foreach (NodeContent child in node.Children)
            {
                FindVertices(child);
            }
        }
    }
}
