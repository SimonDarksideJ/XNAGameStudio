#region File Description
//-----------------------------------------------------------------------------
// VegetationProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace BillboardPipeline
{
    /// <summary>
    /// Custom content processor adds randomly positioned billboards on the
    /// surface of the input mesh. This technique can be used to scatter
    /// grass billboards across a landscape model.
    /// </summary>
    [ContentProcessor]
    public class VegetationProcessor : ModelProcessor
    {
        #region Processor Parameters


        int billboardsPerTriangle = 23;

        [DisplayName("Billboards per Triangle")]
        [DefaultValue(23)]
        [Description("Amount of vegetation per triangle in the landscape geometry.")]
        public int BillboardsPerTriangle
        {
            get { return billboardsPerTriangle; }
            set { billboardsPerTriangle = value; }
        }


        double treeProbability = 0.002;
        [DisplayName("Tree Probability")]
        [DefaultValue(0.002)]
        [Description("The chance that a given piece of vegetation is actually a tree.")]
        public double TreeProbability
        {
            get { return treeProbability; }
            set { treeProbability = value; }
        }


        double catProbability = 0.0001;
        [DisplayName("Cat Probability")]
        [DefaultValue(0.0001)]
        [Description("The chance that a given piece of vegetation is actually a cat.")]
        public double CatProbability
        {
            get { return catProbability; }
            set { catProbability = value; }
        }


        [DefaultValue(false)]
        public override bool PremultiplyTextureAlpha { get; set; }


        #endregion

        Random random = new Random();


        /// <summary>
        /// Override the main Process method.
        /// </summary>
        public override ModelContent Process(NodeContent input,
                                             ContentProcessorContext context)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            // Create vegetation billboards.
            GenerateVegetation(input, input.Identity);

            // Chain to the standard ModelProcessor.
            return base.Process(input, context);
        }


        /// <summary>
        /// Recursive function adds vegetation billboards to all meshes.
        /// </summary>
        void GenerateVegetation(NodeContent node, ContentIdentity identity)
        {
            // First, recurse over any child nodes.
            foreach (NodeContent child in node.Children)
            {
                GenerateVegetation(child, identity);
            }

            // Check whether this node is in fact a mesh.
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Create three new geometry objects, one for each type
                // of billboard that we are going to create. Set different
                // effect parameters to control the size and wind sensitivity
                // for each type of billboard.
                GeometryContent grass = CreateVegetationGeometry("grass.tga", 5, 5, 1, identity);
                GeometryContent trees = CreateVegetationGeometry("tree.tga", 12, 12, 0.5f, identity);
                GeometryContent cats = CreateVegetationGeometry("cat.tga", 5, 5, 0, identity);

                MeshContent vegetationMesh = new MeshContent { Name = "Billboards" };

                // Loop over all the existing geometry in this mesh.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    IList<int> indices = geometry.Indices;
                    IList<Vector3> positions = geometry.Vertices.Positions;
                    IList<Vector3> normals = geometry.Vertices.Channels.Get<Vector3>(VertexChannelNames.Normal());

                    // Loop over all the triangles in this piece of geometry.
                    for (int triangle = 0; triangle < indices.Count; triangle += 3)
                    {
                        // Look up the three indices for this triangle.
                        int i1 = indices[triangle];
                        int i2 = indices[triangle + 1];
                        int i3 = indices[triangle + 2];

                        // Create vegetation billboards to cover this triangle.
                        // A more sophisticated implementation would measure the
                        // size of the triangle to work out how many to create,
                        // but we don't bother since we happen to know that all
                        // our triangles are roughly the same size.
                        for (int count = 0; count < BillboardsPerTriangle; count++)
                        {
                            Vector3 position, normal;
                            
                            // Choose a random location on the triangle.
                            PickRandomPoint(positions[i1], positions[i2], positions[i3],
                                            normals[i1], normals[i2], normals[i3],
                                            out position, out normal);

                            // Randomly choose what type of billboard to create.
                            GeometryContent billboardType;

                            if (random.NextDouble() < TreeProbability)
                            {
                                billboardType = trees;

                                // As a special case, force trees to point straight
                                // upward, even if they are growing on a slope.
                                // That's what trees do in real life, after all!
                                normal = Vector3.Up;
                            }
                            else if (random.NextDouble() < CatProbability)
                            {
                                billboardType = cats;
                            }
                            else
                            {
                                billboardType = grass;
                            }
                            
                            // Add a new billboard to the output geometry.
                            GenerateBillboard(vegetationMesh, billboardType, position, normal);
                        }
                    }
                }

                // Add our new billboard geometry to the main mesh.
                vegetationMesh.Geometry.Add(grass);
                vegetationMesh.Geometry.Add(trees);
                vegetationMesh.Geometry.Add(cats);

                mesh.Children.Add(vegetationMesh);
            }
        }


        /// <summary>
        /// Helper function creates a new geometry object,
        /// and sets it to use our billboard effect.
        /// </summary>
        static GeometryContent CreateVegetationGeometry(string textureFilename,
                                                        float width, float height,
                                                        float windAmount,
                                                        ContentIdentity identity)
        {
            GeometryContent geometry = new GeometryContent();

            // Add the vertex channels needed for our billboard geometry.
            VertexChannelCollection channels = geometry.Vertices.Channels;

            // Add a vertex channel holding normal vectors.
            channels.Add<Vector3>(VertexChannelNames.Normal(), null);

            // Add a vertex channel holding texture coordinates.
            channels.Add<Vector2>(VertexChannelNames.TextureCoordinate(0), null);

            // Add a second texture coordinate channel, holding a per-billboard
            // random number. This is used to make each billboard come out a
            // slightly different size, and to animate at different speeds.
            channels.Add<float>(VertexChannelNames.TextureCoordinate(1), null);

            // Create a material for rendering the billboards.
            EffectMaterialContent material = new EffectMaterialContent();

            // Point the material at our custom billboard effect.
            string directory = Path.GetDirectoryName(identity.SourceFilename);

            string effectFilename = Path.Combine(directory, "Billboard.fx");

            material.Effect = new ExternalReference<EffectContent>(effectFilename);

            // Set the texture to be used by these billboards.
            textureFilename = Path.Combine(directory, textureFilename);

            material.Textures.Add("Texture", new ExternalReference<TextureContent>(textureFilename));

            // Set effect parameters describing the size and
            // wind sensitivity of these billboards.
            material.OpaqueData.Add("BillboardWidth", width);
            material.OpaqueData.Add("BillboardHeight", height);
            material.OpaqueData.Add("WindAmount", windAmount);

            geometry.Material = material;

            return geometry;
        }


        /// <summary>
        /// Helper function chooses a random location on a triangle.
        /// </summary>
        void PickRandomPoint(Vector3 position1, Vector3 position2, Vector3 position3,
                             Vector3 normal1, Vector3 normal2, Vector3 normal3,
                             out Vector3 randomPosition, out Vector3 randomNormal)
        {
            float a = (float)random.NextDouble();
            float b = (float)random.NextDouble();

            if (a + b > 1)
            {
                a = 1 - a;
                b = 1 - b;
            }

            randomPosition = Vector3.Barycentric(position1, position2, position3, a, b);

            randomNormal = Vector3.Barycentric(normal1, normal2, normal3, a, b);

            randomNormal.Normalize();
        }


        /// <summary>
        /// Helper function adds a single new billboard sprite to the output geometry.
        /// </summary>
        private void GenerateBillboard(MeshContent mesh, GeometryContent geometry,
                                       Vector3 position, Vector3 normal)
        {
            VertexContent vertices = geometry.Vertices;
            VertexChannelCollection channels = vertices.Channels;

            // First, create a vertex position entry for this billboard. Each
            // billboard is going to be rendered a quad, so we need to create four
            // vertices, but at this point we only have a single position that is
            // shared by all the vertices. The real position of each vertex will be
            // computed on the fly in the vertex shader, thus allowing us to
            // implement effects like making the billboard rotate to always face the
            // camera, and sway in the wind. As input the vertex shader only wants to
            // know the center point of the billboard, and that is the same for all
            // the vertices, so only a single position is needed here.
            int positionIndex = mesh.Positions.Count;

            mesh.Positions.Add(position);

            // Second, create the four vertices, all referencing the same position.
            int index = vertices.PositionIndices.Count;

            for (int i = 0; i < 4; i++)
            {
                vertices.Add(positionIndex);
            }

            // Third, add normal data for each of the four vertices. A normal for a
            // billboard is kind of a silly thing to define, since we are using a
            // 2D sprite to fake a complex 3D object that would in reality have many
            // different normals across its surface. Here we are just using a copy
            // of the normal from the ground underneath the billboard, which can be
            // used in our lighting computation to make the vegetation darker or
            // lighter depending on the lighting of the underlying landscape.
            VertexChannel<Vector3> normals;
            normals = channels.Get<Vector3>(VertexChannelNames.Normal());

            for (int i = 0; i < 4; i++)
            {
                normals[index + i] = normal;
            }

            // Fourth, add texture coordinates.
            VertexChannel<Vector2> texCoords;
            texCoords = channels.Get<Vector2>(VertexChannelNames.TextureCoordinate(0));

            texCoords[index + 0] = new Vector2(0, 0);
            texCoords[index + 1] = new Vector2(1, 0);
            texCoords[index + 2] = new Vector2(1, 1);
            texCoords[index + 3] = new Vector2(0, 1);

            // Fifth, add a per-billboard random value, which is the same for
            // all four vertices. This is used in the vertex shader to make
            // each billboard a slightly different size, and to be affected
            // differently by the wind animation.
            float randomValue = (float)random.NextDouble() * 2 - 1;

            VertexChannel<float> randomValues;
            randomValues = channels.Get<float>(VertexChannelNames.TextureCoordinate(1));

            for (int i = 0; i < 4; i++)
            {
                randomValues[index + i] = randomValue;
            }

            // Sixth and finally, add indices defining the pair of
            // triangles that will be used to render the billboard.
            geometry.Indices.Add(index + 0);
            geometry.Indices.Add(index + 1);
            geometry.Indices.Add(index + 2);

            geometry.Indices.Add(index + 0);
            geometry.Indices.Add(index + 2);
            geometry.Indices.Add(index + 3);
        }
    }
}
