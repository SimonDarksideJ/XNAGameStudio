#region File Description
//-----------------------------------------------------------------------------
// RenderingInfoReader.cs
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

using TRead = MovipaLibrary.RenderingInfo;
#endregion

namespace MovipaLibrary
{
    /// <summary>
    /// This class restores the xnb file converted by the content pipeline
    /// to the value of RenderingInfoReader.
    /// This ContentTypeReader must be the same as the runtime 
    /// specified in ContentTypeWriter that was used to write the xnb 
    /// whose type is RenderingInfoReader.
    ///
    /// ContentPipelineで変換されたxnbファイルをRenderingInfoReaderの値に復元します。
    /// このContentTypeReaderはRenderingInfoReader型のxnbを書き込む際に使用した
    /// ContentTypeWriterで指定されたランタイムと同じである必要があります。
    /// </summary>
    public class RenderingInfoReader : ContentTypeReader<TRead>
    {
        /// <summary>
        /// Reads RenderingInfo from the xnb file.
        ///
        /// xnbファイルからRenderingInfoを読み込みます。
        /// </summary>
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            TRead info = new TRead();

            // Reads AnimationInfo.
            // 
            // AnimationInfoを読み込みます。
            AnimationInfoReader.ReadAnimationInfo(input, info);

            // Reads RenderingInfo.
            // 
            // RenderingInfoを読み込みます。
            info.Format = input.ReadString();
            info.TotalTexture = input.ReadUInt32();
            info.TotalFrame = input.ReadUInt32();
            info.ImageSize = input.ReadObject<Point>();
            info.FrameRate = input.ReadUInt32();

            return info;
        }
    }
}
