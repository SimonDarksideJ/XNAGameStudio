#region File Description
//-----------------------------------------------------------------------------
// PanelData.cs
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

using Movipa.Util;
#endregion

namespace Movipa.Components.Scene.Puzzle
{
    /// <summary>
    /// Manages the panel status.
    /// This class inherits the Sprite class.
    /// Update and draw processing is performed in the Panel and StyleBase classes.
    /// 
    /// パネルの状態を管理します。
    /// このクラスはSpriteクラスを継承しています。
    /// 更新と描画は、Panelクラスおよび、StyleBaseクラスで行っています。
    /// </summary>
    public class PanelData : Sprite
    {
        #region Public Types
        /// <summary>
        /// Panel status
        /// 
        /// パネルの状態
        /// </summary>
        public enum PanelStatus
        {
            /// <summary>
            /// Normal
            /// 
            /// 通常
            /// </summary>
            None,

            /// <summary>
            /// Rotating left
            /// 
            /// 左回転中
            /// </summary>
            RotateLeft,

            /// <summary>
            /// Rotating right
            ///
            /// 右回転中
            /// </summary>
            RotateRight,

            /// <summary>
            /// Shifting
            ///
            /// 移動中
            /// </summary>
            Move,
        }
        #endregion

        #region Fields
        private PanelStatus status;
        private int id;
        private float flush;
        private float moveCount;
        private float toRotate;
        public Vector2 FromPosition;
        public Vector2 ToPosition;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the panel status.
        /// 
        /// パネルの状態を取得または設定します。
        /// </summary>
        public PanelStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        /// <summary>
        /// Obtains or sets the panel ID.
        /// 
        /// パネルのIDを取得または設定します。
        /// </summary>
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Obtains or sets the completed effect status.
        /// 
        /// 完成エフェクトの状態を取得または設定します。
        /// </summary>
        public float Flush
        {
            get { return flush; }
            set { flush = value; }
        }

        /// <summary>
        /// Obtains or sets the movement amount.
        /// 
        /// 移動量を取得または設定します。
        /// </summary>
        public float MoveCount
        {
            get { return moveCount; }
            set { moveCount = value; }
        }

        /// <summary>
        /// Obtains or sets the rotation target.
        /// 
        /// 回転目標を取得または設定します。
        /// </summary>
        public float ToRotate
        {
            get { return toRotate; }
            set { toRotate = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public PanelData(Game game)
            : base(game)
        {
            flush = 0.0f;
            moveCount = 0.0f;
            FromPosition = Vector2.Zero;
            ToPosition = Vector2.Zero;
            toRotate = 0.0f;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Obtains the panel center coordinates.
        /// 
        /// パネル中央の座標を取得します。
        /// </summary>
        public Vector2 Center
        {
            get { return Vector2.Add(Position, Origin); }
        }
        #endregion
    }
}
