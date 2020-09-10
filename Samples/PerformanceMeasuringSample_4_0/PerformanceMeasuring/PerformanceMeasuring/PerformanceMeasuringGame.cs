#region File Description
//-----------------------------------------------------------------------------
// PerformanceMeasuringGame.cs
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
using PerformanceMeasuring.GameDebugTools;

namespace PerformanceMeasuring
{
    /// <summary>
    /// This sample game shows how to use the GameDebugTools to measure the performance of a game,
    /// as well as how the number of objects and interactions between them can affect performance.
    /// </summary>
    public class PerformanceMeasuringGame : Game
    {
        // The maximum number of spheres in our world
        const int maximumNumberOfSpheres = 200;

        GraphicsDeviceManager graphics;

        // A SpriteBatch, font, and blank texture for drawing our instruction text
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D blank;

        // The text we draw as instructions.
        string instructions =
#if WINDOWS_PHONE
            "Tap - Toggle collisions\nDrag up/down - Change number of spheres";
#else
            "X - Toggle collisions\nUp - Increase number of spheres\nDown - Decrease number of spheres";
#endif

        // The size of the world. The world is a bounding box ranging from -worldSize to worldSize on
        // the X and Z axis, and from 0 to worldSize on the Y axis.
        const float worldSize = 20f;

        // A model for our ground
        Model ground;

        // An array of spheres and the number of currently active spheres
        Sphere[] spheres = new Sphere[maximumNumberOfSpheres];
        int activeSphereCount = 50;

        // Are we colliding the spheres against each other?
        bool collideSpheres = true;

        // Various input states for changing the simulation
        GamePadState gamePad, gamePadPrev;
        KeyboardState keyboard, keyboardPrev;

        public PerformanceMeasuringGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Initialize the DebugSystem and change the visibility of the FpsCounter and TimeRuler.
            
            // The FpsCounter will show us our current "frames per second". On Windows Phone we can
            // have a maximum of 30 frames per second. On Windows and Xbox, we can reach 60 frames 
            // per second with our current game settings.

            // The TimeRuler allows us to instrument our code and figure out where our bottlenecks
            // are so we can speed up slow code.
            DebugSystem.Initialize(this, "Font");
            DebugSystem.Instance.FpsCounter.Visible = true;
            DebugSystem.Instance.TimeRuler.Visible = true;
            DebugSystem.Instance.TimeRuler.ShowLog = true;

            // Enable the Tap and FreeDrag gestures for our input on Windows Phone
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.FreeDrag;

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

            // Load the font for our UI
            font = Content.Load<SpriteFont>("Font");

            // Create a blank texture for our UI drawing
            blank = new Texture2D(GraphicsDevice, 1, 1);
            blank.SetData(new[] { Color.White });

            // Load the ground model
            ground = Content.Load<Model>("Ground");

            // Create our spheres
            CreateSpheres();
        }

        /// <summary>
        /// Helper method that creates all of our spheres.
        /// </summary>
        private void CreateSpheres()
        {
            // Create a random number generator
            Random random = new Random();

            // These are the various colors we use when creating the spheres
            Color[] sphereColors = new[]
            {
                Color.Red,
                Color.Blue,
                Color.Green,
                Color.Orange,
                Color.Pink,
                Color.Purple,
                Color.Yellow
            };

            // The radius of a sphere
            const float radius = 1f;

            for (int i = 0; i < maximumNumberOfSpheres; i++)
            {
                // Create the sphere
                Sphere sphere = new Sphere(GraphicsDevice, radius);

                // Position the sphere in our world
                sphere.Position = new Vector3(
                    RandomFloat(random, -worldSize + radius, worldSize - radius),
                    RandomFloat(random, radius, worldSize - radius),
                    RandomFloat(random, -worldSize + radius, worldSize - radius));

                // Pick a random color for the sphere
                sphere.Color = sphereColors[random.Next(sphereColors.Length)];

                // Create a random velocity vector
                sphere.Velocity = new Vector3(
                    RandomFloat(random, -10f, 10f),
                    RandomFloat(random, -10f, 10f),
                    RandomFloat(random, -10f, 10f));

                // Add the sphere to our array
                spheres[i] = sphere;
            }
        }

