#region File Description
//-----------------------------------------------------------------------------
// SelectMovie.cs
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

using Movipa.Components.Input;
using Movipa.Util;
using MovipaLibrary;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene.Menu
{
    /// <summary>
    /// Menu item for processing movie selection.
    /// It inherits MenuBase and expands menu compilation processing.
    /// This class uses threads for asynchronous loading of movies and for 
    /// loading of thumbnail images. A loading status icon is displayed 
    /// during asynchronous movie loading.
    /// 
    /// ムービー選択を処理するメニュー項目です。
    /// MenuBaseを継承し、メニューを構成する処理を拡張しています。
    /// このクラスは、スレッドを使用して非同期でムービーの読み込みと、
    /// サムネイル画像の読み込みを行っています。
    /// 非同期でムービーを読み込んでいる時は、その状態を示すアイコンを
    /// 描画しています。
    /// </summary>
    public class SelectMovie : MenuBase
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
            /// Initialization
            /// 
            /// 初期化
            /// </summary>
            Loading,

            /// <summary>
            /// Start
            ///
            /// 開始演出
            /// </summary>
            Start,

            /// <summary>
            /// Select
            ///
            /// 選択
            /// </summary>
            Select,

            /// <summary>
            /// Selected
            ///
            /// 選択演出
            /// </summary>
            Selected,
        }
        #endregion

        #region Fields
        // Thumbnail size
        // 
        // サムネイルのサイズ
        private readonly Vector2 ThumbnailSize;

        // Number of panels displaying thumbnails
        // 
        // サムネイルを表示するパネル数
        private const int ThumbnailPanel = 7;

        // Movie draw position
        // 
        // ムービーを描画する位置
        private readonly Vector2 PositionThumbnail;
        private readonly Rectangle MoviePreviewRect;

        // Processing details
        //
        // 処理内容
        private Phase phase;

        // Cursor position
        //
        // カーソル位置
        private int cursor;

        // Sequence
        //
        // シーケンス
        private SequencePlayData seqStart;
        private SequencePlayData seqLoop;
        private SequencePlayData seqMovieWindow;
        private SequencePlayData seqMovieSelect;
        private SequencePlayData seqLeft;
        private SequencePlayData seqRight;
        private SequencePlayData seqLoading;
        private SequencePlayData seqPosMovieTitle;
        private SequencePlayData seqPosMovieCount;

        // Thumbnail texture list
        // 
        // サムネイルのテクスチャリスト
        private List<Texture2D> thumbTextures;

        // Thumbnail sprite list 
        //
        // サムネイルのスプライトリスト
        private List<ThumbnailSprite> thumbSprite;

        // CPU core for load processing
        //
        // 読み込み処理をさせるCPUコア
        private int cpuId;

        // Thumbnail loading class
        // 
        // サムネイルを読み込むクラス
        private ThumbnailLoader thumbLoader;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public SelectMovie(Game game, MenuData data)
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

            point = patternGroup.PatternObjectList[1].Position;
            PositionThumbnail = new Vector2(point.X, point.Y);

            // Sets the thumbnail size.
            //
            // サムネイルのサイズを設定します。
            ThumbnailSize = new Vector2(256.0f, 144.0f);
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
            phase = Phase.Loading;

            // Initializes the sequence.
            //
            // シーケンスの初期化を行います。
            InitializeSequence();

            // Creates a thumbnail list for loading.
            // 
            // 読み込むサムネイルのリストを作成します。
            thumbTextures = new List<Texture2D>();
            List<string> thumbAssetList = new List<string>();
            foreach (string infoPath in GameData.MovieList)
            {
                // Obtains the path to Info, then replaces the asset name 
                // with the Thumbnail and adds it to the list.
                //
                // Infoまでのパスを取得し、アセット名をThumbnailに
                // 置き換えてリストに追加します。
                int length = infoPath.LastIndexOf("/");
                string path = infoPath.Substring(0, length);
                string asset = path + "/Thumbnail";
                thumbAssetList.Add(asset);
            }

            // Begins asynchronous thumbnail loading.
            // 
            // サムネイルの非同期読み込みを開始します。
            thumbLoader = new ThumbnailLoader(Game, 3, thumbAssetList.ToArray());
            thumbLoader.Run();

            // Creates thumbnail sprite.
            //
            // サムネイルのスプライトを作成します。
            thumbSprite = new List<ThumbnailSprite>();
            for (int i = 0; i < ThumbnailPanel; i++)
            {
                ThumbnailSprite sprite = new ThumbnailSprite(Game);

                sprite.Id = i;
                sprite.Position = PositionThumbnail;

                Vector2 target = PositionThumbnail;
                target.X += ThumbnailSize.X * (i - (ThumbnailPanel >> 1));
                sprite.TargetPosition = target;

                // Coordinate correction 
                // 
                // 座標補正
                sprite.Position -= ThumbnailSize * 0.5f;
                sprite.TargetPosition -= ThumbnailSize * 0.5f;

                // Sets initial texture.
                // 
                // 初期テクスチャの設定
                int textureId = cursor + i;
                textureId += GameData.MovieList.Count - (ThumbnailPanel >> 1);
                textureId %= GameData.MovieList.Count;
                sprite.TextureId = textureId;

                thumbSprite.Add(sprite);
            }

            // Starts movie loading.
            // 
            // ムービーの読み込み処理を開始します。
            SetMovie(false);

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
            seqStart = Data.sceneData.CreatePlaySeqData("MovieStart");
            seqLoop = Data.sceneData.CreatePlaySeqData("MovieLoop");
            seqMovieWindow = Data.sceneData.CreatePlaySeqData("MovieWindow");
            seqMovieSelect = Data.sceneData.CreatePlaySeqData("MovieSelect");
            seqLeft = Data.sceneData.CreatePlaySeqData("Left");
            seqRight = Data.sceneData.CreatePlaySeqData("Right");
            seqPosMovieTitle = Data.sceneData.CreatePlaySeqData("PosMovieTitle");
            seqPosMovieCount = Data.sceneData.CreatePlaySeqData("PosMovieCount");
            seqLoading = Data.sceneData.CreatePlaySeqData("Loading");

            seqStart.Replay();
            seqPosMovieTitle.Replay();
            seqPosMovieCount.Replay();
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

            if (phase == Phase.Loading)
            {
                // Performs update during loading.
                // 
                // 読み込み中の更新処理を行います。
                return UpdateLoading();
            }
            else if (phase == Phase.Start)
            {
                // Waits until start animation has finished.
                // 
                // 開始アニメーションが終了するまで待機
                if (!seqStart.SequenceData.IsPlay)
                {
                    // To movie selection
                    // 
                    // ムービー選択処理へ
                    phase = Phase.Select;
                }
            }
            else if (phase == Phase.Select)
            {
                // Performs update during selection.
                // 
                // 選択中の更新処理を行います。
                return UpdateSelect();
            }
            else if (phase == Phase.Selected)
            {
                // Waits until selected animation has finished.
                // 
                // 選択アニメーションが終了するまで待機
                if (!seqMovieSelect.SequenceData.IsPlay)
                {
                    // To division setting
                    // 
                    // 分割数設定へ
                    return CreateMenu(Game, MenuType.SelectDivide, Data);
                }
            }



            return null;
        }


        /// <summary>
        /// Performs update during loading.
        /// 
        /// 読み込み中の更新処理を行います。
        /// </summary>
        private MenuBase UpdateLoading()
        {
            // Waits until thumbnail and movie loading has finished.
            // 
            // サムネイルとムービーの読み込み処理が終了するまで待機します。
            if (thumbTextures == null || !thumbLoader.Initialized)
                return null;

            // Obtains the texture list.
            //
            // テクスチャリストを取得します。
            thumbTextures = thumbLoader.Textures;

            // Sets the texture for the thumbnail sprite.
            //
            // サムネイルのスプライトにテクスチャをセットします。
            SetThumbnailTexture();

            // Replays the sequence animation.
            //
            // シーケンスのアニメーションをリプレイします。
            seqStart.Replay();

            // Sets the processing status to the start animation.
            //
            // 処理状態を開始アニメーションに設定します。
            phase = Phase.Start;

            return null;
        }


        /// <summary>
        /// Performs update during selection.
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

            if (buttons.A[VirtualKeyState.Push])
            {
                // Performs Enter key processing.
                // 
                // 決定キーが押されたときの処理を行います。
                InputSelectKey();
            }
            else if (buttons.B[VirtualKeyState.Push])
            {
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCancel);
                return CreateMenu(Game, MenuType.SelectStyle, Data);
            }
            else if (InputState.IsPush(dPad.Left, leftStick.Left))
            {
                InputLeftKey();
            }
            else if (InputState.IsPush(dPad.Right, leftStick.Right))
            {
                InputRightKey();
            }
            
            return null;
        }


        /// <summary>
        /// Performs Enter key processing.
        /// 
        /// 決定キーが押されたときの処理を行います。
        /// </summary>
        private void InputSelectKey()
        {
            Data.StageSetting.Movie = GameData.MovieList[cursor];
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);

            seqMovieSelect.Replay();
            phase = Phase.Selected;
        }


        /// <summary>
        /// Performs Left key processing.
        /// 
        /// 左キーが押されたときの処理を行います。
        /// </summary>
        private void InputLeftKey()
        {
            // Sets the previous cursor position.
            // 
            // 前のカーソル位置に設定します。
            cursor = CursorPrev();

            // Sets the thumbnail.
            //
            // サムネイルを設定します。
            SetPreviousThumbnail();

            // Plays the cursor movement SoundEffect.
            //
            // カーソル移動のSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);

            // Replays the sequence.
            // 
            // シーケンスをリプレイします。
            seqLeft.Replay();
        }


        /// <summary>
        /// Performs Right key processing.
        /// 
        /// 右キーが押されたときの処理を行います。
        /// </summary>
        private void InputRightKey()
        {
            // Sets the next cursor position.
            // 
            // 次のカーソル位置に設定します。
            cursor = CursorNext();

            // Sets the thumbnail.
            // 
            // サムネイルを設定します。
            SetNextThumbnail();

            // Places the cursor movement SoundEffect.
            // 
            // カーソル移動のSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);

            // Replays the sequence.
            // 
            // シーケンスをリプレイします。
            seqRight.Replay();
        }


        /// <summary>
        /// Updates the sequence.
        /// 
        /// シーケンスの更新処理を行います。
        /// </summary>
        private void UpdateSequence(GameTime gameTime)
        {
            // Updates the sequence except during loading.
            // 
            // 読み込み中以外の場面で、シーケンスを更新します。
            if (phase != Phase.Loading)
            {
                seqStart.Update(gameTime.ElapsedGameTime);
                seqLoop.Update(gameTime.ElapsedGameTime);
                seqMovieWindow.Update(gameTime.ElapsedGameTime);
                seqMovieSelect.Update(gameTime.ElapsedGameTime);
                seqLeft.Update(gameTime.ElapsedGameTime);
                seqRight.Update(gameTime.ElapsedGameTime);
                seqPosMovieTitle.Update(gameTime.ElapsedGameTime);
                seqPosMovieCount.Update(gameTime.ElapsedGameTime);

                foreach (Sprite sprite in thumbSprite)
                {
                    sprite.Update(gameTime);
                }
            }

            seqLoading.Update(gameTime.ElapsedGameTime);
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
            // ムービーのウィンドウを描画します。
            batch.Begin();
            seqMovieWindow.Draw(batch, null);
            batch.End();

            // Draws the movie.
            // 
            // ムービーを描画します。
            DrawMovie(batch);

            batch.Begin();

            // Draws the thumbnail.
            //
            // サムネイルを描画します。
            DrawThumbnail(gameTime, batch);

            // Draws the sequence.
            //
            // シーケンスを描画します。
            DrawSequence(gameTime, batch);

            batch.End();
        }


        /// <summary>
        /// Draws the movie.
        /// 
        /// ムービーを描画します。
        /// </summary>
        private void DrawMovie(SpriteBatch batch)
        {
            // Drawing is not performed during loading.
            // 
            // 読み込み中は描画処理を行いません。
            if (phase == Phase.Loading)
                return;

            if (Data.movieTexture != null)
            {
                // Draws the movie with no alpha.
                // 
                // ムービーをアルファ値無しで描画します。
                batch.Begin(SpriteBlendMode.None);
                batch.Draw(Data.movieTexture, MoviePreviewRect, Color.White);
                batch.End();
            }
            else
            {
                // Draws the load icon if the movie has not been
                // loaded.
                // 
                // ムービーがまだ読み込まれていない場合は
                // ロードアイコンを描画します。
                batch.Begin();
                seqLoading.Draw(batch, null);
                batch.End();
            }
        }


        /// <summary>
        /// Draws the thumbnail.
        /// 
        /// サムネイルを描画します。
        /// </summary>
        private void DrawThumbnail(GameTime gameTime, SpriteBatch batch)
        {
            foreach (Sprite sprite in thumbSprite)
            {
                sprite.Draw(gameTime, batch);
            }
        }


        /// <summary>
        /// Draws the sequence.
        /// 
        /// シーケンスを描画します。
        /// </summary>
        private void DrawSequence(GameTime gameTime, SpriteBatch batch)
        {
            if (phase == Phase.Start)
            {
                seqStart.Draw(batch, null);
            }
            else if (phase == Phase.Select)
            {
                seqLoop.Draw(batch, null);
                seqLeft.Draw(batch, null);
                seqRight.Draw(batch, null);

                // Draws the navigate button.
                // 
                // ナビゲートボタンを描画します。
                DrawNavigate(gameTime, batch, false);
            }
            else if (phase == Phase.Selected)
            {
                seqLoop.Draw(batch, null);
                seqMovieSelect.Draw(batch, null);
                seqLeft.Draw(batch, null);
                seqRight.Draw(batch, null);
            }

            // Draws the movie title.
            // 
            // ムービーのタイトルを描画します。
            DrawMovieTitle(batch);

            // Draws the movie number. 
            // 
            // ムービーの番号を描画します。
            DrawMovieCount(batch);
        }


        /// <summary>
        /// Draws the movie title.
        /// 
        /// ムービーのタイトルを描画します。
        /// </summary>
        private void DrawMovieTitle(SpriteBatch batch)
        {
            // Drawing is not performed if the movie is Null.
            // 
            // ムービーがnullの場合は描画処理を行いません。
            if (Data.movie == null)
                return;


            SequenceBankData seqData = seqPosMovieTitle.SequenceData;
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
                    SpriteFont font = LargeFont;
                    Color color = putInfoData.Color;
                    string text = Data.movie.Info.Name;

                    // Centers the position.
                    //
                    // 位置をセンタリングします。
                    Point point = putInfoData.Position;
                    Vector2 position = new Vector2(point.X, point.Y);
                    position -= font.MeasureString(text) * 0.5f;

                    batch.DrawString(font, text, position, color);
                }
            }
        }


        /// <summary>
        /// Draws the movie number.
        /// 
        /// ムービーの番号を描画します。
        /// </summary>
        private void DrawMovieCount(SpriteBatch batch)
        {
            SequenceBankData seqData = seqPosMovieCount.SequenceData;
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
                    SpriteFont font = LargeFont;
                    Color color = putInfoData.Color;

                    int current = cursor + 1;
                    int total = GameData.MovieList.Count;
                    string format = "{0:00}/{1:00}";
                    string text = string.Format(format, current, total);

                    Point point = putInfoData.Position;
                    Vector2 position = new Vector2(point.X, point.Y);

                    batch.DrawString(font, text, position, color);
                }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Starts loading the movie.
        ///
        /// ムービーの読み込み処理を開始します。
        /// </summary>
        /// <param name="join">
        /// Waiting for movie to finish loading
        ///
        /// ムービーの読み込み終了待ち
        /// </param>
        private void SetMovie(bool join)
        {
            // Performs release processing if 
            // movie object already exists.
            // 
            // 既にムービーオブジェクトが存在している場合は
            // 開放処理を行います。
            if (Data.movie != null)
            {
                Data.movie.Dispose();
                Data.movie = null;
            }

            Data.movieTexture = null;

            // Force quits if movie is already loading.
            // 
            // ムービーが既に読み込み中なら強制終了をします。
            if (Data.movieLoader != null && !Data.movieLoader.Initialized)
            {
                Data.movieLoader.Abort();

                // Performs release processing.
                // 
                // 開放処理を行います。
                if (Data.movieLoader.Movie != null)
                {
                    Data.movieLoader.Movie.Dispose();
                }

                Data.movieLoader = null;
            }

            // Loads movie asynchronously. 
            // For Xbox 360, set CPU core to execute thread.
            // 
            // ムービーを非同期で読み込みます。
            // Xbox 360の場合はスレッドを実行するCPUのコアを設定します。
            AnimationInfo animationInfo =
                Content.Load<AnimationInfo>(GameData.MovieList[cursor]);
            Data.movieLoader = new MovieLoader(Game, 3 + cpuId, animationInfo);
            Data.movieLoader.Run();

            // Changes the next CPU core to be used.
            //
            // 次回使用するCPUコアを変更します。
            cpuId = (cpuId + 1) % 3;

            // If set to synchronous loading, wait for completion.
            // 
            // 同期読み込みが設定されていたら終了するまで待機します。
            if (join)
            {
                Data.movieLoader.Join();
            }

            // Replays the movie title display sequence.
            // 
            // ムービータイトル表示のシーケンスをリプレイします。
            if (seqPosMovieTitle != null)
            {
                seqPosMovieTitle.Replay();
            }

        }


        /// <summary>
        /// Shifts the thumbnail one back.
        /// 
        /// サムネイルを一つ前に移動します。
        /// </summary>
        private void SetPreviousThumbnail()
        {
            ThumbnailSprite frontSprite = thumbSprite[0];
            ThumbnailSprite tailSprite = thumbSprite[thumbSprite.Count - 1];

            Vector2 frontPosition = frontSprite.TargetPosition;

            tailSprite.Position = tailSprite.TargetPosition =
                frontPosition - new Vector2(ThumbnailSize.X, 0);

            int textureId = cursor;
            textureId += GameData.MovieList.Count - (ThumbnailPanel >> 1);
            textureId %= GameData.MovieList.Count;
            tailSprite.TextureId = textureId;

            thumbSprite.Remove(tailSprite);
            thumbSprite.Insert(0, tailSprite);

            foreach (ThumbnailSprite sprite in thumbSprite)
            {
                Vector2 position = sprite.TargetPosition;
                position.X += ThumbnailSize.X;
                sprite.TargetPosition = position;
            }

            SetThumbnailTexture();

            SetMovie(false);
        }


        /// <summary>
        /// Shifts the thumbnail one forward.
        ///
        /// サムネイルを一つ次へ移動します。
        /// </summary>
        private void SetNextThumbnail()
        {
            ThumbnailSprite frontSprite = thumbSprite[0];
            ThumbnailSprite tailSprite = thumbSprite[thumbSprite.Count - 1];

            Vector2 tailPosition = tailSprite.TargetPosition;

            int textureId = cursor;
            textureId += (ThumbnailPanel >> 1) - 1;
            textureId %= GameData.MovieList.Count;
            tailSprite.TextureId = textureId;

            frontSprite.Position = frontSprite.TargetPosition = 
                tailPosition + new Vector2(ThumbnailSize.X, 0);

            thumbSprite.Remove(frontSprite);
            thumbSprite.Add(frontSprite);

            foreach (ThumbnailSprite sprite in thumbSprite)
            {
                Vector2 position = sprite.TargetPosition;
                position.X -= ThumbnailSize.X;
                sprite.TargetPosition = position;
            }

            SetThumbnailTexture();

            SetMovie(false);
        }


        /// <summary>
        /// Sets the texture for the thumbnail sprite.
        /// 
        /// サムネイルのスプライトにテクスチャを設定します。
        /// </summary>
        private void SetThumbnailTexture()
        {
            // Processing is not performed if there is no texture list.
            // 
            // テクスチャリストが無い場合は処理をしません。
            if (thumbTextures == null)
            {
                return;
            }

            // Sets the texture.
            // 
            // テクスチャを設定します。
            foreach (ThumbnailSprite sprite in thumbSprite)
            {
                sprite.Texture = thumbTextures[sprite.TextureId];
            }
        }


        /// <summary>
        /// Returns the previous cursor position.
        /// 
        /// 前のカーソル位置を返します。
        /// </summary>
        private int CursorPrev()
        {
            return ((cursor - 1) < 0) ? GameData.MovieList.Count - 1 : cursor - 1;
        }


        /// <summary>
        /// Returns the next cursor position.
        ///
        /// 次のカーソル位置を返します。
        /// </summary>
        private int CursorNext()
        {
            return (cursor + 1) % GameData.MovieList.Count;
        }
        #endregion
    }
}
