#region File Description
//-----------------------------------------------------------------------------
// StageSettingWriter.cs
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

using TWrite = MovipaLibrary.StageSetting;
using TReader = MovipaLibrary.StageSettingReader;
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
    public class StageSettingWriter : ContentTypeWriter<TWrite>
    {
        /// <summary>
        /// Writes StageSetting to the xnb file.
        ///
        /// StageSettingをxnbファイルへ書き込みます。
        /// </summary>
        protected override void Write(ContentWriter output, TWrite value)
        {
            // Writes StageSetting.
            // 
            // StageSettingを書き込みます。
            output.WriteObject<StageSetting.ModeList>(value.Mode);
            output.WriteObject<StageSetting.StyleList>(value.Style);
            output.WriteObject<StageSetting.RotateMode>(value.Rotate);
            output.Write(value.Movie);
            output.WriteObject<Point>(value.Divide);
            output.Write(value.TimeLimitString);
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
