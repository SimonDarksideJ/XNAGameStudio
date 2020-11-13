#region File Description
//-----------------------------------------------------------------------------
// MenuComponent.cs
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

using Movipa.Util;
using Movipa.Components.Scene.Puzzle;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene.Menu
{
    /// <summary>
    /// Scene component that draws menus.
    /// This class draws the background and renders 
    /// the animation movie.  
    /// It also switches the classes used to process 
    /// items of each menu that has inherited MenuBase.
    ///
    /// メニューの描画を行うシーンコンポーネントです。
    /// このクラスでは、背景の描画と、アニメーションムービーの
    /// レンダリング処理を行っています。
    /// また、MenuBaseを継承した各メニューの項目を処理するクラスの
    /// 切り替え処理も行っています。

    /// </summary>
    public class MenuComponent : SceneComponent
    {
        #region Fields
        /// <summary>
        /// Common data structure
        ///
        /// 共通データ構造体
        /// </summary>
        private MenuData data;

        /// <summary>
        /// Background texture 
        /// 
        /// 背景テクスチャ 
        /// </summary>
        private Texture2D wallpaper;

        /// <summary>
        /// BackgroundMusic cue
        /// 
        /// BackgroundMusicのキュー
        /// </summary>
        private Cue bgm;

        /// <summary>
        /// Menu object currently being executed
        /// 
        /// 現在実行しているメニューオブジェクト
        /// </summary>
        private MenuBase currentMenu;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public MenuComponent(Game game)
            : base(game)
        {
            // Creates common data used in the menu. 
            // 
            // メニューで使用する共通データを作成します。
            data = new MenuData(Game);            
        }


        /// <summary>
        /// Performs initialization processing.
        ///  
        /// 初期化処理を行います。
        /// </summary>
        public override void Initialize()
        {
            // Creates the initial menu.
            // 
            // 最初のメニューを作成します。
            currentMenu = MenuBase.CreateMenu(Game, MenuType.SelectMode, data);

            // Registers the fade-in settings.
            // 
            // フェードインの設定を行います。
            GameData.FadeSeqComponent.Start(FadeType.Normal, FadeMode.FadeIn);

            // Plays the BackgroundMusic and obtains the Cue.
            // 
            // BackgroundMusicを再生し、Cueを取得します。 
            bgm = GameData.Sound.PlayBackgroundMusic(Sounds.SelectBackgroundMusic);

            base.Initialize();
        }


        /// <summary>
        /// Loads the content.
        /// 
        /// コンテントの読み込み処理を行います。
        /// </summary>
        protected override void LoadContent()
        {
            // Loads the background texture.
            // 
            // 背景テクスチャの読み込み
            string asset;
            asset = "Textures/Wallpaper/Wallpaper_006";
            wallpaper = Content.Load<Texture2D>(asset);

            // Loads and sets the sphere model.
            // 
            // 球体モデルの読み込みと設定  
            InitializeModels();

            // Loads the menu scene data.
            // 
            // メニューのシーンデータを読み込む 
            data.sceneData = Content.Load<SceneData>("Layout/menu/menu_Scene");

            // Creates the render data.
            // 
            // レンダーターゲットを作成します。
            InitializeRenderTarget();

            // Initializes the first scene.
            // 
            // 最初のシーンの初期化を実行する  
            currentMenu.RunInitializeThread();

            base.LoadContent();
        }


        /// <summary>
        /// Loads the model data.
        ///
        /// モデルデータの読み込みを行います。
        /// </summary>
        private void InitializeModels()
        {
            // Loads the model.
            // 
            // モデルの読み込み
            Model[] models = new Model[] {
                Content.Load<Model>("Models/sphere01"),
                Content.Load<Model>("Models/sphere02"),
                Content.Load<Model>("Models/sphere11"),
                Content.Load<Model>("Models/sphere12"),
            };

            // Creates the model data.
            // 
            // モデルデータの作成
            data.Spheres = new BasicModelData[][] {
                new BasicModelData[]
                {
                    new BasicModelData(models[0]), new BasicModelData(models[1])
                },
                new BasicModelData[]
                {
                    new BasicModelData(models[2]), new BasicModelData(models[3])
                },
            };

            // Model scale
            // 
            // モデルのスケール
            float[] modelScale = {
                0.9f, 0.88f, data.CursorSphereSize, data.CursorSphereSize };

            // Loading
            // 
            // 読み込み処理   
            for (int i = 0; i < models.Length; i++)
            {
                BasicModelData sphere = data.Spheres[i / 2][i % 2];
                sphere.Scale = modelScale[i];
            }
        }


        /// <summary>
        /// Creates the render target.
        /// 
        /// レンダーターゲットを作成します。
        /// </summary>
        private void InitializeRenderTarget()
        {
            // Creates the split preview RenderTarget. 
            // 
            // 分割数プレビューのRenderTargetを作成
            data.DividePreview = new RenderTarget2D(
                GraphicsDevice,
                GameData.MovieWidth,
                GameData.MovieHeight,
                1,
                SurfaceFormat.Color,
                RenderTargetUsage.PreserveContents);

            // Creates the style animation RenderTarget. 
            // 
            // スタイルアニメーションのRenderTargetを作成
            data.StyleAnimation = new RenderTarget2D(
                GraphicsDevice,
                GameData.StyleWidth,
                GameData.StyleHeight,
                1,
                SurfaceFormat.Color,
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
            // BackgroundMusicの停止
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
            // Updates the model.
            // 
            // モデルの更新処理を行います。
            UpdateModels();

            if (GameData.FadeSeqComponent.FadeMode == FadeMode.FadeIn)
            {
                // Switches fade modes after the fade-in finishes.
                // 
                // フェードインが完了したらフェードのモードを切り替えます。
                if (!GameData.FadeSeqComponent.IsPlay)
                {
                    GameData.FadeSeqComponent.FadeMode = FadeMode.None;
                }
            }
            else if (GameData.FadeSeqComponent.FadeMode == FadeMode.None)
            {
                // Performs main processing.
                // 
                // メインの更新処理を行います。
                UpdateMain(gameTime);
            }
            else if (GameData.FadeSeqComponent.FadeMode == FadeMode.FadeOut)
            {
                // Performs update processing at fade-out.
                // 
                // フェードアウト時の更新処理を行います。
                UpdateFadeOut();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Performs main processing.
        /// 
        /// メインの更新処理を行います。 
        /// </summary>
        private void UpdateMain(GameTime gameTime)
        {
            // Performs an error check.
            // 
            // エラーチェックを行います。
            if (currentMenu == null || !currentMenu.Initialized)
                return;

            // Updates the movie.
            // 
            // ムービーの更新処理を行います。 
            UpdateMovie(gameTime);

            // Updates the menu.
            // 
            // メニューの更新処理を行います。
            MenuBase menu = currentMenu.UpdateMain(gameTime);

            // Switches menus if the next menu has been specified.
            // 
            // 次のメニューが指定されていれば切り替えます。
            if (menu != null)
            {
                // Releases the current menu.
                // 
                // 現在のメニューを開放します。
                currentMenu.Dispose();

                // Sets a new menu.
                // 
                // 新しいメニューを設定します。 
                currentMenu = menu;

                // Executes the initialization thread.
                // 
                // 初期化スレッドを実行します。
                currentMenu.RunInitializeThread();
            }

            base.Update(gameTime);
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

            // Performs release processing after the fade-out finishes.
            // 
            // フェードアウトが終了したら開放処理を行います。  
            if (!GameData.FadeSeqComponent.IsPlay)
            {
                Dispose();
            }
        }



        /// <summary>
        /// Updates the movie.
        /// 
        /// ムービーの更新処理を行います。
        /// </summary>
        private void UpdateMovie(GameTime gameTime)
        {
            // Switches the sequence when the animation movie 
            // loading thread is complete.
            // 
            // アニメーションムービーの読み込みスレッドが完了していたら
            // シーケンスを切り替えます。 
            if (data.movieLoader != null && data.movieLoader.Initialized)
            {
                // Releases the movie if it has already been set.
                // 
                // 既にムービーが設定されていたら快方処理を行います。
                if (data.movie != null)
                {
                    data.movie.Dispose();
                }

                data.movie = data.movieLoader.Movie;
                data.movieLoader = null;
            }

            // Updates the movie if it has been designated.
            // 
            // ムービーが指定されていれば更新処理を行います。
            if (data.movie != null)
            {
                data.movie.Update(gameTime);
            }
        }

        /// <summary>
        /// Updates the model.
        /// 
        /// モデルの更新処理を行います。 
        /// </summary>
        private void UpdateModels()
        {
            Vector3 rotate;

            // Rotates the background sphere.x
            // 
            // 背景の球体を回転させます。
            rotate = data.Spheres[0][0].Rotate;
            rotate.Y += MathHelper.ToRadians(0.1f);
            data.Spheres[0][0].Rotate = rotate;

            rotate = data.Spheres[0][1].Rotate;
            rotate.Y -= MathHelper.ToRadians(0.03f);
            data.Spheres[0][1].Rotate = rotate;

            // Rotates the cursor sphere.
            // 
            // カーソルの球体を回転させます。
            rotate = data.Spheres[1][0].Rotate;
            rotate.Y += data.CursorSphereRotate;
            data.Spheres[1][0].Rotate = rotate;
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
            // Draws the style animation texture.
            // 
            // スタイルアニメーションのテクスチャを描画します。
            DrawStyleAnimation(Batch);

            // Draws the movie animation texture.
            // 
            // ムービーアニメーションのテクスチャを描画します。             
            DrawMovie(gameTime);

            // Uses the movie animation texture 
            // to create the split preview texture.
            // 
            // ムービーアニメーションのテクスチャを使用し、
            // 分割プレビューのテクスチャを作成します。
          
            DrawDividePreview();

            // Draws the background.
            // 
            // 背景を描画します。 
            Batch.Begin();
            Batch.Draw(wallpaper, Vector2.Zero, Color.White);
            Batch.End();

            // Clears the depth buffer and draws the sphere model.
            // 
            // 深度バッファをクリアし、球体のモデルを描画します。
            GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawSpheres(Batch);

            // Performs drawing processing if the menu is set and
            // initialization is complete.
            // 
            // メニューが設定され、初期化が終了していたら描画処理を行います。        
            if (currentMenu != null && currentMenu.Initialized)
            {
                currentMenu.Draw(gameTime, Batch);
            }

            base.Draw(gameTime);
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
            view = Matrix.CreateLookAt(data.CameraPosition, Vector3.Zero, Vector3.Up);

            for (int i = 0; i < 2; i++)
            {
                BasicModelData basicModel = data.Spheres[0][i];
                basicModel.SetRenderState(graphicsDevice, SpriteBlendMode.Additive);
                basicModel.Draw(view, GameData.Projection);
            }
        }


        /// <summary>
        /// Draws the movie animation and sets it in the texture. 
        ///
        /// ムービーのアニメーションを描画し、テクスチャに設定します。
        /// </summary>
        private void DrawMovie(GameTime gameTime)
        {
            // Processing is not performed if the movie has not been set.
            // 
            // ムービーが設定されていない場合は処理を行いません。
            if (data.movie == null)
                return;

            data.movie.Draw(gameTime);
            data.movieTexture = data.movie.Texture;
        }


        /// <summary>
        /// Draws the split preview and sets it in the texture.
        /// 
        /// 分割プレビューを描画してテクスチャに設定します。
        /// </summary>
        private void DrawDividePreview()
        {
            // Performs an error check.
            // 
            // エラーチェックを行います。  
            if (!data.PanelManager.IsInitialized ||
                data.DividePreview == null || 
                data.DividePreview.IsDisposed)
            {
                return;
            }


            // Changes the render target.
            // 
            // 描画先を変更します。
            GraphicsDevice.SetRenderTarget(0, data.DividePreview);

            // Clears the background.
            // 
            // 背景をクリアします。
            GraphicsDevice.Clear(Color.Black);

            // Draws the movie animation.
            // 
            // ムービーアニメーションを描画します。
            if (data.movieTexture != null)
            {
                Batch.Begin();
                Batch.Draw(data.movieTexture, Vector2.Zero, Color.White);
                Batch.End();
            }

            // Draws the split image.
            // 
            // 分割イメージを描画します。
            Matrix projection = GameData.MovieScreenProjection;
            Color fillColor = new Color(0xff, 0x00, 0x00, 0x80);
            for (int x = 0; x < data.PanelManager.PanelCount.X; ++x)
            {
                for (int y = 0; y < data.PanelManager.PanelCount.Y; ++y)
                {
                    // Draws a rectangle at the panel position.
                    // 
                    // パネルの場所に矩形を描画します。 
                    Rectangle rect = data.PanelManager.GetPanel(x, y).RectanglePosition;
                    data.primitiveDraw.FillRect(projection, rect, fillColor);
                    data.primitiveDraw.DrawRect(projection, rect, Color.Yellow);
                }
            }

            // Returns the render target.
            // 
            // 描画先を戻します。
            GraphicsDevice.SetRenderTarget(0, null);

            // Obtains the preview texture.
            // 
            // プレビューのテクスチャを取得します。
            data.divideTexture = 
                (data.DividePreview == null || data.DividePreview.IsDisposed) ? null : 
                data.DividePreview.GetTexture();
        }


        /// <summary>
        /// Draws the style animation and sets it in the texture.
        /// 
        /// スタイルアニメーションを描画してテクスチャに設定します。        
        /// </summary>
        private void DrawStyleAnimation(SpriteBatch batch)
        {
            RenderTarget2D renderTarget = data.StyleAnimation;
            SequencePlayData seqPlayData = data.SeqStyleAnimation;

            // Performs an error check.
            // 
            // エラーチェックをします。
            if (seqPlayData == null || renderTarget == null || renderTarget.IsDisposed)
                return;

            // Changes the render target.
            // 
            // 描画先を変更します。             
            GraphicsDevice.SetRenderTarget(0, renderTarget);

            // Clears the background to a transparent color.
            // 
            // 透過色で背景をクリアします。
            GraphicsDevice.Clear(Color.TransparentBlack);

            // Draws the sequence.
            // 
            // シーケンスを描画します。 
            batch.Begin();
            seqPlayData.Draw(batch, null);
            batch.End();

            // Returns the render target.
            // 
            // 描画先を戻します。 
            GraphicsDevice.SetRenderTarget(0, null);

            // Obtains the style animation texture.
            // 
            // スタイルアニメーションのテクスチャを取得します。 
            data.StyleAnimationTexture =
                (renderTarget == null || renderTarget.IsDisposed) ? null :
                renderTarget.GetTexture();
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases all resources.
        /// 
        /// 全てのリソースを開放します。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (data != null)
                {
                    data.Dispose();
                    data = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}