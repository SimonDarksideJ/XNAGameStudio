#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
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

namespace Flocking
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This public class tracks
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    /// <remarks>
    /// This public class is similar to one in the GameStateManagement sample.
    /// </remarks>
    public class InputState
    {
        #region Fields

        public KeyboardState CurrentKeyState;
        public GamePadState CurrentPadState;

        public KeyboardState LastKeyState;
        public GamePadState LastPadState;

        #endregion

        #region Properties


        /// <summary>
        /// Checks for Y move amount on either keyboard or gamepad.
        /// </summary>
        public float MoveCatY
        {
            get
            {
                if (CurrentKeyState.IsKeyDown(Keys.W))
                {
                    return -1.0f;
                }
                else if (CurrentKeyState.IsKeyDown(Keys.S))
                {
                    return 1.0f;
                }
                else
                {
                    //negative = move up
                    return -(CurrentPadState.ThumbSticks.Left.Y);
                }
            }
        }

        /// <summary>
        /// Checks for X move amount on either keyboard or gamepad.
        /// </summary>
        public float MoveCatX
        {
            get
            {
                if (CurrentKeyState.IsKeyDown(Keys.A))
                {
                    return -1.0f;
                }
                else if (CurrentKeyState.IsKeyDown(Keys.D))
                {
                    return 1.0f;
                }
                else
                {
                    return CurrentPadState.ThumbSticks.Left.X;
                }
            }
        }

        /// <summary>
        /// Checks for slider move amount on either keyboard or gamepad.
        /// </summary>
        public float SliderMove
        {
            get
            {
                if (CurrentKeyState.IsKeyDown(Keys.Left)||
                    CurrentPadState.IsButtonDown(Buttons.DPadLeft))
                {
                    return -1.0f;
                }
                else if (CurrentKeyState.IsKeyDown(Keys.Right) ||
                    CurrentPadState.IsButtonDown(Buttons.DPadRight))
                {
                    return 1.0f;
                }
                return -CurrentPadState.Triggers.Left + CurrentPadState.Triggers.Right;
            }
        }

        /// <summary>
        /// Checks for a "menu cancel" input action (on either keyboard or gamepad).
        /// </summary>
        public bool Exit
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       (CurrentPadState.Buttons.Back == ButtonState.Pressed &&
                        LastPadState.Buttons.Back == ButtonState.Released);
            }
        }

        /// <summary>
        /// Checks for a "reset distances" input action (on either keyboard or gamepad).
        /// </summary>
        public bool ResetDistances
        {
            get
            {
                return IsNewKeyPress(Keys.B) ||
                       (CurrentPadState.Buttons.B == ButtonState.Pressed &&
                        LastPadState.Buttons.B == ButtonState.Released);
            }
        }

        /// <summary>
        /// Checks for a "reset flock" input action (on either keyboard or gamepad).
        /// </summary>
        public bool ResetFlock
        {
            get
            {
                return IsNewKeyPress(Keys.X) ||
                       (CurrentPadState.Buttons.X == ButtonState.Pressed &&
                        LastPadState.Buttons.X == ButtonState.Released);
            }
        }

        /// <summary>
        /// Checks for an "up" input action (on either keyboard or gamepad).
        /// </summary>
        public bool Up
        {
            get
            {
                return IsNewKeyPress(Keys.Up) ||
                       (CurrentPadState.DPad.Up == ButtonState.Pressed &&
                        LastPadState.DPad.Up == ButtonState.Released);
            }
        }

        /// <summary>
        /// Checks for an "down" input action (on either keyboard or gamepad).
        /// </summary>
        public bool Down
        {
            get
            {
                return IsNewKeyPress(Keys.Down) ||
                       (CurrentPadState.DPad.Down == ButtonState.Pressed &&
                        LastPadState.DPad.Down == ButtonState.Released);
            }
        }

        /// <summary>
        /// Add or remove the cat
        /// </summary>
        public bool ToggleCatButton
        {
            get
            {
                return IsNewKeyPress(Keys.Y) ||
                       (CurrentPadState.Buttons.Y == ButtonState.Pressed &&
                        LastPadState.Buttons.Y == ButtonState.Released);
            }
        }


        #endregion

        #region Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            LastKeyState = CurrentKeyState;
            LastPadState = CurrentPadState;

            CurrentKeyState = Keyboard.GetState();
            CurrentPadState = GamePad.GetState(PlayerIndex.One);
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update.
        /// </summary>
        bool IsNewKeyPress(Keys key)
        {
            return (CurrentKeyState.IsKeyDown(key) &&
                    LastKeyState.IsKeyUp(key));
        }


        #endregion
    }
}
