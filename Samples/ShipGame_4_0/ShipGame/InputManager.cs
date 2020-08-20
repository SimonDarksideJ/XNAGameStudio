#region File Description
//-----------------------------------------------------------------------------
// InputManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion



namespace ShipGame
{
    public class InputState
    {
        public GamePadState[] padState;
        public KeyboardState[] keyState;

        public InputState()
        {
            padState = new GamePadState[2];
            keyState = new KeyboardState[2];
            GetInput(false);
        }

        public void GetInput(bool singlePlayer)
        {
            padState[0] = GamePad.GetState(PlayerIndex.One);
            padState[1] = GamePad.GetState(PlayerIndex.Two);
            if (singlePlayer)
                keyState[0] = Keyboard.GetState();
            else
                keyState[1] = Keyboard.GetState();
        }

        public void CopyInput(InputState state)
        {
            padState[0] = state.padState[0];
            padState[1] = state.padState[1];
            keyState[0] = state.keyState[0];
            keyState[1] = state.keyState[1];
        }
    }

    public class InputManager
    {
        InputState currentState;   // current frame input
        InputState lastState;      // last frame input

        /// <summary>
        /// Create a new input manager
        /// </summary>
        public InputManager()
        {
            currentState = new InputState();
            lastState = new InputState();
        }

        /// <summary>
        /// Begin input (aqruire input from all controlls)
        /// </summary>
        public void BeginInputProcessing(bool singlePlayer)
        {
            currentState.GetInput(singlePlayer);
        }

        /// <summary>
        /// End input (save current input to last frame input)
        /// </summary>
        public void EndInputProcessing()
        {
            lastState.CopyInput(currentState);
        }

        /// <summary>
        /// Get the current input state
        /// </summary>
        public InputState CurrentState
        {
            get { return currentState; }
        }

        /// <summary>
        /// Get last frame input state
        /// </summary>
        public InputState LastState
        {
            get { return lastState; }
        }

        /// <summary>
        /// Check if a key is down in current frame for a given player
        /// </summary>
        public bool IsKeyDown(int player, Keys key)
        {
            return currentState.keyState[player].IsKeyDown(key);
        }

        /// <summary>
        /// Check if a key was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsKeyPressed(int player, Keys key)
        {
            return currentState.keyState[player].IsKeyDown(key) &&
                lastState.keyState[player].IsKeyUp(key);
        }

        /// <summary>
        /// Return left stick position in a Vector2
        /// </summary>
        public Vector2 LeftStick(int player)
        {
            return currentState.padState[player].ThumbSticks.Left;
        }

        /// <summary>
        /// Return right stick position in a Vector2
        /// </summary>
        public Vector2 RightStick(int player)
        {
            return currentState.padState[player].ThumbSticks.Right;
        }

        /// <summary>
        /// Check if left trigger was pressed in this frame for a given player
        /// (positive this frame and zero in last frame)
        /// </summary>
        public bool IsTriggerPressedLeft(int player)
        {
            return currentState.padState[player].Triggers.Left > 0 &&
                lastState.padState[player].Triggers.Left == 0;
        }

        /// <summary>
        /// Check if right trigger was pressed in this frame for a given player
        /// (positive this frame and zero in last frame)
        /// </summary>
        public bool IsTriggerPressedRigth(int player)
        {
            return currentState.padState[player].Triggers.Right > 0 &&
                lastState.padState[player].Triggers.Right == 0;
        }

        /// <summary>
        /// Check if back button was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedBack(int player)
        {
            return currentState.padState[player].Buttons.Back == ButtonState.Pressed &&
                lastState.padState[player].Buttons.Back == ButtonState.Released;
        }

        /// <summary>
        /// Check if start button was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedStart(int player)
        {
            return currentState.padState[player].Buttons.Start == ButtonState.Pressed &&
                lastState.padState[player].Buttons.Start == ButtonState.Released;
        }

        /// <summary>
        /// Check if dpad left button was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedDPadLeft(int player)
        {
            return currentState.padState[player].DPad.Left == ButtonState.Pressed &&
                lastState.padState[player].DPad.Left == ButtonState.Released;
        }

