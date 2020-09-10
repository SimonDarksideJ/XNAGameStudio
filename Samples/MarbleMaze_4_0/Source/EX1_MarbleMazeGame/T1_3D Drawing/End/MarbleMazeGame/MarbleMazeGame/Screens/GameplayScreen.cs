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
#endregion

namespace MarbleMazeGame
{
    class GameplayScreen : GameScreen
    {
        #region Fields
        Maze maze;
        Marble marble;
        Camera camera;
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
