#region File Description
//-----------------------------------------------------------------------------
// Button.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManagementSample
{
    /// <summary>
    /// A special button that handles toggling between "On" and "Off"
    /// </summary>
    class BooleanButton : Button
    {
        private string option;
        private bool value;

        /// <summary>
        /// Creates a new BooleanButton.
        /// </summary>
        /// <param name="option">The string text to display for the option.</param>
        /// <param name="value">The initial value of the button.</param>
        public BooleanButton(string option, bool value)
            : base(option)
        {
            this.option = option;
            this.value = value;

            GenerateText();
        }

        protected override void OnTapped()
        {
            // When tapped we need to toggle the value and regenerate the text
            value = !value;
            GenerateText();

            base.OnTapped();
        }

        /// <summary>
        /// Helper that generates the actual Text value the base class uses for drawing.
        /// </summary>
        private void GenerateText()
        {
            Text = string.Format("{0}: {1}", option, value ? "On" : "Off");
        }
    }

    /// <summary>
    /// Represents a touchable button.
    /// </summary>
    class Button
    {
        /// <summary>
        /// The text displayed in the button.
        /// </summary>
        public string Text = "Button";

        /// <summary>
        /// The position of the top-left corner of the button.
        /// </summary>
        public Vector2 Position = Vector2.Zero;

        /// <summary>
        /// The size of the button.
        /// </summary>
        public Vector2 Size = new Vector2(250, 75);

        /// <summary>
        /// The thickness of the border drawn for the button.
        /// </summary>
        public int BorderThickness = 4;

        /// <summary>
        /// The color of the button border.
        /// </summary>
        public Color BorderColor = new Color(200, 200, 200);

        /// <summary>
        /// The color of the button background.
        /// </summary>
        public Color FillColor = new Color(100, 100, 100) * .75f;

        /// <summary>
        /// The color of the text.
        /// </summary>
        public Color TextColor = Color.White;

        /// <summary>
        /// The opacity of the button.
        /// </summary>
        public float Alpha = 0f;

        /// <summary>
        /// Invoked when the button is tapped.
        /// </summary>
        public event EventHandler<EventArgs> Tapped;
        
        /// <summary>
        /// Creates a new Button.
        /// </summary>
        /// <param name="text">The text to display in the button.</param>
        public Button(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Invokes the Tapped event and allows subclasses to perform actions when tapped.
        /// </summary>
        protected virtual void OnTapped()
        {
            if (Tapped != null)
                Tapped(this, EventArgs.Empty);
        }

        /// <summary>
        /// Passes a tap location to the button for handling.
        /// </summary>
        /// <param name="tap">The location of the tap.</param>
        /// <returns>True if the button was tapped, false otherwise.</returns>
        public bool HandleTap(Vector2 tap)
        {
            if (tap.X >= Position.X &&
                tap.Y >= Position.Y &&
                tap.X <= Position.X + Size.X &&
                tap.Y <= Position.Y + Size.Y)
            {
                OnTapped();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Draws the button
        /// </summary>
        /// <param name="screen">The screen drawing the button</param>
        public void Draw(GameScreen screen)
        {
            // Grab some common items from the ScreenManager
            SpriteBatch spriteBatch = screen.ScreenManager.SpriteBatch;
            SpriteFont font = screen.ScreenManager.Font;
            Texture2D blank = screen.ScreenManager.BlankTexture;

            // Compute the button's rectangle
            Rectangle r = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)Size.X,
                (int)Size.Y);

            // Fill the button
            spriteBatch.Draw(blank, r, FillColor * Alpha);

            // Draw the border
            spriteBatch.Draw(
                blank, 
                new Rectangle(r.Left, r.Top, r.Width, BorderThickness),
                BorderColor * Alpha);
            spriteBatch.Draw(
                blank, 
                new Rectangle(r.Left, r.Top, BorderThickness, r.Height),
                BorderColor * Alpha);
            spriteBatch.Draw(
                blank, 
                new Rectangle(r.Right - BorderThickness, r.Top, BorderThickness, r.Height),
                BorderColor * Alpha);
            spriteBatch.Draw(
                blank, 
                new Rectangle(r.Left, r.Bottom - BorderThickness, r.Width, BorderThickness),
                BorderColor * Alpha);

            // Draw the text centered in the button
            Vector2 textSize = font.MeasureString(Text);
            Vector2 textPosition = new Vector2(r.Center.X, r.Center.Y) - textSize / 2f;
            textPosition.X = (int)textPosition.X;
            textPosition.Y = (int)textPosition.Y;
            spriteBatch.DrawString(font, Text, textPosition, TextColor * Alpha);
        }
    }
}
