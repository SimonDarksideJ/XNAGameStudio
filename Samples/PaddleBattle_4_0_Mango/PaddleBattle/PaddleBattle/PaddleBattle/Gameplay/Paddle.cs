#region File Description
//-----------------------------------------------------------------------------
// Paddle.cs
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
    /// The paddles, either controlled by the player or computer.
    /// </summary>
    public class Paddle : Sprite
    {
        // Is this paddle controlled by the computer?
        private bool isAiPaddle;

        /// <summary>
        /// Initializes a new Paddle.
        /// </summary>
        /// <param name="isAiPaddle">Is this the computer paddle?</param>
        public Paddle(bool isAiPaddle)
        {
            this.isAiPaddle = isAiPaddle;
        }

        /// <summary>
        /// Allows the paddle to load content.
        /// </summary>
        public override void LoadContent(ContentManager content)
        {
            // Determine the texture to load based on whether the paddle is the computer or not
            string asset = isAiPaddle ? "paddle_red" : "paddle_blue";
            texture = content.Load<Texture2D>(asset);

            // Set the collision bounds for accurate collision detection to account
            // for the shadows and empty space in the texture
            collisionBounds = new Rectangle(6, 1, 31, 109);
        }

        /// <summary>
        /// Tests for and reacts to a collision with the ball.
        /// </summary>
        /// <param name="ball">The ball to collide with the paddle</param>
        public bool Collide(Ball ball)
        {
            // Test if the bounds of the ball and paddle are intersecting
            if (Bounds.Intersects(ball.Bounds))
            {
                // Get the magnitude of the ball's velocity and in which direction on the X axis the ball is moving
                float ballVelocityMagnitude = ball.Velocity.Length();
                int ballXDirection = Math.Sign(ball.Velocity.X);

                // Compute a value in the range [-1, 1] indicating where on the paddle the collison
                // happened, where -1 is the top and 1 is the bottom
                float paddleCenter = Bounds.Center.Y;
                float ballCenter = ball.Bounds.Center.Y;
                float offset = (ballCenter - paddleCenter) / (Bounds.Height / 2);

                // Now we generate the ball's resulting angle based on the square of the offset to
                // simulate a curved surface rather than a flat surface, thus giving the ball a different
                // bounce based on where it hits the paddle
                float angle = (offset * offset) * MathHelper.ToRadians(60) * Math.Sign(offset);

                // Use this angle to generate the new direction of the ball
                ball.Velocity = new Vector2(
                    (float)Math.Cos(angle) * ballXDirection,
                    (float)Math.Sin(angle));

                // Test to see if the ball hasn't passed the center of the paddle. If it hasn't, we need to flip
                // the X direction of the velocity so it bounces back towards the other paddle.
                if ((ballXDirection > 0 && ball.Bounds.Center.X < Bounds.Center.X) ||
                    (ballXDirection < 0 && ball.Bounds.Center.X > Bounds.Center.X))
                {
                    ball.Velocity.X *= -1;
                }

                // Scale the velocity back to the original magnitude
                ball.Velocity *= ballVelocityMagnitude;

                // Resolve the collision correctly for the paddle based on the ball's new direction
                if (ball.Velocity.X < 0)
                    ball.Position.X = Bounds.Left - ball.BaseCollisionBounds.Width;
                else
                    ball.Position.X = Bounds.Right - ball.BaseCollisionBounds.X;
                
                return true;
            }

            return false;
        }

        /// <summary>
        /// Allows the AI to track the given ball
        /// </summary>
        public void UpdateAI(TimeSpan elapsedTime, Ball ball)
        {
            // We want to try and put the center of the paddle at the same place as the center of the ball
            float targetY = ball.Position.Y + ball.Bounds.Height / 2 - BaseCollisionBounds.Height / 2;

            // Instead of just putting the paddle there, we want to make it slide into place
            const float speed = 120;
            float delta = targetY - Position.Y;

            // If we're within one frame's speed of reaching the destination, simply clamp to that location
            if (Math.Abs(delta) < speed * (float)elapsedTime.TotalSeconds)
            {
                Position.Y = targetY;
            }

            // Otherwise compute the direction and move in the direction towards the ball
            else
            {
                int direction = Math.Sign(delta);
                Position.Y += direction * speed * (float)elapsedTime.TotalSeconds;
            }

            // Ensure the paddle remains on screen
            ClampToScreen();
        }

        /// <summary>
        /// Clamps the paddle to the screen.
        /// </summary>
        public void ClampToScreen()
        {                
            if (Bounds.Top < 0)
            {
                Position.Y = BaseCollisionBounds.Y;
            }
            else if (Bounds.Bottom > 480)
            {
                Position.Y = 480 - BaseCollisionBounds.Height;
            }
        }
    }
}
