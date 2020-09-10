#region File Description
//-----------------------------------------------------------------------------
// FrameRateCounter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FrameRateCounterComponent
{
    /// <summary>
    /// General Timing and Frame Rate Display Component.
    /// Add this to the GameComponentCollection to display the frame rate
    /// </summary>
    public class FrameRateCounter : DrawableGameComponent
    {
        ContentManager  content;
        SpriteBatch     spriteBatch;
        SpriteFont      spriteFont;

        Vector2 fpsScreenLocation;
        int frameRate = 0;
        int frameCounter = 0;
        long elapsedTime = 0;    // Elapsed time in ticks

        #region public FrameRateCounter(Game game)
        /// <summary>
        /// Constructor which initializes the Content Manager which is used later for loading the font for display.
        /// </summary>
        /// <param name="game"></param>
        public FrameRateCounter(Game game)
            : base(game)
        {
            content = new ContentManager(game.Services);
        }
        #endregion

        #region protected override void LoadContent()
        /// <summary>
        /// Graphics device objects are created here including the font.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = content.Load<SpriteFont>("content\\Font");

            fpsScreenLocation = new Vector2(320, 32);
        }
        #endregion

        #region protected override void UnloadContent()
        /// <summary>
        /// Content Unloading
        /// </summary>
        protected override void UnloadContent()
        {
            content.Unload();
        }
        #endregion

        #region public override void Update(GameTime gameTime)
        public override void Update(GameTime gameTime)
        {
            // Add the elapsed time to the total
            elapsedTime += gameTime.ElapsedGameTime.Ticks;
            // Has a second gone by?
            if (elapsedTime > TimeSpan.TicksPerSecond)
            {
                // Remove the second
                elapsedTime -= TimeSpan.TicksPerSecond;
                // Update the frame rate counter
                frameRate = frameCounter;
                // Reset the counter (Updated in Draw())
                frameCounter = 0;
            }
        }
        #endregion

        #region public override void Draw(GameTime gameTime)
        /// <summary>
        /// Frame rate display occurs during the Draw method and uses the Font and Sprite batch to render text.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            frameCounter++;

            string fps = string.Format("fps: {0}", frameRate);

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, fps, fpsScreenLocation, Color.White);
            spriteBatch.End();
        }
        #endregion
    }

}