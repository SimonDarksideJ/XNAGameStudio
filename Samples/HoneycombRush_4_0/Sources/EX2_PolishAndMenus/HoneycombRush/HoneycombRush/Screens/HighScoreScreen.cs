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
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;


#endregion

namespace HoneycombRush
{
    class HighScoreScreen : GameScreen
    {
        #region Fields

        static readonly string HighScoreFilename = "highscores.txt";

        const int highscorePlaces = 5;
        public static List<KeyValuePair<string, int>> highScore = new List<KeyValuePair<string, int>>(highscorePlaces)
        {
            new KeyValuePair<string,int>
                ("Jasper",55000),
            new KeyValuePair<string,int>
                ("Ellen",52750),
            new KeyValuePair<string,int>
                ("Terry",52200),
            new KeyValuePair<string,int>
                ("Lori",50200),
            new KeyValuePair<string,int>
                ("Michael",50750),
        };

        SpriteFont highScoreFont;

        Dictionary<int, string> numberPlaceMapping;


        #endregion

        #region Initialzations


        /// <summary>
        /// Creates a new highscore screen instance.
        /// </summary>
        public HighScoreScreen()
        {
            EnabledGestures = GestureType.Tap;

            numberPlaceMapping = new Dictionary<int, string>();
            initializeMapping();
        }

        /// <summary>
        /// Load screen resources
        /// </summary>
        public override void LoadContent()
        {
            highScoreFont = Load<SpriteFont>(@"Fonts\HighScoreFont");

            base.LoadContent();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles user input as a part of screen logic update.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="input">Input information.</param>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

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
        /// Exit this screen.
        /// </summary>
        private void Exit()
        {
            this.ExitScreen();
            ScreenManager.AddScreen(new BackgroundScreen("titlescreen"), null);
            ScreenManager.AddScreen(new MainMenuScreen(), null);
        }


        #endregion

        #region Render


        /// <summary>
        /// Renders the screen.
        /// </summary>
        /// <param name="gameTime">Game time information</param>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();

            // Draw the highscores table
            for (int i = 0; i < highScore.Count; i++)
            {
                if (!string.IsNullOrEmpty(highScore[i].Key))
                {
                    // Draw place number
                    ScreenManager.SpriteBatch.DrawString(highScoreFont, GetPlaceString(i), 
                        new Vector2(20, i * 72 + 86), Color.Black);

                    // Draw Name
                    ScreenManager.SpriteBatch.DrawString(highScoreFont, highScore[i].Key,
                        new Vector2(210, i * 72 + 86), Color.DarkRed);

                    // Draw score
                    ScreenManager.SpriteBatch.DrawString(highScoreFont, highScore[i].Value.ToString(),
                        new Vector2(560, i * 72 + 86), Color.Yellow);
                }
            }

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Highscore loading/saving logic


        /// <summary>
        /// Check if a score belongs on the high score table.
        /// </summary>
        /// <returns></returns>
        public static bool IsInHighscores(int score)
        {
            // If the score is better than the worst score in the table
            return score > highScore[highscorePlaces - 1].Value;
        }

        /// <summary>
        /// Put high score on highscores table.
        /// </summary>
        /// <param name="name">Player's name.</param>
        /// <param name="score">The player's score.</param>
        public static void PutHighScore(string playerName, int score)
        {
            if (IsInHighscores(score))
            {
                highScore[highscorePlaces - 1] = new KeyValuePair<string, int>(playerName, score);
                OrderGameScore();
                SaveHighscore();
            }
        }

        /// <summary>
        /// Order the high scores table.
        /// </summary>
        private static void OrderGameScore()
        {
            highScore.Sort(CompareScores);
        }

        /// <summary>
        /// Comparison method used to compare two highscore entries.
        /// </summary>
        /// <param name="score1">First highscore entry.</param>
        /// <param name="score2">Second highscore entry.</param>
        /// <returns>1 if the first highscore is smaller than the second, 0 if both
        /// are equal and -1 otherwise.</returns>
        private static int CompareScores(KeyValuePair<string, int> score1,
            KeyValuePair<string, int> score2)
        {
            if (score1.Value < score2.Value)
            {
                return 1;
            }

            if (score1.Value == score2.Value)
            {
                return 0;
            }

            return -1;
        }

        /// <summary>
        /// Saves the current highscore to a text file. 
        /// </summary>
        public static void SaveHighscore()
        {
            // Get the place to store the data
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Create the file to save the data
                using (IsolatedStorageFileStream isfs = isf.CreateFile(HighScoreScreen.HighScoreFilename))
                {
                    using (StreamWriter writer = new StreamWriter(isfs))
                    {
                        for (int i = 0; i < highScore.Count; i++)
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
        public static void LoadHighscores()
        {
            // Get the place the data stored
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Try to open the file
                if (isf.FileExists(HighScoreScreen.HighScoreFilename))
                {
                    using (IsolatedStorageFileStream isfs = 
                        isf.OpenFile(HighScoreScreen.HighScoreFilename, FileMode.Open))
                    {
                        // Get the stream to read the data
                        using (StreamReader reader = new StreamReader(isfs))
                        {
                            // Read the highscores
                            int i = 0;
                            while (!reader.EndOfStream)
                            {
                                string name = reader.ReadLine();
                                string score = reader.ReadLine();                                
                                highScore[i++] = new KeyValuePair<string, int>(name, int.Parse(score));
                            }
                        }
                    }
                }
            }

            OrderGameScore();
        }

        private string GetPlaceString(int number)
        {
            return numberPlaceMapping[number];
        }

        private void initializeMapping()
        {
            numberPlaceMapping.Add(0, "1ST");
            numberPlaceMapping.Add(1, "2ND");
            numberPlaceMapping.Add(2, "3RD");
            numberPlaceMapping.Add(3, "4TH");
            numberPlaceMapping.Add(4, "5TH");
        }


        #endregion
    }
}
