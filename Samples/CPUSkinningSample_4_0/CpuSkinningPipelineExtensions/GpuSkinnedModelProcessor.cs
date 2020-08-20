#region File Description
//-----------------------------------------------------------------------------
// GpuSkinnedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using CpuSkinningDataTypes;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;

namespace CpuSkinningPipelineExtensions
{
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName = "GPU Skinned Model")]
    public class GpuSkinnedModelProcessor : ModelProcessor
    {
        /// <summary>
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelContent object with embedded animation data.
        /// </summary>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            SkinningData skinningData = SkinningHelpers.GetSkinningData(input, context, SkinnedEffect.MaxBones);
            ModelContent model = base.Process(input, context);
            model.Tag = skinningData;
            return model;
        }
    }
}
