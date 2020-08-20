#region File Information
//-----------------------------------------------------------------------------
// AMultilineTextControl.cs
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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace DynamicMenu.Controls
{
    public class MultilineTextControl : TextControl
    {
        #region Fields

        protected const int HORZ_SPACE = 10;
        protected const int VERT_SPACE = 5;

        private int topSpace;
        private int leftSpace;

        private List<string> lines = new List<string>();

        #endregion

        #region Properties

        /// <summary>
        /// The strings currently being shown by the text control
        /// </summary>
        [ContentSerializerIgnore]
        public List<string> Lines
        {
            get { return lines; }
        }

        /// <summary>
        /// The amount of space between the top of the control and the first line of text
        /// </summary>
        [ContentSerializerIgnore]
        public int TopSpace
        {
            get { return topSpace; }
            set { topSpace = value; }
        }

        /// <summary>
        /// The amount of space between the left of the control and the text
        /// </summary>
        [ContentSerializerIgnore]
        public int LeftSpace
        {
            get { return leftSpace; }
            set { leftSpace = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Loads this control's content
        /// </summary>
        public override void LoadContent(GraphicsDevice _graphics, ContentManager _content)
        {
            base.LoadContent(_graphics, _content);

            // Now wrap the text to fit the control
            CalculateLines();
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws this control
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            if (Font == null)
            {
                // No font was loaded, so we can't show text
                return;
            }

            Vector2 extents = Font.MeasureString("A");
            Point topLeft = GetAbsoluteTopLeft();
            int currY = topLeft.Y + topSpace + VERT_SPACE;
            int left = topLeft.X + leftSpace + HORZ_SPACE;

            foreach (string line in lines)
            {
                if (String.IsNullOrEmpty(line))
                {
                    continue;
                }

                Vector2 origin = Vector2.Zero;

                spriteBatch.DrawString(Font, line, new Vector2(left, currY),
                    TextColor, 0, origin, 1, SpriteEffects.None, 1.0f);

                currY += (int)extents.Y + VERT_SPACE;
            }

        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines how to wrap text in this control based on its size
        /// </summary>
        public virtual void CalculateLines()
        {
            lines.Clear();

            if (string.IsNullOrEmpty(Text))
            {
                return;
            }

            Text.Trim();

            int lineWidth = Width - HORZ_SPACE * 2 - leftSpace;

            // Divide the text into words
            string[] strArray = Text.Split(new Char[] { ' ', '\n' });

            string lineStr = "";
            if (Font == null)
            {
                // No font - can't calculate the lines
                return;
            }
            foreach (string str in strArray)
            {
                if (str.Length == 0) continue;

                string tempStr = lineStr;
                //Add a space if we aren't at the beginning of the line
                if (tempStr.Length != 0)
                {
                    tempStr += " ";
                }
                tempStr += str;

                Vector2 extents = Font.MeasureString(tempStr);
                if (extents.X > lineWidth)
                {
                    // Reached the end of the line.  End the current line and start 
                    // the next with the current word
                    lines.Add(lineStr);
                    lineStr = str;
                }
                else
                {
                    // Add the word to the line
                    lineStr = tempStr;
                }
            }

            // See if there is one more line to add
            if (lineStr.Length > 0)
            {
                lines.Add(lineStr);
            }
        }

        #endregion
    }
}
