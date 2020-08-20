#region File Description
//-----------------------------------------------------------------------------
// MaterialsAndLights.cs
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

namespace MaterialsAndLightsSample
{
    /// <summary>
    /// The central class for the sample Game.
    /// </summary>
    public class MaterialsAndLights : Microsoft.Xna.Framework.Game
    {
        #region Sample Fields
        private GraphicsDeviceManager graphics;
        private SampleArcBallCamera camera;
        private Vector2 safeBounds;
        private Vector2 debugTextHeight;
        private SpriteBatch spriteBatch;
        private SpriteFont debugTextFont;
        private GamePadState lastGpState;
        private KeyboardState lastKbState;
        private Effect pointLightMeshEffect;
        private string shaderVersionString = string.Empty;
        private Matrix projection;
        private Random random = new Random();
        #endregion

        #region Scene Fields
        private Matrix meshRotation;
        private Matrix[] meshWorlds;
        private Material[] materials;
        private Material floorMaterial;
        private Matrix floorWorld;
        private int materialRotation = 0;
        private PointLight[] lights;
        private Model[] sampleMeshes;
        private Model pointLightMesh;
        private int numLights;
        private int maxLights;
        private Matrix lightMeshWorld;
        #endregion

        #region Shared Effect Fields
        private Effect baseEffect;
        private EffectParameter viewParameter;
        private EffectParameter projectionParameter;
        private EffectParameter cameraPositionParameter;
        #endregion

        #region Initialization and Cleanup
        public MaterialsAndLights()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            // enable default sample behaviors.
            graphics.PreferredBackBufferWidth = 853;
            graphics.PreferredBackBufferHeight = 480;
            graphics.MinimumVertexShaderProfile = ShaderProfile.VS_2_0;
            graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;
        }


        /// <summary>
        /// Initialize the sample.
        /// </summary>
        protected override void Initialize()
        {
            // create a default world and matrix
            meshRotation = Matrix.Identity;

            // create the mesh array
            sampleMeshes = new Model[5];

            // set up the sample camera
            camera = new SampleArcBallCamera(SampleArcBallCameraMode.RollConstrained);
            camera.Distance = 12;
            // orbit the camera so we're looking down the z=-1 axis,
            // at the "front" of the object
            camera.OrbitRight(MathHelper.Pi);
            // orbit up a bit for perspective
            camera.OrbitUp(.2f);

            //set up a ring of meshes
            meshWorlds = new Matrix[8];
            for (int i = 0; i < 8; i++)
            {
                float theta = MathHelper.TwoPi * ((float) i / 8f);
                meshWorlds[i] = Matrix.CreateTranslation(
                    5f * (float)Math.Sin(theta), 0, 5f * (float)Math.Cos(theta));
            }

            // set the initial material assignments to the geometry
            materialRotation = 2;
            

            // stretch the cube out to represent a "floor"
            // that will help visualize the light radii
            floorWorld = Matrix.CreateScale(30f, 1f, 30f) * 
                Matrix.CreateTranslation(0, -2.2f, 0);
            lightMeshWorld = Matrix.CreateScale(.2f);

            base.Initialize();
        }


        /// <summary>
        /// Load the graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // create the spritebatch for debug text
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            // load meshes
            sampleMeshes[0] = Content.Load<Model>("Meshes\\Cube");
            sampleMeshes[1] = Content.Load<Model>("Meshes\\SphereHighPoly");
            sampleMeshes[2] = Content.Load<Model>("Meshes\\Cylinder");
            sampleMeshes[3] = Content.Load<Model>("Meshes\\Cone");

            pointLightMesh = Content.Load<Model>("Meshes\\SphereLowPoly");

            // load the sprite font for debug text
            debugTextFont = Content.Load<SpriteFont>("DebugText");
            debugTextHeight = new Vector2(0, debugTextFont.LineSpacing + 5);

            //load the effects
            pointLightMeshEffect = Content.Load<Effect>("Effects\\PointLightMesh");


