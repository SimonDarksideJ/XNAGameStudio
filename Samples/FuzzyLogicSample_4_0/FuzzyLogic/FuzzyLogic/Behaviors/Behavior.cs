#region File Description
//-----------------------------------------------------------------------------
// Behavior.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace FuzzyLogic
{
    /// <summary>
    /// Behavior is the base class for the three behaviors in this sample: chasing, 
    /// evading, and wandering. It is an abstract class, leaving the implementation of 
    /// Update up to its subclasses. Entity objects keep track of their current behavior
    /// and tell it to update.
    /// </summary>
    public abstract class Behavior
    {
        /// <summary>
        /// Keep track of the entity that this behavior will modify.
        /// </summary>
        public Entity Entity
        {
            get { return entity; }
            set { entity = value; }
        }
        private Entity entity;

        protected Behavior(Entity entity)
        {
            this.entity = entity;
        }

        public abstract void Update();

        /// <summary>
        /// Turns the entity towards a vector at the given speed. This is the same logic
        /// as the TurnToFace function that was introduced in the Chase and Evade 
        /// sample.
        /// </summary>
        public void TurnToFace(Vector2 facePosition, float turnSpeed)
        {
            float x = facePosition.X - Entity.Position.X;
            float y = facePosition.Y - Entity.Position.Y;

            float desiredAngle = (float)Math.Atan2(y, x);
            float difference = WrapAngle(desiredAngle - Entity.Orientation);

            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);
            Entity.Orientation = WrapAngle(Entity.Orientation + difference);
        }

        /// <summary>
        /// Returns the angle expressed in radians between -Pi and Pi.
        /// <param name="radians">the angle to wrap, in radians.</param>
        /// <returns>the input value expressed in radians from -Pi to Pi.</returns>
        /// </summary>
        public static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }
    }
}
