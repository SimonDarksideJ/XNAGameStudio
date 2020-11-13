#region File Description
//-----------------------------------------------------------------------------
// PuzzleAnimation.cs
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

using MovipaLibrary;
#endregion

namespace Movipa.Components.Animation
{
    /// <summary>
    /// This is an abstract class of the component that performs update and 
    /// drawing processes for animations used in a puzzle. 
    /// Major implementations for drawing are written in the subclass.
    ///
    /// パズルで使用するアニメーションの更新と描画を実行するコンポーネントの
    /// 抽象クラスです。
    /// 描画に関する主な実装は継承先のクラスで記述します。
    /// </summary>
    public abstract class PuzzleAnimation : SceneComponent
    {
        #region Fields
        private AnimationInfo animationInfo;
        protected RenderTarget2D renderTarget;
        protected Texture2D renderTargetTexture;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the movie information.
        ///
        /// ムービー情報を取得します。
        /// </summary>
        public AnimationInfo Info
        {
            get { return animationInfo; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        protected PuzzleAnimation(Game game, AnimationInfo info)
            : base(game)
        {
            animationInfo = info;
        }


        /// <summary>
        /// Reads the contents.
        ///
        /// コンテントの読み込み処理を行います。
        /// </summary>
        protected override void LoadContent()
        {
            // Obtains the parameters.
            // 
            // パラメータを取得します。
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            SurfaceFormat format = pp.BackBufferFormat;
            MultiSampleType msType = pp.MultiSampleType;
            int msQuality = pp.MultiSampleQuality;

            // Creates a render target.
            // 
            // レンダーターゲットを作成します。
            int width = Info.Size.X;
            int height = Info.Size.Y;
            renderTarget = new RenderTarget2D(
                GraphicsDevice, width, height, 1, format, msType, msQuality, 
                RenderTargetUsage.PreserveContents);

            base.LoadContent();
        }

        #endregion

        #region Draw Methods
        /// <summary>
        /// Performs drawing for the render target.
        ///
        /// レンダーターゲットへの描画処理を行います。
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (renderTarget != null && !renderTarget.IsDisposed)
            {
                // Sets the drawing target.
                // 
                // 描画先を設定します。
                GraphicsDevice.SetRenderTarget(0, renderTarget);

                // Performs drawing for the render target.
                // 
                // レンダーターゲットへの描画を行います。
                DrawRenderTarget();

                // Returns the drawing target. 
                // 
                // 描画先を戻します。
                GraphicsDevice.SetRenderTarget(0, null);

                // Obtains the texture.
                // 
                // テクスチャを取得します。
                renderTargetTexture = renderTarget.GetTexture();
            }

            base.Draw(gameTime);
        }


        /// <summary>
        /// Performs drawing for the render target.
        /// 
        /// レンダーターゲットへの描画処理を行います。
        /// </summary>
        protected virtual void DrawRenderTarget() { }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Obtains the texture of the render target.
        /// 
        /// レンダーターゲットのテクスチャを取得します。
        /// </summary>
        public Texture2D Texture
        {
            get { return renderTargetTexture; }
        }


        /// <summary>
        /// Creates an animation component.
        /// 
        /// アニメーションコンポーネントを作成します。
        /// </summary>
        public static PuzzleAnimation CreateAnimationComponent(
            Game game, AnimationInfo info)
        {
            switch (info.Category)
            {
                case AnimationInfo.AnimationInfoCategory.Layout:
                    return new SequencePlayAnimationComponent(
                        game, (LayoutInfo)info);
                case AnimationInfo.AnimationInfoCategory.Rendering:
                    return new MovieComponent(
                        game, (RenderingInfo)info);
                case AnimationInfo.AnimationInfoCategory.SkinnedModelAnimation:
                    return new Animation.ModelAnimation.ModelAnimationComponent(
                        game, (SkinnedModelAnimationInfo)info);
                case AnimationInfo.AnimationInfoCategory.Particle:
                    return new Animation.ParticleComponent(
                        game, (ParticleInfo)info);
                default:
                    throw new ArgumentException("Invalid animation category provided.");
            }
        }

        #endregion
    }
}




