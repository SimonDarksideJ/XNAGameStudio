#region File Description
//-----------------------------------------------------------------------------
// VirtualKeyState.cs
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
    /// Key state for when it is pressed
    ///
    /// キーの押下状態
    /// </summary>
    public enum VirtualKeyState
    {
        /// <summary>
        /// Key is not pressed.
        ///
        /// 離されています。
        /// </summary>
        Free = 0,

        /// <summary>
        /// Key was pressed.
        ///
        /// 押されました。
        /// </summary>
        Push = 1,

        /// <summary>
        /// Key is being pressed.
        ///
        /// 押されています。
        /// </summary>
        Press = 3,

        /// <summary>
        /// Key was released.
        ///
        /// 離されました。
        /// </summary>
        Release = 2,
    }
}