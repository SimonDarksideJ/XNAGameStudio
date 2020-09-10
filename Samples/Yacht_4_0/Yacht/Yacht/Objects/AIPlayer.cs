#region File Description
//-----------------------------------------------------------------------------
// AIPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;


#endregion

namespace Yacht
{
    #region Enums


    /// <summary>
    /// Possible states for an AI player.
    /// </summary>
    enum AIState
    {
        Roll,
        Rolling,
        ChooseDice,
        SelectScore,
        WriteScore
    }


    #endregion    


    /// <summary>
    /// An AI player for the Yacht game.
    /// </summary>
    class AIPlayer : YachtPlayer
    {
        #region Fields


        static Random random = new Random();

        /// <summary>
        /// AI player's state.
        /// </summary>
        public AIState State { get; set; }


        #endregion        

        /// <summary>
        /// Initialize a new AI player.
        /// </summary>
        /// <param name="name">The player's name.</param>
        /// <param name="diceHandler">The <see cref="DiceHandler"/> that the player will use.</param>
        public AIPlayer(string name, DiceHandler diceHandler)
            : base(name, diceHandler)
        {
        }

        /// <summary>
        /// Performs the AI player's game logic, based on its current state.
        /// </summary>
        public override void PerformPlayerLogic()
        {
            // State machine for handling the AI player behavior.
            switch (State)
            {
                case AIState.Roll:
                    // Roll the dice
                    DiceHandler.Roll();
                    State = AIState.Rolling;
                    break;
                case AIState.Rolling:
                    // Wait for the dice to stop rolling
                    if (!DiceHandler.DiceRolling())
                    {
                        State = AIState.ChooseDice;
                    }
                    break;
                case AIState.ChooseDice:
                    // Hold one of the dice
                    DiceHandler.MoveDice(random.Next(0, 5));

                    // Randomly move on to selecting the score, or hold another die
                    if (DiceHandler.GetHoldingDice() != null && random.Next(0, 5) == 1)
                    {
                        State = AIState.SelectScore;
                    }
                    break;
                case AIState.SelectScore:
                    // Select an unused score line
                    if (GameStateHandler.SelectScore((YachtCombination)random.Next(1, 13)))
                    {
                        State = AIState.WriteScore;
                    }
                    break;
                case AIState.WriteScore:
                    // If a valid score was selected then write the score

                    if (GameStateHandler != null && GameStateHandler.IsScoreSelect)
                    {
                        GameStateHandler.FinishTurn();
                        DiceHandler.Reset(GameStateHandler.IsGameOver);
                        State = AIState.Roll;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
