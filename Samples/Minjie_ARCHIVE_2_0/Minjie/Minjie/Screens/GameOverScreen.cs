#region File Description
//-----------------------------------------------------------------------------
// GameOverScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Minjie
{
    /// <summary>
    /// The game over screen displays the result of the game
    /// </summary>
    class GameOverScreen : MenuScreen
    {
        #region Fields


        GameResult gameResult;


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public GameOverScreen(GameResult gameResult)
            : base()
        {
            this.gameResult = gameResult;

            // start the title screen music
            AudioManager.PlayMusic("Music_Win");
        }


        /// <summary>
        /// Load graphics content for the screen.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            switch (gameResult)
            {
                case GameResult.Player1Won:
                    BackgroundTexture =
                        content.Load<Texture2D>("GameOver/p1_win_screen");
                    break;

                case GameResult.Player2Won:
                    BackgroundTexture =
                        content.Load<Texture2D>("GameOver/p2_win_screen");
                    break;

                case GameResult.Tied:
                    BackgroundTexture =
                        content.Load<Texture2D>("GameOver/tied_screen");
                    break;
            }

            base.LoadContent();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input.MenuSelect)
            {
                ExitScreen();
                ScreenManager.AddScreen(new TitleScreen());
            }
            else if (input.MenuCancel)
            {
                OnCancel();
            }
        }


        #endregion
    }
}
