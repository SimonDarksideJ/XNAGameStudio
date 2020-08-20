#region File Description
//-----------------------------------------------------------------------------
// GamePadHelper.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;
#endregion

namespace Spacewar
{
    /// <summary>
    /// Useful class that wraps some game pad stuff to give you indication of single button presses
    /// by remembering previous state. Right now its one shot which means if you call a Pressed function 
    /// that will 'remove' the press.
    /// 
    /// Keyboard support should be mapped in here based on PlayerIndex.
    /// PlayerIndex.One Key mapping is (Keys for player one to use)
    /// PlayerIndex.Two Key mapping is (Keys for Player two to use)
    /// Players Three => Infinity are not supported on a keyboard!
    /// </summary>
    public enum GamePadKeys
    {
        Start = 0,
        Back,
        A,
        B,
        X,
        Y,
        Up,
        Down,
        Left,
        Right,
        LeftTrigger,
        RightTrigger,
        ThumbstickLeftXMin,
        ThumbstickLeftXMax,
        ThumbstickLeftYMin,
        ThumbstickLeftYMax,
        ThumbstickRightXMin,
        ThumbstickRightXMax,
        ThumbstickRightYMin,
        ThumbstickRightYMax
    };

    public class GamePadHelper
    {
        private PlayerIndex player;
        private Keymap keyMapping = new Keymap();
        private KeyboardState keyState;
        private Game game;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="player">Which player. Not that you can't use PlayerIndex.Any with this helper</param>
        public GamePadHelper(PlayerIndex player)
        {
            //Need to store the player. If you try to store a reference to the GamePad here it seems to 'forget'
            this.player = player;

            if (player == PlayerIndex.One)
            {
                keyMapping.Add(GamePadKeys.Start, SpacewarGame.Settings.Player1Start);
                keyMapping.Add(GamePadKeys.Back, SpacewarGame.Settings.Player1Back);
                keyMapping.Add(GamePadKeys.A, SpacewarGame.Settings.Player1A);
                keyMapping.Add(GamePadKeys.B, SpacewarGame.Settings.Player1B);
                keyMapping.Add(GamePadKeys.X, SpacewarGame.Settings.Player1X);
                keyMapping.Add(GamePadKeys.Y, SpacewarGame.Settings.Player1Y);
                keyMapping.Add(GamePadKeys.Up, SpacewarGame.Settings.Player1Up);
                keyMapping.Add(GamePadKeys.Down, SpacewarGame.Settings.Player1Down);
                keyMapping.Add(GamePadKeys.Left, SpacewarGame.Settings.Player1Left);
                keyMapping.Add(GamePadKeys.Right, SpacewarGame.Settings.Player1Right);
                keyMapping.Add(GamePadKeys.LeftTrigger, SpacewarGame.Settings.Player1LeftTrigger);
                keyMapping.Add(GamePadKeys.RightTrigger, SpacewarGame.Settings.Player1RightTrigger);
                keyMapping.Add(GamePadKeys.ThumbstickLeftXMin, SpacewarGame.Settings.Player1ThumbstickLeftXmin);
                keyMapping.Add(GamePadKeys.ThumbstickLeftXMax, SpacewarGame.Settings.Player1ThumbstickLeftXmax);
                keyMapping.Add(GamePadKeys.ThumbstickLeftYMin, SpacewarGame.Settings.Player1ThumbstickLeftYmin);
                keyMapping.Add(GamePadKeys.ThumbstickLeftYMax, SpacewarGame.Settings.Player1ThumbstickLeftYmax);
            }

            if (player == PlayerIndex.Two)
            {
                keyMapping.Add(GamePadKeys.Start, SpacewarGame.Settings.Player2Start);
                keyMapping.Add(GamePadKeys.Back, SpacewarGame.Settings.Player2Back);
                keyMapping.Add(GamePadKeys.A, SpacewarGame.Settings.Player2A);
                keyMapping.Add(GamePadKeys.B, SpacewarGame.Settings.Player2B);
                keyMapping.Add(GamePadKeys.X, SpacewarGame.Settings.Player2X);
                keyMapping.Add(GamePadKeys.Y, SpacewarGame.Settings.Player2Y);
                keyMapping.Add(GamePadKeys.Up, SpacewarGame.Settings.Player2Up);
                keyMapping.Add(GamePadKeys.Down, SpacewarGame.Settings.Player2Down);
                keyMapping.Add(GamePadKeys.Left, SpacewarGame.Settings.Player2Left);
                keyMapping.Add(GamePadKeys.Right, SpacewarGame.Settings.Player2Right);
                keyMapping.Add(GamePadKeys.LeftTrigger, SpacewarGame.Settings.Player2LeftTrigger);
                keyMapping.Add(GamePadKeys.RightTrigger, SpacewarGame.Settings.Player2RightTrigger);
                keyMapping.Add(GamePadKeys.ThumbstickLeftXMin, SpacewarGame.Settings.Player2ThumbstickLeftXmin);
                keyMapping.Add(GamePadKeys.ThumbstickLeftXMax, SpacewarGame.Settings.Player2ThumbstickLeftXmax);
                keyMapping.Add(GamePadKeys.ThumbstickLeftYMin, SpacewarGame.Settings.Player2ThumbstickLeftYmin);
                keyMapping.Add(GamePadKeys.ThumbstickLeftYMax, SpacewarGame.Settings.Player2ThumbstickLeftYmax);
            }
        }

