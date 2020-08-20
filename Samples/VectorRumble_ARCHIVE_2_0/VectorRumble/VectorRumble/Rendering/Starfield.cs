#region File Description
//-----------------------------------------------------------------------------
// Starfield.cs
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

namespace VectorRumble
{
    /// <summary>
    /// A starfield represented by single-pixel "stars", with parallax-scrolling.
    /// </summary>
    class Starfield
    {
        #region Constants
        /// <summary>
        /// The distance that the stars move per second.
        /// </summary>
        const float starVelocity = 32f;
        #endregion

        #region Fields
        Random random;

        /// <summary>
        /// The positions of each star.
        /// </summary>
        Vector2[] starPositions;

        /// <summary>
        /// The depth of each star, used for parallax.
        /// </summary>
        byte[] starDepths;

        /// <summary>
        /// The relative "target position" of the starfield, compared with the
        /// currentPosition field to create motion.
        /// </summary>
        Vector2 targetPosition;

        /// <summary>
        /// The relative "current position" of the starfield, compared with the
        /// targetPosition field to create motion.
        /// </summary>
        Vector2 currentPosition;
        #endregion

        #region Initialization
        /// <summary>
        /// Construct a new starfield.
        /// </summary>
        /// <param name="count">The number of stars in the starfield.</param>
        /// <param name="bounds">The bounds of star generation.</param>
        public Starfield(int count, Rectangle bounds)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            random = new Random();
            currentPosition = targetPosition = new Vector2(
                bounds.Left + (bounds.Right - bounds.Left) / 2,
                bounds.Top + (bounds.Bottom - bounds.Top) / 2);
            starPositions = new Vector2[count];
            starDepths = new byte[count];
            for (int i = 0; i < count; i++)
            {
                starPositions[i] = new Vector2(
                    random.Next(bounds.Left, bounds.Right),
                    random.Next(bounds.Top, bounds.Bottom));
                starDepths[i] = (byte)random.Next(1, 255);
            }
        }
        #endregion

        #region Update and Draw
        /// <summary>
        /// Update the starfield.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public void Update(float elapsedTime)
        {
            if (targetPosition != currentPosition)
            {
                Vector2 movement = targetPosition - currentPosition;
                // if the movement is within 1 square in each direction, 
                // then we're close enough
                if (movement.LengthSquared() < 1.414f)  // approxmation of sqrt(2)
                {
                    currentPosition = targetPosition;
                    return;
                }
                // move the current position
                movement = Vector2.Normalize(movement) * (starVelocity * elapsedTime);
                currentPosition += movement;
                // move the stars, scaled by their depth
                for (int i = 0; i < starPositions.Length; i++)
                {
                    starPositions[i] += movement * ((float)starDepths[i] / 255f);
                }
            }
        }

        /// <summary>
        /// Render the starfield.
        /// </summary>
        /// <param name="spriteBatch">The spritebatch that renders the stars.</param>
        /// <param name="spriteTexture">The simple texture used when rendering.</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D spriteTexture)
        {
            if (spriteBatch == null)
            {
                throw new ArgumentNullException("spriteBatch");
            }
            if (spriteTexture == null)
            {
                throw new ArgumentNullException("spriteTexture");
            }
            for (int i = 0; i < starPositions.Length; i++)
            {
                Color starColor = new Color(starDepths[i], starDepths[i],
                    starDepths[i]);
                spriteBatch.Draw(spriteTexture, new Rectangle((int)starPositions[i].X,
                    (int)starPositions[i].Y, 1, 1), starColor);
            }
        }
        #endregion

        #region Interaction
        /// <summary>
        /// Assign the target position for the starfield to scroll to.
        /// </summary>
        /// <param name="targetPosition">The target position.</param>
        public void SetTargetPosition(Vector2 targetPosition)
        {
            this.targetPosition = targetPosition;
        }
        #endregion
    }
}
