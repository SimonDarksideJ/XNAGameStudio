#region File Description
//-----------------------------------------------------------------------------
// ShatterEffectGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace ShatterSample
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ShatterEffectGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        
        Vector3 lightPosition = Vector3.UnitY;
        Vector4 ambientColor = Color.DarkGray.ToVector4();
        Vector4 diffuseColor = Color.White.ToVector4();
        Vector4 specularColor = Color.White.ToVector4();        
        float specularPower = 50;        

        float time;        

        const float translationRate = 50;
        const float rotationRate = MathHelper.Pi * 3;
        const float duration = 2.0f;

        Model model;
        SpriteFont font;
        SpriteBatch spriteBatch;

        Matrix view;
        Matrix projection;
        Vector3 cameraPosition = new Vector3(-696,429, 835);
        Vector3 targetPosition = new Vector3(0, 60, 0); 

        public ShatterEffectGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            model = Content.Load<Model>("tank");
            font = Content.Load<SpriteFont>("font");
            spriteBatch = new SpriteBatch(this.graphics.GraphicsDevice);

            // Calculate View/Projection Matrices.
            view = Matrix.CreateLookAt(cameraPosition, targetPosition, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f), GraphicsDevice.Viewport.AspectRatio, 1.0f, 10000.0f);
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {               
            // Handle input
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            // Allows the default game to exit on Xbox 360 and Windows
            if (gamePadState.Buttons.Back == ButtonState.Pressed 
                || keyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            // Pressing the Up arrow or the A button on controller will Shatter the 
            // model
            if (keyboardState.IsKeyDown(Keys.Up) ||
                gamePadState.Buttons.A == ButtonState.Pressed)
            {
                time = Math.Min(duration, time + elapsedTime);
            }
            

            // Pressing the Down arrow or the B button on controller will reverse the 
            // Shatter effect 
            if (keyboardState.IsKeyDown(Keys.Down) || 
                gamePadState.Buttons.B == ButtonState.Pressed)
            {
                time = Math.Max(0.0f, time - elapsedTime);
            }

           
            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {                
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    SetupEffect(transforms, mesh, part);
                }
                mesh.Draw();
            }

            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Draw instructions
            spriteBatch.Begin(); 
            spriteBatch.DrawString(font, 
                @"Shatter Model: Hold Arrow Up or A button."+
                "\nReverse Shatter: Hold Arrow Down or B button.", new Vector2(50, 380), 
                 Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
        
        // Set the required values in the shader.
        private void SetupEffect(Matrix[] transforms, ModelMesh mesh, 
                                ModelMeshPart part)
        {
            Effect effect = part.Effect;
            effect.Parameters["TranslationAmount"].SetValue(translationRate * time);
            effect.Parameters["RotationAmount"].SetValue(rotationRate * time);
            effect.Parameters["time"].SetValue(time);            
            effect.Parameters["WorldViewProjection"].SetValue(
                transforms[mesh.ParentBone.Index] * view * projection);
            effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index]);
            effect.Parameters["eyePosition"].SetValue(cameraPosition);
            effect.Parameters["lightPosition"].SetValue(lightPosition);
            effect.Parameters["ambientColor"].SetValue(ambientColor);
            effect.Parameters["diffuseColor"].SetValue(diffuseColor);
            effect.Parameters["specularColor"].SetValue(specularColor);            
            effect.Parameters["specularPower"].SetValue(specularPower);            
        }
    }

    #region Entry Point
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            using (ShatterEffectGame game = new ShatterEffectGame())
            {
                game.Run();
            }
        }
    }
    #endregion
}