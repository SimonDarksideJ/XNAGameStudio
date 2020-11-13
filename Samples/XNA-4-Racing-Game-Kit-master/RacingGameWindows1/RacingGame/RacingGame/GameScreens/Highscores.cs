#region File Description
//-----------------------------------------------------------------------------
// Highscores.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Properties;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Sounds;
#endregion

namespace RacingGame.GameScreens
{
    /// <summary>
    /// Highscores
    /// </summary>
    /// <returns>IGame screen</returns>
    class Highscores : IGameScreen
    {
        #region Highscore helper class
        /// <summary>
        /// Highscore helper class
        /// </summary>
        private struct HighscoreInLevel
        {
            #region Variables
            /// <summary>
            /// Player name
            /// </summary>
            public string name;
            /// <summary>
            /// Highscore points 
            /// </summary>
            public int timeMilliseconds;
            #endregion

            #region Constructor
            /// <summary>
            /// Create highscore
            /// </summary>
            /// <param name="setName">Set name</param>
            /// <param name="setTimeMs">Set time ms</param>
            public HighscoreInLevel(string setName, int setTimeMs)
            {
                name = setName;
                timeMilliseconds = setTimeMs;
            }
            #endregion

            #region ToString
            /// <summary>
            /// To string
            /// </summary>
            /// <returns>String</returns>
            public override string ToString()
            {
                return name + ":" + timeMilliseconds;
            }
            #endregion
        }

        /// <summary>
        /// Number of highscores displayed in this screen.
        /// </summary>
        private const int NumOfHighscores = 10,
            NumOfHighscoreLevels = 3;

        /// <summary>
        /// List of remembered highscores.
        /// </summary>
        private static HighscoreInLevel[,] highscores = null;

        /// <summary>
        /// Write highscores to string. Used to save to highscores settings.
        /// </summary>
        private static void WriteHighscoresToSettings()
        {
            string saveString = "";
            for (int level = 0; level < NumOfHighscoreLevels; level++)
            {
                for (int num = 0; num < NumOfHighscores; num++)
                {
                    saveString += (saveString.Length == 0 ? "" : ",") +
                        highscores[level, num];
                }
            }

            GameSettings.Default.Highscores = saveString;

            ThreadPool.QueueUserWorkItem(new WaitCallback(SaveSettings), null);
        }

        /// <summary>
        /// Callback used for saving the settings from a worker thread
        /// </summary>
        /// <param name="replay">Not used, delegate signature requires it</param>
        private static void SaveSettings(object state)
        {
            GameSettings.Save();
        }

        /// <summary>
        /// Read highscores from settings
        /// </summary>
        /// <returns>True if reading succeeded, false otherwise.</returns>
        private static bool ReadHighscoresFromSettings()
        {
            if (String.IsNullOrEmpty(GameSettings.Default.Highscores))
                return false;

            string highscoreString = GameSettings.Default.Highscores;
            string[] allHighscores = highscoreString.Split(',');
            for (int level = 0; level < NumOfHighscoreLevels; level++)
                for (int num = 0; num < NumOfHighscores &&
                    level * NumOfHighscores + num < allHighscores.Length; num++)
                {
                    string[] oneHighscore =
                        allHighscores[level * NumOfHighscores + num].
                        Split(new char[] { ':' });
                    highscores[level, num] = new HighscoreInLevel(
                        oneHighscore[0], Convert.ToInt32(oneHighscore[1]));
                }

            return true;
        }
        #endregion

        #region Static constructor
        /// <summary>
        /// Create Highscores class, will basically try to load highscore list,
        /// if that fails we generate a standard highscore list!
        /// </summary>
        public static void Initialize()
        {
            // Init highscores
            highscores =
                new HighscoreInLevel[NumOfHighscoreLevels, NumOfHighscores];

            if (ReadHighscoresFromSettings() == false)
            {
                // Generate default lists
                for (int level = 0; level < NumOfHighscoreLevels; level++)
                {
                    for (int rank = 0; rank < NumOfHighscores; rank++)
                    {
                        highscores[level, rank] =
                            new HighscoreInLevel("Player " + (rank + 1).ToString(),
                                (75000 + rank * 5000) * (level + 1));
                    }
                }

                WriteHighscoresToSettings();
            }
        }
        #endregion

