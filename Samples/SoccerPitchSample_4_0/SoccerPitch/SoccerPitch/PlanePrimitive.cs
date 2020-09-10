#region File Description
//-----------------------------------------------------------------------------
// PlanePrimitive.cs
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
    /// Textured plane primitive class for drawing with Tiled texturing.
    /// </summary>
    public class PlanePrimitive : ProceduralPrimitive<VertexPositionNormal>
    {
        const float defaultPlaneSize = 1.0f;
        /// <summary>
        /// Constructs a new cube primitive, using default settings.5
        /// </summary>
        public PlanePrimitive(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, defaultPlaneSize)
        {
        }

        /// <summary>
        /// Constructs a new plane primitive, with the specified size but without UVs.
        /// </summary>
        public PlanePrimitive(GraphicsDevice graphicsDevice, float size)
        {
            VertexPositionNormal vertex;
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
            AddVertex(vertex);

            // Vertex 2
            vertex.Position = (side1 + side2) * halfSize;
            AddVertex(vertex);

            // Vertex 3
            vertex.Position = (side1 -side2) * halfSize;
            AddVertex(vertex);

            // Vertex 4
            vertex.Position = (-side1 -side2) * halfSize;
            AddVertex(vertex);

            InitializePrimitive(graphicsDevice, VertexPositionNormal.VertexDeclaration);
        }
    }

}
