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
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using HoneycombRush.GameDebugTools;


#endregion

namespace HoneycombRush
{
    /// <summary>
    /// This is the class the handle the entire game
    /// </summary>
    public class GameplayScreen : GameScreen
    {
        #region Fields/Properties


        SpriteFont font16px;
        SpriteFont font36px;

        Texture2D arrowTexture;
        Texture2D background;
        Texture2D controlstickBoundary;
        Texture2D controlstick;
        Texture2D beehiveTexture;
        Texture2D smokeButton;
        ScoreBar smokeButtonScorebar;

        Vector2 controlstickStartupPosition;
        Vector2 controlstickBoundaryPosition;
        Vector2 smokeButtonPosition;
        Vector2 lastTouchPosition;

        bool isSmokebuttonClicked;
        bool drawArrow;
        bool drawArrowInInterval;
        bool isInMotion;
        bool isAtStartupCountDown;
        bool isLevelEnd;
        bool levelEnded;
        bool isUserWon;
        bool userTapToExit;

        Dictionary<string, Animation> animations;

        int amountOfSoldierBee;
        int amountOfWorkerBee;
        int arrowCounter;

        List<Beehive> beehives = new List<Beehive>();
        List<Bee> bees = new List<Bee>();

        const string SmokeText = "Smoke";

        TimeSpan gameElapsed;
        TimeSpan startScreenTime;

        BeeKeeper beeKeeper;
        HoneyJar jar;
        Vat vat;

        DifficultyMode gameDifficultyLevel;

        public bool IsStarted
        {
            get
            {
                return !isAtStartupCountDown && !levelEnded;
            }
        }

        private bool IsInMotion
        {
            get
            {
                return isInMotion;
            }
            set
            {
                isInMotion = value;
                if (beeKeeper != null)
                {
                    beeKeeper.IsInMotion = isInMotion;
                }
            }
        }

        private int Score
        {
            get
            {
                int highscoreFactor = ConfigurationManager.ModesConfiguration[gameDifficultyLevel].HighScoreFactor;

                return highscoreFactor * (int)gameElapsed.TotalMilliseconds;
            }
        }

        DebugSystem debugSystem;
        bool showDebugInfo = false;
        Rectangle deviceUpperRightCorner = new Rectangle(750, 0, 50, 50);

        #endregion

        #region Initializations


        /// <summary>
        /// Creates a new gameplay screen.
        /// </summary>
        /// <param name="gameDifficultyMode">The desired game difficulty.</param>
        public GameplayScreen(DifficultyMode gameDifficultyMode)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.0);
            startScreenTime = TimeSpan.FromSeconds(3);

            //Loads configuration
            ConfigurationManager.LoadConfiguration(XDocument.Load("Content/Configuration/Configuration.xml"));
            ConfigurationManager.DifficultyMode = gameDifficultyMode;

            gameDifficultyLevel = gameDifficultyMode;
            gameElapsed = ConfigurationManager.ModesConfiguration[gameDifficultyLevel].GameElapsed;

            amountOfSoldierBee = 4;
            amountOfWorkerBee = 16;

            controlstickBoundaryPosition = new Vector2(34, 347);
            smokeButtonPosition = new Vector2(664, 346);
            controlstickStartupPosition = new Vector2(55, 369);

            IsInMotion = false;
            isAtStartupCountDown = true;
            isLevelEnd = false;

