#region File Description
//-----------------------------------------------------------------------------
// SelectDivide.cs
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
using MovipaLibrary;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene.Menu
{
    /// <summary>
    /// Menu item used to specify the number of 
    /// divisions. It inherits MenuBaseance and 
    /// expands menu compilation processing. When
    /// rotation is off, the number of vertical and
    /// horizontal divisions can be specified. When rotation
    /// is on, only the number of vertical divisions
    /// may be specified. The number of horizontal divisions
    /// is automatically set to the maximum possible number
    /// relative to the vertical divisions.
    ///
    /// 分割数を設定するメニュー項目です。
    /// MenuBaseを継承し、メニューを構成する処理を拡張しています。
    /// 回転が無しの場合は縦と横の分割数を設定できますが、
    /// 回転がありに設定されていた場合は縦の分割数のみ設定可能で、
    /// 横の分割数は縦の分割数に応じた最大数を自動で設定します。
    /// </summary>
    public class SelectDivide : MenuBase
    {
        #region Private Types
        /// <summary>
        /// Processing status
        /// 
        /// 処理状態
        /// </summary>
        private enum Phase
        {
            /// <summary>
            /// Start
            ///
            ///開始演出
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

        /// <summary>
        /// Cursor position
        /// 
        /// カーソル位置
        /// </summary>
        private enum CursorPosition
        {
            /// <summary>
            /// Horizontal divisions
            /// 
            /// 横分割
            /// </summary>
            Left,

            /// <summary>
            /// Vertical divisions
            /// 
            /// 縦分割
            /// </summary>
            Right,

            /// <summary>
            /// Count
            /// 
            /// カウント用
            /// </summary>
            Count,
        }
        #endregion

        #region Fields
        // Maximum number of divisions
        //
        // 分割数最大値
        private readonly Point DivideMax;

        // Minimum number of divisions
        //
        // 分割数最小値
        private readonly Point DivideMin;

        // Movie draw position
        //
        // ムービー描画位置
        private readonly Rectangle MoviePreviewRect;

        // Cursor sphere position
        //
        // カーソル球体の位置
        private readonly Vector3[] SpherePositions;

        // Processing details
        //
        // 処理内容
        private Phase phase;

        // Cursor position
        //
        // カーソル位置
        private CursorPosition cursor;

        // Sequence
        // 
        // シーケンス
        private SequencePlayData seqStart;
        private SequencePlayData seqLoop;
        private SequencePlayData seqSelect;
        private SequencePlayData seqMovieWindow;
        private SequencePlayData seqLeftLoop;
        private SequencePlayData seqLeftUp;
        private SequencePlayData seqLeftDown;
        private SequencePlayData seqRightLoop;
        private SequencePlayData seqRightUp;
        private SequencePlayData seqRightDown;
        private SequencePlayData seqPosDivideWidthStart;
        private SequencePlayData seqPosDivideHeightStart;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public SelectDivide(Game game, MenuData data)
            : base(game, data)
        {
            // Loads the position.
            //
            // ポジションを読み込みます。
            PatternGroupData patternGroup = 
                data.sceneData.PatternGroupDictionary["Pos_PosMovie"];
            Point point;
            point = patternGroup.PatternObjectList[0].Position;
            MoviePreviewRect = new Rectangle(point.X, point.Y, 640, 360);

            // Sets the number of divisions to the maximum value.
            // 
            // 分割数の最大値を設定します。
            DivideMax = new Point(10, 6);

            // Sets the number of divisions to the minimum value.
            // 
            // 分割数の最小値を設定します。
            DivideMin = new Point(2, 2);

            // Sets the cursor position.
            // 
            // カーソルの位置を設定します。
            SpherePositions = new Vector3[] {
                new Vector3(-29.43751f, -47.44127f, 0),
                new Vector3(27.08988f, -47.44127f, 0)
            };
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

            // Initializes the sphere model.
            // 
            // 球体モデルの初期設定をします。
            Data.Spheres[1][0].Position += SpherePositions[0];
            Data.Spheres[1][0].Rotate = new Vector3(0, 0, MathHelper.ToRadians(90.0f));
            Data.Spheres[1][0].Alpha = 0.0f;
            Data.Spheres[1][0].Scale = Data.CursorSphereSize;

            // Initializes the cursor position.
            //
            // カーソルの初期位置を設定します。
            if (Data.StageSetting.Rotate == StageSetting.RotateMode.Off)
            {
                cursor = CursorPosition.Left;
            }
            else if (Data.StageSetting.Rotate == StageSetting.RotateMode.On)
            {
                // When rotation is on, only vertical divisions may be specified.
                // 
                // 回転有りの場合は縦分割のみ設定できます。
                cursor = CursorPosition.Right;
            }

            // Calculates the number of divisions.
            // 
            // 分割数の計算を行います。
            CalcDivide();

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
            SceneData scene = Data.sceneData;
            seqStart = scene.CreatePlaySeqData("DivideStart");
            seqLoop = scene.CreatePlaySeqData("DivideLoop");
            seqSelect = scene.CreatePlaySeqData("DivideSelect");
            seqMovieWindow = scene.CreatePlaySeqData("MovieWindow");
            seqLeftLoop = scene.CreatePlaySeqData("DivideLeftLoop");
            seqLeftUp = scene.CreatePlaySeqData("DivideLeftUp");
            seqLeftDown = scene.CreatePlaySeqData("DivideLeftDown");
            seqRightLoop = scene.CreatePlaySeqData("DivideRightLoop");
            seqRightUp = scene.CreatePlaySeqData("DivideRightUp");
            seqRightDown = scene.CreatePlaySeqData("DivideRightDown");
            seqPosDivideWidthStart = scene.CreatePlaySeqData("PosDivideWidthStart");
            seqPosDivideHeightStart = scene.CreatePlaySeqData("PosDivideHeightStart");

            seqStart.Replay();
            seqPosDivideWidthStart.Replay();
            seqPosDivideHeightStart.Replay();
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
            // シーケンスの更新を行います。
            UpdateSequence(gameTime);

            // Updates the model.
            // 
            // モデルの更新処理を行います。
            UpdateModels();

            if (phase == Phase.Start)
            {
                // Sets to selection process after start animation finishes.
                // 
                // 開始アニメーションが終了したら選択処理へ設定します。
                if (!seqStart.IsPlay)
                {
                    phase = Phase.Select;
                }
            }
            else if (phase == Phase.Select)
            {
                // Updates during selection.
                // 
                // 選択中の更新処理を行います。
                return UpdateSelect();
            }
            else if (phase == Phase.Selected)
            {
                // Switches to the confirmation screen when the selected animation ends.
                //
                // 選択アニメーションが終了したら確認画面へ繊維します。
                if (!seqSelect.IsPlay)
                {
                    return CreateMenu(Game, MenuType.Ready, Data);
                }
            }

            return null;
        }


        /// <summary>
        /// Updates during selection.
        /// 
        /// 選択中の更新処理を行います。
        /// </summary>
        private MenuBase UpdateSelect()
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;
            VirtualPadDPad dPad = virtualPad.DPad;
            VirtualPadDPad leftStick = virtualPad.ThumbSticks.Left;

            if (InputState.IsPushRepeat(dPad.Up, leftStick.Up))
            {
                // Down key increases the number of divisions.
                // 
                // 下キーが押されたら分割数を増やします。
                SetDivide(1);
            }
            else if (InputState.IsPushRepeat(dPad.Down, leftStick.Down))
            {
                // Down key decreases the number of divisions.
                //
                // 下キーが押されたら分割数を減らします。
                SetDivide(-1);
            }
            else if (InputState.IsPush(dPad.Left, leftStick.Left) ||
                InputState.IsPush(dPad.Right, leftStick.Right))
            {
                // Use common processing since there are only two items.
                // However, only the number of vertical divisions may be
                // specified when rotation is on, so the processing is 
                // performed only when rotation is off.
                //
                //項目が2つしかないので共通の処理を使用します。
                // ただし、回転が有りの場合は縦分割のみ設定可能なので、
                // 回転無しの場合のみ処理を行います。
                if (Data.StageSetting.Rotate == StageSetting.RotateMode.Off)
                {;
                    // Moves the cursor position.
                    // 
                    // カーソル位置を移動します。
                    cursor = CursorMove();

                    // SoundEffect playback
                    // 
                    // SoundEffectの再生
                    GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);
                }
            }
            else if (buttons.A[VirtualKeyState.Push])
            {
                // Enter button has been pressed; set processing state after selection.
                // 
                // 決定ボタンが押されたので、処理状態を選択後に設定します。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);
                seqSelect.Replay();
                phase = Phase.Selected;
            }
            else if (buttons.B[VirtualKeyState.Push])
            {
                // Cancel button has been pressed; return to movie selection.
                // 
                // キャンセルボタンが押されたのでムービー選択へ戻ります。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCancel);
                return CreateMenu(Game, MenuType.SelectMovie, Data);
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
            seqSelect.Update(gameTime.ElapsedGameTime);
            seqMovieWindow.Update(gameTime.ElapsedGameTime);
            seqLeftLoop.Update(gameTime.ElapsedGameTime);
            seqLeftUp.Update(gameTime.ElapsedGameTime);
            seqLeftDown.Update(gameTime.ElapsedGameTime);
            seqRightLoop.Update(gameTime.ElapsedGameTime);
            seqRightUp.Update(gameTime.ElapsedGameTime);
            seqRightDown.Update(gameTime.ElapsedGameTime);
            seqPosDivideWidthStart.Update(gameTime.ElapsedGameTime);
            seqPosDivideHeightStart.Update(gameTime.ElapsedGameTime);
        }


        /// <summary>
        /// Updates the model.
        ///
        /// モデルの更新処理を行います。
        /// </summary>
        private void UpdateModels()
        {
            // Updates the position.
            // 
            // ポジションの更新を行います。
            Vector3 position = Data.Spheres[1][0].Position;
            if (cursor == CursorPosition.Left)
            {
                position += (SpherePositions[0] - position) * 0.2f;
            }
            else if (cursor == CursorPosition.Right)
            {
                position += (SpherePositions[1] - position) * 0.2f;
            }
            Data.Spheres[1][0].Position = position;

            // Specifies the transparency and scale settings.
            //
            // 透明度とスケールの設定をします。
            float alpha = Data.Spheres[1][0].Alpha;
            float fadeSpeed = Data.CursorSphereFadeSpeed;
            if (phase != Phase.Selected)
            {
                alpha = MathHelper.Clamp(alpha + fadeSpeed, 0.0f, 1.0f);
            }
            else
            {
                // Performs fade-out with zoom when item is selected.
                // 
                // 項目が決定されるとズームしながらフェードアウトします。
                alpha = MathHelper.Clamp(alpha - fadeSpeed, 0.0f, 1.0f);
                Data.Spheres[1][0].Scale += Data.CursorSphereZoomSpeed;
            }
            Data.Spheres[1][0].Alpha = alpha;
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
            // Draws the model.
            // 
            // モデルの描画を行います。
            DrawModels(batch);

            // Draws the movie window.
            // 
            // ムービーウィンドウを描画します。
            batch.Begin();
            seqMovieWindow.Draw(batch, null);
            batch.End();

            // Draws the division preview.
            // 
            // 分割プレビューを描画します。
            DrawDivideTexture(batch);

            // Draws the sequence.
            // 
            // シーケンスの描画を行います。
            batch.Begin();
            DrawSequence(gameTime, batch);
            batch.End();
        }


        /// <summary>
        /// Draws the model.
        /// 
        /// モデルの描画を行います。
        /// </summary>
        private void DrawModels(SpriteBatch batch)
        {
            GraphicsDevice graphicsDevice = batch.GraphicsDevice;
            Matrix view;
            view = Matrix.CreateLookAt(Data.CameraPosition, Vector3.Zero, Vector3.Up);
            Data.Spheres[1][0].SetRenderState(graphicsDevice, SpriteBlendMode.Additive);
            Data.Spheres[1][0].Draw(view, GameData.Projection);
        }


        /// <summary>
        /// Draws the division preview.
        ///
        /// 分割プレビューを描画します。
        /// </summary>
        private void DrawDivideTexture(SpriteBatch batch)
        {
            // Processing is not performed if the texture is not set. 
            // 
            // テクスチャが設定されていない場合は処理を行いません。
            if (Data.divideTexture == null)
                return;

            // Draws division preview with no alpha.
            // 
            // アルファ無しで分割プレビューを描画します。
            batch.Begin(SpriteBlendMode.None);
            batch.Draw(Data.divideTexture, MoviePreviewRect, Color.White);
            batch.End();
        }


        /// <summary>
        /// Draws the sequence.
        /// 
        /// シーケンスの描画を行います。
        /// </summary>
        private void DrawSequence(GameTime gameTime, SpriteBatch batch)
        {
            if (phase == Phase.Start)
            {
                seqStart.Draw(batch, null);
                DrawSequenceString(batch);
            }
            else if (phase == Phase.Select)
            {
                seqLoop.Draw(batch, null);
                seqLeftLoop.Draw(batch, null);
                seqRightLoop.Draw(batch, null);
                seqLeftUp.Draw(batch, null);
                seqLeftDown.Draw(batch, null);
                seqRightUp.Draw(batch, null);
                seqRightDown.Draw(batch, null);
                DrawSequenceString(batch);

                // Draws the navigate button.
                //
                // ナビゲートボタンの描画を行います。
                DrawNavigate(gameTime, batch, false);
            }
            else if (phase == Phase.Selected)
            {
                seqLoop.Draw(batch, null);
                seqSelect.Draw(batch, null);
                DrawSequenceString(batch);
            }
        }


        /// <summary>
        /// Draws the sequence text string.
        /// 
        /// シーケンスの文字列を描画します。
        /// </summary>
        private void DrawSequenceString(SpriteBatch batch)
        {
            SequenceBankData sequenceBank;

            // Horizontal division text string
            // 
            // 横分割の文字列
            sequenceBank = seqPosDivideWidthStart.SequenceData;
            DrawSequenceString(batch, sequenceBank, Data.StageSetting.Divide.X);

            // Vertical division text string
            // 
            // 縦分割の文字列
            sequenceBank = seqPosDivideHeightStart.SequenceData;
            DrawSequenceString(batch, sequenceBank, Data.StageSetting.Divide.Y);
        }


        /// <summary>
        /// Draws the sequence text string.
        /// 
        /// シーケンスの文字列を描画します。
        /// </summary>
        private void DrawSequenceString(SpriteBatch batch, 
            SequenceBankData sequenceBank, int value)
        {
            foreach (SequenceGroupData seqBodyData in sequenceBank.SequenceGroupList)
            {
                SequenceObjectData seqPartsData = seqBodyData.CurrentObjectList;
                if (seqPartsData == null)
                {
                    continue;
                }

                foreach (PatternObjectData patPartsData in
                    seqPartsData.PatternObjectList)
                {
                    DrawData putInfoData = patPartsData.InterpolationDrawData;
                    SpriteFont font = LargeFont;
                    Color color = putInfoData.Color;
                    Point point = putInfoData.Position;
                    Vector2 position = new Vector2(point.X, point.Y);
                    string text = string.Format("{0:00}", value);
                    batch.DrawString(font, text, position, color);
                }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Calculates the number of divisions.
        /// 
        /// 分割数を計算します。
        /// </summary>
        private void CalcDivide()
        {
            // Creates the panel.
            // 
            // パネルを作成します。
            Data.PanelManager.CreatePanel(GameData.MovieSizePoint, Data.StageSetting);

            // When rotation is off, end without changing.
            // When rotation is on, calculate the optimum number of horizontal divisions.
            // 
            // 回転がオフの状態の時はそのまま終了します。
            // 回転がオンの時は横の分割数が最適になるように計算します。
            if (Data.StageSetting.Rotate == StageSetting.RotateMode.Off)
                return;

            // Obtains the number of divisions.
            //
            // 分割数を取得します。
            Point divide = Data.StageSetting.Divide;

            // Recalculates using the minimum number of horizontal divisions. 
            //
            // 横分割数を最小値にして再計算します。
            divide.X = DivideMin.X;
            Data.StageSetting.Divide = divide;
            Data.PanelManager.CreatePanel(GameData.MovieSizePoint, Data.StageSetting);

            // Divides the movie width by the width of the rectangle. 
            //
            // ムービーの横幅を矩形の横幅で割ります。
            int movieWidth = GameData.MovieSizePoint.X;
            int panelWidth = (int)Data.PanelManager.PanelSize.X;
            int widthCount = (int)(movieWidth / panelWidth);

            // Updates the number of divisions to the calculation result.
            //
            // 割った数を分割数に再設定します。
            divide.X = widthCount;
            Data.StageSetting.Divide = divide;

            // Recalculates the panel. 
            //
            // パネルを再計算します。
            Data.PanelManager.CreatePanel(GameData.MovieSizePoint, Data.StageSetting);
        }


        /// <summary>
        /// Moves the cursor position.
        /// 
        /// カーソル位置を移動します。
        /// </summary>
        private CursorPosition CursorMove()
        {
            int length = (int)CursorPosition.Count;
            return (CursorPosition)(((int)cursor + 1) % length);
        }


        /// <summary>
        /// Sets the number of divisions.
        ///
        /// 分割数の設定をします。
        /// </summary>
        private void SetDivide(int offset)
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadTriggers triggers = virtualPad.Triggers;

            Point divide = Data.StageSetting.Divide;

            if (cursor == CursorPosition.Left)
            {
                // Calculates the number of horizontal divisions.
                //
                // 横の分割数を計算します。
                divide.X += offset;

                // Replays the sequence.
                //
                // シーケンスをリプレイします。
                seqLeftUp.Replay();
            }
            else if (cursor == CursorPosition.Right)
            {
                // Calculates the number of vertical divisions.
                //
                // 縦の分割数を計算します。
                divide.Y += offset;

                // Replays the sequence.
                // 
                // シーケンスをリプレイします。
                seqRightUp.Replay();
            }

            // Limits the number of divisions.
            // The limitation is cancelled if the L trigger or R trigger are pressed.
            // 
            // 分割数の制限をします。
            // LトリガーとRトリガーが押されていた場合は制限を解除します。
            if (triggers.Left[VirtualKeyState.Free] || 
                triggers.Right[VirtualKeyState.Free])
            {
                int max;
                int min;

                max = DivideMax.X;
                min = DivideMin.X;
                divide.X = (divide.X > max) ? max : divide.X;
                divide.X = (divide.X < min) ? min : divide.X;

                max = DivideMax.Y;
                min = DivideMin.Y;
                divide.Y = (divide.Y > max) ? max : divide.Y;
                divide.Y = (divide.Y < min) ? min : divide.Y;
            }

            // Sets the number of divisions.
            //
            // 分割数を設定します。
            Data.StageSetting.Divide = divide;

            // Re-creates the panel.
            // 
            // パネルを再作成します。
            CalcDivide();

            // Plays the SoundEffect for change in number of divisions.
            // 
            // 分割数変更のSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);
        }
        #endregion
    }
}
