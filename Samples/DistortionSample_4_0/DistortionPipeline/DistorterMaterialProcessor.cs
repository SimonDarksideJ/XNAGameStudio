#region File Description
//-----------------------------------------------------------------------------
// DistorterMaterialProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace DistortionPipeline
{
    /// <summary>
    /// Applies the Distorters effect and builds a distortion map if one is specified.
    /// Note that the distortion map is set as a parameter in the model file.
    /// </summary>
    [ContentProcessor]
    class DistorterMaterialProcessor : MaterialProcessor
    {
        /// <summary>
        /// Name of the effect parameter set for the displacement height map texture
        /// </summary>
        public const string DisplacementMapKey = "DisplacementMap";

        /// <summary>
        /// 
        /// </summary>        
        public override MaterialContent Process(MaterialContent input,
            ContentProcessorContext context)
        {
            // Reference the Distorters effect file
            EffectMaterialContent effect = new EffectMaterialContent();
            effect.Effect =
                new ExternalReference<EffectContent>("Distorters.fx");

            // If the model specifies a displacement map, carry it over
            ExternalReference<TextureContent> displacementMap;
            if (input.Textures.TryGetValue(DisplacementMapKey, out displacementMap))
            {
                effect.Textures.Add(DisplacementMapKey, displacementMap);
            }

            // Continue processing the distorter effect with the default effect behavior
            return base.Process(effect, context);
        }

        /// <summary>
        /// Builds Distorter textures with the DisplacementMapProcessor
        /// </summary>        
        protected override ExternalReference<TextureContent> BuildTexture(
            string textureName, ExternalReference<TextureContent> texture,
            ContentProcessorContext context)
        {
            return context.BuildAsset<TextureContent,
                TextureContent>(texture, "DisplacementMapProcessor");
        }
    }
}
