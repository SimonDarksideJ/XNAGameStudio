#region File Description
//-----------------------------------------------------------------------------
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
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
    public class ShipModelProcessor : ModelProcessor
    {
        /// <summary>
        /// Use our custom EnvironmentMappedMaterialProcessor
        /// to convert all the materials on this model.
        /// </summary>
        protected override MaterialContent ConvertMaterial(MaterialContent material,
                                                         ContentProcessorContext context)
        {
            return context.Convert<MaterialContent, MaterialContent>(material,
                                                "ShipMaterialProcessor");
        }
    }
}
