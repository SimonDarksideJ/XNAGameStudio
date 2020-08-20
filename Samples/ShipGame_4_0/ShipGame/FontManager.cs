#region File Description
//-----------------------------------------------------------------------------
// FontManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ShipGame
{
    // supported font types and sizes
    public enum FontType
    {
        ArialSmall = 0,
        ArialMedium,
        ArialLarge
    };

    public class FontManager : IDisposable
    {
        GraphicsDevice graphics;    // graphics device
        SpriteBatch sprite;         // sprite bacth
        List<SpriteFont> fonts;     // list of sprite fonts
        bool textMode;              // in text mode?

        /// <summary>
        /// Create a new font manager
        /// </summary>
        public FontManager(GraphicsDevice gd)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            graphics = gd;
            sprite = new SpriteBatch(gd);
            fonts = new List<SpriteFont>();
            textMode = false;
        }

        /// <summary>
        /// Load resources
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            fonts.Add(content.Load<SpriteFont>("fonts/ArialS"));
            fonts.Add(content.Load<SpriteFont>("fonts/ArialM"));
            fonts.Add(content.Load<SpriteFont>("fonts/ArialL"));
        }

        /// <summary>
        /// Free resources
        /// </summary>
        public void UnloadContent()
        {
            fonts.Clear();
        }

        /// <summary>
        /// Get the current screen rectangle
        /// </summary>
        public Rectangle ScreenRectangle
        {
            get
            {
                return new Rectangle( graphics.Viewport.X, graphics.Viewport.Y,
                    graphics.Viewport.Width, graphics.Viewport.Height);
            }
        }

        /// <summary>
        /// Enter text mode
        /// </summary>
        public void BeginText()
        {
            sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            textMode = true;
        }

        /// <summary>
        /// Drawn text using given font, position and color
#endregion


        /// </summary>
        public void DrawText(FontType font, String text, Vector2 position, Color color)
        {
            if (textMode)
                sprite.DrawString(fonts[(int)font], text, position, color);
        }

        /// <summary>
        /// End text mode
        /// </summary>
        public void EndText()
        {
            sprite.End();
            textMode = false;
        }

        /// <summary>
        /// Draw a texture in screen
        /// </summary>
        public void DrawTexture(
            Texture2D texture, 
            Rectangle rect, 
            Color color,
            BlendState blend)
        {
            if (textMode)
                sprite.End();

            sprite.Begin(SpriteSortMode.Immediate, blend);
            sprite.Draw(texture, rect, color);
            sprite.End();

            if (textMode)
                sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }

        /// <summary>
        /// Draw a texture with rotation
        /// </summary>
        public void DrawTexture(
            Texture2D texture, 
            Rectangle rect, 
            float rotation, 
            Color color,
            BlendState blend)
        {
            if (textMode)
                sprite.End();

            rect.X += rect.Width / 2;
            rect.Y += rect.Height / 2;

            sprite.Begin(SpriteSortMode.Immediate, blend);
            sprite.Draw(texture, rect, null, color, rotation, 
                new Vector2(rect.Width/2, rect.Height/2), SpriteEffects.None, 0);
            sprite.End();

            if (textMode)
                sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }

        /// <summary>
        /// Draw a texture with source and destination rectangles
        /// </summary>
        public void DrawTexture(
            Texture2D texture, 
            Rectangle destinationRect, 
            Rectangle sourceRect, 
            Color color,
            BlendState blend)
        {
            if (textMode)
                sprite.End();

            sprite.Begin(SpriteSortMode.Immediate, blend);
            sprite.Draw(texture, destinationRect, sourceRect, color);
            sprite.End();

            if (textMode)
                sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }

        #region IDisposable Members

        bool isDisposed = false;
        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                if (sprite != null)
                {
                    sprite.Dispose();
                    sprite = null;
                }
            }
        }

        #endregion
    }
}
