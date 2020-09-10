#region File Description
//-----------------------------------------------------------------------------
// HeightmapCollision.cs
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

namespace HeightmapCollision
{
    /// <summary>
    /// Sample showing how to use get the height of a programmatically generated
    /// heightmap.
    /// </summary>
    public class HeightmapCollisionGame : Microsoft.Xna.Framework.Game
    {
        #region Constants

        // This constant controls how quickly the sphere can move forward and backward
        const float SphereVelocity = 2;

        // how quickly the sphere can turn from side to side
        const float SphereTurnSpeed = .025f;

        // the radius of the sphere. We'll use this to keep the sphere above the ground,
        // and when computing how far the sphere has rolled.
        const float SphereRadius = 12.0f;

        // This vector controls how much the camera's position is offset from the
        // sphere. This value can be changed to move the camera further away from or
        // closer to the sphere.
        readonly Vector3 CameraPositionOffset = new Vector3(0, 40, 150);

        // This value controls the point the camera will aim at. This value is an offset
        // from the sphere's position.
        readonly Vector3 CameraTargetOffset = new Vector3(0, 30, 0);


        #endregion

        #region Fields


        GraphicsDeviceManager graphics;

        Model terrain;

        Matrix projectionMatrix;
        Matrix viewMatrix;

        Vector3 spherePosition;
        float sphereFacingDirection;
        Matrix sphereRollingMatrix = Matrix.Identity;

        Model sphere;
        HeightMapInfo heightMapInfo;


        #endregion

        #region Initialization


        public HeightmapCollisionGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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

            sphere = Content.Load<Model>("sphere");
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
            // The camera's position depends on the sphere's facing direction: when the
            // sphere turns, the camera needs to stay behind it. So, we'll calculate a
            // rotation matrix using the sphere's facing direction, and use it to
            // transform the two offset values that control the camera.
            Matrix cameraFacingMatrix = Matrix.CreateRotationY(sphereFacingDirection);
            Vector3 positionOffset = Vector3.Transform(CameraPositionOffset,
                cameraFacingMatrix);
            Vector3 targetOffset = Vector3.Transform(CameraTargetOffset,
                cameraFacingMatrix);

            // once we've transformed the camera's position offset vector, it's easy to
            // figure out where we think the camera should be.
            Vector3 cameraPosition = spherePosition + positionOffset;

            // We don't want the camera to go beneath the heightmap, so if the camera is
            // over the terrain, we'll move it up.
            if (heightMapInfo.IsOnHeightmap(cameraPosition))
            {
                // we don't want the camera to go beneath the terrain's height +
                // a small offset.
                float minimumHeight =
                    heightMapInfo.GetHeight(cameraPosition) + CameraPositionOffset.Y;

                if (cameraPosition.Y < minimumHeight)
                {
                    cameraPosition.Y = minimumHeight;
                }
            }

            // next, we need to calculate the point that the camera is aiming it. That's
            // simple enough - the camera is aiming at the sphere, and has to take the 
            // targetOffset into account.
            Vector3 cameraTarget = spherePosition + targetOffset;


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

            DrawModel(terrain, Matrix.Identity);

            DrawModel(sphere, sphereRollingMatrix *
                Matrix.CreateTranslation(spherePosition));

            // If there was any alpha blended translucent geometry in
            // the scene, that would be drawn here.

            base.Draw(gameTime);
        }


        /// <summary>
        /// Helper for drawing the terrain model.
        /// </summary>
        void DrawModel(Model model, Matrix worldMatrix)
        {
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * worldMatrix;
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

            // Now move the sphere. First, we want to check to see if the sphere should
            // turn. turnAmount will be an accumulation of all the different possible
            // inputs.
            float turnAmount = -currentGamePadState.ThumbSticks.Left.X;
            if (currentKeyboardState.IsKeyDown(Keys.A) ||
                currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                turnAmount += 1;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D) ||
                currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                turnAmount -= 1;
            }

            // clamp the turn amount between -1 and 1, and then use the finished
            // value to turn the sphere.
            turnAmount = MathHelper.Clamp(turnAmount, -1, 1);
            sphereFacingDirection += turnAmount * SphereTurnSpeed;


            // Next, we want to move the sphere forward or back. to do this, 
            // we'll create a Vector3 and modify use the user's input to modify the Z
            // component, which corresponds to the forward direction.
            Vector3 movement = Vector3.Zero;
            movement.Z = -currentGamePadState.ThumbSticks.Left.Y;

            if (currentKeyboardState.IsKeyDown(Keys.W) ||
                currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                movement.Z = -1;
            }
            if (currentKeyboardState.IsKeyDown(Keys.S) ||
                currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                movement.Z = 1;
            }

            // next, we'll create a rotation matrix from the sphereFacingDirection, and
            // use it to transform the vector. If we didn't do this, pressing "up" would
            // always move the ball along +Z. By transforming it, we can move in the
            // direction the sphere is "facing."
            Matrix sphereFacingMatrix = Matrix.CreateRotationY(sphereFacingDirection);
            Vector3 velocity = Vector3.Transform(movement, sphereFacingMatrix);
            velocity *= SphereVelocity;

            // Now we know how much the user wants to move. We'll construct a temporary
            // vector, newSpherePosition, which will represent where the user wants to
            // go. If that value is on the heightmap, we'll allow the move.
            Vector3 newSpherePosition = spherePosition + velocity;
            if (heightMapInfo.IsOnHeightmap(newSpherePosition))
            {
                // finally, we need to see how high the terrain is at the sphere's new
                // position. GetHeight will give us that information, which is offset by
                // the size of the sphere. If we didn't offset by the size of the
                // sphere, it would be drawn halfway through the world, which looks 
                // a little odd.
                newSpherePosition.Y = heightMapInfo.GetHeight(newSpherePosition) +
                    SphereRadius;
            }
            else
            {
                newSpherePosition = spherePosition;
            }

            // now we need to roll the ball "forward." to do this, we first calculate
            // how far it has moved.
            float distanceMoved = Vector3.Distance(spherePosition, newSpherePosition);

            // The length of an arc on a circle or sphere is defined as L = theta * r,
            // where theta is the angle that defines the arc, and r is the radius of
            // the circle.
            // we know L, that's the distance the sphere has moved. we know r, that's
            // our constant "sphereRadius". We want to know theta - that will tell us
            // how much to rotate the sphere. we rearrange the equation to get...
            float theta = distanceMoved / SphereRadius;

            // now that we know how much to rotate the sphere, we have to figure out 
            // whether it will roll forward or backward. We'll base this on the user's
            // input.
            int rollDirection = movement.Z > 0 ? 1 : -1;

            // finally, we'll roll it by rotating around the sphere's "right" vector.
            sphereRollingMatrix *= Matrix.CreateFromAxisAngle(sphereFacingMatrix.Right,
                theta * rollDirection);

            // once we've finished all computations, we can set spherePosition to the
            // new position that we calculated.
            spherePosition = newSpherePosition;
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
            using (HeightmapCollisionGame game = new HeightmapCollisionGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
