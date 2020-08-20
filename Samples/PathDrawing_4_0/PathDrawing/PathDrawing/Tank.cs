#region File Description
//-----------------------------------------------------------------------------
// Tank.cs
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

#endregion

namespace PathDrawing
{
    /// <summary>
    /// A simple object that moves towards it's set destination. This class is largely borrowed from
    /// the Waypoint Sample with the exception that behaviors have been removed and instead we've hard
    /// coded the LinearBehavior into the Tank because other steering behaviors usually cause issues
    /// with path-based waypoints given that the points are generally quite close to each other.
    /// </summary>
    public class Tank
    {
        /// <summary>
        /// The "close enough" limit, if the tank is inside this many pixel 
        /// to it's destination it's considered at it's destination
        /// </summary>
        const float atDestinationLimit = 5f;

        /// <summary>
        /// This is how much the Tank can turn in one second in radians, since Pi 
        /// radians makes half a circle the tank can all the way around in one second
        /// </summary>
        public static float MaxAngularVelocity
        {
            get { return maxAngularVelocity; }
        }
        const float maxAngularVelocity = MathHelper.Pi;

        /// <summary>
        /// This is the Tanks’ best possible movement speed
        /// </summary>
        public static float MaxMoveSpeed
        {
            get { return maxMoveSpeed; }
        }
        const float maxMoveSpeed = 100f;

        /// <summary>
        /// This is most the tank can speed up or slow down in one second
        /// </summary>
        public static float MaxMoveSpeedDelta
        {
            get { return maxMoveSpeedDelta; }
        }
        const float maxMoveSpeedDelta = maxMoveSpeed / 2;

        // Graphics data
        Texture2D tankTexture;
        Vector2 tankTextureCenter;

        // Rotation values
        float rotation;
        bool recomputeTargetRotation = true;
        float targetRotation;
        float previousRotation;
        float rotationInterpolation;

        /// <summary>
        /// Length 1 vector that represents the tanks’ movement and facing direction
        /// </summary>
        public Vector2 Direction
        {
            get { return direction; }
            set { direction = value; }
        }
        protected Vector2 direction;

        /// <summary>
        /// The tank's current movement speed
        /// </summary>
        public float MoveSpeed
        {
            get { return moveSpeed; }
            set { moveSpeed = value; }
        }
        protected float moveSpeed;

        /// <summary>
        /// The tank's location on the map
        /// </summary>
        public Vector2 Location
        {
            get { return location; }
        }
        private Vector2 location;

        /// <summary>
        /// The list of points the tanks will move to in order from first to last
        /// </summary>
        public WaypointList Waypoints
        {
            get { return waypoints; }
        }
        private WaypointList waypoints;


        /// <summary>
        /// Linear distance to the Tank's current destination
        /// </summary>
        public float DistanceToDestination
        {
            get { return Vector2.Distance(location, waypoints.Peek()); }
        }

        /// <summary>
        /// True when the tank is "close enough" to it's destination
        /// </summary>
        public bool AtDestination
        {
            get { return DistanceToDestination < atDestinationLimit; }
        }

        /// <summary>
        /// Tank constructor
        /// </summary>
        public Tank(GraphicsDevice graphicsDevice, ContentManager content)
        {
            location = Vector2.Zero;
            waypoints = new WaypointList();

            tankTexture = content.Load<Texture2D>("tank");

            tankTextureCenter = new Vector2(tankTexture.Width / 2, tankTexture.Height / 2);
        }

        /// <summary>
        /// Reset the Tank's location on the map
        /// </summary>
        /// <param name="newLocation">new location on the map</param>
        public void Reset(Vector2 newLocation)
        {
            location = newLocation;
            waypoints.Clear();
        }

        /// <summary>
        /// Tests if a given point is considered to "hit" the tank.
        /// </summary>
        /// <param name="point">The point to test against.</param>
        /// <returns>True if the point is "hitting" the tank, false otherwise.</returns>
        public bool HitTest(Vector2 point)
        {
            // We leverage a comparison of squared distances to avoid two square root operations,
            // which can be a slow operation if performed frequently.
            return Vector2.DistanceSquared(point, location) < tankTextureCenter.LengthSquared() * 1.5f;
        }

        /// <summary>
        /// Update the Tank's position if it's not "close enough" to 
        /// it's destination
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // If we have any waypoints, the first one on the list is where 
            // we want to go
            if (waypoints.Count > 0)
            {
                if (AtDestination)
                {
                    // If we’re at the destination and there is at least one 
                    // waypoint in the list, get rid of the first one since we’re 
                    // there now
                    waypoints.Dequeue();

                    // Whenever we arrive at a destination, we are going to need to
                    // figure out a new target rotation.
                    recomputeTargetRotation = true;
                }
                else
                {
                    // This gives us a vector that points directly from the tank's
                    // current location to the waypoint.
                    direction = -(Location - Waypoints.Peek());

                    // This scales the vector to 1, we'll use move Speed and elapsed Time 
                    // in the Tank's Update function to find the how far the tank moves
                    direction.Normalize();

                    // If we need to recompute our target rotation...
                    if (recomputeTargetRotation)
                    {
                        // Store the previous rotation for interpolation
                        previousRotation = rotation;

                        // Calculate the new rotation based on the direction
                        targetRotation = (float)Math.Atan2(direction.Y, direction.X);

                        // Reset our interpolation value
                        rotationInterpolation = 0f;

                        // We want to make sure we always turn the shortest way, so we need
                        // to check our rotation values and correct the target value if the
                        // two are more than 180 degrees different.
                        if (targetRotation - previousRotation > MathHelper.Pi)
                            targetRotation -= MathHelper.TwoPi;
                        else if (targetRotation - previousRotation < -MathHelper.Pi)
                            targetRotation += MathHelper.TwoPi;

                        // We don't need to recompute the rotation until we hit the next destination
                        recomputeTargetRotation = false;
                    }

                    // Increase our interpolation value
                    rotationInterpolation = MathHelper.Clamp(rotationInterpolation + elapsedTime * 10f, 0f, 1f);

                    // Calculate our rotation using linear interpolation between our rotation values
                    rotation = previousRotation + (targetRotation - previousRotation) * rotationInterpolation;

                    // Move us along in our direction
                    location += (direction * MoveSpeed * elapsedTime);
                }
            }
        }

        /// <summary>
        /// Draw the Tank
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(tankTexture, location, null, Color.White, rotation, tankTextureCenter, 1f, SpriteEffects.None, 0f);
        }
    }
}
