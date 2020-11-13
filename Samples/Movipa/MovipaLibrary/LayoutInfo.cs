#region File Description
//-----------------------------------------------------------------------------
// LayoutInfo.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#endregion

namespace MovipaLibrary
{
    /// <summary>
    /// This class manages animation information of Layout.
    /// The asset name for SceneData and the sequence name to be used
    /// are managed in this class.
    ///
    /// Layoutのアニメーション情報を持ちます。
    /// SceneDataへのアセット名と、使用するシーケンス名があります。
    /// </summary>
    public class LayoutInfo : AnimationInfo
    {
        #region Fields
        private string sceneDataAsset;
        private string sequence;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the asset name for the scene data.
        ///
        /// シーンデータのアセット名を取得または設定します。
        /// </summary>
        public string SceneDataAsset
        {
            get { return sceneDataAsset; }
            set { sceneDataAsset = value; }
        }


        /// <summary>
        /// Obtains or sets the sequence name.
        ///
        /// シーケンス名を取得または設定します。
        /// </summary>
        public string Sequence
        {
            get { return sequence; }
            set { sequence = value; }
        }
        #endregion
    }
}
