#region File Description
//-----------------------------------------------------------------------------
// TextDisplayComponent.cs
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


#endregion

namespace NinjAcademy
{
    /// <summary>
    /// A component used to display text at a specified location
    /// </summary>
    class TextDisplayComponent : RestorableStateComponent
    {
        #region Fields/Properties


        SpriteBatch spriteBatch;
        SpriteFont font;

        /// <summary>
        /// The text which the component will display.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The color to render the text with.
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// The position where the component will display text.
        /// </summary>
        public Vector2 Position { get; set; }        


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new instance of the component.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="font">Font for displaying the score.</param>        
        public TextDisplayComponent(Game game, SpriteFont font)
            : base(game)
        {
            this.font = font;

            spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            TextColor = Color.White;
        }


        #endregion

        #region Rendering


        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            spriteBatch.DrawString(font, Text, Position, TextColor);

            spriteBatch.End();
        }


        #endregion
    }
}
