#region File Description
//-----------------------------------------------------------------------------
// SelectMode.cs
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
    /// Menu item for processing mode selection.
    /// It inherits MenuBase and expands menu compilation.
    /// This class implements menu mode selection, provides sequence control
    /// and moves the model used as the cursor. 
    /// 
    /// モード選択を処理するメニュー項目です。
    /// MenuBaseを継承し、メニューを構成する処理を拡張しています。
    /// このクラスはメニューのモード選択を実装し、シーケンスの制御と
    /// カーソルに使用しているモデルの移動を行っています。
    /// </summary>
    public class SelectMode : MenuBase
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
            /// Selected
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
            /// Normal mode
            /// 
            /// ノーマルモード
            /// </summary>
            Normal,

            /// <summary>
            /// Free mode
            ///
            /// フリーモード
            /// </summary>
            Free,

            /// <summary>
            /// Count
            ///
            /// カウント用
            /// </summary>
            Count
        }
        #endregion

        #region Fields
        /// <summary>
        /// Cursor sphere position
        /// 
        /// カーソルの球体の位置
        /// </summary>
        private readonly Vector3[] SpherePositions;

        // Cursor position
        // 
        // カーソル位置
        private CursorPosition cursor;

        // Processing status
        // 
        // 処理状態
        private Phase phase;

        // Sequence
        //
        // シーケンス
        private SequencePlayData seqStart;
        private SequencePlayData seqBg;
        private SequencePlayData seqNormal;
        private SequencePlayData seqNormalLoop;
        private SequencePlayData seqNormalSelect;
        private SequencePlayData seqFree;
        private SequencePlayData seqFreeLoop;
        private SequencePlayData seqFreeSelect;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public SelectMode(Game game, MenuData data)
            : base(game, data)
        {
            // Sets the cursor position.
            // 
            // カーソルの位置を設定します。
            SpherePositions = new Vector3[] {
                new Vector3(0, -4.56056f, 0),
                new Vector3(0, -24.94905f, 0)
            };
        }


        /// <summary>
        /// Performs initialization processing.
        /// 
        /// 初期化処理を行います。
        /// </summary>
        public override void Initialize()
        {
            // Initializes the sphere models.
            // 
            // 球体モデルの初期設定をします。
            Data.Spheres[1][0].Position = SpherePositions[0];
            Data.Spheres[1][0].Rotate = new Vector3();
            Data.Spheres[1][0].Alpha = 0.0f;
            Data.Spheres[1][0].Scale = Data.CursorSphereSize;

            // Initializes the sequence.
            // 
            // シーケンスを初期化します。
            InitializeSequence();

            // Sets the initial cursor position.
            // 
            // カーソルの初期位置を設定します。
            cursor = CursorPosition.Normal;

            // Initializes the processing status.
            // 
            // 処理状態の初期設定をします。
            phase = Phase.Start;

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
            seqStart = Data.sceneData.CreatePlaySeqData("ModeStart");
            seqBg = Data.sceneData.CreatePlaySeqData("ModeLoop");
            seqNormal = Data.sceneData.CreatePlaySeqData("ModeNormal");
            seqNormalLoop = Data.sceneData.CreatePlaySeqData("ModeNormalLoop");
            seqNormalSelect = Data.sceneData.CreatePlaySeqData("ModeNormalSelect");
            seqFree = Data.sceneData.CreatePlaySeqData("ModeFree");
            seqFreeLoop = Data.sceneData.CreatePlaySeqData("ModeFreeLoop");
            seqFreeSelect = Data.sceneData.CreatePlaySeqData("ModeFreeSelect");

            // Plays the sequence from the start.
            // 
            // シーケンスをはじめから再生します。
            seqStart.Replay();
            seqStart.Update(new TimeSpan());
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
            // Processing is not performed if initialization is incomplete.
            // 
            // 初期化が未完了の場合は処理を行いません。
            if (!Initialized)
                return null;

            // Updates the sequence.
            // 
            // シーケンスの更新処理を行います。
            UpdateSequence(gameTime);

            // Updates the sphere models.
            // 
            // 球体モデルの更新処理を行います。
            UpdateModels();

            if (phase == Phase.Start)
            {
                // Changes to selection after start animation finishes.
                // 
                // 開始アニメーションが終了したら選択処理に変更します。
                if (!seqStart.SequenceData.IsPlay)
                {
                    phase = Phase.Select;
                }
            }
            else if (phase == Phase.Select)
            {
                // Updates at selection.
                //
                // 選択時の更新処理を行います。
                UpdateSelect(gameTime);
            }
            else if (phase == Phase.Selected)
            {
                // Updates when selection ends.
                //
                // 選択終了時の更新処理を行います。
                return UpdateSelected();
            }

            return null;
        }


        /// <summary>
        /// Updates at selection.
        /// 
        /// 選択時の更新処理を行います。
        /// </summary>
        private void UpdateSelect(GameTime gameTime)
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;
            VirtualPadDPad dPad = virtualPad.DPad;
            VirtualPadDPad leftStick = virtualPad.ThumbSticks.Left;

            if (InputState.IsPush(dPad.Up, leftStick.Up, dPad.Down, leftStick.Down))
            {
                // Moves the cursor.
                // Use common processing since there are only two items.
                // 
                // カーソルを移動します。
                // 項目が2つしか無いので共通の処理を使用します。
                cursor = CursorMove();

                // Plays the SoundEffect.
                // 
                // SoundEffectを再生します。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);

                // Replays the selected cursor sequence.
                //
                // 選択されているカーソルのシーケンスをリプレイします。
                ReplaySequence(gameTime);
            }
            else if (buttons.A[VirtualKeyState.Push])
            {
                // Enter key pressed; process changes.
                //
                // 決定キーが押されたので、処理を変更します。
                phase = Phase.Selected;

                // Plays the SoundEffect.
                // 
                // SoundEffectを再生します。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);

                // Sets game mode and replays the selected item sequence.
                //
                // ゲームモードを設定し、選択された項目のシーケンスをリプレイします。
                if (cursor == CursorPosition.Normal)
                {
                    Data.StageSetting.Mode = StageSetting.ModeList.Normal;

                    seqNormalSelect.Replay();
                    seqNormalSelect.Update(gameTime.ElapsedGameTime);
                }
                else if (cursor == CursorPosition.Free)
                {
                    Data.StageSetting.Mode = StageSetting.ModeList.Free;

                    seqFreeSelect.Replay();
                    seqFreeSelect.Update(gameTime.ElapsedGameTime);
                }
            }
            else if (buttons.B[VirtualKeyState.Push])
            {
                // Cancel button pressed; return to title.
                // 
                // キャンセルボタンが押されたので、タイトルに戻ります。
                GameComponent next = new Movipa.Components.Scene.Title(Game);
                GameData.SceneQueue.Enqueue(next);
                GameData.FadeSeqComponent.Start(FadeType.Normal, FadeMode.FadeOut);
            }
        }


        /// <summary>
        /// Updates when selection ends.
        /// 
        /// 選択終了時の更新処理を行います。
        /// </summary>
        private MenuBase UpdateSelected()
        {
            if (cursor == CursorPosition.Normal)
            {
                // Switches to the file selection menu when normal mode is 
                // selected by the cursor and the selected animation has finished.
                // 
                // カーソルがノーマルモードを選択していて、選択アニメーションが
                // 終了したら、ファイル選択のメニューに遷移します。
                if (!seqNormalSelect.SequenceData.IsPlay)
                {
                    return CreateMenu(Game, MenuType.SelectFile, Data);
                }
            }
            else if (cursor == CursorPosition.Free)
            {
                // Switches to the style selection menu when free mode is 
                // selected by the cursor and the selected animation has finished.
                // 
                // カーソルがフリーモードを選択していて、選択アニメーションが
                // 終了したら、スタイル選択のメニューに遷移します。
                if (!seqFreeSelect.SequenceData.IsPlay)
                {
                    return CreateMenu(Game, MenuType.SelectStyle, Data);
                }
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
            TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;

            seqStart.Update(elapsedGameTime);
            seqBg.Update(elapsedGameTime);
            seqNormal.Update(elapsedGameTime);
            seqNormalLoop.Update(elapsedGameTime);
            seqNormalSelect.Update(elapsedGameTime);
            seqFree.Update(elapsedGameTime);
            seqFreeLoop.Update(elapsedGameTime);
            seqFreeSelect.Update(elapsedGameTime);
        }


        /// <summary>
        /// Updates the model.
        /// 
        /// モデルの更新処理を行います。
        /// </summary>
        private void UpdateModels()
        {
            // Moves the position to the cursor location. 
            // 
            // カーソル位置へポジションを移動します。
            Vector3 position = Data.Spheres[1][0].Position;
            if (cursor == CursorPosition.Normal)
            {
                position += (SpherePositions[0] - position) * 0.2f;
            }
            else if (cursor == CursorPosition.Free)
            {
                position += (SpherePositions[1] - position) * 0.2f;
            }
            Data.Spheres[1][0].Position = position;

            // Modifies the transparency and scale settings. 
            // 
            // 透明度とスケールの変更をします。
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

            batch.Begin();
            if (phase == Phase.Start)
            {
                seqStart.Draw(batch, null);
            }
            else if (phase == Phase.Select)
            {
                // Draws menu name and other sequences.
                // 
                // メニュー名などのシーケンスを描画します。
                seqBg.Draw(batch, null);

                // Draws the item sequence.
                // 
                // 項目のシーケンスを描画します。
                if (cursor == CursorPosition.Normal)
                {
                    seqNormalLoop.Draw(batch, null);
                }
                else if (cursor == CursorPosition.Free)
                {
                    seqFreeLoop.Draw(batch, null);
                }
                seqNormal.Draw(batch, null);
                seqFree.Draw(batch, null);

                // Draws the navigate button.
                // 
                // ナビゲートボタンを描画します。
                DrawNavigate(gameTime, batch, false);
            }
            else if (phase == Phase.Selected)
            {
                // Draws menu name and other sequences.
                // 
                // メニュー名などのシーケンスを描画します。
                seqBg.Draw(batch, null);

                // Draws the item animation.
                // 
                // 項目の決定アニメーションを描画します。
                if (cursor == CursorPosition.Normal)
                {
                    seqNormalSelect.Draw(batch, null);
                }
                else if (cursor == CursorPosition.Free)
                {
                    seqFreeSelect.Draw(batch, null);
                }
            }

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
        #endregion

        #region Helper Methods
        /// <summary>
        /// Returns to the next cursor position.
        /// 
        /// カーソルの次の位置を返します。
        /// </summary>
        private CursorPosition CursorMove()
        {
            return (CursorPosition)(((int)cursor + 1) % (int)CursorPosition.Count);
        }


        /// <summary>
        /// Replays the selected cursor sequence.
        /// 
        /// 選択されているカーソルのシーケンスをリプレイします。
        /// </summary>
        private void ReplaySequence(GameTime gameTime)
        {
            if (cursor == CursorPosition.Normal)
            {
                seqNormal.Replay();
                seqNormal.Update(gameTime.ElapsedGameTime);
            }
            else if (cursor == CursorPosition.Free)
            {
                seqFree.Replay();
                seqFree.Update(gameTime.ElapsedGameTime);
            }
        }
        #endregion
    }
}
