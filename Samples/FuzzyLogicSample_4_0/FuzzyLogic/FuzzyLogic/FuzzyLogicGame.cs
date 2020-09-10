#region File Description
//-----------------------------------------------------------------------------
// FuzzyLogicGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace FuzzyLogic
{
    /// <summary>
    /// This sample shows how an AI can use fuzzy logic to make decisions. It also 
    /// demonstrates a method for organizing different AI behaviors, similar to a state
    /// machine.
    /// </summary>
    public class FuzzyLogicGame : Microsoft.Xna.Framework.Game
    {
        #region Constants

        // This value controls the number of mice that will be in the game. Try 
        // increasing this value! Lots of mice can be fun to watch.
        const int NumberOfMice = 15;

        #endregion

        #region Fields

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        KeyboardState currentKeyboardState;
        GamePadState currentGamePadState;
        KeyboardState lastKeyboardState;
        GamePadState lastGamePadState;

        // The game will keep track of a tank and some mice, which are represented
        // by these two variables.
        Tank tank;
        List<Mouse> mice = new List<Mouse>();

        // This texture is a 1x1 white dot, just like the name suggests. by stretching
        // it, we can use it to draw the bar graph that will show the tank's fuzzy
        // weights.
        Texture2D onePixelWhite;

        // Tells us which of the three fuzzy weights the user is currently modifying.
        // the currently selected weight will have a pulsing red tint.
        int currentlySelectedWeight;

        // Definte the dimensions of the fuzzy logic bars
        Rectangle barDistance = new Rectangle(105, 45, 85, 40);
        Rectangle barAngle = new Rectangle(105, 125, 85, 40);
        Rectangle barTime = new Rectangle(105, 205, 85, 40);

        Rectangle levelBoundary;

        Vector2 lastTouchPoint;
        bool isDragging;

        #endregion

        #region Initialization

        public FuzzyLogicGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;            

#if WINDOWS_PHONE
            
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.IsFullScreen = true;
#endif
        }

        protected override void Initialize()
        {
            // The level boundary is the viewable area but slightly
            // smaller to prevent the Entities from drawing off-screen
            levelBoundary = GraphicsDevice.Viewport.TitleSafeArea;
            levelBoundary.X += 20;
            levelBoundary.Y += 20;
            levelBoundary.Width -= 40;
            levelBoundary.Height -= 40;

            // Now that we've created the graphics device, we can use its title 
            // safe area to create the tank.
            tank = new Tank(levelBoundary, mice);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            tank.LoadContent(Content);
            onePixelWhite = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            onePixelWhite.SetData<Color>(new Color[] { Color.White });
            font = Content.Load<SpriteFont>("hudFont");
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            tank.Update(gameTime);

            // Update all of the mice
            int i = 0;
            while (i < mice.Count)
            {
                mice[i].Update(gameTime);

                // If the tank has caught any of the mice, remove them from the list. 
                // who knows what happen to the mice after they're caught? Whatever it
                // is, it probably isn't pretty.
                if (Vector2.Distance(tank.Position, mice[i].Position) <
                    Tank.CaughtDistance)
                {
                    mice.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            // Now, if the tank has caught any mice, we'll have fewer than our desired
            // number, and we have to repopulate.
            while (mice.Count < NumberOfMice)
            {
                Mouse mouse = new Mouse(levelBoundary, tank);
                mouse.LoadContent(Content);
                mice.Add(mouse);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);

            spriteBatch.Begin();

            foreach (Mouse mouse in mice)
            {
                mouse.Draw(spriteBatch, gameTime);
            }

            tank.Draw(spriteBatch, gameTime);

            // Draw the three bars showing the tank's internal state.
            DrawBar(barDistance, tank.FuzzyDistanceWeight, "Distance", gameTime, currentlySelectedWeight == 0);
            DrawBar(barAngle, tank.FuzzyAngleWeight, "Angle", gameTime, currentlySelectedWeight == 1);
            DrawBar(barTime, tank.FuzzyTimeWeight, "Time", gameTime, currentlySelectedWeight == 2);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// DrawBar is a helper function used by Draw. It is used to draw the three
        /// bars which display the tank's fuzzy weights.
        /// </summary>
        private void DrawBar(Rectangle bar, float barWidthNormalized, string label, GameTime gameTime, bool highlighted)
        {
            Color tintColor = Color.White;

            // if the bar is highlighted, we want to make it pulse with a red tint.
            if (highlighted)
            {
                // to do this, we'll first generate a value t, which we'll use to
                // determine how much tint to have.
                float t = (float)Math.Sin(10 * gameTime.TotalGameTime.TotalSeconds);

                // Sin varies from -1 to 1, and we want t to go from 0 to 1, so we'll 
                // scale it now.
                t = .5f + .5f * t;

                // finally, we'll calculate our tint color by using Lerp to generate
                // a color in between Red and White.
                tintColor = new Color(Vector4.Lerp(
                    Color.Red.ToVector4(), Color.White.ToVector4(), t));
            }

            // calculate how wide the bar should be, and then draw it.
            bar.Width = (int)(bar.Width * barWidthNormalized);
            spriteBatch.Draw(onePixelWhite, bar, tintColor);

            // finally, draw the label to the left of the bar.
            Vector2 labelSize = font.MeasureString(label);
            Vector2 labelPosition = new Vector2(bar.X - 5 - labelSize.X, bar.Y);
            spriteBatch.DrawString(font, label, labelPosition, tintColor);
        }

        #endregion

        #region Handle Input

        bool IsPressed(Keys key)
        {
            return (currentKeyboardState.IsKeyUp(key) &&
                lastKeyboardState.IsKeyDown(key));
        }

        bool IsPressed(Buttons button)
        {
            return (currentGamePadState.IsButtonUp(button) &&
                lastGamePadState.IsButtonDown(button));
        }


        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        void HandleInput()
        {
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
            currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            // Check to see whether the user wants to modify their currently selected
            // weight.
            if (IsPressed(Keys.Up) || IsPressed(Buttons.DPadUp) ||
                IsPressed(Buttons.LeftThumbstickUp))
            {
                currentlySelectedWeight--;
                if (currentlySelectedWeight < 0)
                    currentlySelectedWeight = 2;
            }

            if (IsPressed(Keys.Down) || IsPressed(Buttons.DPadDown) ||
                IsPressed(Buttons.LeftThumbstickDown))
            {
                currentlySelectedWeight = (currentlySelectedWeight + 1) % 3;
            }

            // Figure out how much the user wants to change the current weight, if at 
            // all. the input thumbsticks vary from -1 to 1, which is too much all at 
            // once, so we'll scale it down a bit.
            float changeAmount = currentGamePadState.ThumbSticks.Left.X;

            TouchCollection touchState = TouchPanel.GetState();

            // Interpert touch screen presses - get only the first one for this specific case
            if (touchState.Count > 0)
            {
                TouchLocation location = touchState[0];

                switch (location.State)
                {
                    case TouchLocationState.Pressed:
                        // Save first touch coordinates
                        lastTouchPoint = location.Position;

                        isDragging = true;

                        // Create a rectangle for the touch point
                        Rectangle touch = new Rectangle((int)lastTouchPoint.X, (int)lastTouchPoint.Y, 20, 20);

                        // Check for collision with the bars
                        if (barDistance.Intersects(touch))
                            currentlySelectedWeight = 0;
                        else if (barAngle.Intersects(touch))
                            currentlySelectedWeight = 1;
                        else if (barTime.Intersects(touch))
                            currentlySelectedWeight = 2;

                        changeAmount = 0;
                        break;
                    case TouchLocationState.Moved:
                        if (isDragging && currentlySelectedWeight > -1)
                        {
                            float DragDelta = location.Position.X - lastTouchPoint.X;

                            if (DragDelta > 0)
                                changeAmount = 1;
                            else if (DragDelta < 0)
                                changeAmount = -1.0f;
                        }
                        break;
                    case TouchLocationState.Released:
                        // Make coordinates irrelevant
                        if (isDragging)
                        {
                            lastTouchPoint.X = -1;
                            lastTouchPoint.Y = -1;
                            isDragging = false;
                        }
                        break;
                }
            }

            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentGamePadState.IsButtonDown(Buttons.DPadRight))
            {
                changeAmount = 1;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentGamePadState.IsButtonDown(Buttons.DPadLeft))
            {
                changeAmount = -1f;
            }
            changeAmount *= .025f;

            // Apply to the changeAmount to the currentlySelectedWeight
            switch (currentlySelectedWeight)
            {
                case 0:
                    tank.FuzzyDistanceWeight += changeAmount;
                    break;
                case 1:
                    tank.FuzzyAngleWeight += changeAmount;
                    break;
                case 2:
                    tank.FuzzyTimeWeight += changeAmount;
                    break;
                default:
                    break;
            }

            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;
        }

        #endregion
    }
}
