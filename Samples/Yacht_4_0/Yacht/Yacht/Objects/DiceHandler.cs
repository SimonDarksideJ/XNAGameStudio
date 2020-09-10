#region File Description
//-----------------------------------------------------------------------------
// DiceHandler.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Xml;


#endregion

namespace Yacht
{    
    /// <summary>
    /// Manages and draws the player's dice.
    /// </summary>
    class DiceHandler : IXmlSerializable
    {
        #region Fields and Properties


        public const int DiceAmount = 5;

        /// <summary>
        /// The state of the dice managed by the dice handler.
        /// </summary>
        public DiceState DiceState { get; private set; }

        /// <summary>
        /// The amount of rolls that the player has performed.
        /// </summary>
        public int Rolls 
        { 
            get 
            { 
                return DiceState.Rolls; 
            } 
        }

        Texture2D diceRollBorder;
        Texture2D holdingTray;

        Vector2 holdingTrayPosition;
        Vector2 holdTextPosition;
        Vector2 rollBorderPosition;

        Rectangle screenBounds;


        #endregion

        #region Initializations


        /// <summary>
        /// Initialize a new dice handler.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> depicting the display.</param>
        /// <param name="state">The state of the player's dice.</param>
        public DiceHandler(GraphicsDevice graphicsDevice, DiceState state)
        {
            screenBounds = graphicsDevice.Viewport.Bounds;

            if (state == null)
            {
                DiceState = new DiceState();
                Reset(false);
            }
            else
            {
                DiceState = state;
            }
        }

        /// <summary>
        /// Loads assets used by the dice handler and performs other visual initializations.
        /// </summary>
        /// <param name="contentManager">The content manager to use when loading the assets.</param>
        public void LoadAssets(ContentManager contentManager)
        {
            diceRollBorder = contentManager.Load<Texture2D>(@"Images\diceRollBorder");
            holdingTray = contentManager.Load<Texture2D>(@"Images\holdingTray");

            // Initialize positions
            holdingTrayPosition = new Vector2(screenBounds.Left, screenBounds.Bottom - holdingTray.Bounds.Height);
            holdTextPosition = holdingTrayPosition + new Vector2(20, 30);
            rollBorderPosition = holdingTrayPosition - new Vector2(0, diceRollBorder.Height + 10);
        }


        #endregion

        #region Update and Render


        /// <summary>
        /// Update the dice handler.
        /// </summary>
        public void Update()
        {
            // Update all rolling dice
            for (int i = 0; i < DiceState.RollingDice.Length; i++)
            {
                if (DiceState.RollingDice[i] != null)
                {
                    DiceState.RollingDice[i].Update();
                }
            }

            // Place all dice on the holding tray after 3 rolls
            if (Rolls == 3 && !DiceRolling())
            {
                for (int i = 0; i < DiceAmount; i++)
                {
                    if (DiceState.RollingDice[i] != null)
                    {
                        MoveDice(i);
                    }
                }
            }

            if (DiceRolling())
            {
                AudioManager.PlaySoundRandom("Roll", 4);
            }
        }

        /// <summary>
        /// Draw the dice handler
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use when drawing the dice handler.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(holdingTray, holdingTrayPosition, Color.White);

            spriteBatch.DrawString(YachtGame.Font, "HOLD", holdTextPosition, Color.White);

            spriteBatch.Draw(diceRollBorder, rollBorderPosition, Color.White);

            DrawDice(spriteBatch);
        }

