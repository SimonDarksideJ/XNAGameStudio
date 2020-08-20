#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Devices.Sensors;

namespace AccelerometerSample
{
    /// <summary>
    /// A simple example showing how to use the accelerometer to move
    /// a sprite around the screen
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D asteroidTexture;
        Texture2D backgroundTexture;

        Vector2 logoPosition;
        Vector2 logoVelocity;                                

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;
            graphics.IsFullScreen = true;

            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Initialize the Accelerometer
            Accelerometer.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load the sprite's texture
            asteroidTexture = Content.Load<Texture2D>("asteroid");

            // load the background texture
            backgroundTexture = Content.Load<Texture2D>("space");

            // center the sprite on screen
            Viewport viewport = graphics.GraphicsDevice.Viewport;
            logoPosition = new Vector2(
                (viewport.Width - asteroidTexture.Width) / 2,
                (viewport.Height - asteroidTexture.Height) / 2);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //poll the acceleration value
            Vector3 acceleration = Accelerometer.GetState().Acceleration;

            logoVelocity.X += acceleration.X;
            logoVelocity.Y += -acceleration.Y;

            logoPosition += logoVelocity;

            // keep the sprite on the screen - clamp X
            Viewport viewport = graphics.GraphicsDevice.Viewport;

            if (logoPosition.X < 0)
            {
                logoPosition.X = 0;
                logoVelocity.X = 0;
            }
            else if (logoPosition.X > viewport.Width - asteroidTexture.Width)
            {
                logoPosition.X = viewport.Width - asteroidTexture.Width;
                logoVelocity.X = 0;
            }

            // keep the sprite on the screen - clamp Y
            if (logoPosition.Y < 0)
            {
                logoPosition.Y = 0;
                logoVelocity.Y = 0;
            }
            else if (logoPosition.Y > viewport.Height - asteroidTexture.Height)
            {
                logoPosition.Y = viewport.Height - asteroidTexture.Height;
                logoVelocity.Y = 0;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.  In this case, render
        /// the space background and asteroid at the desired coordinates
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            spriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);

            spriteBatch.Draw(asteroidTexture, logoPosition, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
