#region File Description
//-----------------------------------------------------------------------------
// TextureFontBigNumbers.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.Helpers;
using RacingGame.GameLogic;
using Microsoft.Xna.Framework.Input;
#endregion

namespace RacingGame.Graphics
{
    /// <summary>
    /// TextureFontBigNumbers
    /// </summary>
    public static class TextureFontBigNumbers
    {
        #region Constants
        /// <summary>
        /// Big numbers in the Ingame.png graphic
        /// </summary>
        private static readonly Rectangle[] BigNumberRects =
            {
                // 0
                new Rectangle(2, 342, 80, 133),
                // 1
                new Rectangle(84, 342, 80, 133),
                // 2
                new Rectangle(167, 342, 80, 133),
                // 3
                new Rectangle(247, 342, 78, 133),
                // 4
                new Rectangle(330, 342, 80, 133),
                // 5
                new Rectangle(411, 342, 80, 133),
                // 6
                new Rectangle(495, 342, 80, 133),
                // 7
                new Rectangle(578, 342, 80, 133),
                // 8
                new Rectangle(659, 342, 80, 133),
                // 9
                new Rectangle(749, 342, 80, 133),
            };
        #endregion

        #region Write number
        /// <summary>
        /// Write digit
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="digit">Digit</param>
        /// <returns>Int</returns>
        private static int WriteDigit(int x, int y, int digit)
        {
            if (digit < 0)
                return 0;

            float resScalingX = (float)BaseGame.Width / 1600.0f;
            float resScalingY = (float)BaseGame.Height / 1200.0f;

            Rectangle rect = BigNumberRects[digit % BigNumberRects.Length];
            BaseGame.UI.Ingame.RenderOnScreen(new Rectangle(x, y,
                (int)Math.Round(rect.Width * resScalingX),
                (int)Math.Round(rect.Height * resScalingY)), rect);

            return (int)Math.Round(rect.Width * resScalingX);
        }

        /// <summary>
        /// Write digit
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="height">Height</param>
        /// <param name="digit">Digit</param>
        /// <returns>Int</returns>
        private static int WriteDigit(int x, int y, int height, int digit)
        {
            if (digit < 0)
                return 0;

            float resScalingX = (float)BaseGame.Width / 1600.0f;
            float resScalingY = (float)BaseGame.Height / 1200.0f;
            float scaleFactor = height / (float)BigNumberRects[0].Height;

            Rectangle rect = BigNumberRects[digit % BigNumberRects.Length];
            BaseGame.UI.Ingame.RenderOnScreen(new Rectangle(x, y,
                (int)Math.Round(rect.Width * resScalingX * scaleFactor),
                (int)Math.Round(rect.Height * resScalingY * scaleFactor)), rect);

            return (int)Math.Round(rect.Width * resScalingX * scaleFactor);
        }

        /// <summary>
        /// Write digit
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="digit">Digit</param>
        /// <param name="alpha">Alpha</param>
        /// <returns>Int</returns>
        private static int WriteDigit(int x, int y, int digit, float alpha)
        {
            float resScalingX = (float)BaseGame.Width / 1600.0f;
            float resScalingY = (float)BaseGame.Height / 1200.0f;

            Rectangle rect = BigNumberRects[digit % BigNumberRects.Length];
            BaseGame.UI.Ingame.RenderOnScreen(new Rectangle(x, y,
                (int)Math.Round(rect.Width * resScalingX),
                (int)Math.Round(rect.Height * resScalingY)), rect,
                ColorHelper.ApplyAlphaToColor(Color.White, alpha));

            return (int)Math.Round(rect.Width * resScalingX);
        }

        /// <summary>
        /// Write number
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="number">Number</param>
        /// <returns>Int</returns>
        public static int WriteNumber(int x, int y, int number)
        {
            // Convert to string
            string numberText = number.ToString();
            int width = 0;

            // And now process every letter
            //foreach (char numberChar in numberText.ToCharArray())
            char[] chars = numberText.ToCharArray();
            for (int num = 0; num < chars.Length; num++)
            {
                width += WriteDigit(x + width, y, (int)chars[num] - (int)'0');
            }

            return width;
        }

        /// <summary>
        /// Write number
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="number">Number</param>
        /// <param name="alpha">Alpha</param>
        /// <returns>Int</returns>
        public static int WriteNumber(int x, int y, int number, float alpha)
        {
            // Convert to string
            string numberText = number.ToString();
            int width = 0;

            // And now process every letter
            //foreach (char numberChar in numberText.ToCharArray())
            char[] chars = numberText.ToCharArray();
            for (int num = 0; num < chars.Length; num++)
            {
                width += WriteDigit(
                    x + width, y, (int)chars[num] - (int)'0', alpha);
            }

            return width;
        }

        /// <summary>
        /// Write number
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="height">Height</param>
        /// <param name="number">Number</param>
        /// <returns>Int</returns>
        public static int WriteNumber(int x, int y, int height, int number)
        {
            // Convert to string
            string numberText = number.ToString();
            int width = 0;

            // And now process every letter
            //foreach (char numberChar in numberText.ToCharArray())
            char[] chars = numberText.ToCharArray();
            for (int num = 0; num < chars.Length; num++)
            {
                width += WriteDigit(
                    x + width, y, height, (int)chars[num] - (int)'0');
            }

            return width;
        }

        /// <summary>
        /// Write number centered
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="number">Number</param>
        public static void WriteNumberCentered(int x, int y, int number)
        {
            WriteNumber(
                (int)(x - (number.ToString().Length * BigNumberRects[0].Width / 2) *
                ((float)BaseGame.Width / 1600.0f)),
                y, number);
        }

        /// <summary>
        /// Write number centered
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="number">Number</param>
        /// <param name="alpha">Alpha</param>
        public static void WriteNumberCentered(int x, int y, int number, float alpha)
        {
            WriteNumber(
                (int)(x - (number.ToString().Length * BigNumberRects[0].Width / 2) *
                ((float)BaseGame.Width / 1600.0f)),
                y, number, alpha);
        }
        #endregion
    }
}
