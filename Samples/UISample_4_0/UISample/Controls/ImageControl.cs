//-----------------------------------------------------------------------------
// ImageControl.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace UserInterfaceSample.Controls
{
    /// <summary>
    /// ImageControl is a control that displays a single sprite. By default it displays an entire texture.
    ///
    /// If a null texture is given, this control will use DrawContext.BlankTexture. This allows it to be
    /// used to draw solid-colored rectangles.
    /// </summary>
    public class ImageControl : Control
    {
        private Texture2D texture;

        // Position within the source texture, in texels. Default is (0,0) for the upper-left corner.
        public Vector2 origin;

        // Size in texels of source rectangle. If null (the default), size will be the same as the size of the control.
        // You only need to set this property if you want texels scaled at some other size than 1-to-1; normally
        // you can just set the size of both the source and destination rectangles with the Size property.
        public Vector2? SourceSize;

        // Color to modulate the texture with. The default is white, which displays the original unmodified texture.
        public Color Color;

        // Texture to draw
        public Texture2D Texture
        {
            get { return texture; }
            set
            {
                if (texture != value)
                {
                    texture = value;
                    InvalidateAutoSize();
                }
            }
        }

        public ImageControl() : this(null, Vector2.Zero)
        {
        }

        public ImageControl(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            this.Position = position;
            this.Color = Color.White;
        }

        public override void Draw(DrawContext context)
        {
            base.Draw(context);
            Texture2D drawTexture = texture ?? context.BlankTexture;

            Vector2 actualSourceSize = SourceSize ?? Size;
            Rectangle sourceRectangle = new Rectangle
            {
                X = (int)origin.X,
                Y = (int)origin.Y,
                Width = (int)actualSourceSize.X,
                Height = (int)actualSourceSize.Y,
            };
            Rectangle destRectangle = new Rectangle
            {
                X = (int)context.DrawOffset.X,
                Y = (int)context.DrawOffset.Y,
                Width = (int)Size.X,
                Height = (int)Size.Y
            };
            context.SpriteBatch.Draw(drawTexture, destRectangle, sourceRectangle, Color);
        }

        override public Vector2 ComputeSize()
        {
            if(texture!=null)
            {
                return new Vector2(texture.Width, texture.Height);
            }
            return Vector2.Zero;
        }
    }
}
