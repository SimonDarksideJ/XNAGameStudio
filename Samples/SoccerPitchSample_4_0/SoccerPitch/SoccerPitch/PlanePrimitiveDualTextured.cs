#region File Description
//-----------------------------------------------------------------------------
// PlanePrimitiveDualTextured.cs
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

    public class PlanePrimitiveDualTextured : ProceduralPrimitive<VertexPositionNormalDualTexture>
    {
        /// <summary>
        /// Constructs a new plane primitive, with the specified size, supporting tiling for both sets of UVs.
        /// </summary>
        public PlanePrimitiveDualTextured(GraphicsDevice graphicsDevice, float size, Vector2 Tiling1, Vector2 Tiling2)
        {
            VertexPositionNormalDualTexture vertex;
            vertex.Normal = new Vector3(0.0f, 1.0f, 0.0f);

            // Create each face in turn.
            Vector3 side1 = new Vector3(1, 0.0f, 0.0f);
            Vector3 side2 = new Vector3(0, 0.0f, 1.0f);

            // Six indices (two triangles) per face.
            AddIndex(CurrentVertex + 0);
            AddIndex(CurrentVertex + 2);
            AddIndex(CurrentVertex + 1);

            AddIndex(CurrentVertex + 0);
            AddIndex(CurrentVertex + 3);
            AddIndex(CurrentVertex + 2);

            // Four vertices per face.
            float halfSize = size / 2.0f;
            // Vertex 1
            vertex.Position = (-side1 + side2) * halfSize;
            vertex.TextureCoordinate0 = new Vector2(0.0f, 0.0f);
            vertex.TextureCoordinate1 = new Vector2(0.0f, 0.0f);
            AddVertex(vertex);

            // Vertex 2
            vertex.Position = (side1 + side2) * halfSize;
            vertex.TextureCoordinate0 = new Vector2(0.0f, Tiling1.Y);
            vertex.TextureCoordinate1 = new Vector2(Tiling2.X, 0.0f);
            AddVertex(vertex);

            // Vertex 3
            vertex.Position = (side1 - side2) * halfSize;
            vertex.TextureCoordinate0 = new Vector2(Tiling1.X, Tiling1.Y);
            vertex.TextureCoordinate1 = new Vector2(Tiling2.X, Tiling2.Y);
            AddVertex(vertex);

            // Vertex 4
            vertex.Position = (-side1 -side2) * halfSize;
            vertex.TextureCoordinate0 = new Vector2(Tiling1.X, 0.0f);
            vertex.TextureCoordinate1 = new Vector2(0.0f, Tiling2.Y);
            AddVertex(vertex);

            InitializePrimitive(graphicsDevice, VertexPositionNormalDualTexture.VertexDeclaration);
        }
    }


}
