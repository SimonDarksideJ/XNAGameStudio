#region File Description
//-----------------------------------------------------------------------------
// VirtualPadButtons.cs
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
    /// This class manages the buttons of a virtual pad.
    /// There are the following buttons:
    /// A, B, X, Y, LeftShoulder, RightShoulder, LeftStick, RightStick, Back, and Start.
    ///
    /// 仮想パッドのボタンを管理します。
    /// ボタンにはA、B、X、Y、LeftShoulder、RightShoulder、LeftStick、RightStick、
    /// そしてBackとStartがあります。
    /// </summary>
    public class VirtualPadButtons
    {
        #region Fields
        private InputState a;
        private InputState b;
        private InputState x;
        private InputState y;
        private InputState leftShoulder;
        private InputState rightShoulder;
        private InputState leftStick;
        private InputState rightStick;
        private InputState back;
        private InputState start;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the input state of the A button.
        ///
        /// Aボタンの入力状態を取得します。
        /// </summary>
        public InputState A
        {
            get { return a; }
        }


        /// <summary>
        /// Obtains the input state of the B button.
        ///
        /// Bボタンの入力状態を取得します。
        /// </summary>
        public InputState B
        {
            get { return b; }
        }


        /// <summary>
        /// Obtains the input state of the X button.
        ///
        /// Xボタンの入力状態を取得します。
        /// </summary>
        public InputState X
        {
            get { return x; }
        }


        /// <summary>
        /// Obtains the input state of the Y button.
        /// 
        /// Yボタンの入力状態を取得します。
        /// </summary>
        public InputState Y
        {
            get { return y; }
        }


        /// <summary>
        /// Obtains the input state of the LeftShoulder button.
        ///
        /// LeftShoulderボタンの入力状態を取得します。
        /// </summary>
        public InputState LeftShoulder
        {
            get { return leftShoulder; }
        }


        /// <summary>
        /// Obtains the input state of the RightShoulder button.
        ///
        /// RightShoulderボタンの入力状態を取得します。
        /// </summary>
        public InputState RightShoulder
        {
            get { return rightShoulder; }
        }


        /// <summary>
        /// Obtains the input state of the LeftStick button.
        ///
        /// LeftStickボタンの入力状態を取得します。
        /// </summary>
        public InputState LeftStick
        {
            get { return leftStick; }
        }


        /// <summary>
        /// Obtains the input state of the RightStick button.
        ///
        /// RightStickボタンの入力状態を取得します。
        /// </summary>
        public InputState RightStick
        {
            get { return rightStick; }
        }


        /// <summary>
        /// Obtains the input state of the Back button.
        ///
        /// Backボタンの入力状態を取得します。
        /// </summary>
        public InputState Back
        {
            get { return back; }
        }


        /// <summary>
        /// Obtains the input state of the Start button.
        ///
        /// Startボタンの入力状態を取得します。
        /// </summary>
        public InputState Start
        {
            get { return start; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public VirtualPadButtons()
        {
            a = new InputState();
            b = new InputState();
            x = new InputState();
            y = new InputState();
            leftShoulder = new InputState();
            rightShoulder = new InputState();
            leftStick = new InputState();
            rightStick = new InputState();
            back = new InputState();
            start = new InputState();
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
            a.Update();
            b.Update();
            x.Update();
            y.Update();
            leftShoulder.Update();
            rightShoulder.Update();
            leftStick.Update();
            rightStick.Update();
            back.Update();
            start.Update();
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
            a.SetPress(press);
            b.SetPress(press);
            x.SetPress(press);
            y.SetPress(press);
            leftShoulder.SetPress(press);
            rightShoulder.SetPress(press);
            leftStick.SetPress(press);
            rightStick.SetPress(press);
            back.SetPress(press);
            start.SetPress(press);
        }
        #endregion
    }
}