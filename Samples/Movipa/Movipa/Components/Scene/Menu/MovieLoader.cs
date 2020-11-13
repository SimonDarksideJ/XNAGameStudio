#region File Description
//-----------------------------------------------------------------------------
// MovieLoader.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Movipa.Components.Animation;
using MovipaLibrary;
#endregion

namespace Movipa.Components.Scene.Menu
{
    /// <summary>
    /// Loads and initializes the movie animation 
    /// asynchronously. 
    /// This class is designed to execute the 
    /// Initialize method in a thread by inheriting 
    /// InitializeThread and invoking the associated Run method.
    /// 
    /// ムービーアニメーションの読み込みと初期化を非同期で行います。
    /// このクラスはInitializeThreadを継承し、InitializeThreadの
    /// Runメソッドを呼び出すことで、Initializeメソッドをスレッドで
    /// 実行するようになっています。
    /// </summary>
    public class MovieLoader : InitializeThread
    {
        #region Fields
        // Animation information 
        //
        // アニメーション情報
        private AnimationInfo animationInfo;

        // Loaded movie objects
        //
        // 読み込まれたムービーオブジェクト
        private PuzzleAnimation movie;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the movie objects that have been loaded. 
        ///
        /// 読み込まれたムービーオブジェクトを取得します。
        /// </summary>
        public PuzzleAnimation Movie
        {
            get
            {
                if (!Initialized)
                    return null;

                return movie;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public MovieLoader(Game game, int cpu, AnimationInfo info)
            : base(game, cpu)
        {
            animationInfo = info;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Loads the movie.
        ///
        /// ムービーの読み込み処理を行います。
        /// </summary>
        protected override void Initialize()
        {
            // Sets the CPU core.
            // 
            // CPUコアの設定をします。
            SetCpuCore();

            // Loads the movie.
            // 
            // ムービーを読み込みます。
            movie = PuzzleAnimation.CreateAnimationComponent(Game, animationInfo);

            // Initializes the movie that has been loaded.
            //
            // 読み込んだムービーの初期化を行います。
            movie.Initialize();

            base.Initialize();
        }
        #endregion
    }
}
