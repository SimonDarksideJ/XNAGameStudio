#region File Description
//-----------------------------------------------------------------------------
// Sun.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
#endregion

namespace Spacewar
{
    /// <summary>
    /// Represents the sun game object
    /// </summary>
    public class Sun : SceneItem
    {
        /// <summary>
        /// Creates a new sun object
        /// </summary>
        /// <param name="shape">Which shape to use to draw this</param>
        /// <param name="position">Where to draw it on the screen</param>
        public Sun(Game game, Shape shape, Vector3 position)
            : base(game, shape, position)
        {
            if ((shape == null) || (shape is EvolvedSun))
            {
                center = new Vector3(.5f, .5f, 0);
                rotation = Vector3.Zero;
                radius = 15f;
            }
            else
            {
                scale = new Vector3(8, 8, 8);
                radius = 11f;
            }
        }

        /// <summary>
        /// Update the sun - a simple slow rotation
        /// </summary>
        /// <param name="time">Current game time</param>
        /// <param name="elapsedTime">Elapsed time since last update</param>
        public override void Update(TimeSpan time, TimeSpan elapsedTime)
        {
            rotation.Z += (float)(elapsedTime.TotalSeconds / 10.0f);
            base.Update(time, elapsedTime);
        }
    }
}
