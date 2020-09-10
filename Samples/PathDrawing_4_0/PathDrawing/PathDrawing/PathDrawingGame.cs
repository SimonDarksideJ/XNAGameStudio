#region File Description
//-----------------------------------------------------------------------------
// PathDrawingGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace PathDrawing
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PathDrawingGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;

        // We need a SpriteBatch to do our main drawing
        private SpriteBatch spriteBatch;

        // We also use the PrimitiveBatch to draw the lines for our path.
        private PrimitiveBatch primitiveBatch;

        // A font for drawing our instruction text
        private SpriteFont font;

        // A texture we draw for the ground
        private Texture2D groundTexture;

        // Sets the number of pixels the ground occupies before repeating. Increase to "zoom in" on
        // the ground or decrease to "zoom out".
        private const int groundSize = 300;

        // The tank that will drive around, following our path.
        private Tank tank;

        // Whether or not the user is drawing the path for the Tank to follow
        private bool drawingWaypoints = false;

        public PathDrawingGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;

            // We only care about the FreeDrag gesture for this sample
            TouchPanel.EnabledGestures = GestureType.FreeDrag;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create the PrimitiveBatch for drawing our path
            primitiveBatch = new PrimitiveBatch(GraphicsDevice);

            // Load our font and ground
            font = Content.Load<SpriteFont>("Font");
            groundTexture = Content.Load<Texture2D>("ground");

            // Create the tank
            tank = new Tank(GraphicsDevice, Content);
            tank.Reset(new Vector2(100));
            tank.MoveSpeed = 225f;
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

            // Update the tank
            tank.Update(gameTime);

            // Get the current state of touch input
            TouchCollection touches = TouchPanel.GetState();

            // If we have at least one touch...
            if (touches.Count > 0)
            {
                // If the primary touch is pressed and in the tank, we start drawing our path
                if (touches[0].State == TouchLocationState.Pressed && tank.HitTest(touches[0].Position))
                {
                    // Clear the waypoints to start a new path
                    tank.Waypoints.Clear();

                    // We're now drawing waypoints
                    drawingWaypoints = true;

                    // Use the touch location as the first waypoint
                    tank.Waypoints.Enqueue(touches[0].Position);
                }

                // Otherwise if the primary touch is released, we stop drawing our path
                else if (touches[0].State == TouchLocationState.Released)
                {
                    drawingWaypoints = false;
                }
            }

            // Read all of the gestures
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                // If we have a FreeDrag gesture...
                if (gesture.GestureType == GestureType.FreeDrag)
                {
                    // If we're drawing waypoints and the drag gesture has moved from the last location,
                    // enqueue the position of the gesture as the next waypoint.
                    if (drawingWaypoints && gesture.Delta != Vector2.Zero)
                    {
                        tank.Waypoints.Enqueue(gesture.Position);
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // First draw our ground
            DrawGround();

            // Next draw the path
            DrawPath();

            // Draw our instruction text and tank
            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Drag a path from the tank to have him drive around.", new Vector2(5), Color.White);
            tank.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Helper method to draw our ground texture.
        /// </summary>
        private void DrawGround()
        {
            // Draw the ground using a LinearWrap state so we can repeat the texture across the screen
            spriteBatch.Begin(0, null, SamplerState.LinearWrap, null, null);

            // Compute the source rectangle based on our viewport size and ground scale
            Rectangle source = new Rectangle();
            source.Width = (GraphicsDevice.Viewport.Width / groundSize) * groundTexture.Width;
            source.Height = (GraphicsDevice.Viewport.Height / groundSize) * groundTexture.Height;

            // Draw the ground using our source rectangle which will cause it to wrap across the screen
            spriteBatch.Draw(groundTexture, GraphicsDevice.Viewport.Bounds, source, Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// Helper method to draw the path using PrimitiveBatch.
        /// </summary>
        private void DrawPath()
        {
            // Start drawing lines
            primitiveBatch.Begin(PrimitiveType.LineList);

            // Add the tank's position as the first vertex
            primitiveBatch.AddVertex(tank.Location, Color.White);

            for (int i = 1; i < tank.Waypoints.Count; i++)
            {
                // Add the next waypoint location to our line list
                primitiveBatch.AddVertex(tank.Waypoints[i], Color.White);

                // If we're not at the end of our waypoint list, add this point again to act as the
                // first point for the next line.
                if (i < tank.Waypoints.Count - 1)
                    primitiveBatch.AddVertex(tank.Waypoints[i], Color.White);
            }

            primitiveBatch.End();
        }
    }
}
