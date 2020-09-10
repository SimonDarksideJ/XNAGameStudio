//-----------------------------------------------------------------------------
// Button.cs
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
    /// A Button is a UI Element that has an up & down state, an event handler,
    /// and a string of text
    /// </summary>
    public class Button : UIElement
    {
        BasicEffect effect;
        
        protected SpriteFont buttonFont;
        protected VertexPositionColor[] verts = new VertexPositionColor[6];
        protected Vector2 textOffset;
        protected int pressId;

        protected StringBuilder buttonText = new StringBuilder();
        /// <summary>
        /// Gets or sets the text displayed on the button
        /// </summary>
        public string Text
        {
            get { return buttonText.ToString(); }
            set
            {
                buttonText.Length = 0;
                buttonText.Append(value);
                needsMeasure = true;
            }
        }        

        public delegate void ClickEventHandler(object sender);

        /// <summary>
        /// OnClick fires when the button is pressed and released
        /// </summary>
        public event ClickEventHandler OnClick;


        /// <summary>
        /// Creates a new button object
        /// </summary>
        public Button(GraphicsDevice device, SpriteFont font, string text)
        {
            buttonFont = font;
            Text = text;

            Size = buttonFont.MeasureString(buttonText);

            effect = new BasicEffect(device);
            effect.Projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, device.Viewport.Height, 0, 1, 1000);
            effect.View = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            effect.VertexColorEnabled = true;

            for (int i = 0; i < verts.Length; i++)
            {
                verts[i].Color = Color.White;
            }
        }


        /// <summary>
        /// check to see if the button was pressed or released
        /// </summary>
        public override void HandleTouch(TouchLocation loc)
        {
            if (loc.State == TouchLocationState.Pressed)
            {
                if (pressId == 0 && HitTest(loc.Position))
                {
                    if (IsVisible)
                    {
                        pressId = loc.Id;
                    }
                }
            }
            else if (loc.State == TouchLocationState.Released)
            {
                if (pressId == loc.Id)
                {
                    pressId = 0;

                    if (HitTest(loc.Position) && OnClick != null)
                    {
                        OnClick(this);
                    }
                }
            }
        }

        /// <summary>
        /// Check to see if the button's rectangle contains the given point
        /// </summary>
        protected bool HitTest(Vector2 point)
        {
            return (point.X >= Position.X && point.X < Position.X + Size.X &&
                point.Y >= Position.Y && point.Y < Position.Y + Size.Y);

        }

        /// <summary>
        /// Called when the button's attributes are 'dirty' and the visuals
        /// need to be updated
        /// </summary>
        protected override void Measure()
        {
            if (buttonFont != null && buttonText.Length > 0)
            {
                textOffset = (Size - buttonFont.MeasureString(buttonText)) / 2;
            }

            // this array will be used to draw either a line strip or a triangle list
            verts[0].Position = new Vector3(Position.X, Position.Y, 0);
            verts[1].Position = new Vector3(Position.X + Size.X, Position.Y, 0);
            verts[2].Position = new Vector3(Position.X + Size.X, Position.Y + Size.Y, 0);
            verts[3].Position = new Vector3(Position.X, Position.Y + Size.Y, 0);
            verts[4].Position = verts[0].Position;
            verts[5].Position = verts[2].Position;
        }

        /// <summary>
        /// Renders the button.  This function mixes spritebatch and 3D drawing so it
        /// should not be called from within a SpriteBatch.Begin/End block
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
            {
                return;
            }

            base.Draw(spriteBatch);

            if (needsMeasure)
            {
                Measure();
            }

            // draw a box
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                if (pressId == 0)
                {
                    effect.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, verts, 0, 4);
                }
                else
                {
                    effect.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, verts, 0, 2);
                }
            }

            // draw the text
            spriteBatch.Begin();
            spriteBatch.DrawString(buttonFont, buttonText, Position + textOffset, pressId == 0 ? Color.White : Color.Black);
            spriteBatch.End();
        }
    }
}
