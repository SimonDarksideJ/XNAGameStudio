#region File Description
//-----------------------------------------------------------------------------
// NormalResult.cs
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
using MovipaLibrary;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene.Result
{
    /// <summary>
    /// Scene component displaying the Normal Mode results.
    /// It inherits ResultBase and loads the content to be used,
    /// then performs main update processing.
    /// Additional scores are obtained from completion results for 
    /// the specified stage and the count increments by one points. It also 
    /// implements a fast forward (100 x speed) function which is accessed 
    /// by pressing the A button. 
    /// Save data is recorded when the scene ends. 
    /// 
    /// ノーマルモードのリザルトを表示するシーンコンポーネントです。
    /// ResultBaseを継承し、使用するコンテントの読み込みと、
    /// メインの更新処理の内容をわけています。
    /// 指定されたステージのクリア結果から追加スコアを取得し、
    /// 1点ずつカウントしていく処理を行っていますが、Aボタンを押して
    /// 100倍の速度で早送りする処理も実装しています。
    /// このシーンの終了時にセーブデータを記録しています。
    /// </summary>
    public class NormalResult : ResultBase
    {
        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public NormalResult(Game game, StageResult stageResult)
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
            Navigate.Add(new NavigateData(AppSettings("A_Next"), true));

            base.InitializeNavigate();
        }


        /// <summary>
        /// Loads the content.
        /// 
        /// コンテントの読み込み処理を行います。
        /// </summary>
        protected override void LoadContent()
        {
            string asset = "Layout/Result/result_Scene";
            sceneData = Content.Load<SceneData>(asset);
            seqStart = sceneData.CreatePlaySeqData("ResultNormalStart");
            seqPosition = sceneData.CreatePlaySeqData("PosNormalStart");

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
            if (phase == Phase.Start)
            {
                if (!seqStart.IsPlay)
                {
                    phase = Phase.Select;
                }
            }
            else
            {
                UpdateScore();
            }

            base.UpdateMain(gameTime);
        }


        /// <summary>
        /// Calculates the score.
        /// 
        /// スコアの加算処理を行います。
        /// </summary>
        private void UpdateScore()
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            // Sets the number of score calculations.
            // The count is normally 1, but if the A button is 
            // pressed it becomes 100.
            // 
            // スコアの加算回数を設定します。
            // 通常は1回のカウントですが、Aボタンが押されていれば
            // 100回カウントするようにします。
            int loopCount = (buttons.A[VirtualKeyState.Press]) ? 100 : 1;

            // Calculates the score.
            // 
            // スコアの加算処理を行います。
            if (CalcScore(loopCount) == true)
            {
                // Plays the SoundEffect while the score calculation continues.
                // 
                // スコアの加算処理が継続している場合はSoundEffectを再生します。
                GameData.Sound.PlaySoundEffect(Sounds.ResultScore);
            }
            else
            {
                // Score calculation has finished; Navigate button
                // is displayed.
                // 
                // スコアの加算処理が終了したので、ナビゲートボタンを
                // 表示するようにします。
                drawNavigate = true;

                // Performs fade-out if the A button is pressed.
                // 
                // Aボタンが押されたらフェードアウトを行います。
                if (buttons.A[VirtualKeyState.Push])
                {
                    // Sets the next scene.
                    // 
                    // 次のシーンを設定します。
                    SetNextScene();

                    GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);
                    GameData.FadeSeqComponent.Start(FadeType.Normal, FadeMode.FadeOut);
                }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Calculates the score.
        /// 
        /// スコアの計算をします。
        /// </summary>
        private bool CalcScore(int loopCount)
        {
            bool addFlag = false;

            // Performs the designated number of loop repetitions.
            // 
            // 指定された回数分ループします。
            for (int i = 0; i < loopCount; i++)
            {
                if (CalcScore())
                {
                    addFlag = true;
                }
                else
                {
                    // Processing is suspended if the score is not updated.
                    // 
                    // スコアの更新が行われない場合は処理を中断します。
                    break;
                }
            }

            return addFlag;
        }


        /// <summary>
        /// Calculates the score.
        /// 
        /// スコアの計算をします。
        /// </summary>
        private bool CalcScore()
        {
            bool addFlag = false;

            if (result.SingleScore > 0)
            {
                addFlag = true;

                result.SingleScore--;
                GameData.SaveData.Score++;
            }
            else if (result.DoubleScore > 0)
            {
                addFlag = true;

                result.DoubleScore--;
                GameData.SaveData.Score++;
            }
            else if (result.HintScore > 0)
            {
                addFlag = true;

                result.HintScore--;
                GameData.SaveData.Score++;
            }

            return addFlag;
        }


        /// <summary>
        /// Checks if all stages have been completed, and readies the next scene.
        /// 
        /// 全てのステージをクリアしたかどうか判断し、次のシーンを用意します。
        /// </summary>
        private void SetNextScene()
        {
            // Obtains the stage settings.
            // 
            // ステージの設定を取得します。
            int stage = GameData.SaveData.Stage;

            // Sets the high score.
            // 
            // ハイスコアを設定します。
            if (GameData.SaveData.BestScore < GameData.SaveData.Score)
            {
                GameData.SaveData.BestScore = GameData.SaveData.Score;
            }
            
            // Checks if the next stage exists.
            // 
            // 次のステージが存在するかチェックします。
            if ((stage + 1) >= GameData.StageCollection.Count)
            {
                // Next stage does not exist; all stages have been completed.
                // 
                // 次のステージが存在しないので、全てのステージをクリアしました。

                // Sets the best time.
                // 
                // ベストタイムを設定します。
                if (GameData.SaveData.BestTime == TimeSpan.Zero ||
                    GameData.SaveData.BestTime > GameData.SaveData.TotalPlayTime)
                {
                    GameData.SaveData.BestTime = GameData.SaveData.TotalPlayTime;
                }
                
                // Initializes the stages.
                // 
                // ステージを初期化します。
                GameData.SaveData.Stage = 0;

                // Initializes the score.
                // 
                // スコアを初期化します。
                GameData.SaveData.Score = 0;

                // Registers the Staff Roll scenes.
                // 
                // スタッフロールのシーンを登録します。
                GameData.SceneQueue.Enqueue(new StaffRoll(Game));
            }
            else
            {
                // If the next stage exists, it is set.
                // 
                // ステージが存在するなら次のステージを設定します。
                stage++;
                GameData.SaveData.Stage = stage;

                // Obtains setting information for the next stage.
                // 
                // 次のステージの設定情報を取得します。
                StageSetting stageSetting = GameData.StageCollection[stage];

                // Registers the main game scenes.
                // 
                // メインゲームのシーンを登録します。
                GameData.SceneQueue.Enqueue(
                    new Puzzle.PuzzleComponent(Game, stageSetting));
            }

            // Saves the Save Data.
            // 
            // セーブデータを保存します。
            string filename = GameData.SaveData.FileName;
            string filePath = GameData.Storage.GetStoragePath(filename);
            SettingsSerializer.SaveSaveData(filePath, GameData.SaveData);
        }


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
                    return string.Format("{0:000000}", result.SingleScore);
                case 1:
                    return string.Format("{0:000000}", result.DoubleScore);
                case 2:
                    return string.Format("{0:000000}", result.HintScore);
                case 3:
                    return string.Format("{0:000000}", GameData.SaveData.Score);
            }

            return String.Empty;
        }
        #endregion
    }
}


