#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
#endregion

namespace DistortionSample
{
    /// <summary>
    /// This sample demonstrates a variety of image-distorting 
    /// post-processing techniques.
    /// </summary>
    public class DistortionSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Fields
        GraphicsDeviceManager graphics;

        const float initialViewAngle = MathHelper.Pi / 2f;
        float viewAngle = initialViewAngle;
        const float CameraRotationSpeed = 0.1f;
        const float ViewDistance = 750.0f;

        DistortionComponent distortionComponent;
        Distorter[] distorters;
        int currentDistorter;

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        Vector2 overlayTextLocation;
        Texture2D background;

        KeyboardState lastKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        #endregion

        #region Initialization
        public DistortionSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            distortionComponent = new DistortionComponent(this);
            Components.Add(distortionComponent);
        }


        protected override void Initialize()
        {
            distorters = new Distorter[3];
            currentDistorter = 0;

            base.Initialize();
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("hudFont");
            background = Content.Load<Texture2D>("Sunset");

            distorters[0] = new Distorter();
            distorters[0].ModelName = "Dude";
            distorters[0].World = Matrix.CreateTranslation(0, -40, 0) *
                Matrix.CreateScale(8);
            distorters[0].Model = Content.Load<Model>("Dude");
            distorters[0].Technique =
                DistortionComponent.DistortionTechnique.PullIn;
            distorters[0].DistortionScale = 0.0003f;
            distorters[0].DistortionBlur = true;

            distorters[1] = new Distorter();
            distorters[1].ModelName = "Cylinder";
            distorters[1].World = Matrix.CreateScale(200);
            distorters[1].Model = Content.Load<Model>("Cylinder");
            distorters[1].Technique =
                DistortionComponent.DistortionTechnique.HeatHaze;
            distorters[1].DistortionScale = 0.025f;
            distorters[1].DistortionBlur = true;

            distorters[2] = new Distorter();
            distorters[2].ModelName = "Window";
            distorters[2].World = Matrix.CreateScale(500);
            distorters[2].Model = Content.Load<Model>("Window");
            distorters[2].Technique =
                DistortionComponent.DistortionTechnique.DisplacementMapped;
            distorters[2].DistortionScale = 0.025f;
            distorters[2].DistortionBlur = false;

            overlayTextLocation = new Vector2(
                (float)graphics.GraphicsDevice.Viewport.X +
                (float)graphics.GraphicsDevice.Viewport.Width * 0.1f,
                (float)graphics.GraphicsDevice.Viewport.Y +
                (float)graphics.GraphicsDevice.Viewport.Height * 0.1f
                );
        }


        #endregion

        #region Update and Draw
        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            // update the distortion component
            distortionComponent.Distorter = distorters[currentDistorter];

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            distortionComponent.BeginDraw();
            
            GraphicsDevice.Clear(Color.Black);

            // Draw the background image.
            spriteBatch.Begin(0, BlendState.Opaque);
            spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, Color.White);
            spriteBatch.End();

            // Draw other components (which includes the distortion).
            base.Draw(gameTime);

            // Display some text over the top. Note how we draw this after distortion
            // because we don't want the text to be affected by the postprocessing.
            DrawOverlayText();
        }


        /// <summary>
        /// Displays an overlay showing what the controls are,
        /// and which settings are currently selected.
        /// </summary>
        void DrawOverlayText()
        {
            string text = distorters[currentDistorter].ToString() + "\n\n" +
                "A: Cycle Distorter\n" +
                "X: " + (distorters[currentDistorter].DistortionBlur ? "Disable" : 
                    "Enable") + " Distorter Blur\n" +
                "B: " + (distortionComponent.ShowDistortionMap ? "Hide" : "Show") +
                " Distortion Map\n";

            spriteBatch.Begin();

            // Draw the string twice to create a drop shadow, first colored black
            // and offset one pixel to the bottom right, then again in white at the
            // intended position. this makes text easier to read over the background.
            spriteBatch.DrawString(spriteFont, text, overlayTextLocation + Vector2.One,
                Color.Black);
            spriteBatch.DrawString(spriteFont, text, overlayTextLocation, Color.White);

            spriteBatch.End();
        }
        #endregion

        #region Handle Input
        /// <summary>
        /// Handles input for quitting or changing the sample settings.
        /// </summary>
        private void HandleInput()
        {
            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            // Cycle mode
            if ((currentGamePadState.Buttons.A == ButtonState.Pressed &&
                 lastGamePadState.Buttons.A != ButtonState.Pressed) ||
                (currentKeyboardState.IsKeyDown(Keys.Space) &&
                 lastKeyboardState.IsKeyUp(Keys.Space)) ||
                (currentKeyboardState.IsKeyDown(Keys.A) &&
                 lastKeyboardState.IsKeyUp(Keys.A)))
            {
                currentDistorter = (currentDistorter + 1) % distorters.Length;
                viewAngle = initialViewAngle;
            }

            // Toggle showing the distortion map on or off?
            if ((currentGamePadState.Buttons.B == ButtonState.Pressed &&
                 lastGamePadState.Buttons.B != ButtonState.Pressed) ||
                (currentKeyboardState.IsKeyDown(Keys.Tab) &&
                 lastKeyboardState.IsKeyUp(Keys.Tab)) ||
                (currentKeyboardState.IsKeyDown(Keys.B) &&
                 lastKeyboardState.IsKeyUp(Keys.B)))
            {
                distortionComponent.ShowDistortionMap = 
                    !distortionComponent.ShowDistortionMap;
            }

            // Toggle showing the distortion map on or off?
            if ((currentGamePadState.Buttons.X == ButtonState.Pressed &&
                 lastGamePadState.Buttons.X != ButtonState.Pressed) ||
                (currentKeyboardState.IsKeyDown(Keys.LeftControl) &&
                 lastKeyboardState.IsKeyUp(Keys.LeftControl)) ||
                (currentKeyboardState.IsKeyDown(Keys.X) &&
                 lastKeyboardState.IsKeyUp(Keys.X)))
            {
                distorters[currentDistorter].DistortionBlur = 
                    !distorters[currentDistorter].DistortionBlur;
            }

            // rotate the camera, using the left thumbstick and arrow keys
            float viewAngleChange = currentGamePadState.ThumbSticks.Left.X;
            if (currentKeyboardState.IsKeyDown(Keys.Left))
            {
                viewAngleChange = -1;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Right))
            {
                viewAngleChange = 1;
            }
            viewAngle += viewAngleChange * CameraRotationSpeed;
            distortionComponent.View = Matrix.CreateLookAt(ViewDistance *
                new Vector3((float)Math.Cos(viewAngle), 0, (float)Math.Sin(viewAngle)),
                Vector3.Zero, Vector3.Up);
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
            using (DistortionSampleGame game = new DistortionSampleGame())
            {
                game.Run();
            }
        }
    }
    #endregion
}
