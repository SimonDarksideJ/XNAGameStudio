#region File Description
//-----------------------------------------------------------------------------
// HumanPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using GameStateManagement;
using YachtServices;


#endregion

namespace Yacht
{
    /// <summary>
    /// A human player for the Yacht game.
    /// </summary>
    class HumanPlayer : YachtPlayer
    {
        #region Fields


        InputState input;
        Button roll;
        Button score;
        Rectangle screenBounds;

        GameTypes gameType;

        bool registeredForShakeDetection;
        bool shakeDetect;


        #endregion

        /// <summary>
        /// Initialize a new human player.
        /// </summary>
        /// <param name="name">The name of the player.</param>
        /// <param name="diceHandler">The <see cref="DiceHandler"/> that handles the player's dice.</param>
        /// <param name="gameType">The type of game the player is participating in.</param>
        /// <param name="input">The <see cref="InputState"/> to check for touch input.</param>
        /// <param name="rollButtonTexture">Texture for the roll button.</param>
        /// <param name="scoreButtonTexture">Texture for the score button.</param>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> depicting the display.</param>
        public HumanPlayer(string name, DiceHandler diceHandler, GameTypes gameType, InputState input,
            Rectangle screenBounds)
            : base(name, diceHandler)
        {
            this.input = input;
            this.gameType = gameType;
            this.screenBounds = screenBounds;            
        }

        /// <summary>
        /// Loads assets used by the dice handler and performs other visual initializations.
        /// </summary>
        /// <param name="contentManager">The content manager to use when loading the assets.</param>
        public void LoadAssets(ContentManager contentManager)
        {
            Texture2D rollButtonTexture = contentManager.Load<Texture2D>(@"Images\rollBtn");
            Texture2D scoreButtonTexture = contentManager.Load<Texture2D>(@"Images\scoreBtn");

            // Initialize the buttons            
            Vector2 position = new Vector2(screenBounds.Right - rollButtonTexture.Width - 10,
                screenBounds.Center.Y - rollButtonTexture.Bounds.Height);
            roll = new Button(rollButtonTexture, position, null, null);

            position.X -= scoreButtonTexture.Width + 20;
            score = new Button(scoreButtonTexture, position, null, null);

            roll.Click += roll_Click;
            score.Click += score_Click;
        }

        #region Render
        /// <summary>
        /// Draw the player input elements.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use when drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            roll.Draw(spriteBatch);
            score.Draw(spriteBatch);

            DrawRollCounter(spriteBatch);

            DrawSelectedScore(spriteBatch);
        }

        /// <summary>
        /// Draw the score currently selected by the player.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use when drawing.</param>
        private void DrawSelectedScore(SpriteBatch spriteBatch)
        {
            if (GameStateHandler != null && GameStateHandler.IsScoreSelect)
            {
                Dice[] holdingDice = DiceHandler.GetHoldingDice();
                if (holdingDice != null)
                {
                    // Calculate the score and the position
                    byte selectedScore = 
                        GameStateHandler.CombinationScore(GameStateHandler.SelectedScore.Value, holdingDice);
                    string text = 
                        GameStateHandler.ScoreTypesNames[(int)GameStateHandler.SelectedScore.Value - 1].ToUpper();
                    Vector2 position = new Vector2(score.Position.X, roll.Position.Y);
                    position.Y += roll.Texture.Height + 10;
                    Vector2 measure = YachtGame.Font.MeasureString(text);
                    position.X += score.Texture.Bounds.Center.X - measure.X / 2;
                    spriteBatch.DrawString(YachtGame.Font, text, position, Color.White);

                    text = selectedScore.ToString();
                    position.Y += measure.Y;
                    measure = YachtGame.Font.MeasureString(text);
                    position.X = score.Position.X;
                    position.X += score.Texture.Bounds.Center.X - measure.X / 2;
                    spriteBatch.DrawString(YachtGame.Font, text, position, Color.White);
                }
            }
        }

        /// <summary>
        /// Draw the amount of rolls available.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use when drawing.</param>
        private void DrawRollCounter(SpriteBatch spriteBatch)
        {
            if (DiceHandler.Rolls < 3)
            {
                string text = "ROLLS";
                Vector2 measure = YachtGame.Font.MeasureString(text);
                Vector2 position = new Vector2((int)roll.Position.X,(int)roll.Position.Y);
                position.Y += roll.Texture.Height + 10;
                position.X += roll.Texture.Bounds.Center.X - measure.X / 2;
                spriteBatch.DrawString(YachtGame.Font, text, position, Color.White);

                text = string.Format("X{0}", 3 - DiceHandler.Rolls);
                position.Y += measure.Y;
                measure = YachtGame.Font.MeasureString(text);
                position.X = roll.Position.X;
                position.X += roll.Texture.Bounds.Center.X - measure.X / 2;
                spriteBatch.DrawString(YachtGame.Font, text, position, Color.White);
            }
        }


