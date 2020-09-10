#region File Information
//-----------------------------------------------------------------------------
// Button.cs
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace DynamicMenu.Controls
{
    /// <summary>
    /// A control which reads touch input within its bounds and fires a tapped event when touched
    /// </summary>
    public class Button : TextControl
    {
        #region Fields

        //Keep a button pressed for this long in seconds before executing the event
        private const double PRESS_TIME_SECONDS = .2; 

        private double pressStartTime = 0;

        #endregion

        #region Events

        /// <summary>
        /// Indicates that the button was tapped
        /// </summary>
        public event EventHandler Tapped;

        #endregion

        #region Properties

        /// <summary>
        /// The name of the texture that is shown when this button is pressed
        /// </summary>
        [ContentSerializer(Optional = true)]
        public string PressedTextureName { get; set; }

        /// <summary>
        /// The texture that is shown when this button is pressed
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D PressedTexture { get; set; }

        /// <summary>
        /// Whether the button is pressed
        /// </summary>
        [ContentSerializerIgnore]
        public bool Pressed { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Loads this control's content
        /// </summary>
        public override void LoadContent(GraphicsDevice _graphics, ContentManager _content)
        {
            base.LoadContent(_graphics, _content);

            if (!string.IsNullOrEmpty(PressedTextureName))
            {
                PressedTexture = _content.Load<Texture2D>(PressedTextureName);
            }
        }


        #endregion

        #region Update

        /// <summary>
        /// Updates this control
        /// </summary>
        public override void Update(GameTime gameTime, List<GestureSample> gestures)
        {
            base.Update(gameTime, gestures);

            foreach (GestureSample sample in gestures)
            {
                if (sample.GestureType != GestureType.Tap) continue;

                if (ContainsPos(sample.Position))
                {
                    Pressed = true;
                    pressStartTime = gameTime.TotalGameTime.TotalSeconds;

                    HandlePressed();

                    break;
                }
            }

            if (Pressed && (gameTime.TotalGameTime.TotalSeconds > pressStartTime + PRESS_TIME_SECONDS))
            {
                Pressed = false;

                if (Tapped != null)
                {
                    Tapped(this, new EventArgs());
                }
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws this control
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            DrawCenteredText(spriteBatch, Font, GetAbsoluteRect(), Text, TextColor);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the current texture based on whether the button is pressed
        /// </summary>
        public override Texture2D GetCurrTexture()
        {
            if (Pressed && PressedTexture != null)
            {
                return PressedTexture;
            }
            return base.GetCurrTexture();
        }

        protected virtual void HandlePressed()
        {
            // Can be overriden to do special processing when this is clicked
        }

        #endregion
    }
}
