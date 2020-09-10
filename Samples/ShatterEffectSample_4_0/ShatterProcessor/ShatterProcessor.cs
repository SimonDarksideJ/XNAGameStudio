#region File Description
//-----------------------------------------------------------------------------
// ShatterProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
#endregion

namespace ShatterEffectProcessor
{
    [ContentProcessor]
    public class ShatterProcessor : ModelProcessor
    {
        private string triangleCenterChannel = VertexChannelNames.TextureCoordinate(1);
        private string rotationalVelocityChannel = 
            VertexChannelNames.TextureCoordinate(2);
      
        public override ModelContent Process(NodeContent input, 
                                             ContentProcessorContext context)
        {
            // Break up the mesh to separate triangles.           
            NodeContent processedNode = ProcessMesh(input); 
            return base.Process(processedNode, context);
        }

        /// <summary>
        /// Breaks the input mesh into separate un-indexed triangles.
        /// </summary>
        /// <param name="input">Input MeshContent node.</param>
        /// <returns>Broken MeshContent</returns>
        private MeshContent ProcessMesh(NodeContent input)
        {
            MeshBuilder builder = MeshBuilder.StartMesh("model");
            
            MeshContent mesh = input as MeshContent;
            List<Vector3> normalList = new List<Vector3>();
            List<Vector2> texCoordList = new List<Vector2>();

            if (mesh != null)
            {
                int normalChannel = builder.CreateVertexChannel<Vector3>(
                                               VertexChannelNames.Normal());
                int texChannel = builder.CreateVertexChannel<Vector2>(
                                               VertexChannelNames.TextureCoordinate(0));

                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    IndirectPositionCollection positions = geometry.Vertices.Positions;

                    VertexChannel<Vector3> normals =
                        geometry.Vertices.Channels.Get<Vector3>(
                                                        VertexChannelNames.Normal());
                    
                    VertexChannel<Vector2> texCoords =
                        geometry.Vertices.Channels.Get<Vector2>(
                                            VertexChannelNames.TextureCoordinate(0));

                    // Copy the positions over     
                    // To do that, we traverse the indices and grab the indexed 
                    // position  and add it to the new mesh. This in effect will 
                    // duplicate positions in the mesh reversing the compacting
                    // effect of using index buffers.
                    foreach (int i in geometry.Indices)
                    {
                        builder.CreatePosition(positions[i]);

                        // Save the normals and the texture coordinates for additon to
                        // the mesh later.
                        normalList.Add(normals[i]);
                        texCoordList.Add(texCoords[i]);
                    }
                }

                int index = 0;

                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    // Save the material to the new mesh.
                    builder.SetMaterial(geometry.Material);

                    // Now we create the Triangles. 
                    // To do that, we simply generate an index list that is sequential
                    // from 0 to geometry.Indices.Count
                    // This will create an index buffer that looks like: 0,1,2,3,4,5,...
                    for (int i = 0; i < geometry.Indices.Count; i++)
                    {
                        // Set the normal for the current vertex
                        builder.SetVertexChannelData(normalChannel, normalList[index]);
                        // Set the texture coordinates for the current vertex
                        builder.SetVertexChannelData(texChannel, texCoordList[index]);
                        builder.AddTriangleVertex(index);
                        index++;
                    }
                }
            }

            MeshContent finalMesh = builder.FinishMesh();
            // Copy the transform over from the source mesh to retain parent/child 
            // relative transforms.
            finalMesh.Transform = input.Transform; 

            // Now we take the new MeshContent and calculate the centers of all the
            // triangles. The centers are needed so that we can rotate the triangles
            // around them as we shatter the model.
            foreach (GeometryContent geometry in finalMesh.Geometry)
            {                   
                Vector3[] triangleCenters = new Vector3[geometry.Indices.Count / 3];
                Vector3[] trianglePoints = new Vector3[2];
                
                IndirectPositionCollection positions = geometry.Vertices.Positions;
                
                for (int i = 0; i < positions.Count; i++)
                {
                    Vector3 position = positions[i];

                    if (i % 3 == 2)
                    {
                        // Calculate the center of the triangle.
                        triangleCenters[i / 3] = (trianglePoints[0] + trianglePoints[1]
                                                 + position) / 3;
                    }
                    else
                    {
                        trianglePoints[i % 3] = position;
                    }
                }

                // Add two new channels to the MeshContent:
                // triangleCenterChannel: This is the channel that will store the center
                // of the triangle that this vertex belongs to.
                // rotationalVelocityChannel: This channel has randomly generated values
                // for x,y and z rotational angles. This information will be used to
                // randomly rotate the triangles as they shatter from the model.
                geometry.Vertices.Channels.Add<Vector3>(
                    triangleCenterChannel, 
                    new ReplicateTriangleDataToEachVertex <Vector3>(triangleCenters));
                geometry.Vertices.Channels.Add<Vector3>(
                    rotationalVelocityChannel, 
                    new ReplicateTriangleDataToEachVertex <Vector3>(
                    new RandomVectorEnumerable(triangleCenters.Length)));                
            }

            foreach (NodeContent child in input.Children)
            {
                finalMesh.Children.Add(ProcessMesh(child));
            }

            return finalMesh;
        }

        
        
        /// <summary>
        ///Overriding the ConvertMaterial function of ModelProcessor so that we can
        ///replace the BasicEffect with our own ShatterEffect.   
        /// </summary>
        /// <param name="material">input Material</param>
        /// <param name="context">Content processor context</param>
        /// <returns></returns>
        protected override MaterialContent ConvertMaterial(MaterialContent material, 
                                                    ContentProcessorContext context)
        {
            EffectMaterialContent effect = new EffectMaterialContent();
            // Use our own ShatterEffect.fx instead of BasicEffect.
            effect.Effect = 
                new ExternalReference<EffectContent>("shatterEffect.fx"); 

            foreach (ExternalReference<TextureContent> texture in 
                                                            material.Textures.Values)
            {
                // Add the textures in the source Material to our effect.
                effect.Textures.Add("modelTexture", texture); 
            }
            return base.ConvertMaterial(effect, context);
        }      
    }
}
