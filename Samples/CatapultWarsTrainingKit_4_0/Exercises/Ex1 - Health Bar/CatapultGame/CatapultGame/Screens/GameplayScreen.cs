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
        Texture2D blankTexture;
        SpriteFont hudFont;

        // Rendering members
        Vector2 cloud1Position;
        Vector2 cloud2Position;

        Vector2 playerHUDPosition;
        Vector2 computerHUDPosition;
        Vector2 windArrowPosition;
        Vector2 playerHealthBarPosition;
        Vector2 computerHealthBarPosition;
        Vector2 healthBarFullSize;

        // Gameplay members
        Human player;
        AI computer;
        Vector2 wind;
        bool changeTurn;
        bool isHumanTurn;
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
            blankTexture = Load<Texture2D>("Textures/Backgrounds/blank");
            // Load font
            hudFont = Load<SpriteFont>("Fonts/HUDFont");

            // Define initial cloud position
            cloud1Position = new Vector2(224 - cloud1Texture.Width, 32);
            cloud2Position = new Vector2(64, 90);

            // Define initial HUD positions
            playerHUDPosition = new Vector2(7, 7);
            computerHUDPosition = new Vector2(613, 7);
            windArrowPosition = new Vector2(345, 46);
            Vector2 healthBarOffset = new Vector2(25, 82);
            playerHealthBarPosition = playerHUDPosition + healthBarOffset;
            computerHealthBarPosition = computerHUDPosition + healthBarOffset;
            healthBarFullSize = new Vector2(130, 20);

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

            // If Reset flag raised and both catapults are not animating - 
            // active catapult finished the cycle, new turn!
            if ((player.Catapult.CurrentState == CatapultState.Reset ||
                computer.Catapult.CurrentState == CatapultState.Reset) &&
                !(player.Catapult.AnimationRunning ||
                computer.Catapult.AnimationRunning))
            {
                changeTurn = true;

                if (player.IsActive == true) //Last turn was a human turn?
                {
                    player.IsActive = false;
                    computer.IsActive = true;
                    isHumanTurn = false;
                    player.Catapult.CurrentState = CatapultState.Idle;
                    computer.Catapult.CurrentState = CatapultState.Aiming;
                }
                else //It was an AI turn
                {
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

                Rectangle rect = new Rectangle((int)playerHealthBarPosition.X, (int)playerHealthBarPosition.Y,
                    (int)healthBarFullSize.X * player.Health / 100, (int)healthBarFullSize.Y);
                Rectangle underRect = new Rectangle(rect.X, rect.Y, rect.Width + 1, rect.Height + 1);
                ScreenManager.SpriteBatch.Draw(blankTexture, underRect, Color.Black);
                ScreenManager.SpriteBatch.Draw(blankTexture, rect, Color.Blue);

                // Draw Computer Hud
                ScreenManager.SpriteBatch.Draw(hudBackgroundTexture,
                    computerHUDPosition, Color.White);
                ScreenManager.SpriteBatch.Draw(ammoTypeTexture,
                    computerHUDPosition + new Vector2(33, 35), Color.White);
                DrawString(hudFont, computer.Score.ToString(),
                    computerHUDPosition + new Vector2(123, 35), Color.White);
                DrawString(hudFont, computer.Name,
                    computerHUDPosition + new Vector2(40, 1), Color.Red);

                rect = new Rectangle((int)computerHealthBarPosition.X, (int)computerHealthBarPosition.Y,
                    (int)healthBarFullSize.X * computer.Health / 100, (int)healthBarFullSize.Y);
                underRect = new Rectangle(rect.X, rect.Y, rect.Width + 1, rect.Height + 1);
                ScreenManager.SpriteBatch.Draw(blankTexture, underRect, Color.Black);
                ScreenManager.SpriteBatch.Draw(blankTexture, rect, Color.Red);

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

                if (isHumanTurn)
                {
                    // Prepare human prompt message
                    text = !isDragging ?
                        "Drag Anywhere to Fire" : "Release to Fire!";
                    size = hudFont.MeasureString(text);
                }
                else
                {
                    // Prepare AI message
                    text = "I'll get you yet!";
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
                player.Catapult.CurrentState = CatapultState.Idle;
            }

            ScreenManager.AddScreen(pauseMenuBackground, null);
            ScreenManager.AddScreen(new PauseScreen(pauseMenuBackground, 
                player, computer), null);
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
