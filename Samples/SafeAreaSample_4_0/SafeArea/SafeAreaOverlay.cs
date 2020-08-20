#region File Description
//-----------------------------------------------------------------------------
// SafeAreaOverlay.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SafeArea
{
    /// <summary>
    /// Reusable component makes it easy to check whether your important
    /// graphics are positioned inside the title safe area, by superimposing
    /// a red border that marks the edges of the safe region.
    /// </summary>
    public class SafeAreaOverlay : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        Texture2D dummyTexture;


        /// <summary>
        /// Constructor.
        /// </summary>
        public SafeAreaOverlay(Game game)
            : base(game)
        {
            // Choose a high number, so we will draw on top of other components.
            DrawOrder = 1000;
        }


        /// <summary>
        /// Creates the graphics resources needed to draw the overlay.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a 1x1 white texture.
            dummyTexture = new Texture2D(GraphicsDevice, 1, 1);
            
            dummyTexture.SetData(new Color[] { Color.White });
        }


        /// <summary>
        /// Draws the title safe area.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Look up the current viewport and safe area dimensions.
            Viewport viewport = GraphicsDevice.Viewport;

            Rectangle safeArea = viewport.TitleSafeArea;

            int viewportRight = viewport.X + viewport.Width;
            int viewportBottom = viewport.Y + viewport.Height;

            // Compute four border rectangles around the edges of the safe area.
            Rectangle leftBorder = new Rectangle(viewport.X,
                                                 viewport.Y,
                                                 safeArea.X - viewport.X,
                                                 viewport.Height);

            Rectangle rightBorder = new Rectangle(safeArea.Right,
                                                  viewport.Y,
                                                  viewportRight - safeArea.Right,
                                                  viewport.Height);

            Rectangle topBorder = new Rectangle(safeArea.Left,
                                                viewport.Y,
                                                safeArea.Width,
                                                safeArea.Top - viewport.Y);

            Rectangle bottomBorder = new Rectangle(safeArea.Left,
                                                   safeArea.Bottom,
                                                   safeArea.Width,
                                                   viewportBottom - safeArea.Bottom);

            // Draw the safe area borders.
            Color translucentRed = Color.Red * 0.5f;

            spriteBatch.Begin();

            spriteBatch.Draw(dummyTexture, leftBorder,   translucentRed);
            spriteBatch.Draw(dummyTexture, rightBorder,  translucentRed);
            spriteBatch.Draw(dummyTexture, topBorder,    translucentRed);
            spriteBatch.Draw(dummyTexture, bottomBorder, translucentRed);

            spriteBatch.End();
        }
    }
}
