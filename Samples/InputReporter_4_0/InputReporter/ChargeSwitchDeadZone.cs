#region File Description
//-----------------------------------------------------------------------------
// ChargeSwitchDeadZone.cs
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
    /// ChargeSwitch type for switching between dead-zone types.
    /// </summary>
    class ChargeSwitchDeadZone : ChargeSwitch
    {
        public ChargeSwitchDeadZone(float duration) : base(duration) { }

        protected override bool IsCharging(ref GamePadState gamePadState)
        {
            return (gamePadState.Buttons.Start == ButtonState.Pressed);
        }
    }
}
