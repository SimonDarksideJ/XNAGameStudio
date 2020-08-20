#region File Description
//-----------------------------------------------------------------------------
// AvatarShadowGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AvatarShadows
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class AvatarShadowsGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        // The ground beneath the avatar and the effect we use to render it
        VertexBuffer groundVertices;
        GroundEffect groundEffect;

        // Our list of avatars
        List<Avatar> avatars = new List<Avatar>();
        
        // The rotation of the camera
        float cameraRotation = 0f;

        // The rotation of the light
        float lightRotation = MathHelper.PiOver4;
        
        // States used for our input
        GamePadState gamePad, gamePadPrev;

        // Our render target that will hold the avatar shadows
        RenderTarget2D shadowTarget;

        public AvatarShadowsGame()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720,
                PreferMultiSampling = true
            };
            Content.RootDirectory = "Content";

            // Add the GamerServicesComponent so we can use avatars
            Components.Add(new GamerServicesComponent(this));
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

            // Create our ground vertex buffer
            groundVertices = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            groundVertices.SetData(new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(-15f, 0f, -15f), new Vector2(0f, 0f)),
                new VertexPositionTexture(new Vector3(15f, 0f, -15f), new Vector2(8f, 0f)),
                new VertexPositionTexture(new Vector3(-15f, 0f, 15f), new Vector2(0f, 8f)),
                new VertexPositionTexture(new Vector3(15f, 0f, 15f), new Vector2(8f, 8f)),
            });

            // Load our ground effect and texture
            groundEffect = new GroundEffect(Content.Load<Effect>("GroundEffect"));
            groundEffect.Texture = Content.Load<Texture2D>("ground");

            // Create our avatars in a box centered at the origin
            int numRows = 4;
            float spacing = 1.35f;
            float origin = -(numRows - 1) / 2f * spacing;
            Random random = new Random();
            for (int x = 0; x < numRows; x++)
            {                
                for (int z = 0; z < numRows; z++)
                {
                    // Create a random rotation value for the avatar
                    float rotation = (float)random.NextDouble() * MathHelper.TwoPi;

                    // Position the avatar based on our loops
                    Vector3 position = new Vector3(origin + x * spacing, 0, origin + z * spacing);

                    // Create the avatar, set its world matrix, and add it to our list
                    Avatar avatar = new Avatar();
                    avatar.World = Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(position);
                    avatars.Add(avatar);
                }
            }

            // Create our shadow render target. We use the Alpha8 format because we only care about the alpha
            // channel of the avatar rendering. Our ground effect simply checks the alpha being greater than
            // zero to indicate an area that is shadowed. By using Alpha8, we cut our memory usage by 25% over
            // using SurfaceFormat.Color.
            int width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int height = GraphicsDevice.PresentationParameters.BackBufferHeight;
            shadowTarget = new RenderTarget2D(
                GraphicsDevice, width, height, false, SurfaceFormat.Alpha8, DepthFormat.None);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Update input
            gamePadPrev = gamePad;
            gamePad = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (gamePad.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Rotate the camera with the left thumbstick X axis
            cameraRotation += gamePad.ThumbSticks.Left.X * .03f;

            // Rotate the light with the right thumbstick X axis
            lightRotation += gamePad.ThumbSticks.Right.X * .03f;

            // Update the animations
            foreach (Avatar avatar in avatars)
                avatar.Animation.Update(gameTime.ElapsedGameTime, true);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Reset some states that may have changed when drawing shadows and our instruction text
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Calculate the view matrix from our camera rotation
            Vector3 cameraPos = new Vector3(
                (float)Math.Sin(cameraRotation), .5f, (float)Math.Cos(cameraRotation)) * 8f;
            Matrix view = Matrix.CreateLookAt(cameraPos, new Vector3(0f, 1f, 0f), Vector3.Up);

            // Create our projection matrix
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1f, 1000f);

            // Calculate the light direction from our light rotation
            Vector3 lightDirection = new Vector3(
                (float)Math.Cos(lightRotation), -1f, (float)Math.Sin(lightRotation));
            lightDirection.Normalize();

            // Draw our shadows to our render target first
            DrawAvatarShadows(view, projection, lightDirection);

            // Clear the screen
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the ground
            DrawGround(view, projection);

            // Draw all of our avatars in our scene
            foreach (Avatar avatar in avatars)
            {
                avatar.Renderer.LightDirection = lightDirection;
                avatar.Renderer.World = avatar.World;
                avatar.Renderer.View = view;
                avatar.Renderer.Projection = projection;
                avatar.Renderer.Draw(avatar.Animation);
            }

            // Draw instructions over the scene
            DrawInstructions();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Helper method that draws all avatars into our shadow render target.
        /// </summary>
        private void DrawAvatarShadows(Matrix view, Matrix projection, Vector3 lightDirection)
        {
            // First we draw any avatar shadows to our render target
            GraphicsDevice.SetRenderTarget(shadowTarget);
            GraphicsDevice.Clear(Color.Transparent);

            // We generate a shadow matrix with Matrix.CreateShadow. This matrix is used to flatten an object
            // down to a plane based on a light direction. The light direction required for CreateShadow is
            // the direction TO the light, not from, so we must reverse our vector. We also create our plane
            // slightly offset from the origin to keep our shadow from having Z-fighting issues with the ground.
            Matrix shadowMatrix = Matrix.CreateShadow(-lightDirection, new Plane(Vector3.Up, -.001f));

            // Draw all of our avatars to the shadow render target
            foreach (Avatar avatar in avatars)
            {
                avatar.Renderer.World = avatar.World * shadowMatrix;
                avatar.Renderer.View = view;
                avatar.Renderer.Projection = projection;
                avatar.Renderer.Draw(avatar.Animation);
            }

            // Unset our render target to draw to our back buffer
            GraphicsDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Helper method that draws our ground using our custom ground effect.
        /// </summary>
        private void DrawGround(Matrix view, Matrix projection)
        {
            // Set our matrices
            groundEffect.World = Matrix.Identity;
            groundEffect.View = view;
            groundEffect.Projection = projection;

            // Assign our shadow render target to the effect
            groundEffect.Shadow = shadowTarget;

            // Set our vertex buffer
            GraphicsDevice.SetVertexBuffer(groundVertices);

            // Apply the effect and draw the primitives
            groundEffect.BaseEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

        /// <summary>
        /// Helper that just draws the instruction text to the screen.
        /// </summary>
        private void DrawInstructions()
        {
            // Create our position from the TitleSafeArea
            Vector2 position = new Vector2(
                GraphicsDevice.Viewport.TitleSafeArea.X,
                GraphicsDevice.Viewport.TitleSafeArea.Y);

            // Draw our instructions with SpriteBatch
            string instructions = "Left thumbstick  - Rotate camera\nRight thumbstick - Rotate light";
            spriteBatch.Begin();
            spriteBatch.DrawString(font, instructions, position, Color.White);
            spriteBatch.End();
        }
    }
}