            //////////////////////////////////////////////////////////////
            // Example 1.1                                              //
            //                                                          //
            // To light the materials with a large number of lights     //
            // in a single pass, this example requires Shader Model 3.0 //
            // level hardare.  The Xbox 360 will always use the SM3.0   //
            // version of the shader code, but pre-SM-3.0 PC harware    //
            // requires a fallback.  In this sample, the fallback is    //
            // limited to 2 lights in a single pass.                    //
            //////////////////////////////////////////////////////////////
            if (graphics.GraphicsDevice.GraphicsDeviceCapabilities.
                PixelShaderVersion.Major >= 3)
            {
                baseEffect = Content.Load<Effect>("Effects\\MaterialShader30");
                lights = new PointLight[8];

                maxLights = 8;
                numLights = 1;
                baseEffect.Parameters["numLights"].SetValue(numLights);
                shaderVersionString = "Using Shader Model 3.0";
            }
            else
            {
                baseEffect = Content.Load<Effect>("Effects\\MaterialShader20");
                lights = new PointLight[2];
                maxLights = 2;
                numLights = 1;
                baseEffect.Parameters["numLights"].SetValue(numLights);
                shaderVersionString = "Using Shader Model 2.0";
            }

            // generate random light paramters
            GenerateRandomLights();

            // cache the effect parameters
            viewParameter = baseEffect.Parameters["view"];
            projectionParameter = baseEffect.Parameters["projection"];
            cameraPositionParameter = baseEffect.Parameters["cameraPosition"];

