#region File Description
//-----------------------------------------------------------------------------
// FrameRateCounter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CpuSkinningDemo
{
    /// <summary>
    /// A basic DrawableGameComponent that displays the frame rate to the screen.
    /// </summary>
    public class FrameRateCounter : DrawableGameComponent
    {
        private ContentManager content;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;

        private Vector2 fpsScreenLocation = new Vector2(32, 32);
        private int frameRate = 0;
        private int frameCounter = 0;
        private float elapsedTime = 0f;
        private string fpsString = "fps: ??";

        public FrameRateCounter(Game game)
            : base(game)
        {
            content = new ContentManager(game.Services, "Content");
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = content.Load<SpriteFont>("font");
        }

        protected override void UnloadContent()
        {
            content.Unload();
        }

        public override void Draw(GameTime gameTime)
        {
            frameCounter++;
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (elapsedTime >= 1f)
            {
                elapsedTime -= 1f;
                frameRate = frameCounter;
                frameCounter = 0;

                float averageFrameLength = 1000f / frameRate;
                fpsString = string.Format("fps: {0} ({1} ms)", frameRate, averageFrameLength);
            }

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, fpsString, fpsScreenLocation, Color.White);
            spriteBatch.End();
        }
    }

}