        /// <summary>
        /// Draw all the dice
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use when drawing the dice.</param>
        private void DrawDice(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < DiceAmount; i++)
            {
                if (DiceState.RollingDice[i] != null)
                {
                    DiceState.RollingDice[i].Draw(spriteBatch);
                }
                else if (DiceState.HoldingDice[i] != null)
                {
                    DiceState.HoldingDice[i].Draw(spriteBatch);
                }
            }
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Get the held dice if any exist.
        /// </summary>
        /// <returns>Return the array of held dice or null if no dice are held.</returns>
        public Dice[] GetHoldingDice()
        {
            for (int i = 0; i < DiceState.HoldingDice.Length; i++)
            {
                if (DiceState.HoldingDice[i] != null)
                {
                    return DiceState.HoldingDice;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the rolling dice if any exist.
        /// </summary>
        /// <returns>Return the array of rolling dice or null if no dice are held.</returns>
        public Dice[] GetRollingDice()
        {
            for (int i = 0; i < DiceState.RollingDice.Length; i++)
            {
                if (DiceState.RollingDice[i] != null)
                {
                    return DiceState.RollingDice;
                }
            }

            return null;
        }

        /// <summary>
        /// Resets the handler before starting new turn.
        /// </summary>
        /// <param name="gameOver">Whether or not the game is over.</param>
        public void Reset(bool gameOver)
        {
            Reset(gameOver, true);
        }

        /// <summary>
        /// Resets the handler before starting new turn.
        /// </summary>
        /// <param name="gameOver">Whether or not the game is over.</param>
        /// <param name="resetRolls">Should the roll count be reset or not.</param>
        public void Reset(bool gameOver, bool resetRolls)
        {
            if (resetRolls)
            {
                DiceState.Rolls = 0;
            }
            for (int i = 0; i < DiceAmount; i++)
            {
                // Initialize new dice and arrange them inside the rolling border
                if (gameOver)
                {
                    DiceState.RollingDice[i] = null;
                }
                else
                {

                    DiceState.RollingDice[i] = new Dice()
                    {
                        Position = new Vector2(rollBorderPosition.X + 89 * i + 30, rollBorderPosition.Y + 20)
                    };

                }

                DiceState.HoldingDice[i] = null;
            }
        }

        /// <summary>
        /// Put dice in the right positions on the screen.
        /// </summary>
        public void PositionDice()
        {
            for (int i = 0; i < DiceAmount; i++)
            {
                if (DiceState.RollingDice[i] != null)
                {
                    DiceState.RollingDice[i].Position = new Vector2(rollBorderPosition.X + 89 * i + 30,
                        rollBorderPosition.Y + 20);
                }
                else if (DiceState.HoldingDice[i] != null)
                {
                    DiceState.HoldingDice[i].Position = new Vector2(rollBorderPosition.X + 89 * i + 30,
                        rollBorderPosition.Y + 30 + holdingTray.Height);
                }
            }
        }

        /// <summary>
        /// Roll the dice inside the rolling border.
        /// </summary>
        public void Roll()
        {
            if (Rolls < 3 && !DiceRolling())
            {
                for (int i = 0; i < DiceAmount; i++)
                {
                    if (DiceState.RollingDice[i] != null)
                    {
                        DiceState.RollingDice[i].Roll();
                    }
                }

                DiceState.Rolls++;                
            }
        }

        /// <summary>
        /// Move dice from the roll border to the hold tray or the other way around.
        /// </summary>
        /// <param name="diceIndex">Index of the dice to move.</param>
        public void MoveDice(int diceIndex)
        {
            // Check if the dice can be moved
            if (Rolls > 0 && diceIndex >= 0 && diceIndex < 5)
            {
                // Check if the dice is not in the hold tray and not currently rolling
                if (DiceState.RollingDice[diceIndex] != null && !DiceState.RollingDice[diceIndex].IsRolling)
                {
                    DiceState.RollingDice[diceIndex].Position += new Vector2(0, holdingTray.Height + 10);
                    DiceState.HoldingDice[diceIndex] = DiceState.RollingDice[diceIndex];
                    DiceState.RollingDice[diceIndex] = null;
                    AudioManager.PlaySoundRandom("DieSelect", 2);
                }
                else if (DiceState.HoldingDice[diceIndex] != null)
                {
                    DiceState.HoldingDice[diceIndex].Position -= new Vector2(0, holdingTray.Height + 10);
                    DiceState.RollingDice[diceIndex] = DiceState.HoldingDice[diceIndex];
                    DiceState.HoldingDice[diceIndex] = null;
                    AudioManager.PlaySoundRandom("DieSelect", 2);
                }
            }
        }

        /// <summary>
        /// Check if dice are rolling.
        /// </summary>
        /// <returns>True if any dice are rolling and false otherwise.</returns>
        public bool DiceRolling()
        {
            bool isRolling = false;

            for (int i = 0; i < DiceState.RollingDice.Length; i++)
            {
                if (DiceState.RollingDice[i] != null)
                {
                    isRolling |= DiceState.RollingDice[i].IsRolling;
                }
            }

            return isRolling;
        }

        /// <summary>
        /// Positions the player's dice according to a supplied state.
        /// </summary>
        /// <param name="state">Dice state to use when positioning the dice.</param>
        public void LoadState(DiceState state)
        {
            DiceState = state;
            PositionDice();
        }


        #endregion

        #region IXmlSerializable Methods


        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a serialized DiceHandler object.
        /// </summary>
        /// <param name="reader">The xml reader from which to read.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            DiceState.ReadXml(reader);
        }

        /// <summary>
        /// Serializes the DiceHandler object.
        /// </summary>
        /// <param name="writer">The xml writer to use when writing.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            DiceState.WriteXml(writer);
        }


        #endregion        
    }
}
