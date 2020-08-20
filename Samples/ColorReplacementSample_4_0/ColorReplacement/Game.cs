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

namespace ColorReplacement
{
    /// <summary>
    /// Sample showing how to change the color of select areas on a model.
    /// </summary>
    public class ColorReplacementGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        Matrix view;
        Matrix projection;       

        Model model;

        /// <summary>
        /// Desired color parts of the model will have after color replacement
        /// </summary>
        Vector3 targetColor = Color.Green.ToVector3();

        /// <summary>
        /// Maximum rate at which selected channels of the
        /// target color are changed based on user input
        /// </summary>
        const float ColorChangeRate = 0.01f;

        #endregion

        #region Initialization

        public ColorReplacementGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Create view matrix
            view = Matrix.CreateLookAt(
                new Vector3(0, 2.75f, 5), new Vector3(0, 0.25f, 0), Vector3.Up);
        }


        protected override void Initialize()
        {
            base.Initialize();

            // Create projection matrix
            projection = Matrix.CreatePerspectiveFieldOfView(1, GraphicsDevice.Viewport.AspectRatio, 1.0f, 10000.0f);
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("SpriteFont");
            model = Content.Load<Model>("Car");
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
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw a spinning model
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Matrix world = Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds * 0.2f);

            DrawModel(world);

            DrawOverlayText();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws a model that uses BasicEffect or ReplaceColor.fx on its parts
        /// </summary>
        private void DrawModel(Matrix world)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                Matrix boneWorld = transforms[mesh.ParentBone.Index] * world;

                foreach (Effect effect in mesh.Effects)
                {
                    // The car model has been modified to reference ReplaceColor.fx
                    // The parameters need to be set differently for that effect
                    BasicEffect basicEffect = effect as BasicEffect;
                    if (basicEffect != null)
                    {
                        // Set parameters on a BasicEffect
                        basicEffect.EnableDefaultLighting();
                        basicEffect.World = boneWorld;
                        basicEffect.View = view;
                        basicEffect.Projection = projection;
                    }
                    else
                    {
                        // Set parameters on a color replacement effect
                        effect.Parameters["WorldViewProjection"].SetValue(
                            boneWorld * view * projection);
                        effect.Parameters["World"].SetValue(boneWorld);
                        effect.Parameters["TargetColor"].SetValue(targetColor);
                    }
                }
                mesh.Draw();
            }
        }

        /// <summary>
        /// Draws the text overlay including instructions and
        /// the current target color
        /// </summary>
        private void DrawOverlayText()
        {
            spriteBatch.Begin();

            spriteBatch.DrawString(spriteFont,
                "Hold R/G/B key or X/A/B button and press Up/Down to change color",
                new Vector2(50, 30), Color.Black);

            spriteBatch.DrawString(spriteFont,
                "Red (R key, B button): " + targetColor.X.ToString("0.000"),
                new Vector2(50, 50), Color.Red);
            spriteBatch.DrawString(spriteFont,
                "Green (G key, A button): " + targetColor.Y.ToString("0.000"),
                new Vector2(50, 70), Color.Lime);
            spriteBatch.DrawString(spriteFont,
                "Blue (B key, X button): " + targetColor.Z.ToString("0.000"),
                new Vector2(50, 90), Color.Blue);

            spriteBatch.End();
        }

        #endregion

        #region HandleInput

        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        void HandleInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (keyboardState.IsKeyDown(Keys.Escape) ||
                gamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            // Calculate how much to change the color with up/down
            float colorChange = gamePadState.ThumbSticks.Left.Y * ColorChangeRate;
            if (keyboardState.IsKeyDown(Keys.Up) ||
                gamePadState.DPad.Up == ButtonState.Pressed)
            {
                colorChange = ColorChangeRate;
            }
            else if (keyboardState.IsKeyDown(Keys.Down) ||
                gamePadState.DPad.Down == ButtonState.Pressed)
            {
                colorChange = -ColorChangeRate;
            }

            // Change red
            if (keyboardState.IsKeyDown(Keys.R) ||
                gamePadState.Buttons.B == ButtonState.Pressed)
            {
                targetColor.X = MathHelper.Clamp(targetColor.X + colorChange, 0, 1);
            }

            // Change green
            if (keyboardState.IsKeyDown(Keys.G) ||
                gamePadState.Buttons.A == ButtonState.Pressed)
            {
                targetColor.Y = MathHelper.Clamp(targetColor.Y + colorChange, 0, 1);
            }

            // Change blue
            if (keyboardState.IsKeyDown(Keys.B) ||
                gamePadState.Buttons.X == ButtonState.Pressed)
            {
                targetColor.Z = MathHelper.Clamp(targetColor.Z + colorChange, 0, 1);
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
            using (ColorReplacementGame game = new ColorReplacementGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
