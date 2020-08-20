#region File Description
//-----------------------------------------------------------------------------
// DualTextureProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Collections.Generic;
#endregion

namespace ContentPipelineExtension
{
    /// <summary>
    /// Custom processor for building the model used in the DualTextureEffect demo.
    /// This code automatically applies the DualTextureEffect onto the model, and
    /// also sets it up to reference the correct light map overlay texture.
    /// </summary>
    [ContentProcessor]
    class DualTextureProcessor : ModelProcessor
    {
        protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            DualTextureMaterialContent dual = new DualTextureMaterialContent();

            // Copy the base (diffuse) texture from the existing BasicEffect material.
            dual.Texture = ((BasicMaterialContent)material).Texture;

            // Add the second lightmap texture.
            dual.Texture2 = new ExternalReference<TextureContent>("lightmap.tga");

            return base.ConvertMaterial(dual, context);
        }
    }
}
