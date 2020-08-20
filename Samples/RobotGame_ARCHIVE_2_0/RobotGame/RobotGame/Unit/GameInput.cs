#region File Description
//-----------------------------------------------------------------------------
// GameInput.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RobotGameData.Input;
#endregion

namespace RobotGame
{
    #region Enum

    /// <summary>
    /// game key in the game.
    /// </summary>
    public enum GameKey 
    {
        MoveForward = 0,
        MoveBackward,
        MoveLeft,
        MoveRight,

        TurnLeft,
        TurnRight,
         
        WeaponFire,
        WeaponReload,
        WeaponChange,
        Booster,

        Count
    }

    #endregion

    /// <summary>
    /// It contains an InputComponent and checks the status of the input devices, 
    /// such as keyboard or Xbox360 controller device, and can change the key inputs.
    /// Key settings part takes care of changing the default key configuration.
    /// </summary>
    public class GameInput
    {
        #region Fields

        InputComponent input = null;

        Keys[] gameKeyboard = new Keys[(int)GameKey.Count];
        ControlPad[] gameControlPad = new ControlPad[(int)GameKey.Count];

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameInput(InputComponent input)
        {
            this.input = input;
        }

        /// <summary>
        /// configures the keyboard setting.
        /// </summary>
        /// <param name="gameKey">game key in the game</param>
        /// <param name="setKey">a key of keyboard</param>
        public void SetGameKey(GameKey gameKey, Keys setKey)
        {
            gameKeyboard[(int)gameKey] = setKey;
        }

        /// <summary>
        /// configures the Xbox 360 controller setting.
        /// </summary>
        /// <param name="gameKey">game key in the game</param>
        /// <param name="setKey">a key of Xbox 360 controller</param>
        public void SetGameControlPad(GameKey gameKey, ControlPad setKey)
        {
            gameControlPad[(int)gameKey] = setKey;
        }

        /// <summary>
        /// checks whether specified key has been pressed.
        /// </summary>
        /// <param name="key">game key in the game</param>
        /// <returns></returns>
        public bool IsPressKey(GameKey key)
        {
            if (input.IsConnectedControlPad && 
                input.IsPressControlPad(TranslateToControlPad(key)))
            {
                return true;
            }
            
            return input.IsPressKey(TranslateToKeyboard(key));
        }

        /// <summary>
        /// checks whether specified key has been lifted.
        /// </summary>
        /// <param name="key">game key in the game</param>
        /// <returns></returns>
        public bool IsReleaseKey(GameKey key)
        {
            if (input.IsConnectedControlPad && 
                input.IsReleaseControlPad(TranslateToControlPad(key)))
            {
                return true;
            }

            return input.IsReleaseKey(TranslateToKeyboard(key));
        }

        /// <summary>
        /// checks whether specified key has been pressed only once.
        /// </summary>
        /// <param name="key">game key in the game</param>
        /// <returns></returns>
        public bool IsStrokeKey(GameKey key)
        {
            if (input.IsConnectedControlPad && 
                input.IsStrokeControlPad(TranslateToControlPad(key)))
            {
                return true;
            }
            
            return input.IsStrokeKey(TranslateToKeyboard(key));
        }

        public bool IsPressTurn()
        {
            return (IsPressKey(GameKey.TurnLeft) || IsPressKey(GameKey.TurnRight));
        }

        public bool IsPressMovement()
        {
            return (IsPressKey(GameKey.MoveForward) || 
                    IsPressKey(GameKey.MoveBackward) ||
                    IsPressKey(GameKey.MoveLeft) ||
                    IsPressKey(GameKey.MoveRight));
        }

        private Keys TranslateToKeyboard(GameKey key)
        {
            return gameKeyboard[(int)key];
        }

        private ControlPad TranslateToControlPad(GameKey key)
        {       
            return gameControlPad[(int)key];
        }

        #region Key Settings

