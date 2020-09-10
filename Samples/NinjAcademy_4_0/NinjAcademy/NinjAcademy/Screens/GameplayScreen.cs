#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.GamerServices;


#endregion

namespace NinjAcademy
{
    class GameplayScreen : GameScreen
    {
        #region Fields/Properties


        bool isUpdating = false;
        bool isGameOver = false;
        bool moveToHighScore = false;

        const string GameOverText = "Game Over";
        Vector2 gameOverTextPosition;

        Random random;

        SpriteFont scoreFont;

        Texture2D backgroundTexture;
        Texture2D roomTexture;
        Texture2D bambooTexture;
        Texture2D bambooTopSliceTexture;
        Texture2D bambooBottomSliceTexture;
        Texture2D bambooLeftSliceTexture;
        Texture2D bambooRightSliceTexture;
        Texture2D targetTexture;
        Texture2D swordSlashTexture;
        Texture2D heartTexture;
        Texture2D emptyHeartTexture;
        AnimationStore animationStore;

        GameConfiguration configuration;

        Rectangle viewport;

        GameScreen[] screensToRemove;

        // Configuration related variables
        GamePhase currentPhase;
        TimeSpan configurationPhaseTimer;

        // Gesture recognition related variables
        Vector2? dragPosition = null;

        // The maximal distance between two drag positions which makes them be considered "close"
        const float CloseDragDistance = 25;

        // The minimal drag distance required for a slash to make a sound
        const float AudibleDragDistance = 10;

        readonly TimeSpan swordSlashCheckInterval = TimeSpan.FromMilliseconds(100);
        TimeSpan swordSlashCheckTimer;

        // Constants
        const int MaxThrowingStars = 10;
        const int MaxSwordSlashes = 3;
        const int MaxConveyerTargets = 10;
        const int MaxFallingTargets = 5;
        const int MaxGoldTargets = 5;
        const int MaxFallingGoldTargets = 2;        
        const int MaxBamboos = 6;
        const int MaxDynamites = 3;
        const int MaxBambooSlices = 5;

        // Game components and related variables
        StaticTextureComponent roomComponent;
        HitPointsComponent hitPointsComponent;
        ScoreComponent scoreComponent;

        int throwingStarIndex = 0;
        ThrowingStar[] throwingStarComponents;

        int swordSlashIndex = 0;
        SwordSlash[] swordSlashComponents;
        Vector2 swordSlashOrigin;
        SwordSlash activeSwordSlash;

        // Define the areas on screen where the targets are exposed to hits
        BoundingBox upperTargetArea;
        BoundingBox middleTargetArea;
        BoundingBox lowerTargetArea;

        Stack<Target> upperTargetComponents;
        TimeSpan upperTargetTimer;
        List<Target> upperTargetsInMotion;

        Stack<Target> middleTargetComponents;
        TimeSpan middleTargetTimer;
        List<Target> middleTargetsInMotion;

        Stack<Target> lowerTargetComponents;
        TimeSpan lowerTargetTimer;
        List<Target> lowerTargetsInMotion;

        Stack<Target> goldTargetComponents;
        List<Target> goldTargetsInMotion;

        int fallingTargetIndex = 0;
        LaunchedComponent[] fallingTargetComponents;

        int fallingGoldTargetIndex = 0;
        LaunchedComponent[] fallingGoldTargetComponents;

        Stack<LaunchedComponent> bambooComponents;
        TimeSpan bambooTimer;
        List<LaunchedComponent> inAirBambooComponents;

        int bambooTopSliceIndex = 0;
        LaunchedComponent[] bambooTopSlices;
        int bambooBottomSliceIndex = 0;
        LaunchedComponent[] bambooBottomSlices;
        int bambooLeftSliceIndex = 0;
        LaunchedComponent[] bambooLeftSlices;
        int bambooRightSliceIndex = 0;
        LaunchedComponent[] bambooRightSlices;

        Stack<LaunchedComponent> dynamiteComponents;
        TimeSpan dynamiteTimer;
        List<LaunchedComponent> inAirDynamiteComponents;

        int explosionIndex = 0;
        DisappearingAnimationComponent[] explosionComponents;

        /// <summary>
        /// The player's current hit points. Can only be used after initializing the gameplay screen.
        /// </summary>
        public int HitPoints
        {
            get
            {
                return hitPointsComponent.CurrentHitPoints;
            }
            private set
            {
                hitPointsComponent.CurrentHitPoints = value;
            }
        }

        /// <summary>
        /// The player's current score. Can only be used after initializing the gameplay screen.
        /// </summary>
        public int Score
        {
            get
            {
                return scoreComponent.Score;
            }
            private set
            {
                scoreComponent.Score = value;
            }
        }

        /// <summary>
        /// Gets the amount of game phases have passed.
        /// </summary>
        public int GamePhasesPassed { get; private set; }

        /// <summary>
        /// Returns the amount of time that passes as part of the current phase.
        /// </summary>
        public TimeSpan ElapsedPhaseTime
        {
            get
            {
                return configurationPhaseTimer;
            }
            private set
            {
                configurationPhaseTimer = value;
            }
        }


        #endregion

        #region Initialization


        public GameplayScreen()
        {
            random = new Random();

            EnabledGestures = GestureType.Tap | GestureType.FreeDrag;
        }

        /// <summary>
        /// Performs lightweight initialization/content loading. Heavy duty content loading is performed as part of the
        /// <see cref="LoadAssets"/> method.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();

            viewport = ScreenManager.GraphicsDevice.Viewport.Bounds;

            Vector2 gameOverDimensions = scoreFont.MeasureString(GameOverText);
            gameOverTextPosition = new Vector2(viewport.Center.X - gameOverDimensions.X / 2,
                viewport.Center.Y - gameOverDimensions.Y / 2);
        }

