#region File Description
//-----------------------------------------------------------------------------
// PerformanceUtility.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;
using PerformanceUtility.GameDebugTools;
using System.Globalization;

#endregion

namespace PerformanceUtility
{
    /// <summary>
    /// Sample program for Debug Components.
    /// </summary>
    public class PerformanceUtilityGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        // a blank 1x1 texture
        Texture2D blank;

        // our test sprite
        Texture2D cat;

        // Our debug system. We can keep this reference or use the DebugSystem.Instance
        // property once we've called DebugSystem.Initialize.
        DebugSystem debugSystem;

        // Position for debug command test.
        Vector2 debugPos = new Vector2(100, 100);

        // Stopwatch for TimeRuler test.
        Stopwatch stopwatch = new Stopwatch();

        // Variables for inputs.
        GamePadState padState;
        KeyboardState keyState;

        GamePadState prevPadState;
        KeyboardState prevKeyState;

        #endregion

        #region Initialization

        public PerformanceUtilityGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;
#endif
        }

        protected override void Initialize()
        {
            // initialize the debug system with the game and the name of the font 
            // we want to use for the debugging
            debugSystem = DebugSystem.Initialize(this, "Font");

            // register a new command that lets us move a sprite on the screen
            debugSystem.DebugCommandUI.RegisterCommand(
                "pos",              // Name of command
                "set position",     // Description of command
                PosCommand          // Command execution delegate
                );

            // we use flick and tap in order to bring down the command prompt and
            // open up the keyboard input panel, respectively.
            TouchPanel.EnabledGestures = GestureType.Flick | GestureType.Tap;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // create our SpriteBatch and load our font
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");

            // load our sprite
            cat = Content.Load<Texture2D>("cat");

            // create our blank texture
            blank = new Texture2D(GraphicsDevice, 1, 1);
            blank.SetData(new[] { Color.White });

            base.LoadContent();
        }

        #endregion

        #region Debug command test code.

        /// <summary>
        /// This method is called from DebugCommandHost when the user types the 'pos'
        /// command into the command prompt. This is registered with the command prompt
        /// through the DebugCommandUI.RegisterCommand method we called in Initialize.
        /// </summary>
        void PosCommand(IDebugCommandHost host, string command, IList<string> arguments)
        {
            // if we got two arguments from the command
            if (arguments.Count == 2)
            {
                // process text "pos xPos yPos" by parsing our two arguments
                debugPos.X = Single.Parse(arguments[0], CultureInfo.InvariantCulture);
                debugPos.Y = Single.Parse(arguments[1], CultureInfo.InvariantCulture);
            }
            else
            {
                // if we didn't get two arguments, we echo the current position of the cat
                host.Echo(String.Format("Pos={0},{1}", debugPos.X, debugPos.Y));
            }
        }

        #endregion

        #region Update and Draw

        protected override void Update(GameTime gameTime)
        {
            // tell the TimeRuler that we're starting a new frame. you always want
            // to call this at the start of Update
            debugSystem.TimeRuler.StartFrame();

            // Start measuring time for "Update".
            debugSystem.TimeRuler.BeginMark("Update", Color.Blue);

            HandleInput();
            HandleTouchInput();

            // Simulate game update by doing a busy loop for 1ms
            stopwatch.Reset();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < 1) ;

            // Update other components.
            base.Update(gameTime);

            // Stop measuring time for "Update".
            debugSystem.TimeRuler.EndMark("Update");
        }

        protected override void Draw(GameTime gameTime)
        {
            // Start measuring time for "Draw".
            debugSystem.TimeRuler.BeginMark("Draw", Color.Yellow);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // Show usage.
            string message =
                "A Button, A key: Show/Hide FPS Counter\n" +
                "B Button, B key: Show/Hide Time Ruler\n" +
                "X Button, X key: Show/Hide Time Ruler Log\n" +
                "Tab key, flick down: Open debug command UI\n" +
                "Tab key, flick up: Close debug command UI\n" +
                "Tap: Show keyboard input panel";

            Vector2 size = font.MeasureString(message);
            Layout layout = new Layout(GraphicsDevice.Viewport);

            float margin = font.LineSpacing;
            Rectangle rc = new Rectangle(0, 0,
                                    (int)(size.X + margin),
                                    (int)(size.Y + margin));

            // Compute boarder size, position.
            rc = layout.Place(rc, 0.01f, 0.01f, Alignment.TopRight);
            spriteBatch.Draw(blank, rc, Color.Black * .5f);

            // Draw usage message text.
            layout.ClientArea = rc;
            Vector2 pos = layout.Place(size, 0, 0, Alignment.Center);
            spriteBatch.DrawString(font, message, pos, Color.White);

            // Draw debug command test sprite.
            spriteBatch.Draw(cat, debugPos, Color.White);

            spriteBatch.End();

            // Draw other components.
            base.Draw(gameTime);

            // Stop measuring time for "Draw".
            debugSystem.TimeRuler.EndMark("Draw");
        }

        #endregion

        #region Handle Input

        void HandleInput()
        {
            padState = GamePad.GetState(PlayerIndex.One);
            keyState = Keyboard.GetState();

            // Handle exit game.
            if (padState.IsButtonDown(Buttons.Back) ||
                keyState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            // Ignore key input while DebugCommandUI shows up.
            if (debugSystem.DebugCommandUI.Focused)
                keyState = new KeyboardState();

            // Show/Hide FPS counter by press A button.
            if (IsButtonOrKeyPressed(Buttons.A, Keys.A))
            {
                debugSystem.FpsCounter.Visible = !debugSystem.FpsCounter.Visible;
            }

            // Show/Hide TimeRuler by press B button.
            if (IsButtonOrKeyPressed(Buttons.B, Keys.B))
            {
                debugSystem.TimeRuler.Visible = !debugSystem.TimeRuler.Visible;
            }

            // Show/Hide TimeRuler log by press X button.
            if (IsButtonOrKeyPressed(Buttons.X, Keys.X))
            {
                debugSystem.TimeRuler.Visible = true;
                debugSystem.TimeRuler.ShowLog = !debugSystem.TimeRuler.ShowLog;
            }

            // Copy current input state to previous input state.
            prevPadState = padState;
            prevKeyState = keyState;
        }

        bool IsButtonOrKeyPressed(Buttons button, Keys key)
        {
            return (padState.IsButtonDown(button) && prevPadState.IsButtonUp(button)) ||
                (keyState.IsKeyDown(key) && prevKeyState.IsKeyUp(key));
        }

        void HandleTouchInput()
        {
            // Read Gestures.
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                switch (gesture.GestureType)
                {
                    case GestureType.Tap:
                        // if the user tapped and the debug command UI is focused, 
                        // show the keyboard input
                        if (debugSystem.DebugCommandUI.Focused && !Guide.IsVisible)
                        {
                            Guide.BeginShowKeyboardInput(
                                PlayerIndex.One,
                                "Input Debug Command",
                                "type debug command\n"+
                                    "'help' command shows available commands",
                                "",
                                InputDebugCommandCallback,
                                null);
                        }
                        break;
                    case GestureType.Flick:
                        // if the user flicked, we determine the angle of the flick and
                        // use that to either show or hide the debug command UI
                        if (gesture.Delta.Length() > 5)
                        {
                            const float cos30 = 0.87f;
                            Vector2 nd = Vector2.Normalize(gesture.Delta);

                            // Same as "Vector2.Dot(new Vector2(0,1), nd)" that 
                            // returns flick angle.
                            float dot = -nd.Y;  

                            if (debugSystem.DebugCommandUI.Focused)
                            {
                                if (dot > cos30)
                                    debugSystem.DebugCommandUI.Hide();
                            }
                            else
                            {
                                if (dot < -cos30)
                                    debugSystem.DebugCommandUI.Show();
                            }
                        }
                        break;
                }
            }
        }

        // Invoked after the user inputs text from the on screen keyboard
        private void InputDebugCommandCallback(IAsyncResult result)
        {
            // get the string they entered
            string cmd = Guide.EndShowKeyboardInput(result);

            // if they entered something, execute the command
            if (!string.IsNullOrEmpty(cmd))
                debugSystem.DebugCommandUI.ExecuteCommand(cmd);
        }

        #endregion

    }
}
