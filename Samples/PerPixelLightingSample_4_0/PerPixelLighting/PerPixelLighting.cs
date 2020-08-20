#region File Description
//-----------------------------------------------------------------------------
// PerPixelLighting.cs
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

namespace PerPixelLightingSample
{
    /// <summary>
    /// The central class for the sample Game.
    /// </summary>
    public class PerPixelLighting : Microsoft.Xna.Framework.Game
    {
        #region Sample Fields
        private GraphicsDeviceManager graphics;
        private SampleArcBallCamera camera;
        private Vector2 safeBounds;
        private Vector2 debugTextHeight;
        private Model[] sampleMeshes;
        private SampleGrid grid;
        private int activeMesh, activeEffect, activeTechnique, activeCombination;
        private int[,] effectTechniqueCombinations =
            {
                {0, 0}, {1, 0}, {0, 1}, {1, 1}, {1, 2}
            };
        private int effectTechniqueCombinationCount = 5;
        private SpriteBatch spriteBatch;
        private SpriteFont debugTextFont;
        private GamePadState lastGpState;
        private KeyboardState lastKbState;
        #endregion

        #region Specular Constant Fields
        private const float specularPowerMinimum = 0.5f;
        private const float specularPowerMaximum = 128f;
        private const float specularIntensityMinimum = 0.01f;
        private const float specularIntensityMaximum = 10f;
        #endregion

        /// <summary>
        /// Example 1.1: Effect objects used for this example
        /// </summary>
        #region Effect Fields
        private Effect[] effects;
        
        private EffectParameter[] worldParameter = new EffectParameter[2];
        private EffectParameter[] viewParameter = new EffectParameter[2];
        private EffectParameter[] projectionParameter = new EffectParameter[2];

        private EffectParameter[] cameraPositionParameter = new EffectParameter[2];
        private EffectParameter[] specularPowerParameter = new EffectParameter[2];
        private EffectParameter[] specularIntensityParameter = new EffectParameter[2];
        #endregion

        /// <summary>
        /// Example 1.2: Data fields corresponding to the effect paramters
        /// </summary>
        #region Uniform Data Fields
        private Matrix world;
        private float specularPower, specularIntensity;
        #endregion

        #region Initialization and Cleanup
        public PerPixelLighting()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }


        /// <summary>
        /// Initialize the sample.
        /// </summary>
        protected override void Initialize()
        {
            // create a default world and matrix
            world = Matrix.Identity;

            // create the mesh array
            sampleMeshes = new Model[5];

            // Set up the reference grid
            grid = new SampleGrid();
            grid.GridColor = Color.LimeGreen;
            grid.GridScale = 1.0f;
            grid.GridSize = 32;
            // Set the grid to draw on the x/z plane around the origin
            grid.WorldMatrix = Matrix.Identity;

            // set up the sample camera
            camera = new SampleArcBallCamera(SampleArcBallCameraMode.RollConstrained);
            camera.Distance = 3;
            // orbit the camera so we're looking down the z=-1 axis,
            // at the "front" of the object
            camera.OrbitRight(MathHelper.Pi);
            // orbit up a bit for perspective
            camera.OrbitUp(.2f);

            // set the initial effect, technique, and mesh
            activeMesh = 1;
            activeCombination = 0;
            activeEffect = effectTechniqueCombinations[activeCombination, 0];
            activeTechnique = effectTechniqueCombinations[activeCombination, 1];

            // set the initial specular values
            specularPower = 16;
            specularIntensity = 1;

            base.Initialize();
        }


        /// <summary>
        /// Load the graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Set up the reference grid and sample camera
            grid.LoadGraphicsContent(graphics.GraphicsDevice);

            // create the spritebatch for debug text
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            // load meshes
            sampleMeshes[0] = Content.Load<Model>("Cube");
            sampleMeshes[1] = Content.Load<Model>("SphereHighPoly");
            sampleMeshes[2] = Content.Load<Model>("SphereLowPoly");
            sampleMeshes[3] = Content.Load<Model>("Cylinder");
            sampleMeshes[4] = Content.Load<Model>("Cone");

            // load the sprite font for debug text
            debugTextFont = Content.Load<SpriteFont>("DebugText");
            debugTextHeight = new Vector2(0, debugTextFont.LineSpacing + 5);

            // load the effects
            effects = new Effect[2];
            effects[0] = Content.Load<Effect>("VertexLighting");
            effects[1] = Content.Load<Effect>("PerPixelLighting");

