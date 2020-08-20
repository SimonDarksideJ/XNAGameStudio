#region File Description
//-----------------------------------------------------------------------------
// ShapeRenderingSampleGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShapeRenderingSample
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ShapeRenderingSampleGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // The shapes that we'll be drawing
        BoundingBox box;
        BoundingFrustum frustum;
        BoundingSphere sphere;

        public ShapeRenderingSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;
#endif
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a box that is centered on the origin and extends from (-3, -3, -3) to (3, 3, 3)
            box = new BoundingBox(new Vector3(-3f), new Vector3(3f));

            // Create our frustum to simulate a camera sitting at the origin, looking down the X axis, with a 16x9
            // aspect ratio, a near plane of 1, and a far plane of 5
            Matrix frustumView = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitX, Vector3.Up);
            Matrix frustumProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 16f / 9f, 1f, 5f);
            frustum = new BoundingFrustum(frustumView * frustumProjection);

            // Create a sphere that is centered on the origin and has a radius of 3
            sphere = new BoundingSphere(Vector3.Zero, 3f);

            // Initialize our renderer
            DebugShapeRenderer.Initialize(GraphicsDevice);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allow the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
                this.Exit();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Figure out our camera location. We spin around the origin based on time.
            float angle = (float)gameTime.TotalGameTime.TotalSeconds;
            Vector3 eye = new Vector3((float)Math.Cos(angle * .5f), 0f, (float)Math.Sin(angle * .5f)) * 12f;
            eye.Y = 5f;

            // Construct our view and projection matrices
            Matrix view = Matrix.CreateLookAt(eye, Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, .1f, 1000f);

            // Add our shapes to be rendered
            DebugShapeRenderer.AddBoundingBox(box, Color.Yellow);
            DebugShapeRenderer.AddBoundingFrustum(frustum, Color.Green);
            DebugShapeRenderer.AddBoundingSphere(sphere, Color.Red);

            // Also add a triangle and a line
            DebugShapeRenderer.AddTriangle(new Vector3(-1f, 0f, 0f), new Vector3(1f, 0f, 0f), new Vector3(0f, 2f, 0f), Color.Purple);
            DebugShapeRenderer.AddLine(new Vector3(0f, 0f, 0f), new Vector3(3f, 3f, 3f), Color.Brown);

            // Render our shapes now
            DebugShapeRenderer.Draw(gameTime, view, projection);

            base.Draw(gameTime);
        }
    }
}
