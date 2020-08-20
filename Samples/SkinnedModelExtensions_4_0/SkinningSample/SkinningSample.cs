#region File Description
//-----------------------------------------------------------------------------
// SkinningSample.cs
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
using SkinnedModel;
using Primitives3D;
#endregion

namespace SkinningSample
{
    /// <summary>
    /// Sample game showing how to display skinned character animation.
    /// </summary>
    public class SkinningSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();

        KeyboardState previousKeyboardState = new KeyboardState();
        GamePadState previousGamePadState = new GamePadState();

        SkinnedSphere[] skinnedSpheres;
        BoundingSphere[] boundingSpheres;

        bool showSpheres;
        SpherePrimitive spherePrimitive; 
        
        Model currentModel;
        AnimationPlayer animationPlayer;
        SkinningData skinningData;
        Matrix[] boneTransforms;
        Model baseballBat;

        float cameraArc = 0;
        float cameraRotation = 0;
        float cameraDistance = 100;

        #endregion

        #region Initialization


        public SkinningSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            
            graphics.IsFullScreen = true;            
#endif
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load the model.
            currentModel = Content.Load<Model>("dude");

            // Look up our custom skinning information.
            skinningData = currentModel.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            boneTransforms = new Matrix[skinningData.BindPose.Count];

            // Load the baseball bat model.
            baseballBat = Content.Load<Model>("baseballbat");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);

            AnimationClip clip = skinningData.AnimationClips["Take 001"];

            animationPlayer.StartClip(clip);

            // Load the bounding spheres.
            skinnedSpheres = Content.Load<SkinnedSphere[]>("CollisionSpheres");
            boundingSpheres = new BoundingSphere[skinnedSpheres.Length];

            spherePrimitive = new SpherePrimitive(GraphicsDevice, 1, 12);
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

            // Read gamepad inputs.
            float headRotation = currentGamePadState.ThumbSticks.Left.X;
            float armRotation = Math.Max(currentGamePadState.ThumbSticks.Left.Y, 0);

            // Read keyboard inputs.
            if (currentKeyboardState.IsKeyDown(Keys.PageUp))
                headRotation = -1;
            else if (currentKeyboardState.IsKeyDown(Keys.PageDown))
                headRotation = 1;

            if (currentKeyboardState.IsKeyDown(Keys.Space))
                armRotation = 0.5f;

            // Create rotation matrices for the head and arm bones.
            Matrix headTransform = Matrix.CreateRotationX(headRotation);
            Matrix armTransform = Matrix.CreateRotationY(-armRotation);

            // Tell the animation player to compute the latest bone transform matrices.
            animationPlayer.UpdateBoneTransforms(gameTime.ElapsedGameTime, true);

            // Copy the transforms into our own array, so we can safely modify the values.
            animationPlayer.GetBoneTransforms().CopyTo(boneTransforms, 0);

            // Modify the transform matrices for the head and upper-left arm bones.
            int headIndex = skinningData.BoneIndices["Head"];
            int armIndex = skinningData.BoneIndices["L_UpperArm"];

            boneTransforms[headIndex] = headTransform * boneTransforms[headIndex];
            boneTransforms[armIndex] = armTransform * boneTransforms[armIndex];
            
            // Tell the animation player to recompute the world and skin matrices.
            animationPlayer.UpdateWorldTransforms(Matrix.Identity, boneTransforms);
            animationPlayer.UpdateSkinTransforms();

            UpdateBoundingSpheres(); 
            
            base.Update(gameTime);
        }


        /// <summary>
        /// Updates the boundingSpheres array to match the current animation state.
        /// </summary>
        void UpdateBoundingSpheres()
        {
            // Look up the current world space bone positions.
            Matrix[] worldTransforms = animationPlayer.GetWorldTransforms();

            for (int i = 0; i < skinnedSpheres.Length; i++)
            {
                // Convert the SkinnedSphere description to a BoundingSphere.
                SkinnedSphere source = skinnedSpheres[i];
                Vector3 center = new Vector3(source.Offset, 0, 0);
                BoundingSphere sphere = new BoundingSphere(center, source.Radius);

                // Transform the BoundingSphere by its parent bone matrix,
                // and store the result into the boundingSpheres array.
                int boneIndex = skinningData.BoneIndices[source.BoneName];

                boundingSpheres[i] = sphere.Transform(worldTransforms[boneIndex]);
            }
        }
        
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.CornflowerBlue);

            Matrix[] bones = animationPlayer.GetSkinTransforms();

            // Compute camera matrices.
            Matrix view = Matrix.CreateTranslation(0, -40, 0) * 
                          Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                          Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                          Matrix.CreateLookAt(new Vector3(0, 0, -cameraDistance), 
                                              new Vector3(0, 0, 0), Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    device.Viewport.AspectRatio,
                                                                    1,
                                                                    10000);

            // Render the skinned mesh.
            foreach (ModelMesh mesh in currentModel.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }

            DrawBaseballBat(view, projection);

            if (showSpheres)
            {
                DrawBoundingSpheres(view, projection);
            } 
            
            base.Draw(gameTime);
        }


        /// <summary>
        /// Draws the animated bounding spheres.
        /// </summary>
        void DrawBoundingSpheres(Matrix view, Matrix projection)
        {
            GraphicsDevice.RasterizerState = Wireframe;

            foreach (BoundingSphere sphere in boundingSpheres)
            {
                Matrix world = Matrix.CreateScale(sphere.Radius) *
                               Matrix.CreateTranslation(sphere.Center);

                spherePrimitive.Draw(world, view, projection, Color.White);
            }

            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }


        static RasterizerState Wireframe = new RasterizerState
        {
            FillMode = FillMode.WireFrame
        };


        /// <summary>
        /// Draws the baseball bat.
        /// </summary>
        void DrawBaseballBat(Matrix view, Matrix projection)
        {
            int handIndex = skinningData.BoneIndices["L_Index1"];

            Matrix[] worldTransforms = animationPlayer.GetWorldTransforms();

            // Nudge the bat over so it appears between the left thumb and index finger.
            Matrix batWorldTransform = Matrix.CreateTranslation(-1.3f, 2.1f, 0.1f) *
                                       worldTransforms[handIndex];

            foreach (ModelMesh mesh in baseballBat.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = batWorldTransform;
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();
                }

                mesh.Draw();
            }
        }
        

        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        private void HandleInput()
        {
            previousKeyboardState = currentKeyboardState;
            previousGamePadState = currentGamePadState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            // Toggle the collision sphere display.
            if ((currentKeyboardState.IsKeyDown(Keys.Enter) &&
                 previousKeyboardState.IsKeyUp(Keys.Enter)) ||
                (currentGamePadState.IsButtonDown(Buttons.A) &&
                 previousGamePadState.IsButtonUp(Buttons.A)))
            {
                showSpheres = !showSpheres;
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
                cameraDistance += time * 0.25f;

            if (currentKeyboardState.IsKeyDown(Keys.X))
                cameraDistance -= time * 0.25f;

            cameraDistance += currentGamePadState.Triggers.Left * time * 0.5f;
            cameraDistance -= currentGamePadState.Triggers.Right * time * 0.5f;

            // Limit the camera distance.
            if (cameraDistance > 500.0f)
                cameraDistance = 500.0f;
            else if (cameraDistance < 10.0f)
                cameraDistance = 10.0f;

            if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.R))
            {
                cameraArc = 0;
                cameraRotation = 0;
                cameraDistance = 100;
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
            using (SkinningSampleGame game = new SkinningSampleGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
