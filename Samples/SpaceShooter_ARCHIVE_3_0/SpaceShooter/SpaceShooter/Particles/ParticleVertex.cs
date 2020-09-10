#region File Description
//-----------------------------------------------------------------------------
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


    /// <summary>
    /// Custom vertex structure for drawing point sprite particles.
    /// </summary>
    struct ParticleVertex
    {
        // Stores the starting position of the particle.
        public Vector3 Position;

        // Stores the starting velocity of the particle.
        public Vector3 Velocity;

        // Four random values, used to make each particle look slightly different.
        public Color Random;

        // The time (in seconds) at which this particle was created.
        public float Time;


        // Describe the layout of this vertex structure.
        public static readonly VertexElement[] VertexElements =
        {
            new VertexElement(0, 0, VertexElementFormat.Vector3,
                                    VertexElementMethod.Default,
                                    VertexElementUsage.Position, 0),

            new VertexElement(0, 12, VertexElementFormat.Vector3,
                                     VertexElementMethod.Default,
                                     VertexElementUsage.Normal, 0),

            new VertexElement(0, 24, VertexElementFormat.Color,
                                     VertexElementMethod.Default,
                                     VertexElementUsage.Color, 0),

            new VertexElement(0, 28, VertexElementFormat.Single,
                                     VertexElementMethod.Default,
                                     VertexElementUsage.TextureCoordinate, 0),
        };


        // Describe the size of this vertex structure.
        public const int SizeInBytes = 32;
    }
