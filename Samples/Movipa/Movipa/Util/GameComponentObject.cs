#region File Description
//-----------------------------------------------------------------------------
// GameComponentObject.cs
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
#endregion

namespace Movipa.Util
{
    /// <summary>
    /// Provides Update (used for update events) and Draw (used for draw events).
    /// Update checks that the Enable flag is enabled before proceeding; 
    /// similarly, Draw checks the Visible flag before proceeding.
    /// 
    /// XNAの機能を拡張したオブジェクトクラスです。
    /// 更新用イベントのUpdateと、描画用のイベントのDrawが用意されています。
    /// UpdateはEnableフラグ、DrawはVisibleフラグを見て、無効状態になっていれば
    /// 処理をしないようになっています。
    /// </summary>
    public class GameComponentObject : GameObject
    {
        #region Fields
        private Game game;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains GraphicsDevice.
        /// 
        /// GraphicsDeviceを取得します。
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return game.GraphicsDevice; }
        }

        /// <summary>
        /// Obtains ContentManager.
        /// 
        /// ContentManagerを取得します。
        /// </summary>
        public ContentManager Content
        {
            get { return game.Content; }
        }

        /// <summary>
        /// Obtains Game.
        /// 
        /// Gameを取得します。
        /// </summary>
        public Game Game
        {
            get { return game; }
        }
        #endregion

        #region Public Event

        public class UpdatingEventArgs : EventArgs
        {
            private GameTime gameTime;
            public GameTime GameTime
            {
                get { return gameTime; }
            }


            public UpdatingEventArgs(GameTime gameTime)
                : base()
            {
                this.gameTime = gameTime;
            }
        }
        public event EventHandler<UpdatingEventArgs> Updating;

        public class DrawingEventArgs : EventArgs
        {
            private GameTime gameTime;
            public GameTime GameTime
            {
                get { return gameTime; }
            }

            private SpriteBatch batch;
            public SpriteBatch Batch
            {
                get { return batch; }
            }

            public DrawingEventArgs(GameTime gameTime, SpriteBatch batch)
                : base()
            {
                this.gameTime = gameTime;
                this.batch = batch;
            }
        }
        public event EventHandler<DrawingEventArgs> Drawing;

        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance. 
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public GameComponentObject(Game game)
        {
            this.game = game;
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Executes UpdateEvent.
        /// 
        /// UpdateEventを実行します。
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (Updating != null && !Disposed && Enabled)
            {
                Updating(this, new UpdatingEventArgs(gameTime));
            }
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Executes DrawEvent.
        /// 
        /// DrawEventを実行します。
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            if (Drawing != null && !Disposed && Visible)
            {
                Drawing(this, new DrawingEventArgs(gameTime, batch));
            }
        }
        #endregion
    }
}