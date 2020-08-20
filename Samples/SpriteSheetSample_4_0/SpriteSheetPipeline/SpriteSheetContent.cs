#region File Description
//-----------------------------------------------------------------------------
// SpriteSheetContent.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
#endregion

namespace SpriteSheetPipeline
{
    /// <summary>
    /// Build-time type used to hold the output data from the SpriteSheetProcessor.
    /// This is serialized into XNB format, then at runtime, the ContentManager
    /// loads the data into a SpriteSheet object.
    /// </summary>
    [ContentSerializerRuntimeType("SpriteSheetRuntime.SpriteSheet, SpriteSheetRuntime")]
    public class SpriteSheetContent
    {
        // Single texture contains many separate sprite images.
        public Texture2DContent Texture = new Texture2DContent();

        // Remember where in the texture each sprite has been placed.
        public List<Rectangle> SpriteRectangles = new List<Rectangle>();

        // Store the original sprite filenames, so we can look up sprites by name.
        public Dictionary<string, int> SpriteNames = new Dictionary<string, int>();
    }
}
