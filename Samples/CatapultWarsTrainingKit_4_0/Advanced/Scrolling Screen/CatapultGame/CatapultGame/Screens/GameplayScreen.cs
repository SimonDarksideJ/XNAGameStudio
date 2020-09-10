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
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Devices.Sensors;
using GameStateManagement;
#endregion

namespace CatapultGame
{
    class GameplayScreen : GameScreen
    {
        #region Fields
        // Texture Members
        Texture2D foregroundTexture;
        Texture2D cloud1Texture;
        Texture2D cloud2Texture;
        Texture2D mountainTexture;
        Texture2D skyTexture;
        Texture2D hudBackgroundTexture;
        Texture2D ammoTypeTexture;
        Texture2D windArrowTexture;
        Texture2D defeatTexture;
        Texture2D victoryTexture;
        SpriteFont hudFont;

        // Rendering members        

        Vector2 playerHUDPosition;
        Vector2 computerHUDPosition;
        Vector2 windArrowPosition;

        // Camera related members        
        const float maxCameraSpeed = 10;

        bool isFlying; // Is the camera flying (after being flicked)
        Vector2 flightDestination; // The destination to which the camera is flying
        Point flightDestinationPoint;

        Vector2 catapultCenterOffset;

        // Input members        
        GestureSample? currentSample;
        GestureSample? prevSample;      
        bool isDragging;
        GestureType? lastGestureType;

        // Gameplay members
        Human player;
        AI computer;
        Vector2 wind;
        bool changeTurn;
        bool isHumanTurn;
        bool isCameraMoving;
        bool gameOver;
        Random random;
        const int minWind = 0;
        const int maxWind = 10;

        const float MinScale = 1f;
        const float MaxScale = 2f;
        #endregion

        #region Properties

        public int CameraMinXOffset
        {
            get
            {
                return -(ScaleToCamera(foregroundTexture.Width) - Viewport.Width);
            }
        }

        public int CameraMaxXOffset
        {
            get
            {
                return 0;
            }
        }

        public int CameraMinYOffset
        {
            get
            {
                return -(ScaleToCamera(foregroundTexture.Height) - Viewport.Height);
            }
        }

        public int CameraMaxYOffset
        {
            get
            {
                return ScaleToCamera(skyTexture.Height) -
                    ScaleToCamera(foregroundTexture.Height);
            }
        }

        Vector2 drawOffset;
        /// <summary>
        /// Used to offset the camera position
        /// </summary>
        public Vector2 DrawOffset
        {
            get
            {
                //return drawOffset * DrawScale;
                return drawOffset;
            }
            set
            {
                player.DrawOffset = value;
                computer.DrawOffset = value;
                drawOffset = value;
            }
        }

        float drawScale;
        /// <summary>
        /// Used to zoom the camera in and out
        /// </summary>
        public float DrawScale
        {
            get
            {
                return drawScale;
            }
            set
            {
                player.DrawScale = value;
                computer.DrawScale = value;
                drawScale = value;
            }
        }

        public Viewport Viewport
        {
            get
            {
                return ScreenManager.GraphicsDevice.Viewport;
            }
        }

        /// <summary>
        /// Returns the coordinate currently at the center of the screen
        /// </summary>
        public Vector2 ScreenCenter
        {
            get
            {
                return new Vector2(Viewport.Width / 2 - DrawOffset.X,
                    Viewport.Height / 2 - DrawOffset.Y);
            }
        }

        Vector2 mountainPosition;
        public Vector2 MountainPosition
        {
            get
            {
                return ScaleToCamera(mountainPosition);
            }
        }

        public Vector2 SkyPosition
        {
            get
            {
                return new Vector2(0, -(ScaleToCamera(skyTexture.Height) -
                    ScaleToCamera(foregroundTexture.Height)));
            }
        }

        Vector2 cloud1Position;
        /// <summary>
        /// Used to return the clouds position for drawing purposes, taking scale into
        /// account.
        /// </summary>
        public Vector2 Cloud1Position
        {
            get
            {
                return cloud1Position * DrawScale;
            }
        }

        Vector2 cloud2Position;
        /// <summary>
        /// Used to return the clouds position for drawing purposes, taking scale into
        /// account.
        /// </summary>
        public Vector2 Cloud2Position
        {
            get
            {
                return cloud2Position * DrawScale;
            }
        }
        #endregion

        #region Initialization
        public GameplayScreen()
        {
            EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete |
                GestureType.Flick | GestureType.Tap |
                GestureType.Pinch | GestureType.PinchComplete;

            random = new Random();
        }
        #endregion

