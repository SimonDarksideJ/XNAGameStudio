#region File Information
//-----------------------------------------------------------------------------
// ATextControl.cs
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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace DynamicMenu.Controls
{
    /// <summary>
    /// A control implementing the ITextControl interface, containing text
    /// </summary>
    public abstract class TextControl : Control, ITextControl
    {
        #region Fields

        /// <summary>
        ///  The amount of space to the left and right of the text when auto-sized using the
        ///  AutoPickWidth function
        /// </summary>
        private const int SPACE = 10;

        #endregion

        #region Properties

        /// <summary>
        /// The text shown in the control
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The name of the font to use for this control (full path to a spritefont)
        /// </summary>
        public string FontName { get; set; }

        /// <summary>
        /// The font loaded from the FontName
        /// </summary>
        [ContentSerializerIgnore]
        public SpriteFont Font { get; set; }

        /// <summary>
        /// The color of the text
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Color TextColor { get; set; }

        #endregion

        #region Initialization

        public TextControl()
        {
            TextColor = Color.Black;
        }

        /// <summary>
        /// Call this to automatically pick the width for the text control based on the length of the text
        /// </summary>
        public void AutoPickWidth()
        {
            Vector2 dim = Font.MeasureString(Text);
            Width = SPACE * 2 + (int)dim.X;
        }

        /// <summary>
        /// Loads the content for this control
        /// </summary>
        public override void LoadContent(GraphicsDevice _graphics, ContentManager _content)
        {
            base.LoadContent(_graphics, _content);

            if (!string.IsNullOrEmpty(FontName))
            {
                Font = _content.Load<SpriteFont>(FontName);
            }
        }

        #endregion
    }
}
