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
        Vector2 cloud1Position;
        Vector2 cloud2Position;

        Vector2 playerOneHUDPosition;
        Vector2 playerTwoHUDPosition;
        Vector2 windArrowPosition;

        // Gameplay members
        Human playerOne;
        Human playerTwo;
        Vector2 wind;
        bool changeTurn;
        bool isFirstPlayerTurn;
        bool gameOver;
        Random random;
        const int minWind = 0;
        const int maxWind = 10;

        // Helper members
        bool isDragging;
        #endregion

        #region Initialization
        public GameplayScreen()
        {
            EnabledGestures = GestureType.FreeDrag |
                GestureType.DragComplete |
                GestureType.Tap;

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
            cloud1Position = new Vector2(224 - cloud1Texture.Width, 32);
            cloud2Position = new Vector2(64, 90);

            // Define initial HUD positions
            playerOneHUDPosition = new Vector2(7, 7);
            playerTwoHUDPosition = new Vector2(613, 7);
            windArrowPosition = new Vector2(345, 46);

            // Initialize human & AI players
            playerOne = new Human(ScreenManager.Game, ScreenManager.SpriteBatch, PlayerSide.Left);
            playerOne.Initialize();
            playerOne.Name = "Player 1";

            playerTwo = new Human(ScreenManager.Game, ScreenManager.SpriteBatch, PlayerSide.Right);
            playerTwo.Initialize();
            playerTwo.Name = "Player 2";

            // Identify enemies
            playerOne.Enemy = playerTwo;
            playerTwo.Enemy = playerOne;
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
            if ((playerOne.Catapult.GameOver || playerTwo.Catapult.GameOver) &&
                (gameOver == false))
            {
                gameOver = true;

                if (playerOne.Score > playerTwo.Score)
                {
                    AudioManager.PlaySound("gameOver_Win");
                }
                else
                {
                    AudioManager.PlaySound("gameOver_Lose");
                }

                return;
            }

            // If Reset flag raised and both catapults are not animating - 
            // active catapult finished the cycle, new turn!
            if ((playerOne.Catapult.CurrentState == CatapultState.Reset ||
                playerTwo.Catapult.CurrentState == CatapultState.Reset) &&
                !(playerOne.Catapult.AnimationRunning ||
                playerTwo.Catapult.AnimationRunning))
            {
                changeTurn = true;

                if (playerOne.IsActive == true) //Last turn was a left player turn?
                {
                    playerOne.IsActive = false;
                    playerTwo.IsActive = true;
                    isFirstPlayerTurn = false;
                    playerOne.Catapult.CurrentState = CatapultState.Idle;
                    playerTwo.Catapult.CurrentState = CatapultState.Idle;
                }
                else //It was an right player turn
                {
                    playerOne.IsActive = true;
                    playerTwo.IsActive = false;
                    isFirstPlayerTurn = true;
                    playerTwo.Catapult.CurrentState = CatapultState.Idle;
                    playerOne.Catapult.CurrentState = CatapultState.Idle;
                }
            }

            if (changeTurn)
            {
                // Update wind
                wind = new Vector2(random.Next(-1, 2),
                    random.Next(minWind, maxWind + 1));

                // Set new wind value to the players and 
                playerOne.Catapult.Wind = playerTwo.Catapult.Wind =
                    wind.X > 0 ? wind.Y : -wind.Y;
                changeTurn = false;
            }

            // Update the players
            playerOne.Update(gameTime);
            playerTwo.Update(gameTime);

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
            DrawPlayerTwo(gameTime);
            DrawPlayerOne(gameTime);
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
            else if (isFirstPlayerTurn &&
                (playerOne.Catapult.CurrentState == CatapultState.Idle ||
                    playerOne.Catapult.CurrentState == CatapultState.Aiming))
            {
                // Read all available gestures
                foreach (GestureSample gestureSample in input.Gestures)
                {
                    if (gestureSample.GestureType == GestureType.FreeDrag)
                        isDragging = true;
                    else if (gestureSample.GestureType == GestureType.DragComplete)
                        isDragging = false;

                    playerOne.HandleInput(gestureSample);
                }
            }
            else if (!isFirstPlayerTurn &&
                (playerTwo.Catapult.CurrentState == CatapultState.Idle ||
                    playerTwo.Catapult.CurrentState == CatapultState.Aiming))
            {
                // Read all available gestures
                foreach (GestureSample gestureSample in input.Gestures)
                {
                    if (gestureSample.GestureType == GestureType.FreeDrag)
                        isDragging = true;
                    else if (gestureSample.GestureType == GestureType.DragComplete)
                        isDragging = false;

                    playerTwo.HandleInput(gestureSample);
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
            if (cloud1Position.X > ScreenManager.GraphicsDevice.Viewport.Width)
                cloud1Position.X = -cloud1Texture.Width * 2.0f;
            else if (cloud1Position.X < -cloud1Texture.Width * 2.0f)
                cloud1Position.X = ScreenManager.GraphicsDevice.Viewport.Width;

            cloud2Position += new Vector2(16.0f, 0.0f) * elapsedTime *
                windDirection * wind.Y;
            if (cloud2Position.X > ScreenManager.GraphicsDevice.Viewport.Width)
                cloud2Position.X = -cloud2Texture.Width * 2.0f;
            else if (cloud2Position.X < -cloud2Texture.Width * 2.0f)
                cloud2Position.X = ScreenManager.GraphicsDevice.Viewport.Width;
        }
        #endregion

        #region Draw Helpers
        /// <summary>
        /// Draws the player's catapult
        /// </summary>
        void DrawPlayerOne(GameTime gameTime)
        {
            if (!gameOver)
                playerOne.Draw(gameTime);
        }

        /// <summary>
        /// Draws the AI's catapult
        /// </summary>
        void DrawPlayerTwo(GameTime gameTime)
        {
            if (!gameOver)
                playerTwo.Draw(gameTime);
        }

        /// <summary>
        /// Draw the sky, clouds, mountains, etc. 
        /// </summary>
        private void DrawBackground()
        {
            // Clear the background
            ScreenManager.Game.GraphicsDevice.Clear(Color.White);

            // Draw the Sky
            ScreenManager.SpriteBatch.Draw(skyTexture, Vector2.Zero, Color.White);

            // Draw Cloud #1
            ScreenManager.SpriteBatch.Draw(cloud1Texture,
                cloud1Position, Color.White);

            // Draw the Mountain
            ScreenManager.SpriteBatch.Draw(mountainTexture,
                Vector2.Zero, Color.White);

            // Draw Cloud #2
            ScreenManager.SpriteBatch.Draw(cloud2Texture,
                cloud2Position, Color.White);

            // Draw the Castle, trees, and foreground 
            ScreenManager.SpriteBatch.Draw(foregroundTexture,
                Vector2.Zero, Color.White);
        }

        /// <summary>
        /// Draw the HUD, which consists of the score elements and the GAME OVER tag.
        /// </summary>
        void DrawHud()
        {
            if (gameOver)
            {
                Texture2D texture = victoryTexture;
                string winMessage = "";
                if (playerOne.Score > playerTwo.Score)
                {
                    winMessage ="Player 1 Wins!";
                }
                else
                {
                    winMessage = "Player 2 Wins!";
                }

                ScreenManager.SpriteBatch.Draw(
                    texture,
                    new Vector2(ScreenManager.Game.GraphicsDevice.Viewport.Width / 2 - texture.Width / 2,
                                ScreenManager.Game.GraphicsDevice.Viewport.Height / 2 - texture.Height / 2),
                    Color.White);

                Vector2 size = hudFont.MeasureString(winMessage);
                DrawString(hudFont, winMessage,
                    new Vector2(ScreenManager.Game.GraphicsDevice.Viewport.Width / 2 - size.X / 2,
                        ScreenManager.Game.GraphicsDevice.Viewport.Height / 2 - texture.Height / 2 + 100), 
                    Color.Red);
            }
            else
            {
                // Draw Player Hud
                ScreenManager.SpriteBatch.Draw(hudBackgroundTexture,
                    playerOneHUDPosition, Color.White);
                ScreenManager.SpriteBatch.Draw(ammoTypeTexture,
                    playerOneHUDPosition + new Vector2(33, 35), Color.White);
                DrawString(hudFont, playerOne.Score.ToString(),
                    playerOneHUDPosition + new Vector2(123, 35), Color.White);
                DrawString(hudFont, playerOne.Name,
                    playerOneHUDPosition + new Vector2(40, 1), Color.Blue);

                // Draw Computer Hud
                ScreenManager.SpriteBatch.Draw(hudBackgroundTexture,
                    playerTwoHUDPosition, Color.White);
                ScreenManager.SpriteBatch.Draw(ammoTypeTexture,
                    playerTwoHUDPosition + new Vector2(33, 35), Color.White);
                DrawString(hudFont, playerTwo.Score.ToString(),
                    playerTwoHUDPosition + new Vector2(123, 35), Color.White);
                DrawString(hudFont, playerTwo.Name,
                    playerTwoHUDPosition + new Vector2(40, 1), Color.Red);

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

                if (isFirstPlayerTurn)
                {
                    // Prepare first player prompt message
                    text = !isDragging ?
                        "Player 1, Drag Anywhere to Fire" : "Release to Fire!";
                    size = hudFont.MeasureString(text);
                }
                else
                {
                    // Prepare second player message
                    text = !isDragging ? "Player 2, Drag Anywhere to Fire!" : "Release to Fire!";
                    size = hudFont.MeasureString(text);
                }

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

        /// <summary>
        /// Pause the current game
        /// </summary>
        private void PauseCurrentGame()
        {
            var pauseMenuBackground = new BackgroundScreen();

            if (isDragging)
            {
                isDragging = false;
                playerOne.Catapult.CurrentState = CatapultState.Idle;
            }

            ScreenManager.AddScreen(pauseMenuBackground, null);
            ScreenManager.AddScreen(new PauseScreen(pauseMenuBackground, 
                playerOne, playerTwo), null);
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
            isFirstPlayerTurn = false;
            changeTurn = true;
            playerTwo.Catapult.CurrentState = CatapultState.Reset;
        }
        #endregion
    }
}
