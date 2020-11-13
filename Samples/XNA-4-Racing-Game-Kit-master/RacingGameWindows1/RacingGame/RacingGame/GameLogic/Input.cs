#region File Description
//-----------------------------------------------------------------------------
// Input.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Sounds;
#endregion

namespace RacingGame.GameLogic
{
    /// <summary>
    /// Input helper class, captures all the mouse, keyboard and XBox 360
    /// controller input and provides some nice helper methods and properties.
    /// Will also keep track of the last frame states for comparison if
    /// a button was just pressed this frame, but not already in the last frame.
    /// </summary>
    public static class Input
    {
        #region Variables
#if !XBOX360
        /// <summary>
        /// Mouse state, set every frame in the Update method.
        /// </summary>
        private static MouseState mouseState, mouseStateLastFrame;
#endif

        /// <summary>
        /// Was a mouse detected? Returns true if the user moves the mouse.
        /// On the Xbox 360 there will be no mouse movement and theirfore we
        /// know that we don't have to display the mouse.
        /// </summary>
        private static bool mouseDetected = false;

        /// <summary>
        /// Keyboard state, set every frame in the Update method.
        /// Note: KeyboardState is a class and not a struct,
        /// we have to initialize it here, else we might run into trouble when
        /// accessing any keyboardState data before BaseGame.Update() is called.
        /// We can also NOT use the last state because everytime we call
        /// Keyboard.GetState() the old state is useless (see XNA help for more
        /// information, section Input). We store our own array of keys from
        /// the last frame for comparing stuff.
        /// </summary>
        private static KeyboardState keyboardState =
            Microsoft.Xna.Framework.Input.Keyboard.GetState();

        /// <summary>
        /// Keys pressed last frame, for comparison if a key was just pressed.
        /// </summary>
        private static List<Keys> keysPressedLastFrame = new List<Keys>();

        /// <summary>
        /// GamePad state, set every frame in the Update method.
        /// </summary>
        private static GamePadState gamePadState, gamePadStateLastFrame;

        /// <summary>
        /// Mouse wheel delta this frame. XNA does report only the total
        /// scroll value, but we usually need the current delta!
        /// </summary>
        /// <returns>0</returns>
        private static int mouseWheelDelta = 0;
#if !XBOX360
        private static int mouseWheelValue = 0;
#endif

        /// <summary>
        /// Start dragging pos, will be set when we just pressed the left
        /// mouse button. Used for the MouseDraggingAmount property.
        /// </summary>
        private static Point startDraggingPos;
        #endregion

        #region Mouse Properties
        /// <summary>
        /// Was a mouse detected? Returns true if the user moves the mouse.
        /// On the Xbox 360 there will be no mouse movement and theirfore we
        /// know that we don't have to display the mouse.
        /// </summary>
        /// <returns>Bool</returns>
        public static bool MouseDetected
        {
            get
            {
                return mouseDetected;
            }
        }

        /// <summary>
        /// Mouse position
        /// </summary>
        /// <returns>Point</returns>
        public static Point MousePos
        {
            get
            {
#if !XBOX360
                return new Point(mouseState.X, mouseState.Y);
#else
                return Point.Zero;
#endif
            }
        }

        /// <summary>
        /// X and y movements of the mouse this frame
        /// </summary>
#if !XBOX360
        private static float mouseXMovement, mouseYMovement;
        private static float lastMouseXMovement, lastMouseYMovement;
#endif

        /// <summary>
        /// Mouse x movement
        /// </summary>
        /// <returns>Float</returns>
        public static float MouseXMovement
        {
            get
            {
#if !XBOX360
                return mouseXMovement;
#else
                return 0;
#endif
            }
        }

        /// <summary>
        /// Mouse y movement
        /// </summary>
        /// <returns>Float</returns>
        public static float MouseYMovement
        {
            get
            {
#if !XBOX360
                return mouseYMovement;
#else
                    return 0;
#endif
            }
        }

        /// <summary>
        /// Mouse has moved in either the X or Y direction
        /// </summary>
        /// <returns>Boolean</returns>
        public static bool HasMouseMoved
        {
            get
            {
#if !XBOX360
                //TODO: Introduce a mouse movement threshold constant
                if (MouseXMovement > 1 || MouseYMovement > 1)
                    return true;
#endif
                return false;
            }
        }

