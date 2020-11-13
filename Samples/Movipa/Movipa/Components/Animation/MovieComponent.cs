#region File Description
//-----------------------------------------------------------------------------
// MovieComponent.cs
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
    /// This component is for animations used in puzzles.
    /// This class inherits PuzzleAnimation to read and draw
    /// sequential textures.
    ///
    /// パズルで使用するアニメーションのコンポーネントです。
    /// このクラスはPuzzleAnimationを継承し、連番のテクスチャを
    /// 読み込んで描画します。
    /// </summary>
    public class MovieComponent : PuzzleAnimation
    {
        #region Fields
        /// <summary>
        /// Waiting time for each frame
        ///
        /// フレーム毎のウェイト
        /// </summary>
        private readonly TimeSpan FrameWait;

        /// <summary>
        /// Movie texture
        ///
        /// ムービーのテクスチャ
        /// </summary>
        private Texture2D movieTexture;

        /// <summary>
        /// Movie texture number
        ///
        /// ムービーのテクスチャ番号
        /// </summary>
        private UInt32 textureCount;

        /// <summary>
        /// Movie frame number
        ///
        /// ムービーのフレーム番号
        /// </summary>
        private UInt32 frameCount;

        /// <summary>
        /// Drawing size
        /// 
        /// 描画サイズ
        /// </summary>
        private Rectangle drawRectangle;

        /// <summary>
        /// Original image size
        ///
        /// 元画像サイズ
        /// </summary>
        private Rectangle srcRectangle;

        /// <summary>
        /// Movie information
        ///
        /// ムービー情報
        /// </summary>
        private RenderingInfo renderingInfo;

        /// <summary>
        /// Waiting time
        ///
        /// ウェイト時間
        /// </summary>
        private TimeSpan waitTime;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the movie information.
        ///
        /// ムービー情報を取得します。
        /// </summary>
        public new RenderingInfo Info
        {
            get { return renderingInfo; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public MovieComponent(Game game, RenderingInfo info)
            : base(game, info)
        {
            renderingInfo = info;

            // Sets the waiting time for one frame.
            //
            // 1フレームのウェイトを設定します。
            long tick = TimeSpan.TicksPerSecond / renderingInfo.FrameRate;
            FrameWait = new TimeSpan(tick);
        }


        /// <summary>
        /// Performs initialization.
        ///
        /// 初期化を実行します。
        /// </summary>
        public override void Initialize()
        {
            // Sets the image width.
            // 
            // 画像の幅を設定します。
            srcRectangle = new Rectangle(0, 0, Info.ImageSize.X, Info.ImageSize.Y);

            // Sets the drawing size.
            //
            // 描画のサイズを設定します。
            drawRectangle = new Rectangle(0, 0, Info.Size.X, Info.Size.Y);

            // Initializes the frame waiting time.
            //
            // フレームウェイトを初期化します。
            waitTime = TimeSpan.Zero;

            base.Initialize();
        }


        /// <summary>
        /// Reads the content.
        ///
        /// コンテントの読み込み処理を行います。
        /// </summary>
        protected override void LoadContent()
        {
            string asset;

            // Reads all the sequential numbers and cache textures.
            // 
            // 最初に全ての連番を読み込み、テクスチャのキャッシュを行います。
            for (int i = 0; i < Info.TotalTexture; i++)
            {
                asset = string.Format(Info.Format, i);
                Content.Load<Texture2D>(string.Format(Info.Format, i));
            }

            // Sets the first texture.
            // 
            // 最初のテクスチャを設定します。
            asset = string.Format(Info.Format, textureCount);
            movieTexture = Content.Load<Texture2D>(asset);

            base.LoadContent();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Performs the update process to switch the movie frame.
        ///
        /// ムービーのフレームを切り替える更新処理を行います。
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Calculates the frame waiting time.
            // 
            // フレームのウェイトを計算します。
            waitTime += gameTime.ElapsedGameTime;

            // If the frame waiting time exceeds the preset time, sets the next frame.
            // 
            // ウェイトが終了していたら次のフレームを設定します。
            if (waitTime >= FrameWait)
            {
                // Initialize the waiting time.
                //
                // ウェイトを初期化します。
                waitTime = TimeSpan.Zero;

                UpdateNextFrame();
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Updates the next frame.
        ///
        /// 次のフレームへ更新します。
        /// </summary>
        private void UpdateNextFrame()
        {
            // Increments the frame count.
            //
            // フレーム数を次の値へ設定します。
            frameCount = (frameCount + 1) % Info.TotalFrame;

            // If all frames are completed, returns to the first texture and frame.
            //
            // 全てのフレームが終了していた場合は
            // 最初のテクスチャとフレームに戻します。
            if (frameCount == 0)
            {
                srcRectangle.X = 0;
                srcRectangle.Y = 0;
                textureCount = 0;
                string asset = string.Format(Info.Format, textureCount);
                movieTexture = Content.Load<Texture2D>(asset);
                return;
            }

            // Moves the X coordinate of the transfer source.
            // 
            // 転送元のX座標を移動します。
            int nextX = (srcRectangle.X + srcRectangle.Width);
            srcRectangle.X = nextX % movieTexture.Width;
            if (srcRectangle.X == 0)
            {
                // If the X coordinate is set to 0, moves the Y coordinate.
                // 
                // X座標が0に戻っていた場合はY座標を移動します。
                int nextY = (srcRectangle.Y + srcRectangle.Height);
                srcRectangle.Y = nextY % movieTexture.Height;
                    
                if (srcRectangle.Y == 0)
                {
                    // If both the X and Y coordinates are set to 0, which means 
                    // all frames in this texture are drawn, switches to the
                    // next texture. 
                    // 
                    // X座標もY座標も0に戻ったら、テクスチャ内のフレームが
                    // 全て描画し終わったので次のテクスチャに切り替えます。
                    textureCount = (textureCount + 1) % Info.TotalTexture;
                    string asset = string.Format(Info.Format, textureCount);
                    movieTexture = Content.Load<Texture2D>(asset);
                }
            }
        }

        #endregion

        #region Draw Methods
        /// <summary>
        /// Performs a drawing process for the rendering target.
        ///
        /// レンダーターゲットへの描画処理を行います。
        /// </summary>
        protected override void DrawRenderTarget()
        {
          
           // Clears the background.
           //
           // 背景をクリアします。
            GraphicsDevice.Clear(Color.Black);

            // Draws the texture.
            // 
            // テクスチャを描画します。
            Batch.Begin();
            Batch.Draw(movieTexture, drawRectangle, srcRectangle, Color.White);
            Batch.End();

        }
        #endregion
    }


}




