//-----------------------------------------------------------------------------
// CommonGraphics.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace UserInterfaceSample
{
    public static class CommonGraphics
    {
        /// <summary>
        /// Draw a string centered in the given rectangle
        /// </summary>
        public static void DrawCenteredText(SpriteBatch batch, SpriteFont font, Rectangle rectangle, string text, Color color)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Vector2 size = font.MeasureString(text);
                Vector2 topLeft = new Vector2(rectangle.Center.X, rectangle.Center.Y) - size * 0.5f;
                batch.DrawString(font, text, topLeft, color);
            }
        }

        /// <summary>
        /// Draw the outline of a rectangle using the given SpriteBatch. The supplied texture should be
        /// a single-pixel blank white texture such as ScreenManager.BlankTexture. This function
        /// does not call Begin/End on the batch; you need to do that outside of this call.
        /// </summary>
        public static void DrawRectangle(SpriteBatch batch, Texture2D blankTexture, Rectangle rectangle, Color color)
        {
            DrawSpriteLine(batch, blankTexture, new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Top));
            DrawSpriteLine(batch, blankTexture, new Vector2(rectangle.Left, rectangle.Bottom), new Vector2(rectangle.Right, rectangle.Bottom));
            DrawSpriteLine(batch, blankTexture, new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Left, rectangle.Bottom));
            DrawSpriteLine(batch, blankTexture, new Vector2(rectangle.Right, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom));
        }

        private static void DrawSpriteLine(SpriteBatch batch, Texture2D blankTexture, Vector2 vector1, Vector2 vector2)
        {
            float distance = Vector2.Distance(vector1, vector2);
            float angle = (float)Math.Atan2((vector2.Y - vector1.Y), (vector2.X - vector1.X));

            // stretch the pixel between the two vectors
            batch.Draw(blankTexture,
                    vector1,
                    null,
                    Color.White,
                    angle,
                    Vector2.Zero,
                    new Vector2(distance, 1),
                    SpriteEffects.None,
                    0);
        }
    }
}
