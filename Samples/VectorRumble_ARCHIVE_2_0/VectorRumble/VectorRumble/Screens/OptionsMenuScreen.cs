#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    /// <remarks>
    /// This class is similar to one of the same name in the GameStateManagement sample.
    /// </remarks>
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields


        static string[] asteroidDensity = { "None", "Low", "Medium", "High" };
        static int currentAsteroidDensity = 2;

        static string[] wallStyle = { "None", "One", "Two", "Three" };
        static int currentWallStyle = 0;

        static int scoreLimit = 10;
        static bool motionBlur = true;
        static bool neonEffect = true;

        MenuEntry scoreLimitMenuEntry = new MenuEntry(String.Empty);
        MenuEntry asteroidDensityMenuEntry = new MenuEntry(String.Empty);
        MenuEntry wallStyleMenuEntry = new MenuEntry(String.Empty);
        MenuEntry motionBlurMenuEntry = new MenuEntry(String.Empty);
        MenuEntry neonEffectMenuEntry = new MenuEntry(String.Empty);


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor populates the menu with empty strings: the real values
        /// are filled in by the Update method to reflect the changing settings.
        /// </summary>
        public OptionsMenuScreen()
        {
            currentAsteroidDensity = (int)WorldRules.AsteroidDensity;
            currentWallStyle = (int)WorldRules.WallStyle;
            scoreLimit = WorldRules.ScoreLimit;
            motionBlur = WorldRules.MotionBlur;
            neonEffect = WorldRules.NeonEffect;

            scoreLimitMenuEntry.Selected += ScoreLimitMenuEntrySelected;
            asteroidDensityMenuEntry.Selected += AsteroidDensityMenuEntrySelected;
            wallStyleMenuEntry.Selected += WallStyleMenuEntrySelected;
            motionBlurMenuEntry.Selected += MotionBlurMenuEntrySelected;
            neonEffectMenuEntry.Selected += NeonEffectMenuEntrySelected;

            MenuEntries.Add(scoreLimitMenuEntry);
            MenuEntries.Add(asteroidDensityMenuEntry);
            MenuEntries.Add(wallStyleMenuEntry);
            MenuEntries.Add(motionBlurMenuEntry);
            MenuEntries.Add(neonEffectMenuEntry);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Updates the options screen, filling in the latest values for the menu text.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            scoreLimitMenuEntry.Text = "Score Limit : " + scoreLimit.ToString();
            asteroidDensityMenuEntry.Text = "Asteroid Density : " + 
                asteroidDensity[currentAsteroidDensity];
            wallStyleMenuEntry.Text = "Wall Style : " + wallStyle[currentWallStyle];
            motionBlurMenuEntry.Text = "Motion Blur : " + motionBlur.ToString();
            neonEffectMenuEntry.Text = "Neon Effect : " + neonEffect.ToString();
        }


        /// <summary>
        /// Event handler for when the Score Limit menu entry is selected.
        /// </summary>
        void ScoreLimitMenuEntrySelected(object sender, EventArgs e)
        {
            scoreLimit += 5;
            if (scoreLimit > 25)
                scoreLimit = 5;
        }


        /// <summary>
        /// Event handler for when the Asteroid Density menu entry is selected.
        /// </summary>
        void AsteroidDensityMenuEntrySelected(object sender, EventArgs e)
        {
            currentAsteroidDensity = (currentAsteroidDensity + 1) %
                asteroidDensity.Length;
        }


        /// <summary>
        /// Event handler for when the Wall Style menu entry is selected.
        /// </summary>
        void WallStyleMenuEntrySelected(object sender, EventArgs e)
        {
            currentWallStyle = (currentWallStyle + 1) % wallStyle.Length;
        }


        /// <summary>
        /// Event handler for when the Motion Blur menu entry is selected.
        /// </summary>
        void MotionBlurMenuEntrySelected(object sender, EventArgs e)
        {
            motionBlur = !motionBlur;
        }


        /// <summary>
        /// Event handler for when the NeonEffect menu entry is selected.
        /// </summary>
        void NeonEffectMenuEntrySelected(object sender, EventArgs e)
        {
            neonEffect = !neonEffect;
        }


        /// <summary>
        /// When the user cancels the options screen, go back to the main menu.
        /// </summary>
        protected override void OnCancel()
        {
            WorldRules.AsteroidDensity = 
                (AsteroidDensity)Enum.Parse(typeof(AsteroidDensity), 
                                         asteroidDensity[currentAsteroidDensity], true);
            WorldRules.WallStyle = (WallStyle)Enum.Parse(typeof(WallStyle), 
                wallStyle[currentWallStyle], true);
            WorldRules.ScoreLimit = scoreLimit;
            WorldRules.MotionBlur = motionBlur;
            WorldRules.NeonEffect = neonEffect;

            ExitScreen();
        }


        #endregion
    }
}
