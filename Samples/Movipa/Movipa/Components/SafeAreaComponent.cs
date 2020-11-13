#region File Description
//-----------------------------------------------------------------------------
// SafeAreaComponent.cs
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
using Microsoft.Xna.Framework.Input;
using Movipa.Util;
#endregion

namespace Movipa.Components
{
    /// <summary>
    /// Component that draws the Safe Area.
    /// Push the right stick on the Game Pad to draw.
    /// The scope is set in Scale Properties, and the draw
    /// color in Color Properties. The default scope value is 0.9f (90%).
    /// 
    /// セーフエリアを描画するコンポーネントです。
    /// ゲームパッドの右スティックを押し込んだ状態で描画します。
    /// 有効範囲の設定はScaleプロパティで設定し、描画色はColorプロパティで
    /// 設定します。有効範囲の初期値は0.9f(90%)に設定されています。
    /// </summary>
    public class SafeAreaComponent : DrawableGameComponent
    {
        #region Fields
        /// <summary>
        /// Primitive Draw class
        /// 
        /// 基本描画クラス
        /// </summary>
        private PrimitiveDraw2D primitiveDraw;

        /// <summary>
        /// Scale
        ///
        /// スケール
        /// </summary>
        private float scale;

        /// <summary>
        /// Draw color
        ///
        /// 描画色
        /// </summary>
        private Vector4 color;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the scale.
        ///
        /// スケールを取得または設定します。
        /// </summary>
        public float Scale
        {
            get { return scale; }
            set { scale = MathHelper.Clamp(value, 0, 1); }
        }


        /// <summary>
        /// Obtains or sets the draw color.
        ///
        /// 描画色を取得または設定します。
        /// </summary>
        public Vector4 Color
        {
            get { return color; }
            set { color = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public SafeAreaComponent(Game game)
            : base(game)
        {
            primitiveDraw = new PrimitiveDraw2D(game.GraphicsDevice);
            Scale = 0.9f;
            color = new Vector4(1, 0, 1, 0.25f);
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Switches the component visibility status from the pad input status.
        /// 
        /// パッドの入力状態からコンポーネントの可視状態を切り替えます。
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Make visible when player 1 pushes the right stick.
            // 
            // プレイヤー1の右スティックを押下されていたら可視状態にします。
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
            Visible = (gamePad.Buttons.RightStick == ButtonState.Pressed);

            base.Update(gameTime);
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws the Safe Area.
        /// 
        /// セーフエリアを描画します。
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Sets the draw mode.
            // 
            // 描画モードを設定
            primitiveDraw.SetRenderState(GraphicsDevice, SpriteBlendMode.AlphaBlend);

            // Fills the rectangle list.
            // 
            // 矩形リストを塗りつぶす
            foreach (Rectangle region in GetRegions())
            {
                primitiveDraw.FillRect(null, region, new Color(color));
            }

            base.Draw(gameTime);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Obtains the Safe Area rectangle.
        /// 
        /// セーフエリアの矩形を取得します。
        /// </summary>
        private Rectangle[] GetRegions()
        {
            Vector2 safeSize = GameData.ScreenSizeVector2 * Scale;
            Vector2 outSize = GameData.ScreenSizeVector2 - safeSize;
            Vector2 halfOutSize = outSize * 0.5f;

            Rectangle[] regions = new Rectangle[4];

            // Up 
            // 
            // 上
            regions[0].X = 0;
            regions[0].Y = 0;
            regions[0].Width = GameData.ScreenWidth;
            regions[0].Height = (int)halfOutSize.Y;

            // Down
            // 
            // 下
            regions[1].X = 0;
            regions[1].Y = GameData.ScreenHeight - (int)halfOutSize.Y;
            regions[1].Width = GameData.ScreenWidth;
            regions[1].Height = (int)halfOutSize.Y;

            // Left
            // 
            // 左
            regions[2].X = 0;
            regions[2].Y = (int)halfOutSize.Y;
            regions[2].Width = (int)halfOutSize.X;
            regions[2].Height = GameData.ScreenHeight - (int)outSize.Y;

            // Right
            // 
            // 右
            regions[3].X = GameData.ScreenWidth - (int)halfOutSize.X;
            regions[3].Y = (int)halfOutSize.Y;
            regions[3].Width = (int)halfOutSize.X;
            regions[3].Height = GameData.ScreenHeight - (int)outSize.Y;

            return regions;
        }
        #endregion
    }
}