        #endregion

        /// <summary>
        /// Handle the human player's input.
        /// </summary>
        public override void PerformPlayerLogic()
        {
            // Enable or disable buttons
            roll.Enabled = DiceHandler.Rolls != 3 && !DiceHandler.DiceRolling();
            score.Enabled = GameStateHandler != null && GameStateHandler.IsScoreSelect;

            for (int i = 0; i < input.Gestures.Count; i++)
            {
                roll.HandleInput(input.Gestures[i]);
                score.HandleInput(input.Gestures[i]);
                HandleDiceHandlerInput(input.Gestures[i]);
                HandleSelectScoreInput(input.Gestures[i]);
            }

            HandleShakeInput();
        }

        #region Private Methods
        /// <summary>
        /// Check if the phone was shaken and if so roll the dice.
        /// </summary>
        private void HandleShakeInput()
        {
            // Register for shake detection

            if (!registeredForShakeDetection)
            {
                Accelerometer.ShakeDetected += Accelerometer_ShakeDetected;
                registeredForShakeDetection = true;
            }

            if (shakeDetect)
            {
                DiceHandler.Roll();

                if (gameType == GameTypes.Online)
                {
                    NetworkManager.Instance.ResetTimeout();
                }

                shakeDetect = false;
            }
        }

        /// <summary>
        /// Highlight the score card line that was tapped.
        /// </summary>
        /// <param name="sample">Input gesture performed.</param>
        private void HandleSelectScoreInput(GestureSample sample)
        {
            if (sample.GestureType == GestureType.Tap)
            {
                // Create the touch rectangle
                Rectangle touchRect = new Rectangle((int)sample.Position.X - 5, (int)sample.Position.Y - 5, 10, 10);

                for (int i = 0; i < 12; i++)
                {
                    if (GameStateHandler.IntersectLine(touchRect, i))
                    {
                        GameStateHandler.SelectScore((YachtCombination)(i + 1));
                    }
                }
            }
        }

        /// <summary>
        /// Move dice that are tapped.
        /// </summary>
        /// <param name="sample">Input gesture performed.</param>
        private void HandleDiceHandlerInput(GestureSample sample)
        {
            if (DiceHandler.Rolls < 3)
            {
                Dice[] rollingDice = DiceHandler.GetRollingDice();
                Dice[] holdingDice = DiceHandler.GetHoldingDice();

                if (sample.GestureType == GestureType.Tap)
                {
                    // Create the touch rectangle
                    Rectangle touchRect = new Rectangle((int)sample.Position.X - 5, (int)sample.Position.Y - 5, 10, 10);

                    for (int i = 0; i < DiceHandler.DiceAmount; i++)
                    {
                        // Check for intersection between the touch rectangle and any of the dice
                        if ((rollingDice != null && rollingDice[i] != null &&
                            !rollingDice[i].IsRolling && rollingDice[i].Intersects(touchRect)) ||
                            (holdingDice != null && holdingDice[i] != null && holdingDice[i].Intersects(touchRect)))
                        {
                            DiceHandler.MoveDice(i);
                            if (DiceHandler.GetHoldingDice() == null)
                            {
                                GameStateHandler.SelectScore(null);
                            }
                        }
                    }
                }
            }
        }


        #endregion

        #region Event Handlers


        /// <summary>
        /// Handle the "Score" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void score_Click(object sender, EventArgs e)
        {
            if (GameStateHandler != null && GameStateHandler.IsScoreSelect)
            {
                GameStateHandler.FinishTurn();
                AudioManager.PlaySoundRandom("Pencil", 3);
                DiceHandler.Reset(GameStateHandler.IsGameOver);
            }
        }

        /// <summary>
        /// Handle the "Roll" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void roll_Click(object sender, EventArgs e)
        {
            DiceHandler.Roll();

            if (gameType== GameTypes.Online)
            {
                NetworkManager.Instance.ResetTimeout();
            }
        }

        /// <summary>
        /// Handle shake detection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Accelerometer_ShakeDetected(object sender, EventArgs e)
        {
            shakeDetect = true;
        }


        #endregion
    }
}