        public GamePadState State
        {
            get
            {
                return state;
            }
        }

        private bool AWasReleased;
        private bool BWasReleased;
        private bool YWasReleased;
        private bool XWasReleased;
        private bool StartWasReleased;
        private bool BackWasReleased;
        private bool UpWasReleased;
        private bool DownWasReleased;
        private bool LeftWasReleased;
        private bool RightWasReleased;
        private bool LeftTriggerWasReleased;
        private bool RightTriggerWasReleased;

        private bool kbAWasReleased;
        private bool kbBWasReleased;
        private bool kbYWasReleased;
        private bool kbXWasReleased;
        private bool kbStartWasReleased;
        private bool kbBackWasReleased;
        private bool kbUpWasReleased;
        private bool kbDownWasReleased;
        private bool kbLeftWasReleased;
        private bool kbRightWasReleased;
        private bool kbLeftTriggerWasReleased;
        private bool kbRightTriggerWasReleased;

        private GamePadState state;

        public float ThumbStickLeftX
        {
            get
            {
                float result = 0.0f;
                if (game.IsActive)
                {
                    if (state.IsConnected)
                        result = state.ThumbSticks.Left.X;
                    if (keyState.IsKeyDown(keyMapping.Get(GamePadKeys.ThumbstickLeftXMin)))
                        result = -1.0f;
                    if (keyState.IsKeyDown(keyMapping.Get(GamePadKeys.ThumbstickLeftXMax)))
                        result = 1.0f;
                }
                return result;
            }
        }

        public float ThumbStickLeftY
        {
            get
            {
                float result = 0.0f;
                if (game.IsActive)
                {
                    if (state.IsConnected)
                        result = state.ThumbSticks.Left.Y;
                    if (keyState.IsKeyDown(keyMapping.Get(GamePadKeys.ThumbstickLeftYMin)))
                        result = -1.0f;
                    if (keyState.IsKeyDown(keyMapping.Get(GamePadKeys.ThumbstickLeftYMax)))
                        result = 1.0f;
                }
                return result;
            }
        }

        public float ThumbStickRightX
        {
            get
            {
                float result = 0.0f;
                if (game.IsActive)
                {
                    if (state.IsConnected)
                        result = state.ThumbSticks.Right.X;
                    if (keyState.IsKeyDown(keyMapping.Get(GamePadKeys.ThumbstickRightXMin)))
                        result = -1.0f;
                    if (keyState.IsKeyDown(keyMapping.Get(GamePadKeys.ThumbstickRightXMax)))
                        result = 1.0f;
                }
                return result;
            }
        }

