#region File Description
//-----------------------------------------------------------------------------
// SelectFile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SceneDataLibrary;
using Movipa.Components.Input;
using Movipa.Util;
using MovipaLibrary;
#endregion

namespace Movipa.Components.Scene.Menu
{
    /// <summary>
    /// Menu item used for save file selection process. It inherits 
    /// MenuBase and expands menu compilation processing. 
    /// This class implements menu mode selection, and includes 
    /// asynchronous file searching, save file load, and 
    /// delete functions. 
    /// 
    /// セーブファイルの選択を処理するメニュー項目です。
    /// MenuBaseを継承し、メニューを構成する処理を拡張しています。
    /// このクラスはメニューのモード選択を実装し、非同期のファイル検索と
    /// セーブファイルの読み込み、削除の機能があります。
    /// </summary>
    public class SelectFile : MenuBase
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
            /// Storage selection
            /// 
            /// ストレージ選択
            /// </summary>
            StorageSelect,

            /// <summary>
            /// File search
            ///
            /// ファイル検索
            /// </summary>
            FileSearch,

            /// <summary>
            /// Start
            ///
            /// 開始演出
            /// </summary>
            Start,

            /// <summary>
            /// File selection
            /// 
            /// ファイル選択
            /// </summary>
            Select,

            /// <summary>
            /// File selected
            /// 
            /// ファイル決定演出
            /// </summary>
            Selected,

            /// <summary>
            /// Delete start
            /// 
            /// 削除開始演出
            /// </summary>
            DeleteStart,

