#region File Description
//-----------------------------------------------------------------------------
// TransformedCollisionTestGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace TransformedCollisionTest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TransformedCollisionTestGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;

        // Sprites
        TransformedSprite spriteF = new TransformedSprite();
        TransformedSprite spriteR = new TransformedSprite();

        // Helper for drawing points
        Texture2D pointTexture;
        Vector2 pointOrigin;

        // Screen origin of the collision space visualization
        Vector2 collisionSpaceOrigin;

        // Remember collisions to control background color
        bool collision;

#if !XBOX360 // Mouse is not supported on the 360
        // Retain the previous scroll wheel value in order to calculate a delta
        int lastScrollWheelValue;
#endif


        public TransformedCollisionTestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }


        protected override void Initialize()
        {
            base.Initialize();

            // Start with the sprites not overlapping
            spriteR.Position.X = spriteF.Texture.Width;
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            pointTexture = Content.Load<Texture2D>("Point");
            // The point should be drawn centered
            pointOrigin =
                new Vector2(pointTexture.Width / 2, pointTexture.Height / 2);

            spriteF.Texture = Content.Load<Texture2D>("F");
            spriteR.Texture = Content.Load<Texture2D>("R");

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            // The gray collision space visualization should be drawn centered
            // vertically and slighty away from the left edge of the window.
            Viewport viewport = graphics.GraphicsDevice.Viewport;
            collisionSpaceOrigin =
                new Vector2(viewport.Width * .15f, viewport.Height / 2);
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            // Rebuild the sprite world transforms
            spriteF.UpdateTransform();
            spriteR.UpdateTransform();

            // Invoke the collision test
            collision = spriteF.IntersectPixels(spriteR);


            base.Update(gameTime);
        }

        private void HandleInput()
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

#if !XBOX360 // Mouse is not supported on the 360, so don't handle mouse/keyboard

            MouseState mouseState = Mouse.GetState();            

            // We don't want mouse movements and such when not active
            if (IsActive)
            {
                KeyboardState keyboardState = Keyboard.GetState();

                // Allow exiting with keyboard
                if (keyboardState.IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                }
                // Left mouse button moves the F object
                else if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    HandleInput(spriteF, keyboardState, mouseState);
                }
                // Right mouse button moves the R object
                else if (mouseState.RightButton == ButtonState.Pressed)
                {
                    HandleInput(spriteR, keyboardState, mouseState);
                }
            }

            lastScrollWheelValue = mouseState.ScrollWheelValue;

#endif
            // Allow exiting with game pad
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                Exit();

            // Left trigger/shoulder controls sprite F
            HandleInput(gamePadState.Triggers.Left, gamePadState.Buttons.LeftShoulder,
                spriteF, gamePadState);
            // Right trigger/shoulder controls sprite R
            HandleInput(gamePadState.Triggers.Right, gamePadState.Buttons.RightShoulder,
                spriteR, gamePadState);
        }

#if !XBOX360
        private void HandleInput(TransformedSprite sprite,
            KeyboardState keyboardState, MouseState mouseState)
        {
            const float rotateAndScaleSpeed = 0.02f;
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            float rotationDelta =
                (mouseState.ScrollWheelValue - lastScrollWheelValue) * 0.005f;

            // Click and drag to move sprite
            // Hold left control when dragging to move sprite's origin
            if (keyboardState.IsKeyDown(Keys.LeftControl))
                sprite.Origin = sprite.Position - mousePosition;
            else
                sprite.Position = mousePosition;

            // Hold the left mouse button and spin the mouse wheel to rotate
            // Do it while holding alt to scale
            if (keyboardState.IsKeyDown(Keys.LeftAlt))
                sprite.Scale += rotationDelta;
            else
                sprite.Rotation += rotationDelta;

            // Also allow the left and right arrow keys to rotate
            if (keyboardState.IsKeyDown(Keys.Left))
                sprite.Rotation -= rotateAndScaleSpeed;
            else if (keyboardState.IsKeyDown(Keys.Right))
                sprite.Rotation += rotateAndScaleSpeed;
            // And the up and down arrow keys to scale
            if (keyboardState.IsKeyDown(Keys.Up))
                sprite.Scale += rotateAndScaleSpeed;
            else if (keyboardState.IsKeyDown(Keys.Down))
                sprite.Scale -= rotateAndScaleSpeed;
        }
#endif

        private static void HandleInput(float trigger, ButtonState shoulder,
            TransformedSprite spriteF, GamePadState gamePadState)
        {
            const float rotateAndScaleSpeed = 0.04f;
            Vector2 gamePadMovement = gamePadState.ThumbSticks.Left * 3.0f;
            gamePadMovement.Y = -gamePadMovement.Y;

            // Hold the trigger to transform
            if (trigger > 0)
            {
                // Left stick moves
                spriteF.Position += gamePadMovement;
                // Right stick rotates and scales
                spriteF.Rotation +=
                    gamePadState.ThumbSticks.Right.X * rotateAndScaleSpeed;
                spriteF.Scale +=
                    gamePadState.ThumbSticks.Right.Y * rotateAndScaleSpeed;
            }
            // Hold the shoulder to move the pivot point
            else if (shoulder == ButtonState.Pressed)
            {
                // with the left thumbstick
                spriteF.Origin += gamePadMovement;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Make the background red when the objects are colliding
            if (collision)
                graphics.GraphicsDevice.Clear(Color.Red);
            else
                graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();


            // Calculate R's transform parameters in F's local space
            Matrix transformRtoF = spriteR.Transform * Matrix.Invert(spriteF.Transform);
            Vector2 position =
                new Vector2(transformRtoF.Translation.X, transformRtoF.Translation.Y);
            float rotation = spriteR.Rotation - spriteF.Rotation;
            float scale = spriteR.Scale / spriteF.Scale;

            // Draw light gray F and R in F's local space
            spriteBatch.Draw(spriteF.Texture, collisionSpaceOrigin, null,
                Color.Gray, 0, Vector2.Zero, 1, SpriteEffects.None, 0.2f);
            spriteBatch.Draw(spriteR.Texture, collisionSpaceOrigin + position, null,
                Color.Gray, rotation, Vector2.Zero, scale, SpriteEffects.None, 0.2f);

            // Draw origin's for F and R in F's local space
            DrawPoint(collisionSpaceOrigin, Color.Green);
            DrawPoint(collisionSpaceOrigin + position, Color.Brown);            


            // Draw F and R in world space
            spriteF.Draw(spriteBatch);
            spriteR.Draw(spriteBatch);            

            // Draw origins for F and R in world space
            DrawPoint(spriteF.Position, Color.Yellow);
            DrawPoint(spriteR.Position, Color.Yellow);           


            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawPoint(Vector2 position, Color color)
        {
            spriteBatch.Draw(pointTexture, position, null, color, 0, pointOrigin, 1,
                SpriteEffects.None, 0.1f);
        }
        
    }
}
