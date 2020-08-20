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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Displays the game-over screen, after the player has lost.
    /// </summary>
    class GameOverScreen : GameScreen
    {
        #region Graphics Data


        private Texture2D backTexture;
        private Texture2D selectIconTexture;
        private Texture2D fadeTexture;
        private Vector2 backgroundPosition;
        private Vector2 titlePosition;
        private Vector2 gameOverPosition;
        private Vector2 selectPosition;
        private Vector2 selectIconPosition;


        #endregion


        #region Text Data


        private readonly string titleString = "Game Over";
        private readonly string gameOverString = "The party has been defeated.";
        private readonly string selectString = "Continue";


        #endregion


        #region Initialization


        /// <summary>
        /// Create a new GameOverScreen object.
        /// </summary>
        public GameOverScreen()
            : base()
        {
            AudioManager.PushMusic("LoseTheme",false);
            this.Exiting += new EventHandler(GameOverScreen_Exiting);
        }


        void GameOverScreen_Exiting(object sender, EventArgs e)
        {
            AudioManager.PopMusic();
        }


        /// <summary>
        /// Load the graphics data from the content manager.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            fadeTexture = content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");
            backTexture = content.Load<Texture2D>(@"Textures\GameScreens\PopupScreen");
            selectIconTexture = content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");

            backgroundPosition.X = (viewport.Width - backTexture.Width * ScaledVector2.DrawFactor) / 2;
            backgroundPosition.Y = (viewport.Height - backTexture.Height * ScaledVector2.DrawFactor) / 2;

            titlePosition.X = (viewport.Width -
                Fonts.HeaderFont.MeasureString(titleString).X) / 2;
            titlePosition.Y = backgroundPosition.Y + 70f * ScaledVector2.ScaleFactor;

            gameOverPosition.X = (viewport.Width /2 -
                Fonts.ButtonNamesFont.MeasureString(gameOverString).X / 2);
            gameOverPosition.Y = backgroundPosition.Y + backTexture.Height * ScaledVector2.DrawFactor / 2;

            selectIconPosition.X = viewport.Width / 2 + 260 * ScaledVector2.ScaleFactor;
            selectIconPosition.Y = backgroundPosition.Y + 530f * ScaledVector2.ScaleFactor;
            selectPosition.X = selectIconPosition.X -
                Fonts.ButtonNamesFont.MeasureString(selectString).X - 10f * ScaledVector2.ScaleFactor;
            selectPosition.Y = backgroundPosition.Y + 530f * ScaledVector2.ScaleFactor;
        }


        #endregion


        #region Updating


        /// <summary>
        /// Handles user input.
        /// </summary>
        public override void HandleInput()
        {
            bool backClicked = false;

            if(InputManager.IsButtonClicked(new Rectangle(
                (int)backgroundPosition.X,(int)backgroundPosition.Y,
                (int)(backTexture.Width * ScaledVector2.DrawFactor),
                (int)(backTexture.Height * ScaledVector2.DrawFactor))))
            {
                backClicked = true;
            }

            if (backClicked ||
                InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                ExitScreen();
                ScreenManager.AddScreen(new MainMenuScreen());
                return;
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draws the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // Draw fading screen
            spriteBatch.Draw(fadeTexture, new Rectangle(0, 0, 1280, 720), Color.White);

            // Draw popup texture
            spriteBatch.Draw(backTexture, backgroundPosition,null, Color.White,0f,
                Vector2.Zero,ScaledVector2.DrawFactor, SpriteEffects.None,0f);

            // Draw title
            spriteBatch.DrawString(Fonts.HeaderFont, titleString, titlePosition,
                Fonts.TitleColor);

            // Draw Gameover text
            spriteBatch.DrawString(Fonts.ButtonNamesFont, gameOverString,
                gameOverPosition, Fonts.CountColor);



            spriteBatch.Draw(selectIconTexture, selectIconPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            Vector2 selectTextPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, selectString,
                    new Rectangle((int)selectIconPosition.X, (int)selectIconPosition.Y,
                        selectIconTexture.Width, selectIconTexture.Height));

            spriteBatch.DrawString(Fonts.ButtonNamesFont, selectString, selectTextPosition,
                Color.White);

            spriteBatch.End();
        }


        #endregion
    }
}
