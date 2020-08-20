#region File Description
//-----------------------------------------------------------------------------
// TextItem.cs
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
using RobotGameData.GameInterface;
#endregion

namespace RobotGameData.Text
{
    /// <summary>
    /// a structure with text information to be drawn on screen.
    /// </summary>
    public class TextItem : IIdentity
    {
        #region Fields

        protected int id = 0;
        protected SpriteFont spriteFont = null;
        protected string stringText = String.Empty;
        protected Vector2 position = Vector2.Zero;
        protected Color textColor = Color.White;
        protected float rotation = 0.0f;
        protected float scale = 1.0f;
        protected bool visible = true;

        #endregion

        #region Properties

        public int Id
        {
            get
            {
                if (id == 0)
                {
                    id = GetHashCode();
                }
                return id;
            }
        }

        public SpriteFont Font
        {
            get { return spriteFont; }
            set { spriteFont = value; }
        }

        public string Text
        {
            get { return stringText; }
            set { stringText = value; }
        }

        public int PosX
        {
            get { return (int)position.X; }
            set { position.X = (float)value; }
        }

        public int PosY
        {
            get { return (int)position.Y; }
            set { position.Y = (float)value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Color Color
        {
            get { return textColor; }
            set { textColor = value; }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public TextItem() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="font">sprite font</param>
        /// <param name="text">text</param>
        /// <param name="x">position x</param>
        /// <param name="y">position y</param>
        /// <param name="color">text color</param>
        public TextItem(SpriteFont font, string text, int x, int y, Color color)
        {
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }
            if (String.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text");
            }

            spriteFont = font;
            stringText = text;

            position.X = (int)x;
            position.Y = (int)y;            
            textColor = color;
        }

        public void CopyTo(TextItem target)
        {
            target.Font = Font;
            target.Text = Text;
            target.PosX = PosX;
            target.PosY = PosY;
            target.Color = Color;
        }

        public bool IsSameValue(TextItem item)
        {
            if (Font != item.Font)
                return false;

            if (Text != item.Text)
                return false;

            if (PosX != item.PosX)
                return false;

            if (PosY != item.PosY)
                return false;

            if (Color != item.Color)
                return false;

            return true;
        }
    }
}