        #region Get top lap time
        /// <summary>
        /// Get top lap time
        /// </summary>
        /// <param name="level">Level</param>
        /// <returns>Best lap time</returns>
        public static float GetTopLapTime(int level)
        {
            return (float)highscores[level, 0].timeMilliseconds / 1000.0f;
        }
        #endregion

        #region Get top 5 rank lap times
        /// <summary>
        /// Get top 5 rank lap times
        /// </summary>
        /// <param name="level">Current level</param>
        /// <returns>Array of top 5 times</returns>
        public static int[] GetTop5LapTimes(int level)
        {
            return new int[]
                {
                    highscores[level, 0].timeMilliseconds,
                    highscores[level, 1].timeMilliseconds,
                    highscores[level, 2].timeMilliseconds,
                    highscores[level, 3].timeMilliseconds,
                    highscores[level, 4].timeMilliseconds,
                };
        }
        #endregion

        #region Get rank from current score
        /// <summary>
        /// Get rank from current time.
        /// Used in game to determinate rank while flying around ^^
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="timeMilisec">Time ms</param>
        /// <returns>Int</returns>
        public static int GetRankFromCurrentTime(int level, int timeMilliseconds)
        {
            // Time must be at least 1 second
            if (timeMilliseconds < 1000)
                // Invalid time, return rank 11 (out of highscore)
                return NumOfHighscores;

            // Just compare with all highscores and return the rank we have reached.
            for (int num = 0; num < NumOfHighscores; num++)
            {
                if (timeMilliseconds <= highscores[level, num].timeMilliseconds)
                    return num;
            }

            // No Rank found, use rank 11
            return NumOfHighscores;
        }
        #endregion

        #region Submit highscore after game
        /// <summary>
        /// Submit highscore. Done after each game is over (won or lost).
        /// New highscore will be added to the highscore screen.
        /// In the future: Also send highscores to the online server.
        /// </summary>
        /// <param name="score">Score</param>
        /// <param name="levelName">Level name</param>
        public static void SubmitHighscore(int level, int timeMilliseconds)
        {
            // Search which highscore rank we can replace
            for (int num = 0; num < NumOfHighscores; num++)
            {
                if (timeMilliseconds <= highscores[level, num].timeMilliseconds)
                {
                    // Move all highscores up
                    for (int moveUpNum = NumOfHighscores - 1; moveUpNum > num;
                        moveUpNum--)
                    {
                        highscores[level, moveUpNum] = highscores[level, moveUpNum - 1];
                    }

                    // Add this highscore into the local highscore table
                    highscores[level, num].name = GameSettings.Default.PlayerName;
                    highscores[level, num].timeMilliseconds = timeMilliseconds;

                    // And save that
                    Highscores.WriteHighscoresToSettings();

                    break;
                }
            }

            // Else no highscore was reached, we can't replace any rank.
        }
        #endregion

		#region Update
		/// <summary>
		/// Unimplemented
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update(GameTime gameTime)
		{

		}
		#endregion

        #region Render
        int selectedLevel = 1;
        /// <summary>
        /// Render game screen. Called each frame.
        /// </summary>
        /// <returns>Bool</returns>
        public bool Render()
        {
            // This starts both menu and in game post screen shader!
            BaseGame.UI.PostScreenMenuShader.Start();

            // Render background
            BaseGame.UI.RenderMenuBackground();
            BaseGame.UI.RenderBlackBar(160, 498 - 160);

            // Highscores header
            int posX = 10;
            int posY = 18;
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                posX += 36;
                posY += 26;
            }
            BaseGame.UI.Headers.RenderOnScreenRelative1600(
                posX, posY, UIRenderer.HeaderHighscoresGfxRect);

