#region File Description
//-----------------------------------------------------------------------------
// Cat.cs
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

namespace InverseKinematicsSample
{
    /// <summary>
    /// Entity that always faces the camera
    /// </summary>
    class Cat
    {
        #region Fields and Properties

        //Graphics related things used for drawing
        GraphicsDevice graphicsDevice;
        BasicEffect basicEffect;
        VertexPositionTexture[] vertices;

        /// <summary>
        /// Gets or sets the scale of the entity
        /// </summary>
        public float Scale { get; set; }

        /// <summary>
        /// Gets or sets the 3D position of the entity.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the orientation of this entity.
        /// </summary>
        public Vector3 Up { get; set; }

        /// <summary>
        /// Gets or sets the texture used to display this entity.
        /// </summary>
        public Texture2D Texture { get; set; }

        #endregion


        /// <summary>
        /// Creates a billboard sprite that the IK chains will attempt to reach
        /// </summary>
        public Cat(GraphicsDevice device)
        {
            Up = Vector3.Up;
            
            graphicsDevice = device;

            basicEffect = new BasicEffect(device);

            // Pre-allocate an array of six vertices.
            vertices = new VertexPositionTexture[6];

            vertices[0].Position = new Vector3(1, 1, 0);
            vertices[1].Position = new Vector3(-1, 1, 0);
            vertices[2].Position = new Vector3(-1, -1, 0);
            vertices[3].Position = new Vector3(1, 1, 0);
            vertices[4].Position = new Vector3(-1, -1, 0);
            vertices[5].Position = new Vector3(1, -1, 0);
        }

        /// <summary>
        /// Draw the billboard sprite with transparency.
        /// </summary>
        public void Draw(Vector3 cameraPosition, Matrix view, Matrix projection)
        {                                
            //Create the world transform for the billboarded cat
            Matrix world = Matrix.CreateTranslation(0, 1, 0) *
                           Matrix.CreateScale(Scale) *
                           Matrix.CreateConstrainedBillboard(Position, cameraPosition,
                                                             Up, null, null);

            //Draw the cat
            DrawQuad(Texture, 1, world, view, projection);
        }
        
        /// <summary>
        /// Draws a quadrilateral as part of the 3D world.
        /// </summary>
        public void DrawQuad(Texture2D texture, float textureRepeats,
                             Matrix world, Matrix view, Matrix projection)
        {
            // Set our effect to use the specified texture and camera matrices.
            basicEffect.Texture = texture;
            basicEffect.TextureEnabled = true;

            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;

            // Update our vertex array to use the specified number of texture repeats.
            vertices[0].TextureCoordinate = new Vector2(0, 0);
            vertices[1].TextureCoordinate = new Vector2(textureRepeats, 0);
            vertices[2].TextureCoordinate = new Vector2(textureRepeats, textureRepeats);
            vertices[3].TextureCoordinate = new Vector2(0, 0);
            vertices[4].TextureCoordinate = new Vector2(textureRepeats, textureRepeats);
            vertices[5].TextureCoordinate = new Vector2(0, textureRepeats);

            // Draw the quad.
            basicEffect.CurrentTechnique.Passes[0].Apply();            
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);
        }
    }
}
