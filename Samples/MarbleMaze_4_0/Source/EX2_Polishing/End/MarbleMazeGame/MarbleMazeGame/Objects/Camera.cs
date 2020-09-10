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
        Vector3 position = Vector3.Zero;
        Vector3 target = Vector3.Zero;
        GraphicsDevice graphicsDevice;

        public Vector3 ObjectToFollow { get; set; }
        public Matrix Projection { get; set; }
        public Matrix View { get; set; }

        readonly Vector3 cameraPositionOffset = new Vector3(0, 450, 100);
        readonly Vector3 cameraTargetOffset = new Vector3(0, 0, -50);
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

            base.Initialize();
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the camera to follow the object it is set to follow.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public override void Update(GameTime gameTime)
        {
            // Make the camera follow the object
            position = ObjectToFollow + cameraPositionOffset;

            target = ObjectToFollow + cameraTargetOffset;

            // Create the view matrix
            View = Matrix.CreateLookAt(position, target, Vector3.Up);

            base.Update(gameTime);
        }
        #endregion Update
    }
}
