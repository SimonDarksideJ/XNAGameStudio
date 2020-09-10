//-----------------------------------------------------------------------------
// Slidebar.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace RimLighting
{
    /// <summary>
    /// A Slidebar is a UI Element (control) that has a string of text and a bar whose length can be changed by dragging on it
    /// </summary>
    public class Slidebar : UIElement
    {
        protected Game game;
        protected SpriteFont spriteFont;

        // Color of the control
        public Color Color = Color.White;
        
        public Vector2 TextSize { get; private set; }
        protected StringBuilder sliderText = new StringBuilder();
        
        // Text of the control to show on screen
        public string Text
        {
            get
            {
                return sliderText.ToString();
            }
            set
            {
                sliderText.Length = 0;
                sliderText.Append(value);
                TextSize = spriteFont.MeasureString(value);
                needsMeasure = true;
            }
        }

        // The relative position of the bar to the text
        Vector2 offsetBar;
        
        // The maximum extent of the bar
        Vector2 sizeBar;

        // The current length of the bar which is caculated from current Value together with MinValue and MaxValue
        float currentLength = 0;

        // The maximum and minimum possible values represented by this slidebar
        public float MinValue;
        public float MaxValue;

        // Current Value of the slidebar
        private float valueInt;
        public float Value
        {
            get { return valueInt; }
            set
            {
                valueInt = value;
                currentLength = (valueInt - MinValue) / (MaxValue - MinValue) * sizeBar.X;

                if (OnValueChanged != null)
                    OnValueChanged(this);
            }
        }

        // Is the user currently dragging on this slidebar?
        public bool IsDragging = false;

        // The first position of the touch since the drag began
        Vector2 lastPressPosition;

        // Plain texture used for drawing the bar
        protected static Texture2D texureBlank = null;

        // OnValueChanged event
        // This is triggered either by manually changing Value or dragging on the bar by the user
        public delegate void ValueChangedHandler(object sender);
        public event ValueChangedHandler OnValueChanged = null;

        public Slidebar(Game game, SpriteFont font, float min, float max)
        {
            this.game = game;
            spriteFont = font;

            MinValue = min;
            MaxValue = max;
        }

        /// <summary>
        /// Sets position and size of the bar
        /// </summary>
        /// <param name="offsetX">The relative X coordinate to the Text</param>
        /// <param name="offsetY">The relative Y coordinate to the Text</param>
        /// <param name="maxwidth">The max width of the bar in pixels</param>
        /// <param name="height">The height of the bar in pixels</param>
        public void SetBarOffsetSize(float offsetX, float offsetY, float maxwidth, float height)
        {
            offsetBar = new Vector2(offsetX, offsetY);
            sizeBar = new Vector2(maxwidth, height);
            needsMeasure = true;
        }

        /// <summary>
        /// Sets the floating number range that this slidebar could represent
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void SetRange(float min, float max)
        {
            MinValue = min;
            MaxValue = max;
        }

        protected override void Measure()
        {
            Size = TextSize + offsetBar + sizeBar;
            currentLength = (valueInt - MinValue) / (MaxValue - MinValue) * sizeBar.X;
            needsMeasure = false;
        }

        /// <summary>
        /// Load the texture for rendering the bar if it is not already loaded
        /// </summary>
        protected void LoadContent()
        {
            if (texureBlank == null)
            {
                texureBlank = game.Content.Load<Texture2D>("blankTex");
            }
        }

        /// <summary>
        /// Handle the touch input and update the bar if necessary
        /// </summary>
        /// <param name="loc"></param>
        public override void HandleTouch(TouchLocation loc)
        {
            if (loc.State == TouchLocationState.Pressed && !IsDragging)
            {
                if (loc.Position.Y >= Position.Y && loc.Position.Y <= (Position.Y + offsetBar.Y + sizeBar.Y))
                {
                    IsDragging = true;
                    lastPressPosition = loc.Position;
                }
            }
            else
            {
                if (loc.State == TouchLocationState.Released)
                    IsDragging = false;
            }

            if (IsDragging)
            {
                Vector2 delta = loc.Position - lastPressPosition;
                lastPressPosition = loc.Position;

                currentLength += delta.X;
                if (currentLength < 0) currentLength = 0;
                if (currentLength > sizeBar.X) currentLength = sizeBar.X;
                valueInt = currentLength / sizeBar.X * (MaxValue - MinValue) + MinValue;
                if (OnValueChanged != null)
                    OnValueChanged(this);
            }                             
        }

        private static Rectangle rectangle = new Rectangle();

        /// <summary>
        /// Renders the control to screen
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
            {
                return;
            }

            base.Draw(spriteBatch);            
                        
            LoadContent();            

            spriteBatch.Begin();
            rectangle.X = (int)(Position.X + offsetBar.X);
            rectangle.Y = (int)(Position.Y + offsetBar.Y);
            rectangle.Width = (int)currentLength;
            rectangle.Height = (int)sizeBar.Y;
            spriteBatch.Draw(texureBlank, rectangle, Color);
            spriteBatch.DrawString(spriteFont, Text, Position, Color);
            spriteBatch.End();
        }
    }
}
