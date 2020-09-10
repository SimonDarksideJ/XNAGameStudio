#region File Description
//-----------------------------------------------------------------------------
// HeightMapInfoContent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework;
#endregion

namespace TanksOnAHeightmapPipeline
{
    /// <summary>
    /// HeightMapInfoContent contains information about a size and heights of a
    /// heightmap. When the game is being built, it is constructed by the
    /// TerrainProcessor, and attached to the finished terrain's Tag. When the game is
    /// run, it will be read in as a HeightMapInfo.
    /// </summary>
    public class HeightMapInfoContent
    {
        /// <summary>
        /// This propery is a 2D array of floats, and tells us the height that each
        /// position in the heightmap is.
        /// </summary>
        public float[,] Height
        {
            get { return height; }
        }
        float[,] height;

        /// <summary>
        /// This property is a 2D array of Vector3s, and tells us the normal that each
        /// position in the heightmap is.
        /// </summary>
        public Vector3[,] Normals
        {
            get { return normals; }
            set { normals = value; }
        }
        private Vector3[,] normals;


        /// <summary>
        /// TerrainScale is the distance between each entry in the Height property.
        /// For example, if TerrainScale is 30, Height[0,0] and Height[1,0] are 30
        /// units apart.
        /// </summary>
        public float TerrainScale
        {
            get { return terrainScale; }
        }
        private float terrainScale;

        /// <summary>
        /// This constructor will initialize the height array from the values in the 
        /// bitmap. Each pixel in the bitmap corresponds to one entry in the height
        /// array.
        /// </summary>
        public HeightMapInfoContent(MeshContent terrainMesh, float terrainScale, 
            int terrainWidth, int terrainLength)
        {
            // validate the parameters
            if (terrainMesh == null)
            {
                throw new ArgumentNullException("terrainMesh");
            }
            if (terrainWidth <= 0)
            {
                throw new ArgumentOutOfRangeException("terrainWidth");
            }
            if (terrainLength <= 0)
            {
                throw new ArgumentOutOfRangeException("terrainLength");
            }

            this.terrainScale = terrainScale;

            // create new arrays of the requested size.
            height = new float[terrainWidth, terrainLength];
            normals = new Vector3[terrainWidth, terrainLength];

            // to fill those arrays, we'll look at the position and normal data
            // contained in the terrainMesh.
            GeometryContent geometry = terrainMesh.Geometry[0];
            // we'll go through each vertex....
            for (int i = 0; i < geometry.Vertices.VertexCount; i++)
            {
                // ... and look up its position and normal.
                Vector3 position = geometry.Vertices.Positions[i];
                Vector3 normal = (Vector3)geometry.Vertices.Channels
                    [VertexChannelNames.Normal()][i];

                // from the position's X and Z value, we can tell what X and Y
                // coordinate of the arrays to put the height and normal into.
                int arrayX = (int)
                    ((position.X / terrainScale) + (terrainWidth - 1) / 2.0f);
                int arrayY = (int)
                    ((position.Z / terrainScale) + (terrainLength - 1) / 2.0f);

                height[arrayX, arrayY] = position.Y;
                normals[arrayX, arrayY] = normal;
            }
        }
    }


    /// <summary>
    /// A TypeWriter for HeightMapInfo, which tells the content pipeline how to save the
    /// data in HeightMapInfo. This class should match HeightMapInfoReader: whatever the
    /// writer writes, the reader should read.
    /// </summary>
    [ContentTypeWriter]
    public class HeightMapInfoWriter : ContentTypeWriter<HeightMapInfoContent>
    {
        protected override void Write(ContentWriter output, HeightMapInfoContent value)
        {
            output.Write(value.TerrainScale);

            output.Write(value.Height.GetLength(0));
            output.Write(value.Height.GetLength(1));
            foreach (float height in value.Height)
            {
                output.Write(height);
            }
            foreach (Vector3 normal in value.Normals)
            {
                output.Write(normal);
            }
        }

        /// <summary>
        /// Tells the content pipeline what CLR type the
        /// data will be loaded into at runtime.
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "TanksOnAHeightmap.HeightMapInfo, " +
                "TanksOnAHeightmap, Version=1.0.0.0, Culture=neutral";
        }


        /// <summary>
        /// Tells the content pipeline what worker type
        /// will be used to load the data.
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "TanksOnAHeightmap.HeightMapInfoReader, " +
                "TanksOnAHeightmap, Version=1.0.0.0, Culture=neutral";
        }
    }
}
