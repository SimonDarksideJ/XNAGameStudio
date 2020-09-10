//-----------------------------------------------------------------------------
// CollisionSample.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace CollisionSample
{
    /// <summary>
    /// This sample demonstrates various forms of collision detection for primitives
    /// supplied in the framework, plus oriented bounding boxes and triangles.
    /// </summary>
    public class CollisionSample : Microsoft.Xna.Framework.Game
    {
        #region Constants

        public const int FrustumGroupIndex = 0;
        public const int AABoxGroupIndex = 1;
        public const int OBoxGroupIndex = 2;
        public const int SphereGroupIndex = 3;
        public const int RayGroupIndex = 4;
        public const int NumGroups = 5;

        public const int TriIndex = 0;
        public const int SphereIndex = 1;
        public const int AABoxIndex = 2;
        public const int OBoxIndex = 3;
        public const int NumSecondaryShapes = 4;

        public const float CAMERA_SPACING = 50.0F;

        public const float YAW_RATE = 1;            // radians per second for keyboard controls
        public const float PITCH_RATE = 0.75f;      // radians per second for keyboard controls
        public const float YAW_DRAG_RATE = .01f;     // radians per pixel for drag control
        public const float PITCH_DRAG_RATE = .01f;   // radians per pixel for drag control
        public const float PINCH_ZOOM_RATE = .01f;  // scale factor for pinch-zoom rate
        public const float DISTANCE_RATE = 10;

        #endregion

        #region Fields

        // Rendering helpers
        GraphicsDeviceManager graphics;
        DebugDraw debugDraw;

        // Primary shapes 
        BoundingFrustum primaryFrustum;
        BoundingBox primaryAABox;
        BoundingOrientedBox primaryOBox;
        BoundingSphere primarySphere;
        Ray primaryRay;

        // Secondary shapes.
        Triangle[] secondaryTris = new Triangle[NumGroups];
        BoundingSphere[] secondarySpheres = new BoundingSphere[NumGroups];
        BoundingBox[] secondaryAABoxes = new BoundingBox[NumGroups];
        BoundingOrientedBox[] secondaryOBoxes = new BoundingOrientedBox[NumGroups];

        // Collision results
        ContainmentType[,] collideResults = new ContainmentType[NumGroups, NumSecondaryShapes];
        Vector3? rayHitResult;

        // Camera state
        Vector3[] cameraOrigins = new Vector3[NumGroups];
        int currentCamera;

        bool cameraOrtho;
        float cameraYaw;
        float cameraPitch;
        float cameraDistance;
        Vector3 cameraTarget;

        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        List<GestureSample> currentGestures = new List<GestureSample>();

        KeyboardState previousKeyboardState = new KeyboardState();
        GamePadState previousGamePadState = new GamePadState();

        TimeSpan unpausedClock = new TimeSpan();
        bool paused;

        #endregion

        #region Initialization

        public CollisionSample()
        {
            graphics = new GraphicsDeviceManager(this);
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.Pinch | GestureType.FreeDrag;
#if WINDOWS_PHONE
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;
#endif
#if WINDOWS || XBOX
            graphics.PreferredBackBufferWidth = 853;
            graphics.PreferredBackBufferHeight = 480;
#endif
        }

        // Set up initial bounding shapes for the primary (static) and secondary (moving)
        // bounding shapes along with relevant camera position information.
        protected override void Initialize()
        {
            Console.WriteLine("DEBUG - Game Initialize!");

            debugDraw = new DebugDraw(GraphicsDevice);

            Components.Add(new FrameRateCounter(this));

            // Primary frustum
            Matrix m1 = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1.77778F, 0.5f, 10.0f);
            Matrix m2 = Matrix.CreateTranslation(new Vector3(0, 0, -7));
            primaryFrustum = new BoundingFrustum(Matrix.Multiply(m2, m1));
            cameraOrigins[FrustumGroupIndex] = Vector3.Zero;

            // Primary axis-aligned box
            primaryAABox.Min = new Vector3(CAMERA_SPACING - 3, -4, -5);
            primaryAABox.Max = new Vector3(CAMERA_SPACING + 3, 4, 5);
            cameraOrigins[AABoxGroupIndex] = new Vector3(CAMERA_SPACING, 0, 0);

            // Primary oriented box
            primaryOBox.Center = new Vector3(-CAMERA_SPACING, 0, 0);
            primaryOBox.HalfExtent = new Vector3(3, 4, 5);
            primaryOBox.Orientation = Quaternion.CreateFromYawPitchRoll(0.8f, 0.7f, 0);
            cameraOrigins[OBoxGroupIndex] = primaryOBox.Center;

            // Primary sphere
            primarySphere.Center = new Vector3(0, 0, -CAMERA_SPACING);
            primarySphere.Radius = 5;
            cameraOrigins[SphereGroupIndex] = primarySphere.Center;

            // Primary ray
            primaryRay.Position = new Vector3(0, 0, CAMERA_SPACING);
            primaryRay.Direction = Vector3.UnitZ;
            cameraOrigins[RayGroupIndex] = primaryRay.Position;

            // Initialize all of the secondary objects with default values
            Vector3 half = new Vector3(0.5F, 0.5F, 0.5F);
            for (int i = 0; i < NumGroups; i++)
            {
                secondarySpheres[i] = new BoundingSphere(Vector3.Zero, 1.0f);
                secondaryOBoxes[i] = new BoundingOrientedBox(Vector3.Zero, half, Quaternion.Identity);
                secondaryAABoxes[i] = new BoundingBox(-half, half);
                secondaryTris[i] = new Triangle();
            }

            rayHitResult = null;

            currentCamera = 3;
            cameraOrtho = false;
            cameraYaw = (float)Math.PI * 0.75F;
            cameraPitch = MathHelper.PiOver4;
            cameraDistance = 20;
            cameraTarget = cameraOrigins[0];

            paused = false;
            
            base.Initialize();
        }
        #endregion

        #region Update

        protected override void Update(GameTime gameTime)
        {
            ReadInputDevices();

            if (currentKeyboardState.IsKeyDown(Keys.Escape) || currentGamePadState.IsButtonDown(Buttons.Back))
                this.Exit();

            if (!paused)
            {
                unpausedClock += gameTime.ElapsedGameTime;
            }

            // Run collision even when paused, for timing and debugging, and so the step
            // forward/backward keys work.
            Animate();
            Collide();
            HandleInput(gameTime);

            base.Update(gameTime);
        }

        private void ReadInputDevices()
        {
            previousKeyboardState = currentKeyboardState;
            previousGamePadState = currentGamePadState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentGestures.Clear();
            while (TouchPanel.IsGestureAvailable)
            {
                currentGestures.Add(TouchPanel.ReadGesture());
            }
        }

        /// <summary>
        /// Move all the secondary shapes around, and move the ray so it
        /// sweeps back and forth across its secondry shapes.
        /// </summary>
        private void Animate()
        {
            float t = (float)unpausedClock.TotalSeconds / 2.0F;

            // Rates at which x,y,z cycle
            const float xRate = 1.1f;
            const float yRate = 3.6f;
            const float zRate = 1.9f;
            const float pathSize = 6;

            // how far apart following shapes are, in seconds
            const float gap = 0.25f;

            // orientation for rotatable shapes
            Quaternion orientation = Quaternion.CreateFromYawPitchRoll(t * 0.2f, t * 1.4f, t);

            for (int g = 0; g < NumGroups; g++)
            {
                // Animate spheres
                secondarySpheres[g].Center = cameraOrigins[g];
                secondarySpheres[g].Center.X += pathSize * (float)Math.Sin(xRate * t);
                secondarySpheres[g].Center.Y += pathSize * (float)Math.Sin(yRate * t);
                secondarySpheres[g].Center.Z += pathSize * (float)Math.Sin(zRate * t);

                // Animate oriented boxes
                secondaryOBoxes[g].Center = cameraOrigins[g];
                secondaryOBoxes[g].Orientation = orientation;
                secondaryOBoxes[g].Center.X += pathSize * (float)Math.Sin(xRate * (t - gap));
                secondaryOBoxes[g].Center.Y += pathSize * (float)Math.Sin(yRate * (t - gap));
                secondaryOBoxes[g].Center.Z += pathSize * (float)Math.Sin(zRate * (t - gap));

                // Animate axis-aligned boxes
                Vector3 boxsize = new Vector3(1.0f, 1.3f, 1.9f);
                secondaryAABoxes[g].Min = cameraOrigins[g] - boxsize * 0.5f;
                secondaryAABoxes[g].Min.X += pathSize * (float)Math.Sin(xRate * (t - 2 * gap));
                secondaryAABoxes[g].Min.Y += pathSize * (float)Math.Sin(yRate * (t - 2 * gap));
                secondaryAABoxes[g].Min.Z += pathSize * (float)Math.Sin(zRate * (t - 2 * gap));
                secondaryAABoxes[g].Max = secondaryAABoxes[g].Min + boxsize;

                // Animate triangles
                Vector3 trianglePos = cameraOrigins[g];
                trianglePos.X += pathSize * (float)Math.Sin(xRate * (t - 3 * gap));
                trianglePos.Y += pathSize * (float)Math.Sin(yRate * (t - 3 * gap));
                trianglePos.Z += pathSize * (float)Math.Sin(zRate * (t - 3 * gap));

                // triangle points in local space - equilateral triangle with radius of 2
                secondaryTris[g].V0 = trianglePos + Vector3.Transform(new Vector3(0, 2, 0), orientation);
                secondaryTris[g].V1 = trianglePos + Vector3.Transform(new Vector3(1.73f, -1, 0), orientation);
                secondaryTris[g].V2 = trianglePos + Vector3.Transform(new Vector3(-1.73f, -1, 0), orientation);
            }
            //int index = (int)(t*5);
            //secondaryOBoxes[OBoxGroupIndex] = BoundingOrientedBox.iboxes[index % BoundingOrientedBox.iboxes.Length];

            // Animate primary ray (this is the only animated primary object)
            // It sweeps back and forth across the secondary objects
            const float sweepTime = 3.1f;
            float rayDt = (-Math.Abs((t/sweepTime) % 2.0f - 1.0f) * NumSecondaryShapes + 0.5f) * gap;
            primaryRay.Direction.X = (float)Math.Sin(xRate * (t + rayDt));
            primaryRay.Direction.Y = (float)Math.Sin(yRate * (t + rayDt));
            primaryRay.Direction.Z = (float)Math.Sin(zRate * (t + rayDt));
            primaryRay.Direction.Normalize();
        }

        /// <summary>
        /// Check each pair of objects for collision/containment and store the results for
        /// coloring them at render time.
        /// </summary>
        private void Collide()
        {
            // test collisions between objects and frustum
            collideResults[FrustumGroupIndex, SphereIndex] = primaryFrustum.Contains(secondarySpheres[FrustumGroupIndex]);
            collideResults[FrustumGroupIndex, OBoxIndex] = BoundingOrientedBox.Contains(primaryFrustum, ref secondaryOBoxes[FrustumGroupIndex]);
            collideResults[FrustumGroupIndex, AABoxIndex] = primaryFrustum.Contains(secondaryAABoxes[FrustumGroupIndex]);
            collideResults[FrustumGroupIndex, TriIndex] = TriangleTest.Contains(primaryFrustum, ref secondaryTris[FrustumGroupIndex]);

            // test collisions between objects and aligned box
            collideResults[AABoxGroupIndex, SphereIndex] = primaryAABox.Contains(secondarySpheres[AABoxGroupIndex]);
            collideResults[AABoxGroupIndex, OBoxIndex] = BoundingOrientedBox.Contains(ref primaryAABox, ref secondaryOBoxes[AABoxGroupIndex]);
            collideResults[AABoxGroupIndex, AABoxIndex] = primaryAABox.Contains(secondaryAABoxes[AABoxGroupIndex]);
            collideResults[AABoxGroupIndex, TriIndex] = TriangleTest.Contains(ref primaryAABox, ref secondaryTris[AABoxGroupIndex]);

            // test collisions between objects and oriented box
            collideResults[OBoxGroupIndex, SphereIndex] = primaryOBox.Contains(ref secondarySpheres[OBoxGroupIndex]);
            collideResults[OBoxGroupIndex, OBoxIndex] = primaryOBox.Contains(ref secondaryOBoxes[OBoxGroupIndex]);
            collideResults[OBoxGroupIndex, AABoxIndex] = primaryOBox.Contains(ref secondaryAABoxes[OBoxGroupIndex]);
            collideResults[OBoxGroupIndex, TriIndex] = TriangleTest.Contains(ref primaryOBox, ref secondaryTris[OBoxGroupIndex]);

            // test collisions between objects and sphere
            collideResults[SphereGroupIndex, SphereIndex] = primarySphere.Contains(secondarySpheres[SphereGroupIndex]);
            collideResults[SphereGroupIndex, OBoxIndex] = BoundingOrientedBox.Contains(ref primarySphere, ref secondaryOBoxes[SphereGroupIndex]);
            collideResults[SphereGroupIndex, AABoxIndex] = primarySphere.Contains(secondaryAABoxes[SphereGroupIndex]);
            collideResults[SphereGroupIndex, TriIndex] = TriangleTest.Contains(ref primarySphere, ref secondaryTris[SphereGroupIndex]);

            // test collisions between objects and ray
            float dist = -1;
            collideResults[RayGroupIndex, SphereIndex] =
            collideResults[RayGroupIndex, OBoxIndex] =
            collideResults[RayGroupIndex, AABoxIndex] =
            collideResults[RayGroupIndex, TriIndex] = ContainmentType.Disjoint;
            rayHitResult = null;

            float? r = primaryRay.Intersects(secondarySpheres[RayGroupIndex]);
            if (r.HasValue)
            {
                collideResults[RayGroupIndex, SphereIndex] = ContainmentType.Intersects;
                dist = r.Value;
            }

            r = secondaryOBoxes[RayGroupIndex].Intersects(ref primaryRay);
            if (r.HasValue)
            {
                collideResults[RayGroupIndex, OBoxIndex] = ContainmentType.Intersects;
                dist = r.Value;
            }

            r = primaryRay.Intersects(secondaryAABoxes[RayGroupIndex]);
            if (r.HasValue)
            {
                collideResults[RayGroupIndex, AABoxIndex] = ContainmentType.Intersects;
                dist = r.Value;
            }

            r = TriangleTest.Intersects(ref primaryRay, ref secondaryTris[RayGroupIndex]);
            if (r.HasValue)
            {
                collideResults[RayGroupIndex, TriIndex] = ContainmentType.Intersects;
                dist = r.Value;
            }

            // If one of the ray intersection tests was successful, fDistance will be positive.
            // If so, compute the intersection location and store it in g_RayHitResultBox.
            if (dist > 0)
            {
                rayHitResult = primaryRay.Position + primaryRay.Direction * dist;
            }
        }

        private void HandleInput(GameTime gameTime)
        {
            // Rely on the fact that we are using fixed time-step update
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Allow single-stepping through time
            if (paused)
            {
                if (currentKeyboardState.IsKeyDown(Keys.OemOpenBrackets))
                    unpausedClock -= TimeSpan.FromSeconds(dt);
                if (currentKeyboardState.IsKeyDown(Keys.OemCloseBrackets))
                    unpausedClock += TimeSpan.FromSeconds(dt);
            }

            // Change yaw/pitch based on right thumbstickb
            cameraYaw += currentGamePadState.ThumbSticks.Right.X * dt * YAW_RATE;
            cameraPitch += currentGamePadState.ThumbSticks.Right.Y * dt * PITCH_RATE;

            if (currentKeyboardState.IsKeyDown(Keys.Right))
                cameraYaw += dt * YAW_RATE;
            if (currentKeyboardState.IsKeyDown(Keys.Left))
                cameraYaw -= dt * YAW_RATE;

            if (currentKeyboardState.IsKeyDown(Keys.Up))
                cameraPitch += dt * PITCH_RATE;
            if (currentKeyboardState.IsKeyDown(Keys.Down))
                cameraPitch -= dt * PITCH_RATE;

            // Change distance based on right/left trigger
            if (currentGamePadState.IsButtonDown(Buttons.LeftTrigger)
                || currentKeyboardState.IsKeyDown(Keys.Subtract)
                || currentKeyboardState.IsKeyDown(Keys.OemMinus))
            {
                cameraDistance += dt * DISTANCE_RATE;
            }

            if (currentGamePadState.IsButtonDown(Buttons.RightTrigger)
                || currentKeyboardState.IsKeyDown(Keys.Add)
                || currentKeyboardState.IsKeyDown(Keys.OemPlus))
            {
                cameraDistance -= dt * DISTANCE_RATE;
            }

            // Group cycle
            if ( (currentKeyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G)) ||
                 (currentGamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A)))
            {
                currentCamera = (currentCamera + 1) % NumGroups;
            }

            // Camera reset
            if ((currentKeyboardState.IsKeyDown(Keys.Home) && previousKeyboardState.IsKeyUp(Keys.Home))
                || (currentGamePadState.IsButtonDown(Buttons.Y) && previousGamePadState.IsButtonUp(Buttons.Y)))
            {
                cameraYaw = (float)Math.PI * 0.75F;
                cameraPitch = MathHelper.PiOver4;
                cameraDistance = 40;
            }

            // Orthographic vs. perpsective projection toggle
            if ((currentKeyboardState.IsKeyDown(Keys.B) && previousKeyboardState.IsKeyUp(Keys.B))
                || (currentGamePadState.IsButtonDown(Buttons.B) && previousGamePadState.IsButtonUp(Buttons.B)))
            {
                cameraOrtho = !cameraOrtho;
            }

            if (currentKeyboardState.IsKeyDown(Keys.O))
            {
                cameraOrtho = true;
            }

            if (currentKeyboardState.IsKeyDown(Keys.P))
            {
                cameraOrtho = false;
            }

            // Pause animation
            if ((currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                || (currentGamePadState.IsButtonDown(Buttons.X) && previousGamePadState.IsButtonUp(Buttons.X)))
            {
                paused = !paused;
            }

            // Handle tap, drag, and pinch gestures
            foreach (GestureSample sample in currentGestures)
            {
                switch (sample.GestureType)
                {
                    case GestureType.Tap:
                        currentCamera = (currentCamera + 1) % NumGroups;
                        break;

                    case GestureType.FreeDrag:
                        cameraYaw += sample.Delta.X * -YAW_DRAG_RATE;
                        cameraPitch += sample.Delta.Y * PITCH_DRAG_RATE;
                        break;

                    case GestureType.Pinch:
                        float dOld = Vector2.Distance(sample.Position - sample.Delta, sample.Position2 - sample.Delta2);
                        float dNew = Vector2.Distance(sample.Position, sample.Position2);
                        cameraDistance *= (float)Math.Exp((dOld - dNew) * PINCH_ZOOM_RATE);
                        break;
                }
            }

            // Clamp camera to safe values
            cameraYaw = MathHelper.WrapAngle(cameraYaw);
            cameraPitch = MathHelper.Clamp(cameraPitch, -MathHelper.PiOver2, MathHelper.PiOver2);
            cameraDistance = MathHelper.Clamp(cameraDistance, 2, 80);

            // Handle time-based lerp for group transition
            float lerp = Math.Min(4.0F * dt, 1.0F);
            cameraTarget = (lerp * cameraOrigins[currentCamera]) + ((1.0F - lerp) * cameraTarget);
        }

        #endregion

        #region Draw

        /// <summary>
        /// This draws the bounding shapes and HUD for the sample.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            float aspect = GraphicsDevice.Viewport.AspectRatio;

            float yawCos = (float)Math.Cos(cameraYaw);
            float yawSin = (float)Math.Sin(cameraYaw);

            float pitchCos = (float)Math.Cos(cameraPitch);
            float pitchSin = (float)Math.Sin(cameraPitch);

            Vector3 eye = new Vector3(cameraDistance * pitchCos * yawSin + cameraTarget.X,
                                       cameraDistance * pitchSin + cameraTarget.Y,
                                       cameraDistance * pitchCos * yawCos + cameraTarget.Z);
            Matrix view = Matrix.CreateLookAt(eye, cameraTarget, Vector3.Up);
            Matrix projection = (cameraOrtho)
                                ? Matrix.CreateOrthographic(aspect * cameraDistance, cameraDistance, 1.0F, 1000.0F)
                                : Matrix.CreatePerspectiveFieldOfView((float)(System.Math.PI) / 4.0F, aspect, 1.0F, 1000.0F);

            debugDraw.Begin(view, projection);

            // Draw ground planes
            for (int g = 0; g < NumGroups; ++g)
            {
                Vector3 origin = new Vector3(cameraOrigins[g].X - 20, cameraOrigins[g].Y - 10, cameraOrigins[g].Z - 20);
                debugDraw.DrawWireGrid(Vector3.UnitX*40, Vector3.UnitZ*40, origin, 20, 20, Color.Black);
            }

            DrawPrimaryShapes();

            // Draw secondary shapes
            for (int g = 0; g < NumGroups; g++)
            {
                debugDraw.DrawWireSphere(secondarySpheres[g], GetCollideColor(g, SphereIndex));
                debugDraw.DrawWireBox(secondaryAABoxes[g], GetCollideColor(g, AABoxIndex));
                debugDraw.DrawWireBox(secondaryOBoxes[g], GetCollideColor(g, OBoxIndex));
                debugDraw.DrawWireTriangle(secondaryTris[g], GetCollideColor(g, TriIndex));
            }

            // Draw results of ray-object intersection, if there was a hit this frame
            if (rayHitResult.HasValue)
            {
                Vector3 size = new Vector3(0.05f, 0.05f, 0.05f);
                BoundingBox weeBox = new BoundingBox(rayHitResult.Value - size, rayHitResult.Value + size);
                debugDraw.DrawWireBox(weeBox, Color.Yellow);
            }

            debugDraw.End();

            // Draw overlay text.
            //     string text = "A = (G)roup\nY = Reset (Home)\nB = (O)rtho/(P)erspective\nX = Pause (Space)";

            // spriteBatch.Begin();
            // spriteBatch.DrawString(spriteFont, text, new Vector2(86, 48), Color.White);
            // spriteBatch.End();

            base.Draw(gameTime);
        }

        void DrawPrimaryShapes()
        {
            debugDraw.DrawWireBox(primaryAABox, Color.White);
            debugDraw.DrawWireBox(primaryOBox, Color.White);
            debugDraw.DrawWireFrustum(primaryFrustum, Color.White);
            debugDraw.DrawWireSphere(primarySphere, Color.White);
            debugDraw.DrawRay(primaryRay, Color.Red, 10.0f);
        }

        private Color GetCollideColor(int group, int shape)
        {
            ContainmentType cr = collideResults[group, shape];
            switch (cr)
            {
                case ContainmentType.Contains:
                    return Color.Red;
                case ContainmentType.Disjoint:
                    return Color.LightGray;
                case ContainmentType.Intersects:
                    return Color.Yellow;
                default:
                    return Color.Black;
            }
        }

        #endregion
    }

}
