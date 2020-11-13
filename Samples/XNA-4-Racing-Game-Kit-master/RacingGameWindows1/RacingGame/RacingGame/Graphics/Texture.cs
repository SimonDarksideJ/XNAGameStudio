#region File Description
//-----------------------------------------------------------------------------
// Texture.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using RacingGame;
using RacingGame.GameLogic;
using RacingGame.Helpers;
#endregion

namespace RacingGame.Graphics
{
    /// <summary>
    /// Texture class helping you with using DirectX Textures and handling
    /// possible errors that can happen while loading (stupid DirectX
    /// error messages telling you absolutly nothing, so a lot of pre-checks
    /// help you determinate the error before even calling DirectX methods).
    /// </summary>
    public class Texture : IDisposable
    {
        #region Variables
        public static SpriteBatch alphaSprite;
        public static SpriteBatch additiveSprite;

        /// <summary>
        /// Texture filename
        /// </summary>
        protected string texFilename = "";

        /// <summary>
        /// Get filename of texture.
        /// </summary>
        public string Filename
        {
            get
            {
                return texFilename;
            }
        }

        /// <summary>
        /// Size of texture
        /// </summary>
        protected int texWidth, texHeight;

        /// <summary>
        /// Width of texture
        /// </summary>
        public int Width
        {
            get
            {
                return texWidth;
            }
        }

        /// <summary>
        /// Height of texture
        /// </summary>
        public int Height
        {
            get
            {
                return texHeight;
            }
        }

        /// <summary>
        /// Gfx rectangle
        /// </summary>
        /// <returns>Rectangle</returns>
        public Rectangle GfxRectangle
        {
            get
            {
                return new Rectangle(0, 0, texWidth, texHeight);
            }
        }

        /// <summary>
        /// Size of half a pixel, will be calculated when size is set.
        /// </summary>
        private Vector2 precaledHalfPixelSize = Vector2.Zero;

        /// <summary>
        /// Get the size of half a pixel, used to correct texture
        /// coordinates when rendering on screen, see Texture.RenderOnScreen.
        /// </summary>
        public Vector2 HalfPixelSize
        {
            get
            {
                return precaledHalfPixelSize;
            }
        }

        /// <summary>
        /// Calc half pixel size
        /// </summary>
        protected void CalcHalfPixelSize()
        {
            precaledHalfPixelSize = new Vector2(
                (1.0f / (float)texWidth) / 2.0f,
                (1.0f / (float)texHeight) / 2.0f);
        }

        /// <summary>
        /// XNA Framework Graphic Texture
        /// </summary>
        protected Texture2D internalXnaTexture;

        /// <summary>
        /// XNA Framework texture
        /// </summary>
        public virtual Texture2D XnaTexture
        {
            get
            {
                return internalXnaTexture;
            }
        }

        /// <summary>
        /// Loading succeeded?
        /// </summary>
        protected bool loaded = true;

        /// <summary>
        /// Error?
        /// </summary>
        protected string error = "";

