#region File Description
//-----------------------------------------------------------------------------
// TextBox.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Geolocation
{
    class TextBox : UIElement
    {
        protected SpriteFont spriteFont;

        protected StringBuilder text = new StringBuilder();
        public string Text
        {
            get
            {
                return text.ToString();
            }
            set
            {
                text.Length = 0;
                text.Append(value);
                needsMeasure = true;
            }
        }

        public Color Color;
        
        public TextBox(SpriteFont spriteFont)
        {            
            this.spriteFont = spriteFont;
            
            Text = "";
            size = new Vector2(0, 0);
            Color = Color.White;
            position = new Vector2(0, 0);
        }

        public void WriteLn(string str)
        {
            text.Append(str);
            text.Append("\n");
            needsMeasure = true;
        }

        protected override void Measure()
        {
            if (spriteFont != null && Text.Length > 0)
            {
                size = spriteFont.MeasureString(Text);
            }
            needsMeasure = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, Text, Position, Color);
            spriteBatch.End();
        }        
    }
}
