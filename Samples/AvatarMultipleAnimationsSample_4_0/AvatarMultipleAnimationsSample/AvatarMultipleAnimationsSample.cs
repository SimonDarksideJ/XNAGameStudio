#region File Description
//-----------------------------------------------------------------------------
// AvatarMultipleAnimationsSample.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace AvatarMultipleAnimationsSample
{
    /// <summary>
    /// Defines what animation the avatar should be using
    /// </summary>
    public enum AnimationPlaybackMode { All, Celebrate, Wave };

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class AvatarMultipleAnimationsGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;
        SpriteFont font;

        // Description, renderer, and animation for
        // loading and drawing an animated avatar
        AvatarDescription avatarDescription;
        AvatarRenderer avatarRenderer;
        AvatarAnimation waveAnimation;
        AvatarAnimation celebrateAnimation;

        // List of the final list of bone transforms.
        // The list will contain both transforms from the wave and clap animations.
        List<Matrix> finalBoneTransforms = new List<Matrix>(AvatarRenderer.BoneCount);

        // List of the bone index values for the right arm and its children
        List<int> rightArmBones;

        // Playback mode defines what animations should be playing
        AnimationPlaybackMode animationPlaybackMode;

        // matrices used to draw the avatar
        Matrix world;
        Matrix view;
        Matrix projection;

        // The current input states.  These are updated in the HandleInput function,
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
        public AvatarMultipleAnimationsGame()
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
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");

            // Create random avatar description and load the renderer and animation
            avatarDescription = AvatarDescription.CreateRandom();
            avatarRenderer = new AvatarRenderer(avatarDescription);

            // Load the preset animations
            waveAnimation = new AvatarAnimation(AvatarAnimationPreset.Wave);
            celebrateAnimation = new AvatarAnimation(AvatarAnimationPreset.Celebrate);

            // Find the bone index values for the right arm and its children
            rightArmBones = FindInfluencedBones(AvatarBone.ShoulderRight, 
                                                avatarRenderer.ParentBones);

            for (int i = 0; i < AvatarRenderer.BoneCount; ++i)
            {
                finalBoneTransforms.Add(Matrix.Identity);
            }

            // Initialize the rendering matrices
            world = Matrix.CreateRotationY(MathHelper.Pi);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                     GraphicsDevice.Viewport.AspectRatio,
                                                     .01f, 200.0f);
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
                waveAnimation.Update(gameTime.ElapsedGameTime, true);
                celebrateAnimation.Update(gameTime.ElapsedGameTime, true);

                UpdateTransforms();
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            string message = "Playing: ";
            if (animationPlaybackMode == AnimationPlaybackMode.All)
            {
                message += "Celebrate + Wave";
            }
            else if (animationPlaybackMode == AnimationPlaybackMode.Celebrate)
            {
                message += "Celebrate";
            }
            else if (animationPlaybackMode == AnimationPlaybackMode.Wave)
            {
                message += "Wave";
            }
            message += "\n(Left Shoulder Button)";

            // Draw the text
            spriteBatch.Begin();
            spriteBatch.DrawString(font, message, new Vector2(161, 161), Color.Black);
            spriteBatch.DrawString(font, message, new Vector2(160, 160), Color.White);
            spriteBatch.End();

            // Draw the avatar with the combined transforms
            avatarRenderer.Draw(finalBoneTransforms, celebrateAnimation.Expression);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Combines the transforms of the clap and wave animations
        /// </summary>
        private void UpdateTransforms()
        {
            // List of bone transforms from the clap and wave animations
            ReadOnlyCollection<Matrix> celebrateTransforms = 
                                                       celebrateAnimation.BoneTransforms;
            ReadOnlyCollection<Matrix> waveTransforms = waveAnimation.BoneTransforms;

            // Check to see if we are playing both of the animations 
            if (animationPlaybackMode == AnimationPlaybackMode.All)
            {
                // Copy the celebrate transforms to the final list of transforms
                for (int i = 0; i < finalBoneTransforms.Count; i++)
                {
                    finalBoneTransforms[i] = celebrateTransforms[i];
                }

                // Overwrite the transforms for the bones that are part of the right arm 
                // with the wave animation transforms.
                for (int i = 0; i < rightArmBones.Count; i++)
                {
                    finalBoneTransforms[rightArmBones[i]] =
                                                        waveTransforms[rightArmBones[i]];
                }
            }
            // Check to see if we are just playing the celebrate animation
            else if (animationPlaybackMode == AnimationPlaybackMode.Celebrate)
            {
                // Copy the celebrate transforms to the final list of transforms
                for (int i = 0; i < finalBoneTransforms.Count; i++)
                {
                    finalBoneTransforms[i] = celebrateTransforms[i];
                }
            }
            else if (animationPlaybackMode == AnimationPlaybackMode.Wave)
            {
                // We are just using the wave so use all bones
                for (int i = 0; i < finalBoneTransforms.Count; i++)
                {
                    finalBoneTransforms[i] = waveTransforms[i];
                }
            }
        }

        /// <summary>
        /// Creates a list of bone index values for the given avatar bone 
        /// and its children.
        /// </summary>
        /// <param name="avatarBone">The root bone to start search</param>
        /// <param name="parentBones">List of parent bones from the avatar 
        /// renderer</param>
        /// <returns></returns>
        List<int> FindInfluencedBones(AvatarBone avatarBone, 
                                                     ReadOnlyCollection<int> parentBones)
        {
            // New list of bones that will be influenced
            List<int> influencedList = new List<int>();
            // Add the first bone
            influencedList.Add((int)avatarBone);

            // Start searching after the first bone
            int currentBoneID = influencedList[0] + 1;

            // Loop until we are done with all of the bones
            while (currentBoneID < parentBones.Count)
            {
                // Check to see if the current bone is a child of any of the 
                // previous bones we have found
                if (influencedList.Contains(parentBones[currentBoneID]))
                {
                    // Add the bone to the influenced list
                    influencedList.Add(currentBoneID);
                }
                // Move to the next bone
                currentBoneID++;
            }
            
            return influencedList;
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

            // Check to see if we need to change play modes
            if (currentGamePadState.Buttons.LeftShoulder == ButtonState.Pressed &&
                lastGamePadState.Buttons.LeftShoulder != ButtonState.Pressed)
            {
                animationPlaybackMode += 1;
                if (animationPlaybackMode > AnimationPlaybackMode.Wave)
                {
                    animationPlaybackMode = AnimationPlaybackMode.All;
                }
            }

            // Check to see if we should load another random avatar
            if (currentGamePadState.Buttons.RightShoulder == ButtonState.Pressed &&
                lastGamePadState.Buttons.RightShoulder != ButtonState.Pressed)
            {
                avatarDescription = AvatarDescription.CreateRandom();
                avatarRenderer = new AvatarRenderer(avatarDescription);
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
            using (AvatarMultipleAnimationsGame game = new AvatarMultipleAnimationsGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