        /// <summary>
        /// Check if dpad right button was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedDPadRight(int player)
        {
            return currentState.padState[player].DPad.Right == ButtonState.Pressed &&
                lastState.padState[player].DPad.Right == ButtonState.Released;
        }

        /// <summary>
        /// Check if dpad up button was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedDPadUp(int player)
        {
            return currentState.padState[player].DPad.Up == ButtonState.Pressed &&
                lastState.padState[player].DPad.Up == ButtonState.Released;
        }

        /// <summary>
        /// Check if dpad down button was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedDPadDown(int player)
        {
            return currentState.padState[player].DPad.Down == ButtonState.Pressed &&
                lastState.padState[player].DPad.Down == ButtonState.Released;
        }

        /// <summary>
        /// Check if A button was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedA(int player)
        {
            return currentState.padState[player].Buttons.A == ButtonState.Pressed &&
                lastState.padState[player].Buttons.A == ButtonState.Released;
        }

        /// <summary>
        /// Check if B button was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedB(int player)
        {
            return currentState.padState[player].Buttons.B == ButtonState.Pressed &&
                lastState.padState[player].Buttons.B == ButtonState.Released;
        }

        /// <summary>
        /// Check if X button was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedX(int player)
        {
            return currentState.padState[player].Buttons.X == ButtonState.Pressed &&
                lastState.padState[player].Buttons.X == ButtonState.Released;
        }

        /// <summary>
        /// Check if A button was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedY(int player)
        {
            return currentState.padState[player].Buttons.Y == ButtonState.Pressed &&
                lastState.padState[player].Buttons.Y == ButtonState.Released;
        }

        /// <summary>
        /// Check if left shoulder button was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedLeftShoulder(int player)
        {
            return 
                (currentState.padState[player].Buttons.LeftShoulder == 
                    ButtonState.Pressed) &&
                (lastState.padState[player].Buttons.LeftShoulder == 
                    ButtonState.Released);
        }

        /// <summary>
        /// Check if right shoulder button was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedRightShoulder(int player)
        {
            return 
                (currentState.padState[player].Buttons.RightShoulder == 
                    ButtonState.Pressed) &&
                (lastState.padState[player].Buttons.RightShoulder == 
                    ButtonState.Released);
        }

        /// <summary>
        /// Check if left stick was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedLeftStick(int player)
        {
            return 
                (currentState.padState[player].Buttons.LeftStick == 
                    ButtonState.Pressed) &&
                (lastState.padState[player].Buttons.LeftStick == 
                    ButtonState.Released);
        }

        /// <summary>
        /// Check if right stick was pressed in this frame for a given player
        /// (down in this frame and up in last frame)
        /// </summary>
        public bool IsButtonPressedRightStick(int player)
        {
            return 
                (currentState.padState[player].Buttons.RightStick == 
                    ButtonState.Pressed) &&
                (lastState.padState[player].Buttons.RightStick == 
                    ButtonState.Released);
        }

        /// <summary>
        /// Check left stick as a button for up press
        /// </summary>
        public bool IsButtonPressedLeftStickUp(int player)
        {
            return currentState.padState[player].ThumbSticks.Left.Y > 0.5f &&
                lastState.padState[player].ThumbSticks.Left.Y <= 0.5f;
        }

        /// <summary>
        /// Check left stick as a button for down press
        /// </summary>
        public bool IsButtonPressedLeftStickDown(int player)
        {
            return currentState.padState[player].ThumbSticks.Left.Y < -0.5f &&
                lastState.padState[player].ThumbSticks.Left.Y >= -0.5f;
        }

        /// <summary>
        /// Check left stick as a button for left press
        /// </summary>
        public bool IsButtonPressedLeftStickLeft(int player)
        {
            return currentState.padState[player].ThumbSticks.Left.X < -0.5f &&
                lastState.padState[player].ThumbSticks.Left.X >= -0.5f;
        }

        /// <summary>
        /// Check left stick as a button for right press
        /// </summary>
        public bool IsButtonPressedLeftStickRight(int player)
        {
            return currentState.padState[player].ThumbSticks.Left.X > 0.5f &&
                lastState.padState[player].ThumbSticks.Left.X <= 0.5f;
        }
    }
}
