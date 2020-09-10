//-----------------------------------------------------------------------------
// TiltPerspectiveSample.cs
//
// Demonstrates using the accelerometer to estimate the device orientation
// relative to the user, then rendering with a perspective matrix that
// compensates for the user's eye position.
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;


namespace TiltPerspectiveSample
{
    public class ParallaxSample : Microsoft.Xna.Framework.Game
    {
        #region Fields
        // distance from eye to screen, in pixel units
        const float eyeDistance = 2000;
        
        // distance from eye to near clip plane, as a fraction of the Z distance to the screen. We do it this way
        // because we can end up with some highly skewed projection matrices if the devices is tilted enough.
        const float nearPlane = 0.5f;

        // distance from the eye to the far clip plane, in pixel units
        const float farPlaneDistance = 4000;

        // size of the box that everything takes place in, measured in pixel units
        BoundingBox worldBox = new BoundingBox
        {
            Min = new Vector3(-400, -400, -400),
            Max = new Vector3(400, 400, 0)
        };

        // "down" direction (smoothed accelerometer reading) to use as our reference position. The user
        // can reset this by touching the screen.
        Vector3 referenceDown = -Vector3.UnitZ;

        GraphicsDeviceManager graphics;
        AccelerometerHelper accelerometer;
        DebugDraw worldGeometry;
        BallSimulation ballSimulation;

        Texture2D boxTexture;
        #endregion

        #region Initialization
        public ParallaxSample()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);

            // Request portrait mode
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            // Turn off all phone UI
            graphics.IsFullScreen = true;
            Guide.IsScreenSaverEnabled = false;

            // Frame rate is 30 fps by default
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            accelerometer = new AccelerometerHelper(this);
            Components.Add(accelerometer);

            ballSimulation = new BallSimulation(this);
            ballSimulation.AddWalls(worldBox);
            ballSimulation.AddBalls(25, 25.0f, 75.0f, worldBox);
            Components.Add(ballSimulation);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            boxTexture = Content.Load<Texture2D>("stone4");

            worldGeometry = DebugDraw.CreateBoxInterior(GraphicsDevice, worldBox);
        }
        #endregion

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            accelerometer.Update(gameTime);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (TouchPanel.GetState().Count > 0)
            {
                referenceDown = Vector3.Normalize(accelerometer.SmoothAcceleration);
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            const float zThreshold = 0.4f;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Make the light always come from the actual ceiling
            Vector3 lightDirection = Vector3.Normalize(accelerometer.SmoothAcceleration);
            Vector3 eyeDirection = ComputeEyeVector();

            if (eyeDirection.Z < zThreshold)
            {
                // Limit how far we distort the perspective
                eyeDirection.Z = zThreshold;
                eyeDirection.Normalize();
            }
            Vector3 eyePosition = eyeDirection * eyeDistance;

            Matrix world = Matrix.Identity;

            Matrix view = Matrix.CreateLookAt(
                new Vector3(eyePosition.X, eyePosition.Y, eyePosition.Z),
                new Vector3(eyePosition.X, eyePosition.Y, 0),
                new Vector3(0, 1, 0)
            );

            Matrix projection = Matrix.CreatePerspectiveOffCenter(
                (-eyePosition.X - graphics.GraphicsDevice.Viewport.Width*.5f) * nearPlane,
                (-eyePosition.X + graphics.GraphicsDevice.Viewport.Width * .5f) * nearPlane,
                (-eyePosition.Y - graphics.GraphicsDevice.Viewport.Height * .5f) * nearPlane,
                (-eyePosition.Y + graphics.GraphicsDevice.Viewport.Height * .5f) * nearPlane,
                eyePosition.Z * nearPlane,
                farPlaneDistance);

            worldGeometry.BasicEffect.DirectionalLight0.Direction = lightDirection;
            worldGeometry.Draw(ref world, ref view, ref projection, boxTexture);

            ballSimulation.Draw(view, projection, lightDirection);
            base.Draw(gameTime);
        }

        // Compute (guess) the user's eye direction, given a reference 'down'
        // direction and the current 'down' direction.
        //
        // 'world' vectors in this function refer to the actual real-world
        // coords from the (estimated) user's perspective.
        // We don't really know where the user is relative to the screen,
        // so we have to make an assumption about how they hold and tilt
        // the device.
        Vector3 ComputeEyeVector()
        {
            float referencePitch = (float)Math.Asin(referenceDown.Y);
            float rollEpsilon = .1f;

            Vector3 worldDown = -accelerometer.SmoothAcceleration;
            worldDown.Normalize();

            Vector3 worldRight = Vector3.Cross(Vector3.UnitY, worldDown);

            if (worldRight.LengthSquared() < rollEpsilon)
            {
                // The device is held nearly vertically, so the worldRight vector isn't well-defined
                // (its length is close to zero). Just use our local right vector as the world
                // right vector, which means we generate an orientation with no roll.
                worldRight = Vector3.Right;
            }
            else
            {
                // We have a good 'right' vector; normalize it for CreateFromAxisAngle() below.
                worldRight.Normalize();
            }
            Quaternion rot = Quaternion.CreateFromAxisAngle(worldRight, -referencePitch);
            return Vector3.Transform(worldDown, rot);
        }
    }
}
