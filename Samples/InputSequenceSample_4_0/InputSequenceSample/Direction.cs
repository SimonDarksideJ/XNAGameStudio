#region File Description
//-----------------------------------------------------------------------------
// Direction.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
#endregion

namespace InputSequenceSample
{
    /// <summary>
    /// Helper class for working with the 8-way directions stored in a Buttons enum.
    /// </summary>
    static class Direction
    {
        // Helper bit masks for directions defined with the Buttons flags enum.
        public const Buttons None = 0;
        public const Buttons Up = Buttons.DPadUp | Buttons.LeftThumbstickUp;
        public const Buttons Down = Buttons.DPadDown | Buttons.LeftThumbstickDown;
        public const Buttons Left = Buttons.DPadLeft | Buttons.LeftThumbstickLeft;
        public const Buttons Right = Buttons.DPadRight | Buttons.LeftThumbstickRight;
        public const Buttons UpLeft = Up | Left;
        public const Buttons UpRight = Up | Right;
        public const Buttons DownLeft = Down | Left;
        public const Buttons DownRight = Down | Right;
        public const Buttons Any = Up | Down | Left | Right;

        /// <summary>
        /// Gets the current direction from a game pad and keyboard.
        /// </summary>
        public static Buttons FromInput(GamePadState gamePad, KeyboardState keyboard)
        {
            Buttons direction = None;

            // Get vertical direction.
            if (gamePad.IsButtonDown(Buttons.DPadUp) ||
                gamePad.IsButtonDown(Buttons.LeftThumbstickUp) ||
                keyboard.IsKeyDown(Keys.Up))
            {
                direction |= Up;
            }
            else if (gamePad.IsButtonDown(Buttons.DPadDown) ||
                gamePad.IsButtonDown(Buttons.LeftThumbstickDown) ||
                keyboard.IsKeyDown(Keys.Down))
            {
                direction |= Down;
            }

            // Comebine with horizontal direction.
            if (gamePad.IsButtonDown(Buttons.DPadLeft) ||
                gamePad.IsButtonDown(Buttons.LeftThumbstickLeft) ||
                keyboard.IsKeyDown(Keys.Left))
            {
                direction |= Left;
            }
            else if (gamePad.IsButtonDown(Buttons.DPadRight) ||
                gamePad.IsButtonDown(Buttons.LeftThumbstickRight) ||
                keyboard.IsKeyDown(Keys.Right))
            {
                direction |= Right;
            }

            return direction;
        }

        /// <summary>
        /// Gets the direction without non-direction buttons from a set of Buttons flags.
        /// </summary>
        public static Buttons FromButtons(Buttons buttons)
        {
            // Extract the direction from a full set of buttons using a bit mask.
            return buttons & Any;
        }
    }
}