            // Track selection
            int xPos = BaseGame.XToRes(512 - 160 * 3 / 2 + 25);
            int yPos = BaseGame.YToRes(182);
            int lineHeight = BaseGame.YToRes(27);

            // Beginner track
            bool inBox = Input.MouseInBox(new Rectangle(
                xPos, yPos, BaseGame.XToRes(125), lineHeight));
            TextureFont.WriteText(xPos, yPos, "Beginner",
                selectedLevel == 0 ? Color.Yellow :
                inBox ? Color.White : Color.LightGray);
            if (inBox && Input.MouseLeftButtonJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                selectedLevel = 0;
            }
            xPos += BaseGame.XToRes(160 + 8);

            // Advanced track
            inBox = Input.MouseInBox(new Rectangle(
                xPos, yPos, BaseGame.XToRes(125), lineHeight));
            TextureFont.WriteText(xPos, yPos, "Advanced",
                selectedLevel == 1 ? Color.Yellow :
                inBox ? Color.White : Color.LightGray);
            if (inBox && Input.MouseLeftButtonJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                selectedLevel = 1;
            }
            xPos += BaseGame.XToRes(160 + 30 - 8);

            // Expert track
            inBox = Input.MouseInBox(new Rectangle(
                xPos, yPos, BaseGame.XToRes(125), lineHeight));
            TextureFont.WriteText(xPos, yPos, "Expert",
                selectedLevel == 2 ? Color.Yellow :
                inBox ? Color.White : Color.LightGray);
            if (inBox && Input.MouseLeftButtonJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                selectedLevel = 2;
            }

            // Also handle xbox controller input
            if (Input.GamePadLeftJustPressed ||
                Input.KeyboardLeftJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                selectedLevel = (selectedLevel + 2) % 3;
            }
            else if (Input.GamePadRightJustPressed ||
                Input.KeyboardRightJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                selectedLevel = (selectedLevel + 1) % 3;
            }

            int xPos1 = BaseGame.XToRes(300);
            int xPos2 = BaseGame.XToRes(350);
            int xPos3 = BaseGame.XToRes(640);

            // Draw seperation line
            yPos = BaseGame.YToRes(208);
            BaseGame.DrawLine(
                new Point(xPos1, yPos),
                new Point(xPos3 + TextureFont.GetTextWidth("5:67:89"), yPos),
                new Color(192, 192, 192, 128));
            // And another one, looks better with 2 pixel height
            BaseGame.DrawLine(
                new Point(xPos1, yPos + 1),
                new Point(xPos3 + TextureFont.GetTextWidth("5:67:89"), yPos + 1),
                new Color(192, 192, 192, 128));

            yPos = BaseGame.YToRes(220);

            // Go through all highscores
            for (int num = 0; num < NumOfHighscores; num++)
            {
                Rectangle lineRect = new Rectangle(
                    0, yPos, BaseGame.Width, lineHeight);
                Color col = Input.MouseInBox(lineRect) ?
                    Color.White : new Color(200, 200, 200);
                TextureFont.WriteText(xPos1, yPos,
                    (1 + num) + ".", col);
                TextureFont.WriteText(xPos2, yPos,
                    highscores[selectedLevel, num].name, col);

                TextureFont.WriteGameTime(xPos3, yPos,
                    highscores[selectedLevel, num].timeMilliseconds,
                    Color.Yellow);

                yPos += lineHeight;
            }

            BaseGame.UI.RenderBottomButtons(true);

            if (Input.KeyboardEscapeJustPressed ||
                Input.GamePadBJustPressed ||
                Input.GamePadBackJustPressed ||
                Input.MouseLeftButtonJustPressed &&
                // Don't allow clicking on the controls to quit
                Input.MousePos.Y > yPos)
                return true;

            return false;
        }
        #endregion
    }
}