#region File Description
//-----------------------------------------------------------------------------
// MovipaGame.cs
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
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Movipa.Components;
using Movipa.Components.Input;
using Movipa.Util;
using MovipaLibrary;
#endregion

namespace Movipa
{
    /// <summary>
    /// This class adds the necessary initial components and stores the number of them.
    /// Scene components are added with each game scene. When all components
    /// are finished, if the component count is the same as the initial number,
    /// the game ends. 
    /// 
    /// ゲームの処理を行うメインのクラスです。
    /// このクラスでは、最初に必要なコンポーネントを追加し、その数を保持しておきます。
    /// ゲームの各シーンはシーンコンポーネントが追加され、処理を行いますが、
    /// そのシーンコンポーネントが全て終了し、コンポーネントの数が初期値と同じ数に
    /// なったらゲームを終了します。
    /// </summary>
    public class MovipaGame : Game
    {
        #region Fields
        GraphicsDeviceManager graphics;

        // Default component count 
        // 
        // 標準で使用されるコンポーネント数
        private int defaultComponentCount = -1;

        private SpriteBatch batch;
        public SpriteBatch Batch
        {
            get { return batch; }
        }

        private SpriteFont mediumFont;
        public SpriteFont MediumFont
        {
            get { return mediumFont; }
        }

        private SpriteFont largeFont;
        public SpriteFont LargeFont
        {
            get { return largeFont; }
        }

        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance. 
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public MovipaGame()
        {
            this.IsFixedTimeStep = false;
            graphics = new GraphicsDeviceManager(this);

            // Sets the resolution.
            // 
            // 解像度の設定をします。
            graphics.PreferredBackBufferWidth = GameData.ScreenWidth;
            graphics.PreferredBackBufferHeight = GameData.ScreenHeight;
            
            // set the minimum required shader model
            // -- the skinned shader requires shader model 2.0
            graphics.MinimumVertexShaderProfile = ShaderProfile.VS_2_0;
            graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;
            graphics.PreparingDeviceSettings += 
                new EventHandler<PreparingDeviceSettingsEventArgs>(
                graphics_PreparingDeviceSettings);

            // Sets the default directory for ContentManager.
            // 
            // ContentManagerのルートディレクトリの設定をします。
            Content.RootDirectory = "Content";
        }

        void graphics_PreparingDeviceSettings(object sender, 
            PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = 
                RenderTargetUsage.PreserveContents;
        }


        /// <summary>
        /// Performs initialization processing.
        /// 
        /// 初期化を行います。
        /// </summary>
        protected override void Initialize()
        {
            // Registers the initial component settings.
            // 
            // コンポーネントの初期設定を行います。
            InitializeComponent();

            // Registers the start scene settings.
            // 
            // 開始シーンの設定を行います。
            GameData.SceneQueue.Enqueue(new Movipa.Components.Scene.Logo(this));
            GameData.SceneQueue.Enqueue(new Movipa.Components.Scene.Title(this));

            base.Initialize();
        }


        /// <summary>
        /// Initializes the component.
        /// 
        /// コンポーネントの初期化を行います。
        /// </summary>
        private void InitializeComponent()
        {
            // Adds the gamer service component.
            // 
            // ゲーマーサービスコンポーネントを追加します。
            Components.Add(new GamerServicesComponent(this));

            // Adds the input component.
            // 
            // 入力コンポーネントを追加します。
            GameData.Input = new InputComponent(this);
            Components.Add(GameData.Input);

            // Adds the sound component.
            // 
            // サウンドコンポーネントを追加します。
            GameData.Sound = new SoundComponent(this);
            Components.Add(GameData.Sound);

            // Adds the storage selection component.
            // 
            // ストレージ選択コンポーネントを追加します。
            GameData.Storage = new StorageComponent(this);
            Components.Add(GameData.Storage);

            // Adds the fade component.
            // 
            // フェードコンポーネントを追加します。
            GameData.FadeSeqComponent = new FadeSeqComponent(this);
            GameData.FadeSeqComponent.DrawOrder = 100;
            Components.Add(GameData.FadeSeqComponent);

#if DEBUG
            // Adds the safe area display component (debug only).
            // 
            // セーフエリア表示コンポーネントを追加します。（デバッグのみ）
            SafeAreaComponent safeAreaComponent = new SafeAreaComponent(this);
            //safeAreaComponent.DrawOrder = 1000;
            Components.Add(safeAreaComponent);
#endif

            // Obtains the component count.
            // 
            // コンポーネント数を取得します。
            defaultComponentCount = Components.Count;
        }


        /// <summary>
        /// Loads the content.
        /// 
        /// コンテントの読み込みを行います。
        /// </summary>
        protected override void LoadContent()
        {
            batch = new SpriteBatch(GraphicsDevice);
            mediumFont = Content.Load<SpriteFont>("Textures/Font/MediumGameFont");
            largeFont = Content.Load<SpriteFont>("Textures/Font/LargeGameFont");

            GameData.LoadContent(Content);

            base.LoadContent();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Performs update processing.
        /// 
        /// 更新処理を行います。
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Terminates if all scenes are completed. 
            // 
            // ゲームパッドのBackを押すか、キーボードのESCキーを押すか、
            // 全てのシーンが終了したら終了します。
            if ((GameData.SceneQueue.Count == 0 &&
                Components.Count == defaultComponentCount))
            {
                this.Exit();
            }


            // Performs transition if the scene is completed and 
            // the next scene remains in the queue.
            // 
            // シーンが終了していてキューに次のシーンが残っていれば遷移します。
            if (Components.Count == defaultComponentCount &&
                GameData.SceneQueue.Count > 0)
            {
                // Releases memory for scene switching.
                // 
                // シーン切り替え時にメモリの開放を行います。
                //System.GC.Collect();

                // Retrieves the scene from the queue and adds it to the component.
                //
                // キューからシーンを取り出し、コンポーネントに追加します。
                GameComponent nextScene = GameData.SceneQueue.Dequeue();
                Components.Add(nextScene);
            }

            base.Update(gameTime);
        }
        #endregion


        #region Static Entry Point


        /// <summary>
        /// Generates and executes the MovipaGame class.
        /// 
        /// このアプリケーションのプログラム開始位置です。
        /// Mainクラスを生成し、実行します。
        /// </summary>
        static void Main()
        {
            using (MovipaGame game = new MovipaGame())
            {
                game.Run();
            }
        }


        #endregion
    }
}
