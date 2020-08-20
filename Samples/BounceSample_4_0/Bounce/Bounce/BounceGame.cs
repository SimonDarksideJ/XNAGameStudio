#region File Description
//-----------------------------------------------------------------------------
// BounceGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
#endregion


namespace Bounce
{
    /// <summary>
    /// This sample shows how to draw 3D geometric collisions
    /// </summary>
    ///     
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        KeyboardState currentKeyboardState;
        KeyboardState lastKeyboardState;
        GamePadState currentGamePadState;
        GamePadState lastGamePadState;

        // Uur primitive (only a sphere is currently supported)
        GeometricPrimitive primitive;

        // Store a list of spheres with unique characteristics (position, mass, etc.)
        List<Sphere> spheres = new List<Sphere>();

        // The extents of our world (it's a cube)
        const float worldSize = 3.00f;
        const float floorPlaneHeight = -1.0f;

        // Define how many spheres are initially spawned
        const float numSpheres = 100;

        // dampen velocity each time a collision occurs
        const float collisionDamping = 0.75f;

        // history of accellerator values so we can detect shakes
        float[] accelhistory = new float[2];

        // Store a list of tint sphereColors
        Color[] sphereColors = new Color[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.White,
            Color.Black,
        };

        #endregion

        #region Initialization


        public Game1()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = true;

            TargetElapsedTime = TimeSpan.FromTicks(333333);
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Initialize the Acelerometer wrapper
            Accelerometer.Initialize();

            primitive = new SpherePrimitive(GraphicsDevice);

            Random random = new Random();
            int numsphereColors = sphereColors.Length;

