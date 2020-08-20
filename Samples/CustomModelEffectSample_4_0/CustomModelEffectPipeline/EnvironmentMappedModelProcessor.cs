#region File Description
//-----------------------------------------------------------------------------
// EnvironmentMappedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
#endregion

namespace CustomModelEffectPipeline
{
    /// <summary>
    /// Custom content pipeline processor derives from the built-in
    /// ModelProcessor, extending it to apply an environment mapping
    /// effect to the model as part of the build process.
    /// </summary>
    [ContentProcessor]
    public class EnvironmentMappedModelProcessor : ModelProcessor
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
        /// Use our custom EnvironmentMappedMaterialProcessor
        /// to convert all the materials on this model.
        /// </summary>
        protected override MaterialContent ConvertMaterial(MaterialContent material,
                                                         ContentProcessorContext context)
        {
            OpaqueDataDictionary processorParameters = new OpaqueDataDictionary();
            processorParameters["EnvironmentMap"] = EnvironmentMap;
            processorParameters["ColorKeyColor"] = ColorKeyColor;
            processorParameters["ColorKeyEnabled"] = ColorKeyEnabled;
            processorParameters["TextureFormat"] = TextureFormat;
            processorParameters["GenerateMipmaps"] = GenerateMipmaps;
            processorParameters["ResizeTexturesToPowerOfTwo"] =
                ResizeTexturesToPowerOfTwo;

            return context.Convert<MaterialContent, MaterialContent>(material,
                "EnvironmentMappedMaterialProcessor", processorParameters);

        }
    }
}
