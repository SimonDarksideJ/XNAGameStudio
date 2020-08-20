#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Devices;
#endregion

namespace MarbleMazeGame
{
    class GameplayScreen : GameScreen
    {
        #region Fields
        Maze maze;
        Marble marble;
        Camera camera;

        readonly float angularVelocity = MathHelper.ToRadians(1.5f);
        #endregion

        #region Initializations
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.0);
        }
        #endregion

        #region Loading
        /// <summary>
        /// Load the game content
        /// </summary>
        public override void LoadContent()
        {
            LoadAssets();

            Accelerometer.Initialize();

            base.LoadContent();
        }

        /// <summary>
        /// Load all assets for the game
        /// </summary>
        public void LoadAssets()
        {
            InitializeCamera();
            InitializeMaze();
            InitializeMarble();
        }

        /// <summary>
        /// Initialize the camera
        /// </summary>
        private void InitializeCamera()
        {
            // Create the camera
            camera = new Camera(ScreenManager.Game, ScreenManager.GraphicsDevice);
            camera.Initialize();
        }

        /// <summary>
        /// Initialize maze
        /// </summary>
        private void InitializeMaze()
        {
            maze = new Maze(ScreenManager.Game)
            {
                Position = Vector3.Zero,
                Camera = camera
            };

            maze.Initialize();
        }

        /// <summary>
        /// Initialize the marble
        /// </summary>
        private void InitializeMarble()
        {
            marble = new Marble(ScreenManager.Game)
            {
                Position = Vector3.Zero,
                Camera = camera,
            };

            marble.Initialize();
        }
        #endregion

        #region Update
        /// <summary>
        /// Handle all the input.
        /// </summary>
        /// <param name="input"></param>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Rotate the maze according to accelerometer data
            Vector3 currentAccelerometerState = Accelerometer.GetState().Acceleration;

            if (Microsoft.Devices.Environment.DeviceType == DeviceType.Device)
            {
                //Change the velocity according to acceleration reading
                maze.Rotation.Z = (float)Math.Round(MathHelper.ToRadians(currentAccelerometerState.Y * 30), 2);
                maze.Rotation.X = -(float)Math.Round(MathHelper.ToRadians(currentAccelerometerState.X * 30), 2);
            }
            else if (Microsoft.Devices.Environment.DeviceType == DeviceType.Emulator)
            {
                Vector3 Rotation = Vector3.Zero;

                if (currentAccelerometerState.X != 0)
                {
                    if (currentAccelerometerState.X > 0)
                        Rotation += new Vector3(0, 0, -angularVelocity);
                    else
                        Rotation += new Vector3(0, 0, angularVelocity);
                }

                if (currentAccelerometerState.Y != 0)
                {
                    if (currentAccelerometerState.Y > 0)
                        Rotation += new Vector3(-angularVelocity, 0, 0);
                    else
                        Rotation += new Vector3(angularVelocity, 0, 0);
                }

                // Limit the rotation of the maze to 30 degrees
                maze.Rotation.X =
                    MathHelper.Clamp(maze.Rotation.X + Rotation.X,
                    MathHelper.ToRadians(-30), MathHelper.ToRadians(30));

                maze.Rotation.Z =
                    MathHelper.Clamp(maze.Rotation.Z + Rotation.Z,
                    MathHelper.ToRadians(-30), MathHelper.ToRadians(30));

            }
        }

        /// <summary>
        /// Update all the game component
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // Update all the component of the game
            maze.Update(gameTime);
            marble.Update(gameTime);
            camera.Update(gameTime);
        }
        #endregion

        #region Render
        /// <summary>
        /// Draw all the game component
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.Black);
            ScreenManager.SpriteBatch.Begin();

            // Drawing sprites changes some render states around, which don't play
            // nicely with 3d models. 
            // In particular, we need to enable the depth buffer.
            DepthStencilState depthStensilState =
                new DepthStencilState() { DepthBufferEnable = true };
            ScreenManager.GraphicsDevice.DepthStencilState = depthStensilState;

            // Draw all the game components
            maze.Draw(gameTime);
            marble.Draw(gameTime);

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }
        #endregion
    }
}