        /// <summary>
        /// Is texture valid? Will be false if loading failed.
        /// </summary>
        public virtual bool Valid
        {
            get
            {
                return loaded &&
                    internalXnaTexture != null;
            }
        }
        /// <summary>
        /// Has alpha?
        /// </summary>
        protected bool hasAlpha = false;
        /// <summary>
        /// Has texture alpha information?
        /// </summary>
        public bool HasAlphaPixels
        {
            get
            {
                return hasAlpha;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create texture from given filename.
        /// </summary>
        /// <param name="setFilename">Set filename, must be relative and be a
        /// valid file in the textures directory.</param>
        public Texture(string setFilename)
        {
            if (alphaSprite == null)
                alphaSprite = new SpriteBatch(BaseGame.Device);

            if (additiveSprite == null)
                additiveSprite = new SpriteBatch(BaseGame.Device);

            if (String.IsNullOrEmpty(setFilename))
                throw new ArgumentNullException("setFilename",
                    "Unable to create texture without valid filename.");

            // Set content name (cut off extension!)
            texFilename = Path.GetFileNameWithoutExtension(setFilename);
            string fullFilename =
                Path.Combine(Directories.ContentDirectory + "\\textures", texFilename);

            // Try loading as 2d texture
            internalXnaTexture = BaseGame.Content.Load<Texture2D>(fullFilename);

            // Get info from the texture directly.
            texWidth = internalXnaTexture.Width;
            texHeight = internalXnaTexture.Height;

            // We will use alpha for Dxt3 and Dxt5 textures.
            hasAlpha = (internalXnaTexture.Format == SurfaceFormat.Dxt5 ||
                        internalXnaTexture.Format == SurfaceFormat.Dxt3);

            loaded = true;

            CalcHalfPixelSize();
        }

        /// <summary>
        /// Create texture, protected version for derived classes.
        /// </summary>
        protected Texture()
        {
        }

        /// <summary>
        /// Create texture by just assigning a Texture2D.
        /// </summary>
        /// <param name="tex">Tex</param>
        public Texture(Texture2D tex)
        {
            if (alphaSprite == null)
                alphaSprite = new SpriteBatch(BaseGame.Device);

            if (additiveSprite == null)
                additiveSprite = new SpriteBatch(BaseGame.Device);

            if (tex == null)
                throw new ArgumentNullException("tex");

            internalXnaTexture = tex;

            // Get info from the texture directly.
            texWidth = internalXnaTexture.Width;
            texHeight = internalXnaTexture.Height;

            loaded = true;

            // We will use alpha for Dxt3 and Dxt5 textures
            hasAlpha = (internalXnaTexture.Format == SurfaceFormat.Dxt5 ||
                        internalXnaTexture.Format == SurfaceFormat.Dxt3);

            CalcHalfPixelSize();
        }
        #endregion

        #region Disposing
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (internalXnaTexture != null)
                    internalXnaTexture.Dispose();
                internalXnaTexture = null;
            }

            loaded = false;
        }
        #endregion

        #region Render on screen
        /// <summary>
        /// Render texture at rect directly on screen using pixelRect.
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="pixelRect">Pixel rectangle</param>
        public void RenderOnScreen(Rectangle rect, Rectangle pixelRect)
        {
            alphaSprite.Draw(internalXnaTexture, rect, pixelRect, Color.White);
            //SpriteHelper.AddSpriteToRender(this, rect, pixelRect);
        }

        /// <summary>
        /// Render on screen
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="pixelX">Pixel x</param>
        /// <param name="pixelY">Pixel y</param>
        /// <param name="pixelWidth">Pixel width</param>
        /// <param name="pixelHeight">Pixel height</param>
        public void RenderOnScreen(Rectangle rect,
            int pixelX, int pixelY, int pixelWidth, int pixelHeight)
        {
            alphaSprite.Draw(internalXnaTexture, rect, new Rectangle(pixelX, pixelY,
                pixelWidth, pixelHeight), Color.White);
        }

        /// <summary>
        /// Render on screen
        /// </summary>
        /// <param name="pos">Position</param>
        public void RenderOnScreen(Point pos)
        {
            alphaSprite.Draw(internalXnaTexture,
                new Rectangle(pos.X, pos.Y, texWidth, texHeight),
                new Rectangle(0, 0, texWidth, texHeight), Color.White);
        }

        /// <summary>
        /// Render on screen
        /// </summary>
        /// <param name="renderRect">Render rectangle</param>
        public void RenderOnScreen(Rectangle renderRect)
        {
            alphaSprite.Draw(internalXnaTexture, renderRect, GfxRectangle, Color.White);
            //SpriteHelper.AddSpriteToRender(this,
            //    renderRect, GfxRectangle);
        }

