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

namespace HeightmapCollisionPipeline
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
        public HeightMapInfoContent(PixelBitmapContent<float> bitmap,
            float terrainScale, float terrainBumpiness)
        {
            this.terrainScale = terrainScale;

            height = new float[bitmap.Width, bitmap.Height];
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    // the pixels will vary from 0 (black) to 1 (white).
                    // by subtracting 1, our heights vary from -1 to 0, which we then
                    // multiply by the "bumpiness" to get our final height.
                    height[x, y] = (bitmap.GetPixel(x, y) - 1) * terrainBumpiness;
                }
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
        }

        /// <summary>
        /// Tells the content pipeline what CLR type the
        /// data will be loaded into at runtime.
        /// </summary>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "HeightmapCollision.HeightMapInfo, " +
                "HeightmapCollision, Version=1.0.0.0, Culture=neutral";
        }


        /// <summary>
        /// Tells the content pipeline what worker type
        /// will be used to load the data.
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "HeightmapCollision.HeightMapInfoReader, " +
                "HeightmapCollision, Version=1.0.0.0, Culture=neutral";
        }
    }
}
