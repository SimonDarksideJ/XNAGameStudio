#region File Description
//-----------------------------------------------------------------------------
// MessageBoxScreen.cs
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
using Microsoft.Xna.Framework.Input.Touch;
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    /// <remarks>
    /// Similar to a class found in the Game State Management sample on the 
    /// XNA Creators Club Online website (http://creators.xna.com).
    /// </remarks>
    class MessageBoxScreen : GameScreen
    {
        #region Fields


        string message;

        private Texture2D backgroundTexture;
        private Vector2 backgroundPosition;

        private Texture2D loadingBlackTexture;
        private Rectangle loadingBlackTextureDestination;

        private Texture2D backTexture;
        private Vector2 backPosition;

        private Texture2D selectTexture;
        private Vector2 selectPosition;

        private Vector2 confirmPosition, messagePosition;


        #endregion


        #region Events

        public event EventHandler<EventArgs> Accepted;
        public event EventHandler<EventArgs> Cancelled;

        #endregion


        #region Initialization


        /// <summary>
        /// Constructor lets the caller specify the message.
        /// </summary>
        public MessageBoxScreen(string message)
        {
            this.message = message;

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);
        }


        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            backgroundTexture = content.Load<Texture2D>(@"Textures\MainMenu\Confirm");
            backTexture = content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            selectTexture = content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            loadingBlackTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            backgroundPosition = new Vector2(
                (viewport.Width - (backgroundTexture.Width * ScaledVector2.DrawFactor))  / 2,
                (viewport.Height - (backgroundTexture.Height * ScaledVector2.DrawFactor))  / 2);
            loadingBlackTextureDestination = new Rectangle(viewport.X, viewport.Y,
                viewport.Width, viewport.Height);

            backPosition = backgroundPosition + new Vector2(20f,
                backgroundTexture.Height * ScaledVector2.DrawFactor - 110 * ScaledVector2.ScaleFactor);
            selectPosition = backgroundPosition + new Vector2(
                backgroundTexture.Width * ScaledVector2.DrawFactor - 200 * ScaledVector2.ScaleFactor,
                backgroundTexture.Height * ScaledVector2.DrawFactor - 110 * ScaledVector2.ScaleFactor);

            confirmPosition.X = backgroundPosition.X + (backgroundTexture.Width * ScaledVector2.DrawFactor -
                Fonts.HeaderFont.MeasureString("Confirmation").X) / 2f;
            confirmPosition.Y = backgroundPosition.Y + 47 * ScaledVector2.ScaleFactor;

            message = Fonts.BreakTextIntoLines(message, 36, 10);
            messagePosition.X = backgroundPosition.X + (int)((backgroundTexture.Width * ScaledVector2.DrawFactor -
                Fonts.GearInfoFont.MeasureString(message).X) / 2);
            messagePosition.Y = (backgroundPosition.Y * 2) - 20 * ScaledVector2.ScaleFactor;
        }


        #endregion


        #region Handle Input


        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput()
        {
            bool confirmationClicked = false;
            bool backClicked = false;

                
             if (InputManager.IsButtonClicked(new Rectangle(
                    (int)selectPosition.X,
                    (int)selectPosition.Y,
                    (int)(backTexture.Width * ScaledVector2.DrawFactor ),
                    (int)(backTexture.Height * ScaledVector2.DrawFactor ))))
            {
                confirmationClicked = true;
            }

            if (InputManager.IsButtonClicked(new Rectangle
                    ((int)backPosition.X,
                    (int)backPosition.Y,
                    (int)(backTexture.Width * ScaledVector2.DrawFactor ), 
                    (int)(backTexture.Height * ScaledVector2.DrawFactor ))))
            {
                backClicked = true;
            }


            if (confirmationClicked)
            { 
                // Raise the accepted event, then exit the message box.
                if (Accepted != null)
                    Accepted(this, EventArgs.Empty);

                ExitScreen();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.Back) || backClicked)
            {
                // Raise the cancelled event, then exit the message box.
                if (Cancelled != null)
                    Cancelled(this, EventArgs.Empty);

                ExitScreen();
            }
        }


        #endregion


        #region Draw


        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();
            var position = new Vector2(loadingBlackTextureDestination.X, loadingBlackTextureDestination.Y);
            spriteBatch.Draw(loadingBlackTexture, position, null,Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            spriteBatch.Draw(backgroundTexture, backgroundPosition, null, Color.White, 0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            spriteBatch.Draw(backTexture, backPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            spriteBatch.Draw(selectTexture, selectPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            string noText = "No";
            Vector2 noTextPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, noText,
                new Rectangle((int)backPosition.X, (int)backPosition.Y,
                    (int)(backTexture.Width), (int)(backTexture.Height )));

            spriteBatch.DrawString(Fonts.ButtonNamesFont, noText,noTextPosition,Color.White);


            string yesText = "Yes";
            Vector2 yesTextPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, yesText,
                new Rectangle((int)selectPosition.X, (int)selectPosition.Y,
                    (int)(selectTexture.Width ), (int)(selectTexture.Height )));

            spriteBatch.DrawString(Fonts.ButtonNamesFont, yesText, yesTextPosition, Color.White);
            spriteBatch.DrawString(Fonts.HeaderFont, "Confirmation", confirmPosition,
                Fonts.CountColor);
            spriteBatch.DrawString(Fonts.GearInfoFont, message, messagePosition, 
                Fonts.CountColor);

            spriteBatch.End();
        }


        #endregion
    }
}
