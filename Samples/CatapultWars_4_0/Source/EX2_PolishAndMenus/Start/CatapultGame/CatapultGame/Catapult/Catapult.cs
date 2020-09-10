#region File Description
//-----------------------------------------------------------------------------
// Catapult.cs
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
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Devices;
using System.Xml.Linq;
#endregion

namespace CatapultGame
{
    [Flags]
    public enum CatapultState
    {
        Idle = 0x0,
        Aiming = 0x1,
        Firing = 0x2,
        ProjectileFlying = 0x4,
        ProjectileHit = 0x8,
        Hit = 0x10,
        Reset = 0x20,
        Stalling = 0x40
    }

    class Catapult : DrawableGameComponent
    {
        // MARK: Fields start
        // Hold what the game to which the catapult belongs
        CatapultGame curGame = null;

        SpriteBatch spriteBatch;
        Random random;

        const int winScore = 5;

        public bool AnimationRunning { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        // In some cases the game need to start second animation while first
        // animation is still running;
        // this variable define at which frame the second animation should start
        // UNCOMMENT: Dictionary<string, int> splitFrames;

        Texture2D idleTexture;
        // UNCOMMENT: Dictionary<string, Animation> animations;

        SpriteEffects spriteEffects;

        // Projectile
        Projectile projectile;

        string idleTextureName;
        bool isAI;

        // Game constants
        const float gravity = 500f;

        // State of the catapult during its last update
        CatapultState lastUpdateState = CatapultState.Idle;

        // Used to stall animations
        int stallUpdateCycles;
        // MARK: Fields end


        Vector2 catapultPosition;
        public Vector2 Position
        {
            get
            {
                return catapultPosition;
            }
        }

        CatapultState currentState;
        public CatapultState CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }

        float wind;
        public float Wind
        {
            set
            {
                wind = value;
            }
        }

        Player enemy;
        internal Player Enemy
        {
            set
            {
                enemy = value;
            }
        }

        Player self;
        internal Player Self
        {
            set
            {
                self = value;
            }
        }

        // Describes how powerful the current shot being fired is. The more powerful
        // the shot, the further it goes. 0 is the weakest, 1 is the strongest.
        public float ShotStrength { get; set; }

        public float ShotVelocity { get; set; }

        // Used to determine whether or not the game is over
        public bool GameOver { get; set; }

        public Catapult(Game game)
            : base(game)
        {
            curGame = (CatapultGame)game;
        }

        public Catapult(Game game, SpriteBatch screenSpriteBatch,
            string IdleTexture,
            Vector2 CatapultPosition, SpriteEffects SpriteEffect, bool IsAI)
            : this(game)
        {
            idleTextureName = IdleTexture;
            catapultPosition = CatapultPosition;
            spriteEffects = SpriteEffect;
            spriteBatch = screenSpriteBatch;
            isAI = IsAI;

            // splitFrames = new Dictionary<string, int>();
            // animations = new Dictionary<string, Animation>();
        }

        public override void Initialize()
        {
            // Define initial state of the catapult
            currentState = CatapultState.Idle;

            // Load the idle texture
            idleTexture = curGame.Content.Load<Texture2D>(idleTextureName);

            // Initialize the projectile
            Vector2 projectileStartPosition;
            if (isAI)
                projectileStartPosition = new Vector2(630, 340);
            else
                projectileStartPosition = new Vector2(175, 340);

            // TODO: Update hit offset
            projectile = new Projectile(curGame, spriteBatch,
                "Textures/Ammo/rock_ammo", projectileStartPosition,
                60, isAI, gravity);
            projectile.Initialize();

            IsActive = true;
            AnimationRunning = false;
            stallUpdateCycles = 0;

            // Initialize randomizer
            random = new Random();

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            bool isGroundHit;
            CatapultState postUpdateStateChange = 0;

            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            if (!IsActive)
            {
                base.Update(gameTime);
                return;
            }

            switch (currentState)
            {
                case CatapultState.Idle:
                    // Nothing to do
                    break;
                case CatapultState.Aiming:
                    if (lastUpdateState != CatapultState.Aiming)
                    {
                        // TODO: Play sound

                        AnimationRunning = true;
                        if (isAI == true)
                        {
                            // TODO: Play animation
                            stallUpdateCycles = 20;
                        }
                    }

                    // Progress Aiming "animation"
                    if (isAI == false)
                    {
                        // TODO: Play animation
                    }
                    else
                    {
                        // TODO: Play animation
                        // TODO: take �startStall� into account
                        currentState = (true) ?
                            CatapultState.Stalling : CatapultState.Aiming;
                    }
                    break;
                case CatapultState.Stalling:
                    if (stallUpdateCycles-- <= 0)
                    {
                        // We've finished stalling, fire the projectile
                        Fire(ShotVelocity);
                        postUpdateStateChange = CatapultState.Firing;
                    }
                    break;
                case CatapultState.Firing:
                    // Progress Fire animation
                    if (lastUpdateState != CatapultState.Firing)
                    {
                        // TODO: Play Sounds and animate
                    }

                    // TODO: Play animation

                    // TODO: Fire at the appropriate animation frame
                    postUpdateStateChange = currentState |
                        CatapultState.ProjectileFlying;
                    projectile.ProjectilePosition =
                        projectile.ProjectileStartPosition;
                    break;
                case CatapultState.Firing | CatapultState.ProjectileFlying:
                    // Progress Fire animation                    
                    // TODO: Play animation

                    // Update projectile velocity & position in flight
                    projectile.UpdateProjectileFlightData(gameTime, wind, gravity,
                        out isGroundHit);

                    if (isGroundHit)
                    {
                        // Start hit sequence
                        postUpdateStateChange = CatapultState.ProjectileHit;
                        // TODO: Play animation
                    }
                    break;
                case CatapultState.ProjectileFlying:
                    // Update projectile velocity & position in flight
                    projectile.UpdateProjectileFlightData(gameTime, wind, gravity,
                        out isGroundHit);
                    if (isGroundHit)
                    {
                        // Start hit sequence
                        postUpdateStateChange = CatapultState.ProjectileHit;
                        // TODO: Play animation
                    }

                    break;
                case CatapultState.ProjectileHit:
                    // Check hit on ground impact
                    if (!CheckHit())
                    {
                        if (lastUpdateState != CatapultState.ProjectileHit)
                        {
                            // TODO: Vibrate device and play sound
                        }

                        // TODO: Relate to animation when changing state
                        postUpdateStateChange = CatapultState.Reset;

                        // TODO: Update animation
                    }
                    else
                    {
                        // TODO: Vibrate the device
                    }

                    break;
                case CatapultState.Hit:
                    // TODO: only check score when animation is finished                    
                    if (enemy.Score >= winScore)
                    {
                        GameOver = true;
                        break;
                    }

                    postUpdateStateChange = CatapultState.Reset;

                    // TODO: Update animation
                    break;
                case CatapultState.Reset:
                    AnimationRunning = false;
                    break;
                default:
                    break;
            }

            lastUpdateState = currentState;
            if (postUpdateStateChange != 0)
            {
                currentState = postUpdateStateChange;
            }

            base.Update(gameTime);
        }

