#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Pickture
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        #region Fields

        public const int MaxInputs = 4;

        public readonly KeyboardState[] CurrentKeyboardStates;
        public readonly GamePadState[] CurrentGamePadStates;

        public readonly KeyboardState[] LastKeyboardStates;
        public readonly GamePadState[] LastGamePadStates;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            CurrentKeyboardStates = new KeyboardState[MaxInputs];
            CurrentGamePadStates = new GamePadState[MaxInputs];

            LastKeyboardStates = new KeyboardState[MaxInputs];
            LastGamePadStates = new GamePadState[MaxInputs];
        }


        #endregion

        #region Properties


        /// <summary>
        /// Checks for a "menu up" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuUp
        {
            get
            {
                return IsNewKeyPress(Keys.Up) ||
                       IsNewButtonPress(Buttons.DPadUp) ||
                       IsNewButtonPress(Buttons.LeftThumbstickUp);
            }
        }


        /// <summary>
        /// Checks for a "menu down" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuDown
        {
            get
            {
                return IsNewKeyPress(Keys.Down) ||
                       IsNewButtonPress(Buttons.DPadDown) ||
                       IsNewButtonPress(Buttons.LeftThumbstickDown);
            }
        }


        /// <summary>
        /// Checks for a "menu left" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuLeft
        {
            get
            {
                return IsNewKeyPress(Keys.Left) ||
                       IsNewButtonPress(Buttons.DPadLeft) ||
                       IsNewButtonPress(Buttons.LeftThumbstickLeft);
            }
        }


        /// <summary>
        /// Checks for a "menu right" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuRight
        {
            get
            {
                return IsNewKeyPress(Keys.Right) ||
                       IsNewButtonPress(Buttons.DPadRight) ||
                       IsNewButtonPress(Buttons.LeftThumbstickRight);
            }
        }


        /// <summary>
        /// Checks for a "flip up" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool FlipUp
        {
            get
            {
                return IsNewKeyPress(Keys.W) ||
                       IsNewButtonPress(Buttons.RightThumbstickUp);
            }
        }


        /// <summary>
        /// Checks for a "flip down" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool FlipDown
        {
            get
            {
                return IsNewKeyPress(Keys.S) ||
                       IsNewButtonPress(Buttons.RightThumbstickDown);
            }
        }


        /// <summary>
        /// Checks for a "flip left" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool FlipLeft
        {
            get
            {
                return IsNewKeyPress(Keys.A) ||
                       IsNewButtonPress(Buttons.RightThumbstickLeft);
            }
        }


        /// <summary>
        /// Checks for a "flip right" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool FlipRight
        {
            get
            {
                return IsNewKeyPress(Keys.D) ||
                       IsNewButtonPress(Buttons.RightThumbstickRight);
            }
        }


        /// <summary>
        /// Checks for a "flip camera left" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool FlipCameraLeft
        {
            get
            {
                return IsNewKeyPress(Keys.Q) ||
                       IsNewButtonPress(Buttons.LeftTrigger);
            }
        }


        /// <summary>
        /// Checks for a "flip camera right" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool FlipCameraRight
        {
            get
            {
                return IsNewKeyPress(Keys.E) ||
                       IsNewButtonPress(Buttons.RightTrigger);
            }
        }


        /// <summary>
        /// Checks for a "shift active chip" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool ShiftActiveChip
        {
            get
            {
                return IsNewKeyPress(Keys.Space) ||
                       IsNewButtonPress(Buttons.A);
            }
        }


        /// <summary>
        /// Checks for a "menu select" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuSelect
        {
            get
            {
                return IsNewKeyPress(Keys.Space) ||
                       IsNewKeyPress(Keys.Enter) ||
                       IsNewButtonPress(Buttons.A) ||
                       IsNewButtonPress(Buttons.Start);
            }
        }


        /// <summary>
        /// Checks for a "menu cancel" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuCancel
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       IsNewButtonPress(Buttons.B) ||
                       IsNewButtonPress(Buttons.Back);
            }
        }


        /// <summary>
        /// Checks for a "pause the game" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool PauseGame
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       IsNewButtonPress(Buttons.Back) ||
                       IsNewButtonPress(Buttons.Start);
            }
        }


        #endregion

        #region Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                LastKeyboardStates[i] = CurrentKeyboardStates[i];
                LastGamePadStates[i] = CurrentGamePadStates[i];

                CurrentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
            }
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsNewKeyPress(Keys key)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsNewKeyPress(key, (PlayerIndex)i))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsNewKeyPress(Keys key, PlayerIndex playerIndex)
        {
            return (CurrentKeyboardStates[(int)playerIndex].IsKeyDown(key) &&
                    LastKeyboardStates[(int)playerIndex].IsKeyUp(key));
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsNewButtonPress(Buttons button)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsNewButtonPress(button, (PlayerIndex)i))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsNewButtonPress(Buttons button, PlayerIndex playerIndex)
        {
            return (CurrentGamePadStates[(int)playerIndex].IsButtonDown(button) &&
                    LastGamePadStates[(int)playerIndex].IsButtonUp(button));
        }


        /// <summary>
        /// Checks for a "menu select" input action from the specified player.
        /// </summary>
        public bool IsMenuSelect(PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Space, playerIndex) ||
                   IsNewKeyPress(Keys.Enter, playerIndex) ||
                   IsNewButtonPress(Buttons.A, playerIndex) ||
                   IsNewButtonPress(Buttons.Start, playerIndex);
        }


        /// <summary>
        /// Checks for a "menu cancel" input action from the specified player.
        /// </summary>
        public bool IsMenuCancel(PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Escape, playerIndex) ||
                   IsNewButtonPress(Buttons.B, playerIndex) ||
                   IsNewButtonPress(Buttons.Back, playerIndex);
        }


        #endregion
    }
}