        #region Content Loading/Unloading
        /// <summary>
        /// Loads the game assets and initializes "players"
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();

            // Start the game
            Start();
        }

        public void LoadAssets()
        {
            // Load textures
            foregroundTexture = Load<Texture2D>("Textures/Backgrounds/gameplay_screen");
            cloud1Texture = Load<Texture2D>("Textures/Backgrounds/cloud1");
            cloud2Texture = Load<Texture2D>("Textures/Backgrounds/cloud2");
            mountainTexture = Load<Texture2D>("Textures/Backgrounds/mountain");
            skyTexture = Load<Texture2D>("Textures/Backgrounds/sky");
            defeatTexture = Load<Texture2D>("Textures/Backgrounds/defeat");
            victoryTexture = Load<Texture2D>("Textures/Backgrounds/victory");
            hudBackgroundTexture = Load<Texture2D>("Textures/HUD/hudBackground");
            windArrowTexture = Load<Texture2D>("Textures/HUD/windArrow");
            ammoTypeTexture = Load<Texture2D>("Textures/HUD/ammoType");
            // Load font
            hudFont = Load<SpriteFont>("Fonts/HUDFont");

            // Define initial cloud position
            cloud1Position = new Vector2(224 - cloud1Texture.Width, 0);
            cloud2Position = new Vector2(64, 90);

            // Define initial HUD positions
            playerHUDPosition = new Vector2(7, 7);
            computerHUDPosition = new Vector2(613, 7);
            windArrowPosition = new Vector2(345, 46);


            mountainPosition = new Vector2(400, 0);

            isCameraMoving = true;
            catapultCenterOffset = new Vector2(100, 0);

            // Initialize human & AI players
            player = new Human(ScreenManager.Game, ScreenManager.SpriteBatch);
            player.Initialize();
            player.Name = "Player";

            computer = new AI(ScreenManager.Game, ScreenManager.SpriteBatch);
            computer.Initialize();
            computer.Name = "Phone";

            // Identify enemies
            player.Enemy = computer;
            computer.Enemy = player;

            DrawOffset = new Vector2(-400, 0);
            DrawScale = 1f;
            CenterOnPosition(player.Catapult.Position + catapultCenterOffset);
        }
        #endregion

        #region Update
        /// <summary>
        /// Runs one frame of update for the game.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Check it one of the players reached 5 and stop the game
            if ((player.Catapult.GameOver || computer.Catapult.GameOver) &&
                (gameOver == false))
            {
                gameOver = true;

                if (player.Score > computer.Score)
                {
                    AudioManager.PlaySound("gameOver_Win");
                }
                else
                {
                    AudioManager.PlaySound("gameOver_Lose");
                }

                return;
            }

            if (isFlying)
            {
                if (ScreenCenter == flightDestination)
                {
                    isFlying = false;
                }
                else
                {
                    // Find the direction in which we need to move to get from the 
                    // screen center to our flight destination
                    Vector2 flightVector = flightDestination - ScreenCenter;
                    Vector2 flightMovementVector = flightVector;
                    flightMovementVector.Normalize();
                    flightMovementVector *= 10;
                    flightMovementVector *= (float)(0.25 *
                        (2 + Math.Log((flightVector.Length() + 0.2))));

                    if (flightMovementVector.Length() > flightVector.Length())
                    {
                        DrawOffset -= flightVector;
                    }
                    else
                    {
                        DrawOffset -= flightMovementVector;
                    }

                    CorrectScreenPosition(40, 30);
                }
            }
            else
            {
                CorrectScreenPosition(0, 0);
            }

            // If Reset flag raised and both catapults are not animating - 
            // active catapult finished the cycle, new turn!
            if ((player.Catapult.CurrentState == CatapultState.Reset ||
                computer.Catapult.CurrentState == CatapultState.Reset) &&
                !(player.Catapult.AnimationRunning ||
                computer.Catapult.AnimationRunning))
            {
                changeTurn = true;

                if (player.IsActive == true) // Last turn was a human turn?
                {
                    CenterOnPosition(computer.Catapult.Position - catapultCenterOffset);
                    player.IsActive = false;
                    computer.IsActive = true;
                    isHumanTurn = false;
                    player.Catapult.CurrentState = CatapultState.Idle;
                    computer.Catapult.CurrentState = CatapultState.Aiming;
                }
                else //It was an AI turn
                {
                    isCameraMoving = true;
                    player.IsActive = true;
                    computer.IsActive = false;
                    isHumanTurn = true;
                    computer.Catapult.CurrentState = CatapultState.Idle;
                    player.Catapult.CurrentState = CatapultState.Idle;
                }
            }

