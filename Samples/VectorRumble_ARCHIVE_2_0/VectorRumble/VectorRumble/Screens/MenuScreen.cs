#region File Description
//-----------------------------------------------------------------------------
// MenuScreen.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    /// <remarks>Based on a class in the Game State Management sample.</remarks>
    abstract class MenuScreen : GameScreen
    {
        #region Fields

        List<MenuEntry> menuEntries = new List<MenuEntry>();
        int selectedEntry = 0;
        AudioManager audioManager;

        #endregion

        #region Properties


        /// <summary>
        /// Gets the list of menu entries, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<MenuEntry> MenuEntries
        {
            get { return menuEntries; }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.0);
            TransitionOffTime = TimeSpan.FromSeconds(1.0);

        }


        /// <summary>
        /// Load all content.
        /// </summary>
        public override void LoadContent()
        {
            // retrieve the audio manager, done here in lieu of Initialize
            audioManager = (AudioManager)ScreenManager.Game.Services.GetService(
                                                                typeof(AudioManager));
            base.LoadContent();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            // Move to the previous menu entry?
            if (input.MenuUp)
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = menuEntries.Count - 1;

                audioManager.PlayCue("menuMove");
            }

            // Move to the next menu entry?
            if (input.MenuDown)
            {
                selectedEntry++;

                if (selectedEntry >= menuEntries.Count)
                    selectedEntry = 0;

                audioManager.PlayCue("menuMove");
            }

            // Accept or cancel the menu?
            if (input.MenuSelect)
            {
                OnSelectEntry(selectedEntry);

                audioManager.PlayCue("menuSelect");
            }
            else if (input.MenuCancel)
            {
                OnCancel();
            }
        }


        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex)
        {
            menuEntries[selectedEntry].OnSelectEntry();
        }


        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel()
        {
            ExitScreen();
        }


        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, EventArgs e)
        {
            OnCancel();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == selectedEntry);

                menuEntries[i].Update(this, isSelected, gameTime);
            }
        }


        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);

            Vector2 position = new Vector2(0f, viewportSize.Y * 0.55f);

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            if (ScreenState == ScreenState.TransitionOn)
                position.Y += transitionOffset * 256;
            else
                position.Y += transitionOffset * 512;

            // Draw each menu entry in turn.
            spriteBatch.Begin();

            for (int i = 0; i < menuEntries.Count; i++)
            {
                Color color = Color.White;
                float scale = 1.0f;

                if (IsActive && (i == selectedEntry))
                {
                    // The selected entry is yellow, and has an animating size.
                    double time = gameTime.TotalGameTime.TotalSeconds;
                    float pulsate = (float)Math.Sin(time * 6f) + 1f;

                    color = Color.Orange;
                    scale += pulsate * 0.05f;
                }

                // Modify the alpha to fade text out during transitions.
                color = new Color(color.R, color.G, color.B, TransitionAlpha);

                // Draw text, centered on the middle of each line.
                Vector2 origin = new Vector2(0, ScreenManager.Font.LineSpacing / 2);
                Vector2 size = ScreenManager.Font.MeasureString(menuEntries[i].Text);
                position.X = viewportSize.X / 2f - size.X / 2f * scale;
                spriteBatch.DrawString(ScreenManager.Font, menuEntries[i].Text,
                                                     position, color, 0, origin, scale,
                                                     SpriteEffects.None, 0);

                position.Y += ScreenManager.Font.LineSpacing;
            }

            spriteBatch.End();
        }


        #endregion
    }
}
