#region File Description
//-----------------------------------------------------------------------------
// VirtualPadTriggers.cs
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
    /// This class gets the input state of the triggers of a virtual pad.
    /// The obtained values are converted to digital values, not to analog values.
    ///
    /// 仮想パッドのトリガーの入力状態を取得します。
    /// 取得される値はアナログではなく、デジタルに変換されます。
    /// </summary>
    public class VirtualPadTriggers
    {
        #region Fields
        private InputState left = new InputState();
        private InputState right = new InputState();
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the input state of the left trigger.
        ///
        /// 左トリガーの入力状態を取得します。
        /// </summary>
        public InputState Left
        {
            get { return left; }
        }


        /// <summary>
        /// Obtains the input value of the right trigger.
        ///
        /// 右トリガーの入力状態を取得します。
        /// </summary>
        public InputState Right
        {
            get { return right; }
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Obtains the input state of the trigger.
        ///
        /// トリガーの入力状態を取得します。
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