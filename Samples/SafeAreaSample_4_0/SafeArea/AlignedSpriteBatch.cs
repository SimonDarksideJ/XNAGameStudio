#region File Description
//-----------------------------------------------------------------------------
// AlignedSpriteBatch.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SafeArea
{
    /// <summary>
    /// Flags enum defines the various ways a text string
    /// can be aligned relative to its specified position.
    /// </summary>
    [Flags]
    public enum Alignment
    {
        // Horizontal alignment options.
        Left = 0,
        Right = 1,
        HorizontalCenter = 2,

        // Vertical alignment options.
        Top = 0,
        Bottom = 4,
        VerticalCenter = 8,

        // Combined vertical + horizontal alignment options.
        TopLeft = Top | Left,
        TopRight = Top | Right,
        TopCenter = Top | HorizontalCenter,

        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right,
        BottomCenter = Bottom | HorizontalCenter,

        CenterLeft = VerticalCenter | Left,
        CenterRight = VerticalCenter | Right,
        Center = VerticalCenter | HorizontalCenter,
    }


    /// <summary>
    /// This class derives from the built in SpriteBatch, adding new
    /// logic for aligning text strings in more varied ways than just
    /// the default top left alignment.
    /// </summary>
    public class AlignedSpriteBatch : SpriteBatch
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public AlignedSpriteBatch(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        { }


        /// <summary>
        /// Draws a text string with the specified alignment.
        /// </summary>
        public void DrawString(SpriteFont spriteFont, string text,
                               Vector2 position, Color color, Alignment alignment)
        {
            // Compute horizontal alignment.
            if ((alignment & Alignment.Right) != 0)
            {
                position.X -= spriteFont.MeasureString(text).X;
            }
            else if ((alignment & Alignment.HorizontalCenter) != 0)
            {
                position.X -= spriteFont.MeasureString(text).X / 2;
            }

            // Compute vertical alignment.
            if ((alignment & Alignment.Bottom) != 0)
            {
                position.Y -= spriteFont.LineSpacing;
            }
            else if ((alignment & Alignment.VerticalCenter) != 0)
            {
                position.Y -= spriteFont.LineSpacing / 2;
            }

            // Draw the string.
            DrawString(spriteFont, text, position, color);
        }
    }
}