        public float ThumbStickRightY
        {
            get
            {
                float result = 0.0f;
                if (game.IsActive)
                {
                    if (state.IsConnected)
                        result = state.ThumbSticks.Right.Y;
                    if (keyState.IsKeyDown(keyMapping.Get(GamePadKeys.ThumbstickRightYMin)))
                        result = -1.0f;
                    if (keyState.IsKeyDown(keyMapping.Get(GamePadKeys.ThumbstickRightYMax)))
                        result = 1.0f;
                }
                return result;
            }
        }
        /// <summary>
        /// Has the left trigger been pressed
        /// </summary>
        public bool LeftTriggerPressed
        {
            get
            {
                return ((checkPressed(state.Triggers.Left, ref LeftTriggerWasReleased))
                     || (checkPressed(keyState.IsKeyDown(keyMapping.Get(GamePadKeys.LeftTrigger)), ref kbLeftTriggerWasReleased)));
            }
        }

        /// <summary>
        /// Has the right trigger been pressed
        /// </summary>
        public bool RightTriggerPressed
        {
            get
            {
                return ((checkPressed(state.Triggers.Right, ref RightTriggerWasReleased))
                     || (checkPressed(keyState.IsKeyDown(keyMapping.Get(GamePadKeys.RightTrigger)), ref kbRightTriggerWasReleased)));
            }
        }

        /// <summary>
        /// Has the A button been pressed
        /// </summary>
        public bool APressed
        {
            get
            {
                return ((checkPressed(state.Buttons.A, ref AWasReleased))
                     || (checkPressed(keyState.IsKeyDown(keyMapping.Get(GamePadKeys.A)), ref kbAWasReleased)));
            }
        }

        /// <summary>
        /// Has the B button been pressed
        /// </summary>
        public bool BPressed
        {
            get
            {
                return ((checkPressed(state.Buttons.B, ref BWasReleased))
                     || (checkPressed(keyState.IsKeyDown(keyMapping.Get(GamePadKeys.B)), ref kbBWasReleased)));
            }
        }

        /// <summary>
        /// Has the Y button been pressed
        /// </summary>
        public bool YPressed
        {
            get
            {
                return ((checkPressed(state.Buttons.Y, ref YWasReleased))
                     || (checkPressed(keyState.IsKeyDown(keyMapping.Get(GamePadKeys.Y)), ref kbYWasReleased)));
            }
        }

        /// <summary>
        /// Has the X button been pressed
        /// </summary>
        public bool XPressed
        {
            get
            {
                return ((checkPressed(state.Buttons.X, ref XWasReleased))
                     || (checkPressed(keyState.IsKeyDown(keyMapping.Get(GamePadKeys.X)), ref kbXWasReleased)));
            }
        }

        /// <summary>
        /// Has the start button been pressed
        /// </summary>
        public bool StartPressed
        {
            get
            {
                return ((checkPressed(state.Buttons.Start, ref StartWasReleased))
                     || (checkPressed(keyState.IsKeyDown(keyMapping.Get(GamePadKeys.Start)), ref kbStartWasReleased)));
            }
        }

        /// <summary>
        /// Has the back button been pressed
        /// </summary>
        public bool BackPressed
        {
            get
            {
                return ((checkPressed(state.Buttons.Back, ref BackWasReleased))
                     || (checkPressed(keyState.IsKeyDown(keyMapping.Get(GamePadKeys.Back)), ref kbBackWasReleased)));
            }
        }

        /// <summary>
        /// Has the up dpad been pressed
        /// </summary>
        public bool UpPressed
        {
            get
            {
                return ((checkPressed(state.DPad.Up, ref UpWasReleased))
                     || (checkPressed(keyState.IsKeyDown(keyMapping.Get(GamePadKeys.Up)), ref kbUpWasReleased)));
            }
        }

        /// <summary>
        /// Has the down dpad been pressed
        /// </summary>
        public bool DownPressed
        {
            get
            {
                return ((checkPressed(state.DPad.Down, ref DownWasReleased))
                     || (checkPressed(keyState.IsKeyDown(keyMapping.Get(GamePadKeys.Down)), ref kbDownWasReleased)));
            }
        }