            for (int i = 0; i < 2; i++)
            {
                // cache the effect parameters
                worldParameter[i] = effects[i].Parameters["world"];
                viewParameter[i] = effects[i].Parameters["view"];
                projectionParameter[i] = effects[i].Parameters["projection"];
                cameraPositionParameter[i] = effects[i].Parameters["cameraPosition"];
                specularPowerParameter[i] = effects[i].Parameters["specularPower"];
                specularIntensityParameter[i] = effects[i].Parameters["specularIntensity"];
                cameraPositionParameter[i] = effects[i].Parameters["cameraPosition"];

                //
                // set up some basic effect parameters that do not change during the
                // course of execution
                //

                // set the light colors
                effects[i].Parameters["ambientLightColor"].SetValue(
                    Color.DarkSlateGray.ToVector4());
                effects[i].Parameters["diffuseLightColor"].SetValue(
                    Color.CornflowerBlue.ToVector4());
                effects[i].Parameters["specularLightColor"].SetValue(
                    Color.White.ToVector4());

                // Set the light position to a fixed location.
                // This will place the light source behind, to the right, and above the
                // initial camera position.
                effects[i].Parameters["lightPosition"].SetValue(
                    new Vector3(30f, 30f, 30f));
            }

            // Recalculate the projection properties on every LoadGraphicsContent call.
            // That way, if the window gets resized, then the perspective matrix will be
            // updated accordingly
            float aspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
                (float)graphics.GraphicsDevice.Viewport.Height;
            float fieldOfView = aspectRatio * MathHelper.PiOver4 * 3f / 4f;
            grid.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, .1f, 1000f);

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
            GamePadState gpState = GamePad.GetState(PlayerIndex.One);
            KeyboardState kbState = Keyboard.GetState();

            // Check for exit
            if ((gpState.Buttons.Back == ButtonState.Pressed) ||
                kbState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // Handle inputs for the sample camera
            camera.HandleDefaultGamepadControls(gpState, gameTime);
            camera.HandleDefaultKeyboardControls(kbState, gameTime);

            // Handle inputs specific to this sample
            HandleInput(gameTime, gpState, kbState);

            // The built-in camera class provides the view matrix
            grid.ViewMatrix = camera.ViewMatrix;

            // The camera position should also be updated for the
            // Phong specular component to be meaningful
            cameraPositionParameter[activeEffect].SetValue(camera.Position);

            // replace the "last" gamepad and keyboard states
            lastGpState = gpState;
            lastKbState = kbState;

            base.Update(gameTime);
        }

        private void HandleInput(GameTime gameTime, GamePadState gpState,
            KeyboardState kbState)
        {
            float elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            //Handle input for selecting meshes
            if (((gpState.Buttons.X == ButtonState.Pressed) &&
                (lastGpState.Buttons.X == ButtonState.Released)) ||
                (kbState.IsKeyDown(Keys.Tab) && lastKbState.IsKeyUp(Keys.Tab)))
            {
                //switch the active mesh
                activeMesh = (activeMesh + 1) % sampleMeshes.Length;
            }


            //Handle input for selecting the active effect
            if (((gpState.Buttons.Y == ButtonState.Pressed) &&
                (lastGpState.Buttons.Y == ButtonState.Released)) ||
                (kbState.IsKeyDown(Keys.Space) && lastKbState.IsKeyUp(Keys.Space)))
            {
                activeCombination = (activeCombination + 1) % 
                    effectTechniqueCombinationCount;
                activeEffect = effectTechniqueCombinations[activeCombination, 0];
                activeTechnique = effectTechniqueCombinations[activeCombination, 1];
            }


            //handle mesh rotation inputs
            float dx =
                SampleArcBallCamera.ReadKeyboardAxis(kbState, Keys.Left, Keys.Right) +
                gpState.ThumbSticks.Left.X;
            float dy =
                SampleArcBallCamera.ReadKeyboardAxis(kbState, Keys.Down, Keys.Up) +
                gpState.ThumbSticks.Left.Y;

            //apply mesh rotation to world matrix
            if (dx != 0)
            {
                world *= Matrix.CreateFromAxisAngle(camera.Up, elapsedTime * dx);
            }
            if (dy != 0)
            {
                world *= Matrix.CreateFromAxisAngle(camera.Right, elapsedTime * -dy);
            }


            //handle specular power and intensity inputs
            float dPower = SampleArcBallCamera.ReadKeyboardAxis(kbState, 
                Keys.Multiply, Keys.Divide);
            if (gpState.DPad.Right == ButtonState.Pressed)
            {
                dPower = 1;
            }
            if (gpState.DPad.Left == ButtonState.Pressed)
            {
                dPower = -1;
            }

            float dIntensity = SampleArcBallCamera.ReadKeyboardAxis(kbState, 
                Keys.Add, Keys.Subtract);
            if (gpState.DPad.Up == ButtonState.Pressed)
            {
                dIntensity = 1;
            }
            if (gpState.DPad.Down == ButtonState.Pressed)
            {
                dIntensity = -1;
            }

            if (dPower != 0)
            {
                specularPower *= 1 + (elapsedTime * dPower);
                specularPower = MathHelper.Clamp(specularPower, 
                    specularPowerMinimum, specularPowerMaximum);
            }

            if (dIntensity != 0)
            {
                specularIntensity *= 1 + (elapsedTime * dIntensity);
                specularIntensity = MathHelper.Clamp(specularIntensity, 
                    specularIntensityMinimum, specularIntensityMaximum);
            }

        }

        /// <summary>
        /// Example 1.4
        /// 
        /// The effect parameters set in this function
        /// are shared between all of the rendered elements in the scene.
        /// </summary>
        private void SetSharedEffectParameters()
        {
            worldParameter[activeEffect].SetValue(world);
            viewParameter[activeEffect].SetValue(grid.ViewMatrix);
            projectionParameter[activeEffect].SetValue(grid.ProjectionMatrix);
            specularPowerParameter[activeEffect].SetValue(specularPower);
            specularIntensityParameter[activeEffect].SetValue(specularIntensity);
        }


        /// <summary>
        /// Draw the current scene.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // the SpriteBatch added below to draw the debug text is changing some
            // needed render states, so they are reset here.
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // draw the reference grid so it's easier to get our bearings
            grid.Draw();

            // always set the shared effects parameters
            SetSharedEffectParameters();

            // draw the mesh itself
            DrawSampleMesh(sampleMeshes[activeMesh]);

            // draw the technique name and specular settings
            spriteBatch.Begin();
            spriteBatch.DrawString(debugTextFont, 
                effects[activeEffect].CurrentTechnique.Name,
                safeBounds, Color.White);
            spriteBatch.DrawString(debugTextFont, "Specular Power: " + 
                specularPower.ToString("0.00"),
                safeBounds + (1f * debugTextHeight), Color.White);
            spriteBatch.DrawString(debugTextFont, "Specular Intensity: " + 
                specularIntensity.ToString("0.00"),
                safeBounds + (2f * debugTextHeight), Color.White);
            spriteBatch.End();
            
            base.Draw(gameTime);
        }

        /// <summary>
        /// Example 1.6
        /// 
        /// Draws a sample mesh using a single effect with a single technique.
        /// This pattern is very common in simple effect usage.
        /// </summary>
        /// <param name="sampleMesh"></param>
        public void DrawSampleMesh(Model sampleMesh)
        {
            if (sampleMesh == null)
                return;

            // our sample meshes only contain a single part, so we don't need to bother
            // looping over the ModelMesh and ModelMeshPart collections. If the meshes
            // were more complex, we would repeat all the following code for each part
            ModelMesh mesh = sampleMesh.Meshes[0];
            ModelMeshPart meshPart = mesh.MeshParts[0];

            // set the vertex source to the mesh's vertex buffer
            graphics.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);

            // set the current index buffer to the sample mesh's index buffer
            graphics.GraphicsDevice.Indices = meshPart.IndexBuffer;

            // determine the current effect and technique
            effects[activeEffect].CurrentTechnique = 
                effects[activeEffect].Techniques[activeTechnique];

            // now we loop through the passes in the teqnique, drawing each
            // one in order
            int passCount = effects[activeEffect].CurrentTechnique.Passes.Count;
            for (int i = 0; i < passCount; i++)
            {
                // EffectPass.Apply will update the device to
                // begin using the state information defined in the current pass
                effects[activeEffect].CurrentTechnique.Passes[i].Apply();

                // sampleMesh contains all of the information required to draw
                // the current mesh
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
            using (PerPixelLighting game = new PerPixelLighting())
            {
                game.Run();
            }
        }
        #endregion
    }
}