#region File Information
//-----------------------------------------------------------------------------
// LayoutSample.cs
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
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace LayoutSample
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class LayoutSample : Microsoft.Xna.Framework.Game
    {
        #region Fields
        
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D directions;
        SpriteFont font;

        // Is the orientation locked?
        bool orientationLocked = false;

        // Do we allow dynamically locking/unlocking the orientation?
        bool enableOrientationLocking = false;

        #endregion

        #region Initialization

        public LayoutSample()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // DISCLAMER:
            // Four different scenarios for initializing orientation support are presented below.
            // The first two scenarios are the most common and are recommended for use in most
            // cases; the third scenario is a special case to show the hardware scalar work
            // result; the fourth scenario is for special cases where games want to dynamically
            // support both the landscape and portrait orientations

            // Scenario #1 (default):
            // SupportedOrientations not changed, so the game will support Landscape orientation only

            // Scenario #2: 
            // SupportedOrientations changed to support the Portrait mode only
            // (Uncomment the following two lines)
            // graphics.PreferredBackBufferWidth = 480;
            // graphics.PreferredBackBufferHeight = 800;

            // Scenario #3:
            // SupportedOrientations not changed (default), thus game will support the Landscape 
            // orientations only, but resolution set to half. This makes the hardware scalar work and 
            // automatically scales the presentation to the device's physical resolution
            // (Uncomment the following two lines):
            // graphics.PreferredBackBufferWidth = 400;
            // graphics.PreferredBackBufferHeight = 240;

            // Scenario #4: 
            // Game supports all possible orientations and enablyes dynamically locking/unlocking the
            // orientation.
            // (Uncomment the following lines):
            // graphics.SupportedOrientations = DisplayOrientation.Portrait |
            //                                  DisplayOrientation.LandscapeLeft |
            //                                  DisplayOrientation.LandscapeRight;
            // enableOrientationLocking = true;

            // Switch to full screen mode
            graphics.IsFullScreen = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // For scenario #4, we handle locking/unlocking the orientation by detecting the tap gesture.
            TouchPanel.EnabledGestures = GestureType.Tap;

            base.Initialize();
        }

        #endregion

        #region Load and Unload

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            directions = Content.Load<Texture2D>("directions");
            font = Content.Load<SpriteFont>("Font");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Nothing to unload in this sample
        }

        #endregion

        #region Update and Render

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

            // If we enable locking/unlocking the orientation...
            if (enableOrientationLocking)
            {
                // Read the gestures from the touch panel
                while (TouchPanel.IsGestureAvailable)
                {
                    GestureSample gesture = TouchPanel.ReadGesture();

                    // If the user tapped on the screen...
                    if (gesture.GestureType == GestureType.Tap)
                    {
                        // Toggle the orientation locked state
                        orientationLocked = !orientationLocked;

                        if (orientationLocked)
                        {
                            // If we're locking the orientation, we want to store the current
                            // orientation as well as the current viewport size. we have to
                            // store the viewport size because when we call ApplyChanges(),
                            // our back buffer may change and we want to make sure that it
                            // remains at the current size, rather than any previously set
                            // preferred size.
                            graphics.SupportedOrientations = Window.CurrentOrientation;
                            graphics.PreferredBackBufferWidth = GraphicsDevice.Viewport.Width;
                            graphics.PreferredBackBufferHeight = GraphicsDevice.Viewport.Height;

                        }
                        else
                        {
                            // If we're unlocking the orientation, we simply set our 
                            // supported orientations back to all orientations
                            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft |
                                                             DisplayOrientation.LandscapeRight |
                                                             DisplayOrientation.Portrait;
                        }

                        // ApplyChanges needs to be called if SupportedOrientations was changed
                        graphics.ApplyChanges();
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

            spriteBatch.Begin();

            // Draw the directions texture centered on the screen
            Vector2 position = new Vector2(
               GraphicsDevice.Viewport.Width / 2 - directions.Width / 2,
               GraphicsDevice.Viewport.Height / 2 - directions.Height / 2);

            spriteBatch.Draw(directions, position, Color.White);

            // If we allow locking/unlocking of the orientation, draw the instructions to
            // the screen.
            if (enableOrientationLocking)
            {
                // Create a string of our current state
                string currentState = orientationLocked 
                    ? "Orientation: Locked" 
                    : "Orientation: Unlocked";

                // Create a string for the instructions
                string instructions = orientationLocked 
                    ? "Tap to unlock orientation." 
                    : "Tap to lock orientation.";

                // Draw the text to the screen
                spriteBatch.DrawString(font, currentState, new Vector2(10, 10), Color.White);
                spriteBatch.DrawString(font, instructions, new Vector2(10, 25), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion
    }
}