        /// <summary>
        /// Has the left dpad been pressed
        /// </summary>
        public bool LeftPressed
        {
            get
            {
                return ((checkPressed(state.DPad.Left, ref LeftWasReleased))
                     || (checkPressed(keyState.IsKeyDown(keyMapping.Get(GamePadKeys.Left)), ref kbLeftWasReleased)));
            }
        }

        /// <summary>
        /// Has the right dpad been pressed
        /// </summary>
        public bool RightPressed
        {
            get
            {
                return ((checkPressed(state.DPad.Right, ref RightWasReleased))
                     || (checkPressed(keyState.IsKeyDown(keyMapping.Get(GamePadKeys.Right)), ref kbRightWasReleased)));
            }
        }

        private bool checkPressed(ButtonState buttonState, ref bool controlWasReleased)
        {
            //Buttons are considered pressed when their state = Pressed
            return checkPressed(buttonState == ButtonState.Pressed, ref controlWasReleased);
        }

        private bool checkPressed(float triggerState, ref bool controlWasReleased)
        {
            //Triggers are considered pressed when their value >0
            return checkPressed(triggerState > 0, ref controlWasReleased);
        }

        private bool checkPressed(bool pressed, ref bool controlWasReleased)
        {
            bool returnValue = controlWasReleased && pressed;
            if (game.IsActive)
            {
                //If the item is currently pressed then reset the 'released' indicators
                if (returnValue)
                {
                    controlWasReleased = false;
                }
            }
            else
            {
                return false;  // Control can never be pressed, game is not the active application!
            }

            return returnValue;
        }


        /// <summary>
        /// Updates the states. Should be called once per frame in the game loop otherwise the IsPressed functions
        /// won't work
        /// </summary>
        public void Update(Game game, KeyboardState keyState)
        {
            state = GamePad.GetState(player);
            this.keyState = keyState;
            this.game = game;

            if (player == PlayerIndex.One)
            {
                //Check which buttons have been released so we can detect presses
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player1A)) kbAWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player1B)) kbBWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player1Y)) kbYWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player1X)) kbXWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player1Start)) kbStartWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player1Back)) kbBackWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player1Up)) kbUpWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player1Down)) kbDownWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player1Left)) kbLeftWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player1Right)) kbRightWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player1LeftTrigger)) kbLeftTriggerWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player1RightTrigger)) kbRightTriggerWasReleased = true;
            }
            else
            {
                //Check which buttons have been released so we can detect presses
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player2A)) kbAWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player2B)) kbBWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player2Y)) kbYWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player2X)) kbXWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player2Start)) kbStartWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player2Back)) kbBackWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player2Up)) kbUpWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player2Down)) kbDownWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player2Left)) kbLeftWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player2Right)) kbRightWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player2LeftTrigger)) kbLeftTriggerWasReleased = true;
                if (keyState.IsKeyUp(SpacewarGame.Settings.Player2RightTrigger)) kbRightTriggerWasReleased = true;
            }

            if (state.IsConnected)
            {
                //Check which buttons have been released so we can detect presses
                if (state.Buttons.A == ButtonState.Released) AWasReleased = true;
                if (state.Buttons.B == ButtonState.Released) BWasReleased = true;
                if (state.Buttons.Y == ButtonState.Released) YWasReleased = true;
                if (state.Buttons.X == ButtonState.Released) XWasReleased = true;
                if (state.Buttons.Start == ButtonState.Released) StartWasReleased = true;
                if (state.Buttons.Back == ButtonState.Released) BackWasReleased = true;
                if (state.DPad.Up == ButtonState.Released) UpWasReleased = true;
                if (state.DPad.Down == ButtonState.Released) DownWasReleased = true;
                if (state.DPad.Left == ButtonState.Released) LeftWasReleased = true;
                if (state.DPad.Right == ButtonState.Released) RightWasReleased = true;
                if (state.Triggers.Left == 0f) LeftTriggerWasReleased = true;
                if (state.Triggers.Right == 0f) RightTriggerWasReleased = true;
            }
        }
    }
}
