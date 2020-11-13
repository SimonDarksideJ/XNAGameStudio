#region File Description
//-----------------------------------------------------------------------------
// AnimationInfo.cs
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
    /// This class manages animation information.
    /// If there is information used in a movie, inherit this class.
    /// Type, name, and size of an animation are managed in this class.
    ///
    /// アニメーション情報を持つクラスです。
    /// ムービーで使用する情報を持つ場合はこのクラスを継承します。
    /// アニメーションのタイプ、名前、そしてサイズの情報があります。
    /// </summary>
    public class AnimationInfo
    {
        #region Public Types
        // Animation type
        // 
        // アニメーションの種類
        public enum AnimationInfoCategory
        {
            Layout,
            Rendering,
            SkinnedModelAnimation,
            Particle,
        }
        #endregion

        #region Fields
        private AnimationInfoCategory category;
        private string name;
        private Point size;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the type.
        ///
        /// タイプを取得または設定します。
        /// </summary>
        public AnimationInfoCategory Category
        {
            get { return category; }
            set { category = value; }
        }

        /// <summary>
        /// Obtains or sets the name.
        ///
        /// 名前を取得または設定します。
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Obtains or sets the size.
        ///
        /// サイズを取得または設定します。
        /// </summary>
        public Point Size
        {
            get { return size; }
            set { size = value; }
        }
        #endregion
    }
}
