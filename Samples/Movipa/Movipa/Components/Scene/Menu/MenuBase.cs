#region File Description
//-----------------------------------------------------------------------------
// MenuBase.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Movipa.Components.Scene.Menu
{
    /// <summary>
    /// Abstract class for menu processing.
    /// The menu items inherit this class so that the 
    /// necessary processing is written in Update and Draw.
    /// The CreateMenu method for this class should be 
    /// used to create menu instances. 
    /// 
    /// メニューの処理を行う抽象クラスです。
    /// 各メニューの項目は、このクラスを継承し、必要な処理を
    /// UpdateとDrawに記述するようにします。
    /// 各メニューのインスタンスを作成するには、このクラスの
    /// CreateMenuメソッドを使用して下さい。
    /// </summary>
    public abstract class MenuBase : SceneComponent
    {
        #region Fields
        protected bool initialized = false;
        private MenuData data;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the initialization flag.
        /// 
        /// 初期化フラグを取得します。
        /// </summary>
        public bool Initialized
        {
            get { return initialized; }
        }


        /// <summary>
        /// Obtains the menu data.
        /// 
        /// メニューデータを取得します。
        /// </summary>
        public MenuData Data
        {
            get { return data; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        protected MenuBase(Game game, MenuData menuData)
            : base(game)
        {
            data = menuData;
        }


        /// <summary>
        /// Performs initialization processing.
        /// 
        /// 初期化処理を行います。
        /// </summary>
        public override void Initialize()
        {
            initialized = true;
            base.Initialize();
        }


        /// <summary>
        /// Performs asynchronous initialization processing.
        /// 
        /// 非同期で初期化処理を行います。
        /// </summary>
        public void RunInitializeThread()
        {
            // Starts the initialization thread.
            // 
            // 初期化スレッドを開始します。
            Thread thread = new Thread(new ThreadStart(this.Initialize));
            thread.Start();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Abstract method for update processing.
        /// 
        /// 更新処理の抽象メソッドです。
        /// </summary>
        public virtual MenuBase UpdateMain(GameTime gameTime)
        {
            return null;
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Abstract method for drawing processing.
        ///
        /// 描画処理の抽象メソッドです。
        /// </summary>
        public virtual void Draw(GameTime gameTime, SpriteBatch batch)
        {
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Creates menu instances.
        ///
        /// メニューのインスタンスを作成します。
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="menuType">MenuType</param>
        /// <param name="data">MenuData</param>
        /// <returns>Created menu instances</returns>
        ///  
        /// <returns>作成されたメニューインスタンス</returns>
        public static MenuBase CreateMenu(Game game, MenuType menuType, MenuData data)
        {
            switch (menuType)
            {
                case MenuType.SelectMode:
                    // Mode selection
                    // 
                    // モード選択
                    return new SelectMode(game, data);
                case MenuType.SelectFile:
                    // File selection
                    // 
                    // ファイル選択
                    return new SelectFile(game, data);
                case MenuType.SelectStyle:
                    // Style selection
                    // 
                    // スタイル選択
                    return new SelectStyle(game, data);
                case MenuType.SelectMovie:
                    // Movie selection
                    // 
                    // ムービー選択
                    return new SelectMovie(game, data);
                case MenuType.SelectDivide:
                    // Divisions setting
                    // 
                    // 分割数設定
                    return new SelectDivide(game, data);
                case MenuType.Ready:
                    // Confirmation screen
                    // 
                    // 確認画面
                    return new Ready(game, data);
                default:
                    throw new ArgumentException("Invalid MenuType specified");
            }
        }
        #endregion
    }
}
