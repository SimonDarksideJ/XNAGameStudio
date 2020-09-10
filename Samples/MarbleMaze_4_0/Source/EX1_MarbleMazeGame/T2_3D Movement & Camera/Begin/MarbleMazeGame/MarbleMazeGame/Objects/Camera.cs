#region File Description
//-----------------------------------------------------------------------------
// Camera.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace MarbleMazeGame
{
    public class Camera : GameComponent
    {
        #region Fields
        Vector3 position = new Vector3(0, 1000, 2000);
        Vector3 target = Vector3.Zero;
        GraphicsDevice graphicsDevice;

        public Matrix Projection { get; set; }
        public Matrix View { get; set; }
        #endregion

        #region Initializtion
        public Camera(Game game, GraphicsDevice graphics)
            : base(game)
        {
            this.graphicsDevice = graphics;
        }

        /// <summary>
        /// Initialize the camera
        /// </summary>
        public override void Initialize()
        {
            // Create the projection matrix
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(50), 
                graphicsDevice.Viewport.AspectRatio, 1, 10000);

            // Create the view matrix
            View = Matrix.CreateLookAt(position, target, Vector3.Up);
            base.Initialize();
        }
        #endregion
    }
}
