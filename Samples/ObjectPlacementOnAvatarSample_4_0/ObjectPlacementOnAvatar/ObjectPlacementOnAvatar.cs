#region File Description
//-----------------------------------------------------------------------------
// ObjectPlacementOnAvatar.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace ObjectPlacementOnAvatar
{
    /// <summary>
    /// This sample demonstrates how to place objects onto an avatar while it animates. 
    /// </summary>
    public class ObjectPlacementOnAvatarGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        // Description, renderer, and animation for
        // loading and drawing an animated avatar
        AvatarDescription avatarDescription;
        AvatarRenderer avatarRenderer;
        AvatarAnimation currentAvatarAnimation;
        AvatarAnimation[] avatarAnimations = new AvatarAnimation[4];

        // Model for the baseball bat that is going to
        // be placed on the avatar
        Model baseballBat;

        // Matrices used to draw the avatar
        Matrix world;
        Matrix view;
        Matrix projection;

        // List of the avatar bones in world space
        List<Matrix> bonesWorldSpace;

        // the current input states.  These are updated in the HandleInput function,
        // and used primarily in the UpdateCamera function.
        GamePadState currentGamePadState;
        GamePadState lastGamePadState;

        // the following constants control the speed at which the camera moves
        // how fast does the camera move up, down, left, and right?
        const float CameraRotateSpeed = .1f;
        // how fast does the camera zoom in and out?
        const float CameraZoomSpeed = .01f;
        // the camera can't be further away than this distance
        const float CameraMaxDistance = 10.0f;
        // and it can't be closer than this
        const float CameraMinDistance = 2.0f;

        // the following constants control the camera's default position
        const float CameraDefaultArc = 30.0f;
        const float CameraDefaultRotation = 0;
        const float CameraDefaultDistance = 3.0f;

        // Camera control values
        float cameraArc = CameraDefaultArc;
        float cameraRotation = CameraDefaultRotation;
        float cameraDistance = CameraDefaultDistance;

        #endregion


        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjectPlacementOnAvatarGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferMultiSampling = true;

            // Avatars require GamerServices
            Components.Add(new GamerServicesComponent(this));
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create random avatar description and load the renderer and animation
            avatarDescription = AvatarDescription.CreateRandom();
            avatarRenderer = new AvatarRenderer(avatarDescription);

            // Load 4 of the preset animations
            avatarAnimations[0] = new AvatarAnimation(AvatarAnimationPreset.Stand0);
            avatarAnimations[1] = new AvatarAnimation(AvatarAnimationPreset.Celebrate);
            avatarAnimations[2] = new AvatarAnimation(AvatarAnimationPreset.Clap);
            avatarAnimations[3] = new AvatarAnimation(AvatarAnimationPreset.Stand5);

            // Current animation to play and update
            currentAvatarAnimation = avatarAnimations[0];

            // Load the baseball bat model
            baseballBat = Content.Load<Model>("baseballbat");

            // Initialize the rendering matrices
            world = Matrix.CreateRotationY(MathHelper.Pi);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                     GraphicsDevice.Viewport.AspectRatio,
                                                     .01f, 200.0f);

            // Initialize the list of bones in world space
            bonesWorldSpace = new List<Matrix>(AvatarRenderer.BoneCount);
            for (int i = 0; i < AvatarRenderer.BoneCount; i++)
                bonesWorldSpace.Add(Matrix.Identity);
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

            // Set avatar rendering matrices
            avatarRenderer.World = world;
            avatarRenderer.View = view;
            avatarRenderer.Projection = projection;

            // Update the current animation and world space bones
            if (avatarRenderer.State == AvatarRendererState.Ready)
            {
                currentAvatarAnimation.Update(gameTime.ElapsedGameTime, true);
                BonesToWorldSpace(avatarRenderer,
                                  currentAvatarAnimation,
                                  bonesWorldSpace);
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawBaseballBat();

            avatarRenderer.Draw(currentAvatarAnimation.BoneTransforms,
                                currentAvatarAnimation.Expression);

            base.Draw(gameTime);
        }


        /// <summary>
        /// Draw the baseball bat
        /// </summary>
        private void DrawBaseballBat()
        {
            // Moves the bat closer to where we want it in the hand
            Matrix baseballBatOffset = Matrix.CreateRotationY(MathHelper.ToRadians(-20))
                                       * Matrix.CreateTranslation(0.01f, 0.05f, 0.0f);

            foreach (ModelMesh mesh in baseballBat.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    // Position the bat to be near the avatars right hand. The position
                    // of the right special bone can be found by looking up the value in
                    // our list of world space bones with the index of the bone we are 
                    // looking for. The bat is translated and rotated a small amount to
                    // make it look better in the hand.
                    effect.World = baseballBatOffset *
                                   bonesWorldSpace[(int)AvatarBone.SpecialRight];

                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }


        /// <summary>
        /// Updates a list of matrices to represent the location of the 
        /// avatar bones in world space with the avatar animation applied.
        /// </summary>
        private static void BonesToWorldSpace(AvatarRenderer renderer, AvatarAnimation animation,
                                                        List<Matrix> boneToUpdate)
        {
            // Bind pose of the avatar. 
            // These positions are in local space, and are relative to the parent bone.
            IList<Matrix> bindPose = renderer.BindPose;
            // The current animation pose. 
            // These positions are in local space, and are relative to the parent bone.
            IList<Matrix> animationPose = animation.BoneTransforms;
            // List of parent bones for each bone in the hierarchy 
            IList<int> parentIndex = renderer.ParentBones;

            // Loop all of the bones.
            // Since the bone hierarchy is sorted by depth 
            // we will transform the parent before any child.
            for (int i = 0; i < AvatarRenderer.BoneCount; i++)
            {
                // Find the transform of this bones parent.
                // If this is the first bone use the world matrix used on the avatar
                Matrix parentMatrix = (parentIndex[i] != -1)
                                       ? boneToUpdate[parentIndex[i]]
                                       : renderer.World;
                // Calculate this bones world space position
                boneToUpdate[i] = Matrix.Multiply(Matrix.Multiply(animationPose[i],
                                                                  bindPose[i]),
                                                                  parentMatrix);
            }
        }

        #endregion


        #region Input and Camera

        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        void HandleInput()
        {
            lastGamePadState = currentGamePadState;
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            // Check to see if we should load another random avatar
            if (currentGamePadState.Buttons.RightShoulder == ButtonState.Pressed &&
                lastGamePadState.Buttons.RightShoulder != ButtonState.Pressed)
            {
                avatarDescription = AvatarDescription.CreateRandom();
                avatarRenderer = new AvatarRenderer(avatarDescription);
            }

            // Check to see if we need to play another animation
            if (currentGamePadState.Buttons.A == ButtonState.Pressed &&
                lastGamePadState.Buttons.A != ButtonState.Pressed)
            {
                currentAvatarAnimation = avatarAnimations[1];
                currentAvatarAnimation.CurrentPosition = TimeSpan.Zero;
            }
            else if (currentGamePadState.Buttons.B == ButtonState.Pressed &&
                lastGamePadState.Buttons.B != ButtonState.Pressed)
            {
                currentAvatarAnimation = avatarAnimations[2];
                currentAvatarAnimation.CurrentPosition = TimeSpan.Zero;
            }
            else if (currentGamePadState.Buttons.X == ButtonState.Pressed &&
                lastGamePadState.Buttons.X != ButtonState.Pressed)
            {
                currentAvatarAnimation = avatarAnimations[3];
                currentAvatarAnimation.CurrentPosition = TimeSpan.Zero;
            }
            else if (currentGamePadState.Buttons.Y == ButtonState.Pressed &&
                lastGamePadState.Buttons.Y != ButtonState.Pressed)
            {
                currentAvatarAnimation = avatarAnimations[0];
                currentAvatarAnimation.CurrentPosition = TimeSpan.Zero;
            }
        }


        /// <summary>
        /// Handles input for moving the camera.
        /// </summary>
        void UpdateCamera(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // should we reset the camera?
            if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed)
            {
                cameraArc = CameraDefaultArc;
                cameraDistance = CameraDefaultDistance;
                cameraRotation = CameraDefaultRotation;
            }

            // Check for input to rotate the camera up and down around the model.
            cameraArc += currentGamePadState.ThumbSticks.Right.Y * time *
                CameraRotateSpeed;

            // Limit the arc movement.
            cameraArc = MathHelper.Clamp(cameraArc, -90.0f, 90.0f);

            // Check for input to rotate the camera around the model.
            cameraRotation += currentGamePadState.ThumbSticks.Right.X * time *
                CameraRotateSpeed;

            // Check for input to zoom camera in and out.
            cameraDistance += currentGamePadState.Triggers.Left * time
                * CameraZoomSpeed;
            cameraDistance -= currentGamePadState.Triggers.Right * time
                * CameraZoomSpeed;

            // clamp the camera distance so it doesn't get too close or too far away.
            cameraDistance = MathHelper.Clamp(cameraDistance,
                CameraMinDistance, CameraMaxDistance);

            Matrix unrotatedView = Matrix.CreateLookAt(
                new Vector3(0, 0, cameraDistance), new Vector3(0, 1, 0), Vector3.Up);

            view = Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                          Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                          unrotatedView;
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
            using (ObjectPlacementOnAvatarGame game = new ObjectPlacementOnAvatarGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}