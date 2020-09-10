#region File Description
//-----------------------------------------------------------------------------
// Sprite.cs
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
    /// Base class for the Paddle and Ball
    /// </summary>
    public abstract class Sprite
    {
        // The texture for the sprite
        protected Texture2D texture;

        // Our textures contain shadows, so this specifies the actual area that is collideable
        protected Rectangle collisionBounds;

        public Vector2 Position;

        /// <summary>
        /// Gets the collision bounds in texture space for the sprite.
        /// </summary>
        public Rectangle BaseCollisionBounds
        {
            get
            {
                return collisionBounds;
            }
        }

        /// <summary>
        /// Gets the collision bounds in world space for the sprite.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                // Start with our source collision bounds
                Rectangle bounds = collisionBounds;

                // Offset the bounds by the current position
                bounds.X += (int)Position.X;
                bounds.Y += (int)Position.Y;

                return bounds;
            }
        }
        
        /// <summary>
        /// Positions the sprite centered on the given point.
        /// </summary>
        /// <param name="center">The location on which to center the sprite.</param>
        public void CenterAtLocation(Vector2 center)
        {
            Position = center - new Vector2(texture.Width / 2, texture.Height / 2);
        }

        /// <summary>
        /// Allows the sprite to load content.
        /// </summary>
        /// <param name="content">The ContentManager used to load content.</param>
        public virtual void LoadContent(ContentManager content) { }


        /// <summary>
        /// Draws the sprite.
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, Color.White);
        }
    }
}