            EnabledGestures = GestureType.Tap;
        }


        #endregion

        #region Loading and Unloading


        public override void LoadContent()
        {
            base.LoadContent();

            debugSystem = DebugSystem.Instance;
        }

        /// <summary>
        /// Loads content and assets.
        /// </summary>
        public void LoadAssets()
        {
            // Loads the animation dictionary from an xml file
            animations = new Dictionary<string, Animation>();
            LoadAnimationFromXML();

            // Loads all textures that are required
            LoadTextures();

            // Create all game components
            CreateGameComponents();

            AudioManager.PlayMusic("InGameSong_Loop");
        }

        /// <summary>
        /// Unloads game components which are no longer needed once the game ends.
        /// </summary>
        public override void UnloadContent()
        {
            var componentList = ScreenManager.Game.Components;

            for (int index = 0; index < componentList.Count; index++)
            {
                if (componentList[index] != this && componentList[index] != ScreenManager &&
                    !(componentList[index] is AudioManager))
                {
                    componentList.RemoveAt(index);
                    index--;
                }
            }

            base.UnloadContent();
        }


        #endregion

        #region Update


        /// <summary>
        /// Handle the player's input.
        /// </summary>
        /// <param name="input"></param>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (IsActive)
            {
                if (input == null)
                {
                    throw new ArgumentNullException("input");
                }

                VirtualThumbsticks.Update(input);

                if (input.IsPauseGame(null))
                {
                    PauseCurrentGame();
                }
            }

            if (input.TouchState.Count > 0)
            {
                foreach (TouchLocation touch in input.TouchState)
                {
                    lastTouchPosition = touch.Position;
                }
            }

            isSmokebuttonClicked = false;

            PlayerIndex player;

            if (input.Gestures.Count > 0)
            {
                GestureSample topGesture = input.Gestures[0];

                if (topGesture.GestureType == GestureType.Tap &&
                    deviceUpperRightCorner.Contains(new Point((int)topGesture.Position.X, (int)topGesture.Position.Y)))
                {
                    showDebugInfo = !showDebugInfo;
                }
            }

            // If there was any touch
            if (VirtualThumbsticks.RightThumbstickCenter.HasValue)
            {
                // Button Bounds
                Rectangle buttonRectangle = new Rectangle((int)smokeButtonPosition.X, (int)smokeButtonPosition.Y,
                                                            smokeButton.Width / 2, smokeButton.Height);

                // Touch Bounds
                Rectangle touchRectangle = new Rectangle((int)VirtualThumbsticks.RightThumbstickCenter.Value.X,
                                                        (int)VirtualThumbsticks.RightThumbstickCenter.Value.Y,
                                                        1, 1);
                // If the touch is in the button
                if (buttonRectangle.Contains(touchRectangle) && !beeKeeper.IsCollectingHoney && !beeKeeper.IsStung)
                {
                    isSmokebuttonClicked = true;
                }
            }

            if (input.IsKeyDown(Keys.Space, ControllingPlayer, out player) && !beeKeeper.IsCollectingHoney &&
                !beeKeeper.IsStung)
            {
                isSmokebuttonClicked = true;
            }

            if (input.Gestures.Count > 0)
            {
                if (isLevelEnd)
                {
                    if (input.Gestures[0].GestureType == GestureType.Tap)
                    {
                        userTapToExit = true;
                    }
                }
            }
        }

        /// <summary>
        /// Perform the game's update logic.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="otherScreenHasFocus">Whether or not another screen currently has the focus.</param>
        /// <param name="coveredByOtherScreen">Whether or not this screen is covered by another.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // When the game starts the first thing the user sees is the count down before the game actually begins
            if (isAtStartupCountDown)
            {
                startScreenTime -= gameTime.ElapsedGameTime;
            }

            // Check for and handle a game over
            if (CheckIfCurrentGameFinished())
            {
                base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                return;
            }

            if (!(IsActive && IsStarted))
            {
                base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
                return;
            }            

            // Show all diagnostic counters
            debugSystem.FpsCounter.Visible = showDebugInfo;
            debugSystem.TimeRuler.Visible = showDebugInfo;
            debugSystem.TimeRuler.ShowLog = showDebugInfo;

            gameElapsed -= gameTime.ElapsedGameTime;

            HandleThumbStick();

            HandleSmoke();

            beeKeeper.SetDirection(VirtualThumbsticks.LeftThumbstick);

            HandleCollision(gameTime);

            HandleVatHoneyArrow();

            beeKeeper.DrawOrder = 1;
            int beeKeeperY = (int)(beeKeeper.Position.Y + beeKeeper.Bounds.Height - 2);

            // We want to determine the draw order of the beekeeper,
            // if the beekeeper is under half the height of the beehive 
            // it should be drawn over the beehive.
            foreach (Beehive beehive in beehives)
            {
                if (beeKeeperY > beehive.Bounds.Y)
                {
                    if (beehive.Bounds.Y + beehive.Bounds.Height / 2 < beeKeeperY)
                    {
                        beeKeeper.DrawOrder = Math.Max(beeKeeper.DrawOrder, beehive.Bounds.Y + 1);
                    }
                }
            }

            if (gameElapsed.Minutes == 0 && gameElapsed.Seconds == 10)
            {
                AudioManager.PlaySound("10SecondCountDown");
            }
            if (gameElapsed.Minutes == 0 && gameElapsed.Seconds == 30)
            {
                AudioManager.PlaySound("30SecondWarning");
            }

            // Update the time remaining displayed on the vat
            vat.DrawTimeLeft(gameElapsed);

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        #endregion

        #region Render


        /// <summary>
        /// Draw the game screen.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();

            // Draw the background
            ScreenManager.SpriteBatch.Draw(background,
                new Rectangle(0, 0, ScreenManager.Game.GraphicsDevice.Viewport.Width,
                    ScreenManager.Game.GraphicsDevice.Viewport.Height),
                null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1);

            // Draw count down screen
            if (isAtStartupCountDown)
            {
                DrawStartupString();
            }

            if (IsActive && IsStarted)
            {
                DrawSmokeButton();

                ScreenManager.SpriteBatch.Draw(controlstickBoundary, controlstickBoundaryPosition, Color.White);
                ScreenManager.SpriteBatch.Draw(controlstick, controlstickStartupPosition, Color.White);

                ScreenManager.SpriteBatch.DrawString(font16px, SmokeText, new Vector2(684, 456), Color.White);

                DrawVatHoneyArrow();
            }

            DrawLevelEndIfNecessary();

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Private Methods


        /// <summary>
        /// If the level is over, draws text describing the level's outcome.
        /// </summary>
        private void DrawLevelEndIfNecessary()
        {
            if (isLevelEnd)
            {
                string stringToDisplay = string.Empty;

                if (isUserWon)
                {
                    if (CheckIsInHighScore())
                    {
                        stringToDisplay = "It's a new\nHigh-Score!";
                    }
                    else
                    {
                        stringToDisplay = "You Win!";
                    }
                }
                else
                {
                    stringToDisplay = "Time Is Up!";
                }

                var stringVector = font36px.MeasureString(stringToDisplay);

                ScreenManager.SpriteBatch.DrawString(font36px, stringToDisplay,
                                new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - stringVector.X / 2,
                                            ScreenManager.GraphicsDevice.Viewport.Height / 2 - stringVector.Y / 2),
                                Color.White);
            }
        }

        /// <summary>
        /// Checks if the user's score is a high score.
        /// </summary>
        /// <returns>True if the user has a high score, false otherwise.</returns>
        private bool CheckIsInHighScore()
        {
            // User can be at high score only if he is at Hard level.
            return gameDifficultyLevel == DifficultyMode.Hard && HighScoreScreen.IsInHighscores(Score);
        }

        /// <summary>
        /// Advances to the next screen based on the current difficulty and whether or not the user has won.
        /// </summary>
        /// <param name="isWon">Whether or not the user has won the current level.</param>
        private void MoveToNextScreen(bool isWon)
        {
            ScreenManager.AddScreen(new BackgroundScreen("pauseBackground"), null);

            if (isWon)
            {
                switch (gameDifficultyLevel)
                {
                    case DifficultyMode.Easy:
                    case DifficultyMode.Medium:
                        ScreenManager.AddScreen(
                                new LevelOverScreen("You Finished Level: " + gameDifficultyLevel.ToString(),
                                    ++gameDifficultyLevel), null);
                        break;
                    case DifficultyMode.Hard:
                        ScreenManager.AddScreen(new LevelOverScreen("You Win", null), null);
                        break;
                }
            }
            else
            {
                ScreenManager.AddScreen(new LevelOverScreen("You Lose", null), null);
            }

            AudioManager.StopMusic();
            AudioManager.StopSound("BeeBuzzing_Loop");
        }

        /// <summary>
        /// Pause the game.
        /// </summary>
        private void PauseCurrentGame()
        {
            debugSystem.FpsCounter.Visible = false;
            debugSystem.TimeRuler.Visible = false;
            debugSystem.TimeRuler.ShowLog = false;

            // Pause sounds
            AudioManager.PauseResumeSounds(false);

            // Set pause screen
            ScreenManager.AddScreen(new BackgroundScreen("pauseBackground"), null);
            ScreenManager.AddScreen(new PauseScreen(), null);
        }

        /// <summary>
        /// Loads animation settings from an xml file.
        /// </summary>
        private void LoadAnimationFromXML()
        {
            XDocument doc = XDocument.Load("Content/Textures/AnimationsDefinition.xml");
            XName name = XName.Get("Definition");
            var definitions = doc.Document.Descendants(name);

            // Loop over all definitions in the XML
            foreach (var animationDefinition in definitions)
            {
                // Get the name of the animation
                string animationAlias = animationDefinition.Attribute("Alias").Value;
                Texture2D texture =
                    ScreenManager.Game.Content.Load<Texture2D>(animationDefinition.Attribute("SheetName").Value);

                // Get the frame size (width & height)
                Point frameSize = new Point();
                frameSize.X = int.Parse(animationDefinition.Attribute("FrameWidth").Value);
                frameSize.Y = int.Parse(animationDefinition.Attribute("FrameHeight").Value);

                // Get the frames sheet dimensions
                Point sheetSize = new Point();
                sheetSize.X = int.Parse(animationDefinition.Attribute("SheetColumns").Value);
                sheetSize.Y = int.Parse(animationDefinition.Attribute("SheetRows").Value);

                Animation animation = new Animation(texture, frameSize, sheetSize);

                // Checks for sub-animation definition
                if (animationDefinition.Element("SubDefinition") != null)
                {
                    int startFrame = int.Parse(
                        animationDefinition.Element("SubDefinition").Attribute("StartFrame").Value);

                    int endFrame = int.Parse
                        (animationDefinition.Element("SubDefinition").Attribute("EndFrame").Value);

                    animation.SetSubAnimation(startFrame, endFrame);
                }

                if (animationDefinition.Attribute("Speed") != null)
                {
                    animation.SetFrameInterval(TimeSpan.FromMilliseconds(
                        double.Parse(animationDefinition.Attribute("Speed").Value)));
                }

                // If the definition has an offset defined - it should be  
                // rendered relative to some element/animation

                if (null != animationDefinition.Attribute("OffsetX") &&
                    null != animationDefinition.Attribute("OffsetY"))
                {
                    animation.Offset = new Vector2(int.Parse(animationDefinition.Attribute("OffsetX").Value),
                        int.Parse(animationDefinition.Attribute("OffsetY").Value));
                }

                animations.Add(animationAlias, animation);
            }
        }

        /// <summary>
        /// Create all the game components.
        /// </summary>
        private void CreateGameComponents()
        {
            ScoreBar scoreBar = new ScoreBar(ScreenManager.Game, 0, 100, new Vector2(8, 65), 10, 70, Color.Blue,
                ScoreBar.ScoreBarOrientation.Horizontal, 0, this, true);

            ScreenManager.Game.Components.Add(scoreBar);

            // Create the honey jar
            jar = new HoneyJar(ScreenManager.Game, this, new Vector2(20, 8), scoreBar);
            ScreenManager.Game.Components.Add(jar);

            // Create all the beehives and the bees
            CreateBeehives();

            // Create the smoke gun's score bar
            int totalSmokeAmount = ConfigurationManager.ModesConfiguration[gameDifficultyLevel].TotalSmokeAmount;
            Vector2 smokeButtonPosition = new Vector2(664, 346) + new Vector2(22, smokeButton.Height - 8);

            smokeButtonScorebar = new ScoreBar(ScreenManager.Game, 0, totalSmokeAmount, smokeButtonPosition, 12, 70,
                Color.White, ScoreBar.ScoreBarOrientation.Horizontal, totalSmokeAmount, this, false);

            ScreenManager.Game.Components.Add(smokeButtonScorebar);

            // Creates the BeeKeeper
            beeKeeper = new BeeKeeper(ScreenManager.Game, this);
            beeKeeper.AnimationDefinitions = animations;
            beeKeeper.ThumbStickArea =
                new Rectangle((int)controlstickBoundaryPosition.X, (int)controlstickBoundaryPosition.Y,
                    controlstickBoundary.Width, controlstickBoundary.Height);

            ScreenManager.Game.Components.Add(beeKeeper);

            // Create the vat's score bar
            scoreBar = new ScoreBar(ScreenManager.Game, 0, 300, new Vector2(306, 440), 10, 190, Color.White,
                ScoreBar.ScoreBarOrientation.Horizontal, 0, this, true);
            ScreenManager.Game.Components.Add(scoreBar);

            // Create the vat
            vat = new Vat(ScreenManager.Game, this, ScreenManager.Game.Content.Load<Texture2D>("Textures/vat"),
                new Vector2(294, 355), scoreBar);
            ScreenManager.Game.Components.Add(vat);
            scoreBar.DrawOrder = vat.DrawOrder + 1;
        }

        /// <summary>
        /// Creates all the beehives and bees.
        /// </summary>
        private void CreateBeehives()
        {
            // Init position parameters
            Vector2 scorebarPosition = new Vector2(18, beehiveTexture.Height - 15);
            Vector2[] beehivePositions =
                new Vector2[5] 
                {
                    new Vector2(83, 8), 
                    new Vector2(347, 8), 
                    new Vector2(661, 8), 
                    new Vector2(83, 201), 
                    new Vector2(661, 201)
                };

            // Create the beehives
            for (int beehiveCounter = 0; beehiveCounter < beehivePositions.Length; beehiveCounter++)
            {
                ScoreBar scoreBar = new ScoreBar(ScreenManager.Game, 0, 100, beehivePositions[beehiveCounter] +
                    scorebarPosition, 10, 68, Color.Green, ScoreBar.ScoreBarOrientation.Horizontal, 100, this, false);

                ScreenManager.Game.Components.Add(scoreBar);

                Beehive beehive =
                    new Beehive(ScreenManager.Game, this, beehiveTexture, scoreBar, beehivePositions[beehiveCounter]);

                beehive.AnimationDefinitions = animations;

                ScreenManager.Game.Components.Add(beehive);
                beehives.Add(beehive);
                scoreBar.DrawOrder = beehive.DrawOrder;
            }

            for (int beehiveIndex = 0; beehiveIndex < beehivePositions.Length; beehiveIndex++)
            {
                // Create the Soldier bees
                for (int SoldierBeeCounter = 0; SoldierBeeCounter < amountOfSoldierBee; SoldierBeeCounter++)
                {
                    SoldierBee bee = new SoldierBee(ScreenManager.Game, this, beehives[beehiveIndex]);
                    bee.AnimationDefinitions = animations;
                    ScreenManager.Game.Components.Add(bee);
                    bees.Add(bee);
                }

                // Creates the worker bees
                for (int workerBeeCounter = 0; workerBeeCounter < amountOfWorkerBee; workerBeeCounter++)
                {
                    WorkerBee bee = new WorkerBee(ScreenManager.Game, this, beehives[beehiveIndex]);
                    bee.AnimationDefinitions = animations;
                    ScreenManager.Game.Components.Add(bee);
                    bees.Add(bee);
                }
            }
        }

        /// <summary>
        /// Loads all the necessary textures.
        /// </summary>
        private void LoadTextures()
        {
            beehiveTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/beehive");
            background = ScreenManager.Game.Content.Load<Texture2D>("Textures/Backgrounds/GamePlayBackground");
            controlstickBoundary = ScreenManager.Game.Content.Load<Texture2D>("Textures/controlstickBoundary");
            controlstick = ScreenManager.Game.Content.Load<Texture2D>("Textures/controlstick");
            smokeButton = ScreenManager.Game.Content.Load<Texture2D>("Textures/smokeBtn");
            font16px = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameScreenFont16px");
            arrowTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/arrow");
            font16px = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameScreenFont16px");
            font36px = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/GameScreenFont36px");
        }

        /// <summary>
        /// Handle thumbstick logic
        /// </summary>
        private void HandleThumbStick()
        {
            // Calculate the rectangle of the outer circle of the thumbstick
            Rectangle outerControlstick = new Rectangle(0, (int)controlstickBoundaryPosition.Y - 35,
                controlstickBoundary.Width + 60, controlstickBoundary.Height + 60);

            // Reset the thumbstick position when it is idle
            if (VirtualThumbsticks.LeftThumbstick == Vector2.Zero)
            {
                IsInMotion = false;
                beeKeeper.SetMovement(Vector2.Zero);
                controlstickStartupPosition = new Vector2(55, 369);
            }
            else
            {
                // If not in motion and the touch point is not in the control bounds - there is no movement
                Rectangle touchRectangle = new Rectangle((int)lastTouchPosition.X, (int)lastTouchPosition.Y, 1, 1);

                if (!outerControlstick.Contains(touchRectangle))
                {
                    controlstickStartupPosition = new Vector2(55, 369);
                    IsInMotion = false;
                    return;
                }

                // Move the beekeeper
                SetMotion();

                // Moves the thumbstick's inner circle
                float radius = controlstick.Width / 2 + 35;
                controlstickStartupPosition = new Vector2(55, 369) + (VirtualThumbsticks.LeftThumbstick * radius);
            }
        }

        /// <summary>
        /// Moves the beekeeper.
        /// </summary>
        private void SetMotion()
        {
            // Calculate the beekeeper's location
            Vector2 leftThumbstick = Vector2.Zero;
            leftThumbstick = VirtualThumbsticks.LeftThumbstick;
            Vector2 tempVector = beeKeeper.Position + leftThumbstick * 12f;

            if (tempVector.X < 0 ||
                tempVector.X + beeKeeper.Bounds.Width > ScreenManager.GraphicsDevice.Viewport.Width)
            {
                leftThumbstick.X = 0;
            }
            if (tempVector.Y < 0 ||
                tempVector.Y + beeKeeper.Bounds.Height > ScreenManager.GraphicsDevice.Viewport.Height)
            {
                leftThumbstick.Y = 0;
            }

            if (leftThumbstick == Vector2.Zero)
            {
                isInMotion = false;
            }
            else
            {
                Vector2 beekeeperCalculatedPosition =
                    new Vector2(beeKeeper.CentralCollisionArea.X, beeKeeper.CentralCollisionArea.Y) +
                    leftThumbstick * 12;

                if (!CheckBeehiveCollision(beekeeperCalculatedPosition))
                {
                    beeKeeper.SetMovement(leftThumbstick * 12f);
                    IsInMotion = true;
                }
            }
        }

        /// <summary>
        /// Checks if the beekeeper collides with a beehive.
        /// </summary>
        /// <param name="beekeeperPosition">The beekeeper's position.</param>
        /// <returns>True if the beekeeper collides with a beehive and false otherwise.</returns>
        private bool CheckBeehiveCollision(Vector2 beekeeperPosition)
        {
            // We do not use the beekeeper's collision area property as he has not actually moved at this point and
            // is still in his previous position
            Rectangle beekeeperTempCollisionArea = new Rectangle((int)beekeeperPosition.X, (int)beekeeperPosition.Y,
                beeKeeper.CentralCollisionArea.Width, beeKeeper.CentralCollisionArea.Height);

            foreach (Beehive currentBeehive in beehives)
            {
                if (beekeeperTempCollisionArea.Intersects(currentBeehive.CentralCollisionArea))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check for any of the possible collisions.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        private void HandleCollision(GameTime gameTime)
        {
            bool isCollectingHoney = HandleBeeKeeperBeehiveCollision();

            HandleSmokeBeehiveCollision();

            bool hasCollisionWithVat = HandleVatCollision();

            HandleBeeInteractions(gameTime, hasCollisionWithVat, isCollectingHoney);
        }

        /// <summary>
        /// Handle the interaction of the bees with other game components.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        private void HandleBeeInteractions(GameTime gameTime, bool isBeeKeeperCollideWithVat,
                                        bool isBeeKeeperCollideWithBeehive)
        {
            // Goes over all the bees
            foreach (Bee bee in bees)
            {
                // Check for smoke collisions
                SmokePuff intersectingPuff = beeKeeper.CheckSmokeCollision(bee.Bounds);

                if (intersectingPuff != null)
                {
                    bee.HitBySmoke(intersectingPuff);
                }

                // Check for vat collision
                if (vat.Bounds.HasCollision(bee.Bounds))
                {
                    bee.Collide(vat.Bounds);
                }
                // Check for beekeeper collision
                if (beeKeeper.Bounds.HasCollision(bee.Bounds))
                {
                    if (!bee.IsBeeHit && !isBeeKeeperCollideWithVat && !beeKeeper.IsStung && !beeKeeper.IsFlashing &&
                        !isBeeKeeperCollideWithBeehive)
                    {
                        jar.DecreaseHoneyByPercent(20);
                        beeKeeper.Stung(gameTime.TotalGameTime);
                        AudioManager.PlaySound("HoneyPotBreak");
                        AudioManager.PlaySound("Stung");
                    }

                    bee.Collide(beeKeeper.Bounds);
                }
                // Soldier bee chase logic
                if (bee is SoldierBee)
                {
                    SoldierBee SoldierBee = bee as SoldierBee;
                    SoldierBee.DistanceFromBeeKeeper =
                        (Vector2.Distance(beeKeeper.Bounds.GetVector(), SoldierBee.Bounds.GetVector()));

                    SoldierBee.BeeKeeperVector = beeKeeper.Bounds.GetVector() - SoldierBee.Bounds.GetVector();
                }
            }
        }

        /// <summary>
        /// Handle the beekeeper's collision with the vat component.
        /// </summary>
        /// <returns>True if the beekeeper collides with the vat and false otherwise.</returns>
        private bool HandleVatCollision()
        {
            if (beeKeeper.Bounds.HasCollision(vat.VatDepositArea))
            {
                if (jar.HasHoney && !beeKeeper.IsStung && !beeKeeper.IsDepositingHoney &&
                    VirtualThumbsticks.LeftThumbstick == Vector2.Zero)
                {
                    beeKeeper.StartTransferHoney(4, EndHoneyDeposit);
                }

                return true;
            }

            beeKeeper.EndTransferHoney();
            return false;
        }

        /// <summary>
        /// Handler for finalizing the honey deposit to the vat.
        /// </summary>
        /// <param name="result"></param>
        public void EndHoneyDeposit(IAsyncResult result)
        {
            int HoneyAmount = jar.DecreaseHoneyByPercent(100);
            vat.IncreaseHoney(HoneyAmount);
            AudioManager.StopSound("DepositingIntoVat_Loop");
        }

        /// <summary>
        /// Handle the beekeeper's collision with beehive components.
        /// </summary>
        /// <returns>True if the beekeeper collides with a beehive and false otherwise.</returns>
        /// <remarks>This method is also responsible for allowing bees to regenerate when the beekeeper is not
        /// intersecting with a specific hive.</remarks>
        private bool HandleBeeKeeperBeehiveCollision()
        {
            bool isCollidingWithBeehive = false;

            Beehive collidedBeehive = null;

            // Goes over all the beehives
            foreach (Beehive beehive in beehives)
            {
                // If the beekeeper intersects with the beehive
                if (beeKeeper.Bounds.HasCollision(beehive.Bounds))
                {
                    if (VirtualThumbsticks.LeftThumbstick == Vector2.Zero)
                    {
                        collidedBeehive = beehive;
                        isCollidingWithBeehive = true;
                    }
                }
                else
                {
                    beehive.AllowBeesToGenerate = true;
                }
            }

            if (collidedBeehive != null)
            {
                // The beehive has honey, the jar can carry more honey, and the beekeeper is not stung
                if (collidedBeehive.HasHoney && jar.CanCarryMore && !beeKeeper.IsStung)
                {
                    // Take honey from the beehive and put it in the jar
                    collidedBeehive.DecreaseHoney(1);
                    jar.IncreaseHoney(1);
                    beeKeeper.IsCollectingHoney = true;
                    AudioManager.PlaySound("FillingHoneyPot_Loop");
                }
                else
                {
                    beeKeeper.IsCollectingHoney = false;
                }

                // Bees are not allowed to regenerate while the beekeeper is colliding with their beehive
                isCollidingWithBeehive = true;
                collidedBeehive.AllowBeesToGenerate = false;
            }
            else
            {
                beeKeeper.IsCollectingHoney = false;
                AudioManager.StopSound("FillingHoneyPot_Loop");
            }

            return isCollidingWithBeehive;
        }

        /// <summary>
        /// Handle the smoke puff collision with beehive components.
        /// </summary>
        /// <remarks>Only disables bee regeneration, as it assumes that it will be enabled by 
        /// <see cref="HandleBeeKeeperBeehiveCollision"/></remarks>
        private void HandleSmokeBeehiveCollision()
        {
            foreach (Beehive beehive in beehives)
            {
                foreach (SmokePuff smokePuff in beeKeeper.FiredSmokePuffs)
                {
                    if (beehive.Bounds.HasCollision(smokePuff.CentralCollisionArea))
                    {
                        beehive.AllowBeesToGenerate = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sets an internal value which determines whether or not to display an arrow above the vat.
        /// </summary>
        private void HandleVatHoneyArrow()
        {
            if (jar.HasHoney)
            {
                drawArrow = true;
            }
            else
            {
                drawArrow = false;
            }
        }

        /// <summary>
        /// Handle smoke logic.
        /// </summary>
        private void HandleSmoke()
        {
            // If not currently shooting, refill the gun
            if (!isSmokebuttonClicked)
            {
                smokeButtonScorebar.IncreaseCurrentValue(
                    ConfigurationManager.ModesConfiguration[gameDifficultyLevel].IncreaseAmountSpeed);

                beeKeeper.IsShootingSmoke = false;
            }
            else
            {
                // Check that the gun is not empty
                if (smokeButtonScorebar.CurrentValue <= smokeButtonScorebar.MinValue)
                {
                    beeKeeper.IsShootingSmoke = false;
                }
                else
                {
                    beeKeeper.IsShootingSmoke = true;

                    smokeButtonScorebar.DecreaseCurrentValue(
                        ConfigurationManager.ModesConfiguration[gameDifficultyLevel].DecreaseAmountSpeed);
                }
            }
        }


        /// <summary>
        /// Checks whether the current game is over, and if so performs the necessary actions.
        /// </summary>
        /// <returns>True if the current game is over and false otherwise.</returns>
        private bool CheckIfCurrentGameFinished()
        {
            levelEnded = false;
            isUserWon = vat.CurrentVatCapacity >= vat.MaxVatCapacity;

            // If the vat is full, the player wins
            if (isUserWon || gameElapsed <= TimeSpan.Zero)
            {
                levelEnded = true;
            }

            // if true, game is over
            if (gameElapsed <= TimeSpan.Zero || levelEnded)
            {
                isLevelEnd = true;
                if (userTapToExit)
                {
                    ScreenManager.RemoveScreen(this);

                    if (isUserWon) // True - the user won
                    {
                        // If is in high score, gets is name
                        if (CheckIsInHighScore())
                        {
                            Guide.BeginShowKeyboardInput(PlayerIndex.One,
                                "Player Name", "What is your name (max 15 characters)?", "Player",
                                AfterPlayerEnterName, levelEnded);
                        }

                        AudioManager.PlaySound("Victory");
                    }
                    else
                    {
                        AudioManager.PlaySound("Defeat");
                    }

                    MoveToNextScreen(isUserWon);
                }
            }

            return false;
        }

        /// <summary>
        /// A handler invoked after the user has entered his name.
        /// </summary>
        /// <param name="result"></param>
        private void AfterPlayerEnterName(IAsyncResult result)
        {
            // Get the name entered by the user
            string playerName = Guide.EndShowKeyboardInput(result);

            if (!string.IsNullOrEmpty(playerName))
            {
                // Ensure that it is valid
                if (playerName != null && playerName.Length > 15)
                {
                    playerName = playerName.Substring(0, 15);
                }

                // Puts it in high score
                HighScoreScreen.PutHighScore(playerName, Score);
            }

            // Moves to the next screen
            MoveToNextScreen((bool)result.AsyncState);
        }

        /// <summary>
        /// Draws the arrow in intervals of 20 game update loops.        
        /// </summary>
        private void DrawVatHoneyArrow()
        {
            // If the arrow needs to be drawn, and it is not invisible during the current interval
            if (drawArrow && drawArrowInInterval)
            {
                ScreenManager.SpriteBatch.Draw(arrowTexture, new Vector2(370, 314), Color.White);
                if (arrowCounter == 20)
                {
                    drawArrowInInterval = false;
                    arrowCounter = 0;
                }
                arrowCounter++;
            }
            else
            {
                if (arrowCounter == 20)
                {
                    drawArrowInInterval = true;
                    arrowCounter = 0;
                }
                arrowCounter++;
            }
        }

        /// <summary>
        /// Draws the smoke button.
        /// </summary>
        private void DrawSmokeButton()
        {
            if (isSmokebuttonClicked)
            {
                ScreenManager.SpriteBatch.Draw(
                    smokeButton, new Rectangle((int)smokeButtonPosition.X, (int)smokeButtonPosition.Y, 109, 109),
                    new Rectangle(109, 0, 109, 109), Color.White);
            }
            else
            {
                ScreenManager.SpriteBatch.Draw(
                 smokeButton, new Rectangle((int)smokeButtonPosition.X, (int)smokeButtonPosition.Y, 109, 109),
                 new Rectangle(0, 0, 109, 109), Color.White);
            }
        }

        /// <summary>
        /// Draws the count down string.
        /// </summary>
        private void DrawStartupString()
        {
            // If needed
            if (isAtStartupCountDown)
            {
                string text = string.Empty;

                // If countdown is done
                if (startScreenTime.Seconds == 0)
                {
                    text = "Go!";
                    isAtStartupCountDown = false;
                    AudioManager.PlaySound("BeeBuzzing_Loop", true, .6f);
                }
                else
                {
                    text = startScreenTime.Seconds.ToString();
                }

                Vector2 size = font16px.MeasureString(text);


                Vector2 textPosition = (new Vector2(ScreenManager.GraphicsDevice.Viewport.Width,
                    ScreenManager.GraphicsDevice.Viewport.Height) - size) / 2f;

                ScreenManager.SpriteBatch.DrawString(font36px, text, textPosition, Color.White);
            }
        }


        #endregion
    }
}
