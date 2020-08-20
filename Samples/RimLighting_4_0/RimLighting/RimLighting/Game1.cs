//-----------------------------------------------------------------------------
// Game1.cs
//
// Demonstrates how to use Environment Map Effect to mimic Rim Lighting
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace RimLighting
{   
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SampleGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        // Whether we are rotating world or camera
        enum RotatingMode
        {
            RotatingWorld,
            RotatingCamera
        };
        RotatingMode rotatingMode = RotatingMode.RotatingWorld;

        // Button to switch between the two rotating modes above
        Button buttonToggleWorldCamera;

        // Slidebars to tweak the effect
        Slidebar slideBarEnvironmentMapAmount;
        Slidebar slideBarFresnelFactor;

        // List of all UI Elements, which facilitates calling .Update() and .Draw() on the UI elements
        List<UIElement> uiElementList = new List<UIElement>();

        static Vector3 CameraInitPosition = new Vector3(0, 0, -60);
        ModelViewerCamera modelViewerCamera = new ModelViewerCamera(CameraInitPosition, Vector3.Up, 0, 0, 480, 800);
        
        // The mesh to be rendered
        Model model;

        // The cube texture for rimlighting effect
        TextureCube texureRimlightingCube;

        // Default texture for the mesh
        Texture2D texure2D;

        Matrix matrixWorld = Matrix.Identity;
        Matrix matrixView = Matrix.CreateLookAt(CameraInitPosition,
                                                Vector3.Zero,
                                                Vector3.Up);
        Vector2 vec2RotWorld = Vector2.Zero;
        Vector2 vec2RotCamera = Vector2.Zero;

        public SampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Pre-autoscale settings.
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            graphics.IsFullScreen = true;            
        }                

        void slideBarEnvironmentMapAmount_OnValueChanged(object sender)
        {
            slideBarEnvironmentMapAmount.Text = string.Format("Amount: {0}", slideBarEnvironmentMapAmount.Value);
        }         

        void slideBarFresnelFactor_OnValueChanged(object sender)
        {
            slideBarFresnelFactor.Text = string.Format("Thickness (FresnelFactor): {0}", slideBarFresnelFactor.Value);
        }          

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Font");

            // This is the mesh we want to render
            model = Content.Load<Model>("head");
            
            // An empty white texture used as default texture for the effect
            texure2D = Content.Load<Texture2D>("blankTex");

            // The cubemap used for generating rim light
            texureRimlightingCube = Content.Load<TextureCube>("OutputCube");

            // Add UI controls
            buttonToggleWorldCamera = new Button(GraphicsDevice, spriteFont, "Rotating World")
            {
                Position = new Vector2(0, graphics.PreferredBackBufferHeight - 31),
                Size = new Vector2(160, 30),
                IsVisible = true
            };
            buttonToggleWorldCamera.OnClick += new Button.ClickEventHandler(buttonToggleWorldCamera_OnClick);
            uiElementList.Add(buttonToggleWorldCamera);

            slideBarEnvironmentMapAmount = new Slidebar(this, spriteFont, 0, 5);
            slideBarEnvironmentMapAmount.IsVisible = true;
            slideBarEnvironmentMapAmount.Position = new Vector2(0, graphics.PreferredBackBufferHeight - 180);            
            slideBarEnvironmentMapAmount.OnValueChanged += new Slidebar.ValueChangedHandler(slideBarEnvironmentMapAmount_OnValueChanged);
            // Note setting .Value should be before the next line because this makes .TextSize valid which is used below
            slideBarEnvironmentMapAmount.Value = 2.5f; 
            slideBarEnvironmentMapAmount.SetBarOffsetSize(10, slideBarEnvironmentMapAmount.TextSize.Y, graphics.PreferredBackBufferWidth - 20, 4);
            uiElementList.Add(slideBarEnvironmentMapAmount);   

            slideBarFresnelFactor = new Slidebar(this, spriteFont, 0, 10);
            slideBarFresnelFactor.IsVisible = true;
            slideBarFresnelFactor.Position = new Vector2(0, graphics.PreferredBackBufferHeight - 100);            
            slideBarFresnelFactor.OnValueChanged += new Slidebar.ValueChangedHandler(slideBarFresnelFactor_OnValueChanged);
            slideBarFresnelFactor.Value = 6;
            slideBarFresnelFactor.SetBarOffsetSize(10, slideBarFresnelFactor.TextSize.Y, graphics.PreferredBackBufferWidth - 20, 4);
            uiElementList.Add(slideBarFresnelFactor);               

            base.LoadContent();
        }                            

        void buttonToggleWorldCamera_OnClick(object sender)
        {
            if (rotatingMode == RotatingMode.RotatingWorld)
            {
                rotatingMode = RotatingMode.RotatingCamera;
                buttonToggleWorldCamera.Text = "Rotating Camera";
            }
            else
            {
                rotatingMode = RotatingMode.RotatingWorld;
                buttonToggleWorldCamera.Text = "Rotating World";
            }
        }        

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            TouchCollection tc = TouchPanel.GetState();

            // Update our UI elements
            for (int t = 0; t < tc.Count; t++)
            {
                for (int u = 0; u < uiElementList.Count; ++u)
                    uiElementList[u].HandleTouch(tc[t]);
            }

            // Update World or View matrices according to the user's drag on screen
            if (!slideBarEnvironmentMapAmount.IsDragging && !slideBarFresnelFactor.IsDragging)
            {
                modelViewerCamera.IsRotatingWorld = rotatingMode == RotatingMode.RotatingWorld;
                for (int t = 0; t < tc.Count; ++t)
                    modelViewerCamera.HandleTouch(tc[t]);                                
            }                        

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.3f, 0.3f, 0.3f));

            // Please refer to the sample doc on why the matrix should be set like this for implementing RimLighting
            Matrix world = modelViewerCamera.GetWorldMatrix() * modelViewerCamera.GetViewMatrix();
            Matrix view = Matrix.Identity;
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    GraphicsDevice.Viewport.AspectRatio,
                                                                    1, 10000);
            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Draw the model.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (EnvironmentMapEffect effect in mesh.Effects)
                {
                    effect.EnvironmentMap = texureRimlightingCube;                    

                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = projection;

                    effect.DirectionalLight0.Enabled = true;
                    // Please refer to the sample doc
                    effect.DirectionalLight0.Direction = Vector3.TransformNormal(Vector3.Left, modelViewerCamera.GetViewMatrix()); 
                    effect.DirectionalLight1.Enabled = false;
                    effect.DirectionalLight2.Enabled = false;
                   
                    effect.Texture = texure2D;
                                       
                    effect.DiffuseColor = Color.White.ToVector3();

                    effect.FresnelFactor = slideBarFresnelFactor.Value;
                    effect.EnvironmentMapAmount = slideBarEnvironmentMapAmount.Value;
                }

                mesh.Draw();
            }

            // Draw our UI elements
            for (int i = 0; i < uiElementList.Count; ++i)
            {
                uiElementList[i].Draw(spriteBatch);
            }                

            base.Draw(gameTime);
        }
    }
}
