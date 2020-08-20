#region File Description
//-----------------------------------------------------------------------------
// ShaderMaterialProcessor.cs
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
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace ShaderEffectPipeline
{
    /// <summary>
    /// The ShaderMaterialProcessor is very simple.  It extends the regular
    /// MaterialProcessor, overriding BuildTexture so that normal maps can go through
    /// the ShaderTextureProcessor and be converted to a signed normalmap format.
    /// </summary>
    [ContentProcessor]
    public class ShaderMaterialProcessor : MaterialProcessor
    {
        protected override ExternalReference<TextureContent> BuildTexture
            (string textureName, ExternalReference<TextureContent> texture,
            ContentProcessorContext context)
        {
            if (textureName == ShaderModelProcessor.NormalMapKey)
            {
                // put the normal map through the special ShaderModelProcessor,
                // which will convert it to a signed format.
                return context.BuildAsset<TextureContent, TextureContent>(texture,
                    typeof(ShaderTextureProcessor).Name);
            }

            // Apply default processing to all other textures.
            return base.BuildTexture(textureName, texture, context);
        }
    }
}