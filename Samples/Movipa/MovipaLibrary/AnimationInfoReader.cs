#region File Description
//-----------------------------------------------------------------------------
// AnimationInfoReader.cs
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

using TRead = MovipaLibrary.AnimationInfo;
#endregion

namespace MovipaLibrary
{
    /// <summary>
    /// This class restores the xnb file converted by 
    /// the content pipeline to the value of AnimationInfoReader.
    /// This ContentTypeReader must be the same as the runtime 
    /// specified in ContentTypeWriter that was used to write the xnb 
    /// whose type is AnimationInfoReader.
    ///
    /// ContentPipelineで変換されたxnbファイルをAnimationInfoReaderの値に復元します。
    /// このContentTypeReaderはAnimationInfoReader型のxnbを書き込む際に使用した
    /// ContentTypeWriterで指定されたランタイムと同じである必要があります。
    /// </summary>
    public class AnimationInfoReader : ContentTypeReader<TRead>
    {
        /// <summary>
        /// Reads AnimationInfo from the xnb file.
        ///
        /// xnbファイルからAnimationInfoを読み込みます。
        /// </summary>
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            TRead info = new TRead();

            // Reads AnimationInfo.
            // 
            // AnimationInfoを読み込みます。
            ReadAnimationInfo(input, info);

            return info;
        }


        /// <summary>
        /// Reads AnimationInfo.
        ///
        /// AnimationInfoを読み込みます。
        /// </summary>
        public static void ReadAnimationInfo(ContentReader input, TRead info)
        {
            info.Category = input.ReadObject<AnimationInfo.AnimationInfoCategory>();
            info.Name = input.ReadString();
            info.Size = input.ReadObject<Point>();
        }
    }
}
