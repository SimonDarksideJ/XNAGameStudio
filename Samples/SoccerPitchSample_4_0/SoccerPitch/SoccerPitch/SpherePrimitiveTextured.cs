#region File Description
//-----------------------------------------------------------------------------
// SpherePrimitiveTextured.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SoccerPitch
{

    /// <summary>
    /// Geometric primitive class for drawing a textured sphere. Creates default UVs for the sphere.
    /// </summary>
    public class SpherePrimitiveTextured : ProceduralPrimitive<VertexPositionNormalTexture>
    {
        const int defaultSphereTessellation = 6;
        const float defaultSphereSize = 1.0f;

        /// <summary>
        /// Constructs a new sphere primitive, using default settings.
        /// </summary>
        public SpherePrimitiveTextured(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, defaultSphereSize, defaultSphereTessellation)
        {
        }

        /// <summary>
        /// Constructs a new sphere primitive,
        /// with the specified size and tessellation level.
        /// </summary>
        public SpherePrimitiveTextured(GraphicsDevice graphicsDevice,
                               float diameter, int tessellation)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation must be greater than 3");

            VertexPositionNormalTexture vertex;

            int verticalSegments = tessellation;
            int horizontalSegments = tessellation * 2;

            float radius = diameter / 2;

            vertex = new VertexPositionNormalTexture();

            // Start with a single vertex at the bottom of the sphere.
            vertex.Position = Vector3.Down * radius;
            vertex.Normal = Vector3.Down;
            vertex.TextureCoordinate.X = Vector3.Down.X;
            vertex.TextureCoordinate.Y = Vector3.Down.Y;
            AddVertex(vertex);

            // Create rings of vertices at progressively higher latitudes.
            for (int i = 0; i < verticalSegments - 1; i++)
            {
                float latitude = ((i + 1) * MathHelper.Pi /
                                            verticalSegments) - MathHelper.PiOver2;

                float dy = (float)Math.Sin(latitude);
                float dxz = (float)Math.Cos(latitude);

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j < horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPi / horizontalSegments;

                    float dx = (float)Math.Cos(longitude) * dxz;
                    float dz = (float)Math.Sin(longitude) * dxz;

                    Vector3 normal = new Vector3(dx, dy, dz);
                    vertex.Position = normal * radius;
                    vertex.TextureCoordinate.X = normal.X;
                    vertex.TextureCoordinate.Y = normal.Y;
                    vertex.Normal = normal;
                    AddVertex(vertex);
                }
            }

            // Finish with a single vertex at the top of the sphere.
            vertex.Position = Vector3.Up * radius;
            vertex.Normal = Vector3.Up;
            vertex.TextureCoordinate.X = Vector3.Up.X;
            vertex.TextureCoordinate.Y = Vector3.Up.Y;
            AddVertex(vertex);

            // Create a fan connecting the bottom vertex to the bottom latitude ring.
            for (int i = 0; i < horizontalSegments; i++)
            {
                AddIndex(0);
                AddIndex(1 + (i + 1) % horizontalSegments);
                AddIndex(1 + i);
            }

            // Fill the sphere body with triangles joining each pair of latitude rings.
            for (int i = 0; i < verticalSegments - 2; i++)
            {
                for (int j = 0; j < horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % horizontalSegments;

                    AddIndex(1 + i * horizontalSegments + j);
                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);

                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);
                }
            }

            // Create a fan connecting the top vertex to the top latitude ring.
            for (int i = 0; i < horizontalSegments; i++)
            {
                AddIndex(CurrentVertex - 1);
                AddIndex(CurrentVertex - 2 - (i + 1) % horizontalSegments);
                AddIndex(CurrentVertex - 2 - i);
            }

            InitializePrimitive(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration);
        }
    }


 
}
