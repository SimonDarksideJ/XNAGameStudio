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

namespace Minjie
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    /// <remarks>Based on a similar class in the Game State Management sample.</remarks>
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
        /// Checks for a "exit game" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool ExitGame
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       IsNewButtonPress(Buttons.Back);
            }
        }


        /// <summary>
        /// Checks for a "reset camera" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool ResetCamera
        {
            get
            {
                return IsNewKeyPress(Keys.R) ||
                       IsNewButtonPress(Buttons.RightStick);
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
        /// Helper for checking if a key was pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsKeyPress(Keys key)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsKeyPress(key, (PlayerIndex)i))
                    return true;
            }

            return false;
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
        /// Helper for checking if a key was pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsKeyPress(Keys key, PlayerIndex playerIndex)
        {
            return CurrentKeyboardStates[(int)playerIndex].IsKeyDown(key);
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
        /// Helper for checking if a button was pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsButtonPress(Buttons button)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsButtonPress(button, (PlayerIndex)i))
                    return true;
            }

            return false;
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
        /// Helper for checking if a button was pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsButtonPress(Buttons button, PlayerIndex playerIndex)
        {
            return CurrentGamePadStates[(int)playerIndex].IsButtonDown(button);
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


        /// <summary>
        /// Checks for a "play piece" input action from the specified player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool IsPlayPiece(PlayerIndex playerIndex)
        {
            if (IsNewButtonPress(Buttons.A, playerIndex))
            {
                return true;
            }

            switch (playerIndex)
            {
                case PlayerIndex.One:
                    if (IsNewKeyPress(Keys.Space, playerIndex))
                    {
                        return true;
                    }
                    break;

                case PlayerIndex.Two:
                    if (IsNewKeyPress(Keys.Enter, playerIndex))
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }


        /// <summary>
        /// Checks for a "selection up" input action from the specified player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool IsPieceSelectionUp(PlayerIndex playerIndex)
        {
            if (IsNewButtonPress(Buttons.LeftThumbstickUp, playerIndex) ||
                IsNewButtonPress(Buttons.DPadUp, playerIndex))
            {
                return true;
            }

            switch (playerIndex)
            {
                case PlayerIndex.One:
                    if (IsNewKeyPress(Keys.W, playerIndex))
                    {
                        return true;
                    }
                    break;

                case PlayerIndex.Two:
                    if (IsNewKeyPress(Keys.I, playerIndex))
                    {
                        return true;
                    }
                    break;
            }

            return false; 
        }


        /// <summary>
        /// Checks for a "selection down" input action from the specified player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool IsPieceSelectionDown(PlayerIndex playerIndex)
        {
            if (IsNewButtonPress(Buttons.LeftThumbstickDown, playerIndex) ||
                IsNewButtonPress(Buttons.DPadDown, playerIndex))
            {
                return true;
            }

            switch (playerIndex)
            {
                case PlayerIndex.One:
                    if (IsNewKeyPress(Keys.S, playerIndex))
                    {
                        return true;
                    }
                    break;

                case PlayerIndex.Two:
                    if (IsNewKeyPress(Keys.K, playerIndex))
                    {
                        return true;
                    }
                    break;
            }

            return false; 
        }


        /// <summary>
        /// Checks for a "selection left" input action from the specified player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool IsPieceSelectionLeft(PlayerIndex playerIndex)
        {
            if (IsNewButtonPress(Buttons.LeftThumbstickLeft, playerIndex) ||
                IsNewButtonPress(Buttons.DPadLeft, playerIndex))
            {
                return true;
            }

            switch (playerIndex)
            {
                case PlayerIndex.One:
                    if (IsNewKeyPress(Keys.A, playerIndex))
                    {
                        return true;
                    }
                    break;

                case PlayerIndex.Two:
                    if (IsNewKeyPress(Keys.J, playerIndex))
                    {
                        return true;
                    }
                    break;
            }

            return false; 
        }


        /// <summary>
        /// Checks for a "selection right" input action from the specified player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool IsPieceSelectionRight(PlayerIndex playerIndex)
        {
            if (IsNewButtonPress(Buttons.LeftThumbstickRight, playerIndex) ||
                IsNewButtonPress(Buttons.DPadRight, playerIndex))
            {
                return true;
            }

            switch (playerIndex)
            {
                case PlayerIndex.One:
                    if (IsNewKeyPress(Keys.D, playerIndex))
                    {
                        return true;
                    }
                    break;

                case PlayerIndex.Two:
                    if (IsNewKeyPress(Keys.L, playerIndex))
                    {
                        return true;
                    }
                    break;
            }

            return false; 
        }

        #endregion
    }
}
