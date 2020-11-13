#region File Description
//-----------------------------------------------------------------------------
// SelectStyle.cs
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
    /// Menu item used for style selection.
    /// It inherits MenuBase and expands menu compilation processing.
    /// This class implements menu style selection and rotation selection, and
    /// provides sequence control and cursor model movement.
    /// 
    /// スタイル選択を処理するメニュー項目です。
    /// MenuBaseを継承し、メニューを構成する処理を拡張しています。
    /// このクラスはメニューのスタイル選択と、回転の有無の選択を実装し、
    /// シーケンスの制御とカーソルに使用しているモデルの移動を行っています。
    /// </summary>
    public class SelectStyle : MenuBase
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
            /// Style selection
            /// 
            /// スタイル選択
            /// </summary>
            StyleSelect,

            /// <summary>
            /// Style selected
            ///
            /// スタイル選択演出
            /// </summary>
            StyleSelected,

            /// <summary>
            /// Rotation item start
            ///
            /// 回転項目開始演出
            /// </summary>
            RotateStart,

            /// <summary>
            /// Rotate selection
            ///
            /// 回転選択
            /// </summary>
            RotateSelect,

            /// <summary>
            /// Rotate selected
            ///
            /// 回転選択演出
            /// </summary>
            RotateSelected,
        }


        /// <summary>
        /// Style item
        /// 
        /// スタイルの項目
        /// </summary>
        private enum CursorStyle
        {
            /// <summary>
            /// Change mode
            /// 
            /// チェンジモード
            /// </summary>
            Change,

            /// <summary>
            /// Revolve mode
            ///
            /// リボルヴモード
            /// </summary>
            Revolve,

            /// <summary>
            /// Slide mode
            /// 
            /// スライドモード
            /// </summary>
            Slide,

            /// <summary>
            /// Count
            ///
            /// カウント用
            /// </summary>
            Count,
        }


        /// <summary>
        /// Rotate item
        /// 
        /// 回転の項目
        /// </summary>
        private enum CursorRotate
        {
            /// <summary>
            /// Rotate on
            /// 
            /// 回転有り
            /// </summary>
            On,

            /// <summary>
            /// Rotate off
            ///
            /// 回転無し
            /// </summary>
            Off,

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
        /// Style animation draw position
        /// 
        /// スタイルアニメーションの描画位置
        /// </summary>
        private readonly Vector2 PositionStyleAnimation;

        /// <summary>
        /// Cursor sphere position
        ///
        /// カーソル球体の位置
        /// </summary>
        private readonly Vector3[][] SpherePositions;

        // Processing status
        // 
        // 処理状況
        private Phase phase;

        // Cursor position
        //
        // カーソル位置
        private CursorStyle cursorStyle;
        private CursorRotate cursorRotate;

        // Layout
        private SequencePlayData seqStart;
        private SequencePlayData seqLoop;
        private SequencePlayData seqChange;
        private SequencePlayData seqChangeLoop;
        private SequencePlayData seqChangeSelect;
        private SequencePlayData seqRevolve;
        private SequencePlayData seqRevolveLoop;
        private SequencePlayData seqRevolveSelect;
        private SequencePlayData seqSlide;
        private SequencePlayData seqSlideLoop;
        private SequencePlayData seqSlideSelect;
        private SequencePlayData seqRotateStart;
        private SequencePlayData seqRotateLoop;
        private SequencePlayData seqRotateOn;
        private SequencePlayData seqRotateOnLoop;
        private SequencePlayData seqRotateOff;
        private SequencePlayData seqRotateOffLoop;
        private SequencePlayData seqRotateOnSelect;
        private SequencePlayData seqRotateOffSelect;

        private SceneData sceneStyleChangeRotateOn;
        private SequencePlayData seqStyleChangeRotateOn;

        private SceneData sceneStyleChangeRotateOff;
        private SequencePlayData seqStyleChangeRotateOff;

        private SceneData sceneStyleRevolve;
        private SequencePlayData seqStyleRevolve;

        private SceneData sceneStyleSlide;
        private SequencePlayData seqStyleSlide;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public SelectStyle(Game game, MenuData data)
            : base(game, data)
        {
            // Loads the position.
            // 
            // ポジションを読み込みます。
            PatternGroupData patternGroup = 
                data.sceneData.PatternGroupDictionary["Pos_PosStyle"];
            Point point;
            point = patternGroup.PatternObjectList[0].Position;
            PositionStyleAnimation = new Vector2(point.X, point.Y);

            // Sets the cursor position.
            //
            // カーソル位置を設定します。
            SpherePositions = new Vector3[][] {
                // Select Style
                new Vector3[] {
                    new Vector3(-40.22693f, 4.972153f, 0),
                    new Vector3(-40.22693f, -13.60015f, 0),
                    new Vector3(-40.22693f, -32.24725f, 0) },
                // Rotate Select
                new Vector3[] {
                    new Vector3(-20.6443f, -11.82219f, 0),
                    new Vector3(-20.6443f, -22.5499f, 0) },
            };
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
            // 処理状態の初期設定をします。
            phase = Phase.Start;

            // Initializes the cursor position.
            // 
            // カーソルの初期位置を設定します。
            cursorStyle = CursorStyle.Change;
            cursorRotate = CursorRotate.On;

            // Initializes the sphere model.
            //
            // 球体モデルの初期設定をします。
            Data.Spheres[1][0].Position = SpherePositions[0][0];
            Data.Spheres[1][0].Rotate = new Vector3();
            Data.Spheres[1][0].Alpha = 0.0f;
            Data.Spheres[1][0].Scale = Data.CursorSphereSize;

            // Loads and initializes the sequence.
            //
            // シーケンスの読み込みと初期化を行います。
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
        /// Loads and initializes the sequence.
        ///
        /// シーケンスの読み込みと初期化を行います。
        /// </summary>
        private void InitializeSequence()
        {
            string asset;

            // Loads the style animation.
            // 
            // スタイルアニメーションを読み込みます。
            asset = "Layout/Style/change_rotate_on_Scene";
            sceneStyleChangeRotateOn = Content.Load<SceneData>(asset);
            seqStyleChangeRotateOn = 
                sceneStyleChangeRotateOn.CreatePlaySeqData("Anime");

            asset = "Layout/Style/change_rotate_off_Scene";
            sceneStyleChangeRotateOff = Content.Load<SceneData>(asset);
            seqStyleChangeRotateOff = 
                sceneStyleChangeRotateOff.CreatePlaySeqData("Anime");

            asset = "Layout/Style/revolve_Scene";
            sceneStyleRevolve = Content.Load<SceneData>(asset);
            seqStyleRevolve = 
                sceneStyleRevolve.CreatePlaySeqData("Anime");

            asset = "Layout/Style/slide_Scene";
            sceneStyleSlide = Content.Load<SceneData>(asset);
            seqStyleSlide =
                sceneStyleSlide.CreatePlaySeqData("Anime");


            // Loads the other sequences.
            // 
            // その他のシーケンスを読み込みます。
            seqStart = Data.sceneData.CreatePlaySeqData("StyleStart");
            seqLoop = Data.sceneData.CreatePlaySeqData("StyleLoop");
            seqChange = Data.sceneData.CreatePlaySeqData("StyleChange");
            seqChangeLoop = Data.sceneData.CreatePlaySeqData("StyleChangeLoop");
            seqChangeSelect = Data.sceneData.CreatePlaySeqData("StyleChangeSelect");
            seqRevolve = Data.sceneData.CreatePlaySeqData("StyleRevolve");
            seqRevolveLoop = Data.sceneData.CreatePlaySeqData("StyleRevolveLoop");
            seqRevolveSelect = Data.sceneData.CreatePlaySeqData("StyleRevolveSelect");
            seqSlide = Data.sceneData.CreatePlaySeqData("StyleSlide");
            seqSlideLoop = Data.sceneData.CreatePlaySeqData("StyleSlideLoop");
            seqSlideSelect = Data.sceneData.CreatePlaySeqData("StyleSlideSelect");
            seqSlide = Data.sceneData.CreatePlaySeqData("StyleSlide");
            seqSlideLoop = Data.sceneData.CreatePlaySeqData("StyleSlideLoop");
            seqSlideSelect = Data.sceneData.CreatePlaySeqData("StyleSlideSelect");
            seqRotateStart = Data.sceneData.CreatePlaySeqData("RotateStart");
            seqRotateLoop = Data.sceneData.CreatePlaySeqData("RotateLoop");
            seqRotateOn = Data.sceneData.CreatePlaySeqData("RotateOn");
            seqRotateOnLoop = Data.sceneData.CreatePlaySeqData("RotateOnLoop");
            seqRotateOnSelect = Data.sceneData.CreatePlaySeqData("RotateOnSelect");
            seqRotateOff = Data.sceneData.CreatePlaySeqData("RotateOff");
            seqRotateOffLoop = Data.sceneData.CreatePlaySeqData("RotateOffLoop");
            seqRotateOffSelect = Data.sceneData.CreatePlaySeqData("RotateOffSelect");

            // Replays the first sequence to be displayed.
            // 
            // 最初に表示されるシーケンスをリプレイします。
            seqStart.Replay();
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
            // Processing is not performed if initialization is not complete.
            // 
            // 初期化が終了していなければ処理を行いません。
            if (!Initialized)
                return null;

            // Updates the sequence.
            // 
            // シーケンスの更新処理を行います。
            UpdateSequence(gameTime);

            // Updates the model.
            //
            // モデルの更新処理を行います。
            UpdateModels();

            // Sets the style animation sequence.
            // 
            // スタイルアニメーションのシーケンスを設定します。
            SetStyleAnimation();


            if (phase == Phase.Start)
            {
                // Sets to selection after the style start animation finishes.
                // 
                // スタイル開始アニメーションが終了したら、選択処理に設定します。
                if (!seqStart.IsPlay)
                {
                    phase = Phase.StyleSelect;
                }
            }
            else if (phase == Phase.StyleSelect)
            {
                // Performs update when style is selected.
                //
                // スタイル選択時の更新処理を行います。
                return UpdateStyleSelect();
            }
            else if (phase == Phase.StyleSelected)
            {
                // Performs update after style selection.
                //
                // スタイル選択後の更新処理を行います。
                return UpdateStyleSelected();
            }
            else if (phase == Phase.RotateStart)
            {
                // Sets to selection after the rotation select start animation finishes.
                //
                // 回転選択の開始アニメーションが終了したら選択処理に設定します。
                if (!seqRotateStart.SequenceData.IsPlay)
                {
                    phase = Phase.RotateSelect;
                }
            }
            else if (phase == Phase.RotateSelect)
            {
                // Performs update when rotation is selected.
                //
                // 回転選択時の更新処理を行います。
                return UpdateRotateSelect();
            }
            else if (phase == Phase.RotateSelected)
            {
                // After rotate selection
                //
                // 回転選択後
                if (!seqRotateOnSelect.IsPlay && !seqRotateOffSelect.IsPlay)
                {
                    return CreateMenu(Game, MenuType.SelectMovie, Data);
                }

            }

            return null;
        }


        /// <summary>
        /// Performs update when style is selected.
        /// 
        /// スタイル選択時の更新処理を行います。
        /// </summary>
        private MenuBase UpdateStyleSelect()
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;
            VirtualPadDPad dPad = virtualPad.DPad;
            VirtualPadDPad leftStick = virtualPad.ThumbSticks.Left;

            if (InputState.IsPush(dPad.Up, leftStick.Up))
            {
                // Performs Up key processing when style is selected.
                // 
                // スタイル選択時、上キーが押された時の処理を行います。
                InputStyleUpKey();
            }
            else if (InputState.IsPush(dPad.Down, leftStick.Down))
            {
                // Performs Down key processing when style is selected.
                //
                // スタイル選択時、下キーが押された時の処理を行います。
                InputStyleDownKey();
            }
            else if (buttons.A[VirtualKeyState.Push])
            {
                // Performs Enter key processing when style is selected.
                //
                // スタイル選択時、決定キーが押された時の処理を行います。
                InputStyleSelectKey();
            }
            else if (buttons.B[VirtualKeyState.Push])
            {
                return CreateMenu(Game, MenuType.SelectMode, Data);
            }

            return null;
        }


        /// <summary>
        /// Performs update after style selection.
        /// 
        /// スタイル選択後の更新処理を行います。
        /// </summary>
        private MenuBase UpdateStyleSelected()
        {
            // Processing is not performed while playing the animation.
            // 
            // アニメーション再生中は処理を行いません。
            if (seqChangeSelect.IsPlay ||
                seqRevolveSelect.IsPlay ||
                seqSlideSelect.IsPlay)
            {
                return null;
            }

            if (cursorStyle == CursorStyle.Change)
            {
                // If the change mode is selected by the cursor,
                // set to rotation selection.
                //
                // カーソルがチェンジモードを選択していた場合は
                // 回転の選択処理に設定します。
                phase = Phase.RotateStart;

                // Cursor model settings
                // 
                // カーソルモデルの設定
                Data.Spheres[1][0].Position = SpherePositions[1][0];
                Data.Spheres[1][0].Rotate = new Vector3();
                Data.Spheres[1][0].Alpha = 0.0f;
                Data.Spheres[1][0].Scale = Data.CursorSphereMiniSize;
            }
            else
            {
                // Switches straight to the select movie menu 
                // if the revolve mode and slide mode are selected.
                // 
                // リボルヴモードと、スライドモードを選択していた場合は
                // そのままムービー選択のメニューに遷移します。
                return CreateMenu(Game, MenuType.SelectMovie, Data);
            }

            return null;
        }


        /// <summary>
        /// Performs Up key processing when style is selected.
        /// 
        /// スタイル選択時、上キーが押された時の処理を行います。
        /// </summary>
        private void InputStyleUpKey()
        {
            // Moves the cursor.
            // 
            // カーソルを移動します。
            cursorStyle = CursorStyleUp();

            // Replays the sequence.
            // 
            // シーケンスをリプレイします。
            ReplayStyleAnimation();
            ReplayStyleSequence(cursorStyle);

            // Plays the cursor movement SoundEffect.
            // 
            // カーソル移動のSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);
        }


        /// <summary>
        /// Performs Down key processing when style is selected.
        /// 
        /// スタイル選択時、下キーが押された時の処理を行います。
        /// </summary>
        private void InputStyleDownKey()
        {
            // Moves the cursor.
            //
            // カーソルを移動します。
            cursorStyle = CursorStyleDown();

            // Replays the sequence.
            // 
            // シーケンスをリプレイします。
            ReplayStyleAnimation();
            ReplayStyleSequence(cursorStyle);

            // Plays the cursor movement SoundEffect.
            // 
            // カーソル移動のSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);
        }


        /// <summary>
        /// Performs Enter key processing when style is selected.
        /// 
        /// スタイル選択時、決定キーが押された時の処理を行います。
        /// </summary>
        private void InputStyleSelectKey()
        {
            // Sets processing status to selection completed.
            // 
            // 処理状態を選択完了に設定します。
            phase = Phase.StyleSelected;

            // Plays the SoundEffect.
            // 
            // 決定のSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);

            if (cursorStyle == CursorStyle.Change)
            {
                // Sets to the change mode.
                // 
                // チェンジモードに設定します。
                Data.StageSetting.Style = StageSetting.StyleList.Change;

                // Replays the sequence.
                //
                // シーケンスをリプレイします。
                seqChangeSelect.Replay();
                seqRotateStart.Replay();
            }
            else if (cursorStyle == CursorStyle.Revolve)
            {
                // Sets to the revolve mode.
                //
                // リボルヴモードに設定します。
                Data.StageSetting.Style = StageSetting.StyleList.Revolve;
                Data.StageSetting.Rotate = StageSetting.RotateMode.Off;

                // Replays the sequence.
                // 
                // シーケンスをリプレイします。
                seqRevolveSelect.Replay();
            }
            else if (cursorStyle == CursorStyle.Slide)
            {
                // Sets to the slide mode.
                // 
                // スライドモードに設定します。
                Data.StageSetting.Style = StageSetting.StyleList.Slide;
                Data.StageSetting.Rotate = StageSetting.RotateMode.Off;

                // Replays the sequence. 
                // 
                // シーケンスをリプレイします。
                seqSlideSelect.Replay();
            }

        }



        /// <summary>
        /// Performs update when rotation is selected.
        /// 
        /// 回転選択時の更新処理を行います。
        /// </summary>
        private MenuBase UpdateRotateSelect()
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;
            VirtualPadDPad dPad = virtualPad.DPad;
            VirtualPadDPad leftStick = virtualPad.ThumbSticks.Left;

            if (InputState.IsPush(dPad.Up, leftStick.Up, dPad.Down, leftStick.Down))
            {
                InputRotateMoveKey();
            }
            else if (buttons.A[VirtualKeyState.Push])
            {
                InputRotateSelectKey();
            }
            else if (buttons.B[VirtualKeyState.Push])
            {
                return CreateMenu(Game, MenuType.SelectStyle, Data);
            }

            return null;
        }


        /// <summary>
        /// Performs cursor movement key processing when rotation is selected.
        /// Use common processing, since it has only two items (on and off).
        /// 
        /// 回転選択時、カーソルの移動キーが押されたときの処理を行います。
        /// 項目はOnとOffの2つしかないため、共通の処理を使用します。
        /// </summary>
        private void InputRotateMoveKey()
        {
            // Moves the cursor.
            // 
            // カーソルを移動します。
            cursorRotate = CursorRotateMove();

            // Replays the sequence.
            // 
            // シーケンスをリプレイします。
            ReplayStyleAnimation();
            ReplayRotateSequence(cursorRotate);

            // Plays cursor movement SoundEffect.
            // 
            // カーソル移動のSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);
        }


        /// <summary>
        /// Performs Enter key processing when rotation is selected.
        /// 
        /// 回転選択時、決定キーが押されたときの処理を行います。
        /// </summary>
        private void InputRotateSelectKey()
        {
            // Sets processing status to selection completed.
            // 
            // 処理状態を選択完了に設定します。
            phase = Phase.RotateSelected;

            // Plays the SoundEffect.
            //
            // 決定のSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);

            if (cursorRotate == CursorRotate.On)
            {
                // Enables the rotation settings.
                // 
                // 回転の設定を有効にします。
                Data.StageSetting.Rotate = StageSetting.RotateMode.On;

                // Replays the sequence.
                //
                // シーケンスをリプレイします。
                seqRotateOnSelect.Replay();
            }
            else if (cursorRotate == CursorRotate.Off)
            {
                // Disables the rotation settings.
                //
                // 回転の設定を無効にします。
                Data.StageSetting.Rotate = StageSetting.RotateMode.Off;

                // Replays the sequence.
                // 
                // シーケンスをリプレイします。
                seqRotateOffSelect.Replay();
            }
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
            seqLoop.Update(elapsedGameTime);
            seqChange.Update(elapsedGameTime);
            seqChangeLoop.Update(elapsedGameTime);
            seqChangeSelect.Update(elapsedGameTime);
            seqRevolve.Update(elapsedGameTime);
            seqRevolveLoop.Update(elapsedGameTime);
            seqRevolveSelect.Update(elapsedGameTime);
            seqSlide.Update(elapsedGameTime);
            seqSlideLoop.Update(elapsedGameTime);
            seqSlideSelect.Update(elapsedGameTime);
            seqSlide.Update(elapsedGameTime);
            seqSlideLoop.Update(elapsedGameTime);
            seqSlideSelect.Update(elapsedGameTime);
            seqRotateStart.Update(elapsedGameTime);
            seqRotateLoop.Update(elapsedGameTime);
            seqRotateOn.Update(elapsedGameTime);
            seqRotateOnLoop.Update(elapsedGameTime);
            seqRotateOnSelect.Update(elapsedGameTime);
            seqRotateOff.Update(elapsedGameTime);
            seqRotateOffLoop.Update(elapsedGameTime);
            seqRotateOffSelect.Update(elapsedGameTime);

            seqStyleChangeRotateOn.Update(elapsedGameTime);
            seqStyleChangeRotateOff.Update(elapsedGameTime);
            seqStyleRevolve.Update(elapsedGameTime);
            seqStyleSlide.Update(elapsedGameTime);
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
            if (phase == Phase.StyleSelect)
            {
                if (cursorStyle == CursorStyle.Change)
                {
                    position += (SpherePositions[0][0] - position) * 0.2f;
                }
                else if (cursorStyle == CursorStyle.Revolve)
                {
                    position += (SpherePositions[0][1] - position) * 0.2f;
                }
                else if (cursorStyle == CursorStyle.Slide)
                {
                    position += (SpherePositions[0][2] - position) * 0.2f;
                }
            }
            else if (phase == Phase.RotateSelect)
            {
                if (cursorRotate == CursorRotate.On)
                {
                    position += (SpherePositions[1][0] - position) * 0.2f;
                }
                else if (cursorRotate == CursorRotate.Off)
                {
                    position += (SpherePositions[1][1] - position) * 0.2f;
                }
            }
            Data.Spheres[1][0].Position = position;

            // Specifies the transparency and scale settings.
            // 
            // 透明度とスケールの設定をします。
            float alpha = Data.Spheres[1][0].Alpha;
            float fadeSpeed = Data.CursorSphereFadeSpeed;
            if (phase != Phase.StyleSelected && phase != Phase.RotateSelected)
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
            // Draws the cursor sphere.
            // 
            // カーソルの球体を描画します。
            DrawSpheres(batch);

            // Switches the draw method depending on the process.
            // 
            // 処理内容によって描画メソッドを切り替えます。
            batch.Begin();
            if (phase == Phase.Start)
            {
                DrawStyleStart(batch);
            }
            else if (phase == Phase.StyleSelect)
            {
                DrawStyleSelect(gameTime, batch);
            }
            else if (phase == Phase.StyleSelected)
            {
                DrawStyleSelected(batch);
            }
            else if (phase == Phase.RotateStart)
            {
                DrawRotateStart(batch);
            }
            else if (phase == Phase.RotateSelect)
            {
                DrawRotateSelect(gameTime, batch);
            }
            else if (phase == Phase.RotateSelected)
            {
                DrawRotateSelected(batch);
            }
            batch.End();

            // Draws the style animation.
            // 
            // スタイルアニメーションを描画します。
            DrawStyleAnimation(batch);
        }


        /// <summary>
        /// Performs render processing when the style animation starts.
        /// 
        /// スタイルアニメーション開始時の描画処理を行います。
        /// </summary>
        private void DrawStyleStart(SpriteBatch batch)
        {
            seqStart.Draw(batch, null);
        }


        /// <summary>
        /// Performs render processing when style is selected.
        ///
        /// スタイル選択中の描画処理を行います。
        /// </summary>
        private void DrawStyleSelect(GameTime gameTime, SpriteBatch batch)
        {
            seqLoop.Draw(batch, null);

            if (cursorStyle == CursorStyle.Change)
            {
                seqChangeLoop.Draw(batch, null);
            }
            else if (cursorStyle == CursorStyle.Revolve)
            {
                seqRevolveLoop.Draw(batch, null);
            }
            else if (cursorStyle == CursorStyle.Slide)
            {
                seqSlideLoop.Draw(batch, null);
            }

            seqChange.Draw(batch, null);
            seqRevolve.Draw(batch, null);
            seqSlide.Draw(batch, null);

            // Draws the navigate button.
            // 
            // ナビゲートボタンの描画をします。
            DrawNavigate(gameTime, batch, false);
        }


        /// <summary>
        /// Performs render processing after the style is selected.
        /// 
        /// スタイル選択後の描画処理を行います。
        /// </summary>
        private void DrawStyleSelected(SpriteBatch batch)
        {
            seqLoop.Draw(batch, null);
            if (cursorStyle == CursorStyle.Change)
            {
                seqChangeSelect.Draw(batch, null);
                seqRotateStart.Draw(batch, null);
            }
            else if (cursorStyle == CursorStyle.Revolve)
            {
                seqRevolveSelect.Draw(batch, null);
            }
            else if (cursorStyle == CursorStyle.Slide)
            {
                seqSlideSelect.Draw(batch, null);
            }
        }


        /// <summary>
        /// Performs render processing when the rotation selected animation starts.
        /// 
        /// 回転選択アニメーション開始時の描画処理を行います。
        /// </summary>
        private void DrawRotateStart(SpriteBatch batch)
        {
            seqLoop.Draw(batch, null);
            seqRotateStart.Draw(batch, null);
        }

        /// <summary>
        /// Performs render processing when rotation is selected.
        ///
        /// 回転選択時の描画処理を行います。
        /// </summary>
        private void DrawRotateSelect(GameTime gameTime, SpriteBatch batch)
        {
            seqLoop.Draw(batch, null);
            seqRotateLoop.Draw(batch, null);
            if (cursorRotate == CursorRotate.On)
            {
                seqRotateOnLoop.Draw(batch, null);
            }
            else if (cursorRotate == CursorRotate.Off)
            {
                seqRotateOffLoop.Draw(batch, null);
            }

            seqRotateOn.Draw(batch, null);
            seqRotateOff.Draw(batch, null);

            // Draws the navigate button.
            // 
            // ナビゲートボタンの描画をします。
            DrawNavigate(gameTime, batch, false);
        }


        /// <summary>
        /// Performs render processing after rotation is selected.
        /// 
        /// 回転選択後の描画処理を行います。
        /// </summary>
        private void DrawRotateSelected(SpriteBatch batch)
        {
            seqLoop.Draw(batch, null);
            seqRotateLoop.Draw(batch, null);
            if (cursorRotate == CursorRotate.On)
            {
                seqRotateOnSelect.Draw(batch, null);
            }
            else if (cursorRotate == CursorRotate.Off)
            {
                seqRotateOffSelect.Draw(batch, null);
            }
        }


        /// <summary>
        /// Draws the cursor sphere.
        /// 
        /// カーソルの球体を描画します。
        /// </summary>
        private void DrawSpheres(SpriteBatch batch)
        {
            GraphicsDevice graphicsDevice = batch.GraphicsDevice;
            Matrix view;
            view = Matrix.CreateLookAt(Data.CameraPosition, Vector3.Zero, Vector3.Up);
            Data.Spheres[1][0].SetRenderState(graphicsDevice, SpriteBlendMode.Additive);
            Data.Spheres[1][0].Draw(view, GameData.Projection);
        }


        /// <summary>
        /// Draws the style animation.
        ///
        /// スタイルアニメーションを描画します。
        /// </summary>
        private void DrawStyleAnimation(SpriteBatch batch)
        {
            Texture2D texture = Data.StyleAnimationTexture;

            // The following process is not performed when there
            // is no style animation texture.
            // 
            // スタイルアニメーションのテクスチャが無い場合は
            // 以下の処理を行いません。
            if (texture == null)
                return;

            // Draws style animation texture via addition. 
            // 
            // スタイルアニメーションのテクスチャを加算で描画します。
            batch.Begin(SpriteBlendMode.Additive);
            batch.Draw(texture, PositionStyleAnimation, Color.White);
            batch.End();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Replays the sequence.
        /// 
        /// シーケンスをリプレイします。
        /// </summary>
        private void ReplayStyleAnimation()
        {
            seqStyleChangeRotateOn.Replay();
            seqStyleChangeRotateOff.Replay();
            seqStyleRevolve.Replay();
            seqStyleSlide.Replay();
        }


        /// <summary>
        /// Sets the current style animation.
        ///
        /// 現在のスタイルアニメーションを設定します。
        /// </summary>
        private void SetStyleAnimation()
        {
            if (cursorStyle == CursorStyle.Change)
            {
                if (cursorRotate == CursorRotate.On)
                {
                    // Change : Rotate On
                    Data.SeqStyleAnimation = seqStyleChangeRotateOn;
                }
                else
                {
                    // Change : Rotate Off
                    Data.SeqStyleAnimation = seqStyleChangeRotateOff;
                }
            }
            else if (cursorStyle == CursorStyle.Revolve)
            {
                // Revolve
                Data.SeqStyleAnimation = seqStyleRevolve;
            }
            else if (cursorStyle == CursorStyle.Slide)
            {
                // Slide
                Data.SeqStyleAnimation = seqStyleSlide;
            }
        }


        /// <summary>
        /// Moves the style selection cursor up.
        /// 
        /// スタイル選択のカーソルを上に移動します。
        /// </summary>
        private CursorStyle CursorStyleUp()
        {
            int count = (int)CursorStyle.Count;
            return (CursorStyle)(((int)cursorStyle + (count - 1)) % count);
        }


        /// <summary>
        /// Moves the style selection cursor down.
        /// 
        /// スタイル選択のカーソルを下に移動します。
        /// </summary>
        private CursorStyle CursorStyleDown()
        {
            int count = (int)CursorStyle.Count;
            return (CursorStyle)(((int)cursorStyle + 1) % count);
        }


        /// <summary>
        /// Replays the sequence for the style selection cursor.
        /// 
        /// スタイルを選択しているカーソルのシーケンスをリプレイします。
        /// </summary>
        private void ReplayStyleSequence(CursorStyle cursorStyle)
        {
            ReplayStyleSequence((int)cursorStyle);
        }


        /// <summary>
        /// Replays the sequence for the style selection cursor.
        /// 
        /// スタイルを選択しているカーソルのシーケンスをリプレイします。
        /// </summary>
        private void ReplayStyleSequence(int id)
        {
            SequencePlayData[] seqList = { seqChange, seqRevolve, seqSlide };
            seqList[id].Replay();
        }


        /// <summary>
        /// Moves the rotation selection cursor.
        /// 
        /// 回転選択のカーソルを移動します。
        /// </summary>
        private CursorRotate CursorRotateMove()
        {
            int count = (int)CursorRotate.Count;
            return (CursorRotate)(((int)cursorRotate + 1) % count);
        }
        

        /// <summary>
        /// Replays the sequence for the rotation selection cursor.
        /// 
        /// 回転を選択しているカーソルのシーケンスをリプレイします。
        /// </summary>
        private void ReplayRotateSequence(CursorRotate cursorRotate)
        {
            ReplayRotateSequence((int)cursorRotate);
        }


        /// <summary>
        /// Replays the sequence for the rotation selection cursor.
        /// 
        /// 回転を選択しているカーソルのシーケンスをリプレイします。
        /// </summary>
        private void ReplayRotateSequence(int id)
        {
            SequencePlayData[] seqList = { seqRotateOn, seqRotateOff };
            seqList[id].Replay();
        }
        #endregion
    }
}
