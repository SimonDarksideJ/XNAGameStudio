#region File Description
//-----------------------------------------------------------------------------
// Vat.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#endregion

namespace HoneycombRush
{
    /// <summary>
    /// A game component that represents the vat.
    /// </summary>
    public class Vat : TexturedDrawableGameComponent
    {
        #region Field/Properties


        ScoreBar score;
        SpriteFont font14px;
        SpriteFont font16px;
        SpriteFont font36px;
        Vector2 emptyStringSize;
        Vector2 fullStringSize;
        Vector2 timeDigStringSize;
        Vector2 timeleftStringSize;

        TimeSpan timeLeft;

        const string EmptyString = "Empty";
        const string FullString = "Full";
        const string TimeLeftString = "Time Left";
        string timeLeftString = string.Empty;

        public Vector2 Position
        {
            get
            {
                return position;
            }
        }

        public Texture2D Texture
        {
            get
            {
                return texture;
            }
        }

        public int MaxVatCapacity
        {
            get
            {
                return score.MaxValue;
            }
        }

        public int CurrentVatCapacity
        {
            get
            {
                return score.CurrentValue;
            }
        }

        public override Rectangle CentralCollisionArea
        {
            get
            {
                Rectangle bounds = Bounds;
                int height = (int)Bounds.Height / 10 * 5;
                int width = (int)Bounds.Width / 10 * 8;

                int offsetY = ((int)Bounds.Height - height) / 2;
                int offsetX = ((int)Bounds.Width - width) / 2;


                return new Rectangle((int)Bounds.X + offsetX, (int)Bounds.Y + offsetY, width, height);
            }
        }

        public Rectangle VatDepositArea
        {
            get
            {
                Rectangle bounds = Bounds;

                float sizeFactor = 0.75f;
                float marginFactor = (1 - sizeFactor) / 2;
                int x = bounds.X + (int)(marginFactor * bounds.Width);
                int y = bounds.Y + (int)(marginFactor * bounds.Height);
                int width = (int)(bounds.Width * sizeFactor);
                int height = (int)(bounds.Height * sizeFactor);

                return new Rectangle(x, y, width, height);
            }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new vat instance.
        /// </summary>
        /// <param name="game">The associated game object.</param>
        /// <param name="gamePlayScreen">Gameplay screen where the vat will be displayed.</param>
        /// <param name="texture">The vat's texture.</param>
        /// <param name="position">The position of the vat.</param>
        /// <param name="score">An associated score bar.</param>
        public Vat(Game game, GameplayScreen gamePlayScreen, Texture2D texture, Vector2 position, ScoreBar score)
            : base(game, gamePlayScreen)
        {
            this.texture = texture;
            this.position = position;
            this.score = score;

            DrawOrder = (int)(position.Y + Bounds.Height);            
        }

        /// <summary>
        /// Loads the content that will be used by this component.
        /// </summary>
        protected override void LoadContent()
        {
            font14px = Game.Content.Load<SpriteFont>("Fonts/GameScreenFont14px");
            font16px = Game.Content.Load<SpriteFont>("Fonts/GameScreenFont16px");
            font36px = Game.Content.Load<SpriteFont>("Fonts/GameScreenFont36px");

            fullStringSize = font14px.MeasureString(FullString);
            emptyStringSize = font14px.MeasureString(EmptyString);
            timeleftStringSize = font16px.MeasureString(TimeLeftString);

            base.LoadContent();
        }


        #endregion

        #region Render


        /// <summary>
        /// Draws the component.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            if (!gamePlayScreen.IsActive)
            {
                base.Draw(gameTime);
                return;
            }

            // Draws the texture
            spriteBatch.Begin();
            spriteBatch.Draw(texture, position, Color.White);

            // Draws the "time left" text
            spriteBatch.DrawString(font16px, TimeLeftString,
                position + new Vector2(texture.Width / 2 - timeleftStringSize.X / 2, timeleftStringSize.Y - 8),
                Color.White, 0, Vector2.Zero, 0, SpriteEffects.None, 2f);

            // Draws how much time is left
            timeDigStringSize = font36px.MeasureString(timeLeftString);
            Color colorToDraw = Color.White;

            if (timeLeft.Minutes == 0 && (timeLeft.Seconds == 30 || timeLeft.Seconds <= 10))
            {
                colorToDraw = Color.Red;
            }

            spriteBatch.DrawString(font36px, timeLeftString, 
                position + new Vector2(texture.Width / 2 - timeDigStringSize.X / 2, timeDigStringSize.Y - 30), 
                colorToDraw);

            // Draws the "full" and "empty" strings
            spriteBatch.DrawString(font14px, EmptyString,
                new Vector2(position.X, position.Y + texture.Height - emptyStringSize.Y), Color.White);

            spriteBatch.DrawString(font14px, FullString,
                new Vector2(position.X + texture.Width - fullStringSize.X,
                    position.Y + texture.Height - emptyStringSize.Y), 
                Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Public methods


        /// <summary>
        /// Translates time left in the game to a internal representation string.
        /// </summary>
        /// <param name="timeLeft">Time left before the current level ends.</param>
        public void DrawTimeLeft(TimeSpan timeLeft)
        {
            this.timeLeft = timeLeft;
            timeLeftString = String.Format("{0:00}:{1:00}", timeLeft.Minutes, timeLeft.Seconds);
        }

        /// <summary>
        /// Adds honey to the amount stored in the vat.
        /// </summary>
        /// <param name="value">Amount of honey to add.</param>
        public void IncreaseHoney(int value)
        {
            score.IncreaseCurrentValue(value);
        }


        #endregion
    }
}