        /// <summary>
        /// Render on screen relative for 1024x640 (16:9) graphics.
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="pixelRect">Pixel rectangle</param>
        public void RenderOnScreenRelative16To9(int relX, int relY,
            Rectangle pixelRect)
        {
            alphaSprite.Draw(internalXnaTexture, BaseGame.CalcRectangle(
                relX, relY, pixelRect.Width, pixelRect.Height),
                pixelRect, Color.White);

            //SpriteHelper.AddSpriteToRender(this,
            //    BaseGame.CalcRectangle(
            //    relX, relY, pixelRect.Width, pixelRect.Height),
            //    pixelRect);
        }

        /// <summary>
        /// Render on screen relative 1024x786 (4:3)
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="pixelRect">Pixel rectangle</param>
        public void RenderOnScreenRelative4To3(int relX, int relY,
            Rectangle pixelRect)
        {
            alphaSprite.Draw(internalXnaTexture, BaseGame.CalcRectangleKeep4To3(
                relX, relY, pixelRect.Width, pixelRect.Height),
                pixelRect, Color.White);

            //SpriteHelper.AddSpriteToRender(this,
            //    BaseGame.CalcRectangleKeep4To3(
            //    relX, relY, pixelRect.Width, pixelRect.Height),
            //    pixelRect);
        }

        /// <summary>
        /// Render on screen relative for 1600px width graphics.
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="pixelRect">Pixel rectangle</param>
        public void RenderOnScreenRelative1600(
            int relX, int relY, Rectangle pixelRect)
        {
            alphaSprite.Draw(internalXnaTexture, BaseGame.CalcRectangle1600(
                relX, relY, pixelRect.Width, pixelRect.Height),
                pixelRect, Color.White);

            //SpriteHelper.AddSpriteToRender(this,
            //    BaseGame.CalcRectangle1600(
            //    relX, relY, pixelRect.Width, pixelRect.Height),
            //    pixelRect);
        }

        /// <summary>
        /// Render texture at rect directly on screen using texture cordinates.
        /// This method allows to render with specific color and alpha values.
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="pixelRect">Pixel rectangle</param>
        /// <param name="color">Color</param>
        public void RenderOnScreen(Rectangle rect, Rectangle pixelRect,
            Color color)
        {
            alphaSprite.Draw(internalXnaTexture, rect, pixelRect, color);
            //SpriteHelper.AddSpriteToRender(this, rect, pixelRect, color);
        }

        /// <summary>
        /// Render on screen
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="pixelRect">Pixel rectangle</param>
        /// <param name="color">Color</param>
        /// <param name="blendState">Blend mode</param>
        public void RenderOnScreen(Rectangle rect, Rectangle pixelRect,
            Color color, BlendState blendState)
        {
            if (blendState == BlendState.Additive)
                additiveSprite.Draw(internalXnaTexture, rect, pixelRect, color);
            else
                alphaSprite.Draw(internalXnaTexture, rect, pixelRect, color);
            //SpriteHelper.AddSpriteToRender(this, rect, pixelRect, color, blendState);
        }
        #endregion

        #region Rendering on screen with rotation
        /// <summary>
        /// Render on screen with rotation
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="pixelRect">Pixel rectangle</param>
        /// <param name="rotation">Rotation</param>
        public void RenderOnScreenWithRotation(
            Rectangle rect, Rectangle pixelRect,
            float rotation, Vector2 rotationPoint)
        {
            alphaSprite.Draw(internalXnaTexture, rect, pixelRect, Color.White, rotation,
                rotationPoint, SpriteEffects.None, 0);
        }
        #endregion

        #region To string
        /// <summary>
        /// To string
        /// </summary>
        public override string ToString()
        {
            return "Texture(filename=" + texFilename +
                ", width=" + texWidth +
                ", height=" + texHeight +
                ", xnaTexture=" + (internalXnaTexture != null ? "valid" : "null") + ")";
        }
        #endregion
    }
}
