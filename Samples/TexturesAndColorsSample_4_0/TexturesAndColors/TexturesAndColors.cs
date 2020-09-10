#region File Description
//-----------------------------------------------------------------------------
// TexturesAndColors.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace TexturesAndColorsSample
{
    /// <summary>
    /// The central class for the sample Game.
    /// </summary>
    public class TexturesAndColors : Microsoft.Xna.Framework.Game
    {
        #region Sample Fields
        private GraphicsDeviceManager graphics;
        private Vector2 safeBounds;
        private SpriteBatch spriteBatch;
        private SpriteFont debugTextFont;
        private SampleArcBallCamera camera;
        private Model[] sampleMeshes;
        private Texture2D modelTexture;
        private SampleGrid grid;
        private int activeMesh;
        private int activeTechnique;
        private GamePadState lastGamePadState;
        private KeyboardState lastKeyboardState;
        #endregion

        #region Effect Fields
        private Effect effect;
        private EffectParameter projectionParameter;
        private EffectParameter viewParameter;
        private EffectParameter worldParameter;
        private EffectParameter lightColorParameter;
        private EffectParameter lightDirectionParameter;
        private EffectParameter ambientColorParameter;
        private EffectParameter modelTextureParameter;
        #endregion

        #region Uniform Data Fields
        private Matrix world, view, projection;
        private Vector3 diffuseLightDirection;
        private Vector4 diffuseLightColor;
        private Vector4 ambientLightColor;
        #endregion

        #region Initialization and Cleanup
        public TexturesAndColors()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }


        /// <summary>
        /// Load the graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            //Set up the reference grid and sample camera
            grid = new SampleGrid();
            grid.GridColor = Color.LimeGreen;
            grid.GridScale = 1.0f;
            grid.GridSize = 32;
            grid.LoadGraphicsContent(graphics.GraphicsDevice);


            camera = new SampleArcBallCamera(
                            SampleArcBallCameraMode.RollConstrained);
            camera.Distance = 3;
            //orbit the camera so we're looking down the z=-1 axis
            //the acr-ball camera is traditionally oriented to look
            //at the "front" of an object
            camera.OrbitRight(MathHelper.Pi);
            //orbit up a bit for perspective
            camera.OrbitUp(.2f);

            //load meshes
            sampleMeshes = new Model[5];
            sampleMeshes[0] = Content.Load<Model>("Cube");
            sampleMeshes[1] = Content.Load<Model>("SphereHighPoly");
            sampleMeshes[2] = Content.Load<Model>("SphereLowPoly");
            sampleMeshes[3] = Content.Load<Model>("Cylinder");
            sampleMeshes[4] = Content.Load<Model>("Cone");

            //load texture
            modelTexture = Content.Load<Texture2D>("Clouds");

            //load the effect
            effect = Content.Load<Effect>("TexturesAndColors");

            // The parameters are no longer shared, as they were in the previous 
            // Shader Series sample.  There is only one effect, that uses multiple
            // techniques, so there is only one instance of the parameters.
            worldParameter = effect.Parameters["world"];
            viewParameter = effect.Parameters["view"];
            projectionParameter = effect.Parameters["projection"];
            lightColorParameter = effect.Parameters["lightColor"];
            lightDirectionParameter = effect.Parameters["lightDirection"];
            ambientColorParameter = effect.Parameters["ambientColor"];
            modelTextureParameter = effect.Parameters["modelTexture"];

            //create the spritebatch for debug text
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            //load the sprite font for debug text
            debugTextFont = Content.Load<SpriteFont>("DebugText");



            //Calculate the projection properties first on any 
            //load callback.  That way if the window gets resized,
            //the perspective matrix is updated accordingly
            float aspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
                (float)graphics.GraphicsDevice.Viewport.Height;
            float fov = MathHelper.PiOver4 * aspectRatio * 3 / 4;
            projection = Matrix.CreatePerspectiveFieldOfView(fov,
                aspectRatio, .1f, 1000f);

            //create a default world matrix
            world = Matrix.Identity;


            //grid requires a projection matrix to draw correctly
            grid.ProjectionMatrix = projection;

            //Set the grid to draw on the x/z plane around the origin
            grid.WorldMatrix = Matrix.Identity;

            // calculate the safe left and top edges of the screen
            safeBounds = new Vector2(
                (float)graphics.GraphicsDevice.Viewport.X +
                (float)graphics.GraphicsDevice.Viewport.Width * 0.1f,
                (float)graphics.GraphicsDevice.Viewport.Y +
                (float)graphics.GraphicsDevice.Viewport.Height * 0.1f
                );

        }
        #endregion

        #region Update and Render
        /// <summary>
        /// Update the game world.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();

            //check for exit
            if ((gamePadState.Buttons.Back == ButtonState.Pressed) ||
                keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            //Handle inputs for the sample camera
            camera.HandleDefaultGamepadControls(gamePadState, gameTime);
            camera.HandleDefaultKeyboardControls(keyboardState, gameTime);

            //handle inputs specific to this sample
            HandleInput(gameTime, gamePadState, keyboardState);


            //Set the light direction to a fixed value.
            //This will place the light source behind, to the right, and above the user.
            diffuseLightDirection = new Vector3(-1, -1, -1);

            //ensure the light direction is normalized, or
            //the shader will give some weird results
            diffuseLightDirection.Normalize();

            //set the color of the diffuse light
            diffuseLightColor = Color.CornflowerBlue.ToVector4();
            
            //set the ambient lighting color
            ambientLightColor = Color.DarkSlateGray.ToVector4();
            
            //The built-in camera class provides the view matrix
            view = camera.ViewMatrix;
            
            //additionally, the reference grid included in the sample
            //requires a view matrix to draw correctly
            grid.ViewMatrix = camera.ViewMatrix;


            lastGamePadState = gamePadState;
            lastKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        private void HandleInput(GameTime gameTime, GamePadState gamePadState,
            KeyboardState keyboardState)
        {
            float elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            //Handle input for selecting meshes
            if (((gamePadState.Buttons.X == ButtonState.Pressed) &&
                (lastGamePadState.Buttons.X == ButtonState.Released)) ||
                (keyboardState.IsKeyDown(Keys.Tab) && 
                 lastKeyboardState.IsKeyUp(Keys.Tab)))
            {
                //switch the active mesh
                activeMesh = (activeMesh + 1) % sampleMeshes.Length;
            }


            //Handle input for selecting the active technique
            if (((gamePadState.Buttons.Y == ButtonState.Pressed) &&
                (lastGamePadState.Buttons.Y == ButtonState.Released)) ||
                (keyboardState.IsKeyDown(Keys.Space) && 
                 lastKeyboardState.IsKeyUp(Keys.Space)))
            {
                activeTechnique = (activeTechnique + 1) % effect.Techniques.Count;
            }


            //handle mesh rotation inputs
            float dx =
                SampleArcBallCamera.ReadKeyboardAxis(keyboardState, Keys.Left, 
                Keys.Right) + gamePadState.ThumbSticks.Left.X;
            float dy =
                SampleArcBallCamera.ReadKeyboardAxis(keyboardState, Keys.Down, 
                Keys.Up) + gamePadState.ThumbSticks.Left.Y;

            //apply mesh rotation to world matrix
            if (dx != 0)
            {
                world = world * Matrix.CreateFromAxisAngle(camera.Up,
                    elapsedTime * dx);
            }
            if (dy != 0)
            {
                world = world * Matrix.CreateFromAxisAngle(camera.Right,
                    elapsedTime * -dy);
            }
        }

        /// <summary>
        /// Draw the current scene.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            //the SpriteBatch added below to draw the current technique name
            //is changing some needed render states, so they are reset here.
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //draw the reference grid so it's easier to get our bearings
            grid.Draw();

            //always set the shared effects parameters
            projectionParameter.SetValue(projection);
            viewParameter.SetValue(view);
            worldParameter.SetValue(world);
            ambientColorParameter.SetValue(ambientLightColor);
            lightColorParameter.SetValue(diffuseLightColor);
            lightDirectionParameter.SetValue(diffuseLightDirection);
            //While textures are matched with texture samplers internally,
            //the Effect system automatically handles that for you,
            //generalizing texture assignment as "just another
            //EffectParameter".
            modelTextureParameter.SetValue(modelTexture);

            //finally, draw the mesh itself
            DrawSampleMesh(sampleMeshes[activeMesh]);

            // draw the technique name
            spriteBatch.Begin();
            spriteBatch.DrawString(debugTextFont, effect.CurrentTechnique.Name,
                safeBounds, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws a sample mesh using a single effect with a single technique.
        /// This pattern is very common in simple effect usage.
        /// </summary>
        /// <param name="sampleMesh"></param>
        public void DrawSampleMesh(Model sampleMesh)
        {
            if (sampleMesh == null)
                return;

            //our sample meshes only contain a single part, so we don't need to bother
            //looping over the ModelMesh and ModelMeshPart collections. If the meshes
            //were more complex, we would repeat all the following code for each part
            ModelMesh mesh = sampleMesh.Meshes[0];
            ModelMeshPart meshPart = mesh.MeshParts[0];

            //set the vertex source to the mesh's vertex buffer
            graphics.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);

            //set the current index buffer to the sample mesh's index buffer
            graphics.GraphicsDevice.Indices = meshPart.IndexBuffer;

            // set the current technique based on the user selection
            effect.CurrentTechnique = effect.Techniques[activeTechnique];

            //now we loop through the passes in the teqnique, drawing each
            //one in order
            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
            {
                //EffectPass.Apply will update the device to
                //begin using the state information defined in the current pass
                effect.CurrentTechnique.Passes[i].Apply();

                //sampleMesh contains all of the information required to draw
                //the current mesh
                graphics.GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, 0, 0,
                    meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
            }
        }
        #endregion

        #region Entry Point
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            using (TexturesAndColors game = new TexturesAndColors())
            {
                game.Run();
            }
        }
        #endregion
    }
}