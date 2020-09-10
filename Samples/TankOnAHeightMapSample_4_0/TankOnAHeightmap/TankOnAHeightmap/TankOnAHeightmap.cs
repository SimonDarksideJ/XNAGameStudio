#region File Description
//-----------------------------------------------------------------------------
// TanksOnAHeightmap.cs
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
#endregion

namespace TanksOnAHeightmap
{
    /// <summary>
    /// Sample showing how to use get the height of a programmatically generated
    /// heightmap.
    /// </summary>
    public class TanksOnAHeightmapGame : Microsoft.Xna.Framework.Game
    {
        #region Constants

        // This vector controls how much the camera's position is offset from the
        // tank. This value can be changed to move the camera further away from or
        // closer to the tank.
        readonly Vector3 CameraPositionOffset = new Vector3(0, 40, 150);

        // This value controls the point the camera will aim at. This value is an offset
        // from the tank's position.
        readonly Vector3 CameraTargetOffset = new Vector3(0, 30, 0);


        #endregion

        #region Fields


        GraphicsDeviceManager graphics;

        Model terrain;
        Tank tank;

        Matrix projectionMatrix;
        Matrix viewMatrix;
        
        HeightMapInfo heightMapInfo;

        #endregion

        #region Initialization


        public TanksOnAHeightmapGame()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";

            tank = new Tank();
        }

        protected override void Initialize()
        {
            // now that the GraphicsDevice has been created, we can create the projection matrix.
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f), GraphicsDevice.Viewport.AspectRatio, 1f, 10000);

            base.Initialize();
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            terrain = Content.Load<Model>("terrain");

            // The terrain processor attached a HeightMapInfo to the terrain model's
            // Tag. We'll save that to a member variable now, and use it to
            // calculate the terrain's heights later.
            heightMapInfo = terrain.Tag as HeightMapInfo;
            if (heightMapInfo == null)
            {
                string message = "The terrain model did not have a HeightMapInfo " +
                    "object attached. Are you sure you are using the " +
                    "TerrainProcessor?";
                throw new InvalidOperationException(message);
            }

            tank.LoadContent(Content);
        }

        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            UpdateCamera();            

            base.Update(gameTime);
        }

        /// <summary>
        /// this function will calculate the camera's position and the position of 
        /// its target. From those, we'll update the viewMatrix.
        /// </summary>
        private void UpdateCamera()
        {
            // The camera's position depends on the tank's facing direction: when the
            // tank turns, the camera needs to stay behind it. So, we'll calculate a
            // rotation matrix using the tank's facing direction, and use it to
            // transform the two offset values that control the camera.
            Matrix cameraFacingMatrix = Matrix.CreateRotationY(tank.FacingDirection);
            Vector3 positionOffset = Vector3.Transform(CameraPositionOffset,
                cameraFacingMatrix);
            Vector3 targetOffset = Vector3.Transform(CameraTargetOffset,
                cameraFacingMatrix);

            // once we've transformed the camera's position offset vector, it's easy to
            // figure out where we think the camera should be.
            Vector3 cameraPosition = tank.Position + positionOffset;

            // We don't want the camera to go beneath the heightmap, so if the camera is
            // over the terrain, we'll move it up.
            if (heightMapInfo.IsOnHeightmap(cameraPosition))
            {
                // we don't want the camera to go beneath the terrain's height +
                // a small offset.
                float minimumHeight;
                Vector3 normal;
                heightMapInfo.GetHeightAndNormal
                    (cameraPosition, out minimumHeight, out normal);

                minimumHeight += CameraPositionOffset.Y;

                if (cameraPosition.Y < minimumHeight)
                {
                    cameraPosition.Y = minimumHeight;
                }
            }

            // next, we need to calculate the point that the camera is aiming it. That's
            // simple enough - the camera is aiming at the tank, and has to take the 
            // targetOffset into account.
            Vector3 cameraTarget = tank.Position + targetOffset;


            // with those values, we'll calculate the viewMatrix.
            viewMatrix = Matrix.CreateLookAt(cameraPosition,
                                              cameraTarget,
                                              Vector3.Up);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.Black);

            DrawModel(terrain);

            tank.Draw(viewMatrix, projectionMatrix);

            // If there was any alpha blended translucent geometry in
            // the scene, that would be drawn here.

            base.Draw(gameTime);
        }


        /// <summary>
        /// Helper for drawing the terrain model.
        /// </summary>
        void DrawModel(Model model)
        {
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index];
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    // Set the fog to match the black background color
                    effect.FogEnabled = true;
                    effect.FogColor = Vector3.Zero;
                    effect.FogStart = 1000;
                    effect.FogEnd = 3200;
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
            KeyboardState currentKeyboardState = Keyboard.GetState();
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            tank.HandleInput(currentGamePadState, currentKeyboardState, heightMapInfo);


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
            using (TanksOnAHeightmapGame game = new TanksOnAHeightmapGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
