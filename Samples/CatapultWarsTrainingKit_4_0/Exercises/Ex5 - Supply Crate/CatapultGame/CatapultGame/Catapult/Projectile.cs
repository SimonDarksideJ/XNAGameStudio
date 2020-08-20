#region File Description
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
    public enum ProjectileState
    {
        InFlight,
        HitGround,
        Destroyed
    }

    class Projectile : DrawableGameComponent
    {
        #region Fields/Properties
        protected SpriteBatch spriteBatch;
        protected Game curGame;       

        // List of currently active projectiles. This allows projectiles
        // to spawn other projectiles.
        protected List<Projectile> activeProjectiles;

        // Texture name for projectile
        string textureName;

        // Movement related fields
        protected Vector2 projectileInitialVelocity = Vector2.Zero;
        protected Vector2 projectileRotationPosition = Vector2.Zero;

        protected float gravity;
        public virtual float Wind { get; set; }

        protected float flightTime;

        protected float projectileRotation;
        
        // State related fields
        protected bool isAI;


        protected float hitOffset;        
        
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

        Vector2 currentVelocity = Vector2.Zero;
        public Vector2 CurrentVelocity 
        {
            get
            {
                return currentVelocity;
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

        public ProjectileState State { get; private set; }

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

        /// <summary>
        /// This property can be used to set a hit animation for the projectile.
        /// Must be set and manually initialized before the projectile attempts to
        /// draw frames from the hit animation (after its state changes to "HitGround").
        /// </summary>
        public Animation HitAnimation { get; set; }

        /// <summary>
        /// Used to mark whether or not the projectile's hit was handled.
        /// </summary>
        public bool HitHandled { get; set; }
        #endregion        

        #region Initialization
        public Projectile(Game game)
            : base(game)
        {
            curGame = game;
        }

        public Projectile(Game game, SpriteBatch screenSpriteBatch, 
            List<Projectile> activeProjectiles, string textureName, 
            Vector2 startPosition, float groundHitOffset, bool isAI, 
            float gravity)
            : this(game)
        {
            spriteBatch = screenSpriteBatch;
            this.activeProjectiles = activeProjectiles;
            projectileStartPosition = startPosition;
            this.textureName = textureName;
            this.isAI = isAI;
            hitOffset = groundHitOffset;
            this.gravity = gravity;
        }

        public override void Initialize()
        {
            // Load a projectile texture
            projectileTexture = curGame.Content.Load<Texture2D>(textureName);
        }
        #endregion

        #region Render/Update
        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case ProjectileState.InFlight:
                    UpdateProjectileFlight(gameTime);
                    break;
                case ProjectileState.HitGround:
                    UpdateProjectileHit(gameTime);
                    break;
                default:
                    // Nothing to update in other states
                    break;
            }
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This method is used to update the projectile after it has hit the ground.
        /// This allows derived projectile types to alter the projectile's hit
        /// phase more easily.
        /// </summary>
        protected void UpdateProjectileHit(GameTime gameTime)
        {
            if (HitAnimation.IsActive == false)
            {
                State = ProjectileState.Destroyed;
                return;
            }

            HitAnimation.Update();
        }

        /// <summary>
        /// This method is used to update the projectile while it is in flight.
        /// This allows derived projectile types to alter the projectile's flight
        /// phase more easily.
        /// </summary>
        protected virtual void UpdateProjectileFlight(GameTime gameTime)
        {
            UpdateProjectileFlightData(gameTime, Wind, gravity);
        }

        public override void Draw(GameTime gameTime)
        {
            switch (State)
            {
                case ProjectileState.InFlight:
                    spriteBatch.Draw(projectileTexture, projectilePosition, null,
                    Color.White, projectileRotation,
                    new Vector2(projectileTexture.Width / 2,
                                projectileTexture.Height / 2),
                    1.0f, SpriteEffects.None, 0);
                    break;
                case ProjectileState.HitGround:
                    HitAnimation.Draw(spriteBatch, ProjectileHitPosition,
                        SpriteEffects.None);
                    break;
                default:
                    // Nothing to draw in this case
                    break;
            }         

            base.Draw(gameTime);
        }
        #endregion

        #region Public functionality
        /// <summary>
        /// Helper function - calculates the projectile position and velocity
        /// based on time.
        /// </summary>
        /// <param name="gameTime">The time since last calculation</param>
        private void UpdateProjectileFlightData(GameTime gameTime, float wind, float gravity)
        {
            flightTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate new projectile position using standard
            // formulas, taking the wind as a force.
            int direction = isAI ? -1 : 1;

            float previousXPosition = projectilePosition.X;
            float previousYPosition = projectilePosition.Y;

            projectilePosition.X = projectileStartPosition.X + 
                (direction * projectileInitialVelocity.X * flightTime) + 
                0.5f * (8 * wind * (float)Math.Pow(flightTime, 2));

            currentVelocity.X = projectileInitialVelocity.X + 8 * wind * flightTime;

            projectilePosition.Y = projectileStartPosition.Y -
                (projectileInitialVelocity.Y * flightTime) +
                0.5f * (gravity * (float)Math.Pow(flightTime, 2));

            currentVelocity.Y = projectileInitialVelocity.Y - gravity * flightTime;

            // Calculate the projectile rotation
            projectileRotation += MathHelper.ToRadians(projectileInitialVelocity.X * 0.5f);

            // Check if projectile hit the ground or even passed it 
            // (could happen during normal calculation)
            if (projectilePosition.Y >= 332 + hitOffset)
            {
                projectilePosition.X = previousXPosition;
                projectilePosition.Y = previousYPosition;

                ProjectileHitPosition = new Vector2(previousXPosition, 332);

                State = ProjectileState.HitGround;                
            }            
        }

        public void Fire(float velocityX, float velocityY)
        {
            // Set initial projectile velocity
            projectilePosition = projectileStartPosition;
            projectileInitialVelocity.X = velocityX;
            projectileInitialVelocity.Y = velocityY;
            currentVelocity.X = velocityX;
            currentVelocity.Y = velocityY;
            // Reset calculation variables
            flightTime = 0;
            State = ProjectileState.InFlight;
            HitHandled = false;
        }
        #endregion
    }
}
