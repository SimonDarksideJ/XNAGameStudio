#region File Description
//-----------------------------------------------------------------------------
// DemoGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
#endregion

namespace Graphics3DSample
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Graphics3DSampleGame : Microsoft.Xna.Framework.Game
    {

        #region Fields
        #region Constants
        const int buttonHeight = 70;
        const int buttonWidth = 70;
        const int buttonMargin = 15;
        #endregion

        GraphicsDeviceManager graphics;

        Spaceship spaceship;

        Checkbox[] lightEnablingButtons;
        Checkbox perpixelLightingButton;
        Checkbox animationButton;

        Checkbox backgroundTextureEnablingButton;

        float cameraFOV = 45; // Initial camera FOV (serves as a zoom level)
        float rotationXAmount = 0.0f;
        float rotationYAmount = 0.0f;
        float? prevLength;

        Texture2D background;

        Animation animation;
        Vector2 animationPosition;

        #region Public accessors
        /// <summary>
        /// Provides SporiteBatch to components that draw sprites
        /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }
        #endregion
        #endregion

        #region Initialization
        /// <summary>
        /// Initialization that does not depend on GraphicsDevice
        /// </summary>
        public Graphics3DSampleGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = true;
            graphics.SupportedOrientations =
                DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Initialization that depends on GraphicsDevice but does not depend on Content
        /// </summary>
        protected override void Initialize()
        {
            GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

            SpriteBatch = new SpriteBatch(GraphicsDevice);

            CreateSpaceship();
            CreateLightEnablingButtons();
            CreateBackgroundTextureEnablingButton();
            CreatePerPixelLightingButton();
            CreateAnimationButton();

            //Initialize gestures support - Pinch for Zoom and horizontal drag for rotate
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.PinchComplete;

            base.Initialize();
        }

        /// <summary>
        /// Loads content and creates graphics resources.
        /// </summary>
        protected override void LoadContent()
        {
            background = Content.Load<Texture2D>("Textures/spaceBG");

            animation = CreateAnimation();
            spaceship.Load(this.Content);
            base.LoadContent();
        }

        /// <summary>
        /// Creates animation
        /// </summary>
        /// <returns></returns>
        private Animation CreateAnimation()
        {
            // Load multiple animations form XML definition
            System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load("Content/AnimationDef.xml");
            System.Xml.Linq.XName name = System.Xml.Linq.XName.Get("Definition");
            var definitions = doc.Document.Descendants(name);

            // Get the first (and only in this case) animation from the XML definition
            var definition = definitions.First();

            Texture2D texture = Content.Load<Texture2D>(definition.Attribute("SheetName").Value);

            Point frameSize = new Point();
            frameSize.X = int.Parse(definition.Attribute("FrameWidth").Value);
            frameSize.Y = int.Parse(definition.Attribute("FrameHeight").Value);

            Point sheetSize = new Point();
            sheetSize.X = int.Parse(definition.Attribute("SheetColumns").Value);
            sheetSize.Y = int.Parse(definition.Attribute("SheetRows").Value);

            TimeSpan frameInterval = TimeSpan.FromSeconds((float)1 / int.Parse(definition.Attribute("Speed").Value));

            //Calculate the animation position (in the middle fot he screen)
            animationPosition = new Vector2((graphics.PreferredBackBufferWidth / 2 - frameSize.X),
                                            (graphics.PreferredBackBufferHeight / 2 - frameSize.Y));

            return new Animation(texture, frameSize, sheetSize, frameInterval);
        }

        /// <summary>
        /// Creates spaceship
        /// </summary>
        private void CreateSpaceship()
        {
            spaceship = new Spaceship();
            spaceship.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(cameraFOV),
                GraphicsDevice.Viewport.AspectRatio, 10, 20000);
        }

        /// <summary>
        /// Creates light enabling buttons
        /// </summary>
        private void CreateLightEnablingButtons()
        {
            lightEnablingButtons = new Checkbox[3];
            for (int n = 0; n < lightEnablingButtons.Length; n++)
            {
                lightEnablingButtons[n] = new Checkbox(this, "Buttons/lamp_60x60",
                    new Rectangle(GraphicsDevice.Viewport.Width - (n + 1) * (buttonWidth + buttonMargin),
                        buttonMargin, buttonWidth, buttonHeight), true);
                this.Components.Add(lightEnablingButtons[n]);
            }
        }

        /// <summary>
        /// Creates per-pixel lighting button
        /// </summary>
        private void CreatePerPixelLightingButton()
        {
            perpixelLightingButton = new Checkbox(this, "Buttons/perPixelLight_60x60",
                new Rectangle(GraphicsDevice.Viewport.Width - (buttonWidth + buttonMargin),
                    GraphicsDevice.Viewport.Height - (buttonHeight + buttonMargin),
                    buttonWidth, buttonHeight), false);
            this.Components.Add(perpixelLightingButton);
        }

        /// <summary>
        /// Creates animation button
        /// </summary>
        private void CreateAnimationButton()
        {
            animationButton = new Checkbox(this, "Buttons/animation_60x60",
                new Rectangle(buttonMargin,
                    GraphicsDevice.Viewport.Height - (buttonHeight + buttonMargin),
                    buttonWidth, buttonHeight), false);
            this.Components.Add(animationButton);
        }

        /// <summary>
        /// Create texture enabling button
        /// </summary>
        private void CreateBackgroundTextureEnablingButton()
        {
            backgroundTextureEnablingButton = new Checkbox(this, "Buttons/textureOnOff",
                     new Rectangle(buttonMargin, buttonMargin, buttonWidth, buttonHeight), false);
            this.Components.Add(backgroundTextureEnablingButton);
        }
        #endregion

        #region Update
        /// <summary>
        /// Updates spaceship rendering properties
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Handle touch input first
            HandleInput();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            spaceship.Rotation = GetRotationMatrix();
            spaceship.View = GetViewMatrix();
            spaceship.Lights = lightEnablingButtons.Select(e => e.IsChecked).ToArray();
            spaceship.IsTextureEnabled = true;
            spaceship.IsPerPixelLightingEnabled = perpixelLightingButton.IsChecked;
            spaceship.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(cameraFOV),
                GraphicsDevice.Viewport.AspectRatio, 10, 20000);

            if (animationButton.IsChecked)
            {
                animation.Update(gameTime);
            }

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gestureSample = TouchPanel.ReadGesture();
                switch (gestureSample.GestureType)
                {
                    case GestureType.FreeDrag:
                        rotationXAmount += gestureSample.Delta.X;
                        rotationYAmount -= gestureSample.Delta.Y;
                        break;

                    case GestureType.Pinch:
                        float gestureValue = 0;
                        float minFOV = 60;
                        float maxFOV = 30;
                        float gestureLengthToZoomScale = 10;

                        Vector2 gestureDiff = gestureSample.Position - gestureSample.Position2;
                        gestureValue = gestureDiff.Length() / gestureLengthToZoomScale;

                        if (null != prevLength) // Skip the first pinch event
                            cameraFOV -= gestureValue - prevLength.Value;

                        cameraFOV = MathHelper.Clamp(cameraFOV, maxFOV, minFOV);

                        prevLength = gestureValue;
                        break;

                    case GestureType.PinchComplete:
                        prevLength = null;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Gets spaceship rotation matrix
        /// </summary>
        /// <returns></returns>
        private Matrix GetRotationMatrix()
        {
            Matrix matrix = Matrix.CreateWorld(new Vector3(0, 250, 0), Vector3.Forward, Vector3.Up) *
                Matrix.CreateFromYawPitchRoll((float)Math.PI + MathHelper.PiOver2 + rotationXAmount / 100, rotationYAmount / 100, 0);
            return matrix;
        }

        /// <summary>
        /// Gets spaceship view matrix
        /// </summary>
        private Matrix GetViewMatrix()
        {
            return Matrix.CreateLookAt(
                new Vector3(3500, 400, 0) + new Vector3(0, 250, 0),
                new Vector3(0, 250, 0),
                Vector3.Up);
        }
        #endregion

        #region Draw
        /// <summary>
        /// Draws the game
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (backgroundTextureEnablingButton.IsChecked)
            {
                SpriteBatch.Begin();
                SpriteBatch.Draw(background, Vector2.Zero, Color.White);
                SpriteBatch.End();
            }

            var lights = lightEnablingButtons.Select(e => e.IsChecked).ToArray();

            // Set render states.
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // This draws game components, including the currently active menu screen.
            // Draw the spaceship model
            spaceship.Draw();

            if (animationButton.IsChecked)
            {
                DrawAnimation();
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws animation
        /// </summary>
        private void DrawAnimation()
        {
            float screenHeight = graphics.PreferredBackBufferHeight;
            float scale = (float)(graphics.PreferredBackBufferWidth / 480.0);

            SpriteBatch.Begin();
            animation.Draw(SpriteBatch, animationPosition, 2.0f, SpriteEffects.None);
            SpriteBatch.End();
        }
        #endregion
    }
}
