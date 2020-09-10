#region File Description
//-----------------------------------------------------------------------------
// PhoneMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameStateManagementSample
{
    /// <summary>
    /// Provides a basic base screen for menus on Windows Phone leveraging the Button class.
    /// </summary>
    class PhoneMenuScreen : GameScreen
    {
        List<Button> menuButtons = new List<Button>();
        string menuTitle;

        InputAction menuCancel;

        /// <summary>
        /// Gets the list of buttons, so derived classes can add or change the menu contents.
        /// </summary>
        protected IList<Button> MenuButtons
        {
            get { return menuButtons; }
        }

        /// <summary>
        /// Creates the PhoneMenuScreen with a particular title.
        /// </summary>
        /// <param name="title">The title of the screen</param>
        public PhoneMenuScreen(string title)
        {
            menuTitle = title;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            // Create the menuCancel action
            menuCancel = new InputAction(new Buttons[] { Buttons.Back }, null, true);

            // We need tap gestures to hit the buttons
            EnabledGestures = GestureType.Tap;
        }

        public override void Activate(bool instancePreserved)
        {
            // When the screen is activated, we have a valid ScreenManager so we can arrange
            // our buttons on the screen
            float y = 140f;
            float center = ScreenManager.GraphicsDevice.Viewport.Bounds.Center.X;
            for (int i = 0; i < MenuButtons.Count; i++)
            {
                Button b = MenuButtons[i];

                b.Position = new Vector2(center - b.Size.X / 2, y);
                y += b.Size.Y * 1.5f;
            }

            base.Activate(instancePreserved);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // Update opacity of the buttons
            foreach (Button b in menuButtons)
            {
                b.Alpha = TransitionAlpha;
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// An overrideable method called whenever the menuCancel action is triggered
        /// </summary>
        protected virtual void OnCancel() { }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            // Test for the menuCancel action
            PlayerIndex player;
            if (menuCancel.Evaluate(input, ControllingPlayer, out player))
            {
                OnCancel();
            }

            // Read in our gestures
            foreach (GestureSample gesture in input.Gestures)
            {
                // If we have a tap
                if (gesture.GestureType == GestureType.Tap)
                {
                    // Test the tap against the buttons until one of the buttons handles the tap
                    foreach (Button b in menuButtons)
                    {
                        if (b.HandleTap(gesture.Position))
                            break;
                    }
                }
            }

            base.HandleInput(gameTime, input);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            spriteBatch.Begin();

            // Draw all of the buttons
            foreach (Button b in menuButtons)
                b.Draw(this);

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // Draw the menu title centered on the screen
            Vector2 titlePosition = new Vector2(graphics.Viewport.Width / 2, 80);
            Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
            Color titleColor = new Color(192, 192, 192) * TransitionAlpha;
            float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0, 
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
