#region File Description
//-----------------------------------------------------------------------------
// WanderBehavior.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FuzzyLogic
{
    /// <summary>
    /// WanderBehavior is a Behavior that will make entities move around the screen 
    /// aimlessly. The logic is the same as we have seen in the previous AI sample. 
    /// </summary>
    public class WanderBehavior : Behavior
    {
        // The direction we are currently wandering in.
        private Vector2 wanderDirection;

        // We'll use this random number generator to tweak wanderDirection a little bit
        // on every update.
        private static Random random = new Random();

        public WanderBehavior(Entity entity)
            : base(entity)
        {
            // Initialize wanderDirection so that the entity will start off wandering in
            // the direction he is already going.
            wanderDirection.X = (float)Math.Cos(Entity.Orientation);
            wanderDirection.Y = (float)Math.Sin(Entity.Orientation);
        }

        /// <summary>
        /// Update will make modify the associated entity's orientation and speed in
        /// to make him wander around. The logic contained in this function is the same
        /// as we saw in the Chase and Evade sample.
        /// </summary>
        public override void Update()
        {
            wanderDirection.X +=
                MathHelper.Lerp(-.25f, .25f, (float)random.NextDouble());
            wanderDirection.Y +=
                MathHelper.Lerp(-.25f, .25f, (float)random.NextDouble());

            if (wanderDirection != Vector2.Zero)
            {
                wanderDirection.Normalize();
            }

            TurnToFace(Entity.Position + wanderDirection, .15f * Entity.TurnSpeed);

            // Next, we'll turn the characters back towards the center of the screen, to
            // prevent them from getting stuck on the edges of the screen.   
            Vector2 screenCenter = new Vector2(Entity.LevelBoundary.Width / 2,
                Entity.LevelBoundary.Height / 2);

            float distanceFromCenter = Vector2.Distance(screenCenter, Entity.Position);
            float MaxDistanceFromScreenCenter =
                Math.Min(screenCenter.Y, screenCenter.X);

            float normalizedDistance = distanceFromCenter / MaxDistanceFromScreenCenter;

            float turnToCenterSpeed = .3f * normalizedDistance * normalizedDistance *
                Entity.TurnSpeed;

            // Once we've calculated how much we want to turn towards the center, we can
            // use the TurnToFace function to actually do the work.
            TurnToFace(screenCenter, turnToCenterSpeed);

            Entity.CurrentSpeed = .25f * Entity.MaxSpeed;
        }
    }
}