        /// <summary>
        /// Loads assets required by the gameplay screen.
        /// </summary>
        public void LoadAssets()
        {            
            // Load fonts and perform text calculations
            scoreFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameScreenFont28px");

            // Load required textures
            LoadTextures();

            // Load animations
            animationStore = ScreenManager.Game.Content.Load<AnimationStore>("Textures/Animations");
            animationStore.Initialize(ScreenManager.Game.Content);

            // Load configurations and get the first game phase
            configuration = ScreenManager.Game.Content.Load<GameConfiguration>("Configuration/Configuration");
            // Count the phases passed properly, as the very first phase does not count as a phase that passed
            GamePhasesPassed = -1;
            SwitchConfigurationPhase();

            // Create room overlay
            roomComponent = new StaticTextureComponent(ScreenManager.Game, this, roomTexture, Vector2.Zero);
            roomComponent.DrawOrder = GameConstants.RoomDrawOrder;

            // Initialize the HUD components
            CreateHUDComponents();

            // Create throwing stars and add them to the game components
            CreateThrowingStars();

            // Create sword slashes and add them to the game components
            CreateSwordSlashes();

            // Initialize the target areas
            upperTargetArea = new BoundingBox(new Vector3(GameConstants.UpperTargetAreaTopLeft, 0),
                new Vector3(GameConstants.UpperTargetAreaBottomRight, 0));
            middleTargetArea = new BoundingBox(new Vector3(GameConstants.MiddleTargetAreaTopLeft, 0),
                new Vector3(GameConstants.MiddleTargetAreaBottomRight, 0));
            lowerTargetArea = new BoundingBox(new Vector3(GameConstants.LowerTargetAreaTopLeft, 0),
                new Vector3(GameConstants.LowerTargetAreaBottomRight, 0));

            // Create targets and adds them to the game components
            CreateTargetComponents();

            // Create all components which are tossed from the bottom of the screen
            CreateLaunchedComponents();

            // Create explosion components
            CreateExplosionComponents();

            // Create components which are used to display bamboo slices.
            CreateBambooSliceComponents();

            // Take tombstoning data into account and update the game state
            if (PhoneApplicationService.Current.State.ContainsKey(NinjAcademyGame.GameStateKey))
            {
                GameState gameState = (GameState)PhoneApplicationService.Current.State[NinjAcademyGame.GameStateKey];

                PhoneApplicationService.Current.State.Remove(NinjAcademyGame.GameStateKey);

                Score = gameState.Score;
                HitPoints = gameState.HitPoints;
                ElapsedPhaseTime = gameState.ElapsedPhaseTime;

                for (int i = 0; i < gameState.GamePhasesPassed; i++)
                {
                    SwitchConfigurationPhase();
                }

                if (HitPoints == 0)
                {
                    MarkGameOver();
                }
            }
        }

        /// <summary>
        /// Creates components which are used to display explosions and adds them to the game's component list. 
        /// All components are initially disabled and invisible.
        /// </summary>
        private void CreateExplosionComponents()
        {
            explosionComponents = (DisappearingAnimationComponent[])Array.CreateInstance(
                typeof(DisappearingAnimationComponent), MaxDynamites);

            for (int i = 0; i < MaxDynamites; i++)
            {
                DisappearingAnimationComponent explosion = new DisappearingAnimationComponent(ScreenManager.Game,
                    this, new Animation(animationStore["Explosion"]))
                {
                    DrawOrder = GameConstants.DefaultDrawOrder,
                    Visible = false,
                    Enabled = false
                };

                explosionComponents[i] = explosion;

                ScreenManager.Game.Components.Add(explosion);
            }
        }

        /// <summary>
        /// Creates components which are used to display bamboos after they have been sliced and adds them to the 
        /// game's component list. All components are initially disabled and invisible.
        /// </summary>
        private void CreateBambooSliceComponents()
        {
            SubCreateBambooSliceComponets(ref bambooTopSlices, bambooTopSliceTexture);
            SubCreateBambooSliceComponets(ref bambooBottomSlices, bambooBottomSliceTexture);
            SubCreateBambooSliceComponets(ref bambooLeftSlices, bambooLeftSliceTexture);
            SubCreateBambooSliceComponets(ref bambooRightSlices, bambooRightSliceTexture);
        }

        /// <summary>
        /// Helper method for creating a specific subset of the different types of bamboo slice components. The 
        /// supplied array will be initialized and filled with new components, which will also be added to the game's 
        /// component list.
        /// </summary>
        /// <param name="componentArray">Array which is to contain the components.</param>
        /// <param name="texture">Texture representing the component to add.</param>
        private void SubCreateBambooSliceComponets(ref LaunchedComponent[] componentArray, Texture2D texture)
        {
            componentArray = (LaunchedComponent[])Array.CreateInstance(typeof(LaunchedComponent), MaxBambooSlices);

            for (int i = 0; i < MaxBambooSlices; i++)
            {
                LaunchedComponent slicedCompoent = new LaunchedComponent(ScreenManager.Game, this, texture)
                {
                    DrawOrder = GameConstants.DefaultDrawOrder,
                    Visible = false,
                    Enabled = false,
                    NotifyHeight = GameConstants.OffScreenYCoordinate
                };

                slicedCompoent.DroppedPastHeight += new EventHandler(BambooSliceDroppedOutOfScreen);

                componentArray[i] = slicedCompoent;

                ScreenManager.Game.Components.Add(slicedCompoent);
            }
        }

        /// <summary>
        /// Creates the bamboo, bomb and dynamite components and adds them to the game's component list. All 
        /// components are initially disabled and invisible.
        /// </summary>
        private void CreateLaunchedComponents()
        {
            inAirBambooComponents = new List<LaunchedComponent>(MaxBamboos);
            inAirDynamiteComponents = new List<LaunchedComponent>(MaxDynamites);

            // Create bamboos
            bambooComponents = new Stack<LaunchedComponent>(MaxBamboos);

            for (int i = 0; i < MaxBamboos; i++)
            {
                LaunchedComponent bamboo = new LaunchedComponent(ScreenManager.Game, this, bambooTexture)
                {
                    DrawOrder = GameConstants.DefaultDrawOrder,
                    Visible = false,
                    Enabled = false,
                    NotifyHeight = GameConstants.OffScreenYCoordinate
                };

                bamboo.DroppedPastHeight += new EventHandler(BambooDroppedOutOfScreen);

                bambooComponents.Push(bamboo);

                ScreenManager.Game.Components.Add(bamboo);
            }

            // Create dynamites
            dynamiteComponents = new Stack<LaunchedComponent>(MaxDynamites);

            for (int i = 0; i < MaxDynamites; i++)
            {
                LaunchedComponent dynamite = new LaunchedComponent(ScreenManager.Game, this,
                    new Animation(animationStore["Dynamite"]))
                {
                    DrawOrder = GameConstants.DefaultDrawOrder,
                    Visible = false,
                    Enabled = false,
                    NotifyHeight = GameConstants.OffScreenYCoordinate
                };

                dynamite.DroppedPastHeight += new EventHandler(DynamiteDroppedOutOfScreen);

                dynamiteComponents.Push(dynamite);

                ScreenManager.Game.Components.Add(dynamite);
            }
        }

