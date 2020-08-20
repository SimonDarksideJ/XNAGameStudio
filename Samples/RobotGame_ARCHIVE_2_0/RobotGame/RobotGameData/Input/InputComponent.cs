#region File Description
//-----------------------------------------------------------------------------
// InputComponent.cs
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

namespace RobotGameData.Input
{
    #region Input Key enum

    /// <summary>
    /// Buttons of an Xbox 360 Controller 
    /// </summary>
    public enum ControlPad
    {
        None = 0,
        Start,
        Back,
        A,
        B,
        X,
        Y,

        LeftShoulder,
        LeftStick,
        LeftTrigger,
        LeftThumbStickUp,
        LeftThumbStickDown,
        LeftThumbStickLeft,
        LeftThumbStickRight,

        RightShoulder,
        RightStick,
        RightTrigger,
        RightThumbStickUp,
        RightThumbStickDown,
        RightThumbStickLeft,
        RightThumbStickRight,

        LeftPad,
        RightPad,
        UpPad,
        DownPad,

        Count
    }

    /// <summary>
    /// Triggers of the Xbox360 Controller 
    /// </summary>
    public enum Trigger
    {
        Left = 0,
        Right = 1,

        Count
    }

    #endregion

    /// <summary>
    /// It receives inputs from users, depending on their devices.
    /// It supports a keyboard and an Xbox 360 Controller devices and 
    /// recognizes the input device’s key press state, key release state, 
    /// and key stroke state.
    /// </summary>
    public class InputComponent
    {
        #region Fields

        private PlayerIndex     playerIndex;
        private KeyboardState   keyboardState;
        private KeyboardState   oldKeyboardState;
        private KeyboardState   emptyKeyboardState;
        private GamePadState    gamePadState;        
        private GamePadState    oldGamePadState;
        private GamePadState    emptyGamePadState;

        private float[] triggers = new float[(int) Trigger.Count];
        private float[] oldTriggers = new float[(int)Trigger.Count];
        private Vector2[] thumbStick = new Vector2[(int)Trigger.Count];
        private Vector2[] oldThumbStick = new Vector2[(int)Trigger.Count];

        private float[] vibrationAmount = new float[(int)Trigger.Count];
        private TimeSpan vibrationDurationAccTime = TimeSpan.Zero;
        private TimeSpan vibrationDuration = TimeSpan.Zero;

        #endregion

        #region Properties

        public PlayerIndex PlayerIndex
        {
            get { return playerIndex; }
        }

        public float[] Triggers
        {
            get { return triggers; }
        }

        public Vector2[] ThumbStick
        {
            get { return thumbStick; }
        }

        public float[] VibrationAmount
        {
            get { return vibrationAmount; }
        }

