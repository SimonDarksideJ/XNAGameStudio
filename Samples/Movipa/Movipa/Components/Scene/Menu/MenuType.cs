#region File Description
//-----------------------------------------------------------------------------
// MenuData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

using Movipa.Components.Animation;
using Movipa.Components.Scene.Puzzle;
using Movipa.Util;
using MovipaLibrary;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene.Menu
{
    #region Public Types
    /// <summary>
    /// Defines the menu items.
    /// 
    /// メニューの項目を定義します。
    /// </summary>
    public enum MenuType
    {
        /// <summary>
        /// Mode selection
        /// 
        /// モード選択
        /// </summary>
        SelectMode,

        /// <summary>
        /// Save File selection
        ///
        /// セーブファイル選択
        /// </summary>
        SelectFile,

        /// <summary>
        /// Style selection
        ///
        /// スタイル選択
        /// </summary>
        SelectStyle,

        /// <summary>
        /// Rotation setting
        ///
        /// 回転の設定
        /// </summary>
        RotateSelect,

        /// <summary>
        /// Movie selection
        ///
        /// ムービー選択
        /// </summary>
        SelectMovie,

        /// <summary>
        /// Split setting
        /// 
        /// 分割数の設定
        /// </summary>
        SelectDivide,

        /// <summary>
        /// Confirmation screen 
        ///
        /// 確認画面
        /// </summary>
        Ready,
    }
    #endregion
}