        /// <summary>
        /// Keyboard and controllers default settings for two players
        /// </summary>
        public void SetDefaultKey(PlayerIndex index)
        {
            if (index == PlayerIndex.One)
            {
                // player 1 keys in the game.
                gameKeyboard[(int)GameKey.MoveForward] = Keys.W;
                gameKeyboard[(int)GameKey.MoveBackward] = Keys.S;
                gameKeyboard[(int)GameKey.MoveLeft] = Keys.A;
                gameKeyboard[(int)GameKey.MoveRight] = Keys.D;
                gameKeyboard[(int)GameKey.TurnLeft] = Keys.F;
                gameKeyboard[(int)GameKey.TurnRight] = Keys.H;
                gameKeyboard[(int)GameKey.WeaponFire] = Keys.G;
                gameKeyboard[(int)GameKey.WeaponReload] = Keys.T;
                gameKeyboard[(int)GameKey.WeaponChange] = Keys.V;
                gameKeyboard[(int)GameKey.Booster] = Keys.Space;

                gameControlPad[(int)GameKey.MoveForward] = 
                    ControlPad.LeftThumbStickUp;
                gameControlPad[(int)GameKey.MoveBackward] =
                    ControlPad.LeftThumbStickDown;
                gameControlPad[(int)GameKey.MoveLeft] = 
                    ControlPad.LeftThumbStickLeft;
                gameControlPad[(int)GameKey.MoveRight] = 
                    ControlPad.LeftThumbStickRight;
                gameControlPad[(int)GameKey.TurnLeft] = 
                    ControlPad.RightThumbStickLeft;
                gameControlPad[(int)GameKey.TurnRight] = 
                    ControlPad.RightThumbStickRight;
                gameControlPad[(int)GameKey.WeaponFire] = 
                    ControlPad.RightTrigger;
                gameControlPad[(int)GameKey.WeaponReload] = 
                    ControlPad.RightShoulder;
                gameControlPad[(int)GameKey.WeaponChange] = 
                    ControlPad.LeftShoulder;
                gameControlPad[(int)GameKey.Booster] = 
                    ControlPad.LeftTrigger;
            }
            else if (index == PlayerIndex.Two)
            {
                // player 2 keys in the game.
                gameKeyboard[(int)GameKey.MoveForward] = Keys.Up;
                gameKeyboard[(int)GameKey.MoveBackward] = Keys.Down;
                gameKeyboard[(int)GameKey.MoveLeft] = Keys.Left;
                gameKeyboard[(int)GameKey.MoveRight] = Keys.Right;
                gameKeyboard[(int)GameKey.TurnLeft] = Keys.NumPad4;
                gameKeyboard[(int)GameKey.TurnRight] = Keys.NumPad6;
                gameKeyboard[(int)GameKey.WeaponFire] = Keys.NumPad5;
                gameKeyboard[(int)GameKey.WeaponReload] = Keys.NumPad8;
                gameKeyboard[(int)GameKey.WeaponChange] = Keys.NumPad1;
                gameKeyboard[(int)GameKey.Booster] = Keys.NumPad0;

                gameControlPad[(int)GameKey.MoveForward] = 
                    ControlPad.LeftThumbStickUp;
                gameControlPad[(int)GameKey.MoveBackward] = 
                    ControlPad.LeftThumbStickDown;
                gameControlPad[(int)GameKey.MoveLeft] = 
                    ControlPad.LeftThumbStickLeft;
                gameControlPad[(int)GameKey.MoveRight] =
                    ControlPad.LeftThumbStickRight;
                gameControlPad[(int)GameKey.TurnLeft] =
                    ControlPad.RightThumbStickLeft;
                gameControlPad[(int)GameKey.TurnRight] =
                    ControlPad.RightThumbStickRight;
                gameControlPad[(int)GameKey.WeaponFire] =
                    ControlPad.RightTrigger;
                gameControlPad[(int)GameKey.WeaponReload] = 
                    ControlPad.RightShoulder;
                gameControlPad[(int)GameKey.WeaponChange] =
                    ControlPad.LeftShoulder;
                gameControlPad[(int)GameKey.Booster] = 
                    ControlPad.LeftTrigger;
            }
        }

        #endregion
    }
}
