#region File Description
//-----------------------------------------------------------------------------
// SafeAreaGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace SafeArea
{
    /// <summary>
    /// Sample showing how to handle television safe areas in an XNA Framework game.
    /// </summary>
    public class SafeAreaGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        const int ScreenWidth = 1280;
        const int ScreenHeight = 720;

        GraphicsDeviceManager graphics;
        SafeAreaOverlay safeAreaOverlay;
        AlignedSpriteBatch spriteBatch;
        SpriteFont font;
        
        Texture2D catTexture;
        Texture2D backgroundTexture;

        Vector2 catPosition;
        Vector2 catVelocity;

        Vector2 cameraPosition;

        KeyboardState currentKeyboardState;
        GamePadState currentGamePadState;

        KeyboardState previousKeyboardState;
        GamePadState previousGamePadState;

        #endregion

        #region Initialization


        public SafeAreaGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;

            // In debug Xbox builds, we add a SafeAreaOverlay so we can easily
            // check whether our important graphics are positioned inside the
            // title safe area. But we don't bother with this on other platforms
            // (where the entire screen is safe) or in release mode builds
            // (because we don't want it to show up in the finished game).
#if XBOX && DEBUG
            safeAreaOverlay = new SafeAreaOverlay(this);
            Components.Add(safeAreaOverlay);
#else
            safeAreaOverlay = null;
#endif
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new AlignedSpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Font");

            catTexture = Content.Load<Texture2D>("Cat");
            backgroundTexture = Content.Load<Texture2D>("Background");
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            UpdateCat();
            UpdateCamera();

            base.Update(gameTime);
        }


        /// <summary>
        /// Moves the cat sprite around the screen.
        /// </summary>
        void UpdateCat()
        {
            const float speedOfCat = 0.75f;
            const float catFriction = 0.9f;

            // Apply gamepad input.
            Vector2 flipY = new Vector2(1, -1);

            catVelocity += currentGamePadState.ThumbSticks.Left * flipY * speedOfCat;

            // Apply keyboard input.
            if (currentKeyboardState.IsKeyDown(Keys.Left))
                catVelocity.X -= speedOfCat;

            if (currentKeyboardState.IsKeyDown(Keys.Right))
                catVelocity.X += speedOfCat;

            if (currentKeyboardState.IsKeyDown(Keys.Up))
                catVelocity.Y -= speedOfCat;

            if (currentKeyboardState.IsKeyDown(Keys.Down))
                catVelocity.Y += speedOfCat;

            // Apply velocity and friction.
            catPosition += catVelocity;
            catVelocity *= catFriction;
        }


        /// <summary>
        /// Updates the camera position, scrolling the
        /// screen if the cat gets too close to the edge.
        /// </summary>
        void UpdateCamera()
        {
            // How far away from the camera should we allow the cat
            // to move before we scroll the camera to follow it?
            Vector2 maxScroll = new Vector2(ScreenWidth, ScreenHeight) / 2;

            // Apply a safe area to prevent the cat getting too close to the edge
            // of the screen. Note that this is even more restrictive than the 80%
            // safe area used for the overlays, because we want to start scrolling
            // even before the cat gets right up to the edge of the legal area.
            const float catSafeArea = 0.7f;

            maxScroll *= catSafeArea;

            // Adjust for the size of the cat sprite, so we will start
            // scrolling based on the edge rather than center of the cat.
            maxScroll -= new Vector2(catTexture.Width, catTexture.Height) / 2;

            // Make sure the camera stays within the desired distance of the cat.
            Vector2 min = catPosition - maxScroll;
            Vector2 max = catPosition + maxScroll;

            cameraPosition.X = MathHelper.Clamp(cameraPosition.X, min.X, max.X);
            cameraPosition.Y = MathHelper.Clamp(cameraPosition.Y, min.Y, max.Y);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Work out how far to scroll based on the current camera position.
            Vector2 screenCenter = new Vector2(ScreenWidth, ScreenHeight) / 2;
            Vector2 scrollOffset = screenCenter - cameraPosition;

            // Draw the background, cat, and text overlays.
            spriteBatch.Begin();

            DrawBackground(scrollOffset);
            DrawCat(scrollOffset);
            DrawOverlays();

            spriteBatch.End();

            base.Draw(gameTime);
        }


        /// <summary>
        /// Draws the repeating background texture.
        /// </summary>
        void DrawBackground(Vector2 scrollOffset)
        {
            // Work out the position of the top left visible tile.
            int tileX = (int)scrollOffset.X % backgroundTexture.Width;
            int tileY = (int)scrollOffset.Y % backgroundTexture.Height;

            if (tileX > 0)
                tileX -= backgroundTexture.Width;

            if (tileY > 0)
                tileY -= backgroundTexture.Height;

            // Draw however many repeating tiles are needed to cover the screen.
            for (int x = tileX; x < ScreenWidth; x += backgroundTexture.Width)
            {
                for (int y = tileY; y < ScreenHeight; y += backgroundTexture.Height)
                {
                    spriteBatch.Draw(backgroundTexture, new Vector2(x, y), Color.White);
                }
            }
        }


        /// <summary>
        /// Draws the cat sprite.
        /// </summary>
        void DrawCat(Vector2 scrollOffset)
        {
            Vector2 catCenter = new Vector2(catTexture.Width, catTexture.Height) / 2;

            Vector2 position = catPosition - catCenter + scrollOffset;

            spriteBatch.Draw(catTexture, position, Color.White);
        }


        /// <summary>
        /// Draws text overlays on top of the game graphics.
        /// </summary>
        void DrawOverlays()
        {
            Rectangle safeArea = GraphicsDevice.Viewport.TitleSafeArea;

            // Draw labels in the four corners of the screen,
            // aligned to the edges of the safe area.
            spriteBatch.DrawString(font, "Top Left",
                                   new Vector2(safeArea.Left, safeArea.Top),
                                   Color.White, Alignment.TopLeft);

            spriteBatch.DrawString(font, "Top Right",
                                   new Vector2(safeArea.Right, safeArea.Top),
                                   Color.White, Alignment.TopRight);
            
            spriteBatch.DrawString(font, "Bottom Left",
                                   new Vector2(safeArea.Left, safeArea.Bottom),
                                   Color.White, Alignment.BottomLeft);
            
            spriteBatch.DrawString(font, "Bottom Right",
                                   new Vector2(safeArea.Right, safeArea.Bottom),
                                   Color.White, Alignment.BottomRight);

            // Draw a prompt saying how to toggle the safe area overlay.
            if (safeAreaOverlay != null)
            {
                spriteBatch.DrawString(font, "Press A to toggle the safe area overlay",
                                       new Vector2(safeArea.Center.X, safeArea.Top),
                                       Color.White, Alignment.TopCenter);
            }
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game and toggling the safe area overlay.
        /// </summary>
        private void HandleInput()
        {
            previousKeyboardState = currentKeyboardState;
            previousGamePadState = currentGamePadState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.IsButtonDown(Buttons.Back))
            {
                Exit();
            }

            // Check for showing or hiding the safe area overlay.
            if (safeAreaOverlay != null)
            {
                if ((currentKeyboardState.IsKeyDown(Keys.A) &&
                     previousKeyboardState.IsKeyUp(Keys.A)) ||
                    (currentGamePadState.IsButtonDown(Buttons.A) &&
                     previousGamePadState.IsButtonUp(Buttons.A)))
                {
                    safeAreaOverlay.Visible = !safeAreaOverlay.Visible;
                }
            }
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (SafeAreaGame game = new SafeAreaGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
