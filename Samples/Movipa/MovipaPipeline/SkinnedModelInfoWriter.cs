#region File Description
//-----------------------------------------------------------------------------
// SkinnedModelInfoWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using MovipaLibrary;

using TWrite = MovipaLibrary.SkinnedModelInfo;
using TReader = MovipaLibrary.SkinnedModelInfoReader;
#endregion

namespace MovipaPipeline
{
    /// <summary>
    /// This class writes the data passed from ContentImpoter to the xnb file.
    /// In the game, the ContentTypeReader specified in GetRuntimeReader is used
    /// to read data.
    ///
    /// ContentImpoterから渡されたデータをxnbファイルに書き込みます。
    /// ゲーム内ではGetRuntimeReaderで指定したContentTypeReaderを
    /// 使用して読み込み処理が行われます。
    /// </summary>
    [ContentTypeWriter]
    public class SkinnedModelInfoWriter : ContentTypeWriter<TWrite>
    {
        /// <summary>
        /// Writes SkinnedModelInfo to the xnb file.
        ///
        /// SkinnedModelInfoをxnbファイルへ書き込みます。
        /// </summary>
        protected override void Write(ContentWriter output, TWrite value)
        {
            // Writes SkinnedModelInfo.
            // 
            // SkinnedModelInfoを書き込みます。
            output.Write(value.ModelAsset);
            output.Write(value.AnimationClip);
            output.WriteObject<Vector3>(value.Position);
        }


        /// <summary>
        /// Specifies the ContentTypeReader to be used.
        ///
        /// 使用するContentTypeReaderを指定します。
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(TReader).AssemblyQualifiedName;
        }
    }
}