        public bool IsConnectedControlPad
        {
            get { return gamePadState.IsConnected; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="idx">player index</param>
        public InputComponent(PlayerIndex idx)
        {
            playerIndex = idx;
        }

        /// <summary>
        /// Initialize members
        /// </summary>
        public void Initialize()
        {
            //  Save the empty state
            emptyKeyboardState = Keyboard.GetState();
            emptyGamePadState = GamePad.GetState(playerIndex);
        }

        /// <summary>
        /// Process vibration of an Xbox 360 Controller
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {            
            //  GamePad vibrate
            if (vibrationDurationAccTime < vibrationDuration)
            {
                vibrationDurationAccTime += gameTime.ElapsedGameTime;

                GamePad.SetVibration(PlayerIndex, 
                                    VibrationAmount[(int)Trigger.Left], 
                                    VibrationAmount[(int)Trigger.Right]);
            }
            else if (vibrationDurationAccTime >= vibrationDuration)
            {
                //  Reset
                vibrationDurationAccTime = TimeSpan.Zero;
                vibrationDuration = TimeSpan.Zero;

                GamePad.SetVibration(PlayerIndex, 0.0f, 0.0f);
            }
        }

        /// <summary>
        /// It gets executed before the input processing.
        /// It stores the current input state.
        /// </summary>
        public void PreUpdate()
        {
            //  Check keyboard state
            keyboardState = Keyboard.GetState();

            //  Check GamePad state
            gamePadState = GamePad.GetState(playerIndex);

            //  Check GamePad Tiggers amount
            Triggers[(int)Trigger.Left] = gamePadState.Triggers.Left;
            Triggers[(int)Trigger.Right] = gamePadState.Triggers.Right;

            //  Check GamePad ThumbStick amount
            thumbStick[(int)Trigger.Left] = gamePadState.ThumbSticks.Left;
            thumbStick[(int)Trigger.Right] = gamePadState.ThumbSticks.Right;
        }

        /// <summary>
        /// It gets executed after the input processing.
        /// It stores the current input state to compare at the next frame.
        /// </summary>
        public void PostUpdate()
        {
            // Store key state to old state.
            oldKeyboardState = keyboardState;
            oldGamePadState = gamePadState;

            oldTriggers[(int)Trigger.Left] = Triggers[(int)Trigger.Left];
            oldTriggers[(int)Trigger.Right] = Triggers[(int)Trigger.Right];

            oldThumbStick[(int)Trigger.Left] = thumbStick[(int)Trigger.Left];
            oldThumbStick[(int)Trigger.Right] = thumbStick[(int)Trigger.Right];
        }

        /// <summary>
        /// Reset the input state.
        /// </summary>
        public void Reset()
        {
            //  all key state reset
            keyboardState = oldKeyboardState = emptyKeyboardState;
            gamePadState = oldGamePadState = emptyGamePadState;

            vibrationDurationAccTime = TimeSpan.Zero;
            vibrationDuration = TimeSpan.Zero;

            //  Stop the vibration
            GamePad.SetVibration(PlayerIndex, 0.0f, 0.0f);
        }

        /// <summary>
        /// checks whether the specified button of a keyboard is pressed.
        /// </summary>
        /// <param name="key">input key</param>
        public bool IsPressKey(Keys key)
        {
            return keyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// checks whether the specified button of a keyboard is released.
        /// </summary>
        /// <param name="key">input key</param>
        public bool IsReleaseKey(Keys key)
        {
            return keyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// checks whether the specified button of a keyboard is pressed once 
        /// for this frame.
        /// </summary>
        /// <param name="key">input key</param>
        public bool IsStrokeKey(Keys key)
        {
            //  check stroke keyboard keys
            return (!oldKeyboardState.IsKeyDown(key) && IsPressKey(key));
        }

        /// <summary>
        /// checks whether the specified button of an Xbox 360 controller is pressed.
        /// </summary>
        /// <param name="pad">Button of the Xbox 360 controller</param>
        public bool IsPressControlPad(ControlPad pad)
        {
            return IsControlPadState(gamePadState, pad, ButtonState.Pressed);
        }

        /// <summary>
        /// checks whether the specified button of an Xbox 360 controller is released.
        /// </summary>
        /// <param name="pad">Button of the Xbox 360 controller</param>
        public bool IsReleaseControlPad(ControlPad pad)
        {
            return IsControlPadState(gamePadState, pad, ButtonState.Released);
        }

        /// <summary>
        /// checks whether the specified button of an Xbox 360 controller 
        /// is pressed once for this frame.
        /// </summary>
        /// <param name="pad">Button of the Xbox 360 controller</param>
        public bool IsStrokeControlPad(ControlPad pad)
        {
            switch (pad)
            {
                case ControlPad.LeftThumbStickUp:
                    return IsStrokeThumbStickUp(Trigger.Left);

                case ControlPad.LeftThumbStickDown:
                    return IsStrokeThumbStickDown(Trigger.Left);

                case ControlPad.LeftThumbStickLeft:
                    return IsStrokeThumbStickLeft(Trigger.Left);

                case ControlPad.LeftThumbStickRight:
                    return IsStrokeThumbStickRight(Trigger.Left);

                case ControlPad.RightThumbStickUp:
                    return IsStrokeThumbStickUp(Trigger.Right);

                case ControlPad.RightThumbStickDown:
                    return IsStrokeThumbStickDown(Trigger.Right);

                case ControlPad.RightThumbStickLeft:
                    return IsStrokeThumbStickLeft(Trigger.Right);

                case ControlPad.RightThumbStickRight:
                    return IsStrokeThumbStickRight(Trigger.Right);

                case ControlPad.LeftTrigger:
                    return IsStrokeTriggers(Trigger.Left);

                case ControlPad.RightTrigger:
                    return IsStrokeTriggers(Trigger.Right);
            };

            //  check stroke GamePad buttons
            return (!IsControlPadState(oldGamePadState, pad, ButtonState.Pressed) && 
                                    IsPressControlPad(pad));
        }

        /// <summary>
        /// checks whether the specified trigger of an Xbox 360 controller 
        /// is pressed once for this frame.
        /// </summary>
        /// <param name="index">Thumb stick of the Xbox 360 controller</param>
        public bool IsStrokeControlPadTriggers(Trigger index)
        {
            return (Triggers[(int)index] > 0.0f && oldTriggers[(int)index] <= 0.0f);
        }

        public bool IsStrokeThumbStickUp(Trigger index)
        {
            return (ThumbStick[(int)index].Y > 0.0f && 
                    oldThumbStick[(int)index].Y <= 0.0f);
        }

        public bool IsStrokeThumbStickDown(Trigger index)
        {
            return (ThumbStick[(int)index].Y < 0.0f && 
                    oldThumbStick[(int)index].Y >= 0.0f);
        }

        public bool IsStrokeThumbStickLeft(Trigger index)
        {
            return (ThumbStick[(int)index].X < 0.0f && 
                    oldThumbStick[(int)index].X >= 0.0f);
        }

        public bool IsStrokeThumbStickRight(Trigger index)
        {
            return (ThumbStick[(int)index].X > 0.0f && 
                    oldThumbStick[(int)index].X <= 0.0f);
        }

        public bool IsStrokeTriggers(Trigger index)
        {
            return (Triggers[(int)index] > 0.0f &&
                    oldTriggers[(int)index] <= 0.0f);
        }

        /// <summary>
        /// Set the vibration of an Xbox 360 controller
        /// </summary>
        /// <param name="duration">Vibration duration time</param>
        /// <param name="leftAmount">Left vibration amount</param>
        /// <param name="rightAmount">Right vibration amount</param>
        public void SetGamePadVibration(float duration, float leftAmount, 
                                        float rightAmount)
        {
            leftAmount = MathHelper.Clamp(leftAmount, 0.0f, 1.0f);
            rightAmount = MathHelper.Clamp(rightAmount, 0.0f, 1.0f);

            vibrationDuration = TimeSpan.FromSeconds(duration);
            vibrationAmount[(int)Trigger.Left] = leftAmount;
            vibrationAmount[(int)Trigger.Right] = rightAmount;
        }

        /// <summary>
        /// returns the number of which the trigger of an Xbox 360 controller 
        /// has been pressed.
        /// </summary>
        /// <returns>Pressing amount</returns>
        public float GetGamePadTriggers( Trigger index)
        {
            return Triggers[(int)index];
        }

        /// <summary>
        /// returns the value of the angle which the thumb stick of an 
        /// Xbox 360 controller  has been pressed.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector2 GetThumbStickAmount(Trigger index)
        {
            return ThumbStick[(int)index];
        }

        /// <summary>
        /// compares the input state of the Xbox 360 controller.
        /// </summary>
        /// <param name="GamePadState">target input state</param>
        /// <param name="pad">Button of the Xbox 360 Controller</param>
        /// <param name="state">target button state</param>
        /// <returns></returns>
        private bool IsControlPadState(GamePadState targetGamePadState, ControlPad pad, 
            ButtonState state)
        {
            switch (pad)
            {
                case ControlPad.Start:
                    return (targetGamePadState.Buttons.Start == state);

                case ControlPad.Back:
                    return (targetGamePadState.Buttons.Back == state);

                case ControlPad.A:
                    return (targetGamePadState.Buttons.A == state);

                case ControlPad.B:
                    return (targetGamePadState.Buttons.B == state);

                case ControlPad.X:
                    return (targetGamePadState.Buttons.X == state);

                case ControlPad.Y:
                    return (targetGamePadState.Buttons.Y == state);  
              
                case ControlPad.LeftShoulder:
                    return (targetGamePadState.Buttons.LeftShoulder == state);

                case ControlPad.LeftStick:
                    return (targetGamePadState.Buttons.LeftStick == state);

                case ControlPad.RightShoulder:
                    return (targetGamePadState.Buttons.RightShoulder == state);

                case ControlPad.RightStick:
                    return (targetGamePadState.Buttons.RightStick == state);  
              
                case ControlPad.LeftPad:
                    return (targetGamePadState.DPad.Left == state);

                case ControlPad.RightPad:
                    return (targetGamePadState.DPad.Right == state);

                case ControlPad.UpPad:
                    return (targetGamePadState.DPad.Up == state);

                case ControlPad.DownPad:
                    return (targetGamePadState.DPad.Down == state);

                case ControlPad.LeftTrigger:
                    {
                        if (state == ButtonState.Pressed)
                            return (GetGamePadTriggers(Trigger.Left) > 0.0f);
                        else if (state == ButtonState.Released)
                            return (GetGamePadTriggers(Trigger.Left) == 0.0f);

                        return false;
                    }

                case ControlPad.RightTrigger:
                    {
                        if (state == ButtonState.Pressed)
                            return (GetGamePadTriggers(Trigger.Right) > 0.0f);
                        else if (state == ButtonState.Released)
                            return (GetGamePadTriggers(Trigger.Right) == 0.0f);

                        return false;
                    }

                case ControlPad.LeftThumbStickUp:
                    {
                        if (state == ButtonState.Pressed)
                            return (GetThumbStickAmount(Trigger.Left).Y > 0.0f);
                        else if (state == ButtonState.Released)
                            return (GetThumbStickAmount(Trigger.Left).Y == 0.0f);

                        return false;
                    }

                case ControlPad.LeftThumbStickDown:
                    {
                        if (state == ButtonState.Pressed)
                            return (GetThumbStickAmount(Trigger.Left).Y < 0.0f);
                        else if (state == ButtonState.Released)
                            return (GetThumbStickAmount(Trigger.Left).Y == 0.0f);

                        return false;
                    }

                case ControlPad.LeftThumbStickLeft:
                    {
                        if (state == ButtonState.Pressed)
                            return (GetThumbStickAmount(Trigger.Left).X < 0.0f);
                        else if (state == ButtonState.Released)
                            return (GetThumbStickAmount(Trigger.Left).X == 0.0f);

                        return false;
                    }

                case ControlPad.LeftThumbStickRight:
                    {
                        if (state == ButtonState.Pressed)
                            return (GetThumbStickAmount(Trigger.Left).X > 0.0f);
                        else if (state == ButtonState.Released)
                            return (GetThumbStickAmount(Trigger.Left).X == 0.0f);

                        return false;
                    }

                case ControlPad.RightThumbStickUp:
                    {
                        if (state == ButtonState.Pressed)
                            return (GetThumbStickAmount(Trigger.Right).Y > 0.0f);
                        else if (state == ButtonState.Released)
                            return (GetThumbStickAmount(Trigger.Right).Y == 0.0f);

                        return false;
                    }

                case ControlPad.RightThumbStickDown:
                    {
                        if (state == ButtonState.Pressed)
                            return (GetThumbStickAmount(Trigger.Right).Y < 0.0f);
                        else if (state == ButtonState.Released)
                            return (GetThumbStickAmount(Trigger.Right).Y == 0.0f);

                        return false;
                    }

                case ControlPad.RightThumbStickLeft:
                    {
                        if (state == ButtonState.Pressed)
                            return (GetThumbStickAmount(Trigger.Right).X < 0.0f);
                        else if (state == ButtonState.Released)
                            return (GetThumbStickAmount(Trigger.Right).Y == 0.0f);

                        return false;
                    }

                case ControlPad.RightThumbStickRight:
                    {
                        if (state == ButtonState.Pressed)
                            return (GetThumbStickAmount(Trigger.Right).X > 0.0f);
                        else if (state == ButtonState.Released)
                            return (GetThumbStickAmount(Trigger.Right).Y == 0.0f);

                        return false;
                    }

                default: return false;
            }
        }
    }
}
