#region File Description
//-----------------------------------------------------------------------------
// GameObject.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace Movipa.Util
{
    /// <summary>
    /// Includes initialized, released, visible and enabled status flags.
    /// 
    /// 機能を拡張したオブジェクトクラスです。
    /// 初期化と開放、可視状態や動作状態のフラグも持ちます。
    /// </summary>
    public class GameObject : IDisposable
    {
        #region Fields
        private bool initialized = false;
        private bool disposed = false;
        private bool visible = true;
        private bool enabled = true;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the initialization status.
        /// 
        /// 初期化状態を取得します。
        /// </summary>
        public bool Initialized
        {
            get { return initialized; }
        }

        /// <summary>
        /// Obtains the release status. 
        /// 
        /// 開放状態を取得します。
        /// </summary>
        public bool Disposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Obtains or sets the visibility status.
        ///
        /// 可視状態を取得または設定します。
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        /// <summary>
        /// Obtains or sets the enabled status.
        /// 
        /// 動作状態を取得または設定します。
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Performs initialization processing.
        /// 
        /// 初期化処理を行います。
        /// </summary>
        public virtual void Initialize()
        {
            initialized = true;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases all resources.
        /// 
        /// 全てのリソースを開放します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources.
        /// 
        /// 全てのリソースを開放します。
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed) 
            {
                disposed = true;
            }
        }

        #endregion
    }
}