#region File Description
//-----------------------------------------------------------------------------
// InitializeThread.cs
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
#endregion

namespace Movipa.Components.Scene.Menu
{
    /// <summary>     
    /// Class used for asynchronous initialization.
    /// This class is inherited. 
    /// The initialize method is defined in the inherited 
    /// target class with description of initialization processing.
    /// When the Run method is called, the Initialize method is
    /// executed in the thread, thus enabling asynchronous initialization.
    /// The executing CPU core may be changed in Xbox360 only.
    /// When initialization is completed, the Initialized property becomes True.
    /// 
    /// 非同期で初期化をする為のクラスです。
    /// このクラスは継承して使用します。
    /// 継承先のクラスでInitializeメソッドを定義し、初期化の処理を
    /// 記述します。Runメソッドを呼ぶと、スレッドでInitializeメソッドが
    /// 実行され、非同期で初期化を行うことが出来ます。
    /// Xbox360でのみ、実行させるCPUのコアを変更することが出来ます。
    /// 初期化が終了したら、InitializedプロパティがTrueになります。
    /// </summary>
    public class InitializeThread
    {
        #region Fields
        // CPU core
        //
        // cpuコア
        private int cpuId;

        // Initialized flag
        //  
        // 初期化済みフラグ
        private bool initialized = false;

        // Load thread
        //
        //読み込みスレッド
        private Thread thread;

        // Game object transferred to movie
        //
        // ムービーに渡すGameオブジェクト
        private Game game;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the Game instance.
        /// 
        /// Gameインスタンスを取得します。
        /// </summary>
        public Game Game
        {
            get { return game; }
        }

        /// <summary>
        /// Obtains the initialization status.
        ///
        /// 初期化状態を取得します。
        /// </summary>
        public bool Initialized
        {
            get { return initialized; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        /// <param name="game">Gameインスタンス</param>
        /// <param name="cpu">使用するCPUコア</param>
        public InitializeThread(Game game, int cpu)
        {
            this.game = game;
            cpuId = cpu;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Loads the movie asynchronously.
        ///
        /// ムービーを非同期で読み込みます。 
        /// </summary>
        public void Run()
        {
            // Start the thread.
            // 
            // スレッドの開始
            thread = new Thread(new ThreadStart(this.Initialize));
            thread.Start();
        }


        /// <summary>
        /// Forcibly terminates the thread.
        ///
        /// スレッドを強制終了します。
        /// </summary>
        public void Abort()
        {
            if (thread != null)
                thread.Abort();
        }


        /// <summary>
        /// Waits until the thread terminates.
        ///
        /// スレッドが終了するまで待機します。
        /// </summary>
        public void Join()
        {
            if (thread != null)
                thread.Join();
        }

        
        /// <summary>
        /// Loads the moview.
        ///
        /// ムービーを読み込みます。 
        /// </summary>
        protected virtual void Initialize()
        {
            // Sets the initialized flag.
            // 
            // 初期化済みフラグを設定する
            initialized = true;
        }


        /// <summary>
        /// Sets the CPU core processed in the thread.
        ///
        /// スレッドで処理を行うCPUコアの設定をします。
        /// </summary>
        protected void SetCpuCore()
        {
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(cpuId);
#endif
        }
        #endregion
    }
}
