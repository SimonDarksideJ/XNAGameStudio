#region File Description
//-----------------------------------------------------------------------------
// SkinnedModelInfo.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace MovipaLibrary
{
    /// <summary>
    /// This class manages information of a skin model.
    /// This class is used in SkinnedModelAnimationInfo.
    /// ContentTypeReader and ContentTypeWriter are also provided in this class
    /// so that SkinnedModelAnimationInfo can be simply written in ContentPipeline.
    ///
    /// スキンモデルの情報を持ちます。
    /// このクラスは、SkinnedModelAnimationInfoで使用されています。
    /// SkinnedModelAnimationInfoをContentPipelineでシンプルに記述するため、
    /// このクラスにもContentTypeReaderとContentTypeWriterを用意しています。
    /// </summary>
    public class SkinnedModelInfo
    {
        #region Fields
        private string modelAsset;
        private string animationClip;
        private Vector3 position;
        #endregion

        #region Property
        /// <summary>
        /// Obtains or sets the asset name of the model.
        ///
        /// モデルのアセット名を取得または設定します。
        /// </summary>
        public string ModelAsset
        {
            get { return modelAsset; }
            set { modelAsset = value; }
        }


        /// <summary>
        /// Obtains or sets the clip name of the animation.
        ///
        /// アニメーションのクリップ名を取得または設定します。
        /// </summary>
        public string AnimationClip
        {
            get { return animationClip; }
            set { animationClip = value; }
        }


        /// <summary>
        /// Obtains or sets the model position.
        ///
        /// モデルの位置を取得または設定します。
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        #endregion
    }
}
