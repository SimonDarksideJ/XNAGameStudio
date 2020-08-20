#region File Information
//-----------------------------------------------------------------------------
// ProgressBar.cs
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
#endregion

namespace DynamicMenu.Controls
{
    /// <summary>
    /// A control which shows a progress indicator 
    /// </summary>
    public class ProgressBar : Control
    {
        #region Properties 

        /// <summary>
        /// The name of the texture to use for the left side of the control
        /// </summary>
        public string LeftTextureName { get; set; }

        /// <summary>
        /// The loaded texture for the left side of the control
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D LeftTexture { get; set; }

        /// <summary>
        /// The name of the texture to use for the right side of the control
        /// </summary>
        public string RightTextureName { get; set; }

        /// <summary>
        /// The loaded texture for the right side of the control
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D RightTexture { get; set; }

        /// <summary>
        /// The current position for the progress indicator
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int Position { get; set; }

        /// <summary>
        /// The maximum value for the progress bar
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int MaxValue { get; set; }

        /// <summary>
        /// The color to apply to the left side of the control
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Color LeftColor { get; set; }

        /// <summary>
        /// The color to apply to the right side of the control
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Color RightColor { get; set; }

        /// <summary>
        /// The width of the border around the progress bar
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int BorderWidth { get; set; }

        #endregion

        #region Initialization

        public ProgressBar()
        {
            LeftColor = Color.White;
            RightColor = Color.White;

            Position = 0;
            MaxValue = 100;
            BorderWidth = 0;
        }

        /// <summary>
        /// Loads the control content
        /// </summary>
        public override void LoadContent(GraphicsDevice _graphics, ContentManager _content)
        {
            base.LoadContent(_graphics, _content);

            if (!string.IsNullOrEmpty(LeftTextureName))
            {
                LeftTexture = _content.Load<Texture2D>(LeftTextureName);
            }

            if (!string.IsNullOrEmpty(RightTextureName))
            {
                RightTexture = _content.Load<Texture2D>(RightTextureName);
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the control
        /// </summary>
        public override void Draw(GameTime _gameTime, SpriteBatch _spriteBatch)
        {
            base.Draw(_gameTime, _spriteBatch);

            int leftSideWidth = GetLeftSideWidth();

            Rectangle rect = GetAbsoluteRect();
            rect.Width = leftSideWidth;
            rect.X += BorderWidth;
            rect.Y += BorderWidth;
            rect.Height -= BorderWidth * 2;

            Rectangle srcRect = new Rectangle();
            srcRect.Width = rect.Width;
            srcRect.Height = rect.Height;

            if (LeftTexture != null)
            {
                _spriteBatch.Draw(LeftTexture, rect, srcRect, LeftColor);
            }

            rect = GetAbsoluteRect();
            rect.X += leftSideWidth + BorderWidth;
            rect.Width -= BorderWidth * 2 + leftSideWidth;
            rect.Y += BorderWidth;
            rect.Height -= BorderWidth * 2;

            srcRect = new Rectangle();
            srcRect.Width = rect.Width;
            srcRect.Height = rect.Height;
            srcRect.X = leftSideWidth;

            if (RightTexture != null)
            {
                _spriteBatch.Draw(RightTexture, rect, srcRect, RightColor);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the current width of the left side based on the position and max value
        /// </summary>
        private int GetLeftSideWidth()
        {
            float balance = (float)Position / (float)(MaxValue);
            int progressBarWidth = Width - BorderWidth * 2;

            int leftSideWidth = (int)Math.Floor((float)progressBarWidth * balance);

            return leftSideWidth;
        }

        #endregion
    }
}
