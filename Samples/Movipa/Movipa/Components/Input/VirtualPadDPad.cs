#region File Description
//-----------------------------------------------------------------------------
// VirtualPadDPad.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#endregion

namespace Movipa.Components.Input
{
    /// <summary>
    /// This class manages the cross button of a virtual pad.
    /// There are the following parameters: 
    /// Up, Down, Left, and Right.
    ///
    /// 仮想パッドの十字ボタンを管理します。
    /// Up、Down、Left、Rightのパラメータがあります。
    /// </summary>
    public class VirtualPadDPad
    {
        #region Fields
        private InputState up;
        private InputState down;
        private InputState left;
        private InputState right;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the input state of the Up button.
        ///
        /// Upボタンの入力状態を取得します。
        /// </summary>
        public InputState Up
        {
            get { return up; }
        }


        /// <summary>
        /// Obtains the input state of the Down button.
        ///
        /// Downボタンの入力状態を取得します。
        /// </summary>
        public InputState Down
        {
            get { return down; }
        }


        /// <summary>
        /// Obtains the input state of the Left button.
        ///
        /// Leftボタンの入力状態を取得します。
        /// </summary>
        public InputState Left
        {
            get { return left; }
        }


        /// <summary>
        /// Obtains the input state of the Right button.
        ///
        /// Rightボタンの入力状態を取得します。
        /// </summary>
        public InputState Right
        {
            get { return right; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public VirtualPadDPad()
        {
            up = new InputState();
            down = new InputState();
            left = new InputState();
            right = new InputState();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the state of the button.
        ///
        /// ボタンの状態を更新します。
        /// </summary>
        public void Update()
        {
            up.Update();
            down.Update();
            left.Update();
            right.Update();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Sets the key state for when it is pressed.
        ///
        /// キーの押下状態を設定します。
        /// </summary>
        public void SetPress(bool press)
        {
            up.SetPress(press);
            down.SetPress(press);
            left.SetPress(press);
            right.SetPress(press);
        }
        #endregion
    }
}