#region File Description
//-----------------------------------------------------------------------------
// SimpleButton.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WP7MusicManagement
{
    /// <summary>
    /// A very basic button class.
    /// </summary>
    public class SimpleButton
    {
        /// <summary>
        /// The bounds of the button.
        /// </summary>
        public Rectangle Bounds { get; private set; }

        /// <summary>
        /// The text displayed in the button.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// An event that is invoked when the button is tapped.
        /// </summary>
        public event EventHandler<EventArgs> Tapped;

        /// <summary>
        /// Initializes a new SimpleButton.
        /// </summary>
        /// <param name="bounds">The bounds for the button.</param>
        /// <param name="text">The text displayed in the button.</param>
        public SimpleButton(Rectangle bounds, string text)
        {
            Bounds = bounds;
            Text = text;
        }

        /// <summary>
        /// Tests if a given tap location is in the button bounds and invokes the Tapped
        /// event if necessary.
        /// </summary>
        /// <param name="point">The location of the tap.</param>
        public void CheckForTap(Point point)
        {
            // If the point is in our bounds and our event is non-null, invoke the event.
            if (Bounds.Contains(point) && Tapped != null)
            {
                Tapped(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Draws the button.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch with which to draw the button.</param>
        /// <param name="blank">A blank texture for drawing the button background and border.</param>
        /// <param name="font">A font for drawing the button text.</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D blank, SpriteFont font)
        {
            // Draw the background of the button.
            spriteBatch.Draw(blank, Bounds, Color.Gray);

            // Draw a border around the button.
            spriteBatch.Draw(blank, new Rectangle(Bounds.Left, Bounds.Top, 1, Bounds.Height), Color.Black);
            spriteBatch.Draw(blank, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, 1), Color.Black);
            spriteBatch.Draw(blank, new Rectangle(Bounds.Left, Bounds.Bottom - 1, Bounds.Width, 1), Color.Black);
            spriteBatch.Draw(blank, new Rectangle(Bounds.Right - 1, Bounds.Top, 1, Bounds.Height), Color.Black);

            // Measure the text and center it in our bounds
            Vector2 textSize = font.MeasureString(Text);
            Vector2 pos = new Vector2(
                Bounds.Center.X - textSize.X / 2f, 
                Bounds.Center.Y - textSize.Y / 2f);

            // Cast the position to integers to avoid filtering the text. Not necessary, but it
            // generally makes text look nicer.
            pos.X = (int)pos.X;
            pos.Y = (int)pos.Y;

            // Draw the text.
            spriteBatch.DrawString(font, Text, pos, Color.White);
        }
    }
}
