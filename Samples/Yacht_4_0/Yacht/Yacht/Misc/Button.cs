#region File Description
//-----------------------------------------------------------------------------
// Button.cs
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace Yacht
{
    /// <summary>
    /// Represents an on-screen button that the user can tap.
    /// </summary>
    class Button
    {
        #region Properties

        /// <summary>
        /// Position where the button should be drawn.
        /// </summary>
        public Rectangle Position { get; set; }

        /// <summary>
        /// Texture to use as the button's background.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Whether the button is enabled or not.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Event fired when the button is tapped, if it is enabled.
        /// </summary>
        public event EventHandler Click;

        private SpriteFont font;
        private string text;


        #endregion

        /// <summary>
        /// Initialize a new button.
        /// </summary>
        /// <param name="texture">The button's background.</param>
        /// <param name="position">The button's position on the screen.</param>
        /// <param name="font">the font used to draw the text on the button.</param>
        /// <param name="text">the text to draw on the string.</param>
        public Button(Texture2D texture, Vector2 position, SpriteFont font, string text) :
         this(texture, new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height), font, text)
        {}

        public Button(Texture2D texture, Rectangle position, SpriteFont font, string text)
        {
            Texture = texture;
            Position = position;
            Enabled = true;
            this.font = font;
            this.text = text;
        }

        #region Update and Render


        /// <summary>
        /// Handle input by raising the click event when the button is tapped.
        /// </summary>
        /// <param name="sample">The gesture that was input.</param>
        /// <returns>Return if the button was handled.</returns>
        public bool HandleInput(GestureSample sample)
        {
            if (Enabled && sample.GestureType == GestureType.Tap)
            {
                // Create the touch rectangle
                Rectangle touchRect = new Rectangle((int)sample.Position.X - 1, (int)sample.Position.Y - 1, 2, 2);

                // Create the button bounds rectangle
                Rectangle bounds = Texture.Bounds;
                bounds.X += (int)Position.X;
                bounds.Y += (int)Position.Y;

                // Check for intersection between the rectangles
                if (bounds.Intersects(touchRect) && Click != null)
                {
                    Click(this, EventArgs.Empty);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Draws the button on the screen.
        /// </summary>
        /// <param name="spriteBatch">A <see cref="SpriteBatch"/> to use when drawing the button.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Enabled ? Color.White : Color.Gray);
            if (!string.IsNullOrEmpty(text) && font != null)
            {
                DrawString(spriteBatch);
            }
        }

        private void DrawString(SpriteBatch spriteBatch)
        {
            Vector2 textSize = font.MeasureString(text);

            spriteBatch.DrawString(font, text, new Vector2(Position.X + Position.Width / 2 - textSize.X / 2,
                Position.Y + Position.Height / 2 - textSize.Y /2 - 5), Color.White);
        }


        #endregion
    }
}