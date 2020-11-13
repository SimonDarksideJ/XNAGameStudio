#region File Description
//-----------------------------------------------------------------------------
// VirtualPadThumbSticks.cs
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
    /// This class manages the state of the sticks of a virtual pad.
    /// There are the following two parameters: left stick and right stick.
    /// The obtained values are converted to digital values, not to analog values.
    ///
    /// 仮想パッドのスティックの状態を管理します。
    /// 左スティックと右スティックのパラメータがあります。
    /// 取得される値はアナログではなく、デジタルに変換されます。
    /// </summary>
    public class VirtualPadThumbSticks
    {
        #region Fields
        private VirtualPadDPad left = new VirtualPadDPad();
        private VirtualPadDPad right = new VirtualPadDPad();
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the input state of the left stick.
        ///
        /// 左スティックの入力状態を取得します。
        /// </summary>
        public VirtualPadDPad Left
        {
            get { return left; }
        }


        /// <summary>
        /// Obtains the input state of the right stick.
        ///
        /// 右スティックの入力状態を取得します。
        /// </summary>
        public VirtualPadDPad Right
        {
            get { return right; }
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the input state of the stick.
        ///
        /// スティックの入力状態を更新します。
        /// </summary>
        public void Update()
        {
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
            left.SetPress(press);
            right.SetPress(press);
        }
        #endregion
    }
}