        /// <summary>
        /// Creates the target components and adds them to the game's component list. All targets are
        /// initially disabled and invisible. This also creates the falling target components.
        /// </summary>
        private void CreateTargetComponents()
        {            
            upperTargetComponents = new Stack<Target>(MaxConveyerTargets);
            middleTargetComponents = new Stack<Target>(MaxConveyerTargets);
            lowerTargetComponents = new Stack<Target>(MaxConveyerTargets);
            goldTargetComponents = new Stack<Target>(MaxGoldTargets);

            upperTargetsInMotion = new List<Target>(MaxConveyerTargets);
            middleTargetsInMotion = new List<Target>(MaxConveyerTargets);
            lowerTargetsInMotion = new List<Target>(MaxConveyerTargets);
            goldTargetsInMotion = new List<Target>(MaxGoldTargets);

            fallingTargetComponents = (LaunchedComponent[])Array.CreateInstance(
                typeof(LaunchedComponent), MaxFallingTargets);
            fallingGoldTargetComponents = (LaunchedComponent[])Array.CreateInstance(
                typeof(LaunchedComponent), MaxFallingGoldTargets);

            // Create conveyer belt targets
            for (int i = 0; i < MaxConveyerTargets; i++)
            {
                upperTargetComponents.Push(GetNewConveyerTarget(TargetPosition.Upper));
                middleTargetComponents.Push(GetNewConveyerTarget(TargetPosition.Middle));
                lowerTargetComponents.Push(GetNewConveyerTarget(TargetPosition.Lower));

                ScreenManager.Game.Components.Add(upperTargetComponents.Peek());
                ScreenManager.Game.Components.Add(middleTargetComponents.Peek());
                ScreenManager.Game.Components.Add(lowerTargetComponents.Peek());
            }

            // Create golden targets
            for (int i = 0; i < MaxGoldTargets; i++)
            {
                Target goldTarget = new Target(ScreenManager.Game, this, new Animation(animationStore["GoldTarget"]))
                {
                    DrawOrder = GameConstants.TargetDrawOrder,
                    Visible = false,
                    Enabled = false,
                    IsGolden = true,
                    Designation = TargetPosition.Anywhere
                };

                goldTarget.FinishedMoving += new EventHandler(TargetFinishedMoving);

                goldTargetComponents.Push(goldTarget);

                ScreenManager.Game.Components.Add(goldTarget);
            }

            // Create falling targets
            for (int i = 0; i < MaxFallingTargets; i++)
            {
                LaunchedComponent fallingTarget = new LaunchedComponent(ScreenManager.Game,
                    this, new Animation(animationStore["FallingTarget"]))
                {
                    DrawOrder = GameConstants.FallingTargetDrawOrder,
                    Visible = false,
                    Enabled = false,                    
                    NotifyHeight = GameConstants.OffScreenYCoordinate
                };

                fallingTarget.DroppedPastHeight += new EventHandler(FallingTargetDroppedOutOfScreen);

                fallingTargetComponents[i] = fallingTarget;

                ScreenManager.Game.Components.Add(fallingTarget);
            }

            // Create golden falling targets
            for (int i = 0; i < MaxFallingGoldTargets; i++)
            {
                LaunchedComponent fallingGoldTarget = new LaunchedComponent(ScreenManager.Game,
                    this, new Animation(animationStore["FallingGoldTarget"]))
                {
                    DrawOrder = GameConstants.FallingTargetDrawOrder,
                    Visible = false,
                    Enabled = false,
                    NotifyHeight = GameConstants.OffScreenYCoordinate
                };

                fallingGoldTarget.DroppedPastHeight += new EventHandler(FallingTargetDroppedOutOfScreen);

                fallingGoldTargetComponents[i] = fallingGoldTarget;

                ScreenManager.Game.Components.Add(fallingGoldTarget);
            }
        }

        /// <summary>
        /// Returns a new component which serves as a target moving on the "conveyer belts".
        /// </summary>
        /// <param name="targetPosition">Target's position on one of the possible conveyer belts.</param>
        /// <returns>A component which serves as a target.</returns>
        private Target GetNewConveyerTarget(TargetPosition targetPosition)
        {
            Target newTarget = new Target(ScreenManager.Game, this, targetTexture)
                {
                    DrawOrder = GameConstants.TargetDrawOrder,
                    Visible = false,
                    Enabled = false,
                    IsGolden = false,
                    Designation = targetPosition
                };

            newTarget.FinishedMoving += new EventHandler(TargetFinishedMoving);

            return newTarget;
        }

        /// <summary>
        /// Creates the sword slash components and adds them to the game's component list. All sword slashes are
        /// initially disabled and invisible.
        /// </summary>
        private void CreateSwordSlashes()
        {
            swordSlashComponents = (SwordSlash[])Array.CreateInstance(typeof(SwordSlash), MaxThrowingStars);

            for (int i = 0; i < MaxSwordSlashes; i++)
            {
                SwordSlash swordSlash = new SwordSlash(ScreenManager.Game, this, swordSlashTexture)
                {
                    DrawOrder = GameConstants.SwordSlashDrawOrder,
                    Visible = false,
                    Enabled = false
                };

                swordSlashComponents[i] = swordSlash;

                ScreenManager.Game.Components.Add(swordSlash);
            }
        }

        /// <summary>
        /// Creates the throwing star components and adds them to the game's component list. All throwing stars are
        /// initially disabled and invisible.
        /// </summary>
        private void CreateThrowingStars()
        {
            throwingStarComponents = (ThrowingStar[])Array.CreateInstance(typeof(ThrowingStar), MaxThrowingStars);

            for (int i = 0; i < MaxThrowingStars; i++)
            {
                ThrowingStar throwingStar = new ThrowingStar(ScreenManager.Game, this,
                    new Animation(animationStore["ThrowingStar"]))
                {
                    DrawOrder = GameConstants.ThrowingStarsDrawOrder,
                    Visible = false,
                    Enabled = false
                };

                throwingStar.FinishedMoving += new EventHandler(ThrowingStarHit);

                throwingStarComponents[i] = throwingStar;

                ScreenManager.Game.Components.Add(throwingStar);
            }
        }

        /// <summary>
        /// Creates the components used to display the game HUD.
        /// </summary>
        private void CreateHUDComponents()
        {
            // Create the component for displaying hit points
            hitPointsComponent = new HitPointsComponent(ScreenManager.Game, heartTexture, emptyHeartTexture)
            {
                TotalHitPoints = configuration.PlayerLives,
                CurrentHitPoints = configuration.PlayerLives,
                DrawOrder = GameConstants.HUDDrawOrder
            };

            // Create the component for displaying the score
            scoreComponent = new ScoreComponent(ScreenManager.Game, scoreFont)
            {
                Score = 0,
                DrawOrder = GameConstants.HUDDrawOrder
            };
        }

        /// <summary>
        /// Loads the gameplay screen's textures.
        /// </summary>
        private void LoadTextures()
        {
            backgroundTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/Backgrounds/gameplayBG");
            roomTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/Backgrounds/room");
            bambooTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/Game Elements/bamboo");
            bambooTopSliceTexture = ScreenManager.Game.Content.Load<Texture2D>(
                "Textures/Game Elements/topSliceHorizontal");
            bambooBottomSliceTexture = ScreenManager.Game.Content.Load<Texture2D>(
                "Textures/Game Elements/bottomSliceHorizontal");
            bambooLeftSliceTexture = ScreenManager.Game.Content.Load<Texture2D>(
                "Textures/Game Elements/leftSliceVertical");
            bambooRightSliceTexture = ScreenManager.Game.Content.Load<Texture2D>(
                "Textures/Game Elements/rightSliceVertical");
            targetTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/Game Elements/target");
            swordSlashTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/Game Elements/slice");
            heartTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/Game Elements/heart");
            emptyHeartTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/Game Elements/emptyHeart");
        }

