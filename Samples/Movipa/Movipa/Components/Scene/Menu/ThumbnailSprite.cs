#region File Description
//-----------------------------------------------------------------------------
// ThumbnailSprite.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Movipa.Util;
#endregion

namespace Movipa.Components.Scene.Menu
{
    /// <summary>
    /// Updates and draws the thumbnails.
    /// 
    /// サムネイルの更新と描画を行います。
    /// </summary>
    public class ThumbnailSprite : Sprite
    {
        #region Fields
        private Vector2 targetPosition;
        private int id;
        private int textureId;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the target position.
        /// 
        /// 移動先の位置を取得または設定します。
        /// </summary>
        public Vector2 TargetPosition
        {
            get { return targetPosition; }
            set { targetPosition = value; }
        }


        /// <summary>
        /// Obtains or sets the sprite ID.
        /// 
        /// スプライトのIDを取得または設定します。
        /// </summary>
        public int Id
        {
            get { return id; }
            set { id = value; }
        }


        /// <summary>
        /// Obtains or sets the texture ID.
        /// 
        /// テクスチャのIDを取得または設定します。
        /// </summary>
        public int TextureId
        {
            get { return textureId; }
            set { textureId = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public ThumbnailSprite(Game game)
            : base(game)
        {
            Updating += ThumbnailSpriteUpdating;
            Drawing += ThumbnailSpriteDrawing;
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Performs update processing.
        ///
        /// 更新処理を行います。
        /// </summary>
        void ThumbnailSpriteUpdating(object sender, UpdatingEventArgs args)
        {
            // Slides to the designated position.
            // 
            // 指定された位置までスライドします。
            Position += (TargetPosition - Position) * 0.2f;
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Performs render processing.
        /// 
        /// 描画処理を行います。
        /// </summary>
        void ThumbnailSpriteDrawing(object sender, DrawingEventArgs args)
        {
            // Does not draw if no texture is specified.
            // 
            // テクスチャが指定されていなければ描画をしません。
            if (Texture == null)
                return;

            args.Batch.Draw(Texture, Position, Color.White);
        }
        #endregion
    }
}
