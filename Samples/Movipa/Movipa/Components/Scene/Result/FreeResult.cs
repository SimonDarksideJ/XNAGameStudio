#region File Description
//-----------------------------------------------------------------------------
// FreeResult.cs
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

namespace Movipa.Components.Scene.Result
{
    /// <summary>
    /// Scene component displaying the Free Mode results.
    /// It inherits ResultBase and loads the content to be used,
    /// then performs main update processing.
    ///
    /// フリーモードのリザルトを表示するシーンコンポーネントです。
    /// ResultBaseを継承し、使用するコンテントの読み込みと、
    /// メインの更新処理の内容をわけています。
    /// </summary>
    public class FreeResult : ResultBase
    {
        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public FreeResult(Game game, StageResult stageResult)
            : base(game, stageResult)
        {
        }


        /// <summary>
        /// Initializes the Navigate button.
        /// 
        /// ナビゲートボタンの初期化処理を行います。
        /// </summary>
        protected override void InitializeNavigate()
        {
            Navigate.Clear();
            Navigate.Add(new NavigateData(AppSettings("B_Title")));
            Navigate.Add(new NavigateData(AppSettings("A_Menu"), true));

            // Sets the Navigate button draw status.
            // 
            // ナビゲートボタンの描画状態を設定
            drawNavigate = true;

            base.InitializeNavigate();
        }


        /// <summary>
        /// Loads the content. 
        /// 
        /// コンテントの読み込み処理を行います。
        /// </summary>
        protected override void LoadContent()
        {
            // Loads the sequence data.
            // 
            // シーケンスデータの読み込み
            string asset = "Layout/Result/result_Scene";
            sceneData = Content.Load<SceneData>(asset);
            seqStart = sceneData.CreatePlaySeqData("ResultFreeStart");
            seqPosition = sceneData.CreatePlaySeqData("PosFreeStart");

            base.LoadContent();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Performs main update processing.
        /// 
        /// メインの更新処理を行います。
        /// </summary>
        protected override void UpdateMain(GameTime gameTime)
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            if (phase == Phase.Start)
            {
                // Sets to Select Processing when the sequence finishes.
                // 
                // シーケンスが終了したら、選択処理に設定します。
                if (!seqStart.IsPlay)
                {
                    phase = Phase.Select;
                }
            }
            else if (phase == Phase.Select)
            {
                if (buttons.A[VirtualKeyState.Push])
                {
                    // Performs menu transition when the A button is pressed.
                    // 
                    // Aボタンが押された場合はメニューに遷移します。
                    GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);
                    GameData.SceneQueue.Enqueue(new Menu.MenuComponent(Game));
                    GameData.FadeSeqComponent.Start(FadeType.Normal, FadeMode.FadeOut);
                }
                else if (buttons.B[VirtualKeyState.Push])
                {
                    // Performs title transition when the B button is pressed.
                    // 
                    // Bボタンが押された場合はタイトルに遷移します。
                    GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);
                    GameData.SceneQueue.Enqueue(new Title(Game));
                    GameData.FadeSeqComponent.Start(FadeType.Normal, FadeMode.FadeOut);
                }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Returns the text string to display in the sequence.
        /// 
        /// シーケンスに表示する文字列を返します。
        /// </summary>
        protected override string GetSequenceString(int id)
        {
            switch (id)
            {
                case 0:
                    return result.ClearTime.ToString().Substring(0, 8);
                case 1:
                    return string.Format("{0:000}", result.MoveCount);
            }

            return String.Empty;
        }
        #endregion

    }
}


