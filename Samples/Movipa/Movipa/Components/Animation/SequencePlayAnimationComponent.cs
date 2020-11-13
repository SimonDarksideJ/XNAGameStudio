#region File Description
//-----------------------------------------------------------------------------
// SequencePlayAnimationComponent.cs
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

using SceneDataLibrary;
using MovipaLibrary;
#endregion

namespace Movipa.Components.Animation
{
    /// <summary>
    /// This component is for animations used in puzzles.
    /// This class inherits PuzzleAnimation to play and draw
    /// the sequence of Layout.
    ///
    /// パズルで使用するアニメーションのコンポーネントです。
    /// このクラスはPuzzleAnimationを継承し、Layoutのシーケンスを
    /// 再生して描画します。
    /// </summary>
    public class SequencePlayAnimationComponent : PuzzleAnimation
    {
        #region Fields
        private SceneData sceneData = null;
        private SequencePlayData seqPlayData = null;
        private LayoutInfo layoutToolInfo;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the movie information.
        ///
        /// ムービー情報を取得します。
        /// </summary>
        public new LayoutInfo Info
        {
            get { return layoutToolInfo; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public SequencePlayAnimationComponent(Game game, LayoutInfo info)
            : base(game, info)
        {
            layoutToolInfo = info;
        }


        /// <summary>
        /// Reads the contents.
        ///
        /// コンテントの読み込み処理を行います。
        /// </summary>
        protected override void LoadContent()
        {
            // Reads the Layout data.
            // 
            // Layoutのデータを読み込みます。
            sceneData = Content.Load<SceneData>(Info.SceneDataAsset);
            seqPlayData = sceneData.CreatePlaySeqData(Info.Sequence);

            base.LoadContent();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the sequence.
        ///
        /// シーケンスの更新処理を行います。
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            seqPlayData.Update(gameTime.ElapsedGameTime);
            base.Update(gameTime);
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Performs drawing for the render target.
        ///
        /// レンダーターゲットへの描画処理を行います。
        /// </summary>
        protected override void DrawRenderTarget()
        {
            // Clears the background.
            // 
            // 背景をクリアします。
            GraphicsDevice.Clear(Color.Black);

            // Draws the sequence.
            // 
            // シーケンスの描画を行います。
            Batch.Begin();
            seqPlayData.Draw(Batch, null);
            Batch.End();
        }
        #endregion
   }
}







