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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

#endregion

namespace NinjAcademy
{
    class HighScoreScreen : GameScreen
    {
        #region Fields/Properties


        const int highscorePlaces = 7;

        public static List<KeyValuePair<string, int>> highScore = new List<KeyValuePair<string, int>>(highscorePlaces)
        {
            new KeyValuePair<string,int>
                ("Goku",9001),
            new KeyValuePair<string,int>
                ("Ellen",500),
            new KeyValuePair<string,int>
                ("Terry",250),
            new KeyValuePair<string,int>
                ("Dave",100),
            new KeyValuePair<string,int>
                ("Biff",50),
            new KeyValuePair<string,int>
                ("Michael",20),
            new KeyValuePair<string,int>
                ("Dan Hibiki",10),
        };

        SpriteFont highScoreFont;

        const string HighScoreFilename = "highscores.txt";

        Dictionary<int, string> numberPlaceMapping;

        Texture2D highScoreTitleTexture;
        Vector2 titlePosition;
        Vector2 textShadowVector;

        Rectangle viewport;

        /// <summary>
        /// A value indicating whether high-score data has been loaded.
        /// </summary>
        public static bool HighscoreLoaded { get; private set; }

        /// <summary>
        /// A value indicating whether high-score data has been saved.
        /// </summary>
        public static bool HighscoreSaved { get; private set; }


        #endregion

        #region Initialzations


        static HighScoreScreen()
        {
            HighscoreLoaded = false;
            HighscoreSaved = false;
        }

        /// <summary>
        /// Creates a new high-score screen instance.
        /// </summary>
        public HighScoreScreen()
        {
            EnabledGestures = GestureType.Tap;
            if (HighscoreLoaded == false)
            {
                throw new InvalidOperationException("Missing highscore data");
            }

            numberPlaceMapping = new Dictionary<int, string>();
            InitializeMapping();
        }

        /// <summary>
        /// Load screen resources
        /// </summary>
        public override void LoadContent()
        {
            highScoreFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/HighScoreFont");
            highScoreTitleTexture = ScreenManager.Game.Content.Load<Texture2D>("Textures/highscore_title");

            textShadowVector = new Vector2(4, 4);

            viewport = ScreenManager.GraphicsDevice.Viewport.Bounds;
            titlePosition = new Vector2(viewport.Center.X - highScoreTitleTexture.Width / 2,
                GameConstants.HighScoreTitleTopMargin);

            base.LoadContent();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles user input as a part of screen logic update.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="input">Input information.</param>
        public override void HandleInput(InputState input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (input.IsPauseGame(null))
            {
                Exit();
            }

            // Return to the main menu when a tap gesture is recognized
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
            ScreenManager.AddScreen(new BackgroundScreen("titlescreenBG"), null);
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
            base.Draw(gameTime);

            if (HighscoreLoaded == false)
            {
                return;
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw the screen title
            spriteBatch.Draw(highScoreTitleTexture, titlePosition, Color.White);

            // Draw the high-score table
            for (int i = 0; i < highScore.Count; i++)
            {
                if (!string.IsNullOrEmpty(highScore[i].Key))
                {
                    // Draw place number
                    Vector2 textPosition = new Vector2(GameConstants.HighScorePlaceLeftMargin, 
                        i * GameConstants.HighScoreVerticalJump + GameConstants.HighScoreTopMargin);

                    spriteBatch.DrawString(highScoreFont, GetPlaceString(i), textPosition + textShadowVector, 
                        Color.Black);
                    spriteBatch.DrawString(highScoreFont, GetPlaceString(i), textPosition, Color.White);

                    // Draw Name
                    textPosition.X = GameConstants.HighScoreNameLeftMargin;

                    spriteBatch.DrawString(highScoreFont, highScore[i].Key, textPosition + textShadowVector,
                        Color.Black);
                    spriteBatch.DrawString(highScoreFont, highScore[i].Key, textPosition, Color.White);

                    // Draw score
                    textPosition.X = GameConstants.HighScoreScoreLeftMargin;

                    spriteBatch.DrawString(highScoreFont, highScore[i].Value.ToString(),
                        textPosition + textShadowVector, Color.Black);
                    spriteBatch.DrawString(highScoreFont, highScore[i].Value.ToString(), textPosition, Color.White);
                }
            }

            spriteBatch.End();
        }


        #endregion

        #region Highscore loading/saving logic


        /// <summary>
        /// Check if a score belongs on the high score table.
        /// </summary>
        /// <returns>True if the score belongs on the high-score table, false otherwise.</returns>
        public static bool IsInHighscores(int score)
        {
            // If the score is better than the worst score in the table
            return score > highScore[highscorePlaces - 1].Value;
        }

        /// <summary>
        /// Put high score on high-scores table.
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
        /// Call this method whenever the high-score data changes. This will mark the high-score data as not changed.
        /// </summary>
        public static void HighScoreChanged()
        {
            HighscoreSaved = false;
        }

        /// <summary>
        /// Order the high scores table.
        /// </summary>
        private static void OrderGameScore()
        {
            highScore.Sort(CompareScores);
        }

        /// <summary>
        /// Comparison method used to compare two high-score entries.
        /// </summary>
        /// <param name="score1">First high-score entry.</param>
        /// <param name="score2">Second high-score entry.</param>
        /// <returns>1 if the first high-score is smaller than the second, 0 if both
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
        /// Saves the current high-score to a text file. 
        /// </summary>
        public static void SaveHighscore()
        {
            // Get the place to store the data
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Create the file to save the data
                using (IsolatedStorageFileStream isfs = 
                    isolatedStorageFile.CreateFile(HighScoreScreen.HighScoreFilename))
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

            HighscoreSaved = true;
        }

        /// <summary>
        /// Loads the high score from a text file.  
        /// </summary>
        public static void LoadHighscores()
        {
            // Get the place the data stored
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Try to open the file
                if (isolatedStorageFile.FileExists(HighScoreScreen.HighScoreFilename))
                {
                    using (IsolatedStorageFileStream isfs =
                        isolatedStorageFile.OpenFile(HighScoreScreen.HighScoreFilename, FileMode.Open))
                    {
                        // Get the stream to read the data
                        using (StreamReader reader = new StreamReader(isfs))
                        {
                            // Read the high-scores
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

            HighscoreLoaded = true;
        }

        /// <summary>
        /// Gets a string describing an index's position in the high-score.
        /// </summary>
        /// <param name="number">Score's index.</param>
        /// <returns>A string describing the score's index.</returns>
        private string GetPlaceString(int number)
        {
            return numberPlaceMapping[number];
        }

        /// <summary>
        /// Initializes the mapping between score indices and position strings.
        /// </summary>
        private void InitializeMapping()
        {
            numberPlaceMapping.Add(0, "1.");
            numberPlaceMapping.Add(1, "2.");
            numberPlaceMapping.Add(2, "3.");
            numberPlaceMapping.Add(3, "4.");
            numberPlaceMapping.Add(4, "5.");
            numberPlaceMapping.Add(5, "6.");
            numberPlaceMapping.Add(6, "7.");
        }


        #endregion
    }
}
