#region File Description
//-----------------------------------------------------------------------------
// StageSettingReader.cs
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

using TRead = MovipaLibrary.StageSetting;
#endregion

namespace MovipaLibrary
{
    /// <summary>
    /// This class restores the xnb file converted by the content pipeline
    /// to the value of StageSettingReader.
    /// This ContentTypeReader must be the same as the runtime
    /// specified in ContentTypeWriter that was used to write the xnb
    /// whose type is StageSettingReader.
    ///
    /// ContentPipelineで変換されたxnbファイルをStageSettingReaderの値に復元します。
    /// このContentTypeReaderはStageSettingReader型のxnbを書き込む際に使用した
    /// ContentTypeWriterで指定されたランタイムと同じである必要があります。
    /// </summary>
    public class StageSettingReader : ContentTypeReader<TRead>
    {
        /// <summary>
        /// Reads StageSetting from the xnb file.
        ///
        /// xnbファイルからStageSettingを読み込みます。
        /// </summary>
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            TRead setting = new TRead();

            // Reads StageSetting.
            // 
            // StageSettingを読み込みます。
            setting.Mode = input.ReadObject<StageSetting.ModeList>();
            setting.Style = input.ReadObject<StageSetting.StyleList>();
            setting.Rotate = input.ReadObject<StageSetting.RotateMode>();
            setting.Movie = input.ReadString();
            setting.Divide = input.ReadObject<Point>();
            setting.TimeLimitString = input.ReadString();

            return setting;
        }
    }
}
