#region File Description
//-----------------------------------------------------------------------------
// PlayerMovement.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion



namespace ShipGame
{
    public class PlayerMovement
    {
        public Vector3 position;       // player position
        public Vector3 velocity;       // velocity in local player space
        public Vector3 force;          // forces in local player space

        // player rotation
        public Matrix rotation;                 
        // rotation velocities around each local player axis
        public Vector3 rotationVelocityAxis;
        // rotation forces around each local player axis
        public Vector3 rotationForce;           

        public float maxVelocity;           // maximum player velocity
        public float maxRotationVelocity;   // maximum player rotation velocity

        public float dampingForce;            // damping force
        public float dampingRotationForce;    // damping rotation force

        // maximum force created by input stick
        public float inputForce;
        // maximum rotation force created by input stick
        public float inputRotationForce;   

        /// <summary>
        /// Create a new player movement object for handling player motion
        /// </summary>
        public PlayerMovement()
        {
            position = Vector3.Zero;
            velocity = Vector3.Zero;
            force = Vector3.Zero;

            rotation = Matrix.Identity;
            rotationVelocityAxis = Vector3.Zero;
            rotationForce = Vector3.Zero;

            maxVelocity = GameOptions.MovementVelocity;
            dampingForce = GameOptions.MovementForceDamping;
            inputForce = GameOptions.MovementForce;

            maxRotationVelocity = GameOptions.MovementRotationVelocity;
            dampingRotationForce = GameOptions.MovementRotationForceDamping;
            inputRotationForce = GameOptions.MovementRotationForce;
        }

        /// <summary>
        /// Resets the position and rotation of the player and zero forces
        /// </summary>
        public void Reset(Matrix transfrom)
        {
            rotation = transfrom;
            position = transfrom.Translation;

            velocity = Vector3.Zero;
            force = Vector3.Zero;
            
            rotationVelocityAxis = Vector3.Zero;
            rotationForce = Vector3.Zero;
        }

        /// <summary>
        /// Get the current postion and rotation as a matrix
        /// </summary>
        public Matrix Transform
        {
            get
            {
                Matrix transform;

                // set rotation
                transform = rotation;

                // set translation
                transform.Translation = position;

                return transform;
            }
        }

        /// <summary>
        /// Get the normalized velocity
        /// </summary>
        public float VelocityFactor
        {
            get { return velocity.Length() / maxVelocity; }
        }

        /// <summary>
        /// Get/Set the velocity vector transformed to world space
        /// </summary>
        public Vector3 WorldVelocity
        {
            // transform local velocity to world space
            get
            {
                return velocity.X * rotation.Right +
                velocity.Y * rotation.Up + velocity.Z * rotation.Forward;
            }
            set
            {
                // transform world velocity into local space
                velocity.X = Vector3.Dot(rotation.Right, value);
                velocity.Y = Vector3.Dot(rotation.Up, value);
                velocity.Z = Vector3.Dot(rotation.Forward, value);
            }
        }

        /// <summary>
        /// Process movement input
        /// </summary>
        public void ProcessInput(float elapsedTime, InputState current, int player)
        {
            // camera rotation
            rotationForce.X = 
                inputRotationForce * current.padState[player].ThumbSticks.Right.Y;
            rotationForce.Y = 
                -inputRotationForce * current.padState[player].ThumbSticks.Right.X;
            rotationForce.Z = 0.0f;

            // camera bank
            if (current.padState[player].Buttons.RightShoulder == ButtonState.Pressed)
                rotationForce.Z += inputRotationForce;
            if (current.padState[player].Buttons.LeftShoulder == ButtonState.Pressed)
                rotationForce.Z -= inputRotationForce;

            // move forward/backward
            force.X = inputForce * current.padState[player].ThumbSticks.Left.X;

            if (current.padState[player].Buttons.RightStick == ButtonState.Pressed)
            {
                // slide up/down
                force.Y = inputForce * current.padState[player].ThumbSticks.Left.Y;
                force.Z = 0.0f;
            }
            else
            {
                // slide left/right
                force.Y = 0.0f;
                force.Z = inputForce * current.padState[player].ThumbSticks.Left.Y;
            }

            // keyboard camera rotation
            if (current.keyState[player].IsKeyDown(Keys.Up))
                rotationForce.X = inputRotationForce;
            if (current.keyState[player].IsKeyDown(Keys.Down))
                rotationForce.X = -inputRotationForce;
            if (current.keyState[player].IsKeyDown(Keys.Left))
                rotationForce.Y = inputRotationForce;
            if (current.keyState[player].IsKeyDown(Keys.Right))
                rotationForce.Y = -inputRotationForce;
            // keyboard camera bank
            if (current.keyState[player].IsKeyDown(Keys.A))
                rotationForce.Z = -inputRotationForce;
            if (current.keyState[player].IsKeyDown(Keys.D))
                rotationForce.Z = inputRotationForce;
            // move forward/backward
            if (current.keyState[player].IsKeyDown(Keys.W))
                force.Z = inputForce;
            if (current.keyState[player].IsKeyDown(Keys.S))
                force.Z = -inputForce;
            // slide left/right
            if (current.keyState[player].IsKeyDown(Keys.Q))
                force.X = -inputForce;
            if (current.keyState[player].IsKeyDown(Keys.E))
                force.X = inputForce;
        }

