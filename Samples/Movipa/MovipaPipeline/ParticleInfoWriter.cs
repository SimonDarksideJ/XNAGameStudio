#region File Description
//-----------------------------------------------------------------------------
// ParticleInfoWriter.cs
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

using TWrite = MovipaLibrary.ParticleInfo;
using TReader = MovipaLibrary.ParticleInfoReader;
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
    public class ParticleInfoWriter : ContentTypeWriter<TWrite>
    {
        /// <summary>
        /// Writes ParticleInfo to the xnb file.
        ///
        /// ParticleInfoをxnbファイルへ書き込みます。
        /// </summary>
        protected override void Write(ContentWriter output, TWrite value)
        {
            // Writes AnimationInfo.
            // 
            // AnimationInfoを書き込みます。
            AnimationInfoWriter.WriteAnimationInfo(output, value);

            // Writes ParticleInfo.
            // 
            // ParticleInfoを書き込みます。
            output.Write(value.ParticleTexture);
            output.Write(value.ParticleSize);
            output.Write(value.ParticleMax);
            output.Write(value.ParticleGenerateCount);
            output.Write(value.ParticleJumpPower);
            output.Write(value.ParticleMoveSpeed);
            output.Write(value.ParticleBoundRate);
            output.Write(value.ParticleGravity);
            output.WriteObject<Vector3>(value.CameraUpVector);
            output.WriteObject<Vector3>(value.CameraPosition);
            output.WriteObject<Vector3>(value.CameraLookAt);
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
