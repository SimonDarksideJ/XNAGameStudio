#region File Description
//-----------------------------------------------------------------------------
// Ball.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PaddleBattle
{
    /// <summary>
    /// The ball bouncing in our game.
    /// </summary>
    public class Ball : Sprite
    {
        /// <summary>
        /// The velocity of the ball.
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// Allows the ball to load content.
        /// </summary>
        public override void LoadContent(ContentManager content)
        {
            // Load the ball texture
            texture = content.Load<Texture2D>("ball");

            // Set the collision bounds for accurate collision detection to account
            // for the shadows and empty space in the texture
            collisionBounds = new Rectangle(4, 4, 28, 28);
        }

        /// <summary>
        /// Checks if the ball is off the screen.
        /// </summary>
        /// <returns>
        /// 1 if the ball went off the right side of the screen, 
        /// -1 if the ball went off the left side of the screen,
        /// 0 if the ball is still on the screen.</returns>
        public int IsOffscreen()
        {
            if (Position.X >= 800)
                return 1;
            if (Position.X <= -texture.Width)
                return -1;
            return 0;
        }

        /// <summary>
        /// Updates the ball.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
            Position += Velocity * (float)elapsedTime.TotalSeconds;
        }
    }
}
