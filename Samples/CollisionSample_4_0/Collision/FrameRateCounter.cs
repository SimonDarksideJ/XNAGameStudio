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

namespace CollisionSample
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

        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;

        /// <summary>
        /// Constructor which initializes the Content Manager which is used later for loading the font for display.
        /// </summary>
        /// <param name="game"></param>
        public FrameRateCounter(Game game)
            : base(game)
        {
            content = new ContentManager(game.Services);
        }

        /// <summary>
        /// Graphics device objects are created here including the font.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = content.Load<SpriteFont>("content\\Font");
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void UnloadContent()
        {
            content.Unload();
        }

        /// <summary>
        /// The Update function is where the timing is measured and frame rate is computed.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }

        #region Draw
        /// <summary>
        /// Frame rate display occurs during the Draw method and uses the Font and Sprite batch to render text.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            frameCounter++;

            string fps = string.Format("fps: {0}", frameRate);

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, fps, new Vector2(32, 32), Color.White);
            spriteBatch.End();
        }
        #endregion
    }

}