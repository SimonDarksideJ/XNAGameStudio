#region File Description
//-----------------------------------------------------------------------------
// SpacewarSceneItem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
#endregion

namespace Spacewar
{
    /// <summary>
    /// SpacewarSceneItem is a SceneItem that adds gravity and screen wrapping to the item automatically
    /// </summary>
    public class SpacewarSceneItem : SceneItem
    {
        /// <summary>
        /// Default Constructor - does nothing special
        /// </summary>
        public SpacewarSceneItem(Game game)
            : base(game)
        {
        }

        /// <summary>
        /// This constructor creates an item with a particular shape at a particular point
        /// </summary>
        /// <param name="shape">A shape that represents the item</param>
        /// <param name="initialPosition">The world space position</param>
        public SpacewarSceneItem(Game game, Shape shape, Vector3 initialPosition)
            : base(game, shape, initialPosition)
        {
        }

        /// <summary>
        /// SpacewarSceneItem handles all the gravity and wrapping calculations
        /// </summary>
        /// <param name="time"></param>
        /// <param name="elapsedTime"></param>
        public override void Update(TimeSpan time, TimeSpan elapsedTime)
        {
            if (!Paused)
            {
                //Add in gravity 
                Vector3 forceDirection = Position - new Vector3(SpacewarGame.Settings.SunPosition, 0.0f);
                double distancePower = Math.Pow(forceDirection.Length(), SpacewarGame.Settings.GravityPower);
                double factor = Math.Min(SpacewarGame.Settings.GravityStrength / distancePower, 100.0); //stops insane accelerations at the sun since we have no collisions
                Vector3 gravityAcceleration = Vector3.Multiply(Vector3.Normalize(forceDirection), (float)factor);
                acceleration -= gravityAcceleration;
            }

            //Call the base to update velocity and position
            base.Update(time, elapsedTime);

            //Zero out acceleration - will be reset on next update
            acceleration = Vector3.Zero;

            //Wrap around the screen to the correct position.
            if (SpacewarGame.GameState == GameState.PlayEvolved)
            {
                if (position.X > 400)
                    position.X = -400;
                else if (position.X < -400)
                    position.X = 400;
            }
            else
            {
                if (position.X > 300)
                    position.X = -300;
                else if (position.X < -300)
                    position.X = 300;
            }

            if (position.Y > 250)
                position.Y = -250;
            else if (position.Y < -250)
                position.Y = 250;
        }
    }
}
