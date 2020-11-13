#region File Description
//-----------------------------------------------------------------------------
// Title.cs
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
using Microsoft.Xna.Framework.Input;

using Movipa.Components.Animation;
using Movipa.Components.Input;
using Movipa.Util;
using MovipaLibrary;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene
{
    /// <summary>
    /// Scene component that displays the title.
    /// Provides fade control and item selection.
    /// Items obtain the position from Layout data and draw the text string.
    /// The texture that draws the movie is specified 
    /// in the BasicEffect Texture properties to draw the film model texture. 
    /// 
    /// 
    /// タイトルを表示するシーンコンポーネントです。
    /// フェードの制御と、項目の選択処理を行っています。
    /// 項目はLayoutのデータから位置を取得し、文字列を描画しています。
    /// フィルムモデルのテクスチャにはBasicEffectのTextureプロパティに
    /// ムービーを描画したテクスチャを指定して描画しています。
    /// </summary>
    public class Title : SceneComponent
    {
        #region Private Types
        /// <summary>
        /// Item type specified with cursor 
        /// 
        /// カーソルが指定している項目の種類
        /// </summary>
        private enum CursorType
        {
            /// <summary>
            /// Game start
            /// 
            /// ゲーム開始
            /// </summary>
            Start,

            /// <summary>
            /// Game end
            /// 
            /// ゲーム終了
            /// </summary>
            Quit,

            /// For count
            /// 
            // カウント用
            Count,
        }
        #endregion

        #region Fields
        // Position and size of animation mounted on film
        // 
        // フィルムに貼り付けるアニメーションの位置とサイズ
        private readonly Rectangle animationTexturePosition;

        // Components
        private FadeSeqComponent fade;

        // Film model
        // 
        // フィルムのモデル
        private BasicModelData film;
        private Vector3 filmCameraPosition;
        private Vector3 filmCameraLookAt;
        private RenderTarget2D filmRenderTarget;
        private RenderTarget2D filmModelRenderTarget;

        // Title skinned animation model
        // 
        // タイトルのスキンアニメーションモデル
        private SkinnedModelData titleModel;
        private Vector3 titleModelCameraPosition;
        private Vector3 titleModelCameraLookAt;
        private Vector3 titleModelLightPosition;
        private Plane titleModelLightPlane;
        private RenderTarget2D titleModelRenderTarget;
        private RenderTarget2D shadowRenderTarget;
        private Color shadowColor;

        // Animation
        // 
        // アニメーション
        private PuzzleAnimation animation;

        // Cursor position
        // 
        // カーソル位置
        private CursorType cursor;

        // Coordinates defined in Layout
        // 
        // Layoutで定義された座標
        private Dictionary<string, Vector2> positions;

        // Start text string
        // 
        // 開始の文字列
        private string stringStart;

        // End text string
        // 
        // 終了の文字列
        private string stringQuit;

        // Menu draw font
        // 
        // メニューの描画フォント
        private SpriteFont menuFont;

        // Developer text string
        // 
        // 開発会社の文字列
        private string stringDeveloper;

        // Developer draw font
        // 
        // 開発会社の描画フォント
        private SpriteFont developerFont;

        // Background texture
        // 
        // 背景のテクスチャ
        private Texture2D wallpaperTexture;

        // Film texture
        // 
        // フィルムのテクスチャ
        private Texture2D filmTexture;
        
        // BackgroundMusic Cue
        // 
        // BackgroundMusicのキュー
        private Cue bgm;

        // Layout
        private SceneData sceneData = null;
        private SequencePlayData seqSubTitle = null;
        private SequencePlayData seqStart = null;
        private SequencePlayData seqQuit = null;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public Title(Game game)
            : base(game)
        {
            // Sets the size of the animation to be drawn in the film texture.
            // 
            // フィルムのテクスチャに描画するアニメーションのサイズを設定します。
            animationTexturePosition = new Rectangle(142, 40, 640, 360);
        }


        /// <summary>
        /// Performs initialization processing.
        /// 
        /// 初期化処理を行います。
        /// </summary>
        public override void Initialize()
        {
            // Sets the initial cursor position.
            // 
            // カーソルの初期位置を設定します。
            cursor = CursorType.Start;

            // Sets the film model camera.
            // 
            // フィルムのモデルのカメラを設定します。
            filmCameraPosition = new Vector3(0.0f, 0.0f, 150.0f);
            filmCameraLookAt = Vector3.Zero;

            // Sets the title logo camera.
            // 
            // タイトルロゴのカメラを設定します。
            titleModelCameraPosition = new Vector3(0, 50, -300f);
            titleModelCameraLookAt = new Vector3(0, -20, 0);

            // Sets the position of the light that creates the title logo shadow.
            // 
            // タイトルロゴの影を作るライトの位置を設定します。
            titleModelLightPosition = new Vector3(50, 40, -30);
            titleModelLightPlane = new Plane(Vector3.Up, 0);

            // Obtains the fade component instance.
            // 
            // フェードコンポーネントのインスタンスを取得します。
            fade = GameData.FadeSeqComponent;

            // Sets the fade-in.
            // 
            // フェードインの設定を行います。
            fade.Start(FadeType.Gonzales, FadeMode.FadeIn);

            // Plays the BackgroundMusic and obtains the Cue.
            // 
            // BackgroundMusicを再生し、Cueを取得します。
            bgm = GameData.Sound.PlayBackgroundMusic(Sounds.TitleBackgroundMusic);

            base.Initialize();
        }


        /// <summary>
        /// Initializes the Navigate.
        /// 
        /// ナビゲートの初期化をします。
        /// </summary>
        protected override void InitializeNavigate()
        {
            Navigate.Add(new NavigateData(AppSettings("A_Ok"), true));
            Navigate.Add(new NavigateData(AppSettings("B_Cancel"), true));
            base.InitializeNavigate();
        }


        /// <summary>
        /// Loads the content.
        /// 
        /// コンテントの読み込み処理を行います。
        /// </summary>
        protected override void LoadContent()
        {
            // Obtains the font.
            // 
            // フォントを取得します。
            menuFont = LargeFont;
            developerFont = MediumFont;

            // Obtains the menu text string.
            // 
            // メニューの文字列を取得します。
            stringStart = GameData.AppSettings["TitleStart"];
            stringQuit = GameData.AppSettings["TitleQuit"];
            stringDeveloper = GameData.AppSettings["DeveloperName"];

            // Loads the texture.
            // 
            // テクスチャを読み込みます。
            string asset;
            asset = "Textures/Wallpaper/Wallpaper_002";
            wallpaperTexture = Content.Load<Texture2D>(asset);

            asset = "Textures/Title/Film";
            filmTexture = Content.Load<Texture2D>(asset);

            // Loads and initializes the model data.
            // 
            // モデルデータの読み込みと初期化を行います。
            InitializeModels();

            // Loads and initializes the sequence.
            // 
            // シーケンスの読み込みと初期化を行います。
            InitializeSequence();

            // Loads the movie animation.
            // 
            // ムービーアニメーションの読み込みを行います。
            InitializeAnimation();

            // Obtains the positions from the sequence.
            // 
             // シーケンスから配置を取得します。
            InitializePositions(sceneData);

            // Creates the render target.
            // 
            // レンダーターゲットの作成を行います。
            InitializeRenderTarget();

            base.LoadContent();
        }


        /// <summary>
        /// Loads and initializes the model data.
        /// 
        /// モデルデータの読み込みと初期化を行います。
        /// </summary>
        private void InitializeModels()
        {
            // Loads the film model.
            // 
            // フィルムのモデルを読み込みます。
            film = new BasicModelData(Content.Load<Model>("Models/film"));
            film.Rotate = new Vector3(0.0f, -0.1765043f, -0.4786051f);
            film.FogEnabled = true;
            film.FogColor = Vector3.Zero;
            film.FogStart = 10;
            film.FogEnd = 500;

            // Loads the title model.
            // 
            // タイトルのモデルを読み込みます。
            titleModel = new SkinnedModelData(Content.Load<Model>(GetTitleAsset()), 
                "Take 001");
            titleModel.Rotate = new Vector3(0, MathHelper.ToRadians(180), 0);
            shadowColor = new Color(0, 0, 0, 192);
        }


        /// <summary>
        /// Loads and initializes the sequence.
        /// 
        /// シーケンスの読み込みと初期化を行います。
        /// </summary>
        private void InitializeSequence()
        {
            // Loads the Layout data.
            // 
            // Layoutのデータを読み込みます。
            sceneData = Content.Load<SceneData>("Layout/title/Title_Scene");

            // Creates the sequence.
            // 
            // シーケンスを作成します。
            seqStart = sceneData.CreatePlaySeqData("Start");
            seqQuit = sceneData.CreatePlaySeqData("Quit");
            seqSubTitle = sceneData.CreatePlaySeqData("SubTitle");
        }


        /// <summary>
        /// Loads the movie animation.
        /// 
        /// ムービーアニメーションの読み込みを行います。
        /// </summary>
        private void InitializeAnimation()
        {
            // Loads animations at random.
            // 
            // ランダムでアニメーションを読み込みます。
            Random rnd = new Random();
            int id = rnd.Next(GameData.MovieList.Count);
            string asset = GameData.MovieList[id];
            AnimationInfo animationInfo = Content.Load<AnimationInfo>(asset);
            animation = PuzzleAnimation.CreateAnimationComponent(Game, animationInfo);
            AddComponent(animation);
        }


        /// <summary>
        /// Obtains the positions from the sequence.
        /// 
        /// シーケンスから配置を取得します。
        /// </summary>
        private void InitializePositions(SceneData sceneData)
        {
            PatternGroupData patternGroup;
            Point point;
            Vector2 position;

            positions = new Dictionary<string, Vector2>();

            // Obtains the position of the Start text string.
            // 
            // Startの文字列の位置を取得します。
            patternGroup = sceneData.PatternGroupDictionary["Title_Start_Normal"];
            point = patternGroup.PatternObjectList[0].Position;
            position = new Vector2(point.X, point.Y);
            positions.Add(stringStart, position);

            // Obtains the position of the Quit text string.
            // 
            // Quitの文字列の位置を取得します。
            patternGroup = sceneData.PatternGroupDictionary["Title_Quit_Normal"];
            point = patternGroup.PatternObjectList[0].Position;
            position = new Vector2(point.X, point.Y);
            positions.Add(stringQuit, position);

            // Obtains the position of the developer.
            // 
            // 開発社名の位置を取得します。
            patternGroup = sceneData.PatternGroupDictionary["Title_Developer"];
            point = patternGroup.PatternObjectList[0].Position;
            position = new Vector2(point.X, point.Y);

            // Aligns with the centered coordinates.
            // 
            // センタリングされた座標に合わせます。
            position -= developerFont.MeasureString(stringDeveloper) * 0.5f;

            positions.Add(stringDeveloper, position);
        }



        /// <summary>
        /// Creates the render target.
        /// 
        /// レンダーターゲットを作成します。
        /// </summary>
        private void InitializeRenderTarget()
        {
            // Obtains the parameters.
            // 
            // パラメータを取得します。
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            SurfaceFormat format = pp.BackBufferFormat;
            MultiSampleType msType = pp.MultiSampleType;
            int msQuality = pp.MultiSampleQuality;

            // Obtains the film size.
            // 
            // フィルムのサイズを取得します。
            Point filmSize = new Point(filmTexture.Width, filmTexture.Height);

            // Obtains the screen size.
            // 
            // スクリーンのサイズを取得します。
            int width = GameData.ScreenWidth;
            int height = GameData.ScreenHeight;

            // Creates the render target.
            // 
            // レンダーターゲットを作成します。
            filmRenderTarget = new RenderTarget2D(GraphicsDevice,
                filmSize.X, filmSize.Y, 1, format, msType, msQuality, 
                RenderTargetUsage.PreserveContents);

            filmModelRenderTarget = new RenderTarget2D(GraphicsDevice,
                width, height, 1, format, msType, msQuality,
                RenderTargetUsage.PreserveContents);

            titleModelRenderTarget = new RenderTarget2D(GraphicsDevice,
                width, height, 1, format, msType, msQuality,
                RenderTargetUsage.PreserveContents);

            shadowRenderTarget = new RenderTarget2D(GraphicsDevice,
                width, height, 1, format, msType, msQuality,
                RenderTargetUsage.PreserveContents);
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
            // Updates the sequence.
            // 
            // シーケンスの更新処理を行います。
            UpdateSequence(gameTime);

            // Updates the model.
            // 
            // モデルの更新処理を行います。
            UpdateModels(gameTime);


            if (fade.FadeMode == FadeMode.FadeIn)
            {
                // Switches fade modes after the fade-in has finished.
                // 
                // フェードインが完了したらフェードのモードを切り替えます。
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
                UpdateMain();
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
            seqStart.Update(gameTime.ElapsedGameTime);
            seqQuit.Update(gameTime.ElapsedGameTime);
            seqSubTitle.Update(gameTime.ElapsedGameTime);
        }


        /// <summary>
        /// Updates the model.
        /// 
        /// モデルの更新処理を行います。
        /// </summary>
        private void UpdateModels(GameTime gameTime)
        {
            // Rotates the film.
            // 
            // フィルムを回転させます。
            Vector3 rotate = film.Rotate;
            rotate.X += MathHelper.ToRadians(0.1f);
            film.Rotate = rotate;

            // Runs the title logo animation.
            // 
            // タイトルロゴのアニメーションをさせます。
            titleModel.AnimationPlayer.Update(
                gameTime.ElapsedGameTime, true, Matrix.Identity);
        }


        /// <summary>
        /// Performs main update processing.
        /// 
        /// メインの更新処理を行います。
        /// </summary>
        private void UpdateMain()
        {
            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;
            VirtualPadDPad leftStick = virtualPad.ThumbSticks.Left;
            VirtualPadDPad dPad = virtualPad.DPad;

            if (InputState.IsPush(buttons.A))
            {
                // Confirms with the A button or the Start button,
                // then executes fade-out.
                // 
                // Aボタンまたはスタートボタンで決定をし、
                // フェードアウトの処理を行います。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);
                fade.Start(FadeType.RotateBox, FadeMode.FadeOut);
            }
            else if (InputState.IsPush(buttons.B, buttons.Back))
            {
                cursor = CursorType.Quit;
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectOkay);
                fade.Start(FadeType.RotateBox, FadeMode.FadeOut);               
            }
            else if (InputState.IsPush(dPad.Up, leftStick.Up, dPad.Down, leftStick.Down))
            {
                // Moves the cursor.
                // Common processing is used since there are only two items:
                // Start and Quit.
                // 
                // カーソルの移動処理を行います。
                // 項目はStartとQuitの2つしかないので、共通の処理を使用します。
                cursor = CursorMove();
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);

                // Replays the sequence.
                // 
                // シーケンスをリプレイさせます。
                seqStart.Replay();
                seqQuit.Replay();
            }

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
            float volume = 1.0f - (fade.Count / 60.0f);
            SoundComponent.SetVolume(bgm, volume);

            if (!fade.IsPlay)
            {
                // Performs release processing after the fade-out has finished.
                // 
                // フェードアウトが完了したら開放処理を行います。
                Dispose();

                // Registers the next scene if the cursor has selected Start.
                // 
                // カーソルがStartを選択していたら次のシーンを登録します。
                if (cursor == CursorType.Start)
                {
                    Game.Components.Add(new Menu.MenuComponent(Game));
                }
            }
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Performs drawing processing.
        /// 
        /// 描画処理を行います。
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Draws the film model to the render target.
            // 
            // フィルムのモデルをレンダーターゲットに描画します。
            if (!DrawFilmModel()) return;

            // Draws the title model to the render target.
            // 
            // タイトルのモデルをレンダーターゲットに描画します。
            if (!DrawTitleModel()) return;

            // Draws the title shadow to the render target.
            // 
            // タイトルの影をレンダーターゲットに描画します。
            if (!DrawTitleModelShadow()) return;

            Batch.Begin();

            // Draws the background.
            // 
            // 背景を描画します。
            Batch.Draw(wallpaperTexture, Vector2.Zero, Color.Silver);

            if ((filmModelRenderTarget != null) && !filmModelRenderTarget.IsDisposed)
            {
                // Draws the film.
                // 
                // フィルムを描画します。
                Batch.Draw(filmModelRenderTarget.GetTexture(), Vector2.Zero,
                    Color.Silver);
            }

            if ((shadowRenderTarget != null) && !shadowRenderTarget.IsDisposed)
            {
                // Draws the title shadow.
                // 
                // タイトルの影を描画します。
                Batch.Draw(shadowRenderTarget.GetTexture(), Vector2.Zero, shadowColor);
            }

            if ((titleModelRenderTarget != null) && !titleModelRenderTarget.IsDisposed)
            {
                // Draws the title.
                // 
                // タイトルを描画します。
                Batch.Draw(titleModelRenderTarget.GetTexture(), Vector2.Zero, 
                    Color.White);
            }

            // Draws the title sequence.
            // 
            // タイトルシーケンスの描画します。
            DrawSequence(gameTime, Batch);

            Batch.End();

            base.Draw(gameTime);
        }


        /// <summary>
        /// Draws the sequence.
        /// 
        /// シーケンスを描画します。
        /// </summary>
        private void DrawSequence(GameTime gameTime, SpriteBatch batch)
        {
            // Draws the sub-title.
            // 
            // サブタイトルを描画します。
            seqSubTitle.Draw(batch, null);

            // Draws the developer name.
            // 
            // 開発社名を描画します。
            Vector2 position = positions[stringDeveloper];
            batch.DrawString(developerFont, stringDeveloper, position, Color.White);

            // Draws the sequence text string.
            // 
            // シーケンスの文字列を描画します。
            DrawSequenceString(batch);

            // Draws the Navigate button.
            // 
            // ナビゲートボタンを描画します。
            DrawNavigate(gameTime, false);
        }


        /// <summary>
        /// Draws the sequence text string.
        /// 
        /// シーケンスの文字列を描画します。
        /// </summary>
        private void DrawSequenceString(SpriteBatch batch)
        {
            Vector2 position;
            bool selected;

            position = positions[stringStart];
            selected = (cursor == CursorType.Start);
            DrawSequenceString(batch, seqStart, stringStart, position, selected);

            position = positions[stringQuit];
            selected = (cursor == CursorType.Quit);
            DrawSequenceString(batch, seqQuit, stringQuit, position, selected);
        }


        /// <summary>
        /// Draws the sequence text string.
        /// 
        /// シーケンスの文字列を描画します。
        /// </summary>
        private void DrawSequenceString(SpriteBatch batch, SequencePlayData sequence, 
            string text, Vector2 position, bool selected)
        {
            Color color;

            if (selected)
            {
                // Obtains the selected status color.
                // 
                // 選択状態の色を取得します。
                color = sequence.SequenceData.GetDrawPatternObjectDrawData(0, 0).Color;
            }
            else
            {
                // Obtains the non-selected status color.
                // 
                // 非選択状態の色を取得します。
                SequenceGroupData sequenceGroup;
                sequenceGroup = sequence.SequenceData.SequenceGroupList[0];
                color = sequenceGroup.SequenceObjectList[0].PatternObjectList[0].Color;
            }

            // Draws the text string. 
            // 
            // 文字列を描画します。
            batch.DrawString(menuFont, text, position, color);
        }


        /// <summary>
        /// Draws the film model.
        /// 
        /// フィルムのモデルを描画します。
        /// </summary>
        private bool DrawFilmModel()
        {
            // Draws the film texture.
            // 
            // フィルムのテクスチャを描画します。
            if (!DrawFilmTexture())
            {
                return false;
            }

            if ((filmRenderTarget == null) || filmRenderTarget.IsDisposed)
            {
                return false;
            }
            // Sets the texture in the film model.
            // 
            // フィルムのモデルにテクスチャを設定します。
            film.Texture = filmRenderTarget.GetTexture();

            // Changes the render target.
            // 
            // 描画先を変更します。
            GraphicsDevice.SetRenderTarget(0, filmModelRenderTarget);

            // Clears the background to transparent.
            // 
            // 背景を透過色でクリアします。
            GraphicsDevice.Clear(Color.TransparentBlack);

            // Enables the depth buffer.
            // 
            // 深度バッファを有効にします。
            GraphicsDevice.RenderState.DepthBufferEnable = true;

            // Draws the film model.
            // 
            // フィルムのモデルを描画します。
            Matrix view = Matrix.CreateLookAt(
                filmCameraPosition, filmCameraLookAt, Vector3.Up);
            film.SetRenderState(GraphicsDevice, SpriteBlendMode.AlphaBlend);
            GraphicsDevice.RenderState.CullMode = CullMode.None;
            film.Draw(view, GameData.Projection);

            // Returns the render target.
            // 
            // 描画先を戻します。
            GraphicsDevice.SetRenderTarget(0, null);

            return true;
        }


        /// <summary>
        /// Draws the film texture.
        /// 
        /// フィルムのテクスチャを描画します。
        /// </summary>
        private bool DrawFilmTexture()
        {
            Texture2D animationTexture = animation.Texture;

            // Performs an error check.
            // 
            // エラーチェックを行います。
            if (animationTexture == null ||
                animationTexture.IsDisposed ||
                filmRenderTarget == null ||
                filmRenderTarget.IsDisposed)
            {
                return false;
            }

            // Changes the render target.
            // 
            // 描画先を変更します。
            GraphicsDevice.SetRenderTarget(0, filmRenderTarget);

            // Clears the background to transparent.
            // 
            // 背景を透過色でクリアします。
            GraphicsDevice.Clear(Color.TransparentBlack);

            // Draws the film frame.
            //
            // フィルムの外枠を描画します。
            Batch.Begin();
            Batch.Draw(filmTexture, Vector2.Zero, Color.White);
            Batch.End();

            // Draws the animation.
            //
            // アニメーションを描画します。
            Batch.Begin(SpriteBlendMode.None);
            Batch.Draw(animationTexture, animationTexturePosition, Color.White);
            Batch.End();

            // Returns the render target.
            // 
            // 描画先を戻します。
            GraphicsDevice.SetRenderTarget(0, null);

            return true;
        }


        /// <summary>
        /// Draws the title model.
        /// 
        /// タイトルのモデルを描画します。
        /// </summary>
        private bool DrawTitleModel()
        {
            if ((titleModelRenderTarget == null) || titleModelRenderTarget.IsDisposed)
            {
                return false;
            }
            // Changes the render target.
            // 
            // 描画先を変更します。
            GraphicsDevice.SetRenderTarget(0, titleModelRenderTarget);

            // Clears the background to transparent.
            // 
            // 背景を透過色でクリアします。
            GraphicsDevice.Clear(Color.TransparentBlack);

            // Enables the depth buffer.
            // 
            // 深度バッファを有効にします。
            GraphicsDevice.RenderState.DepthBufferEnable = true;

            // Draws the title model.
            // 
            // タイトルのモデルを描画します。
            Matrix view = Matrix.CreateLookAt(
                titleModelCameraPosition, titleModelCameraLookAt, Vector3.Up);
            titleModel.SetRenderState(GraphicsDevice, SpriteBlendMode.AlphaBlend);
            titleModel.Draw(Matrix.Identity, view, GameData.Projection,
                true, Vector3.One, Vector3.Zero);

            // Returns the render target.
            // 
            // 描画先を戻します。
            GraphicsDevice.SetRenderTarget(0, null);

            return true;
        }


        /// <summary>
        /// Draws the title model shadow.
        /// 
        /// タイトルのモデルの影を描画します。
        /// </summary>
        private bool DrawTitleModelShadow()
        {
            if ((shadowRenderTarget == null) || shadowRenderTarget.IsDisposed)
            {
                return false;
            }

            // Changes the render target.
            // 
            // 描画先を変更します。
            GraphicsDevice.SetRenderTarget(0, shadowRenderTarget);

            // Clears the background to transparent.
            // 
            // 背景を透過色でクリアします。
            GraphicsDevice.Clear(Color.TransparentBlack);

            // Enables the depth buffer.
            // 
            // 深度バッファを有効にします。
            GraphicsDevice.RenderState.DepthBufferEnable = true;

            // Creates the shadow matrix.
            // 
            // 影のマトリックスを作成します。
            Matrix shadowMatrix = Matrix.CreateShadow(
                titleModelLightPosition, titleModelLightPlane);

            // Draws the shadow model.
            // 
            // 影のモデルを描画します。
            Matrix view = Matrix.CreateLookAt(
                titleModelCameraPosition, titleModelCameraLookAt, Vector3.Up);
            Vector3 rotate = titleModel.Rotate;
            titleModel.Rotate = Vector3.Zero;
            titleModel.SetRenderState(GraphicsDevice, SpriteBlendMode.AlphaBlend);
            titleModel.Draw(shadowMatrix, view, GameData.Projection, false);
            titleModel.Rotate = rotate;

            // Returns the render target.
            // 
            // 描画先を戻します。
            GraphicsDevice.SetRenderTarget(0, null);

            return true;
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Obtains the title asset name at random.
        /// 
        /// タイトルのアセット名をランダムで取得します。
        /// </summary>
        private string GetTitleAsset()
        {
            int i = Random.Next(6) + 1;
            string asset = string.Format("Models/Title/movipa_title_{0:00}", i);
            return asset;
        }


        /// <summary>
        /// Returns the next cursor position.
        /// 
        /// カーソルの次の位置を返します。
        /// </summary>
        private CursorType CursorMove()
        {
            return (CursorType)(((int)cursor + 1) % (int)CursorType.Count);
        }
        #endregion
    }
}


