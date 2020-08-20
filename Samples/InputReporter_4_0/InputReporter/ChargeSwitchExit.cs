#region File Description
//-----------------------------------------------------------------------------
// ChargeSwitchExit.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Input;
#endregion

namespace InputReporter
{
    /// <summary>
    /// ChargeSwitch type for exiting the game.
    /// </summary>
    class ChargeSwitchExit : ChargeSwitch
    {
        public ChargeSwitchExit(float duration) : base(duration) { }

        protected override bool IsCharging(ref GamePadState gamePadState)
        {
            return (gamePadState.Buttons.Back == ButtonState.Pressed);
        }
    }
}