            // create the materials
            materials = new Material[8];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = new Material(Content, graphics.GraphicsDevice,
                    baseEffect);
            }
            floorMaterial = new Material(Content, graphics.GraphicsDevice,
                baseEffect);


            //////////////////////////////////////////////////////////////
            // Example 1.2                                              //
            //                                                          //
            // Each material will have a unique set of properties.  For //
            // the purposes of this sample, the materials are assigned  //
            // at runtime.  Often, these kinds of material properties   //
            // are specific to the Model being used, and can be         //
            // imported through the Content Pipeline using custom       //
            // processors and/or importers.                             //
            //////////////////////////////////////////////////////////////
            materials[0].SetBasicProperties(Color.Purple, .5f, 1.2f);
            materials[1].SetBasicProperties(Color.Orange, 8f, 2f);
            materials[2].SetBasicProperties(Color.Green, 2f, 3f);
            materials[3].SetBasicProperties(Color.White, 256f, 16f);
            materials[4].SetTexturedMaterial(Color.CornflowerBlue, 64f, 4f, null,
                "Scratches", .5f, .5f);
            materials[5].SetTexturedMaterial(Color.LemonChiffon, 32f, 4f, "Marble",
                "Scratches", 1f, 1f);
            materials[6].SetTexturedMaterial(Color.White, 32f, 16f, "Wood", null,
                3f, 3f);
            materials[7].SetTexturedMaterial(Color.White, 64f, 64f, "Hexes",
                "HexesSpecular", 4f, 2f);

            floorMaterial.SetTexturedMaterial(Color.White, 16f, .8f, "Grid",
                "Grid", 15f, 15f);


            //////////////////////////////////////////////////////////////
            // Example 1.3                                              //
            //                                                          //
            // The ambient light color does not change at all during    //
            // runtime, and it is applied to all materials in the       //
            // scene.  All of the clone textures for each material will //
            // be updated since the clones use the same effect pool and //
            // the ambientLightColor parameter is maked "shared" in the //
            // effect.  By sharing this parameter amongst all materials //
            // the usage is both more readable and optimized.           //
            //////////////////////////////////////////////////////////////
            baseEffect.Parameters["ambientLightColor"].SetValue(
                new Vector4(.15f, .15f, .15f, 1.0f));

            // Recalculate the projection properties on every LoadGraphicsContent call.
            // That way, if the window gets resized, then the perspective matrix will
            // be updated accordingly
            float aspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
                (float)graphics.GraphicsDevice.Viewport.Height;
            float fieldOfView = aspectRatio * MathHelper.PiOver4 * 3f / 4f;
            projection = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, .1f, 1000f);
            projectionParameter.SetValue(projection);

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
        /// This function populates the lights list with semi-random
        /// light properties.
        /// </summary>
        public void GenerateRandomLights()
        {
            for (int i = 0; i < lights.Length; i++)
            {
                //generate a "circle" of lights
                float theta = MathHelper.TwoPi * ((float)(i / 2) / (float)lights.Length)
                    + (MathHelper.Pi * i);
                Vector4 position = new Vector4(
                    8f * (float)Math.Sin(theta), 3, 8f * (float)Math.Cos(theta), 1.0f);


                //////////////////////////////////////////////////////////////
                // Example 1.4                                              //
                //                                                          //
                // The lights paramter is an arrray of light structures.    //
                // Each light structure coresponds to an individual light,  //
                // so an element in the array is associated with the new    //
                // PointLight instance.                                     //
                //////////////////////////////////////////////////////////////
                lights[i] = new PointLight(
                    position, baseEffect.Parameters["lights"].Elements[i]);

                //randomize the color, range, and power of the lights
                lights[i].Range = 25f + ((float)random.NextDouble() * 10f);
                lights[i].Falloff = 2f + ((float)random.NextDouble() * 4f);
                lights[i].Color = new Color(new Vector3((float)random.NextDouble(),
                    (float)random.NextDouble(), (float)random.NextDouble()));
            }

            // always set light 0 to pure white as a reference point
            lights[0].Color = Color.White;

        }

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

            // handle inputs for the sample camera
            camera.HandleDefaultGamepadControls(gpState, gameTime);
            camera.HandleDefaultKeyboardControls(kbState, gameTime);

            // handle inputs specific to this sample
            HandleInput(gameTime, gpState, kbState);

            // The camera position should also be updated for the
            // Phong specular component to be meaningful.
            cameraPositionParameter.SetValue(camera.Position);

            // replace the "last" gamepad and keyboard states
            lastGpState = gpState;
            lastKbState = kbState;

            base.Update(gameTime);
        }

        private void HandleInput(GameTime gameTime, GamePadState gpState,
            KeyboardState kbState)
        {
            float elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            // handle input for rotating the materials
            if (((gpState.Buttons.X == ButtonState.Pressed) &&
                (lastGpState.Buttons.X == ButtonState.Released)) ||
                (kbState.IsKeyDown(Keys.Tab) && lastKbState.IsKeyUp(Keys.Tab)))
            {
                // switch the active mesh
                materialRotation = (materialRotation + 1) % materials.Length;
            }


            // handle input for adding lights to the scene
            if (((gpState.Buttons.RightShoulder == ButtonState.Pressed) &&
                (lastGpState.Buttons.RightShoulder == ButtonState.Released)) ||
                (kbState.IsKeyDown(Keys.Add) && lastKbState.IsKeyUp(Keys.Add)))
            {
                numLights = ((numLights) % (maxLights )) + 1;

                baseEffect.Parameters["numLights"].SetValue(numLights);
            }


            // handle input for removing lights from the scene
            if (((gpState.Buttons.LeftShoulder == ButtonState.Pressed) &&
                (lastGpState.Buttons.LeftShoulder == ButtonState.Released)) ||
                (kbState.IsKeyDown(Keys.Subtract) &&
                lastKbState.IsKeyUp(Keys.Subtract)))
            {
                numLights = (numLights - 1);
                if (numLights < 1) numLights = maxLights;

                baseEffect.Parameters["numLights"].SetValue(numLights);
            }

            // handle input for generating new values for the lights
            if (((gpState.Buttons.Y == ButtonState.Pressed) &&
                (lastGpState.Buttons.Y == ButtonState.Released)) ||
                (kbState.IsKeyDown(Keys.Space) && lastKbState.IsKeyUp(Keys.Space)))
            {
                GenerateRandomLights();
            }

            // handle mesh rotation inputs
            float dx =
                SampleArcBallCamera.ReadKeyboardAxis(kbState, Keys.Left, Keys.Right) +
                gpState.ThumbSticks.Left.X;
            float dy =
                SampleArcBallCamera.ReadKeyboardAxis(kbState, Keys.Down, Keys.Up) +
                gpState.ThumbSticks.Left.Y;

            // apply mesh rotation from inputs
            if (dx != 0)
            {
                meshRotation *= Matrix.CreateFromAxisAngle(camera.Up,
                    elapsedTime * dx);
            }
            if (dy != 0)
            {
                meshRotation *= Matrix.CreateFromAxisAngle(camera.Right,
                    elapsedTime * -dy);
            }

            // handle the light rotation inputs
            float lightRotation =
                SampleArcBallCamera.ReadKeyboardAxis(kbState, Keys.PageUp,
                Keys.PageDown) + gpState.Triggers.Right - gpState.Triggers.Left;

            if (lightRotation != 0)
            {
                Matrix rotation = Matrix.CreateRotationY(lightRotation * elapsedTime);

                foreach (PointLight light in lights)
                {
                    // rotate all of the lights by transforming their positions
                    light.Position = Vector4.Transform(light.Position, rotation);
                }
            }
        }


        /// <summary>
        /// Draw the current scene.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // enable the depth buffer since geometry will be drawn
            graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            // always set the shared effects parameters
            viewParameter.SetValue(camera.ViewMatrix);
            cameraPositionParameter.SetValue(camera.Position);

            //////////////////////////////////////////////////////////////
            // Example 1.5                                              //
            //                                                          //
            // The materials in this sample can be applied to any of    //
            // the primitive meshs in the scene.  Sorting by materials  //
            // can generally improve performance by minimizing          //
            // state switching on the graphics device.                  //
            //////////////////////////////////////////////////////////////
            for (int i = 0; i < 8; i++)
            {
                Matrix world = meshRotation * meshWorlds[i];
                materials[(i + materialRotation) % 8].DrawModelWithMaterial(
                    sampleMeshes[i % 4], ref world);
            }
            
            floorMaterial.DrawModelWithMaterial(sampleMeshes[0], ref floorWorld);

            // draw a representation of the point lights in the scene
            DrawLights();

            // draw the technique name and specular settings
            spriteBatch.Begin();
            spriteBatch.DrawString(debugTextFont, 
                shaderVersionString,
                safeBounds, Color.White);
            spriteBatch.DrawString(debugTextFont, 
                "Number of lights: " + numLights + " out of " + maxLights ,
                safeBounds + (1f * debugTextHeight), Color.White);
            spriteBatch.DrawString(debugTextFont,
                "Use Shoulder Buttons to add lights",
                safeBounds + (2f * debugTextHeight), Color.White);
            spriteBatch.End();
            
            base.Draw(gameTime);
        }

        /// <summary>
        /// This simple draw function is used to draw the on-screen
        /// representation of the lights affecting the meshes in the scene.
        /// </summary>
        public void DrawLights()
        {
            ModelMesh mesh = pointLightMesh.Meshes[0];
            ModelMeshPart meshPart = mesh.MeshParts[0];

            graphics.GraphicsDevice.Vertices[0].SetSource(
                mesh.VertexBuffer, meshPart.StreamOffset, meshPart.VertexStride);
            graphics.GraphicsDevice.VertexDeclaration = meshPart.VertexDeclaration;
            graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
           

            pointLightMeshEffect.Begin(SaveStateMode.None);
            pointLightMeshEffect.CurrentTechnique.Passes[0].Begin();
            

            for (int i = 0; i < numLights; i++)
            {
                lightMeshWorld.M41 = lights[i].Position.X;
                lightMeshWorld.M42 = lights[i].Position.Y;
                lightMeshWorld.M43 = lights[i].Position.Z;

                pointLightMeshEffect.Parameters["world"].SetValue(lightMeshWorld);
                pointLightMeshEffect.Parameters["lightColor"].SetValue(
                    lights[i].Color.ToVector4());
                pointLightMeshEffect.CommitChanges();

                graphics.GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, meshPart.BaseVertex, 0,
                    meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);

            }
            pointLightMeshEffect.CurrentTechnique.Passes[0].End();
            pointLightMeshEffect.End();
        }
        #endregion

        #region Entry Point
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            using (MaterialsAndLights game = new MaterialsAndLights())
            {
                game.Run();
            }
        }
        #endregion
    }
}