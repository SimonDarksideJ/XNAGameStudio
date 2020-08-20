//-----------------------------------------------------------------------------
// UIElement.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace RimLighting
{
    /// <summary>
    /// Defines the base class for a drawable UI control
    /// </summary>
    public abstract class UIElement
    {        
        protected bool needsMeasure;

        protected Vector2 position = new Vector2();
        /// <summary>
        /// Gets or sets the position of the UIElement
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; needsMeasure = true; }
        }

        protected Vector2 size = new Vector2();
        /// <summary>
        /// Gets or sets the size of the UIElement
        /// </summary>
        public Vector2 Size
        {
            get { return size; }
            set { size = value; needsMeasure = true; }
        }

        /// <summary>
        /// Gets or sets the visibility of the UIElement
        /// </summary>
        public bool IsVisible { get; set; }


        protected UIElement()
        {
            IsVisible = true;
        }


        // derived classes should implement this when they can be sized & formatted
        protected abstract void Measure();

        /// <summary>
        /// Renders a UIElement, measuring first if necessary
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (needsMeasure)
            {
                Measure();
            }
        }

        public virtual void HandleTouch(TouchLocation loc) { }

        protected static char[] splitTokens = { ' ', '-' };
        protected static string spaceString = " ";

        /// <summary>
        /// A simple word-wrap algorithm that formats based on word-breaks.
        /// it's not completely accurate with respect to kerning & spaces and
        /// doesn't handle localized text, but is easy to read for sample use.
        /// </summary>
        protected static string WordWrap(string input, int width, SpriteFont font)
        {
            StringBuilder output = new StringBuilder();
            output.Length = 0;

            string[] wordArray = input.Split(splitTokens, StringSplitOptions.None);

            int space = (int)font.MeasureString(spaceString).X;

            int lineLength = 0;
            int wordLength = 0;
            int wordCount = 0;

            for (int i = 0; i < wordArray.Length; i++)
            {
                wordLength = (int)font.MeasureString(wordArray[i]).X;

                // don't overflow the desired width unless there are no other words on the line
                if (wordCount > 0 && wordLength + lineLength > width)
                {
                    output.Append(System.Environment.NewLine);
                    lineLength = 0;
                    wordCount = 0;
                }

                output.Append(wordArray[i]);
                output.Append(spaceString);
                lineLength += wordLength + space;
                wordCount++;
            }

            return output.ToString();
        }
    }
}
