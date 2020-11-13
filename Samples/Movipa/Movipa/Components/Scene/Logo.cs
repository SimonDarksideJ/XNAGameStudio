#region File Description
//-----------------------------------------------------------------------------
// Logo.cs
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

using Movipa.Components.Input;
#endregion

namespace Movipa.Components.Scene
{
    /// <summary>
    /// This scene component draws the logo. 
    /// It provides fade control and text string rendering. 
    /// 
    /// ロゴの描画を行うシーンコンポーネントです。
    /// フェードの制御と、文字列の描画処理を行っています。
    /// </summary>
    public class Logo : SceneComponent
    {
        #region Fields
        private readonly TimeSpan WaitTime = new TimeSpan(0, 0, 2);

        // Components
        private FadeSeqComponent fade;

        private string developerName;
        private SpriteFont developerFont;
        private Vector2 drawPosition;
        private TimeSpan viewTime;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public Logo(Game game)
            : base(game)
        {
        }


        /// <summary>
        /// Performs initialization processing.
        ///
        /// 初期化処理を行います。
        /// </summary>
        public override void Initialize()
        {
            // Obtains the developer’s name.
            // 
            // 開発名を取得します。
            developerName = GameData.AppSettings["DeveloperName"];

            // Specifies the font settings.
            // 
            // フォントを設定します。
            developerFont = MediumFont;

            // Sets the draw position.
            // 
            // 描画位置を設定します。
            drawPosition = GameData.ScreenSizeVector2 * 0.5f;
            drawPosition -= developerFont.MeasureString(developerName) * 0.5f;

            // Initializes the display time.
            // 
            // 表示時間の初期化します。
            viewTime = TimeSpan.Zero;

            // Obtains the fade component instance. 
            // 
            // フェードコンポーネントのインスタンスを取得します。
            fade = GameData.FadeSeqComponent;

            // Specifies the fade-in settings.
            //
            // フェードインの処理を設定します。
            fade.Start(FadeType.Normal, FadeMode.FadeIn);

            base.Initialize();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Performs scene update processing.
        ///
        /// シーンの更新処理を行います。
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (fade.FadeMode == FadeMode.FadeIn)
            {
                // Switches the fade mode after the fade-in finishes.
                //
                // フェードインが完了したらフェードのモードを切り替えます。
                if (!fade.IsPlay)
                {
                    fade.FadeMode = FadeMode.None;
                }
            }
            else if (fade.FadeMode == FadeMode.None)
            {
                // Updates Main.
                //
                // メインの更新処理を行います。
                UpdateMain(gameTime);
            }
            else if (fade.FadeMode == FadeMode.FadeOut)
            {
                // Fade-out
                // 
                // フェードアウト
                if (!fade.IsPlay)
                {
                    // Terminates after the fade-out finishes.
                    // 
                    // フェードアウトが終了したら終了します。
                    Dispose();
                }
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Performs main update processing.
        /// 
        /// メインの更新処理を行います。
        /// </summary>
        private void UpdateMain(GameTime gameTime)
        {
            // Obtains Pad information.
            // 
            // パッドの情報を取得します。
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];

            // Calculates the display time.
            // 
            // 表示時間を加算します。
            viewTime += gameTime.ElapsedGameTime;

            // Starts the fade-out when the display time is 
            // exceeded or the A button is pressed.
            // 
            // 表示の時間が過ぎたか、またはAボタンが押されたら
            // フェードアウトを開始します。
            if (viewTime > WaitTime || virtualPad.Buttons.A[VirtualKeyState.Push])
            {
                fade.Start(FadeType.Gonzales, FadeMode.FadeOut);
            }
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws the scene.
        ///
        /// シーンの描画処理を行います。
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Clears the background to white.
            // 
            // 白で背景をクリアします。
            GraphicsDevice.Clear(Color.White);

            // Draws the text.
            // 
            // 文字を描画します。
            Batch.Begin();
            Batch.DrawString(developerFont, developerName, drawPosition, Color.White);
            Batch.End();

            base.Draw(gameTime);
        }
        #endregion
    }
}


