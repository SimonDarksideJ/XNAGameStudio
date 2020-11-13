#region File Description
//-----------------------------------------------------------------------------
// ThumbnailLoader.cs
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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Movipa.Components.Scene.Menu
{
    /// <summary>
    /// Loads movie thumbnails asynchronously.
    /// The load list should be specified in the constructor.
    /// This class is designed to execute the Initialize method
    /// in a thread by inheriting InitializeThread and 
    /// invoking the associated Run method. 
    /// 
    /// ムービーのサムネイルを非同期で読み込みます。
    /// 読み込むリストはコンストラクタに指定して下さい。
    /// このクラスはInitializeThreadを継承し、InitializeThreadの
    /// Runメソッドを呼び出すことで、Initializeメソッドをスレッドで
    /// 実行するようになっています。
    /// </summary>
    public class ThumbnailLoader : InitializeThread
    {
        #region Fields
        // Asset list to load
        // 
        // 読み込むアセットリスト
        private string[] list;

        // Loaded texture list
        // 
        // 読み込まれたテクスチャリスト
        private List<Texture2D> textures;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the texture list.
        /// 
        /// テクスチャのリストを取得します。
        /// </summary>
        public List<Texture2D> Textures
        {
            get
            {
                if (!Initialized)
                    return null;

                return textures;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public ThumbnailLoader(Game game, int cpu, string[] assetList)
            : base(game, cpu)
        {
            list = assetList;
            textures = new List<Texture2D>();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Loads the asset list.
        ///
        /// アセットリストの読み込み処理を行います。
        /// </summary>
        protected override void Initialize()
        {
            // Sets the CPU core.
            // 
            // CPUコアの設定をします。
            SetCpuCore();

            // Loads all assets in the list.
            // 
            // リストにあるアセットを全て読み込みます。
            foreach (string asset in list)
            {
                Texture2D texture = Game.Content.Load<Texture2D>(asset);
                textures.Add(texture);
            }

            base.Initialize();
        }
        #endregion
    }
}
