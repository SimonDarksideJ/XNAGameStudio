#region File Description
//-----------------------------------------------------------------------------
// StaffRoll.cs
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
    /// Scene component that displays Staff Roll.
    /// Changes the playback speed for sequences created in Layout.
    /// Pressing the A button during sequence playback switches to 5x playback speed.
    /// 
    /// スタッフロールを表示するシーンコンポーネントです。
    /// Layoutで作成されたシーケンスの再生速度を変更する処理を行っています。
    /// 再生中にAボタンを押すと、5倍の速度でシーケンスを再生します。
    /// </summary>
    public class StaffRoll : SceneComponent
    {
        #region Fields
        // Staff Roll playback speed
        // 
        // スタッフロールの再生倍率
        private const int SkipSpeed = 5;

        // Layout4 scene data
        // 
        // Layout4シーンデータ
        private SceneData sceneData;

        // Layout4 sequence array
        // 
        // Layout4シーケンス配列
        private SequencePlayData[] seqStaffRoll;

        // Current sequence number
        // 
        // 現在のシーケンス番号
        int seqIndex;

        // BackgroundMusic Cue
        // 
        // BackgroundMusicのキュー
        Cue bgm;

        // BackgroundMusic volume
        // 
        // BackgroundMusicのボリューム
        float bgmVolume;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public StaffRoll(Game game)
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
            // Initializes the sequence number.
            // 
            // シーケンスの番号を初期化します。
            seqIndex = 0;

            // Sets the initial volume value.
            // 
            // ボリュームの初期値を設定します。
            bgmVolume = 1.0f;

            // Sets the fade-in.
            // 
            // フェードインの設定を行います。
            GameData.FadeSeqComponent.Start(FadeType.Normal, FadeMode.FadeIn);

            base.Initialize();
        }


        /// <summary>
        /// Loads the content.
        /// 
        /// コンテントの読み込み処理を行います。
        /// </summary>
        protected override void LoadContent()
        {
            // Loads the Layout scene data.
            // 
            // Layoutのシーンデータを読み込みます。
            string asset = "Layout/StaffRoll/staffroll_Scene";
            sceneData = Content.Load<SceneData>(asset);

            // Loads the Layout sequence data into the array.
            // 
            // 
            // Layoutのシーケンスデータを配列に読み込みます。
            seqStaffRoll = new SequencePlayData[] {
                sceneData.CreatePlaySeqData("Planner01"),
                sceneData.CreatePlaySeqData("Programmer02"),
                sceneData.CreatePlaySeqData("MusicComposer03"),
                sceneData.CreatePlaySeqData("GraphicDesigner04"),
                sceneData.CreatePlaySeqData("DevelopedBy05"),
            };

            // Plays the BackgroundMusic and obtains the Cue.
            // 
            // BackgroundMusicの再生を行い、Cueを取得します。
            bgm = GameData.Sound.PlayBackgroundMusic(Sounds.TitleBackgroundMusic);

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
            if (seqIndex < seqStaffRoll.Length)
            {
                // Updates the sequence.
                // 
                // シーケンスの更新処理を行います。
                UpdateSequence(gameTime);
            }
            else
            {
                // Performs fade-out since 
                // all sequences are completed.
                // 
                // シーケンスが全て終了しているので、
                // フェードアウト処理を行います。
                UpdateFadeOut();
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Updates the sequence.
        /// 
        /// シーケンスの更新処理を行います。
        /// </summary>
        private void UpdateSequence(GameTime gameTime)
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            // Sets the update count.
            // If the A button has been pressed, performs update processing for 
            // the number of times specified in SkipSpeed.
            // 
            // 更新回数を設定します。
            // Aボタンが押されていればSkipSpeedで指定されている回数だけ
            // 更新処理を行います。
            int updateCount = (buttons.A[VirtualKeyState.Press]) ? SkipSpeed : 1;

            // Updates the sequence.
            // 
            // シーケンスの更新を行います。
            for (int i = 0; i < updateCount; i++)
            {
                seqStaffRoll[seqIndex].Update(gameTime.ElapsedGameTime);
            }

            // If sequence playback has finished, changes to the next sequence.
            // 
            // シーケンスが再生終了していれば次のシーケンスへ変更します。
            if (!seqStaffRoll[seqIndex].IsPlay || 
                buttons.B[VirtualKeyState.Push] || buttons.Back[VirtualKeyState.Push])
            {
                seqIndex++;
            }
        }


        /// <summary>
        /// Performs fade-out.
        /// 
        /// フェードアウト処理を行います。
        /// </summary>
        private void UpdateFadeOut()
        {
            // Sets the BackgroundMusic volume.
            // 
            // BackgroundMusicのボリュームを設定します。
            bgmVolume -= 0.01f;
            SoundComponent.SetVolume(bgm, bgmVolume);

            // If the BackgroundMusic volume reaches 0, ends the scene
            // and registers the game over scene.
            // 
            // BackgroundMusicボリュームが0になればシーンを終了し、
            // ゲームオーバーのシーンを登録します。
            if (bgmVolume < 0)
            {
                GameData.SceneQueue.Enqueue(new GameOver(Game));
                Dispose();
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

            // Performs drawing processing if the sequences are not all finished.
            // 
            // まだシーケンスが全て終了していなければ描画処理を行います。
            if (seqIndex < seqStaffRoll.Length)
            {
                Batch.Begin();
                seqStaffRoll[seqIndex].Draw(Batch, null);
                Batch.End();
            }

            base.Draw(gameTime);
        }
        #endregion

    }
}


