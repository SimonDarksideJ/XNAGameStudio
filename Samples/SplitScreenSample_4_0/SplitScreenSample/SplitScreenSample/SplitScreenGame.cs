#region File Description
//-----------------------------------------------------------------------------
// SplitScreenGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace SplitScreenSample
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SplitScreenGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        // We use SpriteBatch to draw a dividing line between our viewports to make it
        // easier to visualize.
        SpriteBatch spriteBatch;
        Texture2D blank;

        // Define the viewports that we wish to render to. We will draw two viewports:
        // - The top half of the screen
        // - The bottom half of the screen
        Viewport playerOneViewport;
        Viewport playerTwoViewport;

        // Each viewport will need a different view and projection matrix in
        // order for them to render the scene from different cameras.
        Matrix playerOneView, playerOneProjection;
        Matrix playerTwoView, playerTwoProjection;

        // We're leveraging the Tank class from the Simple Animation Sample to help
        // illustrate the effect of multiple viewports.
        Tank tank;
        
        public SplitScreenGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            graphics.IsFullScreen = true;
            TargetElapsedTime = TimeSpan.FromTicks(333333);
#endif
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create and load our tank
            tank = new Tank();
            tank.Load(Content);
            
            // Create the SpriteBatch and texture we'll use to draw our viewport edges.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            blank = new Texture2D(GraphicsDevice, 1, 1);
            blank.SetData(new[] { Color.White });

            // Create the viewports
            playerOneViewport = new Viewport
            {
                MinDepth = 0,
                MaxDepth = 1,
                X = 0,
                Y = 0,
                Width = GraphicsDevice.Viewport.Width,
                Height = GraphicsDevice.Viewport.Height / 2,
            };
            playerTwoViewport = new Viewport
            {
                MinDepth = 0,
                MaxDepth = 1,
                X = 0,
                Y = GraphicsDevice.Viewport.Height / 2,
                Width = GraphicsDevice.Viewport.Width,
                Height = GraphicsDevice.Viewport.Height / 2,
            };

            // Create the view and projection matrix for each of the viewports
            playerOneView = Matrix.CreateLookAt(
                new Vector3(400f, 900f, 200f), 
                new Vector3(-100f, 0f, 0f), 
                Vector3.Up);
            playerOneProjection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, playerOneViewport.AspectRatio, 10f, 5000f);

            playerTwoView = Matrix.CreateLookAt(
                new Vector3(0f, 800f, 800f), 
                Vector3.Zero, 
                Vector3.Up);
            playerTwoProjection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, playerTwoViewport.AspectRatio, 10f, 5000f);
        }
        
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (keyState.IsKeyDown(Keys.Escape) || gamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }
            
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            // We animate the tank just like the Simple Animation Sample, to show that the same
            // object really is being drawn on both viewports.
            tank.WheelRotation = time * 5;
            tank.SteerRotation = (float)Math.Sin(time * 0.75f) * 0.5f;
            tank.TurretRotation = (float)Math.Sin(time * 0.333f) * 1.25f;
            tank.CannonRotation = (float)Math.Sin(time * 0.25f) * 0.333f - 0.333f;
            tank.HatchRotation = MathHelper.Clamp((float)Math.Sin(time * 2) * 2, -1, 0);

            // We'll rotate our player two around the tank
            playerTwoView = Matrix.CreateLookAt(
                new Vector3((float)Math.Cos(time), 1f, (float)Math.Sin(time)) * 800f,
                Vector3.Zero,
                Vector3.Up);

            base.Update(gameTime);
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Clear the backbuffer. We don't clear inside our DrawScene method because Clear is based
            // on the backbuffer and not the current viewport. This means that if we called Clear for
            // each viewport, only the last one would appear on screen.
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Here we want to reset some render states our 3D rendering needs that our SpriteBatch changed
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Draw our scene with all of our viewports and their respective view/projection matrices.
            DrawScene(gameTime, playerOneViewport, playerOneView, playerOneProjection);
            DrawScene(gameTime, playerTwoViewport, playerTwoView, playerTwoProjection);

            // Now we'll draw the viewport edges on top so we can visualize the viewports more easily.
            DrawViewportEdges(playerOneViewport);
            DrawViewportEdges(playerTwoViewport);

            base.Draw(gameTime);
        }

        /// <summary>
        /// DrawScene is our main rendering method. By rendering the entire scene inside of this method,
        /// we enable ourselves to be able to render the scene using any viewport we may want.
        /// </summary>
        private void DrawScene(GameTime gameTime, Viewport viewport, Matrix view, Matrix projection)
        {
            // Set our viewport. We store the old viewport so we can restore it when we're done in case
            // we want to render to the full viewport at some point.
            Viewport oldViewport = GraphicsDevice.Viewport;
            GraphicsDevice.Viewport = viewport;
            
            // Here we'd want to draw our entire scene. For this sample, that's just the tank.
            tank.Draw(Matrix.Identity, view, projection);

            // Now that we're done, set our old viewport back on the device
            GraphicsDevice.Viewport = oldViewport;
        }

        /// <summary>
        /// A helper to draw the edges of a viewport.
        /// </summary>
        private void DrawViewportEdges(Viewport viewport)
        {
            const int edgeWidth = 2;

            // We now compute four rectangles that make up our edges
            Rectangle topEdge = new Rectangle(
                viewport.X - edgeWidth / 2,
                viewport.Y - edgeWidth / 2,
                viewport.Width + edgeWidth,
                edgeWidth);
            Rectangle bottomEdge = new Rectangle(
                viewport.X - edgeWidth / 2,
                viewport.Y + viewport.Height - edgeWidth / 2,
                viewport.Width + edgeWidth,
                edgeWidth);
            Rectangle leftEdge = new Rectangle(
                viewport.X - edgeWidth / 2,
                viewport.Y - edgeWidth / 2,
                edgeWidth,
                viewport.Height + edgeWidth);
            Rectangle rightEdge = new Rectangle(
                viewport.X + viewport.Width - edgeWidth / 2,
                viewport.Y - edgeWidth / 2,
                edgeWidth,
                viewport.Height + edgeWidth);

            // We just use SpriteBatch to draw the four rectangles
            spriteBatch.Begin();
            spriteBatch.Draw(blank, topEdge, Color.Black);
            spriteBatch.Draw(blank, bottomEdge, Color.Black);
            spriteBatch.Draw(blank, leftEdge, Color.Black);
            spriteBatch.Draw(blank, rightEdge, Color.Black);
            spriteBatch.End();
        }
    }
}
