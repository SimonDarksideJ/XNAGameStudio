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

namespace Minjie
{
    /// <summary>
    /// Helper class represents a single entry in a MenuScreen. This also provides 
    /// an event that will be raised when the menu entry is selected.
    /// </summary>
    /// <remarks>Based on a similar class in the Game State Management sample.</remarks>
    class MenuEntry
    {
        #region Fields

        /// <summary>
        /// The texture rendered for this entry.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The origin of the texture.
        /// </summary>
        Vector2 origin;

        /// <summary>
        /// Tracks a fading selection effect on the entry.
        /// </summary>
        /// <remarks>
        /// The entries transition out of the selection effect when they are deselected.
        /// </remarks>
        float selectionFade;

        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the texture of this menu entry.
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
            set 
            {
                texture = value;
                if (texture == null)
                {
                    origin = Vector2.Zero;
                }
                else
                {
                    origin = new Vector2(texture.Width / 2, texture.Height / 2);
                }
            }
        }


        #endregion

        #region Events


        /// <summary>
        /// Event raised when the menu entry is selected.
        /// </summary>
        public event EventHandler<EventArgs> Selected;


        /// <summary>
        /// Method for raising the Selected event.
        /// </summary>
        protected internal virtual void OnSelectEntry()
        {
            if (Selected != null)
                Selected(this, EventArgs.Empty);
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new menu entry.
        /// </summary>
        public MenuEntry() { }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the menu entry.
        /// </summary>
        public virtual void Update(MenuScreen screen, bool isSelected,
                                                      GameTime gameTime)
        {
            // When the menu selection changes, entries gradually fade between
            // their selected and deselected appearance, rather than instantly
            // popping to the new state.
            float fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

            if (isSelected)
                selectionFade = Math.Min(selectionFade + fadeSpeed, 1);
            else
                selectionFade = Math.Max(selectionFade - fadeSpeed, 0);
        }


        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public virtual void Draw(MenuScreen screen, Vector2 position,
                                 bool isSelected, GameTime gameTime)
        {
            // Pulsate the size of the selected menu entry.
            double time = gameTime.TotalGameTime.TotalSeconds;
            
            float pulsate = (float)Math.Sin(time * 6) + 1;
            
            float scale = 0.7f + pulsate * 0.05f * selectionFade;

            // Draw texture, centered on the screen
            ScreenManager screenManager = screen.ScreenManager;
            SpriteBatch spriteBatch = screenManager.SpriteBatch;

            if (texture != null)
            {
                spriteBatch.Draw(texture, position, null, Color.White, 0f, origin, 
                    scale, SpriteEffects.None, 0);
            }
        }


        /// <summary>
        /// Queries how much space this menu entry requires.
        /// </summary>
        public virtual int GetHeight(MenuScreen screen)
        {
            return (int)Math.Floor(texture.Height * 0.8f);
        }


        #endregion
    }
}
