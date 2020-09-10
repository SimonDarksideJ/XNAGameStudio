#region File Description
//-----------------------------------------------------------------------------
// EnvironmentMappedMaterialProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.IO;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
#endregion

namespace CustomModelEffectPipeline
{
    /// <summary>
    /// Custom content pipeline processor derives from the built-in
    /// MaterialProcessor. This changes the material to use our custom
    /// environment mapping effect, and also builds the environment map
    /// texture in a special way.
    /// </summary>
    [ContentProcessor]
    public class EnvironmentMappedMaterialProcessor : MaterialProcessor
    {
        private string environmentMap = "seattle.bmp";
        [DisplayName("Environment Map")]
        [DefaultValue("seattle.bmp")]
        [Description("The environment map applied to the model.")]
        public string EnvironmentMap
        {
            get { return environmentMap; }
            set { environmentMap = value; }
        }


        /// <summary>
        /// Converts a material.
        /// </summary>
        public override MaterialContent Process(MaterialContent input,
                                                ContentProcessorContext context)
        {
            // Create a new effect material.
            EffectMaterialContent customMaterial = new EffectMaterialContent();

            // Point the new material at our custom effect file.
            string effectFile = Path.GetFullPath("EnvironmentMap.fx");

            customMaterial.Effect = new ExternalReference<EffectContent>(effectFile);

            // Copy texture data across from the original material.
            BasicMaterialContent basicMaterial = (BasicMaterialContent)input;

            if (basicMaterial.Texture != null)
            {
                customMaterial.Textures.Add("Texture", basicMaterial.Texture);
                customMaterial.OpaqueData.Add("TextureEnabled", true);
            }

            // Add the reflection texture.
            string envmap = Path.GetFullPath(EnvironmentMap);

            customMaterial.Textures.Add("EnvironmentMap",
                                        new ExternalReference<TextureContent>(envmap));

            // Chain to the base material processor.
            return base.Process(customMaterial, context);
        }


        /// <summary>
        /// Builds a texture for use by this material.
        /// </summary>
        protected override ExternalReference<TextureContent> BuildTexture(
                                            string textureName,
                                            ExternalReference<TextureContent> texture,
                                            ContentProcessorContext context)
        {
            // Use our custom CubemapProcessor for the environment map texture.
            if (textureName == "EnvironmentMap")
            {
                return context.BuildAsset<TextureContent,
                                          TextureContent>(texture, "CubemapProcessor");
            }

            // Apply default processing to all other textures.
            return base.BuildTexture(textureName, texture, context);
        }
    }
}
