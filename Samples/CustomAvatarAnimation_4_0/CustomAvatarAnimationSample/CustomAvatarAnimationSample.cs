#region File Description
//-----------------------------------------------------------------------------
// CustomAvatarAnimationSample.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CustomAvatarAnimation;
#endregion

namespace CustomAvatarAnimationSample
{
    /// <summary>
    /// The possible animation types on the avatar.
    /// </summary>
    public enum AnimationType { Idle1, Idle2, Idle3, Idle4, Walk, 
                                Jump, Kick, Punch, Faint };


    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CustomAvatarAnimationSampleGame : Microsoft.Xna.Framework.Game
    {

        #region Graphics Data

        GraphicsDeviceManager graphics;

        // Model to display the ground
        Model groundModel;

        // World, View and Projection matricies used for rendering
        Matrix world;
        Matrix view;
        Matrix projection;

        // the following constants control the camera's default position
        const float CameraDefaultArc = MathHelper.Pi / 10;
        const float CameraDefaultRotation = MathHelper.Pi;
        const float CameraDefaultDistance = 2.5f;

        // Camera values
        float cameraArc = CameraDefaultArc;
        float cameraRotation = CameraDefaultRotation;
        float cameraDistance = CameraDefaultDistance;

        #endregion


        #region Avatar Data

        // The AvatarDescription and AvatarRenderer that are used to render the avatar
        AvatarRenderer avatarRenderer;
        AvatarDescription avatarDescription;

        // Animations that will be used
        IAvatarAnimation[] animations;

        // Tells us if we are using an idle, walking, or other animations
        AnimationType currentType;

        #endregion


        #region Fields

        // Store the current and last gamepad state
        GamePadState currentGamePadState;
        GamePadState lastGamePadState;

        // A random number generator for picking new idle animations
        Random random = new Random();

        #endregion


        #region Initialization

        /// <summary>
        /// Creates a new AvatarCustomAnimationSample object.
        /// </summary>
        public CustomAvatarAnimationSampleGame()
        {
            // initialize the graphics
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferMultiSampling = true;

            // initialize gamer services
            Components.Add(new GamerServicesComponent(this));

            SignedInGamer.SignedIn += new EventHandler<SignedInEventArgs>(SignedInGamer_SignedIn);
        }

        
        /// <summary>
        /// Handle signed in gamer event as start avatar loading
        /// </summary>
        void SignedInGamer_SignedIn(object sender, SignedInEventArgs e)
        {
            // Only load the avatar for player one
            if (e.Gamer.PlayerIndex == PlayerIndex.One)
            {
                // Load the player one avatar
                LoadAvatar(e.Gamer);
            }
        }


        /// <summary>
        /// Load all graphical content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load custom animations
            CustomAvatarAnimationData animationData;

            // We will use 8 different animations
            animations = new IAvatarAnimation[9];

            // Load the idle animations
            for (int i = 0; i < 4; i++)
            {
                animations[i] = new AvatarAnimation(
                         (AvatarAnimationPreset)((int)AvatarAnimationPreset.Stand0 + i));
            }


            // Load the walk animation
            animationData = Content.Load<CustomAvatarAnimationData>("Walk");
            animations[4] = new CustomAvatarAnimationPlayer(animationData.Name, animationData.Length, 
                                          animationData.Keyframes, animationData.ExpressionKeyframes);

            // Load the jump animation
            animationData = Content.Load<CustomAvatarAnimationData>("Jump");
            animations[5] = new CustomAvatarAnimationPlayer(animationData.Name, animationData.Length,
                                          animationData.Keyframes, animationData.ExpressionKeyframes);

            // Load the kick animation
            animationData = Content.Load<CustomAvatarAnimationData>("Kick");
            animations[6] = new CustomAvatarAnimationPlayer(animationData.Name, animationData.Length,
                                          animationData.Keyframes, animationData.ExpressionKeyframes);

            // Load the punch animation
            animationData = Content.Load<CustomAvatarAnimationData>("Punch");
            animations[7] = new CustomAvatarAnimationPlayer(animationData.Name, animationData.Length,
                                          animationData.Keyframes, animationData.ExpressionKeyframes);

            // Load the faint animation
            animationData = Content.Load<CustomAvatarAnimationData>("Faint");
            animations[8] = new CustomAvatarAnimationPlayer(animationData.Name, animationData.Length,
                                          animationData.Keyframes, animationData.ExpressionKeyframes);

            // Load the model for the ground
            groundModel = Content.Load<Model>("ground");

            // Select a random idle animation to start
            PlayRandomIdle();

            // Create the projection to use
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                                      GraphicsDevice.Viewport.AspectRatio, .01f, 200.0f);
            world = Matrix.Identity;
        }

        #endregion


        #region Updating

        protected override void Update(GameTime gameTime)
        {
            // Get the current gamepad state and store the old
            lastGamePadState = currentGamePadState;
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Handle gamer input
            HandleAvatarInput(gameTime);
            HandleCameraInput();

            // Loop animation if we are walking
            bool loopAnimation = (currentType == AnimationType.Walk);

            // Update the current animation
            animations[(int)currentType].Update(gameTime.ElapsedGameTime, loopAnimation);

            // Check to see if we need to end the animation
            if (!loopAnimation)
            {
                if (animations[(int)currentType].CurrentPosition ==
                    animations[(int)currentType].Length)
                {
                    // Start new idle animation
                    PlayRandomIdle();
                }
            }

            base.Update(gameTime);
        }

        #endregion


        #region Drawing

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the ground first since the avatar has transparent parts
            groundModel.Draw(Matrix.Identity, view, projection);

            // Draw the avatar
            if (avatarRenderer != null)
            {
                avatarRenderer.Draw(animations[(int)currentType]);
            }

            base.Draw(gameTime);
        }

        #endregion


        #region Input Handling

        /// <summary>
        /// Check for user input to play animations on the avatar
        /// </summary>
        private void HandleAvatarInput(GameTime gameTime)
        {
            // Check to see if the user wants to load a random avatar 
            // by pressing the Right Shoulder
            if (currentGamePadState.Buttons.RightShoulder == ButtonState.Pressed &&
                   lastGamePadState.Buttons.RightShoulder != ButtonState.Pressed)
            {
                LoadRandomAvatar();
            }
            // Check to see if the user wants to load the users avatar 
            // by pressing the Left Shoulder
            else if (currentGamePadState.Buttons.LeftShoulder == ButtonState.Pressed &&
                        lastGamePadState.Buttons.LeftShoulder != ButtonState.Pressed)
            {
                // Load the users avatar
                if (SignedInGamer.SignedInGamers[PlayerIndex.One] != null)
                {
                    LoadAvatar(SignedInGamer.SignedInGamers[PlayerIndex.One]);
                }
            }

            // Check to see if the user wants to play one of the other 
            // animations by pressing one of the gamepad buttons
            if (currentGamePadState.Buttons.A == ButtonState.Pressed &&
                lastGamePadState.Buttons.A != ButtonState.Pressed)
            {
                PlayAnimation(AnimationType.Jump);
            }
            if (currentGamePadState.Buttons.B == ButtonState.Pressed &&
                lastGamePadState.Buttons.B != ButtonState.Pressed)
            {
                PlayAnimation(AnimationType.Kick);
            }
            if (currentGamePadState.Buttons.X == ButtonState.Pressed &&
                lastGamePadState.Buttons.X != ButtonState.Pressed)
            {
                PlayAnimation(AnimationType.Punch);
            }
            if (currentGamePadState.Buttons.Y == ButtonState.Pressed &&
                lastGamePadState.Buttons.Y != ButtonState.Pressed)
            {
                PlayAnimation(AnimationType.Faint);
            }

            // Update the avatars location
            UpdateAvatarMovement(gameTime);
        }


        /// <summary>
        /// Update the avatars movement based on user input
        /// </summary>
        private void UpdateAvatarMovement(GameTime gameTime)
        {
            // Create vector from the left thumbstick location
            Vector2 leftThumbStick = currentGamePadState.ThumbSticks.Left;

            // The direction for our Avatar
            Vector3 avatarForward = world.Forward;

            // The amount we want to translate
            Vector3 translate = Vector3.Zero;

            // Clamp thumbstick to make sure the user really wants to move
            if (leftThumbStick.Length() > 0.2f)
            {
                // Create our direction vector
                leftThumbStick.Normalize();

                // Find the new avatar forward
                avatarForward.X = leftThumbStick.X;
                avatarForward.Y = 0;
                avatarForward.Z = -leftThumbStick.Y;
                // Translate the thumbstick using the current camera rotation
                avatarForward = Vector3.Transform(avatarForward,
                                                 Matrix.CreateRotationY(cameraRotation));
                avatarForward.Normalize();

                // Determine the amount of translation
                translate = avatarForward
                            * ((float)gameTime.ElapsedGameTime.TotalMilliseconds
                            * 0.0009f);

                // We are now walking
                currentType = AnimationType.Walk;
            }
            else
            {
                // If we were walking last frame pick a random idle animation
                if (currentType == AnimationType.Walk)
                {
                    PlayRandomIdle();
                }
            }

            // Update the world matrix
            world.Forward = avatarForward;

            // Normalize the matrix
            world.Right = Vector3.Cross(world.Forward, Vector3.Up);
            world.Right = Vector3.Normalize(world.Right);
            world.Up = Vector3.Cross(world.Right, world.Forward);
            world.Up = Vector3.Normalize(world.Up);

            // Add translation
            world.Translation += translate;

            // Set the avatar renderer world matrix
            if (avatarRenderer != null)
            {
                avatarRenderer.World = world;
            }
        }


        /// <summary>
        /// Move camera based on user input
        /// </summary>
        private void HandleCameraInput()
        {
            // should we reset the camera?
            if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed)
            {
                cameraArc = CameraDefaultArc;
                cameraDistance = CameraDefaultDistance;
                cameraRotation = CameraDefaultRotation;
            }

            // Update Camera
            cameraArc -= currentGamePadState.ThumbSticks.Right.Y * 0.05f;
            cameraRotation += currentGamePadState.ThumbSticks.Right.X * 0.1f;
            cameraDistance += currentGamePadState.Triggers.Left * 0.1f;
            cameraDistance -= currentGamePadState.Triggers.Right * 0.1f;

            // Limit the camera movement
            if (cameraDistance > 5.0f)
                cameraDistance = 5.0f;
            else if (cameraDistance < 2.0f)
                cameraDistance = 2.0f;

            if (cameraArc > MathHelper.Pi / 5)
                cameraArc = MathHelper.Pi / 5;
            else if (cameraArc < -(MathHelper.Pi / 5))
                cameraArc = -(MathHelper.Pi / 5);

            // Update the camera position
            Vector3 cameraPos = new Vector3(0, cameraDistance, cameraDistance);
            cameraPos = Vector3.Transform(cameraPos, Matrix.CreateRotationX(cameraArc));
            cameraPos = Vector3.Transform(cameraPos,
                                          Matrix.CreateRotationY(cameraRotation));

            cameraPos += world.Translation;

            // Create new view matrix
            view = Matrix.CreateLookAt(cameraPos, world.Translation
                                                  + new Vector3(0, 1.2f, 0), Vector3.Up);

            // Set the new view on the avatar renderer
            if (avatarRenderer != null)
            {
                avatarRenderer.View = view;
            }
        }

        #endregion


        #region Animation Selection

        /// <summary>
        /// Start playing a random idle animation
        /// </summary>
        private void PlayRandomIdle()
        {
            PlayAnimation((AnimationType)random.Next((int)AnimationType.Idle4));
        }


        /// <summary>
        /// Start playing one of the other animations that were loaded
        /// </summary>
        private void PlayAnimation(AnimationType animation)
        {
            animations[(int)animation].CurrentPosition = TimeSpan.Zero;
            currentType = animation;
        }

        #endregion


        #region Avatar Loading

        /// <summary>
        /// Load the avatar for a gamer
        /// </summary>
        private void LoadAvatar(Gamer gamer)
        {
            UnloadAvatar();

            AvatarDescription.BeginGetFromGamer(gamer, LoadAvatarDescription, null);
        }


        /// <summary>
        /// AsyncCallback for loading the AvatarDescription
        /// </summary>
        private void LoadAvatarDescription(IAsyncResult result)
        {
            // Get the AvatarDescription for the gamer
            avatarDescription = AvatarDescription.EndGetFromGamer(result);

            // Load the AvatarRenderer if description is valid
            if (avatarDescription.IsValid)
            {
                avatarRenderer = new AvatarRenderer(avatarDescription);
                avatarRenderer.Projection = projection;
            }
            // Load random for an invalid description
            else
            {
                LoadRandomAvatar();
            }
        }


        /// <summary>
        /// Load a random avatar
        /// </summary>
        private void LoadRandomAvatar()
        {
            UnloadAvatar();

            avatarDescription = AvatarDescription.CreateRandom();
            avatarRenderer = new AvatarRenderer(avatarDescription);
            avatarRenderer.Projection = projection;
        }


        /// <summary>
        /// Unloads the current avatar
        /// </summary>
        private void UnloadAvatar()
        {
            // Dispose the current Avatar
            if (avatarRenderer != null)
            {
                avatarRenderer.Dispose();
                avatarRenderer = null;
            }
        }

        #endregion


        #region Entry Point

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (CustomAvatarAnimationSampleGame game =
                                                   new CustomAvatarAnimationSampleGame())
            {
                game.Run();
            }
        }

        #endregion
    }
}
