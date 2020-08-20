#region File Description
//-----------------------------------------------------------------------------
// VertexLighting.cs
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

namespace VertexLightingSample
{
    /// <summary>
    /// The central class for the sample Game.
    /// </summary>
    public class VertexLighting : Microsoft.Xna.Framework.Game
    {
        #region Sample Fields
        private GraphicsDeviceManager graphics;
        private SampleArcBallCamera camera;
        private Model[] sampleMeshes;
        private SampleGrid grid;
        private int activeMesh;
        private bool enableAdvancedEffect = true;
        private GamePadState lastGpState;
        private KeyboardState lastKbState;
        #endregion

        /// <summary>
        /// Example 1.1: Effect objects used for this example
        /// </summary>
        #region Effect Fields
        private Effect noLightingEffect;
        private Effect vertexLightingEffect;
        private EffectParameter projectionParameter;
        private EffectParameter viewParameter;
        private EffectParameter worldParameter;
        private EffectParameter lightColorParameter;
        private EffectParameter lightDirectionParameter;
        private EffectParameter ambientColorParameter;
        #endregion

        /// <summary>
        /// Example 1.2: Data fields corresponding to the effect paramters
        /// </summary>
        #region Uniform Data Fields
        private Matrix world, view, projection;
        private Vector3 diffuseLightDirection;
        private Vector4 diffuseLightColor;
        private Vector4 ambientLightColor;
        #endregion

        #region Initialization and Cleanup
        public VertexLighting()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }


        /// <summary>
        /// Initialize the sample.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
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

            sampleMeshes = new Model[5];

            //load meshes
            sampleMeshes[0] = Content.Load<Model>("Cube");
            sampleMeshes[1] = Content.Load<Model>("SphereHighPoly");
            sampleMeshes[2] = Content.Load<Model>("SphereLowPoly");
            sampleMeshes[3] = Content.Load<Model>("Cylinder");
            sampleMeshes[4] = Content.Load<Model>("Cone");

            //Example 1.2
            //create the effect objects that correspond to the effect files
            //that have been imported via the Content Pipeline
            noLightingEffect = Content.Load<Effect>("FlatShaded");
            vertexLightingEffect = Content.Load<Effect>("VertexLighting");

            GetEffectParameters();

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
        }


        /// <summary>
        /// Example 1.3
        /// This function obtains EffectParameter objects from the Effect objects.
        /// The EffectParameters are handles to the values in the shaders and are
        /// effectively how your C# code and your shader code communicate.
        /// </summary>
        private void GetEffectParameters()
        {


            //These parameters are used by both vertexLightingEffect and 
            //noLightingEffect, so we must take care to look up the correct ones.
            if (enableAdvancedEffect)
            {
                worldParameter = vertexLightingEffect.Parameters["world"];
                viewParameter = vertexLightingEffect.Parameters["view"];
                projectionParameter = vertexLightingEffect.Parameters["projection"];
            }
            else
            {
                worldParameter = noLightingEffect.Parameters["world"];
                viewParameter = noLightingEffect.Parameters["view"];
                projectionParameter = noLightingEffect.Parameters["projection"];
            }


            //These effect parameters are only used by vertexLightingEffect
            //to indicate the lights' colors and direction
            lightColorParameter = vertexLightingEffect.Parameters["lightColor"];
            lightDirectionParameter = vertexLightingEffect.Parameters["lightDirection"];
            ambientColorParameter = vertexLightingEffect.Parameters["ambientColor"];
            
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

            //check for exit
            if ((gpState.Buttons.Back == ButtonState.Pressed) ||
                kbState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            //Handle inputs for the sample camera
            camera.HandleDefaultGamepadControls(
                gpState, gameTime);
            camera.HandleDefaultKeyboardControls(
                kbState, gameTime);

            //handle inputs specific to this sample
            HandleInput(gameTime, gpState, kbState);


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
                //toggle the advanced effect
                enableAdvancedEffect = !enableAdvancedEffect;
                GetEffectParameters();
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
        /// Example 1.4
        /// 
        /// The effect parameters set in this function
        /// are shared between all of the rendered elements in the scene.
        /// </summary>
        private void SetSharedEffectParameters()
        {
            projectionParameter.SetValue(projection);
            viewParameter.SetValue(view);
            worldParameter.SetValue(world);
        }


        /// <summary>
        /// Draw the current scene.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            //draw the reference grid so it's easier to get our bearings
            grid.Draw();

            //always set the shared effects parameters
            SetSharedEffectParameters();

            if (enableAdvancedEffect)
            {
                //Example 1.5
                //Since we're using the advanced effect, we'll be setting the effect
                //parameters for the lighting effect.
                ambientColorParameter.SetValue(ambientLightColor);
                lightColorParameter.SetValue(diffuseLightColor);
                lightDirectionParameter.SetValue(diffuseLightDirection);
            }

            //finally, draw the mesh itself
            DrawSampleMesh(sampleMeshes[activeMesh]);
            
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

            //our sample meshes only contain a single part, so we don't need to bother
            //looping over the ModelMesh and ModelMeshPart collections. If the meshes
            //were more complex, we would repeat all the following code for each part
            ModelMesh mesh = sampleMesh.Meshes[0];
            ModelMeshPart meshPart = mesh.MeshParts[0];

            //set the vertex source to the mesh's vertex buffer
            graphics.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);

            //set the current index buffer to the sample mesh's index buffer
            graphics.GraphicsDevice.Indices = meshPart.IndexBuffer;

            //figure out which effect we're using currently
            Effect effect;
            if (enableAdvancedEffect) effect = vertexLightingEffect;
            else effect = noLightingEffect;


            //at this point' we're ready to begin drawing

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
            using (VertexLighting game = new VertexLighting())
            {
                game.Run();
            }
        }
        #endregion
    }
}