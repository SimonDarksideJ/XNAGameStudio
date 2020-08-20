#region File Description
//-----------------------------------------------------------------------------
// CameraShakeGame.cs
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

namespace CameraShake
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CameraShakeGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        // Two models that make up our scene
        Model ground;
        Model tank;

        // Our camera for viewing the scene
        Camera camera = new Camera();

        // Input states for GamePad and Keyboard
        GamePadState gamePad, gamePadPrev;
        KeyboardState keyboard, keyboardPrev;

        public CameraShakeGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            graphics.IsFullScreen = true;
            TargetElapsedTime = TimeSpan.FromTicks(333333);
#endif

            // Add our VibrationManager to the game for handling controller vibration
            Components.Add(new VibrationManager(this));

            // We use Tap and DoubleTap on Windows Phone
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.DoubleTap;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");

            ground = Content.Load<Model>("Ground");
            tank = Content.Load<Model>("Tank");

            camera.Position = new Vector3(1000f);
            camera.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 10f, 10000f);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Update our input state
            gamePadPrev = gamePad;
            keyboardPrev = keyboard;
            gamePad = GamePad.GetState(PlayerIndex.One);
            keyboard = Keyboard.GetState();
            
            // Allows the game to exit
            if (gamePad.Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            bool shake = false;
            bool longShake = false;

            // Check for the A button/key for a short shake
            if ((gamePad.IsButtonDown(Buttons.A) && gamePadPrev.IsButtonUp(Buttons.A)) ||
                (keyboard.IsKeyDown(Keys.A) && keyboardPrev.IsKeyUp(Keys.A)))
            {
                shake = true;
            }

            // Check for the X button/key for a long shake
            if ((gamePad.IsButtonDown(Buttons.X) && gamePadPrev.IsButtonUp(Buttons.X)) ||
                (keyboard.IsKeyDown(Keys.X) && keyboardPrev.IsKeyUp(Keys.X)))
            {
                longShake = true;
            }

            // Read all gestures
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                // Taps generate a short shake
                if (gesture.GestureType == GestureType.Tap)
                {
                    shake = true;
                }

                // Double taps generate a long shake
                else if (gesture.GestureType == GestureType.DoubleTap)
                {
                    longShake = true;
                }
            }

            // If we're performing a long shake, call the Shake method with a 2 second length
            if (longShake)
            {
                camera.Shake(25f, 2f);
                VibrationManager.Vibrate(PlayerIndex.One, .5f, .5f, 2f);
            }

            // If we're performing a short shake, call the Shake method with a .4 second length
            else if (shake)
            {
                camera.Shake(25f, .4f);
                VibrationManager.Vibrate(PlayerIndex.One, .5f, .5f, .4f);
            }
            
            // Update our camera
            camera.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Set some render states for our 3D rendering
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Draw our ground and tank scene using our camera's View and Projection matrices
            ground.Draw(Matrix.CreateScale(.1f), camera.View, camera.Projection);
            tank.Draw(Matrix.Identity, camera.View, camera.Projection);
            
            // Draw our instruction text
            DrawInstructions();

            base.Draw(gameTime);
        }

        private void DrawInstructions()
        {
            // Our instructions are based on our platform
            string instructions = string.Empty;
#if WINDOWS_PHONE
            instructions = "Tap - Short shake\nDouble tap - Long shake";
#else
            instructions = "A - Short shake\nX - Long shake";
#endif

            // Position our text based on the title safe area
            Vector2 position = new Vector2(
                GraphicsDevice.Viewport.TitleSafeArea.X, 
                GraphicsDevice.Viewport.TitleSafeArea.Y);

            // Use SpriteBatch to draw our text
            spriteBatch.Begin();
            spriteBatch.DrawString(font, instructions, position + Vector2.One, Color.Black);
            spriteBatch.DrawString(font, instructions, position, Color.White);
            spriteBatch.End();
        }
    }
}
