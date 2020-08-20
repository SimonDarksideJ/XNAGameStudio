#region File Description
//-----------------------------------------------------------------------------
// Font.cs
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
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Spacewar
{
    /// <summary>
    /// Different fonts for use in SpacewarGame
    /// </summary>
    public enum FontStyle
    {
        /// <summary>
        /// Large gold font for in game scores
        /// </summary>
        Score,

        /// <summary>
        /// Large font used on weapon selection screen
        /// </summary>
        WeaponLarge,

        /// <summary>
        /// Small font used on weapon selection screen
        /// </summary>
        WeaponSmall,

        /// <summary>
        /// Font used for countdown on game screen
        /// </summary>
        GameCountDown,

        /// <summary>
        /// Font that stores the 'player1' and 'player2' strings "1" and "2"
        /// </summary>
        GamePlayerNames,

        /// <summary>
        /// The blue score buttons "0" and "1"
        /// </summary>
        ScoreButtons,

        /// <summary>
        /// The weapon icons "01234" for player1 "56789" for player2
        /// </summary>
        WeaponIcons,

        /// <summary>
        /// The 5 states of the healthbar "54321"
        /// </summary>
        HealthBar,

        /// <summary>
        /// The names of the 3 ships "012"
        /// </summary>
        ShipNames,

    }

    /// <summary>
    /// Utility wrapper for pulling digits from a small font sheet
    /// </summary>
    public static class Font
    {
        /// <summary>
        /// Assumes fonts are in  single row in the order 01234567890$,
        /// </summary>

        private static SpriteBatch batch;

        private struct FontInfo
        {
            public string Filename;
            public string Characters;
            public int StartOffset;
            public int CharacterSpacing;
            public int CharacterWidth;
            public int CharacterHeight;

            public FontInfo(string fileName, string characters, int startOffset, int characterSpacing, int characterWidth, int characterHeight)
            {
                Filename = fileName;
                Characters = characters;
                CharacterHeight = characterHeight;
                StartOffset = startOffset;
                CharacterSpacing = characterSpacing;
                CharacterWidth = characterWidth;
            }
        }

        private static FontInfo[] _fontInfo = new FontInfo[] 
        {
            new FontInfo(@"fonts\in-game_score", "0123456789", 0, 60, 58, 100),
            new FontInfo(@"fonts\weapon_large_font", "0123456789$,ptsx=", 0, 20, 18, 35),
            new FontInfo(@"fonts\weapon_small_font", "0123456789$,", 0, 15, 13, 30),
            new FontInfo(@"fonts\ingame_counter", "0123456789:", 0, 30, 24, 70),
            new FontInfo(@"fonts\in-game_player_text", "12", 0, 120, 120, 30),
            new FontInfo(@"fonts\hud_round_button", "01", 0, 28, 28, 22),
            new FontInfo(@"fonts\hud_weapon_icons", "0123456789", 0, 150, 150, 150),
            new FontInfo(@"fonts\health", "54321", 0, 50, 50, 70),
            new FontInfo(@"fonts\Ship_names", "012", 0, 200, 200, 30),
        };

        public static void Init(Game game)
        {
            if (game != null)
            {
                IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));

                if (batch == null)
                    batch = new SpriteBatch(graphicsService.GraphicsDevice);
            }
        }

        public static void Dispose()
        {
            if (batch != null)
            {
                batch.Dispose();
                batch = null;
            }
        }

        /// <summary>
        /// Starts a batch for efficient font drawing
        /// </summary>
        public static void Begin()
        {
            batch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);
        }

        /// <summary>
        /// Ends a batch of font draw calls
        /// </summary>
        public static void End()
        {
            batch.End();
        }

        /// <summary>
        /// Draws some text from the given font
        /// </summary>
        /// <param name="fontStyle">Which font to use</param>
        /// <param name="x">X position in screen pixel space</param>
        /// <param name="y">Y position in screen pixel space</param>
        /// <param name="number">The number to draw</param>
        /// <param name="color">The color to draw it in</param>
        public static void Draw(FontStyle fontStyle, int x, int y, int number, Vector4 color)
        {
            Draw(fontStyle, x, y, number.ToString(), color);
        }

        /// <summary>
        /// Draws some text from the given font
        /// </summary>
        /// <param name="fontStyle">Which font to use</param>
        /// <param name="x">X position in screen pixel space</param>
        /// <param name="y">Y position in screen pixel space</param>
        /// <param name="number">The number to draw</param>
        public static void Draw(FontStyle fontStyle, int x, int y, int number)
        {
            //No color - use 'white' i.e. use whatever is in the file
            Draw(fontStyle, x, y, number.ToString(), new Vector4(1f, 1f, 1f, 1f));
        }

        /// <summary>
        /// Draws some text from the given font
        /// </summary>
        /// <param name="fontStyle">Which font to use</param>
        /// <param name="x">X position in screen pixel space</param>
        /// <param name="y">Y position in screen pixel space</param>
        /// <param name="digits">The characters to draw</param>
        public static void Draw(FontStyle fontStyle, int x, int y, string digits)
        {
            //No color - use 'white' i.e. use whatever is in the file
            Draw(fontStyle, x, y, digits, new Vector4(1f, 1f, 1f, 1f));
        }

        /// <summary>
        /// Draws some text from the given font
        /// </summary>
        /// <param name="fontStyle">Which font to use</param>
        /// <param name="x">X position in screen pixel space</param>
        /// <param name="y">Y position in screen pixel space</param>
        /// <param name="digits">The characters to draw</param>
        /// <param name="color">The color to draw it in</param>
        public static void Draw(FontStyle fontStyle, int x, int y, string digits, Vector4 color)
        {
            float xPosition = x;
            FontInfo fontInfo = _fontInfo[(int)fontStyle];

            for (int i = 0; i < digits.Length; i++)
            {
                //Don't draw anything if its a space character
                if (digits[i] != ' ')
                {
                    //Look up the character position
                    int character = fontInfo.Characters.IndexOf(digits[i]);

                    //Draw the correct character at the correct position
                    batch.Draw(
                        SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + fontInfo.Filename),
                        new Vector2(xPosition, (float)y),
                        new Rectangle(character * fontInfo.CharacterSpacing + fontInfo.StartOffset, 0, fontInfo.CharacterWidth, fontInfo.CharacterHeight),
                        new Color(color));
                }

                //Move the position of the next character.
                //If the character is a comma or colon then use a 'fudge factor' to make the font look a little proportional
                xPosition += (digits[i] == ',' || digits[i] == ':') ? fontInfo.CharacterWidth / 2 : fontInfo.CharacterWidth;
            }
        }
    }
}
