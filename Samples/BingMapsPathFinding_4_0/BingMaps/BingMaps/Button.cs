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
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;


#endregion

namespace BingMaps
{
    class Button
    {
        #region Fields and Properties

        
        public Rectangle Bounds { get; set; }
        public Color BackgroundColor { get; set; }
        public Color TextColor { get; set; }
        public string Text { get; set; }
        public event EventHandler Click;

        Texture2D blank;
        SpriteFont font;
        SpriteBatch spriteBatch;


        #endregion

        #region Initialization        


        /// <summary>
        /// Creates a new button instance.
        /// </summary>
        /// <param name="text">The text that will appear at the center of the button.</param>
        /// <param name="font">Font used to write the button's text.</param>
        /// <param name="textColor">The color for the button's text.</param>
        /// <param name="bounds">The button's bounds.</param>
        /// <param name="backgroundColor">The button's background color.</param>
        /// <param name="texture">The button's texture.</param>
        /// <param name="spriteBatch">Sprite batch to use when rendering the button.</param>
        public Button(string text, SpriteFont font, Color textColor, Rectangle bounds, Color backgroundColor, 
            Texture2D texture, SpriteBatch spriteBatch)
        {
            Text = text;
            TextColor = textColor;
            Bounds = bounds;
            BackgroundColor = backgroundColor;            
            blank = texture;
            this.spriteBatch = spriteBatch;
            this.font = font;
        }


        #endregion

        #region Public Methods        


        /// <summary>
        /// Handles the user's taps on the button.
        /// </summary>
        /// <param name="sample">User's touch input.</param>
        /// <returns>True if the button was pressed and false otherwise.</returns>
        public bool HandleInput(GestureSample sample)
        {
            if (sample.GestureType == GestureType.Tap)
            {
                Rectangle touchRect = new Rectangle((int)sample.Position.X - 1, (int)sample.Position.Y - 1, 2, 2);
                if (Bounds.Intersects(touchRect) && Click != null)
                {
                    Click(this, EventArgs.Empty);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Renders the button. Assumes SpriteBatch.Begin has already been called.
        /// </summary>
        public void Draw()
        {            
            spriteBatch.Draw(blank, Bounds, BackgroundColor);

            Vector2 measure = font.MeasureString(Text);
            Vector2 position = new Vector2(Bounds.Center.X - measure.X / 2, Bounds.Center.Y - measure.Y / 2);
            spriteBatch.DrawString(font, Text, position, TextColor);
        }


        #endregion
    }
}
