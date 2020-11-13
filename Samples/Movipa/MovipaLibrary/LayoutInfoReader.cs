#region File Description
//-----------------------------------------------------------------------------
// LayoutInfoReader.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using TRead = MovipaLibrary.LayoutInfo;
#endregion

namespace MovipaLibrary
{
    /// <summary>
    /// This class restores the xnb file converted by the content pipeline
    /// to the value of LayoutInfoReader.
    /// This ContentTypeReader must be the same as the runtime 
    /// specified in ContentTypeWriter that was used to write the xnb 
    /// whose type is LayoutInfoReader.
    ///
    /// ContentPipelineで変換されたxnbファイルをLayoutInfoReaderの値に復元します。
    /// このContentTypeReaderはLayoutInfoReader型のxnbを書き込む際に使用した
    /// ContentTypeWriterで指定されたランタイムと同じである必要があります。
    /// </summary>
    public class LayoutInfoReader : ContentTypeReader<TRead>
    {
        /// <summary>
        /// Reads LayoutInfo from the xnb file.
        ///
        /// xnbファイルからLayoutInfoを読み込みます。
        /// </summary>
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            TRead info = new TRead();

            // Reads AnimationInfo.
            // 
            // AnimationInfoを読み込みます。
            AnimationInfoReader.ReadAnimationInfo(input, info);

            // Reads LayoutInfo.
            // 
            // LayoutInfoを読み込みます。
            info.SceneDataAsset = input.ReadString();
            info.Sequence = input.ReadString();

            return info;
        }
    }
}
