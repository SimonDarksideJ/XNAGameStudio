#region File Description
//-----------------------------------------------------------------------------
// Particle.cs
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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Spacewar
{
    /// <summary>
    /// Represents a single particle
    /// </summary>
    public class Particle : SceneItem
    {
        /// <summary>
        /// The color of this particle
        /// </summary>
        private Vector4 color;

        private Vector4 startColor;
        private Vector4 endColor;
        private TimeSpan endTime = TimeSpan.Zero;
        private TimeSpan lifetime;

        #region Properties
        public Vector4 Color
        {
            get
            {
                return color;
            }
        }
        #endregion
        /// <summary>
        /// Creates a new particle, with a start and end color and a velocity
        /// </summary>
        /// <param name="game">Instance of the game we are drawing fonts for</param>
        /// <param name="position">Start position</param>
        /// <param name="velocity">Velocity</param>
        /// <param name="startColor">Start Color including alpha</param>
        /// <param name="endColor">End Color including alpha</param>
        /// <param name="lifetime">How long in seconds before it fades and disappears. This time will transition through the start/end color cycle</param>
        public Particle(Game game, Vector2 position, Vector2 velocity, Vector4 startColor, Vector4 endColor, TimeSpan lifetime)
            : base(game, new Vector3(position, 0.0f))
        {
            Velocity = new Vector3(velocity, 0.0f);
            this.startColor = startColor;
            this.endColor = endColor;
            this.lifetime = lifetime;
        }

        /// <summary>
        /// Update all this particle
        /// </summary>
        /// <param name="time">Current game time</param>
        /// <param name="elapsedTime">Elapsed time since last update</param>
        public override void Update(TimeSpan time, TimeSpan elapsedTime)
        {
            //Start the animation 1st time round
            if (endTime == TimeSpan.Zero)
            {
                endTime = time + lifetime;
            }

            //End the animation when its time is due as long as lifet
            if (time > endTime)
            {
                Delete = true;
            }

            //Fade between the colors
            float percentLife = (float)((endTime.TotalSeconds - time.TotalSeconds) / lifetime.TotalSeconds);

            color = Vector4.Lerp(endColor, startColor, percentLife);

            //Do any velocity moving
            base.Update(time, elapsedTime);
        }
    }
}
