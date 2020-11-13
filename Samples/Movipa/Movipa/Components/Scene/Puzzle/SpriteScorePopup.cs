#region File Description
//-----------------------------------------------------------------------------
// SpriteScorePopup.cs
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

namespace Movipa.Components.Scene.Puzzle
{
    /// <summary>
    /// Sprite class that draws the score.
    /// This class inherits the Sprite class and performs update and drawing
    /// processing. At first appearance, it performs a bounce before shifting to 
    /// the designated position. Once the designated position is attained, it
    /// performs a fade-out; when this has finished, it performs release processing.
    /// 
    /// スコアを描画するスプライトのクラスです。
    /// このクラスはSpriteクラスを継承し、更新と描画の処理を行います。
    /// 出現時、まずバウンドを行い、バウンドが終了したら指定の
    /// 位置へ移動します。指定の位置へ到着したらフェードアウトを開始し、
    /// フェードアウトが完了すると、開放処理を行います。
    /// </summary>
    public class SpriteScorePopup : Sprite
    {
        #region Private Types
        /// <summary>
        /// Processing status
        /// 
        /// 処理状態
        /// </summary>
        enum Phase
        {
            /// <summary>
            /// Bounce
            /// 
            /// バウンド
            /// </summary>
            Bound,

            /// <summary>
            /// Move
            /// 
            /// 移動
            /// </summary>
            Move,

            /// <summary>
            /// Fade-out
            /// 
            /// フェードアウト
            /// </summary>
            FadeOut,
        }
        #endregion

        #region Fields
        public int Score;
        public Vector2 DefaultPosition;
        public Vector2 TargetPosition;

        private Phase phase;
        private SpriteFont font;
        private float jumpPower;
        private float jumpBoundPower;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public SpriteScorePopup(Game game)
            : base(game)
        {
            MovipaGame movipaGame = game as MovipaGame;
            if (movipaGame != null)
            {
                font = movipaGame.MediumFont;
            }
            Updating += SpriteScorePopupUpdating;
            Drawing += SpriteScorePopupDrawing;
        }

        
        /// <summary>
        /// Performs initialization processing. 
        /// 
        /// 初期化処理を行います。
        /// </summary>
        public override void Initialize()
        {
            // Initializes the processing status.
            // 
            // 処理状態の初期設定を行います。
            phase = Phase.Bound;

            // Sets the jump parameters.
            // 
            // ジャンプのパラメータを設定します。
            jumpPower = 8.0f;
            jumpBoundPower = 8.0f;

            // Sets the draw color.
            //
            // 描画色を設定します。
            Color = Color.White;

            base.Initialize();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Performs update processing.
        /// 
        /// 更新処理を行います。
        /// </summary>
        void SpriteScorePopupUpdating(object sender, UpdatingEventArgs args)
        {
            if (phase == Phase.Bound)
            {
                // Performs updates from first appearance through to end of bounce.
                // 
                // 出現からバウンド終了までの更新処理を行います。
                UpdateBound();
            }
            else if (phase == Phase.Move)
            {
                // Performs updates following movement.
                // 
                // 移動時の更新処理を行います。
                UpdateMove();
            }
            else if (phase == Phase.FadeOut)
            {
                // Performs updates following fade-out.
                // 
                // フェードアウト時の更新処理を行います。
                UpdateFadeOut();
            }
        }
        

        /// <summary>
        /// Performs updates from first appearance through to end of bounce.
        /// 
        /// 出現からバウンド終了までの更新処理を行います。
        /// </summary>
        private void UpdateBound()
        {
            // Shifts the Y coordinate.
            // 
            // Y座標を移動させます。
            position.Y -= jumpPower;

            // Performs bounce processing if it is lower than the 
            // initial appearance coordinates.
            // 
            // 初期出現座標よりも下がった場合、バウンド処理をします。
            if (Position.Y > DefaultPosition.Y)
            {
                // Sets the Y coordinate to the initial coordinate.
                // 
                // Y座標を初期座標へ設定します。
                position.Y = DefaultPosition.Y;

                // Reduces the bounce power.
                // 
                // バウンド力を減らします。
                jumpBoundPower *= 0.5f;

                // Sets the amount of Y coordinate travel.
                // 
                // Y座標の移動量を設定します。
                jumpPower = jumpBoundPower;

                // Sets to Movement Processing when there is no more bounce power.
                // 
                // バウンド力が無くなった場合は移動処理へ設定します。
                if (jumpBoundPower < 0.1f)
                {
                    phase = Phase.Move;
                }
            }
            else
            {
                // Reduces the amount of Y coordinate travel.
                // 
                // Y座標の移動量を減らします。
                jumpPower -= 1.0f;
            }
        }


        /// <summary>
        /// Performs updates following movement.
        /// 
        /// 移動時の更新処理を行います。
        /// </summary>
        private void UpdateMove()
        {
            // Moves to the target position.
            // 
            // 目的地まで移動させます。
            Position += (TargetPosition - Position) * 0.1f;

            // Performs fade-out upon arrival at the target position.
            // 
            // 目的地に到達した場合はフェードアウト処理を行います。
            if (Vector2.Distance(TargetPosition, Position) < 1.0f)
            {
                phase = Phase.FadeOut;
            }
        }


        /// <summary>
        /// Performs updates following fade-out.
        /// 
        /// フェードアウト時の更新処理を行います。
        /// </summary>
        private void UpdateFadeOut()
        {
            // Reduces the transparency color.
            // 
            // 透過色を減らします。
            Vector4 color = Color.ToVector4();
            color.W = MathHelper.Clamp(color.W - 0.1f, 0, 1);

            // Sets a new color.
            // 
            // 新しい色を設定します。
            Color = new Color(color);

            // Performs release processing when it is totally transparent.
            // 
            // 完全に透過されたら開放処理を行います。
            if (color.W <= 0)
            {
                Dispose();
            }
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Performs drawing processing.
        /// 
        /// 描画処理を行います。
        /// </summary>
        void SpriteScorePopupDrawing(object sender, DrawingEventArgs args)
        {
            string value = string.Format("{0:00}", Score);
            Vector2 position = Position - (font.MeasureString(value) * 0.5f);
            args.Batch.DrawString(font, value, position, Color);
        }
        #endregion
    }

}
