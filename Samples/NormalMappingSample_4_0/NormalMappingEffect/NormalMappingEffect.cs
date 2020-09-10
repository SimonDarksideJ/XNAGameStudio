#region File Description
//-----------------------------------------------------------------------------
// NormalMappingEffect.cs
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

namespace NormalMappingEffect
{
    /// <summary>
    /// Sample shows how to render a model using a custom effect.
    /// </summary>
    public class NormalMappingEffectGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        KeyboardState lastKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();

        Model model;

        // the next 4 fields are inputs to the normal mapping effect, and will be set
        // at load time.  change these to change the light properties to modify
        // the appearance of the model.
        Vector4 lightColor = new Vector4(1, 1, 1, 1);
        Vector4 ambientLightColor = new Vector4(.2f, .2f, .2f, 1);
        float shininess = .3f;
        float specularPower = 4.0f;
        
        // the sample arc ball camera values
        float cameraArc = 0;
        float cameraRotation = 45;
        float cameraDistance = 1500;

        // the light rotates around the origin using these 3 constants.  the light
        // position is set in the draw function.
        const float LightHeight = 600;
        const float LightRotationRadius = 800;
        const float LightRotationSpeed = .5f;
        bool rotateLight = true;
        float lightRotation;

        #endregion

        #region Initialization


        public NormalMappingEffectGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            model = Content.Load<Model>("lizard");
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["LightColor"].SetValue(lightColor);
                    effect.Parameters["AmbientLightColor"].SetValue
                        (ambientLightColor);

                    effect.Parameters["Shininess"].SetValue(shininess);
                    effect.Parameters["SpecularPower"].SetValue(specularPower);
                }
            }
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            UpdateCamera(gameTime);

            // Turn on the rotating light
            if ((currentGamePadState.Buttons.A == ButtonState.Pressed &&
                lastGamePadState.Buttons.A != ButtonState.Pressed) ||
                (currentKeyboardState.IsKeyUp(Keys.Space) &&
                lastKeyboardState.IsKeyDown(Keys.Space)))
            {
                rotateLight = !rotateLight;
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.CornflowerBlue);

            // Compute camera matrices.
            Matrix unrotatedView = Matrix.CreateLookAt(
                new Vector3(0, 0, -cameraDistance), Vector3.Zero, Vector3.Up);

            Matrix view = Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                          Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                          unrotatedView;
                          

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    device.Viewport.AspectRatio,
                                                                    1,
                                                                    10000);

            if (rotateLight)
            {
                lightRotation +=
                    (float)gameTime.ElapsedGameTime.TotalSeconds * LightRotationSpeed;
            }

            Matrix lightRotationMatrix = Matrix.CreateRotationY(lightRotation);
            Vector3 lightPosition = new Vector3(LightRotationRadius, LightHeight, 0);
            lightPosition = Vector3.Transform(lightPosition, lightRotationMatrix);

            // Draw the model.
            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    Matrix world = transforms[mesh.ParentBone.Index];

                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["LightPosition"].SetValue(lightPosition);
                }

                mesh.Draw();
            }

            base.Draw(gameTime);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game.
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
        }


        /// <summary>
        /// Handles camera input.
        /// </summary>
        private void UpdateCamera(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check for input to rotate the camera up and down around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentKeyboardState.IsKeyDown(Keys.W))
            {
                cameraArc += time * 0.1f;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentKeyboardState.IsKeyDown(Keys.S))
            {
                cameraArc -= time * 0.1f;
            }

            cameraArc += currentGamePadState.ThumbSticks.Right.Y * time * 0.25f;

            // Limit the arc movement.
            if (cameraArc > 90.0f)
                cameraArc = 90.0f;
            else if (cameraArc < -90.0f)
                cameraArc = -90.0f;

            // Check for input to rotate the camera around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentKeyboardState.IsKeyDown(Keys.D))
            {
                cameraRotation += time * 0.1f;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentKeyboardState.IsKeyDown(Keys.A))
            {
                cameraRotation -= time * 0.1f;
            }

            cameraRotation += currentGamePadState.ThumbSticks.Right.X * time * 0.25f;

            // Check for input to zoom camera in and out.
            if (currentKeyboardState.IsKeyDown(Keys.Z))
                cameraDistance += time * 0.5f;

            if (currentKeyboardState.IsKeyDown(Keys.X))
                cameraDistance -= time * 0.5f;

            cameraDistance += currentGamePadState.Triggers.Left * time;
            cameraDistance -= currentGamePadState.Triggers.Right * time;

            // Limit the camera distance.
            if (cameraDistance > 5000.0f)
                cameraDistance = 5000.0f;
            else if (cameraDistance < 350.0f)
                cameraDistance = 350.0f;

            if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.R))
            {
                cameraArc = 0;
                cameraRotation = 45;
                cameraDistance = 1500;
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
            using (NormalMappingEffectGame game = new NormalMappingEffectGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}