        /// <summary>
        /// Mouse left button pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool MouseLeftButtonPressed
        {
            get
            {
#if !XBOX360
                return mouseState.LeftButton == ButtonState.Pressed;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Mouse right button pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool MouseRightButtonPressed
        {
            get
            {
#if !XBOX360
                return mouseState.RightButton == ButtonState.Pressed;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Mouse middle button pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool MouseMiddleButtonPressed
        {
            get
            {
#if !XBOX360
                return mouseState.MiddleButton == ButtonState.Pressed;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Mouse left button just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool MouseLeftButtonJustPressed
        {
            get
            {
#if !XBOX360
                return mouseState.LeftButton == ButtonState.Pressed &&
                       mouseStateLastFrame.LeftButton == ButtonState.Released;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Mouse right button just pressed
        /// </summary>
        /// <returns>Bool</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Makes this class reuseable.")]
        public static bool MouseRightButtonJustPressed
        {
            get
            {
#if !XBOX360
                return mouseState.RightButton == ButtonState.Pressed &&
                       mouseStateLastFrame.RightButton == ButtonState.Released;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Mouse dragging amount
        /// </summary>
        /// <returns>Point</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Makes this class reuseable.")]
        public static Point MouseDraggingAmount
        {
            get
            {
                return new Point(
                    startDraggingPos.X - MousePos.X,
                    startDraggingPos.Y - MousePos.Y);
            }
        }

        /// <summary>
        /// Reset mouse dragging amount
        /// </summary>
        public static void ResetMouseDraggingAmount()
        {
            startDraggingPos = MousePos;
        }

        /// <summary>
        /// Mouse wheel delta
        /// </summary>
        /// <returns>Int</returns>
        public static int MouseWheelDelta
        {
            get
            {
                return mouseWheelDelta;
            }
        }

        /// <summary>
        /// Mouse in box
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <returns>Bool</returns>
        public static bool MouseInBox(Rectangle rect)
        {
#if !XBOX360
            bool ret = mouseState.X >= rect.X &&
                       mouseState.Y >= rect.Y &&
                       mouseState.X < rect.Right &&
                       mouseState.Y < rect.Bottom;
            bool lastRet = mouseStateLastFrame.X >= rect.X &&
                           mouseStateLastFrame.Y >= rect.Y &&
                           mouseStateLastFrame.X < rect.Right &&
                           mouseStateLastFrame.Y < rect.Bottom;

            // Highlight happend?
            if (ret &&
                lastRet == false)
                Sound.Play(Sound.Sounds.Highlight);

            return ret;
#else
            return false;
#endif
        }

        /// <summary>
        /// Mouse in box relative
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <returns>Bool</returns>
        public static bool MouseInBoxRelative(Rectangle rect)
        {
            float widthFactor = BaseGame.Width / 1024.0f;
            float heightFactor = BaseGame.Height / 640.0f;
            return MouseInBox(new Rectangle(
                (int)Math.Round(rect.X * widthFactor),
                (int)Math.Round(rect.Y * heightFactor),
                (int)Math.Round(rect.Right * widthFactor),
                (int)Math.Round(rect.Bottom * heightFactor)));
        }
        #endregion

        #region Keyboard Properties
        /// <summary>
        /// Keyboard
        /// </summary>
        /// <returns>Keyboard state</returns>
        public static KeyboardState Keyboard
        {
            get
            {
                return keyboardState;
            }
        }

        public static bool IsSpecialKey(Keys key)
        {
            // All keys except A-Z, 0-9 and `-\[];',./= (and space) are special keys.
            // With shift pressed this also results in this keys:
            // ~_|{}:"<>? !@#$%^&*().
            int keyNum = (int)key;
            if ((keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z) ||
                (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9) ||
                key == Keys.Space || // well, space ^^
                key == Keys.OemTilde || // `~
                key == Keys.OemMinus || // -_
                key == Keys.OemPipe || // \|
                key == Keys.OemOpenBrackets || // [{
                key == Keys.OemCloseBrackets || // ]}
                key == Keys.OemQuotes || // '"
                key == Keys.OemQuestion || // /?
                key == Keys.OemPlus) // =+
            {
                return false;
            }

            // Else is is a special key
            return true;
        }

        /// <summary>
        /// Key to char helper conversion method.
        /// Note: If the keys are mapped other than on a default QWERTY
        /// keyboard, this method will not work properly. Most keyboards
        /// will return the same for A-Z and 0-9, but the special keys
        /// might be different.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Char</returns>
        public static char KeyToChar(Keys key, bool shiftPressed)
        {
            // If key will not be found, just return space
            char ret = ' ';
            int keyNum = (int)key;
            if (keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z)
            {
                if (shiftPressed)
                    ret = key.ToString()[0];
                else
                    ret = key.ToString().ToLower()[0];
            }
            else if (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9 &&
                shiftPressed == false)
            {
                ret = (char)((int)'0' + (keyNum - Keys.D0));
            }
            else if (key == Keys.D1 && shiftPressed)
                ret = '!';
            else if (key == Keys.D2 && shiftPressed)
                ret = '@';
            else if (key == Keys.D3 && shiftPressed)
                ret = '#';
            else if (key == Keys.D4 && shiftPressed)
                ret = '$';
            else if (key == Keys.D5 && shiftPressed)
                ret = '%';
            else if (key == Keys.D6 && shiftPressed)
                ret = '^';
            else if (key == Keys.D7 && shiftPressed)
                ret = '&';
            else if (key == Keys.D8 && shiftPressed)
                ret = '*';
            else if (key == Keys.D9 && shiftPressed)
                ret = '(';
            else if (key == Keys.D0 && shiftPressed)
                ret = ')';
            else if (key == Keys.OemTilde)
                ret = shiftPressed ? '~' : '`';
            else if (key == Keys.OemMinus)
                ret = shiftPressed ? '_' : '-';
            else if (key == Keys.OemPipe)
                ret = shiftPressed ? '|' : '\\';
            else if (key == Keys.OemOpenBrackets)
                ret = shiftPressed ? '{' : '[';
            else if (key == Keys.OemCloseBrackets)
                ret = shiftPressed ? '}' : ']';
            else if (key == Keys.OemSemicolon)
                ret = shiftPressed ? ':' : ';';
            else if (key == Keys.OemQuotes)
                ret = shiftPressed ? '"' : '\'';
            else if (key == Keys.OemComma)
                ret = shiftPressed ? '<' : '.';
            else if (key == Keys.OemPeriod)
                ret = shiftPressed ? '>' : ',';
            else if (key == Keys.OemQuestion)
                ret = shiftPressed ? '?' : '/';
            else if (key == Keys.OemPlus)
                ret = shiftPressed ? '+' : '=';

            // Return result
            return ret;
        }

        /// <summary>
        /// Handle keyboard input helper method to catch keyboard input
        /// for an input text. Only used to enter the player name in the game.
        /// </summary>
        /// <param name="inputText">Input text</param>
        public static void HandleKeyboardInput(ref string inputText)
        {
            // Is a shift key pressed (we have to check both, left and right)
            bool isShiftPressed =
                keyboardState.IsKeyDown(Keys.LeftShift) ||
                keyboardState.IsKeyDown(Keys.RightShift);

            // Go through all pressed keys
            foreach (Keys pressedKey in keyboardState.GetPressedKeys())
                // Only process if it was not pressed last frame
                if (keysPressedLastFrame.Contains(pressedKey) == false)
                {
                    // No special key?
                    if (IsSpecialKey(pressedKey) == false &&
                        // Max. allow 32 chars
                        inputText.Length < 32)
                    {
                        // Then add the letter to our inputText.
                        // Check also the shift state!
                        inputText += KeyToChar(pressedKey, isShiftPressed);
                    }
                    else if (pressedKey == Keys.Back &&
                        inputText.Length > 0)
                    {
                        // Remove 1 character at end
                        inputText = inputText.Substring(0, inputText.Length - 1);
                    }
                }
        }

        /// <summary>
        /// Keyboard key just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool KeyboardKeyJustPressed(Keys key)
        {
            return keyboardState.IsKeyDown(key) &&
                keysPressedLastFrame.Contains(key) == false;
        }

        /// <summary>
        /// Keyboard space just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool KeyboardSpaceJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Space) &&
                    keysPressedLastFrame.Contains(Keys.Space) == false;
            }
        }

        /// <summary>
        /// Keyboard F1 just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool KeyboardF1JustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.F1) &&
                    keysPressedLastFrame.Contains(Keys.F1) == false;
            }
        }

        /// <summary>
        /// Keyboard escape just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool KeyboardEscapeJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Escape) &&
                    keysPressedLastFrame.Contains(Keys.Escape) == false;
            }
        }

        /// <summary>
        /// Keyboard left just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool KeyboardLeftJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Left) &&
                    keysPressedLastFrame.Contains(Keys.Left) == false;
            }
        }

        /// <summary>
        /// Keyboard right just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool KeyboardRightJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Right) &&
                    keysPressedLastFrame.Contains(Keys.Right) == false;
            }
        }