            /// <summary>
            /// Delete selection
            /// 
            /// 削除選択
            /// </summary>
            DeleteSelect,
        }

        /// <summary>
        /// Cursor position
        /// 
        /// カーソル位置。
        /// </summary>
        private enum CursorPosition
        {
            /// <summary>
            /// File 1
            /// 
            /// ファイル1。
            /// </summary>
            File1,

            /// <summary>
            /// File 2
            /// 
            /// ファイル2。
            /// </summary>
            File2,

            /// <summary>
            /// File 3
            /// 
            /// ファイル3。
            /// </summary>
            File3,

            /// <summary>
            /// Count
            /// 
            /// カウント用。
            /// </summary>
            Count
        }
        #endregion

        #region Fields
        /// <summary>
        /// Name of storage container 
        /// 
        /// ストレージで使用するコンテナ名
        /// </summary>
        private const string ContainerName = "Movipa";

        // Cursor position
        // 
        // カーソル位置
        private CursorPosition cursor;

        // Processing status
        // 
        // 処理状態
        private Phase phase;

        // Game settings information 
        //
        // ゲームの設定情報
        private SaveFileLoader saveFileLoader;
        private SaveData[] saveData;

        // Delete window visibility
        // 
        // 削除ウィンドウの可視状態
        private bool visibleDeleteWindow;

        // Sequence
        //
        // シーケンス
        private SequencePlayData seqStart;
        private SequencePlayData seqPosStart;
        private SequencePlayData seqLoop;
        private SequencePlayData seqFile1;
        private SequencePlayData seqFile1Loop;
        private SequencePlayData seqFile1LoopOff;
        private SequencePlayData seqFile1FadeOut;
        private SequencePlayData seqPosFile1FadeOut;
        private SequencePlayData seqFile2;
        private SequencePlayData seqFile2Loop;
        private SequencePlayData seqFile2LoopOff;
        private SequencePlayData seqFile2FadeOut;
        private SequencePlayData seqPosFile2FadeOut;
        private SequencePlayData seqFile3;
        private SequencePlayData seqFile3Loop;
        private SequencePlayData seqFile3LoopOff;
        private SequencePlayData seqFile3FadeOut;
        private SequencePlayData seqPosFile3FadeOut;
        private SequencePlayData seqDeleteStart;
        private SequencePlayData seqDeleteLoop;
        private SequencePlayData seqPosDelete;

        // Position
        //
        // ポジション
        private SequenceGroupData[] seqPosFile1;
        private SequenceGroupData[] seqPosFile2;
        private SequenceGroupData[] seqPosFile3;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public SelectFile(Game game, MenuData data)
            : base(game, data)
        {
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
            phase = Phase.StorageSelect;

            // Sets the initial cursor position.
            //
            // カーソルの初期位置を設定します。
            cursor = CursorPosition.File1;

            // Sets the delete window visibility to invisible. 
            // 
            // 削除ウィンドウの可視状態を不可視に設定します。
            visibleDeleteWindow = false;

            // Initializes the sequence.
            // 
            // シーケンスの初期化を行います。
            InitializeSequence();

            // Displays the storage selector.
            //
            // ストレージセレクタを表示します。
            InitializeStorageSelect();

            base.Initialize();
        }


        /// <summary>
        /// Initializes storage selection.
        /// 
        /// ストレージ選択の初期化を行います。
        /// </summary>
        private static void InitializeStorageSelect()
        {
            // Displays the storage selection screen.
            // 
            // ストレージ選択の画面を表示します。
            GameData.Storage.ShowStorageDeviceSelector(ContainerName, PlayerIndex.One);
        }


        /// <summary>
        /// Initializes file searching.
        /// 
        /// ファイル検索の初期化を行います。
        /// </summary>
        private void InitializeFileSearch()
        {
            if (saveFileLoader != null)
                return;

            // Creates the file loader.
            // 
            // ファイルローダーを作成します。
            saveFileLoader = new SaveFileLoader(Game, 3);

            // Starts the file search.
            //
            // ファイル検索を開始します。
            saveFileLoader.Run();
        }


        /// <summary>
        /// Initializes the navigate.
        /// 
        /// ナビゲートの初期化をします。
        /// </summary>
        protected override void InitializeNavigate()
        {
            InitializeNavigate_Select();
        }


        /// <summary>
        /// Initializes the sequence.
        /// 
        /// シーケンスの初期化を行います。
        /// </summary>
        private void InitializeSequence()
        {
            // Loads the sequence.
            // 
            // シーケンスの読み込みを行います。
            seqStart = Data.sceneData.CreatePlaySeqData("FileStart");
            seqPosStart = Data.sceneData.CreatePlaySeqData("PosFileStart");
            seqLoop = Data.sceneData.CreatePlaySeqData("FileLoop");
            seqFile1 = Data.sceneData.CreatePlaySeqData("File1");
            seqFile1Loop = Data.sceneData.CreatePlaySeqData("File1Loop");
            seqFile1LoopOff = Data.sceneData.CreatePlaySeqData("File1LoopOff");
            seqFile1FadeOut = Data.sceneData.CreatePlaySeqData("File1FadeOut");
            seqPosFile1FadeOut = Data.sceneData.CreatePlaySeqData("PosFile1FadeOut");
            seqFile2 = Data.sceneData.CreatePlaySeqData("File2");
            seqFile2Loop = Data.sceneData.CreatePlaySeqData("File2Loop");
            seqFile2LoopOff = Data.sceneData.CreatePlaySeqData("File2LoopOff");
            seqFile2FadeOut = Data.sceneData.CreatePlaySeqData("File2FadeOut");
            seqPosFile2FadeOut = Data.sceneData.CreatePlaySeqData("PosFile2FadeOut");
            seqFile3 = Data.sceneData.CreatePlaySeqData("File3");
            seqFile3Loop = Data.sceneData.CreatePlaySeqData("File3Loop");
            seqFile3LoopOff = Data.sceneData.CreatePlaySeqData("File3LoopOff");
            seqFile3FadeOut = Data.sceneData.CreatePlaySeqData("File3FadeOut");
            seqPosFile3FadeOut = Data.sceneData.CreatePlaySeqData("PosFile3FadeOut");

            seqDeleteStart = Data.sceneData.CreatePlaySeqData("DelStart");
            seqDeleteLoop = Data.sceneData.CreatePlaySeqData("DelLoop");
            seqPosDelete = Data.sceneData.CreatePlaySeqData("PosDelStart");


            // Obtains the position from the sequence.
            //
            // シーケンスからポジションを取得します。
            seqPosFile1 = new SequenceGroupData[] {
                seqPosStart.SequenceData.SequenceGroupList[0],
                seqPosStart.SequenceData.SequenceGroupList[1],
                seqPosStart.SequenceData.SequenceGroupList[2],
                seqPosStart.SequenceData.SequenceGroupList[3],
                seqPosStart.SequenceData.SequenceGroupList[4],
            };
            seqPosFile2 = new SequenceGroupData[] {
                seqPosStart.SequenceData.SequenceGroupList[5],
                seqPosStart.SequenceData.SequenceGroupList[6],
                seqPosStart.SequenceData.SequenceGroupList[7],
                seqPosStart.SequenceData.SequenceGroupList[8],
                seqPosStart.SequenceData.SequenceGroupList[9],
            };
            seqPosFile3 = new SequenceGroupData[] {
                seqPosStart.SequenceData.SequenceGroupList[10],
                seqPosStart.SequenceData.SequenceGroupList[11],
                seqPosStart.SequenceData.SequenceGroupList[12],
                seqPosStart.SequenceData.SequenceGroupList[13],
                seqPosStart.SequenceData.SequenceGroupList[14],
            };
        }


        /// <summary>
        /// Sets the navigate for file selection.
        /// 
        /// ファイル選択時のナビゲートを設定します。
        /// </summary>
        private void InitializeNavigate_Select()
        {
            Navigate.Clear();
            Navigate.Add(new NavigateData(AppSettings("X_Delete")));
            Navigate.Add(new NavigateData(AppSettings("B_Cancel")));
            Navigate.Add(new NavigateData(AppSettings("A_Ok"), true));
        }


        /// <summary>
        /// Sets the navigate for file deletion.
        ///
        /// ファイル削除時のナビゲートを設定します。
        /// </summary>
        private void InitializeNavigate_Delete()
        {
            Navigate.Clear();
            Navigate.Add(new NavigateData(AppSettings("X_Delete")));
            Navigate.Add(new NavigateData(AppSettings("B_Cancel")));
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Performs update processing.
        ///
        /// 更新処理を行います。
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns>
        /// Next menu
        /// 
        /// 次のメニュー
        /// </returns>
        public override MenuBase UpdateMain(GameTime gameTime)
        {
            // Updates the sequence
            // 
            // シーケンスの更新処理
            UpdateSequence(gameTime);

            if (phase == Phase.StorageSelect)
            {
                // Performs update at storage selection.
                // 
                // ストレージ選択時の更新処理を行います。
                return UpdateStorageSelect();
            }
            else if (phase == Phase.FileSearch)
            {
                // Performs update at file search.
                //
                // ファイル検索時の更新処理を行います。
                return UpdateFileSearch();
            }
            else if (phase == Phase.Start)
            {
                // Sets to selection after start animation finishes.
                //
                // 開始アニメーションが終了したら選択処理へ設定します。
                if (!seqStart.SequenceData.IsPlay)
                {
                    phase = Phase.Select;
                }
            }
            else if (phase == Phase.Select)
            {
                // Updates at file selection.
                //
                // ファイル選択時の更新処理を行います。
                return UpdateSelect();
            }
            else if (phase == Phase.Selected)
            {
                // Performs update when file is selected.
                //
                // ファイルが選択された時の更新処理を行います。
                return UpdateSelected();
            }
            else if (phase == Phase.DeleteStart)
            {
                // Sets to delete selection when delete
                // window start animation finishes.
                //
                // 削除ウィンドウの開始アニメーションが終了したら
                // 削除選択の処理へ設定します。
                if (!seqDeleteStart.IsPlay && !seqPosDelete.IsPlay)
                {
                    phase = Phase.DeleteSelect;
                }
            }
            else if (phase == Phase.DeleteSelect)
            {
                // Performs update at file deletion. 
                //
                // ファイル削除時の更新処理を行います。
                UpdateDeleteSelect();
            }

            return null;
        }


        /// <summary>
        /// Performs update at storage selection.
        /// 
        /// ストレージ選択時の更新処理を行います。
        /// </summary>
        private MenuBase UpdateStorageSelect()
        {
            // Begins file search after storage has been selected.
            // 
            // ストレージ選択が完了したらファイル検索を開始します。
            if (!GameData.Storage.IsVisible && GameData.Storage.IsConnected)
            {
                phase = Phase.FileSearch;
            }

            return null;
        }


        /// <summary>
        /// Performs update at file search.
        /// 
        /// ファイル検索時の更新処理を行います。
        /// </summary>
        private MenuBase UpdateFileSearch()
        {
            // Starts file search thread.
            // 
            // ファイル検索のスレッドを開始します。
            if (saveFileLoader == null)
            {
                InitializeFileSearch();
            }

            // Starts file selection process after file search has finished.
            // 
            // ファイル検索が終了したら、ファイル選択処理を開始します。
            if (saveFileLoader.GetGameSettings() != null)
            {
                saveData = saveFileLoader.GetGameSettings();
                saveFileLoader = null;

                seqStart.Replay();
                seqPosStart.Replay();

                phase = Phase.Start;
            }

            return null;
        }

        /// <summary>
        /// Performs update at file selection.
        /// 
        /// ファイル選択時の更新処理を行います。
        /// </summary>
        private MenuBase UpdateSelect()
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;
            VirtualPadDPad dPad = virtualPad.DPad;
            VirtualPadDPad leftStick = virtualPad.ThumbSticks.Left;

            if (InputState.IsPush(dPad.Up, leftStick.Up))
            {
                // Performs Up key processing.
                // 
                // 上キーを押した時の処理を行います。
                InputUpKey();
            }
            else if (InputState.IsPush(dPad.Down, leftStick.Down))
            {
                // Performs Down key processing.
                // 
                // 下キーを押した時の処理を行います。
                InputDownKey();
            }
            else if (buttons.A[VirtualKeyState.Push])
            {
                // Performs Enter key processing.
                // 
                // 決定キーを押した時の処理を行います。
                InputSelectKey();
            }
            else if (buttons.B[VirtualKeyState.Push])
            {
                // Switches to mode selection due to cancel.
                //
                // キャンセルされたので、モード選択に遷移します。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCancel);
                return CreateMenu(Game, MenuType.SelectMode, Data);
            }
            else if (buttons.X[VirtualKeyState.Push])
            {
                // Performs Delete button processing.
                // 
                // 削除ボタンを押した時の処理を行います。
                return InputDeleteKey();
            }

            return null;
        }


        /// <summary>
        /// Performs update when file is selected.
        /// 
        /// ファイルが選択された時の更新処理を行います。
        /// </summary>
        private MenuBase UpdateSelected()
        {
            // Processing is not performed until the selected animation has finished.
            // 
            // 選択アニメーションが終了するまで処理を行いません。
            if (!seqFile1FadeOut.IsPlay ||
                !seqFile2FadeOut.IsPlay ||
                !seqFile3FadeOut.IsPlay)
            {
                // Loads stage information based on number of stages recorded in file. 
                // 
                // ファイルに記録されたステージ数を元に、
                // ステージ情報を読み込みます。
                int stage = GameData.SaveData.Stage;
                StageSetting stageSetting = GameData.StageCollection[stage];

                // Registers game window scene.
                // 
                // ゲーム画面のシーンを登録します。
                GameData.SceneQueue.Enqueue(
                    new Puzzle.PuzzleComponent(Game, stageSetting));

                // Specifies fade-out settings.
                //
                // フェードアウトの設定を行います。
                GameData.FadeSeqComponent.Start(FadeType.Normal, FadeMode.FadeOut);
            }
            
            return null;
        }


        /// <summary>
        /// Performs Up key processing.
        /// 
        /// 上キーを押した時の処理を行います。
        /// </summary>
        private void InputUpKey()
        {
            // Moves the cursor position.
            // 
            // カーソルの位置を移動します。
            cursor = CursorPrev();

            // Replays the sequence.
            // 
            // シーケンスをリプレイします。
            ReplaySequences(cursor);

            // Plays SoundEffect.
            // 
            // SoundEffectを再生
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);
        }


        /// <summary>
        /// Performs Down key processing.
        /// 
        /// 下キーを押した時の処理を行います。
        /// </summary>
        private void InputDownKey()
        {
            // Moves the cursor position.
            // 
            // カーソルの位置を移動します。
            cursor = CursorNext();

            // Replays the sequence.
            //
            // シーケンスをリプレイします。
            ReplaySequences(cursor);

            // Plays SoundEffect.
            // 
            // SoundEffectを再生
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);
        }


        /// <summary>
        /// Performs Enter key processing.
        /// 
        /// 決定キーを押した時の処理を行います。
        /// </summary>
        private void InputSelectKey()
        {
            // Obtains settings data.
            // 
            // 設定データを取得します。
            SaveData gameSettings = saveData[(int)cursor];

            // Creates a new file if no data exists.
            // 
            // データが無い場合はファイルを新規作成します。
            if (gameSettings == null)
            {
                // Sets full path filename.
                // 
                // ファイル名をフルパスで設定します。
                string filename = string.Format("SaveData{0}.xml", cursor);
                string filePath = GameData.Storage.GetStoragePath(filename);

                // Creates a new file.
                // 
                // ファイルを新規作成します。
                gameSettings = CreateSaveData(filePath);
            }

            // Calculates the play count. 
            // 
            // プレイカウントを加算します。
            gameSettings.PlayCount++;

            // Sets the save data to be used.
            // 
            // 使用するセーブデータを設定します。
            GameData.SaveData = gameSettings;

            // Replays the sequence.
            //
            // シーケンスをリプレイします。
            seqFile1FadeOut.Replay();
            seqPosFile1FadeOut.Replay();
            seqFile2FadeOut.Replay();
            seqPosFile2FadeOut.Replay();
            seqFile3FadeOut.Replay();
            seqPosFile3FadeOut.Replay();

            // Plays the chosen SoundEffect.
            //
            // 決定のSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);

            // Sets the processing status to Selected.
            //
            // 処理状態を選択済みに設定します。
            phase = Phase.Selected;
        }

        /// <summary>
        /// Performs update processing at file deletion.
        /// 
        /// ファイル削除時の更新処理を行います。
        /// </summary>
        private MenuBase UpdateDeleteSelect()
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            if (buttons.X[VirtualKeyState.Push])
            {
                // Deletes the file when X button is pressed.
                // 
                // Xボタンが押されたら、ファイルの削除処理を実行します。
                SaveData gameSettings = saveData[(int)cursor];

                // Deletes the file and (where necessary) plays the SoundEffect.  
                // 
                // ファイル削除を実行し、結果に応じたSoundEffectを再生します。
                DeleteSaveData(gameSettings.FileName);
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);

                // Sets the processing status to file search.
                // 
                // 処理状態をファイル検索に設定します。
                phase = Phase.FileSearch;
                visibleDeleteWindow = false;
            }
            else if (buttons.B[VirtualKeyState.Push])
            {
                // Deletes cancelled; cancel tone sounds.
                //
                // 削除処理がキャンセルされたので、キャンセル音を再生します。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCancel);

                // Sets processing status to file search.
                // 
                // 処理状態をファイル検索に設定します。
                phase = Phase.FileSearch;
                visibleDeleteWindow = false;
            }

            return null;
        }


        /// <summary>
        /// Performs delete button processing.
        /// 
        /// 削除ボタンを押したときの処理を行います。
        /// </summary>
        private MenuBase InputDeleteKey()
        {
            // Obtains settings data.
            // 
            // 設定データを取得します。
            SaveData gameSettings = saveData[(int)cursor];

            // Processing is not performed if there is no data.
            // 
            // データが無ければ処理を行いません。
            if (gameSettings == null)
            {
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCancel);
                return null;
            }

            // Replays the delete window sequence.
            // 
            // 削除ウィンドウのシーケンスをリプレイします。
            seqDeleteStart.Replay();
            seqPosDelete.Replay();

            // Sets the delete window visibility to visible.
            // 
            // 削除ウィンドウを可視状態に設定します。
            visibleDeleteWindow = true;

            // Sets the navigate button in delete item.
            // 
            // ナビゲートボタンを削除の項目に設定します。
            InitializeNavigate_Delete();

            // Sets processing status to delete selection.
            //
            // 処理状態を削除選択に設定します。
            phase = Phase.DeleteStart;

            // Plays the chosen SoundEffect.
            // 
            // 決定のSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);

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
            seqPosStart.Update(elapsedGameTime);
            seqLoop.Update(elapsedGameTime);
            seqFile1.Update(elapsedGameTime);
            seqFile1Loop.Update(elapsedGameTime);
            seqFile1LoopOff.Update(elapsedGameTime);
            seqFile1FadeOut.Update(elapsedGameTime);
            seqPosFile1FadeOut.Update(elapsedGameTime);
            seqFile2.Update(elapsedGameTime);
            seqFile2Loop.Update(elapsedGameTime);
            seqFile2LoopOff.Update(elapsedGameTime);
            seqFile2FadeOut.Update(elapsedGameTime);
            seqPosFile2FadeOut.Update(elapsedGameTime);
            seqFile3.Update(elapsedGameTime);
            seqFile3Loop.Update(elapsedGameTime);
            seqFile3LoopOff.Update(elapsedGameTime);
            seqFile3FadeOut.Update(elapsedGameTime);
            seqPosFile3FadeOut.Update(elapsedGameTime);
            seqDeleteStart.Update(elapsedGameTime);
            seqDeleteLoop.Update(elapsedGameTime);
            seqPosDelete.Update(elapsedGameTime);
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
            batch.Begin();

            if (phase == Phase.Start)
            {
                // Draws the start animation.
                // 
                // 開始アニメーションの描画を行います。
                DrawPhaseStart(batch);
            }
            else if (phase == Phase.Selected)
            {
                // Draws the selected animation.
                // 
                // 選択後のアニメーションを描画します。
                DrawPhaseSelected(batch);
            }
            else if (phase > Phase.FileSearch)
            {
                // Performs rendering except during file search 
                // and the above process branch.  Also executed 
                // during file deletion.
                //
                // ファイル検索中と、上記の処理分岐以外での描画を行います。
                // ファイル削除中もこの処理は実行されます。
                DrawPhaseSelect(gameTime, batch);
            }

            // Draws the delete window.
            // 
            // 削除ウィンドウを描画します。
            DrawDeleteWindow(gameTime, batch);

            batch.End();
        }


        /// <summary>
        /// Draws the start animation.
        /// 
        /// 開始アニメーションの描画を行います。
        /// </summary>
        private void DrawPhaseStart(SpriteBatch batch)
        {
            seqStart.Draw(batch, null);
            //SelectFile_DrawString(gameTime, batch);
            DrawSequenceString(batch, seqPosFile1, 0);
            DrawSequenceString(batch, seqPosFile2, 1);
            DrawSequenceString(batch, seqPosFile3, 2);
        }


        /// <summary>
        /// Performs rendering during file selection.
        /// 
        /// ファイル選択中の描画を行います。
        /// </summary>
        private void DrawPhaseSelect(GameTime gameTime, SpriteBatch batch)
        {
            seqLoop.Draw(batch, null);

            seqFile1.Draw(batch, null);
            seqFile2.Draw(batch, null);
            seqFile3.Draw(batch, null);

            // Determines file selection status and performs rendering accordingly.
            // 
            // ファイル選択中の状態を判別して描画します。
            SequencePlayData seqPlayData;
            CursorPosition file;

            file = CursorPosition.File1;
            seqPlayData = (cursor == file) ? seqFile1Loop : seqFile1LoopOff;
            seqPlayData.Draw(batch, null);

            file = CursorPosition.File2;
            seqPlayData = (cursor == file) ? seqFile2Loop : seqFile2LoopOff;
            seqPlayData.Draw(batch, null);

            file = CursorPosition.File3;
            seqPlayData = (cursor == file) ? seqFile3Loop : seqFile3LoopOff;
            seqPlayData.Draw(batch, null);

            // Draws file text strings.
            //
            // ファイルの内容の文字列を描画します。
            DrawSequenceString(batch, seqPosFile1, 0);
            DrawSequenceString(batch, seqPosFile2, 1);
            DrawSequenceString(batch, seqPosFile3, 2);

            // Draws the navigate button when the delete window is not displayed.
            // 
            // 削除ウィンドウが非表示の場合はナビゲートボタンを描画します。
            if (!visibleDeleteWindow)
            {
                DrawNavigate(gameTime, batch, false);
            }
        }


        /// <summary>
        /// Draws the selected animation.
        /// 
        /// 選択後のアニメーションを描画します。
        /// </summary>
        private void DrawPhaseSelected(SpriteBatch batch)
        {
            seqLoop.Draw(batch, null);

            seqFile1FadeOut.Draw(batch, null);
            seqFile2FadeOut.Draw(batch, null);
            seqFile3FadeOut.Draw(batch, null);

            // Draws the window text string.
            // 
            // ウィンドウの文字列を描画します。
            SequencePlayData[] sequences = new SequencePlayData[] {
                seqPosFile1FadeOut,
                seqPosFile2FadeOut,
                seqPosFile3FadeOut
            };

            for (int i = 0; i < sequences.Length; i++)
            {
                SequencePlayData sequence = sequences[i];
                
                SequenceGroupData[] sequenceGroups;
                sequenceGroups = sequence.SequenceData.SequenceGroupList.ToArray();
                DrawSequenceString(batch, sequenceGroups, i);
            }
        }


        /// <summary>
        /// Draws the delete window.
        /// 
        /// 削除ウィンドウを描画します。
        /// </summary>
        private void DrawDeleteWindow(GameTime gameTime, SpriteBatch batch)
        {
            // Processing is not performed when the delete window is invisible.
            // 
            // 削除ウィンドウの可視状態が不可視の場合は処理をしません。
            if (!visibleDeleteWindow)
            {
                return;
            }

            if (phase == Phase.DeleteStart)
            {
                // Draws the delete window start animation. 
                // 
                // 削除ウィンドウの開始アニメーションを描画します。
                seqDeleteStart.Draw(batch, null);

                SequenceGroupData[] sequenceGroups;
                sequenceGroups = seqPosDelete.SequenceData.SequenceGroupList.ToArray();
                DrawSequenceString(batch, sequenceGroups, (int)cursor);
            }
            else if (phase == Phase.DeleteSelect)
            {
                seqDeleteLoop.Draw(batch, null);

                SequenceGroupData[] sequenceGroups;
                sequenceGroups = seqPosDelete.SequenceData.SequenceGroupList.ToArray();
                DrawSequenceString(batch, sequenceGroups, (int)cursor);

                // Draws the navigate button.
                // 
                // ナビゲートボタンの描画を行います。
                DrawNavigate(gameTime, batch, false);
            }
        }


        /// <summary>
        /// Draws the window text string.
        /// 
        /// ウィンドウの文字列を描画します。
        /// </summary>
        private void DrawSequenceString(SpriteBatch batch, 
            SequenceGroupData[] seqBodyDataList, int fileId)
        {
            // Processing is not performed if there is no settings data.
            // 
            // 設定データが無い場合は処理を行いません。
            if (saveData == null)
            {
                return;
            }

            for (int i = 0; i < seqBodyDataList.Length; i++)
            {
                SaveData gameSettings = saveData[fileId];
                SequenceGroupData seqBodyData = seqBodyDataList[i];
                SequenceObjectData seqPartsData = seqBodyData.CurrentObjectList;
                if (seqPartsData == null)
                {
                    continue;
                }

                List<PatternObjectData> list = seqPartsData.PatternObjectList;
                foreach (PatternObjectData patPartsData in list)
                {
                    DrawData info = patPartsData.InterpolationDrawData;
                    int posType = i % 5;

                    if (posType == 0)
                    {
                        // Draws the file header text string.
                        // 
                        // ファイルヘッダの文字列を描画します。
                        if (!DrawFileHeader(batch, info, gameSettings, fileId))
                        {
                            // Processing omitted for new items with no data.
                            // 
                            // データが無く、新規の項目ならば処理を抜けます。
                            return;
                        }
                    }
                    else if (posType == 1)
                    {
                        // Draws the Best Time header.
                        // 
                        // ベストタイムのヘッダを描画します。
                        DrawBestTimeHeader(batch, info);
                    }
                    else if (posType == 2)
                    {
                        // Draws the Best Score header.
                        //
                        // ベストスコアのヘッダを描画します。
                        DrawBestScoreHeader(batch, info);
                    }
                    else if (posType == 3)
                    {
                        // Draws the Best Time value.
                        //
                        // ベストタイムの値を描画します。
                        DrawBestTimeValue(batch, info, gameSettings);
                    }
                    else if (posType == 4)
                    {
                        // Draws the Best Score value.
                        //
                        // ベストスコアの値を描画します。
                        DrawBestScoreValue(batch, info, gameSettings);
                    }
                }
            }

        }


        /// <summary>
        /// Draws the file header text string.
        /// 
        /// ファイルヘッダの文字列を描画します。
        /// </summary>
        private bool DrawFileHeader(SpriteBatch batch, DrawData info, 
            SaveData gameSettings, int fileId)
        {
            SpriteFont font = LargeFont;
            Vector2 position = new Vector2(info.Position.X, info.Position.Y);
            string text = GetFileHeaderString(gameSettings, fileId);
            position -= font.MeasureString(text) * 0.5f;
            batch.DrawString(font, text, position, info.Color);

            return (gameSettings != null);
        }


        /// <summary>
        /// Draws the Best Time header.
        /// 
        /// ベストタイムのヘッダを描画します。
        /// </summary>
        private void DrawBestTimeHeader(SpriteBatch batch, DrawData info)
        {
            SpriteFont font = MediumFont;
            Vector2 position = new Vector2(info.Position.X, info.Position.Y);
            string text = "Best Time";
            batch.DrawString(font, text, position, info.Color);
        }


        /// <summary>
        /// Draws the Best Time value.
        ///
        /// ベストタイムの値を描画します。
        /// </summary>
        private void DrawBestTimeValue(
            SpriteBatch batch, DrawData info, SaveData gameSettings)
        {
            SpriteFont font = MediumFont;
            Vector2 position = new Vector2(info.Position.X, info.Position.Y);
            string timeText = gameSettings.BestTime == TimeSpan.Zero ? "None" : 
                gameSettings.BestTime.ToString().Substring(0, 8);
            position.X -= font.MeasureString(timeText).X;
            batch.DrawString(font, timeText, position, info.Color);
        }


        /// <summary>
        /// Draws the Best Score header.
        ///
        /// ベストスコアのヘッダを描画します。
        /// </summary>
        private void DrawBestScoreHeader(SpriteBatch batch, DrawData info)
        {
            SpriteFont font = MediumFont;
            Vector2 position = new Vector2(info.Position.X, info.Position.Y);
            string value = "Best Score";
            batch.DrawString(font, value, position, info.Color);
        }


        /// <summary>
        /// Draws the Best Score value.
        ///
        /// ベストスコアの値を描画します。
        /// </summary>
        private void DrawBestScoreValue(SpriteBatch batch, DrawData info, 
            SaveData gameSettings)
        {
            SpriteFont font = MediumFont;
            Vector2 position = new Vector2(info.Position.X, info.Position.Y);
            string value =
                string.Format("{0:000000}", gameSettings.BestScore);
            position.X -= font.MeasureString(value).X;
            batch.DrawString(font, value, position, info.Color);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Creates a new save file and returns the instance.
        /// 
        /// セーブファイルを新規作成し、インスタンスを返します。
        /// </summary>
        private static SaveData CreateSaveData(string filename)
        {
            // Sets the full path for the file.
            // 
            // ファイルのフルパスを設定します。
            string filePath = GameData.Storage.GetStoragePath(filename);

            // Creates the save file instance and set the file name.
            //
            // セーブファイルのインスタンスを作成し、ファイル名を設定します。
            SaveData setting = new SaveData();
            setting.FileName = filename;

            // Serializes and saves the save data.
            // 
            // セーブデータをシリアライズして保存します。
            SettingsSerializer.SaveSaveData(filePath, setting);

            return setting;
        }


        /// <summary>
        /// Deletes the file.
        /// 
        /// ファイルを削除します。
        /// </summary>
        private static void DeleteSaveData(string filename)
        {
            // Checks for the file; if the file is not found, 
            // processing is not performed.
            // 
            // ファイルの有無をチェックし、無ければ処理を行いません。
            if (!File.Exists(filename))
            {
                return;
            }

            // If the file to be deleted can be found, the processing is performed.
            //
            // 削除対象のファイルが見つかれば削除処理を行います。
            File.Delete(filename);
        }


        /// <summary>
        /// Obtains the previous cursor position.
        /// 
        /// カーソルの前の位置を取得します。
        /// </summary>
        private CursorPosition CursorPrev()
        {
            int count = (int)CursorPosition.Count;
            return (CursorPosition)(((int)cursor + (count - 1)) % count);
        }

        /// <summary>
        /// Obtains the next cursor position.
        /// 
        /// カーソルの次の位置を取得します。
        /// </summary>
        private CursorPosition CursorNext()
        {
            int count = (int)CursorPosition.Count;
            return (CursorPosition)(((int)cursor + 1) % count);
        }


        /// <summary>
        /// Replays the file item sequence.
        ///
        /// ファイル項目のシーケンスをリプレイします。
        /// </summary>
        private void ReplaySequences(CursorPosition cursorPosition)
        {
            ReplaySequences((int)cursorPosition);
        }


        /// <summary>
        /// Replays the file item sequence.
        /// 
        /// ファイル項目のシーケンスをリプレイします。
        /// </summary>
        private void ReplaySequences(int id)
        {
            // Sets the sequence array to be replayed.
            // 
            // リプレイするシーケンスの配列をセット
            SequencePlayData[] seqFiles = { seqFile1, seqFile2, seqFile3 };

            // Replays the sequence.
            // 
            // シーケンスをリプレイする
            seqFiles[id].Replay();
        }


        /// <summary>
        /// Obtains the file header text string.
        /// 
        /// ファイルヘッダの文字列を取得します。
        /// </summary>
        private static string GetFileHeaderString(SaveData gameSettings, int fileId)
        {
            string text = String.Empty;
            if (gameSettings == null)
            {
                text = "New";
            }
            else
            {
                string format = "File[{0}] Stage {1}";
                int stage = gameSettings.Stage + 1;
                text = string.Format(format, fileId, stage);
            }

            return text;
        }
        #endregion
    }
}
