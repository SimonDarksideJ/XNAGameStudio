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
#endregion

namespace BloomPostprocess
{
    /// <summary>
    /// Sample showing how to implement a bloom postprocess,
    /// adding a glowing effect over the top of an existing scene.
    /// </summary>
    public class BloomPostprocessGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        BloomComponent bloom;

        int bloomSettingsIndex = 0;

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        Texture2D background;
        Model model;

        KeyboardState lastKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();

        #endregion

        #region Initialization


        public BloomPostprocessGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);

            bloom = new BloomComponent(this);

            Components.Add(bloom);
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("hudFont");
            background = Content.Load<Texture2D>("sunset");
            model = Content.Load<Model>("tank");
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;
            Viewport viewport = device.Viewport;

            bloom.BeginDraw();

            device.Clear(Color.Black);

            // Draw the background image.
            spriteBatch.Begin(0, BlendState.Opaque);
            
            spriteBatch.Draw(background,
                             new Rectangle(0, 0, viewport.Width, viewport.Height),
                             Color.White);
            
            spriteBatch.End();

            // Draw the spinning model.
            device.DepthStencilState = DepthStencilState.Default;

            DrawModel(gameTime);

            // Draw other components (which includes the bloom).
            base.Draw(gameTime);

            // Display some text over the top. Note how we draw this after the bloom,
            // because we don't want the text to be affected by the postprocessing.
            DrawOverlayText();
        }


        /// <summary>
        /// Helper for drawing the spinning 3D model.
        /// </summary>
        void DrawModel(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            Viewport viewport = graphics.GraphicsDevice.Viewport;
            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

            // Create camera matrices.
            Matrix world = Matrix.CreateRotationY(time * 0.42f);
            
            Matrix view = Matrix.CreateLookAt(new Vector3(750, 100, 0),
                                              new Vector3(0, 300, 0),
                                              Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspectRatio,
                                                                    1, 10000);

            // Look up the bone transform matrices.
            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();

                    // Override the default specular color to make it nice and bright,
                    // so we'll get some decent glints that the bloom can key off.
                    effect.SpecularColor = Vector3.One;
                }

                mesh.Draw();
            }
        }


        /// <summary>
        /// Displays an overlay showing what the controls are,
        /// and which settings are currently selected.
        /// </summary>
        void DrawOverlayText()
        {
            string text = "A = settings (" + bloom.Settings.Name + ")\n" +
                          "B = toggle bloom (" + (bloom.Visible ? "on" : "off") + ")\n" +
                          "X = show buffer (" + bloom.ShowBuffer.ToString() + ")";

            spriteBatch.Begin();

            // Draw the string twice to create a drop shadow, first colored black
            // and offset one pixel to the bottom right, then again in white at the
            // intended position. This makes text easier to read over the background.
            spriteBatch.DrawString(spriteFont, text, new Vector2(65, 65), Color.Black);
            spriteBatch.DrawString(spriteFont, text, new Vector2(64, 64), Color.White);

            spriteBatch.End();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting or changing the bloom settings.
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

            // Switch to the next bloom settings preset?
            if ((currentGamePadState.Buttons.A == ButtonState.Pressed &&
                 lastGamePadState.Buttons.A != ButtonState.Pressed) ||
                (currentKeyboardState.IsKeyDown(Keys.A) &&
                 lastKeyboardState.IsKeyUp(Keys.A)))
            {
                bloomSettingsIndex = (bloomSettingsIndex + 1) %
                                     BloomSettings.PresetSettings.Length;
             
                bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
                bloom.Visible = true;
            }

            // Toggle bloom on or off?
            if ((currentGamePadState.Buttons.B == ButtonState.Pressed &&
                 lastGamePadState.Buttons.B != ButtonState.Pressed) ||
                (currentKeyboardState.IsKeyDown(Keys.B) &&
                 lastKeyboardState.IsKeyUp(Keys.B)))
            {
                bloom.Visible = !bloom.Visible;
            }

            // Cycle through the intermediate buffer debug display modes?
            if ((currentGamePadState.Buttons.X == ButtonState.Pressed &&
                 lastGamePadState.Buttons.X != ButtonState.Pressed) ||
                (currentKeyboardState.IsKeyDown(Keys.X) &&
                 lastKeyboardState.IsKeyUp(Keys.X)))
            {
                bloom.Visible = true;
                bloom.ShowBuffer++;

                if (bloom.ShowBuffer > BloomComponent.IntermediateBuffer.FinalResult)
                    bloom.ShowBuffer= 0;
            }
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
            using (BloomPostprocessGame game = new BloomPostprocessGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
