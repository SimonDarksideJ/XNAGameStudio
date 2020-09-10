#region File Description
//-----------------------------------------------------------------------------
// SkyContent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content;
#endregion

namespace GeneratedGeometryPipeline
{
    /// <summary>
    /// Design time class for holding a skydome. This is created by
    /// the SkyProcessor, then written out to a compiled XNB file.
    /// At runtime, the data is loaded into the runtime Sky class.
    /// </summary>
    [ContentSerializerRuntimeType("GeneratedGeometry.Sky, GeneratedGeometry")]
    public class SkyContent
    {
        public ModelContent Model;
        public TextureContent Texture;
    }
}