            if (changeTurn)
            {
                // Update wind
                wind = new Vector2(random.Next(-1, 2),
                    random.Next(minWind, maxWind + 1));

                // Set new wind value to the players and 
                player.Catapult.Wind = computer.Catapult.Wind =
                    wind.X > 0 ? wind.Y : -wind.Y;
                changeTurn = false;
            }

            // Update the players
            player.Update(gameTime);
            computer.Update(gameTime);

            // Updates the clouds position
            UpdateClouds(elapsed);

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
        #endregion

        #region Draw
        /// <summary>
        /// Draw the game world, effects, and HUD
        /// </summary>
        /// <param name="gameTime">The elapsed time since last Draw</param>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();

            // Render all parts of the screen
            DrawBackground();
            DrawComputer(gameTime);
            DrawPlayer(gameTime);
            DrawHud();

            ScreenManager.SpriteBatch.End();
        }
        #endregion

        #region Input
        /// <summary>
        /// Input helper method provided by GameScreen.  Packages up the various input
        /// values for ease of use.
        /// </summary>
        /// <param name="input">The state of the gamepads</param>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (gameOver)
            {
                if (input.IsPauseGame(null))
                {
                    FinishCurrentGame();
                }

                foreach (GestureSample gestureSample in input.Gestures)
                {
                    if (gestureSample.GestureType == GestureType.Tap)
                    {
                        FinishCurrentGame();
                    }
                }

                return;
            }

            if (input.IsPauseGame(null))
            {
                PauseCurrentGame();
            }
            else if (isCameraMoving) // Handle camera movement
            {
                // Read all available gestures
                foreach (GestureSample gestureSample in input.Gestures)
                {
                    switch (gestureSample.GestureType)
                    {
                        case GestureType.FreeDrag:
                            if (CatapultTapped(gestureSample.Position))
                            {
                                // Allow player to fire
                                isCameraMoving = false;
                                CenterOnPosition(player.Catapult.Position +
                                    catapultCenterOffset);
                            }
                            else
                            {
                                isDragging = true;

                                // Move screen according to delta
                                Vector2 newOffset = DrawOffset;
                                newOffset += gestureSample.Delta;
                                DrawOffset = ClampDrawOffset(newOffset);
                            }
                            break;
                        case GestureType.DragComplete:
                            // turn off dragging state
                            ResetDragState();
                            break;
                        case GestureType.Tap:
                            if (isCameraMoving)
                            {
                                isFlying = false;
                            }
                            break;
                        case GestureType.Flick:
                            // Ignore flicks which appear as part of a pinch ending
                            if (lastGestureType != GestureType.PinchComplete)
                            {
                                FlyToPositionNoScale(
                                    ScreenCenter - gestureSample.Delta);
                            }
                            break;
                        case GestureType.Pinch:

                            // Store last drag location
                            if (null == prevSample)
                            {
                                prevSample = gestureSample;
                            }
                            else
                            {
                                prevSample = currentSample;
                            }

                            // save the current gesture sample 
                            currentSample = gestureSample;

                            float currentLength = (currentSample.Value.Position -
                                currentSample.Value.Position2).Length();
                            float previousLength = (prevSample.Value.Position -
                                prevSample.Value.Position2).Length();

                            float scaleChange = (currentLength - previousLength) * 0.05f;

                            Vector2 previousCenter = ScreenCenter;
                            float previousScale = DrawScale;

                            DrawScale += scaleChange;

                            DrawScale = MathHelper.Clamp(DrawScale, MinScale, MaxScale);

                            CenterOnPositionNoScale(previousCenter * DrawScale / previousScale);
                            break;
                        case GestureType.PinchComplete:
                            ResetPinchState();
                            break;
                        default:
                            break;
                    }

                    lastGestureType = gestureSample.GestureType;
                }
            }
            else if (isHumanTurn &&
                (player.Catapult.CurrentState == CatapultState.Idle ||
                    player.Catapult.CurrentState == CatapultState.Aiming))
            {
                // Read all available gestures
                foreach (GestureSample gestureSample in input.Gestures)
                {
                    if (gestureSample.GestureType == GestureType.FreeDrag)
                        isDragging = true;
                    else if (gestureSample.GestureType == GestureType.DragComplete)
                        isDragging = false;

                    player.HandleInput(gestureSample);
                }
            }
        }
        #endregion

        #region Update Helpers
        private void UpdateClouds(float elapsedTime)
        {
            // Move the clouds according to the wind
            int windDirection = wind.X > 0 ? 1 : -1;

            cloud1Position += new Vector2(24.0f, 0.0f) * elapsedTime *
                windDirection * wind.Y;
            if (cloud1Position.X > foregroundTexture.Width)
                cloud1Position.X = -cloud1Texture.Width * 2.0f;
            else if (cloud1Position.X < -cloud1Texture.Width * 2.0f)
                cloud1Position.X = foregroundTexture.Width;

            cloud2Position += new Vector2(16.0f, 0.0f) * elapsedTime *
                windDirection * wind.Y;
            if (cloud2Position.X > foregroundTexture.Width)
                cloud2Position.X = -cloud2Texture.Width * 2.0f;
            else if (cloud2Position.X < -cloud2Texture.Width * 2.0f)
                cloud2Position.X = foregroundTexture.Width;
        }
        #endregion

        #region Draw Helpers
        /// <summary>
        /// Draws the player's catapult
        /// </summary>
        void DrawPlayer(GameTime gameTime)
        {
            if (!gameOver)
                player.Draw(gameTime);
        }

        /// <summary>
        /// Draws the AI's catapult
        /// </summary>
        void DrawComputer(GameTime gameTime)
        {
            if (!gameOver)
                computer.Draw(gameTime);
        }

        /// <summary>
        /// Draw the sky, clouds, mountains, etc. 
        /// </summary>
        private void DrawBackground()
        {
            // Clear the background
            ScreenManager.Game.GraphicsDevice.Clear(Color.Black);

            // Draw the Sky
            ScreenManager.SpriteBatch.Draw(skyTexture,
                SkyPosition + new Vector2(0, DrawOffset.Y), null, Color.White,
                0, Vector2.Zero, DrawScale, SpriteEffects.None, 0);

            // Draw Cloud #1
            ScreenManager.SpriteBatch.Draw(cloud1Texture,
                cloud1Position * DrawScale + DrawOffset, null,
                Color.White, 0, Vector2.Zero, DrawScale, SpriteEffects.None, 0);

            // Draw the Mountain
            ScreenManager.SpriteBatch.Draw(mountainTexture,
                MountainPosition + DrawOffset, null,
                Color.White, 0, Vector2.Zero, DrawScale, SpriteEffects.None, 0);

            // Draw Cloud #2
            ScreenManager.SpriteBatch.Draw(cloud2Texture,
                cloud2Position * DrawScale + DrawOffset, null,
                Color.White, 0, Vector2.Zero, DrawScale, SpriteEffects.None, 0);

            // Draw the Castle, trees, and foreground 
            ScreenManager.SpriteBatch.Draw(foregroundTexture, DrawOffset, null,
                Color.White, 0, Vector2.Zero, DrawScale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Draw the HUD, which consists of the score elements and the GAME OVER tag.
        /// </summary>
        void DrawHud()
        {
            if (gameOver)
            {
                Texture2D texture;
                if (player.Score > computer.Score)
                {
                    texture = victoryTexture;
                }
                else
                {
                    texture = defeatTexture;
                }

                ScreenManager.SpriteBatch.Draw(
                    texture,
                    new Vector2(ScreenManager.Game.GraphicsDevice.Viewport.Width / 2 - texture.Width / 2,
                                ScreenManager.Game.GraphicsDevice.Viewport.Height / 2 - texture.Height / 2),
                    Color.White);
            }
            else
            {
                // Draw Player Hud
                ScreenManager.SpriteBatch.Draw(hudBackgroundTexture,
                    playerHUDPosition, Color.White);
                ScreenManager.SpriteBatch.Draw(ammoTypeTexture,
                    playerHUDPosition + new Vector2(33, 35), Color.White);
                DrawString(hudFont, player.Score.ToString(),
                    playerHUDPosition + new Vector2(123, 35), Color.White);
                DrawString(hudFont, player.Name,
                    playerHUDPosition + new Vector2(40, 1), Color.Blue);

                // Draw Computer Hud
                ScreenManager.SpriteBatch.Draw(hudBackgroundTexture,
                    computerHUDPosition, Color.White);
                ScreenManager.SpriteBatch.Draw(ammoTypeTexture,
                    computerHUDPosition + new Vector2(33, 35), Color.White);
                DrawString(hudFont, computer.Score.ToString(),
                    computerHUDPosition + new Vector2(123, 35), Color.White);
                DrawString(hudFont, computer.Name,
                    computerHUDPosition + new Vector2(40, 1), Color.Red);

                // Draw Wind direction
                string text = "WIND";
                Vector2 size = hudFont.MeasureString(text);
                Vector2 windarrowScale = new Vector2(wind.Y / 10, 1);
                ScreenManager.SpriteBatch.Draw(windArrowTexture,
                    windArrowPosition, null, Color.White, 0, Vector2.Zero,
                    windarrowScale, wind.X > 0
                    ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

                DrawString(hudFont, text,
                    windArrowPosition - new Vector2(0, size.Y), Color.Black);
                if (wind.Y == 0)
                {
                    text = "NONE";
                    DrawString(hudFont, text, windArrowPosition, Color.Black);
                }

                if (isCameraMoving == false)
                {
                    if (isHumanTurn)
                    {
                        // Prepare human prompt message
                        text = !isDragging ?
                            "Drag Anywhere to Fire" : "Release to Fire!";
                    }
                    else
                    {
                        // Prepare AI message
                        text = "I'll get you yet!";
                    }
                }
                else
                {
                    text = "Drag from your catapult to start shooting";
                }

                size = hudFont.MeasureString(text);

                DrawString(hudFont, text,
                    new Vector2(
                        ScreenManager.GraphicsDevice.Viewport.Width / 2 - size.X / 2,
                        ScreenManager.GraphicsDevice.Viewport.Height - size.Y),
                        Color.Green);
            }
        }

        /// <summary>
        /// A simple helper to draw shadowed text.
        /// </summary>
        void DrawString(SpriteFont font, string text, Vector2 position, Color color)
        {
            ScreenManager.SpriteBatch.DrawString(font, text,
                new Vector2(position.X + 1, position.Y + 1), Color.Black);
            ScreenManager.SpriteBatch.DrawString(font, text, position, color);
        }

        /// <summary>
        /// A simple helper to draw shadowed text.
        /// </summary>
        void DrawString(SpriteFont font, string text, Vector2 position, Color color, float fontScale)
        {
            ScreenManager.SpriteBatch.DrawString(font, text, new Vector2(position.X + 1,
                position.Y + 1), Color.Black, 0, new Vector2(0, font.LineSpacing / 2),
                fontScale, SpriteEffects.None, 0);
            ScreenManager.SpriteBatch.DrawString(font, text, position, color, 0,
                new Vector2(0, font.LineSpacing / 2), fontScale, SpriteEffects.None, 0);
        }
        #endregion

        #region Input Helpers
        /// <summary>
        /// Finish the current game
        /// </summary>
        private void FinishCurrentGame()
        {
            ExitScreen();
        }

        public void ResetDragState()
        {
            isDragging = false;
        }

        public void ResetPinchState()
        {
            prevSample = null;
            currentSample = null;
        }

        /// <summary>
        /// Checks if the user tapped his own catapult.
        /// </summary>
        /// <param name="vector2">The tap location on the device's display.</param>
        /// <returns>True if the player's catapult was clicked, 
        /// false otherwise.</returns>
        private bool CatapultTapped(Vector2 tapPoint)
        {
            tapPoint -= DrawOffset; // Take draw offset into account

            Vector2 catapultPos = player.Catapult.Position * DrawScale;

            Vector3 min = new Vector3(catapultPos, 0);
            Vector3 max = new Vector3(catapultPos +
            new Vector2(player.Catapult.Width * DrawScale,
                player.Catapult.Height * DrawScale), 0);
            BoundingBox catapultBox = new BoundingBox(min, max);
            min = new Vector3(tapPoint.X - ScaleToCamera(10),
                tapPoint.Y + ScaleToCamera(10), 0);
            max = new Vector3(tapPoint.X + ScaleToCamera(10),
                tapPoint.Y - ScaleToCamera(10), 0);
            BoundingBox tapBox = new BoundingBox(min, max);

            return catapultBox.Intersects(tapBox);
        }

        /// <summary>
        /// Pause the current game
        /// </summary>
        private void PauseCurrentGame()
        {
            var pauseMenuBackground = new BackgroundScreen();

            if (isDragging)
            {
                isDragging = false;
                player.Catapult.CurrentState = CatapultState.Idle;
            }

            ScreenManager.AddScreen(pauseMenuBackground, null);
            ScreenManager.AddScreen(new PauseScreen(pauseMenuBackground,
                player, computer), null);
        }
        #endregion

        #region Camera helpers
        /// <summary>
        /// Used to scale an integer value according to the current camera zoom
        /// </summary>
        /// <param name="value">The value to scale.</param>
        /// <returns>The scaled value.</returns>
        public int ScaleToCamera(int value)
        {
            return (int)(value * DrawScale);
        }

        /// <summary>
        /// Used to scale a 2D vector according to the current camera zoom
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <returns>The scaled vector.</returns>
        public Vector2 ScaleToCamera(Vector2 vector)
        {
            return vector * DrawScale;
        }

        /// <summary>
        /// Used to cause the screen to fly back into gameworld bounds if the camera
        /// has panned too far.
        /// </summary>
        /// <param name="xTolerance">The distance from the horizontal edge of the
        /// game world before position correction will occur.</param>
        /// <param name="yTolerance">The distance from the vertical edge of the
        /// game world before position correction will occur.</param>
        private void CorrectScreenPosition(int xTolerance, int yTolerance)
        {
            // If we moved beyond the screen bounds, move back
            Vector2 correctionVector = ScreenCenter;
            bool needCorrection = false;

            if (DrawOffset.X > CameraMaxXOffset + xTolerance)
            {
                //correctionVector.X = Viewport.Width / 2;
                correctionVector.X = CameraMaxXOffset + Viewport.Width / 2;
                needCorrection = true;
            }
            else if (DrawOffset.X < CameraMinXOffset - xTolerance)
            {
                correctionVector.X = ScaleToCamera(foregroundTexture.Width) -
                    Viewport.Width / 2;
                needCorrection = true;
            }

            if (DrawOffset.Y > CameraMaxYOffset + yTolerance)
            {
                correctionVector.Y = SkyPosition.Y + Viewport.Height / 2;
                needCorrection = true;
            }
            else if (DrawOffset.Y < CameraMinYOffset - yTolerance)
            {
                correctionVector.Y = ScaleToCamera(foregroundTexture.Height) -
                    Viewport.Height / 2;
                needCorrection = true;
            }

            if (needCorrection)
            {
                FlyToPositionNoScale(correctionVector);
            }
        }

        /// <summary>
        /// Shifts the display so that the designated point is in the center of the
        /// display, assuming that is possible.
        /// </summary>
        /// <param name="centerLocation">The location to center on.</param>
        public void CenterOnPosition(Vector2 centerLocation)
        {
            centerLocation *= DrawScale;

            CenterOnPositionNoScale(centerLocation);
        }

        public void CenterOnPositionNoScale(Vector2 centerLocation)
        {
            int viewWidth = ScreenManager.GraphicsDevice.Viewport.Width;
            int viewHeight = ScreenManager.GraphicsDevice.Viewport.Height;

            Vector2 newOffset = new Vector2(
                viewWidth / 2 - centerLocation.X,
                viewHeight / 2 - centerLocation.Y);

            DrawOffset = ClampDrawOffset(newOffset); ;
        }

        /// <summary>
        /// Causes the camera to smoothly fly to a specified destination.
        /// The camera will aim for the destination to be visible on screen.
        /// </summary>
        /// <param name="centerLocation">The location to fly to.</param>
        public void FlyToPosition(Vector2 flightDestination)
        {
            flightDestination *= DrawScale;

            FlyToPositionNoScale(flightDestination);
        }

        private void FlyToPositionNoScale(Vector2 flightDestination)
        {
            flightDestinationPoint =
                new Point((int)flightDestination.X, (int)flightDestination.Y);
            this.flightDestination = flightDestination;
            isFlying = true;
        }

        /// <summary>
        /// Used to make sure an offset is not outside the bounds of the game world.
        /// </summary>
        /// <param name="offset">Offset to clamp.</param>
        /// <returns>The offset after clamping it to fit inside the 
        /// game world.</returns>
        private Vector2 ClampDrawOffset(Vector2 offset)
        {
            offset.X = MathHelper.Clamp(offset.X,
                            CameraMinXOffset, CameraMaxXOffset);
            offset.Y = MathHelper.Clamp(offset.Y,
                CameraMinYOffset, CameraMaxYOffset);

            return offset;
        }
        #endregion

        #region Gameplay Helpers
        /// <summary>
        /// Starts a new game session, setting all game states to initial values.
        /// </summary>
        void Start()
        {
            // Set initial wind direction
            wind = Vector2.Zero;
            isHumanTurn = false;
            changeTurn = true;
            computer.Catapult.CurrentState = CatapultState.Reset;
        }
        #endregion
    }
}
