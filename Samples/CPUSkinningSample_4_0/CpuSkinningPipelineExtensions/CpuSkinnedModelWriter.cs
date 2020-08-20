#region File Description
//-----------------------------------------------------------------------------
// CpuSkinnedModelWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace CpuSkinningPipelineExtensions
{
    /// <summary>
    /// Writes out a CpuSkinnedModelContent object to an XNB file to be read in as
    /// a CpuSkinnedModel.
    /// </summary>
    [ContentTypeWriter]
    class CpuSkinnedModelWriter : ContentTypeWriter<CpuSkinnedModelContent>
    {
        protected override void Write(ContentWriter output, CpuSkinnedModelContent value)
        {
            output.WriteObject(value.ModelParts);
            output.WriteObject(value.SkinningData);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "CpuSkinningDataTypes.CpuSkinnedModel, CpuSkinningDataTypes";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "CpuSkinningDataTypes.CpuSkinnedModelReader, CpuSkinningDataTypes";
        }
    }

    /// <summary>
    /// Writes out a CpuSkinnedModelPartContent object to be read in as a CpuSkinnedModelPart
    /// </summary>
    [ContentTypeWriter]
    class CpuSkinnedModelPartWriter : ContentTypeWriter<CpuSkinnedModelPartContent>
    {
        protected override void Write(ContentWriter output, CpuSkinnedModelPartContent value)
        {
            output.Write(value.TriangleCount);
            output.WriteObject(value.Vertices);
            output.WriteObject(value.IndexCollection);
            output.WriteSharedResource(value.Material);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "CpuSkinningDataTypes.CpuSkinnedModelPart, CpuSkinningDataTypes";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "CpuSkinningDataTypes.CpuSkinnedModelPartReader, CpuSkinningDataTypes";
        }
    }
}
