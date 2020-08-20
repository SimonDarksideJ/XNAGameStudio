#region File Description
//-----------------------------------------------------------------------------
// AvatarAnimationBlendingSample.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace AvatarAnimationBlendingSample
{
    /// <summary>
    /// This sample demonstrates how to blend multiple avatar animations
    /// </summary>
    public class AvatarAnimationBlendingGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;
        SpriteFont font;

        // Description, renderer, and animation for
        // loading and drawing an animated avatar
        AvatarDescription avatarDescription;
        AvatarRenderer avatarRenderer;
        AvatarBlendedAnimation blendedAnimation;
        AvatarAnimation[] avatarAnimations;
        AvatarAnimation noBlendingAnimation;

        // Should we use blending?
        bool useAnimationBlending = true;

        // matrices used to draw the avatar
        Matrix world;
        Matrix view;
        Matrix projection;

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
        public AvatarAnimationBlendingGame()
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

            // Load 4 of the preset animations
            avatarAnimations = new AvatarAnimation[4];
            avatarAnimations[0] = new AvatarAnimation(AvatarAnimationPreset.Stand0);
            avatarAnimations[1] = new AvatarAnimation(AvatarAnimationPreset.Celebrate);
            avatarAnimations[2] = new AvatarAnimation(AvatarAnimationPreset.Clap);
            avatarAnimations[3] = new AvatarAnimation(AvatarAnimationPreset.Wave);

            // Create new blended animation
            blendedAnimation = new AvatarBlendedAnimation(avatarAnimations[0]);

            noBlendingAnimation = avatarAnimations[0];

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
                if (useAnimationBlending)
                {
                    blendedAnimation.Update(gameTime.ElapsedGameTime, true);
                }
                else
                {
                    noBlendingAnimation.Update(gameTime.ElapsedGameTime, true);
                }
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            string message = "Blending: " + (useAnimationBlending ? "On" : "Off") +
                             "\n(Left Shoulder Button)";

            // Draw the text
            spriteBatch.Begin();
            spriteBatch.DrawString(font, message, new Vector2(161, 161), Color.Black);
            spriteBatch.DrawString(font, message, new Vector2(160, 160), Color.White);
            spriteBatch.End();

            // Draw the avatar
            if (useAnimationBlending)
                avatarRenderer.Draw(blendedAnimation);
            else
                avatarRenderer.Draw(noBlendingAnimation);

            base.Draw(gameTime);
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

            // Check to see if we need to change blending bool
            if (currentGamePadState.Buttons.LeftShoulder == ButtonState.Pressed &&
                lastGamePadState.Buttons.LeftShoulder != ButtonState.Pressed)
            {
                useAnimationBlending = !useAnimationBlending;
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
                blendedAnimation.Play(avatarAnimations[1]);
                noBlendingAnimation = avatarAnimations[1];
            }
            else if (currentGamePadState.Buttons.B == ButtonState.Pressed &&
                lastGamePadState.Buttons.B != ButtonState.Pressed)
            {
                blendedAnimation.Play(avatarAnimations[2]);
                noBlendingAnimation = avatarAnimations[2];
            }
            else if (currentGamePadState.Buttons.X == ButtonState.Pressed &&
                lastGamePadState.Buttons.X != ButtonState.Pressed)
            {
                blendedAnimation.Play(avatarAnimations[0]);
                noBlendingAnimation = avatarAnimations[0];
            }
            else if (currentGamePadState.Buttons.Y == ButtonState.Pressed &&
                lastGamePadState.Buttons.Y != ButtonState.Pressed)
            {
                blendedAnimation.Play(avatarAnimations[3]);
                noBlendingAnimation = avatarAnimations[3];
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
            using (AvatarAnimationBlendingGame game = new AvatarAnimationBlendingGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}