#region File Description
//-----------------------------------------------------------------------------
// CpuSkinningDemoGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using CpuSkinningDataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace CpuSkinningDemo
{
    public class CpuSkinningDemoGame : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont font;

        // strings used to display the current render mode
        private const string gpuSkinningOn = "GPU skinning";
        private const string cpuSkinningOn = "CPU skinning";

        // this Model uses the GPU based SkinnedEffect
        private Model gpuDude;

        // this is our custom model class that uses CPU skinning and BasicEffect
        private CpuSkinnedModel cpuDude;

        // this handles our animation
        private AnimationPlayer animationPlayer;

        // whether or not to display the CPU skinned model
        private bool displayCpuModel = false;

        // camera settings
        private float cameraRotation = 0;
        private float cameraArc = 0;

        // mouse input used on Windows only
#if WINDOWS
        MouseState mouse, mousePrev;
#endif
        
        public CpuSkinningDemoGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);

#if WINDOWS_PHONE
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;
#else
            IsMouseVisible = true;
#endif

            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // add our frame rate counter
            Components.Add(new FrameRateCounter(this));
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");

            // load our two models
            gpuDude = Content.Load<Model>("dude_gpu");
            cpuDude = Content.Load<CpuSkinnedModel>("dude_cpu");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(cpuDude.SkinningData);
            AnimationClip clip = cpuDude.SkinningData.AnimationClips["Take 001"];
            animationPlayer.StartClip(clip);

            // use gestures for input. taps change rendering mode, drags move the camera.
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.FreeDrag;
        }
        
        protected override void Update(GameTime gameTime)
        {
            // exit on the back button
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            // update animations
            animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);

            // read in all available gestures
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                // on tap, switch rendering mode
                if (gesture.GestureType == GestureType.Tap)
                {
                    displayCpuModel = !displayCpuModel;
                }

                // on drag, rotate the camera
                else if (gesture.GestureType == GestureType.FreeDrag)
                {
                    HandleDrag(gesture.Delta);
                }
            }
            
#if WINDOWS
            mousePrev = mouse;
            mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                HandleDrag(new Vector2(mouse.X - mousePrev.X, mouse.Y - mousePrev.Y));
            }
            else if (mouse.RightButton == ButtonState.Pressed && mousePrev.RightButton == ButtonState.Released)
            {
                displayCpuModel = !displayCpuModel;
            }
#endif

            base.Update(gameTime);
        }

        private void HandleDrag(Vector2 delta)
        {
            cameraRotation += delta.X / 4;
            cameraArc = MathHelper.Clamp(cameraArc - delta.Y / 4, -70, 70);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Compute camera matrices.
            const float cameraDistance = 100;
            Matrix view = Matrix.CreateTranslation(0, -40, 0) *
                          Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                          Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                          Matrix.CreateLookAt(new Vector3(0, 0, -cameraDistance), new Vector3(0, 0, 0), Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1000);

            // Draw the background.
            GraphicsDevice.Clear(Color.Black);

            // reset graphics state to be appropriate for drawing our model
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // draw the correct model based on our current rendering mode
            if (displayCpuModel)
            {
                foreach (CpuSkinnedModelPart modelPart in cpuDude.Parts)
                {
                    modelPart.SetBones(animationPlayer.SkinTransforms);

                    modelPart.Effect.SpecularColor = Vector3.Zero;

                    ConfigureEffectMatrices(modelPart.Effect, Matrix.Identity, view, projection);
                    ConfigureEffectLighting(modelPart.Effect);

                    modelPart.Draw();
                }
            }
            else
            {
                foreach (ModelMesh mesh in gpuDude.Meshes)
                {
                    foreach (SkinnedEffect effect in mesh.Effects)
                    {
                        effect.SetBoneTransforms(animationPlayer.SkinTransforms);

                        effect.SpecularColor = Vector3.Zero;

                        ConfigureEffectMatrices(effect, Matrix.Identity, view, projection);
                        ConfigureEffectLighting(effect);
                    }

                    mesh.Draw();
                }
            }

            // draw our current skinning mode to the screen
            spriteBatch.Begin();
            spriteBatch.DrawString(font, displayCpuModel ? cpuSkinningOn : gpuSkinningOn, new Vector2(32, GraphicsDevice.Viewport.Height - 100), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Helper method to set effect matrices.
        /// </summary>
        private void ConfigureEffectMatrices(IEffectMatrices effect, Matrix world, Matrix view, Matrix projection)
        {
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;
        }

        /// <summary>
        /// Helper to configure effect lighting.
        /// </summary>
        private void ConfigureEffectLighting(IEffectLights effect)
        {
            effect.EnableDefaultLighting();
            effect.DirectionalLight0.Direction = Vector3.Backward;
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight1.Enabled = false;
            effect.DirectionalLight2.Enabled = false;
        }

#if WINDOWS || XBOX
        static void Main()
        {
            using (CpuSkinningDemoGame game = new CpuSkinningDemoGame())
            {
                game.Run();
            }
        }
#endif
    }
}
