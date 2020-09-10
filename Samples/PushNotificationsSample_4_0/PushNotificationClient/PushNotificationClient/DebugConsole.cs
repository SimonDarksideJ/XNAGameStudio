//-----------------------------------------------------------------------------
// DebugConsole.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace PushNotificationClient
{
    /// <summary>
    /// A simple console for outputting multiple lines of text on the display.
    /// Adding text to the console is thread-safe, so it can be used from the
    /// UI thread or async callback functions.
    /// </summary>
    class DebugConsole
    {
        // Stores a line of text plus a height returned from SpriteFont.MeasureString().
        struct LineEntry
        {
            public string text;
            public int height;
        }

        SpriteFont consoleFont;
        List<LineEntry> outputText = new List<LineEntry>();

        Rectangle bounds;
        int currentConsoleHeight = 0;

        object lockObject = new object();


        /// <summary>
        /// Constructs a console with the given SpriteFont and display rectangle.
        /// </summary>
        public DebugConsole(SpriteFont font, Rectangle viewportBounds)
        {
            consoleFont = font;
            bounds = viewportBounds;
        }


        /// <summary>
        /// Adds a line of text to the console.  If there isn't enough room to push
        /// the new line at the end, previous lines are popped from the beginning
        /// to make space.
        /// </summary>
        public void AddLine(string text)
        {
            LineEntry entry = new LineEntry();
            entry.text = text;
            entry.height = (int)consoleFont.MeasureString(text).Y;

            lock (lockObject)
            {
                currentConsoleHeight += entry.height;

                while (currentConsoleHeight > bounds.Height && outputText.Count > 0)
                {
                    currentConsoleHeight -= outputText[0].height;
                    outputText.RemoveAt(0);
                }

                outputText.Add(entry);
            }

            // print to the IDE as well.
            Debug.WriteLine(text);
        }


        /// <summary>
        /// Renders the console using the given SpriteBatch.
        /// </summary>
        public void Draw(SpriteBatch spritebatch)
        {
            Vector2 position = new Vector2(bounds.X, bounds.Y);

            spritebatch.Begin();

            lock (lockObject)
            {
                for (int i = 0; i < outputText.Count; i++)
                {
                    spritebatch.DrawString(consoleFont, outputText[i].text, position, Color.White);
                    position.Y += outputText[i].height;
                }
            }

            spritebatch.End();
        }
    }
}