        /// <summary>
        /// A helper method that generates a random float in a given range.
        /// </summary>
        private static float RandomFloat(Random random, float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // We must call StartFrame at the top of Update to indicate to the TimeRuler
            // that a new frame has started.
            DebugSystem.Instance.TimeRuler.StartFrame();

            // We can now begin measuring our Update method
            DebugSystem.Instance.TimeRuler.BeginMark("Update", Color.Blue);

            // Update the input state for the game
            UpdateInput(gameTime);

            // Update all of the spheres in the game, handling collision if desired
            UpdateSpheres(gameTime);

            base.Update(gameTime);

            // End measuring the Update method
            DebugSystem.Instance.TimeRuler.EndMark("Update");
        }

        /// <summary>
        /// Helper method to handle the input for the sample.
        /// </summary>
        private void UpdateInput(GameTime gameTime)
        {
            // Update the game pad and keyboard input states
            gamePadPrev = gamePad;
            gamePad = GamePad.GetState(PlayerIndex.One);
            keyboardPrev = keyboard;
            keyboard = Keyboard.GetState();

            // If we've pressed the Back button or Escape key, exit the game
            if (gamePad.IsButtonDown(Buttons.Back) || keyboard.IsKeyDown(Keys.Escape))
                Exit();

            // If the X key or button was pressed, toggle whether or not we're colliding the spheres
            if ((gamePad.IsButtonDown(Buttons.X) && gamePadPrev.IsButtonUp(Buttons.X)) ||
                (keyboard.IsKeyDown(Keys.X) && keyboardPrev.IsKeyUp(Keys.X)))
            {
                collideSpheres = !collideSpheres;
            }

            // If the user pressed Up or Down on the keyboard, DPad, or left thumbstick, we adjust
            // the number of active spheres in our scene
            if (gamePad.IsButtonDown(Buttons.DPadUp) ||
                gamePad.IsButtonDown(Buttons.LeftThumbstickUp) ||
                keyboard.IsKeyDown(Keys.Up))
            {
                activeSphereCount++;
            }
            else if (gamePad.IsButtonDown(Buttons.DPadDown) ||
                gamePad.IsButtonDown(Buttons.LeftThumbstickDown) ||
                keyboard.IsKeyDown(Keys.Down))
            {
                activeSphereCount--;
            }

            // Poll for all the gestures our game received
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                // If the user tapped the screen, toggle whether or not we're colliding the spheres
                if (gesture.GestureType == GestureType.Tap)
                {
                    collideSpheres = !collideSpheres;
                }

                // If the user dragged on the screen, we adjust the number of active spheres in the scene
                else if (gesture.GestureType == GestureType.FreeDrag)
                {
                    activeSphereCount -= Math.Sign(gesture.Delta.Y);
                }
            }

