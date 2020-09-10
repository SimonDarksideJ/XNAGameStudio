#region File Description
//-----------------------------------------------------------------------------
// DistorterModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace DistortionPipeline
{
    /// <summary>
    /// Processor for models which will be used as distorters. This will invoke
    /// the DistorterMaterialProcessor to apply the Distorter.fx effect.
    /// </summary>
    [ContentProcessor]
    class DistorterModelProcessor : ModelProcessor
    {
        protected override MaterialContent ConvertMaterial(MaterialContent material,
            ContentProcessorContext context)
        {
            return context.Convert<MaterialContent, MaterialContent>(material,
                                    "DistorterMaterialProcessor");
        }
    }

}
