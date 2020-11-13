#region File Description
//-----------------------------------------------------------------------------
// PanelAfterImage.cs
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

using Movipa.Util;
#endregion

namespace Movipa.Components.Scene.Puzzle.Style
{
    /// <summary>
    /// Panel after-image class.
    /// This class inherits the Sprite class and performs update and draw processing.
    /// Sprites are managed by the StyleBase class.
    /// 
    /// パネルの残像クラスです。
    /// このクラスはSpriteクラスを継承し、更新と描画の処理を行います。
    /// スプライトの管理はStyleBaseクラスで行っています。
    /// </summary>
    public class PanelAfterImage : Sprite
    {
        #region Initialization
        /// <summary>
        /// Initializes the instance. 
        /// 
        /// インスタンスの初期化を行います。
        /// </summary>
        public PanelAfterImage(Game game)
            : base(game)
        {
            Updating += PanelAfterImageUpdating;
            Drawing += PanelAfterImageDrawing;
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Performs update processing.
        /// 
        /// 更新処理を行います。
        /// </summary>
        void PanelAfterImageUpdating(object sender, UpdatingEventArgs args)
        {
            // Reduces the transparency color value.
            // 
            // 透過色の値を下げます。
            Vector4 colorVector4 = Color.ToVector4();
            colorVector4.W = MathHelper.Clamp(colorVector4.W - 0.1f, 0.0f, 1.0f);
            Color = new Color(colorVector4);

            // Performs release processing when the transparency color value reaches 0.
            //
            // 透過色の値が0になったら開放処理を行います。
            if (Color.A == 0)
                Dispose();
        }
        #endregion

        #region Drawing Methods

        /// <summary>
        /// Performs a primitive draw.
        /// This method needs to be invoked in advance since the 
        /// SpriteBatch Begin and End are not performed.
        ///
        /// 基本的な描画を行います。
        /// このメソッドではSpriteBatchのBegin/Endが行われないので
        /// 事前に呼び出して下さい。
        /// </summary>
        void PanelAfterImageDrawing(object sender, DrawingEventArgs args)
        {
            base.Draw(args.Batch);
        }

        #endregion
    }
}
