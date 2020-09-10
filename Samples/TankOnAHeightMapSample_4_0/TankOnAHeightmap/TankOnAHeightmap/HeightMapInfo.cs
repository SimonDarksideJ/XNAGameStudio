#region File Description
//-----------------------------------------------------------------------------
// HeightMapInfo.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
#endregion

namespace TanksOnAHeightmap
{
    /// <summary>
    /// HeightMapInfo is a collection of data about the heightmap. It includes
    /// information about how high the terrain is, and how far apart each vertex is.
    /// It also has several functions to get information about the heightmap, including
    /// its height and normal at different points, and whether a point is on the 
    /// heightmap. It is the runtime equivalent of HeightMapInfoContent.
    /// </summary>
    public class HeightMapInfo
    {
        #region Private fields


        // TerrainScale is the distance between each entry in the Height property.
        // For example, if TerrainScale is 30, Height[0,0] and Height[1,0] are 30
        // units apart.        
        private float terrainScale;

        // This 2D array of floats tells us the height that each position in the 
        // heightmap is.
        private float[,] heights;

        private Vector3[,] normals;

        // the position of the heightmap's -x, -z corner, in worldspace.
        private Vector3 heightmapPosition;

        // the total width of the heightmap, including terrainscale.
        private float heightmapWidth;

        // the total height of the height map, including terrainscale.
        private float heightmapHeight;


        #endregion


        // the constructor will initialize all of the member variables.
        public HeightMapInfo(float[,] heights, Vector3[,] normals, float terrainScale)
        {
            if (heights == null)
            {
                throw new ArgumentNullException("heights");
            }
            if (normals == null)
            {
                throw new ArgumentNullException("normals");
            }

            this.terrainScale = terrainScale;
            this.heights = heights;
            this.normals = normals;

            heightmapWidth = (heights.GetLength(0) - 1) * terrainScale;
            heightmapHeight = (heights.GetLength(1) - 1) * terrainScale;

            heightmapPosition.X = -(heights.GetLength(0) - 1) / 2.0f * terrainScale;
            heightmapPosition.Z = -(heights.GetLength(1) - 1) / 2.0f * terrainScale;
        }


        // This function takes in a position, and tells whether or not the position is 
        // on the heightmap.
        public bool IsOnHeightmap(Vector3 position)
        {
            // first we'll figure out where on the heightmap "position" is...
            Vector3 positionOnHeightmap = position - heightmapPosition;

            // ... and then check to see if that value goes outside the bounds of the
            // heightmap.
            return (positionOnHeightmap.X > 0 &&
                positionOnHeightmap.X < heightmapWidth &&
                positionOnHeightmap.Z > 0 &&
                positionOnHeightmap.Z < heightmapHeight);
        }

        // This function takes in a position, and has two out parameters: the 
        // heightmap's height and normal at that point. Be careful - this function will 
        // throw an IndexOutOfRangeException if position isn't on the heightmap!        
        public void GetHeightAndNormal
            (Vector3 position, out float height, out Vector3 normal)
        {
            // the first thing we need to do is figure out where on the heightmap
            // "position" is. This'll make the math much simpler later.
            Vector3 positionOnHeightmap = position - heightmapPosition;

            // we'll use integer division to figure out where in the "heights" array
            // positionOnHeightmap is. Remember that integer division always rounds
            // down, so that the result of these divisions is the indices of the "upper
            // left" of the 4 corners of that cell.
            int left, top;
            left = (int)positionOnHeightmap.X / (int)terrainScale;
            top = (int)positionOnHeightmap.Z / (int)terrainScale;

            // next, we'll use modulus to find out how far away we are from the upper
            // left corner of the cell. Mod will give us a value from 0 to terrainScale,
            // which we then divide by terrainScale to normalize 0 to 1.
            float xNormalized = (positionOnHeightmap.X % terrainScale) / terrainScale;
            float zNormalized = (positionOnHeightmap.Z % terrainScale) / terrainScale;

            // Now that we've calculated the indices of the corners of our cell, and
            // where we are in that cell, we'll use bilinear interpolation to calculuate
            // our height. This process is best explained with a diagram, so please see
            // the accompanying doc for more information.
            // First, calculate the heights on the bottom and top edge of our cell by
            // interpolating from the left and right sides.
            float topHeight = MathHelper.Lerp(
                heights[left, top],
                heights[left + 1, top],
                xNormalized);

            float bottomHeight = MathHelper.Lerp(
                heights[left, top + 1],
                heights[left + 1, top + 1],
                xNormalized);

            // next, interpolate between those two values to calculate the height at our
            // position.
            height = MathHelper.Lerp(topHeight, bottomHeight, zNormalized);

            // We'll repeat the same process to calculate the normal.
            Vector3 topNormal = Vector3.Lerp(
                normals[left, top],
                normals[left + 1, top],
                xNormalized);

            Vector3 bottomNormal = Vector3.Lerp(
                normals[left, top + 1],
                normals[left + 1, top + 1],
                xNormalized);

            normal = Vector3.Lerp(topNormal, bottomNormal, zNormalized);
            normal.Normalize();
        }
    }



    /// <summary>
    /// This class will load the HeightMapInfo when the game starts. This class needs 
    /// to match the HeightMapInfoWriter.
    /// </summary>
    public class HeightMapInfoReader : ContentTypeReader<HeightMapInfo>
    {
        protected override HeightMapInfo Read(ContentReader input,
            HeightMapInfo existingInstance)
        {
            float terrainScale = input.ReadSingle();
            int width = input.ReadInt32();
            int height = input.ReadInt32();
            float[,] heights = new float[width, height];
            Vector3[,] normals = new Vector3[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    heights[x, z] = input.ReadSingle();
                }
            }
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    normals[x, z] = input.ReadVector3();
                }
            }
            return new HeightMapInfo(heights, normals, terrainScale);
        }
    }
}
