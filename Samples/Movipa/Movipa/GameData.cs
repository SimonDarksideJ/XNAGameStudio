#region File Description
//-----------------------------------------------------------------------------
// GameData.cs
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
using Microsoft.Xna.Framework.Storage;
using Movipa.Components;
using Movipa.Components.Input;
using Movipa.Util;
using MovipaLibrary;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Movipa
{
    #region Public Types
    /// <summary>
    /// SpriteFont list used in game
    /// 
    /// ゲームで使用するSpriteFontのリスト
    /// </summary>
    public enum FontList
    {
        /// <summary>
        /// Medium font size
        /// The following characters are replaced by special graphics:
        /// { Controller A button
        /// | Controller B button
        /// } Controller X button
        /// ~ Controller Y button
        /// 
        /// 中サイズのフォント
        /// 以下の文字が特殊グラフィックに差し替えられている
        ///  { コントローラのAボタン
        ///  | コントローラのBボタン
        ///  } コントローラのXボタン
        ///  ~ コントローラのYボタン
        /// </summary>
        Medium,
        
        /// <summary>
        /// Large font size
        /// 
        /// 大きいサイズのフォント
        /// </summary>
        Large,
    }
    #endregion

    /// <summary>
    /// Manages global variables used by the game.
    /// Contains screen size constants as well as instances for components and save data.
    /// Static member variables are initialized by a static constructor.
    /// 
    /// ゲームで使用する広域変数を管理します。
    /// 画面サイズの定数や、コンポーネント、セーブデータなどのインスタンスを持っています。
    /// 静的メンバ変数の初期化は、静的コンストラクタで処理を行うようにしています。
    /// </summary>
    public static class GameData
    {
        #region Fields
        #region SizeInfo
        // Screen size
        // 
        // スクリーンサイズ
        public const int ScreenWidth = 1280;
        public const int ScreenHeight = 720;
        public static readonly Rectangle ScreenSizeRect = 
            new Rectangle(0, 0, ScreenWidth, ScreenHeight);
        public static readonly Point ScreenSizePoint = 
            new Point(ScreenWidth, ScreenHeight);
        public static readonly Vector2 ScreenSizeVector2 = 
            new Vector2(ScreenWidth, ScreenHeight);

        // Movie size
        // 
        // ムービーサイズ
        public const int MovieWidth = 1024;
        public const int MovieHeight = 576;
        public static readonly Rectangle MovieSizeRect = 
            new Rectangle(0, 0, MovieWidth, MovieHeight);
        public static readonly Point MovieSizePoint = 
            new Point(MovieWidth, MovieHeight);
        public static readonly Vector2 MovieSizeVector2 = 
            new Vector2(MovieWidth, MovieHeight);

        // Style animation size
        // 
        // スタイルアニメーションサイズ
        public const int StyleWidth = 480;
        public const int StyleHeight = 270;
        public static readonly Rectangle StyleSizeRect = 
            new Rectangle(0, 0, StyleWidth, StyleHeight);
        public static readonly Point StyleSizePoint = 
            new Point(StyleWidth, StyleHeight);
        public static readonly Vector2 StyleSizeVector2 =
            new Vector2(StyleWidth, StyleHeight); 

        // Projection
        // 
        // プロジェクション
        public static readonly Matrix Projection = 
            CreateProjection(ScreenSizeVector2, 10000);
        public static readonly Matrix ScreenProjection = 
            CreateScreenProjection(ScreenSizeVector2);
        public static readonly Matrix MovieScreenProjection =
            CreateScreenProjection(MovieSizeVector2);

        #endregion

        private static Dictionary<string, string> appSettings;
        private static List<StageSetting> stageCollection;
        private static SaveData saveData = null;
        private static List<string> movieList;

        // Components
        private static StorageComponent storageComponent;
        private static InputComponent input = null;
        private static FadeSeqComponent fadeSeqComponent;
        private static SoundComponent soundComponent;
        private static Queue<GameComponent> sceneQueue = new Queue<GameComponent>();
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the storage component.
        /// 
        /// ストレージコンポーネントを取得または設定します。
        /// </summary>
        public static StorageComponent Storage
        {
            get { return storageComponent; }
            set { storageComponent = value; }
        }


        /// <summary>
        /// Obtains or sets the associative array where the text string is stored.
        /// 
        /// 文字列が格納される連想配列を取得または設定します。
        /// </summary>
        public static Dictionary<string, string> AppSettings
        {
            get { return appSettings; }
        }


        /// <summary>
        /// Obtains or sets the Normal Mode storage information.
        /// 
        /// ノーマルモードのステージ情報を取得または設定します。
        /// </summary>
        public static List<StageSetting> StageCollection
        {
            get { return stageCollection; }
        }


        /// <summary>
        /// Obtains or sets the Save Data used in Normal Mode.
        /// 
        /// ノーマルモードで使用されるセーブデータを取得または設定します。
        /// </summary>
        public static SaveData SaveData
        {
            get { return saveData; }
            set { saveData = value; }
        }


        /// <summary>
        /// Obtains or sets the input component.
        /// 
        /// 入力コンポーネントを取得または設定します。
        /// </summary>
        public static InputComponent Input
        {
            get { return input; }
            set { input = value; }
        }


        /// <summary>
        /// Obtains or sets the entry scene queue.
        /// 
        /// エントリーするシーンキューを取得または設定します。
        /// </summary>
        public static Queue<GameComponent> SceneQueue
        {
            get { return sceneQueue; }
        }


        /// <summary>
        /// Obtains or sets the AnimationInfo asset name 
        /// list for movies used in the game.
        ///
        /// ゲーム内で使用するムービーのAnimationInfoのアセット名リストを
        /// 取得または設定します。
        /// </summary>
        public static List<string> MovieList
        {
            get { return movieList; }
        }


        /// <summary>
        /// Obtains or sets the component for fade processing.
        /// 
        /// フェード処理用のコンポーネントを取得または設定します。
        /// </summary>
        public static FadeSeqComponent FadeSeqComponent
        {
            get { return fadeSeqComponent; }
            set { fadeSeqComponent = value; }
        }


        /// <summary>
        /// Obtains or sets the component for fade processing.
        /// 
        /// フェード処理用のコンポーネントを取得または設定します。
        /// </summary>
        public static SoundComponent Sound
        {
            get { return soundComponent; }
            set { soundComponent = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Load content for the static types.
        /// </summary>
        /// <param name="content">The content manager used to load.</param>
        public static void LoadContent(ContentManager content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            // Loads App.config.
            // 
            // App.configを読み込みます。
            appSettings = content.Load<Dictionary<string, string>>("App.config");

            // Loads the stage settings.
            // 
            // ステージ設定を読み込みます。
            stageCollection = content.Load<List<StageSetting>>("StageData");

            // Loads the movie list.
            // 
            // ムービーリストを読み込みます。
            movieList = content.Load<List<string>>("MovieList");
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Creates projection transformation matrix.
        /// 
        /// 射影変換行列を作成します。
        /// </summary>
        private static Matrix CreateProjection(Vector2 size, float far)
        {
            return Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                size.X / size.Y,
                0.1f,
                far);
        }


        /// <summary>
        /// Creates orthogonal projection matrix.
        /// 
        /// 直交射影行列を作成します。
        /// </summary>
        private static Matrix CreateScreenProjection(Vector2 size)
        {
            return Matrix.CreateOrthographicOffCenter(
                0.0f,
                size.X,
                size.Y,
                0.0f,
                0.0f,
                1.0f);
        }
        #endregion
    }
}


