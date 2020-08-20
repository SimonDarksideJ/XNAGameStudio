#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement; 
#endregion

namespace MarbleMazeGame
{
    class BackgroundScreen : GameScreen
    {
        #region Fields
        Texture2D background; 
        #endregion

        #region Initialization
        public BackgroundScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        } 
        #endregion

        #region Loading
        /// <summary>
        /// Load screen resources
        /// </summary>
        public override void LoadContent()
        {
            background = Load<Texture2D>(@"Images\titleScreen");
        } 
        #endregion

        #region Update
        /// <summary>
        /// Update the screen
        /// </summary>
        /// <param name="gameTime">The game time</param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                            bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        } 
        #endregion

        #region Render
        /// <summary>
        /// Renders the screen
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;            

            spriteBatch.Begin();

            // Draw background
            spriteBatch.Draw(background, new Vector2(0, 0),
                 Color.White * TransitionAlpha);

            spriteBatch.End();
        } 
        #endregion
    }
}
