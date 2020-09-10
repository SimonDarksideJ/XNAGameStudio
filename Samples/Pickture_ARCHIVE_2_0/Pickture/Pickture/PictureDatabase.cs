#region File Description
//-----------------------------------------------------------------------------
// PictureDatabase.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.IO;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Pickture
{
    /// <summary>
    /// Manages the set of pictures available for use as puzzles.
    /// </summary>
    static class PictureDatabase
    {
        const int PictureCount = 11;

        static List<string> textureNames = new List<string>();
        static int nextIndex = 0;

        public static void Initialize()
        {
            // Simply load a fixed set of pictures with known names
            for (int i = 1; i <= PictureCount; i++)
                textureNames.Add("Pictures/" + i.ToString());

            // Sort the list randomly. This is done up front to ensure no repetitions
            // until a complete cycle of all photos has occured.
            RandomHelper.Randomize(textureNames);
        }

        /// <summary>
        /// Get the next range of pictures.
        /// </summary>
        /// <param name="numTextures">The number of pictures to be in the set.</param>
        /// <returns>A PictureSet with content references, but has not yet loaded
        /// the textures into memory.</returns>
        public static PictureSet GetNextPictureSet(int numTextures)
        {
            // Grab textures sequentially from a pre-randomized list
            string[] setNames = new string[numTextures];
            for (int i = 0; i < numTextures; i++)
            {
                setNames[i] = textureNames[nextIndex];
                // Loop around and reuse textures if necessary
                nextIndex = (nextIndex + 1) % textureNames.Count;
            }

            return new PictureSet(setNames);
        }

        /// <summary>
        /// Number of pictures in this database.
        /// </summary>
        public static int Count
        {
            get { return textureNames.Count; }
        }
    }
}