            // Make sure we clamp our active sphere count so that we don't go above our maximum or below 1
            activeSphereCount = Math.Max(Math.Min(activeSphereCount, maximumNumberOfSpheres), 1);
        }

        /// <summary>
        /// Helper method that updates our sphere simulation.
        /// </summary>
        private void UpdateSpheres(GameTime gameTime)
        {
            // Update all spheres and perform collision against the world
            for (int i = 0; i < activeSphereCount; i++)
            {
                Sphere s = spheres[i];
                s.Update(gameTime);
                BounceSphereInWorld(s);
            }

            // If we are colliding spheres against each other
            if (collideSpheres)
            {
                // Iterate over the list twice to compare the spheres against each other
                for (int i = 0; i < activeSphereCount; i++)
                {
                    for (int j = 0; j < activeSphereCount; j++)
                    {
                        // Make sure we don't collid a sphere with itself
                        if (i == j)
                            continue;

                        // Get the spheres
                        Sphere a = spheres[i];
                        Sphere b = spheres[j];

                        // If the spheres are intersecting
                        if (a.Bounds.Intersects(b.Bounds))
                        {
                            // Get the vector between their centers
                            Vector3 delta = b.Position - a.Position;

                            // Calculate the point halfway between the spheres
                            Vector3 center = a.Position + delta / 2f;

                            // Normalize the delta vector
                            delta.Normalize();

                            // Move the spheres to resolve the collision
                            a.Position = center - delta * a.Radius;
                            b.Position = center + delta * b.Radius;

                            // Reflect the velocities to bounce the spheres
                            a.Velocity = Vector3.Normalize(Vector3.Reflect(a.Velocity, delta)) * b.Velocity.Length();
                            b.Velocity = Vector3.Normalize(Vector3.Reflect(b.Velocity, delta)) * a.Velocity.Length();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper method that keeps a sphere in the world by bouncing it off the walls.
        /// </summary>
        private static void BounceSphereInWorld(Sphere s)
        {
            // First test along the X axis, flipping the velocity if a collision occurs.
            if (s.Position.X < -worldSize + s.Radius)
            {
                s.Position.X = -worldSize + s.Radius;
                if (s.Velocity.X < 0f)
                    s.Velocity.X *= -1f;
            }
            else if (s.Position.X > worldSize - s.Radius)
            {
                s.Position.X = worldSize - s.Radius;
                if (s.Velocity.X > 0f)
                    s.Velocity.X *= -1f;
            }

            // Then we test the Y axis
            if (s.Position.Y < s.Radius)
            {
                s.Position.Y = s.Radius;
                if (s.Velocity.Y < 0f)
                    s.Velocity.Y *= -1f;
            }
            else if (s.Position.Y > worldSize - s.Radius)
            {
                s.Position.Y = worldSize - s.Radius;
                if (s.Velocity.Y > 0f)
                    s.Velocity.Y *= -1f;
            }

            // And lastly the Z axis
            if (s.Position.Z < -worldSize + s.Radius)
            {
                s.Position.Z = -worldSize + s.Radius;
                if (s.Velocity.Z < 0f)
                    s.Velocity.Z *= -1f;
            }
            else if (s.Position.Z > worldSize - s.Radius)
            {
                s.Position.Z = worldSize - s.Radius;
                if (s.Velocity.Z > 0f)
                    s.Velocity.Z *= -1f;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Begin measuring our Draw method
            DebugSystem.Instance.TimeRuler.BeginMark("Draw", Color.Red);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Create a view and projection matrix for our camera
            Matrix view = Matrix.CreateLookAt(
                new Vector3(worldSize, worldSize, worldSize) * 1.5f, Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.01f, 100f);

            // Set our sampler state to allow the ground to have a repeated texture
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Draw the ground scaled to our world
            ground.Draw(Matrix.CreateScale(worldSize, 1f, worldSize), view, projection);

            // Draw all of our spheres
            for (int i = 0; i < activeSphereCount; i++)
            {
                spheres[i].Draw(view, projection);
            }

            // Draw the demo text on top of the scene
            DrawDemoText();

            base.Draw(gameTime);

            // End measuring our Draw method
            DebugSystem.Instance.TimeRuler.EndMark("Draw");
        }

        /// <summary>
        /// Helper that draws our demo text, including our current settings and the instructions.
        /// </summary>
        private void DrawDemoText()
        {
            // Create the text based on our current states
            string demoText = string.Format(
                "Sphere count: {0}\nCollisions Enabled: {1}\n\n{2}", 
                activeSphereCount, collideSpheres, instructions);

            // Measure our text and calculate the correct position to draw it
            Vector2 size = font.MeasureString(demoText);
            Vector2 pos = new Vector2(
                GraphicsDevice.Viewport.TitleSafeArea.Right - size.X,
                GraphicsDevice.Viewport.TitleSafeArea.Top);

            spriteBatch.Begin();

            // Draw a blank box as a background for our text
            spriteBatch.Draw(
                blank, 
                new Rectangle((int)pos.X - 5, (int)pos.Y, (int)size.X + 10, (int)size.Y + 5), 
                Color.Black * .5f);

            // Draw the text itself
            spriteBatch.DrawString(font, demoText, pos, Color.White);

            spriteBatch.End();
        }
    }
}