        public void Update(float elapsedTime)
        {
            // apply force
            velocity += force * elapsedTime;

            // apply damping
            if (force.X > -0.001f && force.X < 0.001f)
                if (velocity.X > 0)
                    velocity.X = Math.Max(0.0f, velocity.X - dampingForce * elapsedTime);
                else
                    velocity.X = Math.Min(0.0f, velocity.X + dampingForce * elapsedTime);
            if (force.Y > -0.001f && force.Y < 0.001f)
                if (velocity.Y > 0)
                    velocity.Y = Math.Max(0.0f, velocity.Y - dampingForce * elapsedTime);
                else
                    velocity.Y = Math.Min(0.0f, velocity.Y + dampingForce * elapsedTime);
            if (force.Z > -0.001f && force.Z < 0.001f)
                if (velocity.Z > 0)
                    velocity.Z = Math.Max(0.0f, velocity.Z - dampingForce * elapsedTime);
                else
                    velocity.Z = Math.Min(0.0f, velocity.Z + dampingForce * elapsedTime);

            // crop with maximum velocity
            float velocityLength = velocity.Length();
            if (velocityLength > maxVelocity)
                velocity = Vector3.Normalize(velocity) * maxVelocity;

            // apply velocity
            position += rotation.Right * velocity.X * elapsedTime;
            position += rotation.Up * velocity.Y * elapsedTime;
            position += rotation.Forward * velocity.Z * elapsedTime;
            
            // apply rot force
            rotationVelocityAxis += rotationForce * elapsedTime;

            // apply rot damping
            if (rotationForce.X > -0.001f && rotationForce.X < 0.001f)
                if (rotationVelocityAxis.X > 0)
                    rotationVelocityAxis.X = Math.Max(0.0f, 
                                    rotationVelocityAxis.X - 
                                    dampingRotationForce * elapsedTime);
                else
                    rotationVelocityAxis.X = Math.Min(0.0f, 
                                    rotationVelocityAxis.X + 
                                    dampingRotationForce * elapsedTime);
            
            if (rotationForce.Y > -0.001f && rotationForce.Y < 0.001f)
                if (rotationVelocityAxis.Y > 0)
                    rotationVelocityAxis.Y = Math.Max(0.0f, 
                                    rotationVelocityAxis.Y - 
                                    dampingRotationForce * elapsedTime);
                else
                    rotationVelocityAxis.Y = Math.Min(0.0f, 
                                    rotationVelocityAxis.Y + 
                                    dampingRotationForce * elapsedTime);
            
            if (rotationForce.Z > -0.001f && rotationForce.Z < 0.001f)
                if (rotationVelocityAxis.Z > 0)
                    rotationVelocityAxis.Z = Math.Max(0.0f, 
                                    rotationVelocityAxis.Z - 
                                    dampingRotationForce * elapsedTime);
                else
                    rotationVelocityAxis.Z = Math.Min(0.0f, 
                                    rotationVelocityAxis.Z + 
                                    dampingRotationForce * elapsedTime);

            // crop with maximum rot velocity
            float rotationVelocityLength = rotationVelocityAxis.Length();
            if (rotationVelocityLength > maxRotationVelocity)
                rotationVelocityAxis = Vector3.Normalize(rotationVelocityAxis) * 
                    maxRotationVelocity;

            // apply rot vel
            Matrix rotationVelocity = Matrix.Identity;

            if (rotationVelocityAxis.X < -0.001f || rotationVelocityAxis.X > 0.001f)
                rotationVelocity = rotationVelocity * 
                    Matrix.CreateFromAxisAngle(rotation.Right, 
                    rotationVelocityAxis.X * elapsedTime);

            if (rotationVelocityAxis.Y < -0.001f || rotationVelocityAxis.Y > 0.001f)
                rotationVelocity = rotationVelocity * 
                    Matrix.CreateFromAxisAngle(rotation.Up, 
                    rotationVelocityAxis.Y * elapsedTime);

            if (rotationVelocityAxis.Z < -0.001f || rotationVelocityAxis.Z > 0.001f)
                rotationVelocity = rotationVelocity * 
                    Matrix.CreateFromAxisAngle(rotation.Forward, 
                    rotationVelocityAxis.Z * elapsedTime);

            rotation = rotation * rotationVelocity;
        }
    }
}
