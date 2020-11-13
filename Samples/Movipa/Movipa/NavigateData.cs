#region File Description
//-----------------------------------------------------------------------------
// NavigateData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#endregion

namespace Movipa
{
    /// <summary>
    /// Manages the Navigate button and text string status,
    /// including text string to display and blink status.
    /// Navigate drawing is performed by the SceneComponent class.
    /// 
    /// ナビゲートボタンと文字列の状態を管理します。
    /// 表示する文字列と、点滅の状態を持っています。
    /// ナビゲートの描画はSceneComponentクラスで行っています。
    /// </summary>
    public class NavigateData
    {
        #region Fields
        /// <summary>
        /// Text string displayed in Navigate
        /// 
        /// ナビゲートに表示する文字列
        /// </summary>
        private string message = String.Empty;

        /// <summary>
        /// Blink mode
        /// 
        /// 点滅するモード
        /// </summary>
        private bool blink = false;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public NavigateData()
        {
        }


        /// <summary>
        /// Specifies the text string to display and initializes the instance.
        ///
        /// 表示する文字列を指定してインスタンスを初期化します。
        /// </summary>
        /// <param name="text">Text string to display</param>
        ///  
        /// <param name="text">表示する文字列</param>
        public NavigateData(string text)
        {
            Message = text;
        }


        /// <summary>
        /// Specifies the text string to display and the blink mode, 
        /// then initializes the instance.
        /// <param name="text">Text string to display</param>
        /// <param name="blink">Blink status</param>
        ///  
        /// </summary>
        /// 表示する文字列と点滅モードを指定してインスタンスを初期化します。
        /// <param name="text">表示する文字列</param>
        /// <param name="blink">点滅状態</param>
        public NavigateData(string text, bool blink)
        {
            Message = text;
            Blink = blink;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the text string to display in the Navigate.
        /// 
        /// ナビゲートに表示する文字列を取得または設定します。
        /// </summary>
        public string Message
        {
            get { return message; }
            set { message = value; }
        }


        /// <summary>
        /// Obtains or sets the blink mode.
        /// 
        /// 点滅モードを取得または設定します。
        /// </summary>
        public bool Blink
        {
            get { return blink; }
            set { blink = value; }
        }
        #endregion
    }
}
