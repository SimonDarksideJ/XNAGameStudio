#region File Description
//-----------------------------------------------------------------------------
// ChargeSwitch.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace InputReporter
{
    /// <summary>
    /// A GamePad-controlled switch that fires after the switch "charges up"
    /// (typically by holding a button down) for a set duration.
    /// </summary>
    /// <remarks>
    /// Since all buttons are relevant for data viewing, we don't want to directly
    /// tie any button press to an action, like exiting the application.  Our solution
    /// is to provide "charging" switches that run an event, like exiting, after holding
    /// the button down for a specified amount of time.
    /// </remarks>
    abstract class ChargeSwitch
    {
        public delegate void FireDelegate();
        public event FireDelegate Fire;

        private float duration = 3f;
        private float remaining = 0f;
        private bool active = false;
        public bool Active
        {
            get { return active; }
        }


        public ChargeSwitch(float duration)
        {
            Reset(duration);
        }

        public void Update(GameTime gameTime, ref GamePadState gamePadState)
        {
            active = IsCharging(ref gamePadState);
            if (active)
            {
                if (remaining > 0f)
                {
                    remaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (remaining <= 0f)
                    {
                        if (Fire != null)
                        {
                            Fire();
                        }
                    }
                }
            }
            else
            {
                // reset to the current duration
                Reset(duration);
            }
        }

        public void Reset(float duration)
        {
            if (duration < 0f)
            {
                throw new ArgumentOutOfRangeException("duration");
            }
            this.remaining = this.duration = duration;
        }

        protected abstract bool IsCharging(ref GamePadState gamePadState);
    }
}
