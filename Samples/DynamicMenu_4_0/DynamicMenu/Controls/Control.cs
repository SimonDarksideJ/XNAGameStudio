#region File Information
//-----------------------------------------------------------------------------
// Controls.cs
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using DynamicMenu.Transitions;
#endregion

namespace DynamicMenu.Controls
{
    /// <summary>
    /// The base class for all controls
    /// </summary>
    public abstract class Control : IControl
    {
        #region Fields

        private List<Transition> activeTransitions = new List<Transition>();

        #endregion

        #region Properties

        /// <summary>
        /// The left position of this control relative from its parent
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// The top position of this control relative from its parent
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// The width of this control 
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of this control
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The bottom position of this control relative from its parent
        /// </summary>
        [ContentSerializerIgnore]
        public int Bottom
        {
            get { return Top + Height; }
        }

        /// <summary>
        /// The right position of this control relative from its parent
        /// </summary>
        [ContentSerializerIgnore]
        public int Right
        {
            get { return Left + Width; }
        }

        /// <summary>
        /// The name of this control
        /// </summary>
        [ContentSerializer(Optional = true)]
        public string Name { get; set; }

        /// <summary>
        /// The name of the background texture for this control
        /// </summary>
        [ContentSerializer(Optional = true)]
        public string BackTextureName { get; set; }

        /// <summary>
        /// The Background texture for this control
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D BackTexture { get; set; }

        /// <summary>
        /// Whether this control is currently visible and enabled
        /// </summary>
        [ContentSerializer(Optional = true)]
        public bool Visible { get; set; }

        /// <summary>
        /// The hugh applied to this control
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Color Hue { get; set; }

        /// <summary>
        /// The parent of this control
        /// </summary>
        [ContentSerializerIgnore]
        public IControl Parent { get; set; }

        /// <summary>
        /// This can be used to store information related to this control
        /// </summary>
        [ContentSerializerIgnore]
        public object Tag { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Public constructor
        /// </summary>
        public Control()
        {
            Visible = true;
            Hue = Color.White;
        }

        /// <summary>
        /// Control initialization
        /// </summary>
        virtual public void Initialize()
        {
            // Do nothing here
        }

        /// <summary>
        /// Loads the content for this control
        /// </summary>
        virtual public void LoadContent(GraphicsDevice _graphics, ContentManager _content)
        {
            if (!string.IsNullOrEmpty(BackTextureName))
            {
                BackTexture = _content.Load<Texture2D>(BackTextureName);
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates this control, called once per game frame
        /// </summary>
        /// <param name="gameTime">The current game time</param>
        /// <param name="gestures">The list of recorded gestures for this frame</param>
        virtual public void Update(GameTime gameTime, List<GestureSample> gestures)
        {
            List<Transition> toRemove = new List<Transition>();
            List<Transition> curTransitions = new List<Transition>();
            curTransitions.AddRange(activeTransitions);

            // Update any applied transitions
            foreach (Transition transition in curTransitions)
            {
                transition.Update(gameTime);
                if (!transition.TransitionActive)
                {
                    toRemove.Add(transition);
                }
            }

            // Remove any completed transitions
            foreach (Transition transition in toRemove)
            {
                activeTransitions.Remove(transition);
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the control, called once per frame
        /// </summary>
        /// <param name="gameTime">The current game time</param>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        virtual public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Texture2D currTexture = GetCurrTexture();

            if (currTexture != null)
            {
                Rectangle rect = GetAbsoluteRect();

                spriteBatch.Draw(currTexture, rect, null, Hue, 0f, new Vector2(), SpriteEffects.None, 0f);
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Overridable by decendants.  This returns the current texture being used as the background image.
        /// </summary>
        /// <returns>The current texture</returns>
        virtual public Texture2D GetCurrTexture()
        {
            return BackTexture;
        }

        /// <summary>
        /// Gets the top left position of this control in screen coordinates.
        /// </summary>
        /// <returns>The top left point</returns>
        public Point GetAbsoluteTopLeft()
        {
            Point absoluteTopLeft = new Point(Left, Top);

            if (Parent != null)
            {
                Point parentTopLeft = Parent.GetAbsoluteTopLeft();
                absoluteTopLeft.X += parentTopLeft.X;
                absoluteTopLeft.Y += parentTopLeft.Y;
            }

            return absoluteTopLeft;
        }

        /// <summary>
        /// Gets the boundaries for this control in screen coordinates.
        /// </summary>
        /// <returns>The rect boundaries</returns>
        public Rectangle GetAbsoluteRect()
        {
            Point topLeft = GetAbsoluteTopLeft();

            return new Rectangle(topLeft.X, topLeft.Y, Width, Height);
        }

        /// <summary>
        /// Starts the passed in transition on this control.
        /// </summary>
        /// <param name="transition">The transition to apply</param>
        public void ApplyTransition(Transition transition)
        {
            activeTransitions.Add(transition);
            transition.Control = this;
            transition.StartTranstion();
        }

        /// <summary>
        /// Draws the specified text in the center of the control.
        /// </summary>
        /// <param name="_spriteBatch">The sprite batch to draw with</param>
        /// <param name="_font">The font to use for the text</param>
        /// <param name="_rect">The boundaries to center the text within</param>
        /// <param name="_text">The text to draw</param>
        /// <param name="_color">The hue to use for the text</param>
        protected void DrawCenteredText(SpriteBatch _spriteBatch, SpriteFont _font, Rectangle _rect, string _text, Color _color)
        {
            if (_font == null || string.IsNullOrEmpty(_text)) return;

            // Center the text in the rect
            Vector2 midPoint = new Vector2(_rect.X + _rect.Width / 2, _rect.Y + _rect.Height / 2);
            Vector2 stringSize = _font.MeasureString(_text);
            Vector2 fontPos = new Vector2(midPoint.X - stringSize.X * .5f, midPoint.Y - stringSize.Y * .5f);

            _spriteBatch.DrawString(_font, _text, fontPos, _color);
        }

        /// <summary>
        /// Indicates whether the specified position falls within the control
        /// </summary>
        /// <param name="pos">The position to check</param>
        /// <returns>Whether the position is contained within the control</returns>
        protected bool ContainsPos(Vector2 pos)
        {
            Rectangle rect = GetAbsoluteRect();
            return rect.Contains((int)pos.X, (int)pos.Y);
        }

        #endregion
    }
}
