#region File Description
//-----------------------------------------------------------------------------
// MenuEntry.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Pickture
{
    /// <summary>
    /// Helper class represents a single entry in a MenuScreen. 
    /// Draws an image instead of a line of text.
    /// </summary>
    class ImageMenuEntry : MenuEntry
    {
        #region Fields


        /// <summary>
        /// The image drawn for this menu entry.
        /// </summary>
        Texture2D texture;


        /// <summary>
        /// The origin of the texture, typically the center.
        /// </summary>
        Vector2 textureOrigin = Vector2.Zero;


        #endregion


        #region Properties


        /// <summary>
        /// Gets or sets the image drawn for this menu entry.
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
            set 
            { 
                texture = value;
                if (texture == null)
                {
                    textureOrigin = Vector2.Zero;
                }
                else
                {
                    textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
                }
            }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new ImageMenuEntry object with the specified texture.
        /// </summary>
        public ImageMenuEntry(Texture2D texture) : base("ImageMenuEntry")
        {
            this.texture = texture;
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draws the ImageMenuEntry object.
        /// </summary>
        /// <remarks>Falls back to text display if the texture is null.</remarks>
        public override void Draw(MenuScreen screen, Vector2 position, 
                                  bool isSelected, GameTime gameTime)
        {
            if (texture == null)
            {
                base.Draw(screen, position, isSelected, gameTime);
            }
            else
            {
                // Pulsate the size of the selected menu entry.
                double time = gameTime.TotalGameTime.TotalSeconds;

                float pulsate = (float)Math.Sin(time * 6) + 1;

                float scale = 1 + pulsate * 0.05f * selectionFade;

                // Modify the alpha to fade text out during transitions.
                Color color = new Color(Vector4.One * 
                    (1.0f - screen.TransitionPosition));

                // Draw text, centered on the middle of each line.
                ScreenManager screenManager = screen.ScreenManager;
                SpriteBatch spriteBatch = screenManager.SpriteBatch;

                spriteBatch.Draw(texture, position, null, color, 0,
                                 textureOrigin, scale, SpriteEffects.None, 0);
            }
        }


        #endregion
    }
}