        private bool CheckHit()
        {
            bool bRes = false;
            // Build a sphere around the projectile
            Vector3 center = new Vector3(projectile.ProjectilePosition, 0);
            BoundingSphere sphere = new BoundingSphere(center,
                Math.Max(projectile.ProjectileTexture.Width / 2,
                projectile.ProjectileTexture.Height / 2));

            // Check Self-Hit - create a bounding box around self
            // TODO: Take asset size into account
            Vector3 min = new Vector3(catapultPosition, 0);
            Vector3 max = new Vector3(catapultPosition + new Vector2(75, 60), 0);
            BoundingBox selfBox = new BoundingBox(min, max);

            // Check enemy - create a bounding box around the enemy
            // TODO: Take asset size into account
            min = new Vector3(enemy.Catapult.Position, 0);
            max = new Vector3(enemy.Catapult.Position + new Vector2(75, 60), 0);
            BoundingBox enemyBox = new BoundingBox(min, max);

            // Check self hit
            if (sphere.Intersects(selfBox) && currentState != CatapultState.Hit)
            {
                // TODO: Play self hit sound

                // Launch hit animation sequence on self
                Hit();
                enemy.Score++;
                bRes = true;
            }
            // Check if enemy was hit
            else if (sphere.Intersects(enemyBox)
                && enemy.Catapult.CurrentState != CatapultState.Hit
                && enemy.Catapult.CurrentState != CatapultState.Reset)
            {
                // TODO: Play enemy hit sound

                // Launch enemy hit animaton
                enemy.Catapult.Hit();
                self.Score++;
                bRes = true;
                currentState = CatapultState.Reset;
            }

            return bRes;
        }

        public void Hit()
        {
            AnimationRunning = true;
            // TODO: Start animations
            currentState = CatapultState.Hit;
        }

        public void Fire(float velocity)
        {
            projectile.Fire(velocity, velocity);
        }


        public override void Draw(GameTime gameTime)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            switch (lastUpdateState)
            {
                case CatapultState.Idle:
                    DrawIdleCatapult();
                    break;
                case CatapultState.Aiming:
                    // TODO: Handle aiming animation
                    break;
                case CatapultState.Firing:
                    // TODO: Handle firing animation
                    break;
                case CatapultState.Firing | CatapultState.ProjectileFlying:
                case CatapultState.ProjectileFlying:
                    // TODO: Handle firing animation
                    projectile.Draw(gameTime);
                    break;
                case CatapultState.ProjectileHit:
                    // Draw the catapult
                    DrawIdleCatapult();

                    // TODO: Handle projectile hit animation
                    break;
                case CatapultState.Hit:
                    // TODO: Handle catapult destruction animation
                    // TODO: Handle explosion animation
                    break;
                case CatapultState.Reset:
                    DrawIdleCatapult();
                    break;
                default:
                    break;
            }

            base.Draw(gameTime);
        }

        private void DrawIdleCatapult()
        {
            spriteBatch.Draw(idleTexture, catapultPosition, null, Color.White,
                0.0f, Vector2.Zero, 1.0f,
                spriteEffects, 0);
        }

    }
}
