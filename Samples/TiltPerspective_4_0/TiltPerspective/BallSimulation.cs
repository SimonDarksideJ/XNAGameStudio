//-----------------------------------------------------------------------------
// Bounce.cs
// Demonstrates basic sphere->plane and sphere->sphere collision physics,
// accelerometer driven gravity, shake detection, and basic rendering of procedure.
// geometry.
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;


namespace TiltPerspectiveSample
{
 
    public class Ball
    {
        public Vector3 Position;
        public Vector3 Velocity;

        public float Radius;
        public float Mass;
        public Color Color;
    }

    /// <summary>
    /// This sample shows how to draw 3D geometric collisions
    /// </summary>
    /// 
    public class BallSimulation : GameComponent
    {
        #region Fields

        // our primitive for rendering (only a sphere is currently supported)
        GeometricPrimitive spherePrimitive;
        
        // Store a list of balls with unique characteristics (position, mass, etc.)
        List<Ball> Balls = new List<Ball>();
        List<Plane> Walls = new List<Plane>();

        // dampen velocity each time a collision occurs
        const float wallElasticity = 0.75f;
        const float ballElasticity = 0.75f;

        // Store a list of tint ballColors
        Color[] ballColors = new Color[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.White,
            Color.Black,
        };


        #endregion

        #region Initialization
        public BallSimulation(Game game)
            : base(game)
        {
            Balls = new List<Ball>();
            Walls = new List<Plane>();
        }

        public void AddWall(Plane wallPlane)
        {
            Walls.Add(wallPlane);
        }

        public void AddWalls(BoundingBox worldBox)
        {
            AddWall(new Plane(-Vector3.UnitX, worldBox.Max.X));
            AddWall(new Plane(Vector3.UnitX, -worldBox.Min.X));
            AddWall(new Plane(-Vector3.UnitY, worldBox.Max.Y));
            AddWall(new Plane(Vector3.UnitY, -worldBox.Min.Y));
            //AddWall(new Plane(-Vector3.UnitZ, worldBox.Max.Z));
            AddWall(new Plane(Vector3.UnitZ, -worldBox.Min.Z));
        }

        // Add some balls to the world
        public void AddBalls(int n, float minRadius, float maxRadius, BoundingBox inBox)
        {
            Random rng = RandomUtil.SharedRandom;

            for (int i = 0; i < n; ++i)
            {
                float r = rng.NextFloat(minRadius,maxRadius);

                Ball newBall = new Ball
                {
                    Velocity = Vector3.Zero,
                    Color = ballColors[i % ballColors.Length],
                    Radius = r,
                    Position = new Vector3(
                        rng.NextFloat(inBox.Min.X + r, inBox.Max.X - r),
                        rng.NextFloat(inBox.Min.Y + r, inBox.Max.Y - r),
                        rng.NextFloat(inBox.Min.Z + r, inBox.Max.Z - r)),

                    // Since the spheres we draw are kind of plastic looking, it looks better if we make mass
                    // proportional to r^2 (like a hollow inflatable ball) rather than r^3 (solid).
                    Mass = r * r
                };
                Balls.Add( newBall );
            }
        }

        public override void  Initialize()
        {
            base.Initialize();

            spherePrimitive = new SpherePrimitive(Game.GraphicsDevice);
        }


        #endregion

        #region Update and Draw

        /// <summary>
        // sphereCollisionImplicit()
        //
        // Given 2 balls with velocity, mass, and size, evaluate whether a collision occured.
        // If it did, move them to be non-penetrating and update their velocities.
        /// </summary>
        private void sphereCollisionImplicit(Ball ball1, Ball ball2)
        {
            Vector3 relativepos = ball2.Position - ball1.Position;
            float distance2 = relativepos.LengthSquared();
            float radii = ball1.Radius + ball2.Radius;
            if (distance2 >= radii * radii)
                return; // no collision

            float distance = relativepos.Length();
            Vector3 relativeUnit = relativepos * (1.0f / distance);
            Vector3 penetration = relativeUnit * (radii - distance);

            // Adjust the spheres' relative positions
            float mass1 = ball1.Mass;
            float mass2 = ball2.Mass;

            float m_inv = 1.0f / (mass1 + mass2);
            float weight1 = mass1 * m_inv; // relative weight of ball 1
            float weight2 = mass2 * m_inv; // relative weight of ball 2. w1+w2==1.0

            ball1.Position -= weight2 * penetration;
            ball2.Position += weight1 * penetration;

            // Adjust the objects’ relative velocities, if they are
            // moving toward each other.
            //
            // Note that we're assuming no friction, or equivalently, no angular momentum.
            //
            // velocityTotal = velocity of v2 in v1 stationary ref. frame
            // get reference frame of common center of mass

            Vector3 centerVelocity = ball1.Velocity * weight1 + ball2.Velocity * weight2;
            Vector3 relativeMomentum = (ball2.Velocity - centerVelocity) * mass2;
            float contactImpulse = Vector3.Dot(relativeMomentum, relativeUnit);
            if (contactImpulse < 0)
            {
                relativeMomentum -= relativeUnit * (contactImpulse * (ballElasticity + 1));
                ball1.Velocity = (-relativeMomentum) / mass1 + centerVelocity;
                ball2.Velocity = relativeMomentum / mass2 + centerVelocity;
            }
        }

