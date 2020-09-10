#region File Information
//-----------------------------------------------------------------------------
// Label.cs
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
#endregion

namespace DynamicMenu.Controls
{
    /// <summary>
    /// A simple control which shows text centered in its bounds
    /// </summary>
    public class Label : TextControl
    {
        /// <summary>
        /// Draws the control, called once per frame
        /// </summary>
        /// <param name="gameTime">The current game time</param>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            DrawCenteredText(spriteBatch, Font, GetAbsoluteRect(), Text, TextColor);
        }
    }
}
