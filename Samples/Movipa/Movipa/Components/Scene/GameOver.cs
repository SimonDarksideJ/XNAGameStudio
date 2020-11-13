#region File Description
//-----------------------------------------------------------------------------
// GameOver.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using Movipa.Components.Input;
using Movipa.Util;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene
{
    /// <summary>
    /// This scene component displays game over.
    /// It provides fade control and basic replay
    /// functionality for Layout sequences.
    ///
    /// ゲームオーバーを表示するシーンコンポーネントです。
    /// フェードの制御と、Layoutで作成されたシーケンスの
    /// シンプルな再生処理を行っています。

    /// </summary>
    public class GameOver : SceneComponent
    {
        #region Fields
        // Components
        private FadeSeqComponent fade;

        // Layout
        private SceneData sceneData;
        private SequencePlayData seqStart;

        // BackgroundMusic
        private Cue bgm;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public GameOver(Game game)
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
            // Obtains the fade component instance. 
            //
            // フェードコンポーネントのインスタンスを取得します。
            fade = GameData.FadeSeqComponent;

            // Specifies the fade-in settings.
            //
            // フェードインの設定を行います。
            fade.Start(FadeType.Normal, FadeMode.FadeIn);

            base.Initialize();
        }


        /// <summary>
        /// Initializes the navigate.
        /// 
        /// ナビゲートの初期化をします。
        /// </summary>
        protected override void InitializeNavigate()
        {
            Navigate.Add(new NavigateData(AppSettings("A_Ok"), true));
            base.InitializeNavigate();
        }


        /// <summary>
        /// Loads the content.
        ///
        /// コンテントの読み込み処理を行います。
        /// </summary>
        protected override void LoadContent()
        {
            // Obtains the Layout sequence.
            // 
            // Layoutのシーケンスを取得します。
            string asset = "Layout/GameOver/gameover_Scene";
            sceneData = Content.Load<SceneData>(asset);
            seqStart = sceneData.CreatePlaySeqData("GameOver");

            // Plays the BackgroundMusic and obtains Cue.
            // 
            // BackgroundMusicを再生し、Cueを取得します。
            bgm = GameData.Sound.PlayBackgroundMusic(Sounds.GameOverBackgroundMusic);

            base.LoadContent();
        }


        /// <summary>
        /// Releases all resources.
        /// 
        /// 全てのリソースを開放します。
        /// </summary>
        protected override void UnloadContent()
        {
            // Stops the BackgroundMusic.
            // 
            // BackgroundMusicを停止します。
            SoundComponent.Stop(bgm);

            base.UnloadContent();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Performs update processing.
        /// 
        /// 更新処理を行います。
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
                UpdateMain();
            }
            else if (fade.FadeMode == FadeMode.FadeOut)
            {
                // Updates fade-out. 
                // 
                // フェードアウトの更新処理を行います。
                UpdateFadeOut();
            }

            // Updates sequences except fade-in.
            //
            // フェードイン以外でシーケンスの更新をします。
            if (fade.FadeMode != FadeMode.FadeIn)
            {
                seqStart.Update(gameTime.ElapsedGameTime);
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Performs main update processing.
        ///
        /// メインの更新処理を行います。
        /// </summary>
        private void UpdateMain()
        {
            // Obtains Pad information. 
            //
            // パッドの情報を取得します。
            VirtualPadState virtualPad =
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            // Performs the fade-out after the sequence terminates 
            // or if the A button is pressed.
            //
            // シーケンスが再生終了、またはAボタンを押されたらフェードアウトします。
            if (!seqStart.IsPlay || buttons.A[VirtualKeyState.Push] || 
                buttons.B[VirtualKeyState.Push] || buttons.Back[VirtualKeyState.Push])
            {
                fade.Start(FadeType.Normal, FadeMode.FadeOut);
            }
        }


        /// <summary>
        /// Performs update processing at fade-out.
        /// 
        /// フェードアウト時の更新処理を行います。
        /// </summary>
        private void UpdateFadeOut()
        {
            // Fades out the BackgroundMusic volume.
            // 
            // BackgroundMusicのボリュームをフェードアウトします。
            float volume = 1.0f - (fade.Count / 60.0f);
            SoundComponent.SetVolume(bgm, volume);

            // Switches scenes after the fade finishes.
            //
            // フェードが終了するとシーンの切り替えを行います。
            if (!fade.IsPlay)
            {
                Dispose();

                // Performs entry for title screen scenes.
                // 
                // タイトル画面のシーンをエントリーします。
                GameData.SceneQueue.Enqueue(new Title(Game));
            }
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws the sequence.
        ///
        /// シーケンスの描画処理を行います。
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Clears the background.
            // 
            // 背景をクリアします。
            GraphicsDevice.Clear(Color.Black);

            // Draws the sequence and navigate button. 
            // 
            // シーケンスとナビゲートボタンを描画します。
            Batch.Begin();
            seqStart.Draw(Batch, null);
            DrawNavigate(gameTime, false);
            Batch.End();

            base.Draw(gameTime);
        }
        #endregion
    }
}


