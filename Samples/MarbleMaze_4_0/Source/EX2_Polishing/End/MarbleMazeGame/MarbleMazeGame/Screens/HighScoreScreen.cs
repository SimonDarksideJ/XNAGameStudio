#region File Description
//-----------------------------------------------------------------------------
// LoadingScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GameStateManagement;
using Microsoft.Xna.Framework.Input.Touch;

#endregion

namespace MarbleMazeGame
{
    class HighScoreScreen : GameScreen
    {
        #region Fields
        const int highscorePlaces = 10;
        public static KeyValuePair<string, TimeSpan>[] highScore = new KeyValuePair<string, TimeSpan>[highscorePlaces] 
        {
            new KeyValuePair<string,TimeSpan>("Jasper",TimeSpan.FromSeconds(90)),
            new KeyValuePair<string,TimeSpan>("Ellen",TimeSpan.FromSeconds(110)),
            new KeyValuePair<string,TimeSpan>("Terry",TimeSpan.FromSeconds(130)),
            new KeyValuePair<string,TimeSpan>("Lori",TimeSpan.FromSeconds(150)),
            new KeyValuePair<string,TimeSpan>("Michael",TimeSpan.FromSeconds(170)),
            new KeyValuePair<string,TimeSpan>("Carol",TimeSpan.FromSeconds(190)),
            new KeyValuePair<string,TimeSpan>("Toni",TimeSpan.FromSeconds(210)),
            new KeyValuePair<string,TimeSpan>("Cassie",TimeSpan.FromSeconds(230)),
            new KeyValuePair<string,TimeSpan>("Luca",TimeSpan.FromSeconds(250)),
            new KeyValuePair<string,TimeSpan>("Brian",TimeSpan.FromSeconds(270))
        };

        SpriteFont highScoreFont;
        #endregion

        #region Initialzations
        public HighScoreScreen()
        {
            EnabledGestures = GestureType.Tap;
        }
        #endregion

        #region Loading
        /// <summary>
        /// Load screen resources
        /// </summary>
        public override void LoadContent()
        {
            highScoreFont = Load<SpriteFont>(@"Fonts\MenuFont");

            base.LoadContent();
        }
        #endregion

        #region Handle Input
        /// <summary>
        /// Handles user input as a part of screen logic update
        /// </summary>
        /// <param name="input"></param>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (input.IsPauseGame(null))
            {
                Exit();
            }

            // Return to the main menu when a tep gesture is recognized
            if (input.Gestures.Count > 0)
            {
                GestureSample sample = input.Gestures[0];
                if (sample.GestureType == GestureType.Tap)
                {
                    Exit();

                    input.Gestures.Clear();
                }
            }
        }

        /// <summary>
        /// Exit this screen
        /// </summary>
        private void Exit()
        {
            this.ExitScreen();
            ScreenManager.AddScreen(new BackgroundScreen(), null);
            ScreenManager.AddScreen(new MainMenuScreen(), null);
        }
        #endregion

        #region Render
        /// <summary>
        /// Renders the screen
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();

            // Draw the title
            ScreenManager.SpriteBatch.DrawString(highScoreFont, "High Scores", 
                new Vector2(30, 30), Color.White);

            // Draw the highscores table
            for (int i = 0; i < highScore.Length; i++)
            {
                ScreenManager.SpriteBatch.DrawString(highScoreFont, 
                    String.Format("{0}. {1}", i + 1, highScore[i].Key), 
                    new Vector2(100, i * 40 + 70), Color.YellowGreen);
                ScreenManager.SpriteBatch.DrawString(highScoreFont, 
                    String.Format("{0:00}:{1:00}", highScore[i].Value.Minutes, 
                    highScore[i].Value.Seconds), new Vector2(500, i * 40 + 70), 
                    Color.YellowGreen);
            }

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }
        #endregion

        #region Highscore loading/saving logic
        /// <summary>
        /// Check if a score belongs on the high score table
        /// </summary>
        /// <returns></returns>
        public static bool IsInHighscores(TimeSpan gameTime)
        {
            // If the score is better than the worst score in the table
            return gameTime < highScore[highscorePlaces - 1].Value;
        }

        /// <summary>
        /// Put high score on highscores table
        /// </summary>
        /// <param name="name"></param>
        /// <param name="gameTime"></param>
        public static void PutHighScore(string playerName, TimeSpan gameTime)
        {
            if (IsInHighscores(gameTime))
            {
                highScore[highscorePlaces - 1] = 
                    new KeyValuePair<string, TimeSpan>(playerName, gameTime);
                OrderGameScore();
            }
        }

        /// <summary>
        /// Order the high scores table
        /// </summary>
        private static void OrderGameScore()
        {
            highScore = (highScore.OrderBy(e => e.Value.Ticks)).ToArray();
        }

        /// <summary>
        /// Saves the current highscore to a text file. 
        /// </summary>
        public static void SaveHighscore()
        {
            // Get the place to store the data
            using (IsolatedStorageFile isf = 
                IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Create the file to save the data
                using (IsolatedStorageFileStream isfs = 
                    new IsolatedStorageFileStream("highscores.txt",
                    FileMode.Create, isf))
                {
                    using (StreamWriter writer = new StreamWriter(isfs))
                    {
                        for (int i = 0; i < highScore.Length; i++)
                        {
                            // Write the scores
                            writer.WriteLine(highScore[i].Key);
                            writer.WriteLine(highScore[i].Value.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the high score from a text file.  
        /// </summary>
        public static void LoadHighscore()
        {
            // Get the place the data stored
            using (IsolatedStorageFile isf = 
                IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Try to open the file
                if (isf.FileExists("highscores.txt"))
                {
                    using (IsolatedStorageFileStream isfs = 
                        new IsolatedStorageFileStream("highscores.txt",
                        FileMode.Open, isf))
                    {
                        // Get the stream to read the data
                        using (StreamReader reader = new StreamReader(isfs))
                        {
                            // Read the highscores
                            int i = 0;
                            while (!reader.EndOfStream)
                            {
                                string[] line = new[] 
                                { reader.ReadLine(), reader.ReadLine() };
                                highScore[i++] = 
                                    new KeyValuePair<string, TimeSpan>(line[0],
                                    TimeSpan.Parse(line[1]));
                            }
                        }
                    }
                }
            }


            OrderGameScore();
        }
        #endregion
    }
}
