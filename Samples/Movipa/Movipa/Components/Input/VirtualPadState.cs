#region File Description
//-----------------------------------------------------------------------------
// VirtualPadState.cs
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
    /// This class manages the input state of a virtual pad.
    /// There are the following parameters: 
    /// button, cross button, stick, and trigger.
    ///
    /// 仮想パッドの入力状態を管理します。
    /// ボタン、十字ボタン、スティック、トリガーのパラメータがあります。
    /// </summary>
    public class VirtualPadState
    {
        #region Fields
        private VirtualPadButtons buttons;
        private VirtualPadDPad dPad;
        private VirtualPadThumbSticks thumbSticks;
        private VirtualPadTriggers triggers;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the input state of the button.
        ///
        /// ボタンの入力状態を取得します。
        /// </summary>
        public VirtualPadButtons Buttons
        {
            get { return buttons; }
        }


        /// <summary>
        /// Obtains the input state of the cross button.
        ///
        /// 十字ボタンの入力状態を取得します。
        /// </summary>
        public VirtualPadDPad DPad
        {
            get { return dPad; }
        }


        /// <summary>
        /// Obtains the input state of the stick.
        ///
        /// スティックの入力状態を取得します。
        /// </summary>
        public VirtualPadThumbSticks ThumbSticks
        {
            get { return thumbSticks; }
        }


        /// <summary>
        /// Obtains the input state of the trigger.
        ///
        /// トリガーの入力状態を取得します。
        /// </summary>
        public VirtualPadTriggers Triggers
        {
            get { return triggers; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public VirtualPadState()
        {
            buttons = new VirtualPadButtons();
            dPad = new VirtualPadDPad();
            thumbSticks = new VirtualPadThumbSticks();
            triggers = new VirtualPadTriggers();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the input state of the virtual pad.
        ///
        /// 仮想パッドの入力状態を更新します。
        /// </summary>
        public void Update()
        {
            buttons.Update();
            dPad.Update();
            thumbSticks.Update();
            triggers.Update();
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
            buttons.SetPress(press);
            dPad.SetPress(press);
            thumbSticks.SetPress(press);
            triggers.SetPress(press);
        }
        #endregion
    }
}