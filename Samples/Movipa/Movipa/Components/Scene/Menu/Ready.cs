#region File Description
//-----------------------------------------------------------------------------
// Ready.cs
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
using Movipa.Util;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene.Menu
{
    /// <summary>
    /// This menu is used to display the confirmation
    /// screen. It inherits MenuBaseance 
    /// and expands menu compilation processing. Menu 
    /// selection is followed by fade-out and 
    /// transition to the puzzle scene.
    /// 
    /// 確認の画面を表示するメニュー
    /// MenuBaseを継承し、メニューを構成する処理を拡張しています。
    /// このメニューで決定されれば、フェードアウトし、パズルのシーンへ
    /// 遷移します。
    /// </summary>
    public class Ready : MenuBase
    {
        #region Private Types
        /// <summary>
        ///Processing status
        ///
        /// 処理状態
        /// </summary>
        private enum Phase
        {
            /// <summary>
            /// Start
            ///
            /// 開始演出
            /// </summary>
            Start,

            /// <summary>
            /// Normal processing
            ///
            /// 通常処理
            /// </summary>
            Select,

            /// <summary>
            /// Select
            ///
            /// 選択演出
            /// </summary>
            Selected,
        }
        #endregion

        #region Fields
        // Movie render area
        //
        // ムービーの描画領域
        private readonly Rectangle MoviePreviewRect;

        // Processing details
        //
        //処理内容
        private Phase phase;

        // Sequence
        // 
        // シーケンス
        private SequencePlayData seqStart;
        private SequencePlayData seqLoop;
        private SequencePlayData seqMovieWindow;
        private SequencePlayData seqPosStart;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        /// <param name="data">
        /// Common data structure
        ///
        /// 共通データ構造体
        /// </param>
        public Ready(Game game, MenuData data)
            : base(game, data)
        {
            // Loads the position.
            //
            // ポジションを読み込みます。
            Point point;
            PatternGroupData patternGroup;
            patternGroup = data.sceneData.PatternGroupDictionary["Pos_PosMovie"];
            point = patternGroup.PatternObjectList[0].Position;
            MoviePreviewRect = new Rectangle(point.X, point.Y, 640, 360);
        }


        /// <summary>
        /// Performs initialization processing.
        ///
        /// 初期化の処理を行います。
        /// </summary>
        public override void Initialize()
        {
            // Initializes the processing status.
            //
            // 処理状態の初期設定をします。
            phase = Phase.Start;

            // Initializes the sequence.
            //
            // シーケンスの初期化を行います。
            InitializeSequence();

            base.Initialize();
        }


        /// <summary>
        /// Initializes the navigate.
        /// 
        /// ナビゲートの初期化をします。
        /// </summary>
        protected override void InitializeNavigate()
        {
            Navigate.Clear();
            Navigate.Add(new NavigateData(AppSettings("B_Cancel"), false));
            Navigate.Add(new NavigateData(AppSettings("A_Ok"), true));
        }


        /// <summary>
        /// Initializes the sequence.
        ///
        /// シーケンスの初期化を行います。
        /// </summary>
        private void InitializeSequence()
        {
            seqStart = Data.sceneData.CreatePlaySeqData("ReadyStart");
            seqLoop = Data.sceneData.CreatePlaySeqData("ReadyLoop");
            seqMovieWindow = Data.sceneData.CreatePlaySeqData("MovieWindow");
            seqPosStart = Data.sceneData.CreatePlaySeqData("PosReadyStart");

            seqStart.Replay();
            seqPosStart.Replay();
        }

        #endregion

        #region Update Methods
        /// <summary>
        /// Performs update processing.
        ///
        /// 更新処理を行います。
        /// </summary>
        public override MenuBase UpdateMain(GameTime gameTime)
        {
            // Updates the sequence.
            //
            // シーケンスの更新
            UpdateSequence(gameTime);

            if (phase == Phase.Start)
            {
                // Sets to selection process after start animation finishes.
                // 
                // 開始アニメーションが終了したら選択処理に設定します。
                if (!seqStart.SequenceData.IsPlay)
                {
                    phase = Phase.Select;
                }
            }
            else if (phase == Phase.Select)
            {
                // Performs update processing at selection.
                //
                // 選択時の更新処理を行います。
                return UpdateSelect();
            }
            else if (phase == Phase.Selected)
            {
                // Registers puzzle scene then perform fade-out.
                // 
                // パズルのシーンを登録して、フェードアウトします。
                GameData.SceneQueue.Enqueue(
                    new Puzzle.PuzzleComponent(Game, Data.StageSetting));
                GameData.FadeSeqComponent.Start(FadeType.Normal, FadeMode.FadeOut);
            }

            return null;
        }


        /// <summary>
        /// Performs update processing at selection.
        /// 
        /// 選択時の更新処理を行います。
        /// </summary>
        private MenuBase UpdateSelect()
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            if (buttons.A[VirtualKeyState.Push])
            {
                // Performs the post-selection process when the enter button is pressed. 
                //
                // 決定ボタンが押されたら選択後の処理に設定します。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);
                phase = Phase.Selected;
            }
            else if (buttons.B[VirtualKeyState.Push])
            {
                // Returns to split settings when the cancel button is pressed.
                // 
                // キャンセルボタンが押されたので、分割数の設定に戻ります。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCancel);
                return CreateMenu(Game, MenuType.SelectDivide, Data);
            }

            return null;
        }


        /// <summary>
        /// Updates the sequence.
        ///
        /// シーケンスの更新処理を行います。
        /// </summary>
        private void UpdateSequence(GameTime gameTime)
        {
            seqStart.Update(gameTime.ElapsedGameTime);
            seqLoop.Update(gameTime.ElapsedGameTime);
            seqMovieWindow.Update(gameTime.ElapsedGameTime);
            seqPosStart.Update(gameTime.ElapsedGameTime);
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Performs render processing.
        ///
        /// 描画処理を行います。
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch batch)
        {
            // Draws the movie window. 
            //
            // ムービーウィンドウを描画します。
            batch.Begin();
            seqMovieWindow.Draw(batch, null);
            batch.End();

            // Draws the split preview. 
            //
            // 分割プレビューを描画します。
            DrawDivideTexture(batch);

            batch.Begin();

            if (phase == Phase.Start)
            {
                seqStart.Draw(batch, null);
            }
            else if (phase == Phase.Select)
            {
                seqLoop.Draw(batch, null);

                // Draws the navigate.
                // 
                // ナビゲートの描画
                DrawNavigate(gameTime, batch, false);
            }
            else if (phase == Phase.Selected)
            {
                seqLoop.Draw(batch, null);
            }

            DrawSequenceString(batch);

            batch.End();
        }


        /// <summary>
        /// Draws the split preview.
        /// 
        /// 分割プレビューを描画します。
        /// </summary>
        private void DrawDivideTexture(SpriteBatch batch)
        {
            // Unable to draw if no texture.
            // 
            // テクスチャが無い場合は描画処理を行いません。
            if (Data.divideTexture == null)
                return;

            // Draws with no alpha.
            //
            // アルファ無しで描画をします。
            batch.Begin(SpriteBlendMode.None);
            batch.Draw(Data.divideTexture, MoviePreviewRect, Color.White);
            batch.End();
        }


        /// <summary>
        /// Draws text string based on sequence.
        /// 
        /// シーケンスを元に文字列を描画します。
        /// </summary>
        private void DrawSequenceString(SpriteBatch batch)
        {
            SequenceBankData seqData;
            seqData = seqPosStart.SequenceData;
            foreach (SequenceGroupData seqBodyData in seqData.SequenceGroupList)
            {
                SequenceObjectData seqPartsData = seqBodyData.CurrentObjectList;
                if (seqPartsData == null)
                {
                    continue;
                }

                List<PatternObjectData> list = seqPartsData.PatternObjectList;
                foreach (PatternObjectData patPartsData in list)
                {
                    DrawData putInfoData = patPartsData.InterpolationDrawData;
                    SpriteFont font = MediumFont;
                    Color color = putInfoData.Color;

                    string text = GetDrawString();
                    Point point = putInfoData.Position;
                    Vector2 position = new Vector2(point.X, point.Y);
                    position -= font.MeasureString(text) * 0.5f;

                    batch.DrawString(font, text, position, color);
                }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Obtains the text string for drawing.
        /// 
        /// 描画用の文字列を取得します。
        /// </summary>
        /// <returns>
        /// Text string for drawing
        ///
        /// 描画用の文字列
        /// </returns>
        private string GetDrawString()
        {
            Point divide = Data.StageSetting.Divide;

            string text = String.Empty;
            text += string.Format("Style : {0}\n", Data.StageSetting.Style);
            text += string.Format("Rotate : {0}\n", Data.StageSetting.Rotate);
            text += string.Format("Movie : {0}\n", Data.movie.Info.Name);
            text += string.Format("Divide : {0:00}x{1:00}", divide.X, divide.Y);

            return text;
        }
        #endregion
    }
}