        /// <summary>
        /// Keyboard up just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool KeyboardUpJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Up) &&
                    keysPressedLastFrame.Contains(Keys.Up) == false;
            }
        }

        /// <summary>
        /// Keyboard down just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool KeyboardDownJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Down) &&
                    keysPressedLastFrame.Contains(Keys.Down) == false;
            }
        }

        /// <summary>
        /// Keyboard left pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool KeyboardLeftPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Left);
            }
        }

        /// <summary>
        /// Keyboard right pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool KeyboardRightPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Right);
            }
        }

        /// <summary>
        /// Keyboard up pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool KeyboardUpPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Up);
            }
        }

        /// <summary>
        /// Keyboard down pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool KeyboardDownPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Down);
            }
        }
        #endregion

        #region GamePad Properties
        /// <summary>
        /// Game pad
        /// </summary>
        /// <returns>Game pad state</returns>
        public static GamePadState GamePad
        {
            get
            {
                return gamePadState;
            }
        }

        /// <summary>
        /// Is game pad connected
        /// </summary>
        /// <returns>Bool</returns>
        public static bool IsGamePadConnected
        {
            get
            {
                return gamePadState.IsConnected;
            }
        }

        /// <summary>
        /// Game pad start pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadStartPressed
        {
            get
            {
                return gamePadState.Buttons.Start == ButtonState.Pressed;
            }
        }

        /// <summary>
        /// Game pad a pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadAPressed
        {
            get
            {
                return gamePadState.Buttons.A == ButtonState.Pressed;
            }
        }

        /// <summary>
        /// Game pad b pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadBPressed
        {
            get
            {
                return gamePadState.Buttons.B == ButtonState.Pressed;
            }
        }

        /// <summary>
        /// Game pad x pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadXPressed
        {
            get
            {
                return gamePadState.Buttons.X == ButtonState.Pressed;
            }
        }

        /// <summary>
        /// Game pad y pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadYPressed
        {
            get
            {
                return gamePadState.Buttons.Y == ButtonState.Pressed;
            }
        }

        /// <summary>
        /// Game pad left pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadLeftPressed
        {
            get
            {
                return gamePadState.DPad.Left == ButtonState.Pressed ||
                    gamePadState.ThumbSticks.Left.X < -0.75f;
            }
        }

        /// <summary>
        /// Game pad right pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadRightPressed
        {
            get
            {
                return gamePadState.DPad.Right == ButtonState.Pressed ||
                    gamePadState.ThumbSticks.Left.X > 0.75f;
            }
        }

        /// <summary>
        /// Game pad left just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadLeftJustPressed
        {
            get
            {
                return (gamePadState.DPad.Left == ButtonState.Pressed &&
                    gamePadStateLastFrame.DPad.Left == ButtonState.Released) ||
                    (gamePadState.ThumbSticks.Left.X < -0.75f &&
                    gamePadStateLastFrame.ThumbSticks.Left.X > -0.75f);
            }
        }

        /// <summary>
        /// Game pad right just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadRightJustPressed
        {
            get
            {
                return (gamePadState.DPad.Right == ButtonState.Pressed &&
                    gamePadStateLastFrame.DPad.Right == ButtonState.Released) ||
                    (gamePadState.ThumbSticks.Left.X > 0.75f &&
                    gamePadStateLastFrame.ThumbSticks.Left.X < 0.75f);
            }
        }

        /// <summary>
        /// Game pad up just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadUpJustPressed
        {
            get
            {
                return (gamePadState.DPad.Up == ButtonState.Pressed &&
                    gamePadStateLastFrame.DPad.Up == ButtonState.Released) ||
                    (gamePadState.ThumbSticks.Left.Y > 0.75f &&
                    gamePadStateLastFrame.ThumbSticks.Left.Y < 0.75f);
            }
        }

        /// <summary>
        /// Game pad down just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadDownJustPressed
        {
            get
            {
                return (gamePadState.DPad.Down == ButtonState.Pressed &&
                    gamePadStateLastFrame.DPad.Down == ButtonState.Released) ||
                    (gamePadState.ThumbSticks.Left.Y < -0.75f &&
                    gamePadStateLastFrame.ThumbSticks.Left.Y > -0.75f);
            }
        }

        /// <summary>
        /// Game pad up pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadUpPressed
        {
            get
            {
                return gamePadState.DPad.Up == ButtonState.Pressed ||
                    gamePadState.ThumbSticks.Left.Y > 0.75f;
            }
        }

        /// <summary>
        /// Game pad down pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadDownPressed
        {
            get
            {
                return gamePadState.DPad.Down == ButtonState.Pressed ||
                    gamePadState.ThumbSticks.Left.Y < -0.75f;
            }
        }

        /// <summary>
        /// Game pad a just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadAJustPressed
        {
            get
            {
                return gamePadState.Buttons.A == ButtonState.Pressed &&
                    gamePadStateLastFrame.Buttons.A == ButtonState.Released;
            }
        }

        /// <summary>
        /// Game pad b just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadBJustPressed
        {
            get
            {
                return gamePadState.Buttons.B == ButtonState.Pressed &&
                    gamePadStateLastFrame.Buttons.B == ButtonState.Released;
            }
        }

        /// <summary>
        /// Game pad x just pressed
        /// </summary>
        /// <returns>Bool</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Makes this class reuseable.")]
        public static bool GamePadXJustPressed
        {
            get
            {
                return gamePadState.Buttons.X == ButtonState.Pressed &&
                    gamePadStateLastFrame.Buttons.X == ButtonState.Released;
            }
        }

        /// <summary>
        /// Game pad y just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadYJustPressed
        {
            get
            {
                return gamePadState.Buttons.Y == ButtonState.Pressed &&
                    gamePadStateLastFrame.Buttons.Y == ButtonState.Released;
            }
        }

        /// <summary>
        /// Game pad back just pressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadBackJustPressed
        {
            get
            {
                return gamePadState.Buttons.Back == ButtonState.Pressed &&
                    gamePadStateLastFrame.Buttons.Back == ButtonState.Released;
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Update, called from BaseGame.Update().
        /// Will catch all new states for keyboard, mouse and the gamepad.
        /// </summary>
        internal static void Update()
        {
#if XBOX360
            // No mouse support on the XBox360 yet :(
            mouseDetected = false;
#else
            // Handle mouse input variables
            mouseStateLastFrame = mouseState;
            mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            // Update mouseXMovement and mouseYMovement
            lastMouseXMovement += mouseState.X - mouseStateLastFrame.X;
            lastMouseYMovement += mouseState.Y - mouseStateLastFrame.Y;
            mouseXMovement = lastMouseXMovement / 2.0f;
            mouseYMovement = lastMouseYMovement / 2.0f;
            lastMouseXMovement -= lastMouseXMovement / 2.0f;
            lastMouseYMovement -= lastMouseYMovement / 2.0f;

            if (MouseLeftButtonPressed == false)
                startDraggingPos = MousePos;
            mouseWheelDelta = mouseState.ScrollWheelValue - mouseWheelValue;
            mouseWheelValue = mouseState.ScrollWheelValue;

            // If we are in the game and don't show the mouse cursor anyway,
            // reset it to the center to allow moving it around.
            if (RacingGameManager.InMenu == false &&
                // App must be active
                RacingGameManager.IsAppActive)
            {
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(
                    BaseGame.Width / 2, BaseGame.Height / 2);
                // Also use this for the current mouse pos for next frame,
                // else the mouseXMovement is messed up!
                mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            }

            // Check if mouse was moved this frame if it is not detected yet.
            // This allows us to ignore the mouse even when it is captured
            // on a windows machine if just the gamepad or keyboard is used.
            if (mouseDetected == false)// &&
                //always returns false: Microsoft.Xna.Framework.Input.Mouse.IsCaptured)
                mouseDetected = mouseState.X != mouseStateLastFrame.X ||
                    mouseState.Y != mouseStateLastFrame.Y ||
                    mouseState.LeftButton != mouseStateLastFrame.LeftButton;
#endif

            // Handle keyboard input
            keysPressedLastFrame = new List<Keys>(keyboardState.GetPressedKeys());
            keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            // And finally catch the XBox Controller input (only use 1 player here)
            gamePadStateLastFrame = gamePadState;
            gamePadState =
                Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.One);
        }
        #endregion
    }
}
