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
    #region Catapult states definition enum
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
    #endregion

    public class Catapult : DrawableGameComponent
    {
        #region Variables/Fields and Properties
        // Hold what the game to which the catapult belongs
        CatapultGame curGame = null;

        SpriteBatch spriteBatch;
        Random random;

        public bool AnimationRunning { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        // In some cases the game need to start second animation while first animation is still running;
        // this variable define at which frame the second animation should start
        Dictionary<string, int> splitFrames;

        Texture2D idleTexture;
        Dictionary<string, Animation> animations;

        SpriteEffects spriteEffects;

        // Projectile
        Projectile projectile;

        string idleTextureName;
        bool isLeftSide;

        // Game constants
        const float gravity = 500f;

        // State of the catapult during its last update
        CatapultState lastUpdateState = CatapultState.Idle;

        // Used to stall animations
        int stallUpdateCycles;

        // Current state of the Catapult
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

        Vector2 catapultPosition;
        public Vector2 Position
        {
            get
            {
                return catapultPosition;
            }
        }

        /// <summary>
        /// Describes how powerful the current shot being fired is. The more powerful
        /// the shot, the further it goes. 0 is the weakest, 1 is the strongest.
        /// </summary>
        public float ShotStrength { get; set; }

        public float ShotVelocity { get; set; }

        /// <summary>
        /// Used to determine whether or not the game is over
        /// </summary>
        public bool GameOver { get; set; }

        const int winScore = 5;
        #endregion

        #region Initialization
        public Catapult(Game game)
            : base(game)
        {
            curGame = (CatapultGame)game;
        }

        public Catapult(Game game, SpriteBatch screenSpriteBatch,
          string IdleTexture,
          Vector2 CatapultPosition, SpriteEffects SpriteEffect, bool isLeftSide)
            : this(game)
        {
            idleTextureName = IdleTexture;
            catapultPosition = CatapultPosition;
            spriteEffects = SpriteEffect;
            spriteBatch = screenSpriteBatch;
            this.isLeftSide = isLeftSide;

            splitFrames = new Dictionary<string, int>();
            animations = new Dictionary<string, Animation>();
        }

        /// <summary>
        /// Function initializes the catapult instance and loads the animations from XML definition sheet
        /// </summary>
        public override void Initialize()
        {
            // Define initial state of the catapult
            IsActive = true;
            AnimationRunning = false;
            currentState = CatapultState.Idle;
            stallUpdateCycles = 0;

            // Load multiple animations form XML definition
            XDocument doc = XDocument.Load("Content/Textures/Catapults/AnimationsDef.xml");
            XName name = XName.Get("Definition");
            var definitions = doc.Document.Descendants(name);

            // Loop over all definitions in XML
            foreach (var animationDefinition in definitions)
            {
                bool? toLoad = null;
                bool val;
                if (bool.TryParse(animationDefinition.Attribute("IsAI").Value, out val))
                    toLoad = val;

                // Check if the animation definition need to be loaded for current catapult
                if (toLoad == isLeftSide || null == toLoad)
                {
                    // Get a name of the animation
                    string animatonAlias = animationDefinition.Attribute("Alias").Value;
                    Texture2D texture =
                        curGame.Content.Load<Texture2D>(animationDefinition.Attribute("SheetName").Value);

                    // Get the frame size (width & height)
                    Point frameSize = new Point();
                    frameSize.X = int.Parse(animationDefinition.Attribute("FrameWidth").Value);
                    frameSize.Y = int.Parse(animationDefinition.Attribute("FrameHeight").Value);

                    // Get the frames sheet dimensions
                    Point sheetSize = new Point();
                    sheetSize.X = int.Parse(animationDefinition.Attribute("SheetColumns").Value);
                    sheetSize.Y = int.Parse(animationDefinition.Attribute("SheetRows").Value);

                    // If definition has a "SplitFrame" - means that other animation should start here - load it
                    if (null != animationDefinition.Attribute("SplitFrame"))
                        splitFrames.Add(animatonAlias,
                            int.Parse(animationDefinition.Attribute("SplitFrame").Value));

                    // Defing animation speed
                    TimeSpan frameInterval = TimeSpan.FromSeconds((float)1 /
                        int.Parse(animationDefinition.Attribute("Speed").Value));

                    Animation animation = new Animation(texture, frameSize, sheetSize);

                    // If definition has an offset defined - means that it should be rendered relatively
                    // to some element/other animation - load it
                    if (null != animationDefinition.Attribute("OffsetX") &&
                      null != animationDefinition.Attribute("OffsetY"))
                    {
                        animation.Offset = new Vector2(int.Parse(animationDefinition.Attribute("OffsetX").Value),
                            int.Parse(animationDefinition.Attribute("OffsetY").Value));
                    }

                    animations.Add(animatonAlias, animation);
                }
            }

            // Load the textures
            idleTexture = curGame.Content.Load<Texture2D>(idleTextureName);

            // Initialize the projectile
            Vector2 projectileStartPosition;
            if (isLeftSide)
                projectileStartPosition = new Vector2(630, 340);
            else
                projectileStartPosition = new Vector2(175, 340);

            projectile = new Projectile(curGame, spriteBatch, "Textures/Ammo/rock_ammo",
              projectileStartPosition, animations["Fire"].FrameSize.Y, isLeftSide, gravity);
            projectile.Initialize();

            // Initialize randomizer
            random = new Random();

            base.Initialize();
        }
        #endregion

        #region Update and Render
        public override void Update(GameTime gameTime)
        {
            bool isGroundHit;
            CatapultState postUpdateStateChange = 0;

            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            // The catapult is inactive, so there is nothing to update
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
                        AudioManager.PlaySound("ropeStretch", true);

                        AnimationRunning = true;
                        animations["Aim"].PlayFromFrameIndex(0);
                        stallUpdateCycles = 20;
                    }

                    UpdateAimAccordingToShotStrength();
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
                        AudioManager.StopSound("ropeStretch");
                        AudioManager.PlaySound("catapultFire");
                        StartFiringFromLastAimPosition();
                    }

                    animations["Fire"].Update();

                    // If in the "split" point of the animation start 
                    // projectile fire sequence
                    if (animations["Fire"].FrameIndex == splitFrames["Fire"])
                    {
                        postUpdateStateChange =
                            currentState | CatapultState.ProjectileFlying;
                        projectile.ProjectilePosition =
                            projectile.ProjectileStartPosition;
                    }
                    break;
                case CatapultState.Firing | CatapultState.ProjectileFlying:
                    // Progress Fire animation                    
                    animations["Fire"].Update();

                    // Update projectile velocity & position in flight
                    projectile.UpdateProjectileFlightData(gameTime, wind,
                        gravity, out isGroundHit);

                    if (isGroundHit)
                    {
                        // Start hit sequence
                        postUpdateStateChange = CatapultState.ProjectileHit;
                        animations["fireMiss"].PlayFromFrameIndex(0);
                    }
                    break;
                case CatapultState.ProjectileFlying:
                    // Update projectile velocity & position in flight
                    projectile.UpdateProjectileFlightData(gameTime, wind,
                        gravity, out isGroundHit);
                    if (isGroundHit)
                    {
                        // Start hit sequence
                        postUpdateStateChange = CatapultState.ProjectileHit;
                        animations["fireMiss"].PlayFromFrameIndex(0);
                    }

                    break;
                case CatapultState.ProjectileHit:
                    // Check hit on ground impact
                    if (!CheckHit())
                    {
                        if (lastUpdateState != CatapultState.ProjectileHit)
                        {
                            VibrateController.Default.Start(
                                TimeSpan.FromMilliseconds(100));
                            // Play hit sound only on a missed hit,
                            // a direct hit will trigger the explosion sound
                            AudioManager.PlaySound("boulderHit");
                        }

                        // Hit animation finished playing
                        if (animations["fireMiss"].IsActive == false)
                        {
                            postUpdateStateChange = CatapultState.Reset;
                        }

                        animations["fireMiss"].Update();
                    }
                    else
                    {
                        // Catapult hit - start longer vibration on any catapult hit 
                        // Remember that the call to "CheckHit" updates the catapult's
                        // state to "Hit"
                        VibrateController.Default.Start(
                            TimeSpan.FromMilliseconds(500));
                    }

                    break;
                case CatapultState.Hit:
                    // Progress hit animation
                    if ((animations["Destroyed"].IsActive == false) &&
                        (animations["hitSmoke"].IsActive == false))
                    {
                        if (enemy.Score >= winScore)
                        {
                            GameOver = true;
                            break;
                        }

                        postUpdateStateChange = CatapultState.Reset;
                    }

                    animations["Destroyed"].Update();
                    animations["hitSmoke"].Update();

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

        /// <summary>
        /// Used to check if the current aim animation frame represents the shot
        /// strength set for the catapult.
        /// </summary>
        /// <returns>True if the current frame represents the shot strength,
        /// false otherwise.</returns>
        private bool AimReachedShotStrength()
        {
            return (animations["Aim"].FrameIndex ==
                (Convert.ToInt32(animations["Aim"].FrameCount * ShotStrength) - 1));
        }

        private void UpdateAimAccordingToShotStrength()
        {
            var aimAnimation = animations["Aim"];
            int frameToDisplay =
                Convert.ToInt32(aimAnimation.FrameCount * ShotStrength);
            aimAnimation.FrameIndex = frameToDisplay;
        }

        /// <summary>
        /// Calculates the frame from which to start the firing animation, 
        /// and activates it.
        /// </summary>
        private void StartFiringFromLastAimPosition()
        {
            int startFrame = animations["Aim"].FrameCount -
                animations["Aim"].FrameIndex;
            animations["Fire"].PlayFromFrameIndex(startFrame);
        }

        public override void Draw(GameTime gameTime)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            // Using the last update state makes sure we do not draw
            // before updating animations properly
            switch (lastUpdateState)
            {
                case CatapultState.Idle:
                    DrawIdleCatapult();
                    break;
                case CatapultState.Aiming:
                case CatapultState.Stalling:
                    animations["Aim"].Draw(spriteBatch, catapultPosition,
                        spriteEffects);
                    break;
                case CatapultState.Firing:
                    animations["Fire"].Draw(spriteBatch, catapultPosition,
                        spriteEffects);
                    break;
                case CatapultState.Firing | CatapultState.ProjectileFlying:
                case CatapultState.ProjectileFlying:
                    animations["Fire"].Draw(spriteBatch, catapultPosition,
                        spriteEffects);

                    projectile.Draw(gameTime);
                    break;
                case CatapultState.ProjectileHit:
                    // Draw the catapult
                    DrawIdleCatapult();

                    // Projectile Hit animation
                    animations["fireMiss"].Draw(spriteBatch,
                        projectile.ProjectileHitPosition, spriteEffects);
                    break;
                case CatapultState.Hit:
                    // Catapult hit animation
                    animations["Destroyed"].Draw(spriteBatch, catapultPosition,
                        spriteEffects);

                    // Projectile smoke animation
                    animations["hitSmoke"].Draw(spriteBatch, catapultPosition,
                        spriteEffects);
                    break;
                case CatapultState.Reset:
                    DrawIdleCatapult();
                    break;
                default:
                    break;
            }

            base.Draw(gameTime);
        }
        #endregion

        #region Hit
        /// <summary>
        /// Start Hit sequence on catapult - could be executed on self or from enemy in case of hit
        /// </summary>
        public void Hit()
        {
            AnimationRunning = true;
            animations["Destroyed"].PlayFromFrameIndex(0);
            animations["hitSmoke"].PlayFromFrameIndex(0);
            currentState = CatapultState.Hit;
        }
        #endregion

        public void Fire(float velocity)
        {
            projectile.Fire(velocity, velocity);
        }

        #region Helper Functions
        /// <summary>
        /// Check if projectile hit some catapult. The possibilities are:
        /// Nothing hit, Hit enemy, Hit self
        /// </summary>
        /// <returns></returns>
        private bool CheckHit()
        {
            bool bRes = false;
            // Build a sphere around a projectile
            Vector3 center = new Vector3(projectile.ProjectilePosition, 0);
            BoundingSphere sphere = new BoundingSphere(center,
                Math.Max(projectile.ProjectileTexture.Width / 2,
                projectile.ProjectileTexture.Height / 2));

            // Check Self-Hit - create a bounding box around self
            Vector3 min = new Vector3(catapultPosition, 0);
            Vector3 max = new Vector3(catapultPosition +
                new Vector2(animations["Fire"].FrameSize.X,
                    animations["Fire"].FrameSize.Y), 0);
            BoundingBox selfBox = new BoundingBox(min, max);

            // Check enemy - create a bounding box around the enemy
            min = new Vector3(enemy.Catapult.Position, 0);
            max = new Vector3(enemy.Catapult.Position +
                new Vector2(animations["Fire"].FrameSize.X,
                    animations["Fire"].FrameSize.Y), 0);
            BoundingBox enemyBox = new BoundingBox(min, max);

            // Check self hit
            if (sphere.Intersects(selfBox) && currentState != CatapultState.Hit)
            {
                AudioManager.PlaySound("catapultExplosion");
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
                AudioManager.PlaySound("catapultExplosion");
                // Launch enemy hit animaton
                enemy.Catapult.Hit();
                self.Score++;
                bRes = true;
                currentState = CatapultState.Reset;
            }

            return bRes;
        }

        /// <summary>
        /// Draw catapult in Idle state
        /// </summary>
        private void DrawIdleCatapult()
        {
            spriteBatch.Draw(idleTexture, catapultPosition, null, Color.White,
              0.0f, Vector2.Zero, 1.0f,
              spriteEffects, 0);
        }
        #endregion

    }
}
