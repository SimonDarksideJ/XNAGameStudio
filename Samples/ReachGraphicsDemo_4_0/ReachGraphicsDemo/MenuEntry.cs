#region File Description
//-----------------------------------------------------------------------------
// MenuEntry.cs
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
using Microsoft.Xna.Framework.Input;
#endregion

namespace XnaGraphicsDemo
{
    /// <summary>
    /// Base class for each entry in a MenuComponent.
    /// </summary>
    class MenuEntry
    {
        // Constants.
        public const int Height = 64;
        public const int Border = 32;


        // Properties.
        public virtual string Text { get; set; }
        public Vector2 Position { get; set; }
        public bool IsFocused { get; set; }
        public bool IsDraggable { get; set; }
        public Action Clicked { get; set; }

        public Color Color { get { return IsFocused ? Color.Blue : Color.White; } }

        Vector2 positionOffset;

        
        /// <summary>
        /// Draws the menu entry.
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D blankTexture)
        {
            positionOffset = new Vector2(0, (Height - font.LineSpacing) / 2);

            spriteBatch.DrawString(font, Text, Position + positionOffset, Color);
        }


        /// <summary>
        /// Handles clicks on this menu entry.
        /// </summary>
        public virtual void OnClicked()
        {
            // If we have a click delegate, call that now.
            if (Clicked != null)
                Clicked();

            // If we are not draggable, spawn a visual feedback effect.
            if (!IsDraggable)
                DemoGame.SpawnZoomyText(Text, Position + positionOffset);
        }


        /// <summary>
        /// Handles dragging this menu entry from left to right.
        /// </summary>
        public virtual void OnDragged(float delta)
        {
        }
    }


    /// <summary>
    /// Menu entry subclass for boolean toggle values.
    /// </summary>
    class BoolMenuEntry : MenuEntry
    {
        // Properties.
        public bool Value { get; set; }
        public string Label { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        public BoolMenuEntry(string label)
        {
            Label = label;
        }


        /// <summary>
        /// Click handler toggles the boolean value.
        /// </summary>
        public override void OnClicked()
        {
            Value = !Value;

            base.OnClicked();
        }


        /// <summary>
        /// Customize our text string.
        /// </summary>
        public override string Text
        {
            get { return Label + " " + (Value ? "on" : "off"); }
            set { }
        }
    }


    /// <summary>
    /// Menu entry subclass for floating point slider values.
    /// </summary>
    class FloatMenuEntry : MenuEntry
    {
        // Properties.
        public float Value { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        public FloatMenuEntry()
        {
            IsDraggable = true;
        }


        /// <summary>
        /// Drag handler changes the slider position.
        /// </summary>
        public override void OnDragged(float delta)
        {
            const float speed = 1f / 300;

            Value = MathHelper.Clamp(Value + delta * speed, 0, 1);
        }


        /// <summary>
        /// Custom draw function displays a slider bar in addition to the item text.
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D blankTexture)
        {
            base.Draw(spriteBatch, font, blankTexture);

            Vector2 size = font.MeasureString(Text);
            size.Y /= 2;

            Vector2 pos = Position + size;

            pos.X += 8;
            pos.Y += (Height - font.LineSpacing) / 2;

            float w = 480 - Border - pos.X;

            spriteBatch.Draw(blankTexture, new Rectangle((int)pos.X, (int)pos.Y - 3, (int)(w * Value), 6), Color);
        }
    }
}