            float xpos = -10.0f, zpos = -2.0f, ypos = floorPlaneHeight;
            for (int i = 0; i < numSpheres; i++)
            {
                Sphere newSphere = new Sphere();
                // Generate a a randomized velocity impulse
                newSphere.Velocity.X = 0.2f * random.Next(-10, 10);
                newSphere.Velocity.Z = 0.2f * random.Next(-10, 10);
                newSphere.Velocity.Y = 0.2f * random.Next(-3, 3);

                // Generate a random color
                newSphere.Color = sphereColors[i % numsphereColors];

                // first 2 spheres are large, rest are small.
                newSphere.Radius = 0.10f + ((float)(random.Next(100)) / 100.0f) * 0.15f;

                // Set initial position
                newSphere.Position.X = xpos;
                newSphere.Position.Y = ypos + newSphere.Radius * 6.0f;
                newSphere.Position.Z = zpos;

                // Set mass based on size (sphere volume function)
                newSphere.Mass = (float)(Math.PI) * (newSphere.Radius * newSphere.Radius * newSphere.Radius);

                spheres.Add(newSphere);

                // Set new position
                xpos += 1.5f;
                if (xpos > 20.0f)
                {
                    xpos = -10.0f;
                    zpos -= 1.5f;
                }
            }
        }


        #endregion

        #region Update and Draw

        /// <summary>
        // Given 2 spheres with velocity, mass and size, evaluate whether
        // a collision occured, and if so, excatly where, and move sphere 2
        // at the contact point with sphere 1, and generaet new velocities.
        /// </summary>
        private void SphereCollisionImplicit(Sphere sphere1, Sphere sphere2)
        {
            const float K_ELASTIC = 0.75f;

            Vector3 relativepos = sphere2.Position - sphere1.Position;
            float distance2 = relativepos.LengthSquared();
            float radii = sphere1.Radius + sphere2.Radius;
            if (distance2 >= radii * radii)
                return; // No collision

            float distance = relativepos.Length();
            Vector3 relativeUnit = relativepos * (1.0f / distance);
            Vector3 penetration = relativeUnit * (radii - distance);

            // Adjust the spheres' relative positions
            float mass1 = sphere1.Mass;
            float mass2 = sphere2.Mass;

            float m_inv = 1.0f / (mass1 + mass2);
            float weight1 = mass1 * m_inv; // relative weight of sphere 1
            float weight2 = mass2 * m_inv; // relative weight of sphere 2. w1+w2==1.0

            sphere1.Position -= weight2 * penetration;
            sphere2.Position += weight1 * penetration;

            // Adjust the objects’ relative velocities, if they are
            // moving toward each other.
            //
            // Note that we're assuming no friction, or equivalently, no angular momentum.
            //
            // velocityTotal = velocity of v2 in v1 stationary ref. frame
            // get reference frame of common center of mass
            Vector3 velocity1 = sphere1.Velocity;
            Vector3 velocity2 = sphere2.Velocity;

            Vector3 velocityTotal = velocity1 * weight1 + velocity2 * weight2;
            Vector3 i2 = (velocity2 - velocityTotal) * mass2;
            if (Vector3.Dot(i2, relativeUnit) < 0)
            {
                // i1+i2 == 0, approx
                Vector3 di = Vector3.Dot(i2, relativeUnit) * relativeUnit;
                i2 -= di * (K_ELASTIC + 1);
                sphere1.Velocity = (-i2) / mass1 + velocityTotal;
                sphere2.Velocity = i2 / mass2 + velocityTotal;
            }
        }

        /// <summary>        
        /// Evalute sphere physics, generate new velocities based on collisions, and update
        /// sphere positions
        /// </summary>
        protected void UpdateSpheres(GameTime gameTime)
        {
            Vector3 gravity = Vector3.UnitY * -4.0f;
            float shakeForce = 1.0f;

            // Tilt limit
            const float limit = 0.85f;

            // Tilt offset that determines where 'flat'
            const float tiltoffset = 0.76f;

            double accelReadingZ;
            double accelReadingY;

            float elapsedGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Read the current state of the accelerometer
            Vector3 currentAccelerometerReading = Accelerometer.GetState().Acceleration;

            // We need to reverse the accelerometer reading for the y axis depending on orientation            
            if (Window.CurrentOrientation == DisplayOrientation.LandscapeLeft)
            {
                currentAccelerometerReading.Y = -currentAccelerometerReading.Y;
            }

            // Compute accelerometer magnitude
            Vector3 accelMag = Vector3.Zero;
            accelMag = currentAccelerometerReading;
            float newMag = accelMag.Length();

            // Detect a shake gesture. Search for a peak that is > 1.3f in magnitude.
            if (accelhistory[1] > 1.3f && accelhistory[0] < accelhistory[1] && accelhistory[1] > newMag)
            {
                // Compute shake force
                shakeForce += 10.0f * (accelhistory[1] - 1.3f) / 3.5f;
            }

            // MRU cache for accelerometer magnitudes
            accelhistory[0] = accelhistory[1];
            accelhistory[1] = newMag;

            // Read accelerometer Z and Y, which will be used for modifying gravity
            accelReadingZ = currentAccelerometerReading.Z;
            accelReadingY = currentAccelerometerReading.Y;

            // Limit the rotation based on accelerometer
            float rotateX = Math.Max(Math.Min((float)-(accelReadingZ + tiltoffset), limit), -limit);
            float rotateY = Math.Max(Math.Min((float)accelReadingY, limit), -limit);

            // Rotate gravity vector based on accelerometer input
            Matrix rotationToBeDone = Matrix.CreateRotationX(rotateX * MathHelper.PiOver2);
            Matrix rotationToBeDone2 = Matrix.CreateRotationZ(rotateY * MathHelper.PiOver2);

            gravity = Vector3.Transform(gravity, rotationToBeDone);
            gravity = Vector3.Transform(gravity, rotationToBeDone2);

            // Update positions, add air friction and gravity
            for (int i = 0; i < numSpheres; i++)
            {
                Sphere mySphere = spheres[i];
                mySphere.Position += mySphere.Velocity * elapsedGameTime * 0.99f;
                // Apply accelleration due to gravity
                mySphere.Velocity += gravity * elapsedGameTime;
            }

            // Resolve sphere-sphere collisions
            for (int i = 0; i < numSpheres; i++)
            {
                for (int j = i + 1; j < numSpheres; j++)
                {
                    SphereCollisionImplicit(spheres[i], spheres[j]);
                }
            }

            // Resolve collisions with floor
            for (int i = 0; i < numSpheres; i++)
            {
                Sphere mySphere = spheres[i];
                if (mySphere.Position.Y < floorPlaneHeight + mySphere.Radius)
                {
                    // subtract out pre-accelerated gravity first
                    mySphere.Velocity -= gravity * elapsedGameTime;

                    // apply shake force
                    if (shakeForce > 1.0f)
                    {
                        // Add some upward momentum
                        mySphere.Velocity.Y += 0.1f;

                        // Compute current speed
                        float speed = mySphere.Velocity.Length();

                        // Limit new speed
                        float speedadjust = speed;
                        speedadjust = Math.Min(speed, 4.0f);
                        speedadjust = Math.Max(speed, 2.0f);

                        // normalize
                        mySphere.Velocity *= 1.0f / speed;

                        // accellerate based on shake force
                        mySphere.Velocity *= speedadjust * shakeForce;
                    }


                    mySphere.Position.Y = floorPlaneHeight + mySphere.Radius;
                    if (mySphere.Velocity.Y < 0)
                    {
                        // Determine "complete rest"
                        if (mySphere.Velocity.Y > (gravity.Y * elapsedGameTime * 2.0f) &&
                            mySphere.Velocity.LengthSquared() < (0.5 * 0.5))
                        {
                            mySphere.Velocity.Y = 0.0f;
                        }
                        else // Otherwise, bounce.
                            mySphere.Velocity.Y = -mySphere.Velocity.Y * collisionDamping;

                    }
                }

                // Resolves collisions with walls
                if (mySphere.Position.X < -worldSize + mySphere.Radius)
                {
                    mySphere.Position.X = -worldSize + mySphere.Radius;
                    if (mySphere.Velocity.X < 0)
                        mySphere.Velocity.X = -mySphere.Velocity.X * collisionDamping;
                }

                if (mySphere.Position.X > worldSize - mySphere.Radius)
                {
                    mySphere.Position.X = worldSize - mySphere.Radius;
                    if (mySphere.Velocity.X > 0)
                        mySphere.Velocity.X = -mySphere.Velocity.X * collisionDamping;
                }

                if (mySphere.Position.Z < -worldSize + mySphere.Radius)
                {
                    mySphere.Position.Z = -worldSize + mySphere.Radius;
                    if (mySphere.Velocity.Z < 0)
                        mySphere.Velocity.Z = -mySphere.Velocity.Z * collisionDamping;
                }

                if (mySphere.Position.Z > worldSize - mySphere.Radius)
                {
                    mySphere.Position.Z = worldSize - mySphere.Radius;
                    if (mySphere.Velocity.Z > 0)
                        mySphere.Velocity.Z = -mySphere.Velocity.Z * collisionDamping;
                }
            }
        }


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            UpdateSpheres(gameTime);

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Create camera matrices, making the object spin.
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            float yaw = time * 0.4f;
            float pitch = time * 0.7f;
            float roll = time * 1.1f;

            Vector3 cameraLookat = new Vector3(0.0f, 0.0f, 2.5f);

            float aspect = GraphicsDevice.Viewport.AspectRatio;

            Matrix world = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            Matrix view = Matrix.CreateLookAt(cameraLookat, Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 100);

            Matrix worldTranslation = Matrix.CreateTranslation(0.0f, 0.0f, -5.0f);
            world *= worldTranslation;

            // Draw the current primitive.
            GeometricPrimitive currentPrimitive = primitive;

            // Since we know where the ground plane is, generate a very simple but correct shadow
            // by squashing the geometry into the XZ plane and rendering it at the ground plane directly
            // below the sphere
            Matrix shadowMatrix = Matrix.Identity;
            shadowMatrix.M12 = 0.0f;
            shadowMatrix.M22 = 0.0f;
            shadowMatrix.M23 = 0.0f;

            Matrix matScale;
            for (int i = 0; i < numSpheres; i++)
            {
                // Generate a scale matrix
                matScale = Matrix.CreateScale(spheres[i].Radius / 0.5f);
                Matrix worldX = world * matScale;

                // Translate
                worldTranslation = Matrix.CreateTranslation(spheres[i].Position);
                worldX *= worldTranslation;

                currentPrimitive.Draw(worldX, view, projection, spheres[i].Color, false);

                // Render shadow
                worldX *= shadowMatrix;
                worldX.M42 = -1.0f;
                currentPrimitive.Draw(worldX, view, projection, Color.Black, true);
            }

            base.Draw(gameTime);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting or changing settings.
        /// </summary>
        void HandleInput()
        {
            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (IsPressed(Keys.Escape, Buttons.Back))
            {
                Exit();
            }
        }


        /// <summary>
        /// Checks whether the specified key or button has been pressed.
        /// </summary>
        bool IsPressed(Keys key, Buttons button)
        {
            return (currentKeyboardState.IsKeyDown(key) &&
                    lastKeyboardState.IsKeyUp(key)) ||
                   (currentGamePadState.IsButtonDown(button) &&
                    lastGamePadState.IsButtonUp(button));
        }


        #endregion
    }
}