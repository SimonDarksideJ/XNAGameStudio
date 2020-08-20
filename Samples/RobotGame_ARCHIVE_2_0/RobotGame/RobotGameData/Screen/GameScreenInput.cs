#region File Description
//-----------------------------------------------------------------------------
// GameScreenInput.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RobotGameData.Input;
#endregion

namespace RobotGameData.Screen
{
    /// <summary>
    /// Helper for reading input from keyboard and ControlPad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class GameScreenInput
    {
        #region Fields

        InputComponent input = null;
        bool enable = true;  

        #endregion

        #region Properties

        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }

        /// <summary>
        /// Checks for a "menu up" input action (on either keyboard or GamePad).
        /// </summary>
        public bool MenuUp
        {
            get
            {
                if (!Enable) return false;

                return (
                    (input.IsStrokeKey(Keys.W) && 
                        (input.PlayerIndex == PlayerIndex.One)) || 
                    (input.IsStrokeKey(Keys.Up) && 
                        (input.PlayerIndex == PlayerIndex.Two)) || 
                    input.IsStrokeControlPad(ControlPad.UpPad) ||
                    input.IsStrokeControlPad(ControlPad.LeftThumbStickUp));
            }
        }


        /// <summary>
        /// Checks for a "menu down" input action (on either keyboard or GamePad).
        /// </summary>
        public bool MenuDown
        {
            get
            {
                if (!Enable) return false;

                return (
                    (input.IsStrokeKey(Keys.S) && 
                        (input.PlayerIndex == PlayerIndex.One)) ||
                    (input.IsStrokeKey(Keys.Down) && 
                        (input.PlayerIndex == PlayerIndex.Two)) ||
                    input.IsStrokeControlPad(ControlPad.DownPad) ||
                    input.IsStrokeControlPad(ControlPad.LeftThumbStickDown));
            }
        }


        /// <summary>
        /// Checks for a "menu left" input action (on either keyboard or GamePad).
        /// </summary>
        public bool MenuLeft
        {
            get
            {
                if (!Enable) return false;

                return (
                    (input.IsStrokeKey(Keys.A) && 
                        (input.PlayerIndex == PlayerIndex.One)) ||
                    (input.IsStrokeKey(Keys.Left) &&
                        (input.PlayerIndex == PlayerIndex.Two)) || 
                    input.IsStrokeControlPad(ControlPad.LeftPad) ||
                    input.IsStrokeControlPad(ControlPad.LeftThumbStickLeft));
            }
        }

        /// <summary>
        /// Checks for a "menu right" input action (on either keyboard or GamePad).
        /// </summary>
        public bool MenuRight
        {
            get
            {
                if (!Enable) return false;

                return (
                    (input.IsStrokeKey(Keys.D) &&
                        (input.PlayerIndex == PlayerIndex.One)) ||
                    (input.IsStrokeKey(Keys.Right) && 
                        (input.PlayerIndex == PlayerIndex.Two)) ||
                    input.IsStrokeControlPad(ControlPad.RightPad) ||
                    input.IsStrokeControlPad(ControlPad.LeftThumbStickRight));
            }
        }


        /// <summary>
        /// Checks for a "menu select" input action (on either keyboard or GamePad).
        /// </summary>
        public bool MenuSelect
        {
            get
            {
                if (!Enable) return false;

                return 
                    (!input.IsPressKey(Keys.LeftAlt) && 
                        !input.IsPressKey(Keys.RightAlt) &&
                    (input.IsStrokeKey(Keys.Space) || input.IsStrokeKey(Keys.Enter)) ||
                        input.IsStrokeControlPad(ControlPad.A));
            }
        }


        /// <summary>
        /// Checks for a "menu cancel" input action (on either keyboard or GamePad).
        /// </summary>
        public bool MenuExit
        {
            get
            {
                if (!Enable) return false;

                return (input.IsStrokeKey(Keys.Escape) ||
                        input.IsStrokeControlPad(ControlPad.Back));
            }
        }

        /// <summary>
        /// Checks for a "menu cancel" input action (on either keyboard or GamePad).
        /// </summary>
        public bool MenuCancel
        {
            get
            {
                if (!Enable) return false;

                return (input.IsStrokeKey(Keys.LeftControl) ||
                        input.IsStrokeControlPad(ControlPad.B) ||
                        input.IsStrokeControlPad(ControlPad.Y));
            }
        }


        /// <summary>
        /// Checks for a "pause the game" input action (on either keyboard or GamePad).
        /// </summary>
        public bool PauseGame
        {
            get
            {
                if (!Enable) return false;

                return (input.IsStrokeKey(Keys.Escape) || 
                        input.IsStrokeControlPad(ControlPad.Start) ||
                        input.IsStrokeControlPad(ControlPad.Back));  
            }
        }


        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">player index</param>
        public GameScreenInput(PlayerIndex index)
        {
            input = FrameworkCore.InputManager.GetInputComponent(index);
        }

        /// <summary>
        /// resets input state.
        /// </summary>
        public void Reset()
        {
            input.Reset();

            enable = true;
        }
    }
}
