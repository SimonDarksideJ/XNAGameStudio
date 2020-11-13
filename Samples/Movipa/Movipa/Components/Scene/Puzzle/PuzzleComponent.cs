#region File Description
//-----------------------------------------------------------------------------
// PuzzleComponent.cs
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

using Movipa.Components.Animation;
using Movipa.Components.Input;
using Movipa.Components.Scene.Puzzle.Style;
using Movipa.Util;
using MovipaLibrary;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene.Puzzle
{
    /// <summary>
    /// Scene Component for processing the puzzle.
    /// Invoked in both Normal Mode and Free Mode, it
    /// changes the processing within individual methods.
    /// ChangeComponent, RevolveComponent and SlideComponent inherited
    /// from StyleBase are used to interchange panels.
    /// This component is created using the CreateStyleComponent method.
    /// 
    /// パズルの処理をするシーンコンポーネントです。
    /// ノーマルモードと、フリーモード共通で呼び出され、
    /// 各メソッド内で処理内容を変更しています。
    /// パネルの入れ替え処理には、StyleBaseを継承したChangeComponent、
    /// RevolveComponent、SlideComponentを使用します。
    /// このコンポーネントの作成にはCreateStyleComponentメソッドを使用しています。
    /// </summary>
    public class PuzzleComponent : SceneComponent
    {
        #region Public Types
        /// <summary>
        /// Game status
        /// 
        /// ゲームの状態
        /// </summary>
        public enum Phase
        {
            /// <summary>
            /// Display name and number of stages 
            /// 
            /// ステージ数と名前表示
            /// </summary>
            StageTitle,

            /// <summary>
            /// Display completed diagram
            /// 
            /// 完成図表示
            /// </summary>
            FirstPreview,

            /// <summary>
            /// Shuffle initial panels
            ///
            /// 初期パネルのシャッフル
            /// </summary>
            Shuffle,

            /// <summary>
            /// Start countdown
            ///
            /// 開始カウントダウン
            /// </summary>
            CountDown,

            /// <summary>
            /// Panel select
            ///
            /// パネルセレクト
            /// </summary>
            PanelSelect,

            /// <summary>
            /// Panel action
            ///
            /// パネル移動演出
            /// </summary>
            PanelAction,

            /// <summary>
            /// Completed judgment 
            ///
            /// 完成判定
            /// </summary>
            Judge,

            /// <summary>
            /// Completed
            ///
            /// 完成演出
            /// </summary>
            Complete,

            /// <summary>
            /// Time over
            ///
            /// タイムオーバー演出
            /// </summary>
            Timeover,
        };


        /// <summary>
        /// Paused cursor select status 
        /// 
        /// ポーズ中のカーソル選択状態
        /// </summary>
        public enum PauseCursor
        {
            /// <summary>
            /// Return to game
            ///
            /// ゲームに戻る
            /// </summary>
            ReturnToGame,

            /// <summary>
            /// Return to title
            ///
            /// タイトルに戻る
            /// </summary>
            GoToTitle,

            /// <summary>
            /// Count
            /// 
            /// カウント用
            /// </summary>
            Count
        };
        #endregion

        #region Private Types
        /// <summary>
        /// Position list
        ///
        /// ポジションリスト
        /// </summary>
        enum PositionList
        {
            /// <summary>
            /// Preview
            ///
            /// プレビュー
            /// </summary>
            Preview,

            /// <summary>
            /// Movie
            ///
            /// ムービー
            /// </summary>
            Movie,

            /// <summary>
            /// Score
            ///
            /// スコア
            /// </summary>
            Score,

            /// <summary>
            /// Time
            ///
            /// タイム
            /// </summary>
            Time,

            /// <summary>
            /// Number remaining
            ///
            /// 残り数
            /// </summary>
            Rest,

            /// <summary>
            /// Help icon
            ///
            /// ヘルプアイコン
            /// </summary>
            HelpIcon,
        }
        #endregion

        #region Fields
        // Seconds for first preview shown
        // 
        // 最初に表示するプレビューの秒数
        private readonly TimeSpan FirstPreviewTime = new TimeSpan(0, 0, 3);

        // Seconds to stop animation in help item
        // 
        // ヘルプアイテムでアニメーションを止める秒数
        private readonly TimeSpan HelpItemTime = new TimeSpan(0, 0, 3);

        /// <summary>
        /// Preview zoom speed
        /// 
        /// プレビュー拡大縮小速度
        /// </summary>
        private const float ThumbZoomSpeed = 0.05f;

        /// <summary>
        /// Score when aligned in single
        ///
        /// シングルで揃った時のスコア
        /// </summary>
        private const int ScoreSingle = 10;

        /// <summary>
        /// Score when aligned in double
        ///
        /// ダブルで揃った時のスコア
        /// </summary>
        private const int ScoreDouble = 50;

        /// <summary>
        /// Bonus score for help item
        ///
        /// ヘルプアイテムのボーナススコア
        /// </summary>
        private const int ScoreHelpItem = 100;

        /// <summary>
        /// Help item count
        ///
        /// ヘルプアイテムの所持数
        /// </summary>
        private const int HelpItemCount = 3;

        // Components
        private StyleBase style;
        private FadeSeqComponent fade;

        // Processing status
        // 
        // 処理状態
        private Phase phase;

        // Display time for first preview shown
        // 
        // 最初に表示するプレビューの表示時間
        private TimeSpan firstPreviewTime;

        // Play time
        //
        // プレイ時間
        private TimeSpan playTime;

        // Movie rectangle vertices list
        // 
        // ムービー矩形の頂点リスト
        private Vector2[] movieFramePosition;

        // Rectangle list of movie excluding panels 
        // 
        // パネルを除いたムービーの矩形リスト
        private Rectangle[] movieFrameSrc;

        // Preview zoom status
        // 
        // プレビューのズーム状態
        private bool thumbZoom;

        // Preview zoom ratio
        // 
        // プレビューの拡大率
        private float thumbZoomRate;

        // Movie display rectangle
        // 
        // ムービーの表示矩形
        private Rectangle movieRect;

        // Preview display rectangle
        // 
        // プレビューの表示矩形
        private Rectangle movieThumbRect;

        // Popup score sprite list
        // 
        // ポップアップスコアのスプライトリスト
        private List<SpriteScorePopup> scorePopupList;

        // Current score
        // 
        // 現在のスコア
        private int score;

        // Displayed score
        // 
        // 表示するスコア
        // Defined separately for addition animation.
        // 
        // 加算アニメーションの為に別にしている。
        private int scoreView;

        // Remaining shuffle count
        // 
        // シャッフルの残り回数
        private int shuffleCount;

        // Pause status
        // 
        // ポーズ状態
        private bool isPause;

        // Paused cursor position
        // 
        // ポーズのカーソル位置
        private PauseCursor pauseCursor;

        // Cross-fade color
        // 
        // クロスフェード用カラー
        private Vector4 completeColor;

        // Cross-fade texture
        // 
        // クロスフェード用テクスチャ
        private ResolveTexture2D resolveTexture = null;

        // Stage information 
        // 
        // ステージ情報
        private StageSetting setting;

        // Play result
        // 
        // プレイ結果
        private StageResult result;

        // Panel manager class
        // 
        // パネル管理クラス
        private PanelManager panelManager;

        // Background texture
        // 
        // 背景テクスチャ
        private Texture2D wallpaperTexture;

        // Cursor position
        // 
        // カーソル位置
        private Point cursor = new Point();

        // BackgroundMusic
        private Cue bgm;

        // Movie animation
        // 
        // ムービーアニメーション
        private PuzzleAnimation movie;

        // Movie animation texture
        // 
        // ムービーアニメーションのテクスチャ
        private Texture2D movieTexture;

        // Help item count
        // 
        // ヘルプアイテムの所持数
        private int helpItemCount;

        // Help item stop time
        // 
        // ヘルプアイテムの停止時間
        private TimeSpan helpItemTime;

        // Layout
        private SceneData sceneData;
        private SequencePlayData seqMainFrame;
        private SequencePlayData seqCountDown;
        private SequencePlayData seqTimeUp;
        private SequencePlayData seqHelpIcon;
        private SequencePlayData seqRefStart;
        private SequencePlayData seqRefLoop;
        private SequencePlayData seqRefNaviStart;
        private SequencePlayData seqRefNaviLoop;
        private SequencePlayData seqRefReturnToGameOn;
        private SequencePlayData seqRefGoToTitleOn;
        private SequencePlayData seqStage;
        private SequencePlayData seqComplete;

        // Position list
        // 
        // ポジションリスト
        private Dictionary<PositionList, Vector2> positions;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the processing status.
        /// 
        /// 処理状態を取得または設定します。
        /// </summary>
        public Phase GamePhase
        {
            get { return phase; }
            set { phase = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public PuzzleComponent(Game game, StageSetting stageSetting)
            : base(game)
        {
            setting = stageSetting;
        }


        /// <summary>
        /// Performs initialization processing.
        ///
        /// 初期化処理を行います。
        /// </summary>
        public override void Initialize()
        {
            // Initializes the processing status. 
            // In Normal Mode, sets to stage display.
            // 
            // 処理状態の初期設定を行います。
            // ノーマルモードの場合はステージを表示する処理に設定します。
            GamePhase = (setting.Mode == StageSetting.ModeList.Normal) ?
                Phase.StageTitle : Phase.FirstPreview;

            // Initializes the member variables.
            // 
            // メンバ変数の初期化を行います。
            score = 0;
            scoreView = 0;
            result = new StageResult();
            isPause = false;
            playTime = new TimeSpan();
            firstPreviewTime = FirstPreviewTime;
            helpItemCount = HelpItemCount;
            helpItemTime = new TimeSpan();
            thumbZoomRate = 1.0f;
            thumbZoom = true;
            completeColor = Vector4.One;

            // Sets the shuffle count.
            // The shuffle count is twice the total number of panels.
            // 
            // シャッフルする回数を設定します。
            // シャッフル回数は総パネル数を2倍した数値です。
            shuffleCount = (setting.Divide.X * setting.Divide.Y) * 2;

            // Creates the panel management class.
            // 
            // パネル管理クラスを作成します。
            panelManager = new PanelManager(Game);

            // Creates the panel.
            // 
            // パネルを作成します。
            panelManager.CreatePanel(GameData.MovieSizePoint, setting);

            // Adds the component for interchanging panels.
            // 
            // パネルを入れ替えるコンポーネントを追加します。
            style = CreateStyleComponent();
            AddComponent(style);

            // Sets to No Drawing on the component side.
            // 
            // コンポーネント側で描画しない設定をします。
            style.Visible = false;

            // Creates the popup score sprite array.
            // 
            // ポップアップするスコアのスプライト配列を作成します。
            scorePopupList = new List<SpriteScorePopup>();

            // Obtains the fade component instance. 
            // 
            // フェードコンポーネントのインスタンスを取得します。
            fade = GameData.FadeSeqComponent;

            // Sets the fade-in processing.
            // 
            // フェードインの処理を設定します。
            fade.Start(FadeType.Normal, FadeMode.FadeIn);

            base.Initialize();
        }


        /// <summary>
        /// Loads the content.
        /// 
        /// コンテントを読み込みます。
        /// </summary>
        protected override void LoadContent()
        {
            // Loads the background texture.
            // 
            // 背景のテクスチャを読み込みます。
            string asset = "Textures/Wallpaper/Wallpaper_006";
            wallpaperTexture = Content.Load<Texture2D>(asset);

            // Initializes the sequence.
            // 
            // シーケンスの初期化を行います。
            InitializeSequence();

            // Initializes the movie.
            // 
            // ムービーの初期化を行います。
            InitializeMovie();

            // Plays the BackgroundMusic and obtains the Cue.
            // 
            // BackgroundMusicを再生し、Cueを取得します。
            bgm = GameData.Sound.PlayBackgroundMusic(Sounds.GameBackgroundMusic);
            
            base.LoadContent();
        }


        /// <summary>
        /// Initializes the sequence.
        /// 
        /// シーケンスの初期化を行います。
        /// </summary>
        private void InitializeSequence()
        {
            // Loads the SceneData.
            // 
            // SceneDataを読み込みます。
            string asset = "Layout/game/game_Scene";
            sceneData = Content.Load<SceneData>(asset);

            // Creates the sequence.
            // 
            // シーケンスを作成します。
            string name = (setting.Mode == StageSetting.ModeList.Normal) ?
                "GameNormal" : "GameFree";
            seqMainFrame = sceneData.CreatePlaySeqData(name);

            seqCountDown = sceneData.CreatePlaySeqData("StartCount");
            seqTimeUp = sceneData.CreatePlaySeqData("TimeUp");
            seqHelpIcon = sceneData.CreatePlaySeqData("HelpIcon");
            seqRefStart = sceneData.CreatePlaySeqData("RefStart");
            seqRefLoop = sceneData.CreatePlaySeqData("RefLoop");
            seqRefReturnToGameOn = sceneData.CreatePlaySeqData("RefReturnToGameOn");
            seqRefGoToTitleOn = sceneData.CreatePlaySeqData("RefGoToTitleOn");
            seqStage = sceneData.CreatePlaySeqData("Stage");
            seqComplete = sceneData.CreatePlaySeqData("Complete");

            // Creates the sequence in accordance with the style.
            // 
            // スタイルに応じたシーケンスを作成します。
            if (setting.Style == StageSetting.StyleList.Change &&
                setting.Rotate == StageSetting.RotateMode.On)
            {
                seqRefNaviStart = sceneData.CreatePlaySeqData("RefChangeRotateStart");
                seqRefNaviLoop = sceneData.CreatePlaySeqData("RefChangeRotateLoop");
            }
            else if (setting.Style == StageSetting.StyleList.Change &&
                setting.Rotate == StageSetting.RotateMode.Off)
            {
                seqRefNaviStart = sceneData.CreatePlaySeqData("RefChangeStart");
                seqRefNaviLoop = sceneData.CreatePlaySeqData("RefChangeLoop");
            }
            else if (setting.Style == StageSetting.StyleList.Revolve)
            {
                seqRefNaviStart = sceneData.CreatePlaySeqData("RefRevolveStart");
                seqRefNaviLoop = sceneData.CreatePlaySeqData("RefRevolveLoop");
            }
            else if (setting.Style == StageSetting.StyleList.Slide)
            {
                seqRefNaviStart = sceneData.CreatePlaySeqData("RefSlideStart");
                seqRefNaviLoop = sceneData.CreatePlaySeqData("RefSlideLoop");
            }

            // Replays the sequence.
            // 
            // シーケンスをリプレイします。
            seqCountDown.Replay();
            seqTimeUp.Replay();
            seqComplete.Replay();

            // Loads the position.
            // 
            // ポジションを読み込みます。
            positions = new Dictionary<PositionList, Vector2>();
            PatternGroupData patternGroup;
            PatternObjectData patternObject;
            Point point;

            // Obtains the pattern group.
            // 
            // パターングループを取得します。
            patternGroup = sceneData.PatternGroupDictionary["Main_Pos"];

            // Obtains the preview position.
            // 
            // プレビューのポジションを取得します。
            patternObject = patternGroup.PatternObjectList[0];
            point = patternObject.Position;
            positions.Add(PositionList.Preview, new Vector2(point.X, point.Y));
            movieThumbRect = new Rectangle(
                point.X,
                point.Y,
                (int)(patternObject.Rect.Width * patternObject.Scale.X),
                (int)(patternObject.Rect.Height * patternObject.Scale.Y));


            // Obtains the movie position.
            // 
            // ムービーのポジションを取得します。
            patternObject = patternGroup.PatternObjectList[1];
            point = patternObject.Position;
            positions.Add(PositionList.Movie, new Vector2(point.X, point.Y));
            movieRect = new Rectangle(
                point.X,
                point.Y,
                (int)(patternObject.Rect.Width * patternObject.Scale.X),
                (int)(patternObject.Rect.Height * patternObject.Scale.Y));

            // Obtains the score position.
            // 
            // スコアのポジションを取得します。
            point = patternGroup.PatternObjectList[2].Position;
            positions.Add(PositionList.Score, new Vector2(point.X, point.Y));

            // Obtains the time position.
            // 
            // タイムのポジションを取得します。
            point = patternGroup.PatternObjectList[3].Position;
            positions.Add(PositionList.Time, new Vector2(point.X, point.Y));

            // Obtains the remaining panel count position.
            // 
            // 残りパネル数のポジションを取得します。
            point = patternGroup.PatternObjectList[4].Position;
            positions.Add(PositionList.Rest, new Vector2(point.X, point.Y));

            // Obtains the help icon position.
            // 
            // ヘルプアイコンのポジションを取得します。
            point = patternGroup.PatternObjectList[5].Position;
            positions.Add(PositionList.HelpIcon, new Vector2(point.X, point.Y));
        }


        /// <summary>
        /// Initializes the movie.
        /// 
        /// ムービーの初期化を行います。
        /// </summary>
        private void InitializeMovie()
        {
            // Loads movie information.
            // 
            // ムービーの情報を読み込みます。
            AnimationInfo info = Content.Load<AnimationInfo>(setting.Movie);

            // Adds movie components.
            // 
            // ムービーのコンポーネントを追加します。
            movie = CreateMovie(info);
            AddComponent(movie);

            // Turns automatic movie updating off.
            //
            // ムービーを自動で更新しないように設定します。
            movie.Enabled = false;

            // Obtains the movie outer rectangle.
            // 
            // ムービー外の矩形を取得します。
            movieFramePosition = GetMovieFramePosition();
            movieFrameSrc = GetMovieFrameSource();

            // Sets the panel draw position offset.
            // 
            // パネル描画位置のオフセットを設定します。
            panelManager.DrawOffset = positions[PositionList.Movie];
        }


        /// <summary>
        /// Creates the screen back buffer.
        /// 
        /// 画面のバックバッファを作成します。
        /// </summary>
        private void InitializeBackBuffer()
        {
            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            resolveTexture = new ResolveTexture2D(
                GraphicsDevice,
                pp.BackBufferWidth,
                pp.BackBufferHeight,
                1,
                pp.BackBufferFormat);
        }


        /// <summary>
        /// Releases all content.
        ///
        /// 全てのコンテントを開放します。
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
            // Updates the sequence.
            // 
            // シーケンスの更新処理を行います。
            UpdateSequence(gameTime);

            // Updates the preview.
            //
            // プレビューの更新処理を行います。
            UpdatePreview();

            // Updates the movie.
            // 
            // ムービーの更新処理を行います。
            UpdateMovie(gameTime);

            // Updates the score.
            // 
            // スコアの更新処理を行います。
            UpdateScore(gameTime);

            if (fade.FadeMode == FadeMode.FadeIn)
            {
                // Sets to Main Processing after the fade-in finishes.
                // 
                // フェードインが終了したら、メインの処理に設定します。
                if (!fade.IsPlay)
                {
                    fade.FadeMode = FadeMode.None;
                }
            }
            else if (fade.FadeMode == FadeMode.None)
            {
                // Performs main update processing.
                // 
                // メインの更新処理を行います。
                UpdateMain(gameTime);
            }
            else if (fade.FadeMode == FadeMode.FadeOut)
            {
                // Performs update processing at fade-out.
                // 
                // フェードアウト時の更新処理を行います。
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
            // Updates the background sequence.
            // 
            // 背景のシーケンスを更新します。
            seqMainFrame.Update(gameTime.ElapsedGameTime);

            // Updates the help icon sequence.
            // 
            // ヘルプアイコンのシーケンスを更新します。
            seqHelpIcon.Update(gameTime.ElapsedGameTime);

            // Updates the pause screen sequence. 
            // 
            // ポーズ画面のシーケンスを更新します。
            seqRefStart.Update(gameTime.ElapsedGameTime);
            seqRefLoop.Update(gameTime.ElapsedGameTime);
            seqRefNaviStart.Update(gameTime.ElapsedGameTime);
            seqRefNaviLoop.Update(gameTime.ElapsedGameTime);
            seqRefReturnToGameOn.Update(gameTime.ElapsedGameTime);
            seqRefGoToTitleOn.Update(gameTime.ElapsedGameTime);
        }


        /// <summary>
        /// Updates the preview.
        /// 
        /// プレビューの更新処理を行います。
        /// </summary>
        private void UpdatePreview()
        {
            float zoomRate = (thumbZoom) ? ThumbZoomSpeed : -ThumbZoomSpeed;
            thumbZoomRate = MathHelper.Clamp(thumbZoomRate + zoomRate, 0.0f, 1.0f);
        }


        /// <summary>
        /// Updates the movie. 
        /// 
        /// ムービーの更新処理を行います。
        /// </summary>
        private void UpdateMovie(GameTime gameTime)
        {
            GameTime time;

            // Reduces the hint item elapsed time.
            // 
            // ヒントアイテムの使用時間を減らします。
            helpItemTime -= gameTime.ElapsedGameTime;

            // If the hint time is greater than 0, the movie update time is set to zero.
            // If the hint time is negative, the normal update time is set.
            // 
            // ヒントタイムが0より多ければ、ムービーの更新時間をゼロに設定します。
            // ヒントタイムがマイナスならば、通常の更新時間を設定します。
            time = (helpItemTime <= TimeSpan.Zero) ? gameTime : new GameTime();

            // Updates the movie.
            // 
            // ムービーを更新します。
            movie.Update(time);
        }


        /// <summary>
        /// Performs update processing at fade-out.
        /// 
        /// フェードアウト時の更新処理を行います。
        /// </summary>
        private void UpdateFadeOut()
        {
            // Sets the BackgroundMusic volume.
            // 
            // BackgroundMusicのボリュームを設定します。
            float volume = 1.0f - (GameData.FadeSeqComponent.Count / 60.0f);
            SoundComponent.SetVolume(bgm, volume);

            // Performs release processing when the fade-out has finished.
            // 
            // フェードアウトが終了したら開放処理を行います。
            if (!fade.IsPlay)
            {
                Dispose();
            }
        }




        /// <summary>
        /// Performs main update processing.
        /// 
        /// メインの更新処理を行います。
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public void UpdateMain(GameTime gameTime)
        {
            // Determines the pause.
            // 
            // ポーズの判定をします。
            if (isPause)
            {
                // Performs update processing during pause.
                // 
                // ポーズ中の更新処理を行います。
                UpdatePause();
                return;
            }
            else
            {
                // Checks the pause button status, and sets to Pause 
                // if it has been pressed.
                // 
                // ポーズボタンが押されたかチェックし、
                // 押されていればポーズの設定を行います。
                if (CheckPause())
                {
                    return;
                }
            }

            if (GamePhase == Phase.StageTitle)
            {
                // Performs update processing when the number of stages 
                // and the name are displayed. 
                // 
                // ステージ数を名前を表示しているときの更新処理を行います。
                UpdateStageTitle(gameTime);
            }
            else if (GamePhase == Phase.FirstPreview)
            {
                // Performs update processing to display the completed picture preview.
                // 
                // 完成図のプレビューを表示する更新処理を行います。
                UpdateFirstPreview(gameTime);
            }
            else if (GamePhase == Phase.Shuffle)
            {
                // Performs update processing at panel shuffle.
                //
                // パネルシャッフル時の更新処理を行います。
                UpdateShuffle();
            }
            else if (GamePhase == Phase.CountDown)
            {
                // Performs update processing during countdown.
                // 
                // カウントダウン中の更新処理を行います。
                UpdateCountDown(gameTime);
            }
            else if (GamePhase == Phase.PanelSelect)
            {
                // Performs update processing at panel selection.
                // 
                // パネル選択時の更新処理を行います。
                UpdatePanelSelect(gameTime);
            }
            else if (GamePhase == Phase.PanelAction)
            {
                // Performs updates processing during panel action.
                // 
                // パネル動作中の更新処理を行います。
                UpdatePanelAction();
            }
            else if (GamePhase == Phase.Judge)
            {
                // Performs update processing to judge panels.
                // 
                // パネルの判定をする更新処理を行います。
                UpdateJudge();
            }
            else if (GamePhase == Phase.Complete)
            {
                // Performs update processing at completion.
                // 
                // 完成時の更新処理を行います。
                UpdateComplete(gameTime);
            }
            else if (GamePhase == Phase.Complete)
            {
                // Performs update processing at completion.
                // 
                // 完成時の更新処理を行います。
                UpdateComplete(gameTime);
            }
            else if (GamePhase == Phase.Timeover)
            {
                // Performs update processing at time over.
                // 
                // タイムオーバー時の更新処理を行います。
                UpdateTimeOver(gameTime);
            }
        }


        /// <summary>
        /// Performs update processing when the number of stages 
        /// and the name are displayed.
        /// 
        /// ステージ数を名前を表示しているときの更新処理を行います。
        /// </summary>
        public void UpdateStageTitle(GameTime gameTime)
        {
            seqStage.Update(gameTime.ElapsedGameTime);

            // Sets to Initial Preview Processing after sequence playback finishes.

            // 
            // シーケンスの再生が終了したら、最初のプレビュー処理に設定します。
            if (!seqStage.IsPlay)
            {
                GamePhase = Phase.FirstPreview;
            }
        }


        /// <summary>
        /// Performs update processing to display completed picture preview.
        /// 
        /// 完成図のプレビューを表示する更新処理を行います。
        /// </summary>
        public void UpdateFirstPreview(GameTime gameTime)
        {
            // Reduces the display time.
            // 
            // 表示時間を減らします。
            firstPreviewTime -= gameTime.ElapsedGameTime;

            if (firstPreviewTime < TimeSpan.Zero)
            {
                // Sets the thumbnail to reduced size.
                // 
                // サムネイルを縮小に設定します。
                if (thumbZoom)
                {
                    thumbZoom = false;
                }

                // Sets processing status to Shuffle after 
                // thumbnail reduction has finished.
                // 
                // サムネイルの縮小が完了したら処理状態をシャッフルに設定します。
                if (thumbZoomRate == 0.0f)
                {
                    GamePhase = Phase.Shuffle;
                }
            }
        }


        /// <summary>
        /// Performs update processing at panel shuffle.
        /// 
        /// パネルシャッフル時の更新処理を行います。
        /// </summary>
        public void UpdateShuffle()
        {
            // Obtains the number of completed panels.
            // 
            // 完成枚数を取得します。
            int complete = panelManager.PanelCompleteCount(setting);

            // Checks to see if all shuffles are completed.
            // 
            // 全てのシャッフルが完了したかチェックします。
            if (shuffleCount == 0 && complete == 0)
            {
                // Performs the panel judgment and sets the processing status
                // to Countdown.
                // 
                // パネルの判定をし、処理状態をカウントダウンに設定します。
                panelManager.PanelCompleteCheck(setting);

                GamePhase = Phase.CountDown;
            }

            // Shuffles the panels.
            // 
            // パネルをシャッフルします。
            if (style.RandomShuffle())
            {
                // Reduces the shuffle count.
                // 
                // シャッフル回数を減らします。
                if (shuffleCount > 0)
                {
                    shuffleCount--;
                }
            }
        }


        /// <summary>
        /// Performs update processing during countdown.
        /// 
        /// カウントダウン中の更新処理を行います。
        /// </summary>
        public void UpdateCountDown(GameTime gameTime)
        {
            seqCountDown.Update(gameTime.ElapsedGameTime);

            // Sets the processing status to Panel Selection
            // after the sequence finishes.
            // 
            // シーケンスが終了したら処理状態をパネルの選択に設定します。
            if (!seqCountDown.IsPlay)
            {
                // Enables panel selection.
                // 
                // パネルの選択を可能にします。
                style.SelectEnabled = true;

                GamePhase = Phase.PanelSelect;
            }
        }


        /// <summary>
        /// Performs update processing at panel selection.
        /// 
        /// パネル選択時の更新処理を行います。
        /// </summary>
        public void UpdatePanelSelect(GameTime gameTime)
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            // Updates the play time. 
            // 
            // プレイタイムの更新処理を行います。
            UpdatePlayTime(gameTime);

            // Performs time limit judgment.
            // 
            // タイムリミットの判定を行います。
            if (IsTimeLimit())
            {
                // Sets to the same value as the time limit 
                // to prevent a negative time readout.
                // 
                // タイム表記がマイナスに行かないように、
                // 制限時間と同じ値を設定します。
                playTime = setting.TimeLimit;

                // Sets the processing status to Time Over.
                //
                // 処理状態をタイムオーバーに設定します。
                GamePhase = Phase.Timeover;
            }

            // Changes the processing status if the panel is currently in action.
            // 
            // パネルが動作中ならば、処理状態を変更します。
            if (style.IsPanelAction)
            {
                GamePhase = Phase.PanelAction;
            }

            // Uses the help item.
            //
            // ヘルプアイテムを使用します。
            if (buttons.LeftShoulder[VirtualKeyState.Push])
            {
                UseHelpItem();
            }

            // Sets the thumbnail size status.
            // 
            // サムネイルのズーム状態を設定します。
            thumbZoom = buttons.RightShoulder[VirtualKeyState.Press];
        }


        /// <summary>
        /// Updates the play time.
        /// 
        /// プレイタイムの更新処理を行います。
        /// </summary>
        private void UpdatePlayTime(GameTime gameTime)
        {
            playTime += gameTime.ElapsedGameTime;
            result.ClearTime = playTime;
            if (setting.Mode == StageSetting.ModeList.Normal)
            {
                GameData.SaveData.TotalPlayTime += gameTime.ElapsedGameTime;
            }
        }


        /// <summary>
        /// Performs update processing during panel actions.
        /// 
        /// パネル動作中の更新処理を行います。
        /// </summary>
        public void UpdatePanelAction()
        {
            // Sets the processing status to Judge after the panel actions finish.
            // 
            // パネルの動作が完了したら、処理状態を判定に設定します。
            if (!style.IsPanelAction)
            {
                GamePhase = Phase.Judge;
            }
        }


        /// <summary>
        /// Updates panel judgment.
        /// 
        /// パネルの判定をする更新処理を行います。
        /// </summary>
        public void UpdateJudge()
        {
            // Obtains the newly completed panel list.
            // 
            // 新たに完成されたパネルのリストを取得します。
            List<PanelData> list = panelManager.PanelCompleteCheck(setting);

            // Plays the SoundEffect if all panels are prepared.
            // 
            // パネルが揃っていればSoundEffectを再生します。
            if (list.Count > 0)
            {
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);
            }

            // Checks the number of prepared panels.
            // 
            // 揃った個数をチェックします。
            if (list.Count == 1)
            {
                PanelCompleteSingle(list);
            }
            else if (list.Count == 2)
            {
                PanelCompleteDouble(list);
            }

            // Sets the processing status to Completed if all panels are complete.
            // 
            // 全て完成したら、処理状態を完成に設定します。
            if (panelManager.PanelCompleteRatio(setting) >= 100)
            {
                GamePhase = Phase.Complete;

                // Disables panel selection.
                // 
                // パネルを選択不可に設定します。
                style.SelectEnabled = false;
            }
            else
            {
                // If still not completed, sets the processing status to Panel Select.
                // 
                // まだ完成していない場合は、処理状態をパネル選択に設定します。
                GamePhase = Phase.PanelSelect;
            }
        }


        /// <summary>
        /// Performs update processing at completion.
        /// 
        /// 完成時の更新処理を行います。
        /// </summary>
        public void UpdateComplete(GameTime gameTime)
        {
            // Processing is not performed during score updates.
            // 
            // スコアが更新中なら処理をしません。
            if (scorePopupList.Count > 0 || score != scoreView)
                return;

            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            // Updates the complete sequence.
            // 
            // コンプリートのシーケンスを更新します。
            seqComplete.Update(gameTime.ElapsedGameTime);

            // Sets the cross-fade buffer.
            // 
            // クロスフェード用のバッファを設定します。
            if (resolveTexture == null)
            {
                // Creates the back buffer.
                // 
                // バックバッファの作成をします。
                InitializeBackBuffer();

                // Transfers the current screen to the back buffer.
                // 
                // 現在の画面をバックバッファに転送します。
                GraphicsDevice.ResolveBackBuffer(resolveTexture);

                // Plays the clear SoundEffect.
                // 
                // クリアのSoundEffectを再生します。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectClear);

                // Registers the Navigate button settings.
                // 
                // ナビゲートボタンの設定をします。
                Navigate.Clear();
                Navigate.Add(new NavigateData(AppSettings("A_Ok"), true));
            }

            // Reduces the cross-fade transparency color.
            // 
            // クロスフェード用の透過色を下げていきます。
            completeColor.W = MathHelper.Clamp(completeColor.W - 0.01f, 0.0f, 1.0f);

            // Omits processing where transparency color still remains.
            // 
            // 透過色がまだ残っている場合は処理を抜けます。
            if (completeColor.W > 0)
                return;

            // Once the performance is completely finished, the A button switches 
            // to the results display screen.
            // 
            // 演出が全て終わったらAボタンで結果表示画面に遷移します。
            if (buttons.A[VirtualKeyState.Push])
            {
                // Plays the SoundEffect of the Enter button.
                // 
                // 決定ボタンのSoundEffectを再生します。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);

                // Adds the bonus based on the number of help items remaining.
                //
                // 残りのヘルプアイテムの数に応じてボーナスを加算します。
                result.HintScore = ScoreHelpItem * helpItemCount;

                // Sets the move count.
                // 
                // 移動回数の設定をします。
                result.MoveCount = style.MoveCount;

                // Adds the scene for the next transition.
                // Divides the result display classes for 
                // Normal Mode and Free Mode. 
                // 
                // 次に遷移するシーンの追加をします。
                // ノーマルモードと、フリーモードで結果表示画面の
                // クラスをわけています。
                SceneComponent scene;
                if (setting.Mode == StageSetting.ModeList.Normal)
                {
                    scene = new Result.NormalResult(Game, result);
                }
                else
                {
                    scene = new Result.FreeResult(Game, result);
                }
                GameData.SceneQueue.Enqueue(scene);

                // Sets the fade-out process.
                // 
                // フェードアウトの処理を設定します。
                GameData.FadeSeqComponent.Start(FadeType.RotateBox, FadeMode.FadeOut);
            }
        }


        /// <summary>
        /// Performs time over update processing.
        /// 
        /// タイムオーバー時の更新処理を行います。
        /// </summary>
        public void UpdateTimeOver(GameTime gameTime)
        {
            seqTimeUp.Update(gameTime.ElapsedGameTime);

            // When the time over animation finishes, registers the 
            // game over scene and sets the fade-out process.
            // 
            // タイムオーバーのアニメーションが終了したらゲームオーバーの
            // シーンを登録し、フェードアウトの処理を設定します。
            if (!seqTimeUp.IsPlay)
            {
                GameData.SceneQueue.Enqueue(new GameOver(Game));
                GameData.FadeSeqComponent.Start(FadeType.Normal, FadeMode.FadeOut);
            }
        }


        /// <summary>
        /// Performs update processing during pause.
        /// 
        /// ポーズ中の更新処理を行います。
        /// </summary>
        public void UpdatePause()
        { 
            // The following process is not performed while the sequence is playing.
            // 
            // シーケンスが再生中なら以下の処理を行いません。
            if (seqRefNaviStart.IsPlay)
                return;

            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;
            VirtualPadDPad dPad = virtualPad.DPad;
            VirtualPadDPad leftStick = virtualPad.ThumbSticks.Left;

            if (InputState.IsPush(buttons.B, buttons.Start))
            {
                // Plays the canceled SoundEffect and releases the pause status
                // when either the B button or the start button is pressed.
                //
                // Bボタンか、スタートボタンが押されたらキャンセルの
                // SoundEffectを再生し、ポーズ状態を解除します。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCancel);

                isPause = false;

                // Enables the panel selection.
                // 
                // パネル選択可能状態にする
                style.SelectEnabled = true;
            }
            else if (buttons.A[VirtualKeyState.Push])
            {
                if (pauseCursor == PauseCursor.GoToTitle)
                {
                    // When confirmed with the A button, Return to Title
                    // is selected, so it plays the determined SoundEffect and 
                    // registers the title scene, then sets the fade-out process.
                    // 
                    // Aボタンで決定時、タイトルに戻るを選択しているので
                    // 決定のSoundEffectを再生し、タイトルのシーンを登録してから
                    // フェードアウトの処理を設定します。
                    GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);

                    // Adds the scene for the next transition
                    // 
                    // 次に遷移するシーンの追加
                    GameData.SceneQueue.Enqueue(new Title(Game));
                    fade.Start(FadeType.RotateBox, FadeMode.FadeOut);
                }
                else if (pauseCursor == PauseCursor.ReturnToGame)
                {
                    // When confirmed with the A button, the Return to Game
                    // item is selected, so the behavior is the same as Cancel.
                    // 
                    // Aボタンで決定時、ゲームに戻る項目を選択しているので
                    // キャンセルと同じ挙動をします。
                    GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCancel);

                    isPause = false;

                    // Enables the panel selection.
                    //
                    // パネル選択可能状態にする
                    style.SelectEnabled = true;
                }
            }
            else if (InputState.IsPush(dPad.Up, leftStick.Up, dPad.Down, 
                leftStick.Down))
            {
                // Moves the cursor vertically. 
                // There are only two items, so common processing is used.
                //
                // 上下でカーソルを移動します。
                // 項目が2つしかないので、共通の処理を使用します。

                int count = (int)PauseCursor.Count;
                int cursor = (int)pauseCursor;

                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor2);
                pauseCursor = (PauseCursor)((cursor + 1) % count);
            }
        }


        /// <summary>
        /// Updates the score.
        /// 
        /// スコアの更新処理を行います。
        /// </summary>
        public void UpdateScore(GameTime gameTime)
        {
            // Updates the sprite.
            // 
            // スプライトの更新を行います。
            for (int i = 0; i < scorePopupList.Count; i++)
            {
                SpriteScorePopup sprite = scorePopupList[i];
                sprite.Update(gameTime);

                // Adds up the score and deletes it from the array
                // after the sprite action finishes.
                // 
                // スプライトの動作が終わったらスコアを加算し、
                // 配列から削除します。
                if (sprite.Disposed)
                {
                    score += sprite.Score;
                    scorePopupList.Remove(sprite);
                }
            }

            // Adds up the display score so that it tracks the actual score.
            // 
            // 表示用のスコアは、実際のスコアを追うように加算します。
            if (score > scoreView)
            {
                scoreView++;
            }
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Performs drawing processing.
        /// 
        /// 描画処理を行います。
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            // Draws the game.
            // 
            // ムービー描画
            movieTexture = movie.Texture;
            style.MovieTexture = movieTexture;

            // Draws the background.
            // 
            // 背景を描画する
            Batch.Begin();
            {
                // Draws the BG.
                // 
                // BGを描画
                Batch.Draw(wallpaperTexture, Vector2.Zero, Color.White);

                // Draws the main frame.
                // 
                // メインのフレームを描画
                seqMainFrame.Draw(Batch, null);

                // Draws the help icon.
                // 
                // ヘルプアイコンの描画
                DrawHelpIcon(Batch);

                // Draws the text string.
                // 
                // 文字列の描画
                SpriteFont font = LargeFont;
                DrawTextScore(Batch, font);
                DrawTextTime(Batch, font);
                DrawTextRest(Batch, font);
            }
            Batch.End();

            // Draws the movie outer frame.
            // 
            // ムービーの外枠を描画
            DrawOutMovie();

            // Draws the movie panels.
            // 
            // ムービーのパネル描画
            style.DrawPanels(gameTime);

            // Draws the cursor.
            //
            // カーソル描画
            style.DrawCursor(gameTime);

            // Draws the popup score.
            //
            // ポップアップスコアを描画
            Batch.Begin();
            {
                SpriteBatch batch = Batch;
                foreach (SpriteScorePopup sprite in scorePopupList)
                {
                    sprite.Draw(gameTime, batch);
                }
            }
            Batch.End();

            // Draws the preview.
            //
            // プレビューを描画
            DrawThumbnail();

            // Performs drawing processing as per status.
            //
            // 状態に応じた描画
            DrawMain(gameTime);

            base.Draw(gameTime);
        }


        /// <summary>
        /// Performs main drawing processing.
        /// 
        /// メインの描画処理を行います。
        /// </summary>
        private void DrawMain(GameTime gameTime)
        {
            if (GamePhase == Phase.StageTitle)
            {
                // Draws the number of stages and the title.
                // 
                // ステージ数とタイトルの描画処理を行います。
                DrawStageTitle();
            }
            else if (GamePhase == Phase.CountDown)
            {
                // Performs countdown drawing.
                // 
                // カウントダウンの描画処理を行います。
                DrawCountDown();
            }
            else if (GamePhase == Phase.Complete)
            {
                // Performs drawing processing at completion.
                // 
                // 完成時の描画処理を行います。
                DrawComplete(gameTime);
            }
            else if (GamePhase == Phase.Timeover)
            {
                // Performs over time drawing.
                // 
                // タイムオーバーの描画処理を行います。
                DrawTimeOver();
            }

            // Performs drawing processing during pause.
            // 
            // ポーズ中の描画を行います。
            if (isPause)
            {
                DrawPause(gameTime);
            }

        }


        /// <summary>
        /// Draws the number of stages and the title.
        /// 
        /// ステージ数とタイトルの描画処理を行います。
        /// </summary>
        private void DrawStageTitle()
        {
            Batch.Begin();

            // Draws the sequence.
            // 
            // シーケンスの描画をします。
            seqStage.Draw(Batch, null);


            SequenceGroupData seqBodyData;
            string text;

            // Draws the text string for the number of stages.
            // 
            // ステージ数の文字列を描画します。
            text = string.Format("{0:00}", GameData.SaveData.Stage + 1);
            seqBodyData = seqStage.SequenceData.SequenceGroupList[2];
            DrawStageTitle(seqBodyData, text);

            // Draws the text string for the stage name.
            // 
            // ステージ名の文字列を描画します。
            text = movie.Info.Name;
            seqBodyData = seqStage.SequenceData.SequenceGroupList[3];
            DrawStageTitle(seqBodyData, text);

            Batch.End();
        }


        /// <summary>
        /// Draws the number of stages and the title.
        /// 
        /// ステージ数とタイトルの描画処理を行います。
        /// </summary>
        private void DrawStageTitle(SequenceGroupData sequenceGroup, string text)
        {
            SequenceObjectData sequenceObject = sequenceGroup.CurrentObjectList;

            // Processing is not performed if data cannot be obtained.
            // 
            // データが取得できなかった場合は処理を行いません。
            if (sequenceObject == null)
                return;

            List<PatternObjectData> list = sequenceObject.PatternObjectList;
            foreach (PatternObjectData patternObject in list)
            {
                SpriteFont font = LargeFont;
                DrawData putInfoData = patternObject.InterpolationDrawData;
                Color color = putInfoData.Color;
                Point point = putInfoData.Position;
                Vector2 position = new Vector2(point.X, point.Y);

                // Centers the Y coordinate.
                // 
                // Y座標をセンタリングします。
                position.Y -= (font.MeasureString(text).Y * 0.5f);

                // Draws the text string.
                // 
                // 文字列の描画をします。
                Batch.DrawString(font, text, position, color);
            }
        }


        /// <summary>
        /// Performs countdown drawing.
        /// 
        /// カウントダウンの描画処理を行います。
        /// </summary>
        private void DrawCountDown()
        {
            Batch.Begin();
            seqCountDown.Draw(Batch, null);
            Batch.End();
        }


        /// <summary>
        /// Performs drawing processing at completion.
        /// 
        /// 完成時の描画処理を行います。
        /// </summary>
        private void DrawComplete(GameTime gameTime)
        {
            // Processing is not performed while score is displayed. 
            // 
            // スコアが表示中ならば処理をしない
            if (scorePopupList.Count > 0 || score != scoreView)
                return;

            // Draws the movie in full screen.
            // 
            // ムービーをフルスクリーンで描画します。
            if (resolveTexture != null && movieTexture != null)
            {
                Batch.Begin(SpriteBlendMode.None);
                Batch.Draw(movieTexture, GameData.ScreenSizeRect, null, Color.White);
                Batch.End();
            }


            Batch.Begin();
            
            // Draws the cross-fade back buffer.
            // 
            // クロスフェード用のバックバッファを描画します。
            if (resolveTexture != null)
            {
                Batch.Draw(resolveTexture, Vector2.Zero, new Color(completeColor));

                // Draws the Navigate button.
                // 
                // ナビゲートボタンを描画します。
                DrawNavigate(gameTime, false);
            }

            // Performs drawing processing while the completed animation is being played.
            // 
            // コンプリートアニメーションが再生中なら描画処理を行います。
            if (seqComplete.SequenceData.IsPlay)
                seqComplete.Draw(Batch, null);

            Batch.End();
        }


        /// <summary>
        /// Performs time over drawing processing.
        /// 
        /// タイムオーバーの描画処理を行います。
        /// </summary>
        public void DrawTimeOver()
        {
            Batch.Begin();
            {
                seqTimeUp.Draw(Batch, null);
            }
            Batch.End();
        }


        /// <summary>
        /// Draws the Pause screen.
        /// 
        /// ポーズ画面の描画処理を行います。
        /// </summary>
        public void DrawPause(GameTime gameTime)
        {
            Batch.Begin();

            if (seqRefNaviStart.IsPlay)
            {
                seqRefStart.Draw(Batch, null);
                seqRefNaviStart.Draw(Batch, null);
            }
            else
            {
                seqRefLoop.Draw(Batch, null);
                seqRefNaviLoop.Draw(Batch, null);

                // Sets the selection cursor sequence.
                // 
                // 選択カーソルのシーケンスを設定します。
                SequencePlayData seqPlayData;
                seqPlayData = (pauseCursor == PauseCursor.ReturnToGame) ?
                    seqRefReturnToGameOn : seqRefGoToTitleOn;

                // Draws the sequence.
                // 
                // シーケンスを描画します。
                seqPlayData.Draw(Batch, null);

                // Draws the Navigate button.
                // 
                // ナビゲートボタンの描画を行います。
                DrawNavigate(gameTime, false);
            }

            Batch.End();
        }


        /// <summary>
        /// Draws the Preview screen.
        /// 
        /// プレビュー画面の描画処理を行います。
        /// </summary>
        public void DrawThumbnail()
        {
            // Processing is not performed if there is no movie texture.
            // 
            // ムービーのテクスチャが無い場合は処理を行いません。
            if (movieTexture == null)
                return;

            // Sets the movie rectangle.
            // 
            // ムービーの矩形を設定します。
            Vector4 movie = new Vector4();
            movie.X = movieRect.X;
            movie.Y = movieRect.Y;
            movie.Z = movieRect.Width;
            movie.W = movieRect.Height;

            // Sets the thumbnail rectangle.
            // 
            // サムネイルの矩形を設定します。
            Vector4 thumb = new Vector4();
            thumb.X = movieThumbRect.X;
            thumb.Y = movieThumbRect.Y;
            thumb.Z = movieThumbRect.Width;
            thumb.W = movieThumbRect.Height;

            // Calculates the size.
            // 
            // サイズを計算します。
            Vector4 size = Vector4.Lerp(thumb, movie, thumbZoomRate);
            Rectangle rect = new Rectangle();
            rect.X = (int)size.X;
            rect.Y = (int)size.Y;
            rect.Width = (int)size.Z;
            rect.Height = (int)size.W;

            // Draws the thumbnail.
            // 
            // サムネイルを描画します。
            Batch.Begin(SpriteBlendMode.None);
            Batch.Draw(movieTexture, rect, Color.White);
            Batch.End();
        }


        /// <summary>
        /// Draws the help icon.
        /// 
        /// ヘルプアイコンの描画処理を行います。
        /// </summary>
        public void DrawHelpIcon(SpriteBatch batch)
        {
            // hints are only used in Normal mode
            if (setting.Mode != StageSetting.ModeList.Normal)
            {
                return;
            }

            SequenceBankData sequenceBank = seqHelpIcon.SequenceData;
            SequenceGroupData sequenceGroup = sequenceBank.SequenceGroupList[0];
            SequenceObjectData sequenceObject = sequenceGroup.SequenceObjectList[0];
            PatternObjectData patternObject = sequenceObject.PatternObjectList[0];
            Rectangle rect = patternObject.Rect;

            DrawData info = new DrawData();

            // Shifts the drawing position by the number of remaining help icons.
            // 
            // ヘルプアイコンの残りの数だけ、位置をずらして描画します。
            for (int i = 0; i < helpItemCount; i++)
            {
                Vector2 position = positions[PositionList.HelpIcon];
                position.X += (rect.Width * i);
                info.Position = new Point((int)position.X, (int)position.Y);
                seqHelpIcon.Draw(batch, info);
            }
        }


        /// <summary>
        /// Draws the score.
        /// 
        /// スコアの描画処理を行います。
        /// </summary>
        public void DrawTextScore(SpriteBatch batch, SpriteFont font)
        {
            // The score is drawn only in Normal Mode.
            // 
            // Normalモード以外はスコアの描画を行いません。
            if (setting.Mode != StageSetting.ModeList.Normal)
                return;


            string text = string.Format("{0:00000}", scoreView);
            Vector2 position = positions[PositionList.Score];
            batch.DrawString(font, text, position, Color.White);
        }


        /// <summary>
        /// Draws the remaining time and elapsed time.
        /// 
        /// 残り時間・経過時間の描画処理を行います。
        /// </summary>
        public void DrawTextTime(SpriteBatch batch, SpriteFont font)
        {
            string text;
            if (setting.Mode == StageSetting.ModeList.Normal)
            {
                string time = (setting.TimeLimit - playTime).ToString(); 
                text = time.Substring(0, 8);
            }
            else
            {
                text = playTime.ToString().Substring(0, 8);
            }
            Vector2 position = positions[PositionList.Time];
            batch.DrawString(font, text, position, Color.White);
        }


        /// <summary>
        /// Draws the number of remaining panels.
        /// 
        /// パネルの残り枚数の描画処理を行います。
        /// </summary>
        public void DrawTextRest(SpriteBatch batch, SpriteFont font)
        {
            int rest = panelManager.PanelRestCount(setting);
            string value = string.Format("{0:00}", rest);
            Vector2 position = positions[PositionList.Rest];
            batch.DrawString(font, value, position, Color.White);
        }


        /// <summary>
        /// Draws the area with movie panels excluded.
        /// 
        /// ムービーのパネルを除いたエリアの描画処理を行います。
        /// </summary>
        private void DrawOutMovie()
        {
            // Drawing processing is not performed if there is no movie texture.
            // 
            // ムービーのテクスチャが無ければ描画処理を行いません。
            if (movieTexture == null)
                return;

            // Obtains the rectangle list.
            // 
            // 矩形のリストを取得します。
            Vector2[] positions = movieFramePosition;
            Rectangle[] rectangles = movieFrameSrc;

            Batch.Begin(SpriteBlendMode.None);
            for (int i = 0; i < rectangles.Length; i++)
            {
                Batch.Draw(movieTexture, positions[i], rectangles[i], Color.Silver);
            }
            Batch.End();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Movie sizes that do not fit in the panels are returned as rectangles.
        /// 
        /// パネルに収まらなかったムービーのサイズを矩形で返します。
        /// </summary>
        private Vector2[] GetMovieFramePosition()
        {
            Vector2 offset = positions[PositionList.Movie];
            Vector2 top;
            Vector2 bottom;
            Vector2 left;
            Vector2 right;

            float movieHeight = movieRect.Height;
            float panelHeight = panelManager.PanelArea.W;

            float panelX = panelManager.PanelArea.X;
            float panelWidth = panelManager.PanelArea.Z;

            top = offset;

            bottom = offset;
            bottom.Y += (movieHeight - (movieHeight - panelHeight) * 0.5f);
            
            left = offset;

            right = offset;
            right.X += panelX + panelWidth;

            return new Vector2[] { top, bottom, left, right };
        }


        /// <summary>
        /// Obtains rectangles for non-panel sections.
        /// 
        /// パネル以外の部分の矩形を取得します。
        /// </summary>
        private Rectangle[] GetMovieFrameSource()
        {
            Rectangle top;
            Rectangle bottom;
            Rectangle left;
            Rectangle right;

            float panelX = panelManager.PanelArea.X;
            float panelWidth = panelManager.PanelArea.Z;
            float panelHeight = panelManager.PanelArea.W;

            top = new Rectangle();
            top.Width = movieRect.Width;
            top.Height = (int)((movieRect.Height - panelHeight) * 0.5f);

            bottom = new Rectangle();
            bottom.Y = movieRect.Height - (int)((movieRect.Height - panelHeight) * 0.5f);
            bottom.Width = movieRect.Width;
            bottom.Height = (int)((movieRect.Height - panelHeight) * 0.5f);

            left = new Rectangle();
            left.Width = (int)(panelX);
            left.Height = movieRect.Height;

            right = new Rectangle();
            right.X = (int)(panelX + panelWidth);
            right.Width = (int)((movieRect.Width - panelWidth) * 0.5f);
            right.Height = movieRect.Height;

            return new Rectangle[] { top, bottom, left, right };
        }


        /// <summary>
        /// Adds up the completed single score.
        /// 
        /// シングル揃いのスコアを加算します。
        /// </summary>
        public void AddSingleScore(long score)
        {
            result.SingleScore += score;
        }


        /// <summary>
        /// Adds up the completed double score.
        /// 
        /// ダブル揃いのスコアを加算します。
        /// </summary>
        public void AddDoubleScore(long score)
        {
            result.DoubleScore += score;
        }


        /// <summary>
        /// Performs processing if panels are completed in single.
        /// 
        /// シングルで揃った場合の処理を行う。
        /// </summary>
        public void PanelCompleteSingle(List<PanelData> list)
        {
            // Processing is performed only in Change. 
            // 
            // Change以外では処理を行いません。
            if (setting.Style != StageSetting.StyleList.Change)
                return;

            // Processing is only performed in Normal Mode. 
            // 
            // ノーマルモード以外では処理を行いません。
            if (setting.Mode != StageSetting.ModeList.Normal)
                return;

            // Adds up the score.
            // 
            // スコアを加算します。
            AddSingleScore(ScoreSingle);

            // Creates the score sprite.
            // 
            // スコアのスプライトを作成します。
            foreach (PanelData panel in list)
            {
                // Creates the sprite.
                // 
                // スプライトを作成します。
                SpriteScorePopup sprite = CreateSpriteScore(panel, ScoreSingle);

                // Adds the sprite to the array.
                // 
                // 配列に追加します。
                scorePopupList.Add(sprite);
            }
        }


        /// <summary>
        /// Performs processing if panels are completed in double.
        /// 
        /// パネルがダブルで揃った時の処理を行います。
        /// </summary>
        public void PanelCompleteDouble(List<PanelData> list)
        {
            // Processing is performed only in Change.
            // 
            // Change以外では処理を行いません。
            if (setting.Style != StageSetting.StyleList.Change)
                return;

            // Processing is performed only in Normal Mode.
            // 
            // ノーマルモード以外では処理を行いません。
            if (setting.Mode != StageSetting.ModeList.Normal)
                return;

            // Adds up the score.
            // 
            // スコアを加算します。
            AddDoubleScore(ScoreDouble * 2);

            // Creates the score sprite.
            // 
            // スコアのスプライトを作成します。
            foreach (PanelData panel in list)
            {
                // Creates the sprite.
                // 
                // スプライトを作成します。
                SpriteScorePopup sprite = CreateSpriteScore(panel, ScoreDouble);

                // Adds the sprite to the array.
                // 
                // 配列に追加します。
                scorePopupList.Add(sprite);
            }
        }


        /// <summary>
        /// Creates and returns the score sprite.
        /// 
        /// スコアのスプライトを作成して返します。
        /// </summary>
        private SpriteScorePopup CreateSpriteScore(PanelData panel, int score)
        {
            SpriteScorePopup sprite = new SpriteScorePopup(Game);
            sprite.Initialize();

            // Sets the score initial position. 
            // 
            // スコアの初期位置を設定します。
            sprite.Position = panel.Center + panelManager.DrawOffset;
            sprite.DefaultPosition = sprite.Position;

            // Sets the score target position.
            // 
            // スコアの移動先を設定します。
            Vector2 target;
            target = positions[PositionList.Score] + new Vector2(128, 32);
            sprite.TargetPosition = target;

            // Sets the score.
            // 
            // スコアの設定をします。
            sprite.Score = score;

            return sprite;
        }


        /// <summary>
        /// Creates and returns the component for the specified style.
        /// 
        /// 指定したスタイルのコンポーネントを作成して返します。
        /// </summary>
        private StyleBase CreateStyleComponent()
        {
            return CreateStyleComponent(setting, cursor, panelManager);
        }


        /// <summary>
        /// Creates and returns the component for the specified style.
        /// 
        /// 指定したスタイルのコンポーネントを作成して返します。
        /// </summary>
        private StyleBase CreateStyleComponent(
            StageSetting setting, Point cursor, PanelManager manager)
        {
            StyleBase component = null;

            if (setting.Style == StageSetting.StyleList.Change)
            {
                component = new ChangeComponent(Game, setting, cursor, manager);
            }
            else if (setting.Style == StageSetting.StyleList.Revolve)
            {
                component = new RevolveComponent(Game, setting, cursor, manager);
            }
            else if (setting.Style == StageSetting.StyleList.Slide)
            {
                component = new SlideComponent(Game, setting, cursor, manager);
            }

            return component;
        }

        
        /// <summary>
        /// Creates and returns the movie component.
        /// 
        /// ムービーコンポーネントを作成して返します。
        /// </summary>
        private PuzzleAnimation CreateMovie(AnimationInfo info)
        {
            return PuzzleAnimation.CreateAnimationComponent(Game, info);
        }


        /// <summary>
        /// Checks the pause.
        /// 
        /// ポーズのチェックをします。
        /// </summary>
        private bool CheckPause()
        {
            VirtualPadState virtualPad =
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            // Processing is performed only when start has been pressed. 
            // 
            // スタートが押されていない場合は処理を行いません。
            if (!buttons.Start[VirtualKeyState.Push])
                return false;

            // Start has been pressed; Pause flag is set.
            // 
            // スタートが押されていたので、ポーズのフラグを設定します。
            isPause = true;

            // Initializes the pause screen cursor.
            // 
            // ポーズ画面のカーソルの初期設定をします。
            pauseCursor = PauseCursor.ReturnToGame;

            // Sets the panel selection in the background so that it will not move.
            //
            // 裏でパネル選択が動かないように設定します。
            style.SelectEnabled = false;

            // Rests the sequence displayed on the pause screen.
            // 
            // ポーズ画面に表示するシーケンスをリセットします。
            seqRefStart.Replay();
            seqRefLoop.Replay();
            seqRefNaviStart.Replay();
            seqRefNaviLoop.Replay();
            seqRefReturnToGameOn.Replay();
            seqRefGoToTitleOn.Replay();

            // Sets the Navigate button.
            // 
            // ナビゲートボタンの設定を行います。
            Navigate.Clear();
            Navigate.Add(new NavigateData(AppSettings("B_Cancel"), false));
            Navigate.Add(new NavigateData(AppSettings("A_Ok"), true));

            return true;
        }


        /// <summary>
        /// Checks the time limit.
        /// 
        /// タイムリミットのチェックを行います。
        /// </summary>
        private bool IsTimeLimit()
        {
            // The time limit judgment is performed only in Normal Mode.
            // 
            // ノーマルモード以外ではタイムリミットの判定を行いません。
            if (setting.Mode != StageSetting.ModeList.Normal)
                return false;

            return (playTime >= setting.TimeLimit);
        }


        /// <summary>
        /// Uses the help item.
        /// 
        /// ヘルプアイテムを使用します。
        /// </summary>
        private bool UseHelpItem()
        {
            // Processing is not performed if no items are remaining.
            // 
            // アイテムの残り個数が無ければ処理をしません。
            if (helpItemCount <= 0)
                return false;

            // Processing is not performed while items are in use.
            // 
            // アイテム使用中なら処理をしません。
            if (helpItemTime > TimeSpan.Zero)
                return false;

            // Sets the item usage time.
            // 
            // アイテムの使用時間を設定します。
            helpItemTime = HelpItemTime;

            // In Normal Mode, reduces the usage repetitions.
            // 
            // ノーマルモードなら使用回数を減らします。
            if (setting.Mode == StageSetting.ModeList.Normal)
                helpItemCount--;

            return true;
        }
        #endregion
    }
}


