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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using Microsoft.Xna.Framework.GamerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Input.Touch;

#endregion

namespace MemoryMadness
{
    class GameplayScreen : GameScreen
    {
        private bool isLevelChange;

        private bool isActive;        
        public new bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;

                // TODO #1
                if (null != currentLevel)
                    currentLevel.IsActive = value;
            }
        }

        bool moveToHighScore = false;

        // Gameplay variables
        // TODO #2
        public Level currentLevel;
        int currentLevelNumber;
        int movesPerformed = 0;

        int maxLevelNumber;

        // Rendering variables
        SpriteFont levelNumberFont;
        SpriteFont textFont;
        Texture2D background;
        Texture2D buttonsTexture;

        // Input related variables
        TimeSpan inputTimeMeasure;
        TimeSpan inputGracePeriod = TimeSpan.FromMilliseconds(150);
        TouchInputState inputState = TouchInputState.Idle;
        List<TouchLocation> lastPressInput;

        public GameplayScreen(int levelNumber)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.0);

            currentLevelNumber = levelNumber;
        }

        public GameplayScreen(int levelNumber, int movesPerformed)
            : this(levelNumber)
        {
            this.movesPerformed = movesPerformed;
        }

        public void LoadAssets()
        {
            levelNumberFont =
                ScreenManager.Game.Content.Load<SpriteFont>(@"Fonts\GameplayLargeFont");
            textFont =
                ScreenManager.Game.Content.Load<SpriteFont>(@"Fonts\GameplaySmallFont");
            background =
                ScreenManager.Game.Content.Load<Texture2D>(
                    @"Textures\Backgrounds\gameplayBG");
            buttonsTexture =
                ScreenManager.Game.Content.Load<Texture2D>(@"Textures\ButtonStates");
        }

        public override void LoadContent()
        {
            LoadAssets();

            XDocument doc = XDocument.Load(@"Content\Gameplay\LevelDefinitions.xml");
            var levels = doc.Document.Descendants(XName.Get("Level"));
            foreach (var level in levels)
            {
                maxLevelNumber++;
            }

            // Resolution for a possible situation which can occur while debugging the 
            // game. The game may remember it is on a level which is higher than the
            // highest available level, following a change to the definition file.
            if (currentLevelNumber > maxLevelNumber)
                currentLevelNumber = 1;

            //TODO #3
            InitializeLevel();

            base.LoadContent();
        }

        private void InitializeLevel()
        {
            currentLevel = new Level(ScreenManager.Game,
                         ScreenManager.SpriteBatch,
                         currentLevelNumber, movesPerformed, buttonsTexture);
            currentLevel.IsActive = true;

            ScreenManager.Game.Components.Add(currentLevel);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);
            ScreenManager.SpriteBatch.Begin();

            ScreenManager.SpriteBatch.Draw(background, Vector2.Zero, Color.White);

            // TODO #4
            DrawLevelText();

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawLevelText()
        {
            if (IsActive)
            {
                string text;
                Vector2 size;
                Vector2 position;

                if (currentLevel.CurrentState == LevelState.NotReady)
                {
                    text = "Preparing...";
                    size = textFont.MeasureString(text);
                    position = new Vector2((ScreenManager.GraphicsDevice.Viewport.Width - size.X) / 2, 
                        (ScreenManager.GraphicsDevice.Viewport.Height - size.Y) / 2);
                    position.X += 20f;
                    ScreenManager.SpriteBatch.DrawString(textFont, text,
                        position, Color.White, 0f, Vector2.Zero, 0.9f, SpriteEffects.None, 0f);
                }
                else
                {
                    Color levelColor = Color.White;

                    switch (currentLevel.CurrentState)
                    {
                        case LevelState.NotReady:
                        case LevelState.Ready:
                            break;
                        case LevelState.Flashing:
                            levelColor = Color.Yellow;
                            break;
                        case LevelState.Started:
                        case LevelState.Success:
                        case LevelState.InProcess:
                        case LevelState.FinishedOk:
                            levelColor = Color.LimeGreen;
                            break;
                        case LevelState.Fault:
                        case LevelState.FinishedFail:
                            levelColor = Color.Red;
                            break;
                        default:
                            break;
                    }

                    // Draw "Level" text
                    text = "Level";
                    size = textFont.MeasureString(text);
                    position = new Vector2(70, (
                        ScreenManager.GraphicsDevice.Viewport.Height - size.Y) / 2);

                    ScreenManager.SpriteBatch.DrawString(
                        textFont, text, position, levelColor);

                    // Draw level number
                    text = currentLevelNumber.ToString("D2");
                    size = levelNumberFont.MeasureString(text);
                    position = new Vector2(290, (
                        ScreenManager.GraphicsDevice.Viewport.Height - size.Y) / 2);

                    ScreenManager.SpriteBatch.DrawString(
                        levelNumberFont, text, position, levelColor);
                }
            }
        }

        public override void HandleInput(InputState input)
        {
            if (IsActive)
            {
                if (input == null)
                    throw new ArgumentNullException("input");

                if (input.IsPauseGame(null))
                {
                    // TODO #7
                }

                if (input.TouchState.Count > 0)
                {
                    // We are about to handle touch input
                    switch (inputState)
                    {
                        case TouchInputState.Idle:
                            // We have yet to receive input, start grace period
                            inputTimeMeasure = TimeSpan.Zero;
                            inputState = TouchInputState.GracePeriod;
                            lastPressInput = new List<TouchLocation>();
                            foreach (var touch in input.TouchState)
                            {
                                if (touch.State == TouchLocationState.Pressed)
                                {
                                    lastPressInput.Add(touch);
                                }
                            }
                            break;
                        case TouchInputState.GracePeriod:
                            // Do nothing during the grace period other than remembering 
                            // additional presses
                            foreach (var touch in input.TouchState)
                            {
                                if (touch.State == TouchLocationState.Pressed)
                                {
                                    lastPressInput.Add(touch);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            //TODO #8
            //if ((moveToHighScore) && (!AudioManager.AreSoundsPlaying()))
            //{
            //    ScreenManager.Game.Components.Remove(currentLevel);

            //    foreach (GameScreen screen in ScreenManager.GetScreens())
            //        screen.ExitScreen();

            //    ScreenManager.AddScreen(new BackgroundScreen(true), null);
            //    ScreenManager.AddScreen(new HighScoreScreen(), null);
            //}

            if (!IsActive || moveToHighScore)
            {
                base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
                return;
            }

            bool isLevelChange = false;

            if ((inputState == TouchInputState.GracePeriod) && (isActive))
            {
                inputTimeMeasure += gameTime.ElapsedGameTime;

                // if the input grace period is over, handle the touch input
                if (inputTimeMeasure >= inputGracePeriod)
                {
                    currentLevel.RegisterTouch(lastPressInput);
                    inputState = TouchInputState.Idle;
                }
            }

            if (currentLevel.CurrentState == LevelState.FinishedOk && isActive)
            {
                //TODO #9
                

                if (currentLevelNumber < maxLevelNumber)
                {
                    currentLevelNumber++;
                    isLevelChange = true;
                }
                else
                {
                    //TODO #10 - REPLACE
                    ScreenManager.Game.Exit();
                    //FinishCurrentGame();
                }
            }
            else if (currentLevel.CurrentState == LevelState.FinishedFail)
            {
                //TODO #11
                

                currentLevelNumber = 1;
                isLevelChange = true;
            }

            if (isLevelChange)
            {
                ScreenManager.Game.Components.Remove(currentLevel);

                currentLevel = new Level(ScreenManager.Game,
                                            ScreenManager.SpriteBatch,
                                            currentLevelNumber, buttonsTexture);
                currentLevel.IsActive = true;

                ScreenManager.Game.Components.Add(currentLevel);
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}
