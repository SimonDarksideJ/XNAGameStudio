#region File Description
//-----------------------------------------------------------------------------
// SkinnedModelAnimationInfoReader.cs
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

using TRead = MovipaLibrary.SkinnedModelAnimationInfo;
#endregion

namespace MovipaLibrary
{
    /// <summary>
    /// This class restores the xnb file converted by the content pipeline
    /// to the value of SkinnedModelAnimationInfoReader.
    /// This ContentTypeReader must be the same as the runtime 
    /// specified in ContentTypeWriter that was used to write the xnb
    /// whose type is SkinnedModelAnimationInfoReader.
    ///
    /// ContentPipeline‚Å•ÏŠ·‚³‚ê‚½xnbƒtƒ@ƒCƒ‹‚ğSkinnedModelAnimationInfoReader‚
    /// Ì’l‚É•œŒ³‚µ‚Ü‚·B ‚±‚ÌContentTypeReader‚ÍSkinnedModelAnimationInfoReader‚
    /// Ìxnb‚ğ‘‚«‚ŞÛ‚Ég—p‚µ‚½ ContentTypeWriter‚Åw’è‚³‚ê‚½ƒ‰ƒ“ƒ^ƒCƒ€‚Æ“¯‚¶‚Å‚
    /// ‚é•K—v‚ª‚ ‚è‚Ü‚·B
    /// </summary>
    public class SkinnedModelAnimationInfoReader : ContentTypeReader<TRead>
    {
        /// <summary>
        /// Reads SkinnedModelAnimationInfo from the xnb file.
        ///
        /// xnbƒtƒ@ƒCƒ‹‚©‚çSkinnedModelAnimationInfo‚ğ“Ç‚İ‚İ‚Ü‚·B
        /// </summary>
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            TRead info = new TRead();

            // Reads AnimationInfo.
            // 
            // AnimationInfo‚ğ“Ç‚İ‚İ‚Ü‚·B
            AnimationInfoReader.ReadAnimationInfo(input, info);

            // Reads SkinnedModelAnimationInfo.
            // 
            // SkinnedModelAnimationInfo‚ğ“Ç‚İ‚İ‚Ü‚·B
            info.SkinnedModelInfoCollection.AddRange(
                input.ReadObject<List<SkinnedModelInfo>>());
            info.CameraUpVector = input.ReadObject<Vector3>();
            info.CameraPosition = input.ReadObject<Vector3>();
            info.CameraLookAt = input.ReadObject<Vector3>();

            return info;
        }
    }
}