        /// <summary>
        /// Performs final initialization actions before the screen is displayed.
        /// </summary>
        public void PreDisplayInitialization()
        {
            isUpdating = true;

            ScreenManager.Game.Components.Add(roomComponent);
            ScreenManager.Game.Components.Add(hitPointsComponent);
            ScreenManager.Game.Components.Add(scoreComponent);
        }


        #endregion

        #region Update


        /// <summary>
        /// Performs the game's update logic.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="otherScreenHasFocus">Whether another screen currently has the focus.</param>
        /// <param name="coveredByOtherScreen">Whether the screen is currently covered by another screen.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (!isUpdating)
            {
                return;
            }

            if (moveToHighScore)
            {
                isUpdating = false;

                foreach (GameScreen screen in screensToRemove)
                {
                    screen.ExitScreen();
                }                

                AudioManager.PlayMusic("Menu Music");

                return;
            }

            // Make the currently displayed sword slash fade if necessary 
            if (dragPosition.HasValue)
            {                                
                swordSlashCheckTimer += gameTime.ElapsedGameTime;

                if (swordSlashCheckTimer >= swordSlashCheckInterval)
                {
                    activeSwordSlash.Fade(GameConstants.SwordSlashFadeDuration);

                    dragPosition = null;
                }
            }

            ManageGamePhase(gameTime);
        }

        /// <summary>
        /// Performs all actions necessary as part of the current game phase, such as creating targets on the conveyer
        /// belts, launching bamboos, etc.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        private void ManageGamePhase(GameTime gameTime)
        {
            // Keep track of the phase's progress
            configurationPhaseTimer += gameTime.ElapsedGameTime;

            // Move to the next game phase if necessary
            if (currentPhase.Duration >= TimeSpan.Zero && configurationPhaseTimer >= currentPhase.Duration)
            {
                SwitchConfigurationPhase();
            }

            // Manage the targets
            upperTargetTimer += gameTime.ElapsedGameTime;
            middleTargetTimer += gameTime.ElapsedGameTime;
            lowerTargetTimer += gameTime.ElapsedGameTime;

            upperTargetTimer = ManagePhaseTargets(gameTime, upperTargetTimer,
                currentPhase.TargetAppearanceIntervals[0], currentPhase.TargetAppearanceProbabilities[0],
                upperTargetComponents, GameConstants.UpperTargetOrigin, GameConstants.UpperTargetDestination);
            middleTargetTimer = ManagePhaseTargets(gameTime, middleTargetTimer,
                currentPhase.TargetAppearanceIntervals[1], currentPhase.TargetAppearanceProbabilities[1],
                middleTargetComponents, GameConstants.MiddleTargetOrigin, GameConstants.MiddleTargetDestination);
            lowerTargetTimer = ManagePhaseTargets(gameTime, lowerTargetTimer,
                currentPhase.TargetAppearanceIntervals[2], currentPhase.TargetAppearanceProbabilities[2],
                lowerTargetComponents, GameConstants.LowerTargetOrigin, GameConstants.LowerTargetDestination);

            // Manage the phase's launched components
            ManagePhaseBamboos(gameTime);

            ManagePhaseDynamites(gameTime);
        }

        /// <summary>
        /// Launches dynamite sticks when appropriate according to the current game phase.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        private void ManagePhaseDynamites(GameTime gameTime)
        {
            dynamiteTimer += gameTime.ElapsedGameTime;

            if (dynamiteTimer >= currentPhase.DynamiteAppearanceInterval)
            {
                dynamiteTimer = TimeSpan.Zero;

                if (dynamiteComponents.Count > 0 && random.NextDouble() <= currentPhase.DynamiteAppearanceProbablity)
                {
                    int dynamiteAmount = GetDynamiteAmount();
                    dynamiteAmount = Math.Min(dynamiteAmount, dynamiteComponents.Count);

                    AudioManager.PlaySound("Dynamite");

                    for (int i = 0; i < dynamiteAmount; i++)
                    {
                        LaunchedComponent launchedDynamite = dynamiteComponents.Pop();
                        inAirDynamiteComponents.Add(launchedDynamite);

                        Vector2 launchSpeed = GetLaunchSpeed();

                        launchedDynamite.Launch(GetLaunchPosition(), launchSpeed, GameConstants.LaunchAcceleration,
                            GetLaunchRotation(launchSpeed));
                        launchedDynamite.Enabled = true;
                        launchedDynamite.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the amount of dynamite sticks to launch according to the current game phase.
        /// </summary>
        /// <returns>The number of dynamite sticks that should be launched.</returns>
        private int GetDynamiteAmount()
        {
            double randomNumber = random.NextDouble();

            double totalProbability = 0;

            for (int i = 0; i < currentPhase.DynamiteAmountProbabilities.Length; i++)
            {
                totalProbability += currentPhase.DynamiteAmountProbabilities[i];
                if (randomNumber <= totalProbability)
                {
                    return i + 1;
                }
            }

            return currentPhase.DynamiteAmountProbabilities.Length;
        }

        /// <summary>
        /// Launches a bamboo when appropriate according to the current game phase.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        private void ManagePhaseBamboos(GameTime gameTime)
        {
            bambooTimer += gameTime.ElapsedGameTime;

            if (bambooTimer >= currentPhase.BambooAppearanceInterval)
            {
                bambooTimer = TimeSpan.Zero;

                if (bambooComponents.Count > 0 && random.NextDouble() <= currentPhase.BambooAppearanceProbablity)
                {
                    LaunchedComponent launchedBamboo = bambooComponents.Pop();
                    inAirBambooComponents.Add(launchedBamboo);

                    Vector2 launchSpeed = GetLaunchSpeed();

                    launchedBamboo.Launch(GetLaunchPosition(), launchSpeed, GameConstants.LaunchAcceleration,
                        GetLaunchRotation(launchSpeed));
                    launchedBamboo.Enabled = true;
                    launchedBamboo.Visible = true;
                }
            }
        }


        /// <summary>
        /// Returns a rotation speed based on a launched component's movement speed.
        /// </summary>
        /// <param name="launchSpeed"></param>
        /// <returns></returns>
        private float GetLaunchRotation(Vector2 launchSpeed)
        {
            return 5 * launchSpeed.X / 150;
        }

        /// <summary>
        /// Returns a random launch speed at which to launch an appropriate game element.
        /// </summary>
        /// <returns>A random launch speed at which to launch an appropriate game element.</returns>
        private Vector2 GetLaunchSpeed()
        {
            return new Vector2(-150 + (float)(random.NextDouble() * 300), -500 - (float)(random.NextDouble() * 150));
        }

        /// <summary>
        /// Returns a random position from which to launch an appropriate game element.
        /// </summary>
        /// <returns>A random position from which to launch an appropriate game element.</returns>
        private Vector2 GetLaunchPosition()
        {
            return new Vector2(300 + (float)(random.NextDouble() * 200), 500);
        }

        /// <summary>
        /// Places targets on the conveyer belts as dictated by the current game phase.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="timer">Time passed since the last target was added.</param>
        /// <param name="interval">Time interval after which there is a chance to add a new target.</param>
        /// <param name="probability">The probability to add a target at each interval.</param>
        /// <param name="targetStack">A stack containing available target components.</param>
        /// <param name="origin">Origin of the target's movement.</param>
        /// <param name="destination">Destination of the target's movement.</param>
        /// <returns>The supplied <paramref name="timer"/> value, after adding the elapsed game time to it, or setting
        /// it to zero if <paramref name="interval"/> has elapsed.</returns>
        private TimeSpan ManagePhaseTargets(GameTime gameTime, TimeSpan timer, TimeSpan interval, double probability,
            Stack<Target> targetStack, Vector2 origin, Vector2 destination)
        {
            timer += gameTime.ElapsedGameTime;

            if (timer >= interval)
            {
                timer = TimeSpan.Zero;

                // Place a new target on the conveyer belt according to probability and if we haven't exceeded the
                // maximal on-screen amount
                if (targetStack.Count > 0 && random.NextDouble() <= probability)
                {
                    Target addedTarget;

                    // Place a golden target instead if appropriate and we did not exceed the total amount
                    if (goldTargetComponents.Count > 0 && random.NextDouble() <= currentPhase.GoldTargetProbablity)
                    {
                        addedTarget = goldTargetComponents.Pop();
                    }
                    else
                    {
                        addedTarget = targetStack.Pop();
                    }

                    addedTarget.Move(GameConstants.TargetSpeed, origin, destination);
                    addedTarget.Enabled = true;
                    addedTarget.Visible = true;

                    switch (addedTarget.Designation)
                    {
                        case TargetPosition.Upper:
                            upperTargetsInMotion.Add(addedTarget);
                            break;
                        case TargetPosition.Middle:
                            middleTargetsInMotion.Add(addedTarget);
                            break;
                        case TargetPosition.Lower:
                            lowerTargetsInMotion.Add(addedTarget);
                            break;
                        case TargetPosition.Anywhere:
                            goldTargetsInMotion.Add(addedTarget);
                            break;
                        default:
                            break;
                    }
                }
            }

            return timer;
        }

        /// <summary>
        /// Returns the sword slash component which was least recently used.
        /// </summary>        
        private SwordSlash GetSwordSlash()
        {
            SwordSlash result = swordSlashComponents[swordSlashIndex++];

            if (swordSlashIndex >= MaxSwordSlashes)
            {
                swordSlashIndex = 0;
            }

            return result;
        }

        /// <summary>
        /// Called after performing a sword slash to potentially slice components which appear on screen.
        /// </summary>
        /// <param name="swordSlashOrigin">Sword slash's origin.</param>
        /// <param name="vector2">Sword slash's destination.</param>
        /// <returns>True if any components were sliced and false otherwise.</returns>
        private bool SliceComponents(Vector2 origin, Vector2 destination)
        {
            Line sliceLine = new Line(origin, destination);

            return SliceBamboo(sliceLine) || SliceDynamite(sliceLine);            
        }

        /// <summary>
        /// Checks if any on screen dynamite sticks have been sliced and acts accordingly.
        /// </summary>
        /// <param name="sliceLine">Line depicting the sword slash.</param>
        /// <returns>True if any dynamite sticks were sliced and false otherwise.</returns>
        private bool SliceDynamite(Line sliceLine)
        {
            bool result = false;

            for (int dynamiteIndex = 0; dynamiteIndex < inAirDynamiteComponents.Count; dynamiteIndex++)
            {
                LaunchedComponent dynamite = inAirDynamiteComponents[dynamiteIndex];

                Line[] dynamiteEdges = dynamite.GetEdges();

                for (int edgeIndex = 0; edgeIndex < dynamiteEdges.Length; edgeIndex++)
                {
                    // See if there is intersection with any of the edges
                    if (dynamiteEdges[edgeIndex].GetIntersection(sliceLine).HasValue)
                    {
                        result = true;

                        AudioManager.PlaySound("Explosion");

                        // Swap the current dynamite, which we would like to remove, with the last one and remove the 
                        // end of the list
                        int lastIndex = inAirDynamiteComponents.Count - 1;
                        inAirDynamiteComponents[dynamiteIndex] = inAirDynamiteComponents[lastIndex];
                        inAirDynamiteComponents.RemoveAt(lastIndex);
                        dynamiteIndex--;

                        dynamiteComponents.Push(dynamite);
                        dynamite.Enabled = false;
                        dynamite.Visible = false;

                        ShowExplosion(dynamite.Position);

                        HitPoints = 0;
                        
                        MarkGameOver();                        

                        // We don't need to check the rest of the edges
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Performs actions necessary to transition the game to a state just before ending.
        /// </summary>
        private void MarkGameOver()
        {
            isGameOver = true;

            AudioManager.PlaySound("Game Over");

            // Cause no new components to appear
            currentPhase = new GamePhase()
            {
                BambooAppearanceProbablity = 0,
                BambooAppearanceInterval = TimeSpan.FromSeconds(10),
                Duration = TimeSpan.FromSeconds(-1),
                DynamiteAppearanceInterval = TimeSpan.FromSeconds(10),
                DynamiteAppearanceProbablity = 0,
                TargetAppearanceIntervals =
                    new TimeSpan[] 
                                    { 
                                        TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10) 
                                    },
                TargetAppearanceProbabilities = new double[] { 0, 0, 0 }
            };

            ScreenManager.Game.Components.Add(
                new TextDisplayComponent(ScreenManager.Game, scoreFont)
                {
                    Position = gameOverTextPosition,
                    Text = GameOverText,
                    TextColor = Color.Red,
                    DrawOrder = GameConstants.HUDDrawOrder
                });
        }

        /// <summary>
        /// Checks if any on screen bamboos have been sliced and acts accordingly.
        /// </summary>
        /// <param name="sliceLine">Line depicting the sword slash.</param>
        /// <returns>True if any bamboos were sliced and false otherwise.</returns>
        private bool SliceBamboo(Line sliceLine)
        {
            bool result = false;

            for (int bambooIndex = 0; bambooIndex < inAirBambooComponents.Count; bambooIndex++)
            {
                LaunchedComponent bamboo = inAirBambooComponents[bambooIndex];

                Line[] bambooEdges = bamboo.GetEdges();

                bool slicedRight = false;
                bool slicedLeft = false;
                int edgesSliced = 0;

                if (bambooEdges[0].GetIntersection(sliceLine).HasValue)
                {
                    // Top edge sliced
                    edgesSliced++;
                }
                if (bambooEdges[1].GetIntersection(sliceLine).HasValue)
                {
                    // Right edge sliced
                    slicedRight = true;
                    edgesSliced++;
                }
                if (bambooEdges[2].GetIntersection(sliceLine).HasValue)
                {
                    // Bottom edge sliced
                    edgesSliced++;
                }
                if (bambooEdges[3].GetIntersection(sliceLine).HasValue)
                {
                    // Left edge sliced
                    slicedLeft = true;
                    edgesSliced++;
                }

                // Slicing only 1 edge (or less) will not split the bamboo
                if (edgesSliced >= 2)
                {
                    result = true;

                    AudioManager.PlaySound("Bamboo Slice");

                    if (slicedLeft && slicedRight)
                    {
                        SplitBambooHorizontally(bamboo);
                    }
                    else
                    {
                        SplitBambooVertically(bamboo);
                    }

                    // Swap the current bamboo, which we would like to remove, with the last one and remove the end
                    // of the list
                    int lastIndex = inAirBambooComponents.Count - 1;
                    inAirBambooComponents[bambooIndex] = inAirBambooComponents[lastIndex];
                    inAirBambooComponents.RemoveAt(lastIndex);
                    bambooIndex--;

                    bambooComponents.Push(bamboo);
                    bamboo.Enabled = false;
                    bamboo.Visible = false;

                    scoreComponent.Score = scoreComponent.Score + configuration.PointsPerBamboo;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates two slices of a bamboo after it was hit with the sword from top to bottom. The slices will be 
        /// aligned with the original bamboo, which is assumed to be removed from the display.
        /// </summary>
        /// <param name="bamboo">The bamboo that was sliced.</param>        
        private void SplitBambooVertically(LaunchedComponent bamboo)
        {
            // Find the position from which to launch each bamboo slice
            Vector2 toLeftHalfCenter = new Vector2(-bamboo.Width / 4f, 0);
            toLeftHalfCenter = Vector2.Transform(toLeftHalfCenter, Matrix.CreateRotationZ(bamboo.Rotation));
            Vector2 toRightHalfCenter = -toLeftHalfCenter;

            toLeftHalfCenter += bamboo.Position;
            toRightHalfCenter += bamboo.Position;

            // Initialize the left slice component
            LaunchedComponent leftSlice = bambooLeftSlices[bambooLeftSliceIndex++];

            leftSlice.Visible = true;
            leftSlice.Enabled = true;

            leftSlice.Launch(toLeftHalfCenter, bamboo.Velocity + GetSliceVelocityVariation(), bamboo.Acceleration,
                bamboo.Rotation, bamboo.AngularVelocity * 0.25f);

            if (bambooLeftSliceIndex >= MaxSwordSlashes)
            {
                bambooLeftSliceIndex = 0;
            }

            // Initialize the right slice component
            LaunchedComponent rightSlice = bambooRightSlices[bambooRightSliceIndex++];

            rightSlice.Visible = true;
            rightSlice.Enabled = true;

            rightSlice.Launch(toRightHalfCenter, bamboo.Velocity + GetSliceVelocityVariation(), bamboo.Acceleration,
                bamboo.Rotation, bamboo.AngularVelocity * 0.25f);

            if (bambooRightSliceIndex >= MaxSwordSlashes)
            {
                bambooRightSliceIndex = 0;
            }
        }

        /// <summary>
        /// Creates two slices of a bamboo after it was hit with the sword from side to side. The slices will be 
        /// aligned with the original bamboo, which is assumed to be removed from the display.
        /// </summary>
        /// <param name="bamboo">The bamboo that was sliced.</param>
        private void SplitBambooHorizontally(LaunchedComponent bamboo)
        {
            // Find the position from which to launch each bamboo slice
            Vector2 toTopHalfCenter = new Vector2(0, -bamboo.Height / 4f);
            toTopHalfCenter = Vector2.Transform(toTopHalfCenter, Matrix.CreateRotationZ(bamboo.Rotation));
            Vector2 toBottomHalfCenter = -toTopHalfCenter;

            toTopHalfCenter += bamboo.Position;
            toBottomHalfCenter += bamboo.Position;

            // Initialize the top slice component
            LaunchedComponent topSlice = bambooTopSlices[bambooTopSliceIndex++];

            topSlice.Visible = true;
            topSlice.Enabled = true;

            topSlice.Launch(toTopHalfCenter, bamboo.Velocity + GetSliceVelocityVariation(), bamboo.Acceleration,
                bamboo.Rotation, bamboo.AngularVelocity * 0.25f);

            if (bambooTopSliceIndex >= MaxSwordSlashes)
            {
                bambooTopSliceIndex = 0;
            }

            // Initialize the bottom slice component
            LaunchedComponent bottomSlice = bambooBottomSlices[bambooBottomSliceIndex++];

            bottomSlice.Visible = true;
            bottomSlice.Enabled = true;

            bottomSlice.Launch(toBottomHalfCenter, bamboo.Velocity + GetSliceVelocityVariation(), bamboo.Acceleration,
                bamboo.Rotation, bamboo.AngularVelocity * 0.25f);

            if (bambooBottomSliceIndex >= MaxSwordSlashes)
            {
                bambooBottomSliceIndex = 0;
            }
        }

        /// <summary>
        /// Returns a vector to add to a sliced component's velocity in order to have its slices move at variable
        /// speeds.
        /// </summary>
        /// <returns></returns>
        private Vector2 GetSliceVelocityVariation()
        {
            return new Vector2(-30 + (float)random.NextDouble() * 60, -10 + (float)random.NextDouble() * 20);
        }

        /// <summary>
        /// Exits the gameplay screen and moves to the high-score screen, allowing the user to input his name if his
        /// score is high enough.
        /// </summary>
        private void EndGame()
        {
            if (HighScoreScreen.IsInHighscores(Score))
            {
                AudioManager.PlaySound("HighScore");

                Guide.BeginShowKeyboardInput(PlayerIndex.One, "A new high-score!", "Please enter your name:",
                    "Player", UserSuppliedName, null);
            }
            else
            {
                screensToRemove = ScreenManager.GetScreens();

                ScreenManager.AddScreen(new BackgroundScreen("highScoreBG"), null);
                ScreenManager.AddScreen(new HighScoreScreen(), null);

                moveToHighScore = true;
            }
        }


        #endregion

        #region Input Handling


        /// <summary>
        /// Handles user input.
        /// </summary>
        /// <param name="input">User input information.</param>
        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);

            // Handle input when the game is over
            if (isGameOver)
            {
                if (input.IsPauseGame(null))
                {
                    EndGame();
                }

                for (int i = 0; i < input.Gestures.Count; i++)
                {
                    GestureSample gesture = input.Gestures[i];

                    switch (gesture.GestureType)
                    {
                        case GestureType.Tap:
                            EndGame();
                            break;
                        default:
                            break;
                    }
                }

                return;
            }

            // Handle input when the game is not over
            if (input.IsPauseGame(null))
            {
                PauseGame();
            }

            for (int i = 0; i < input.Gestures.Count; i++)
            {
                GestureSample gesture = input.Gestures[i];

                switch (gesture.GestureType)
                {
                    case GestureType.Tap:
                        // Throw a throwing star
                        AudioManager.PlaySound("Shuriken");

                        throwingStarComponents[throwingStarIndex++].Throw(gesture.Position);

                        if (throwingStarIndex >= MaxThrowingStars)
                        {
                            throwingStarIndex = 0;
                        }

                        break;
                    case GestureType.FreeDrag:
                        HandleDrag(gesture);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Performs necessary operations to translate drag gestures into sword slashes.
        /// </summary>
        /// <param name="gesture">A free drag gesture performed by the user.</param>
        private void HandleDrag(GestureSample gesture)
        {
            float dragDistance = 0;

            // Handle a new drag sequence
            if (!dragPosition.HasValue)
            {
                swordSlashOrigin = gesture.Position;
                swordSlashCheckTimer = TimeSpan.Zero;
                activeSwordSlash = GetSwordSlash();
                activeSwordSlash.Stretch = 0;
                activeSwordSlash.Reset();
            }
            else
            {
                dragDistance = (gesture.Position - dragPosition.Value).Length();

                // Reset the sword slash timer for each significant drag
                if (dragDistance > CloseDragDistance)
                {
                    swordSlashCheckTimer = TimeSpan.Zero;
                }
            }

            dragPosition = gesture.Position;

            activeSwordSlash.PositionSlash(swordSlashOrigin, dragPosition.Value);

            if (!SliceComponents(swordSlashOrigin, dragPosition.Value) && dragDistance > AudibleDragDistance)
            {
                AudioManager.PlaySound("Sword Slash");
            }
        }


        #endregion

        #region Rendering


        /// <summary>
        /// Renders the screen.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, viewport, Color.White);

            spriteBatch.End();
        }        

        /// <summary>
        /// Displays an explosion at the specified position.
        /// </summary>
        /// <param name="position">Position to display an explosion.</param>
        private void ShowExplosion(Vector2 position)
        {
            explosionComponents[explosionIndex++].Show(position);

            if (explosionIndex >= MaxDynamites)
            {
                explosionIndex = 0;
            }
        }


        #endregion

        #region Event Handlers and Related Methods


        /// <summary>
        /// Called when a target safely reaches the other end of the screen.
        /// </summary>
        /// <param name="sender">Component representing the target.</param>
        /// <param name="e">Contains no data.</param>
        void TargetFinishedMoving(object sender, EventArgs e)
        {
            Target target = (Target)sender;
            target.Enabled = false;
            target.Visible = false;

            switch (target.Designation)
            {
                case TargetPosition.Upper:
                    upperTargetComponents.Push(target);
                    upperTargetsInMotion.Remove(target);
                    break;
                case TargetPosition.Middle:
                    middleTargetComponents.Push(target);
                    middleTargetsInMotion.Remove(target);
                    break;
                case TargetPosition.Lower:
                    lowerTargetComponents.Push(target);
                    lowerTargetsInMotion.Remove(target);
                    break;
                case TargetPosition.Anywhere:
                    goldTargetComponents.Push(target);
                    goldTargetsInMotion.Remove(target);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Called when a bamboo falls out of the bottom of the screen.
        /// </summary>
        /// <param name="sender">Component representing the bamboo.</param>
        /// <param name="e">Contains no data.</param>
        void BambooDroppedOutOfScreen(object sender, EventArgs e)
        {
            LaunchedComponent bamboo = (LaunchedComponent)sender;
            bamboo.Enabled = false;
            bamboo.Visible = false;

            bambooComponents.Push(bamboo);
            inAirBambooComponents.Remove(bamboo);

            HitPoints = Math.Max(HitPoints - 1, 0);

            if (HitPoints == 0)
            {
                MarkGameOver();
            }
        }

        /// <summary>
        /// Called when a bamboo slice falls out of the bottom of the screen.
        /// </summary>
        /// <param name="sender">Component representing the bamboo slice.</param>
        /// <param name="e">Contains no data.</param>
        void BambooSliceDroppedOutOfScreen(object sender, EventArgs e)
        {
            LaunchedComponent bambooSlice = (LaunchedComponent)sender;
            bambooSlice.Enabled = false;
            bambooSlice.Visible = false;
        }

        /// <summary>
        /// Called when a dynamite falls out of the bottom of the screen.
        /// </summary>
        /// <param name="sender">Component representing the dynamite.</param>
        /// <param name="e">Contains no data.</param>
        void DynamiteDroppedOutOfScreen(object sender, EventArgs e)
        {
            LaunchedComponent dynamite = (LaunchedComponent)sender;
            dynamite.Enabled = false;
            dynamite.Visible = false;

            dynamiteComponents.Push(dynamite);
            inAirDynamiteComponents.Remove(dynamite);
        }

        /// <summary>
        /// Called when a target falls out of the bottom of the screen.
        /// </summary>
        /// <param name="sender">Component representing the falling target.</param>
        /// <param name="e">Contains no data.</param>
        void FallingTargetDroppedOutOfScreen(object sender, EventArgs e)
        {
            LaunchedComponent fallingTarget = (LaunchedComponent)sender;
            fallingTarget.Enabled = false;
            fallingTarget.Visible = false;
        }

        /// <summary>
        /// Called when a throwing star "hits" (reaches the tap location).
        /// </summary>
        /// <param name="sender">Throwing star involved.</param>
        /// <param name="e">Contains no data.</param>
        void ThrowingStarHit(object sender, EventArgs e)
        {
            ThrowingStar throwingStar = (ThrowingStar)sender;

            Vector3 throwingStarPosition3D = new Vector3(throwingStar.Position, 0);

            // See if any targets were hit
            if (upperTargetArea.Contains(throwingStarPosition3D) == ContainmentType.Contains)
            {
                throwingStar.Enabled = false;
                throwingStar.Visible = false;
                CheckForTargetHits(throwingStarPosition3D, upperTargetsInMotion);
            }
            else if (middleTargetArea.Contains(throwingStarPosition3D) == ContainmentType.Contains)
            {
                throwingStar.Enabled = false;
                throwingStar.Visible = false;
                CheckForTargetHits(throwingStarPosition3D, middleTargetsInMotion);
            }
            else if (lowerTargetArea.Contains(throwingStarPosition3D) == ContainmentType.Contains)
            {
                throwingStar.Enabled = false;
                throwingStar.Visible = false;
                CheckForTargetHits(throwingStarPosition3D, lowerTargetsInMotion);
            }

            if (CheckForTargetHits(throwingStarPosition3D, goldTargetsInMotion))
            {
                throwingStar.Visible = false;
            }

            // The throwing star should simply remain on screen, "lodged" into something
            throwingStar.Enabled = false;
        }

        /// <summary>
        /// Goes over a list of target and see if any of them were hit. If so, updates the score and target component
        /// members as needed. Will also create a falling target animation at the appropriate location.
        /// </summary>
        /// <param name="hitPosition">Position hit by a throwing star.</param>
        /// <param name="targets">List of targets to check for hits.</param>
        /// <returns>True if a target was hit and false otherwise.</returns>
        private bool CheckForTargetHits(Vector3 hitPosition, List<Target> targets)
        {
            for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
            {
                Target target = targets[targetIndex];

                // See if the target was hit
                if (target.CheckHit(hitPosition))
                {
                    if (target.IsGolden)
                    {
                        AudioManager.PlaySound("Shuriken Hit Gold");
                    }
                    else
                    {
                        AudioManager.PlaySound("Shuriken Hit");
                    }

                    // The target no longer needs to be seen or updated
                    target.Enabled = false;
                    target.Visible = false;

                    // Remove the target from the appropriate active list and return it to the available target stack
                    switch (target.Designation)
                    {
                        case TargetPosition.Upper:
                            upperTargetComponents.Push(target);
                            upperTargetsInMotion.Remove(target);
                            break;
                        case TargetPosition.Middle:
                            middleTargetComponents.Push(target);
                            middleTargetsInMotion.Remove(target);
                            break;
                        case TargetPosition.Lower:
                            lowerTargetComponents.Push(target);
                            lowerTargetsInMotion.Remove(target);
                            break;
                        case TargetPosition.Anywhere:
                            goldTargetComponents.Push(target);
                            goldTargetsInMotion.Remove(target);
                            break;
                        default:
                            break;
                    }

                    DropTargetFromPosition(target.Position, target.IsGolden);

                    scoreComponent.Score = scoreComponent.Score + 
                        (target.IsGolden ? configuration.PointsPerGoldTarget : configuration.PointsPerTarget);

                    // One position can only hit one target
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a dropping target animation from a specified position.
        /// </summary>        
        /// <param name="initialPosition">Animation's start position.</param>
        /// <param name="isGolden">Whether the target to drop is golden or not.</param>
        private void DropTargetFromPosition(Vector2 initialPosition, bool isGolden)
        {
            if (!isGolden)
            {
                LaunchedComponent fallingTarget = fallingTargetComponents[fallingTargetIndex++];

                fallingTarget.ResetAnimation();
                fallingTarget.Launch(initialPosition, Vector2.Zero, GameConstants.LaunchAcceleration, 0);

                fallingTarget.Enabled = true;
                fallingTarget.Visible = true;

                if (fallingTargetIndex >= MaxFallingTargets)
                {
                    fallingTargetIndex = 0;
                }
            }
            else
            {
                LaunchedComponent fallingGoldTarget = fallingGoldTargetComponents[fallingGoldTargetIndex++];

                fallingGoldTarget.ResetAnimation();
                fallingGoldTarget.Launch(initialPosition, Vector2.Zero, GameConstants.LaunchAcceleration, 0);

                fallingGoldTarget.Enabled = true;
                fallingGoldTarget.Visible = true;

                if (fallingGoldTargetIndex >= MaxFallingGoldTargets)
                {
                    fallingGoldTargetIndex = 0;
                }
            }
        }

        /// <summary>
        /// Called when the user supplies his name using the guide.
        /// </summary>
        /// <param name="result">Asynchronous operation result.</param>
        private void UserSuppliedName(IAsyncResult result)
        {
            string playerName = Guide.EndShowKeyboardInput(result);

            if (playerName != null)
            {
                if (playerName.Length > 25)
                {
                    playerName = playerName.Substring(0, 25);
                }

                HighScoreScreen.PutHighScore(playerName, Score);
            }

            screensToRemove = ScreenManager.GetScreens();

            ScreenManager.AddScreen(new BackgroundScreen("highScoreBG"), null);
            ScreenManager.AddScreen(new HighScoreScreen(), null);

            moveToHighScore = true;
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Pauses the current game.
        /// </summary>
        public void PauseGame()
        {
            isUpdating = false;

            AudioManager.PauseResumeSounds(false);

            // Go over all components, store the state of components we added, disable them and make them vanish
            for (int i = 0; i < ScreenManager.Game.Components.Count; i++)
            {
                RestorableStateComponent component = ScreenManager.Game.Components[i] as RestorableStateComponent;

                if (component != null)
                {
                    component.StoreState();
                    component.Enabled = false;
                    component.Visible = false;
                }
            }

            ScreenManager.AddScreen(new BackgroundScreen("titlescreenBG"), null);
            ScreenManager.AddScreen(new PauseScreen(this), null);
        }

        /// <summary>
        /// Resumes the game after being paused.
        /// </summary>
        public void ResumeGame()
        {
            isUpdating = true;

            AudioManager.PauseResumeSounds(false);

            // Go over all components and restore the state of components that we have added
            for (int i = 0; i < ScreenManager.Game.Components.Count; i++)
            {
                RestorableStateComponent component = ScreenManager.Game.Components[i] as RestorableStateComponent;

                if (component != null)
                {
                    component.RestoreState();
                }
            }
        }

        /// <summary>
        /// Performs cleanup before the screen exits.
        /// </summary>
        public override void UnloadContent()
        {
            base.UnloadContent();

            // Go over all gameplay screen specific components and remove them from the component list
            for (int i = 0; i < ScreenManager.Game.Components.Count; i++)
            {
                RestorableStateComponent component = ScreenManager.Game.Components[i] as RestorableStateComponent;

                if (component != null)
                {
                    int lastIndex = ScreenManager.Game.Components.Count - 1;
                    ScreenManager.Game.Components.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Switches to the next configuration phase. Assumes that we are not currently in the final configuration
        /// phase. The phase switched to will be removed from the configuration's phase list.
        /// </summary>
        public void SwitchConfigurationPhase()
        {
            if (configuration.Phases.Count > GamePhasesPassed)
            {
                GamePhasesPassed++;

                currentPhase = configuration.Phases[GamePhasesPassed];

                bambooTimer = TimeSpan.Zero;
                dynamiteTimer = TimeSpan.Zero;
                lowerTargetTimer = TimeSpan.Zero;
                middleTargetTimer = TimeSpan.Zero;
                upperTargetTimer = TimeSpan.Zero;                
            }
            else
            {
                MarkGameOver();
            }
        }


        #endregion
    }
}