        /// <summary>
        /// UpdateBalls()
        /// Evalute ball physics, generate new velocities based on collisions, and update
        /// ball positions
        /// </summary>
        protected void UpdateBalls(float dt)
        {
            IAccelerometerService accel = (IAccelerometerService)Game.Services.GetService(typeof(IAccelerometerService));
            Vector3 gravity = accel.RawAcceleration * 2000;

            // Update positions, add air friction and gravity
            foreach(Ball ball in Balls)
            {
                ball.Position += ball.Velocity * dt; // +(gravity * (dt * dt * 0.5f));
                ball.Velocity += gravity * dt;
            }

            // Resolve ball-ball collisions
            int ballCount = Balls.Count;
            for (int i = 0; i < ballCount; i++)
            {
                for (int j = i + 1; j < ballCount; j++)
                {
                    sphereCollisionImplicit(Balls[i], Balls[j]);
                }
            }

            // Resolve collisions with floor
            for (int i = 0; i < ballCount; i++)
            {
                Ball ball = Balls[i];

                // Resolves collisions with walls
                foreach (Plane wall in Walls)
                {
                    float depth = wall.DotCoordinate(ball.Position) - ball.Radius;
                    if (depth < 0)
                    {
                        // collision with wall
                        ball.Position -= wall.Normal * depth;

                        float impact = wall.DotNormal(ball.Velocity);
                        if (impact < 0)
                        {
                            ball.Velocity -= wall.Normal * (impact * (wallElasticity + 1));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // We break each game update into multiple simulation substeps,
            // to get more accurate handling of multi-object collisions.
            const int substeps = 4;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = 0; i < substeps; i++)
            {
                UpdateBalls(dt / substeps);
            }
            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        public void Draw(Matrix view, Matrix projection, Vector3 lightDirection)
        {
            GraphicsDevice graphicsDevice = Game.GraphicsDevice;

            spherePrimitive.LightDirection = lightDirection;

            RasterizerState tmpRasterizerState = new RasterizerState();
            tmpRasterizerState.MultiSampleAntiAlias = true;
            tmpRasterizerState.FillMode = FillMode.Solid;
            tmpRasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            
            // Apply the updated Rasterizer state
            graphicsDevice.RasterizerState = tmpRasterizerState;

            foreach (Ball ball in Balls)
            {
                Matrix world = Matrix.CreateScale(ball.Radius * 2.0f);
                world.Translation = ball.Position;
                spherePrimitive.Draw(world, view, projection, ball.Color, false);
            }

            // Since we know where the ground planes are, generate simple shadows
            // by squashing the geometry onto these planes and rendering it in black.
            foreach (Plane plane in Walls)
            {
                const float epsilon = 0.01f;
                const float planeOffset = 0.5f; // offset shadows to avoid depth buffer artifacts
                if (plane.DotNormal(lightDirection) >= -epsilon)
                    continue;

                Matrix shadowMatrix = Matrix.CreateShadow(lightDirection, new Plane(-plane.Normal, planeOffset - plane.D));

                foreach (Ball ball in Balls)
                {
                    Matrix world = Matrix.CreateScale(ball.Radius * 2.0f);
                    world.Translation = ball.Position;

                    world *= shadowMatrix;
                    spherePrimitive.Draw(world, view, projection, Color.Black, false);
                }
            }

            // Reset the fill mode renderstate.
            tmpRasterizerState = new RasterizerState();
            tmpRasterizerState.FillMode = FillMode.Solid;
            tmpRasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            graphicsDevice.RasterizerState = tmpRasterizerState;
        }


        #endregion
    }
}
    
