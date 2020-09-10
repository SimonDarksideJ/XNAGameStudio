#region File Description
//-----------------------------------------------------------------------------
// Projectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region File Information
//-----------------------------------------------------------------------------
// Projectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace CatapultGame
{
    class Projectile : DrawableGameComponent
    {
        #region Fields/Properties
        SpriteBatch spriteBatch;
        Game curGame;
        Random random;

        // Textures for projectile
        string textureName;
        // Position and speed of projectile
        Vector2 projectileVelocity = Vector2.Zero;
        float projectileInitialVelocityY;
        Vector2 projectileRotationPosition = Vector2.Zero;
        float projectileRotation;
        float flightTime;
        bool isAI;
        float hitOffset;
        float gravity;

        Vector2 projectileStartPosition;
        public Vector2 ProjectileStartPosition
        {
            get
            {
                return projectileStartPosition;
            }
            set
            {
                projectileStartPosition = value;
            }
        }

        Vector2 projectilePosition = Vector2.Zero;
        public Vector2 ProjectilePosition
        {
            get
            {
                return projectilePosition;
            }
            set
            {
                projectilePosition = value;
            }
        }

        /// <summary>
        /// Gets the position where the projectile hit the ground.
        /// Only valid after a hit occurs.
        /// </summary>
        public Vector2 ProjectileHitPosition { get; private set; }

        Texture2D projectileTexture;
        public Texture2D ProjectileTexture
        {
            get
            {
                return projectileTexture;
            }
            set
            {
                projectileTexture = value;
            }
        }
        #endregion

        #region Initialization
        public Projectile(Game game)
            : base(game)
        {
            curGame = game;
            random = new Random();
        }

        public Projectile(Game game, SpriteBatch screenSpriteBatch,
          string TextureName, Vector2 startPosition, float groundHitOffset,
          bool isAi, float Gravity)
            : this(game)
        {
            spriteBatch = screenSpriteBatch;
            projectileStartPosition = startPosition;
            textureName = TextureName;
            isAI = isAi;
            hitOffset = groundHitOffset;
            gravity = Gravity;
        }

        public override void Initialize()
        {
            // Load a projectile texture
            projectileTexture = curGame.Content.Load<Texture2D>(textureName);
        }
        #endregion

        #region Render
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(projectileTexture, projectilePosition, null, 
                Color.White, projectileRotation,
                new Vector2(projectileTexture.Width / 2,
                            projectileTexture.Height / 2),
                1.0f, SpriteEffects.None, 0);

            base.Draw(gameTime);
        }
        #endregion

        #region Public functionality
        /// <summary>
        /// Helper function - calculates the projectile position and velocity based on time.
        /// </summary>
        /// <param name="gameTime">The time since last calculation</param>
        public void UpdateProjectileFlightData(GameTime gameTime, float wind, float gravity, out bool groundHit)
        {
            flightTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate new projectile position using standard
            // formulas, taking the wind as a force.
            int direction = isAI ? -1 : 1;

            float previousXPosition = projectilePosition.X;
            float previousYPosition = projectilePosition.Y;

            projectilePosition.X = projectileStartPosition.X + 
                (direction * projectileVelocity.X * flightTime) + 
                0.5f * (8 * wind * (float)Math.Pow(flightTime, 2));

            projectilePosition.Y = projectileStartPosition.Y -
                (projectileVelocity.Y * flightTime) +
                0.5f * (gravity * (float)Math.Pow(flightTime, 2));            

            // Calculate the projectile rotation
            projectileRotation += MathHelper.ToRadians(projectileVelocity.X * 0.5f);

            // Check if projectile hit the ground or even passed it 
            // (could happen during normal calculation)
            if (projectilePosition.Y >= 332 + hitOffset)
            {
                projectilePosition.X = previousXPosition;
                projectilePosition.Y = previousYPosition;

                ProjectileHitPosition = new Vector2(previousXPosition, 332);

                groundHit = true;
            }
            else
            {
                groundHit = false;
            }
        }

        public void Fire(float velocityX, float velocityY)
        {
            // Set initial projectile velocity
            projectileVelocity.X = velocityX;
            projectileVelocity.Y = velocityY;
            projectileInitialVelocityY = projectileVelocity.Y;
            // Reset calculation variables
            flightTime = 0;
        }
        #endregion
    }
}
