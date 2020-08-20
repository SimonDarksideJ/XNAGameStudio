#region File Description
//-----------------------------------------------------------------------------
// BasicAIPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using YachtServices; 


#endregion

namespace Yacht
{


    /// <summary>
    /// A basic AI player that performs random moves.
    /// </summary>
    public class AIPlayerBehavior
    {
        enum YachtCombination
        {
            Yacht = 12,
            LargeStraight = 11,
            SmallStraight = 10,
            FourOfAKind = 9,
            FullHouse = 8,
            Choise = 7,
            Sixes = 6,
            Fives = 5,
            Fours = 4,
            Threes = 3,
            Twos = 2,
            Ones = 1
        }


        #region Fields
        

        static Random random = new Random();

        #endregion

        /// <summary>
        /// Randomize how much dice where selected and there value, and then choose the highest score available.  
        /// </summary>
        /// <param name="scoreCard">array containing the entire scores of the player.</param>
        /// <returns>The best move available.</returns>
        public YachtStep Play(byte[] scoreCard)
        {
            // Randomize how many die was selected
            int[] dice = new int[random.Next(1,5)];

            // Randomize for each selected die value
            for (int i = 0; i < dice.Length; i++)
            {
                dice[i] = random.Next(1, 6);
            }

            byte highestScore = byte.MinValue;
            int scoreCardIndex = -1;

            // Goes over all the score option, and gets the highest
            for (int i = 0; i < (int)YachtCombination.Yacht; i++)
            {
                // Checks that the current score cell in the card is free
                if (scoreCard[i] == ServiceConstants.NullScore)
                {
                    byte currentScore = CalculateDiceScore(dice, (YachtCombination)i+1);
                    if (currentScore > highestScore)
                    {
                        highestScore = currentScore;
                        scoreCardIndex = i;
                    }
                }
            }

            // Occurs when the score is zero
            if (scoreCardIndex == -1)
            {
                for (int i = 0; i < scoreCard.Length; i++)
                {
                    if (scoreCard[i] == ServiceConstants.NullScore)
                    {
                        scoreCardIndex = i;
                        break;
                    }
                }
            }

            return new YachtStep(scoreCardIndex, highestScore, 0, 0);
        }

        /// <summary>
        /// Calculate the 
        /// </summary>
        /// <param name="dice"></param>
        /// <param name="stepType"></param>
        /// <returns></returns>
        private byte CalculateDiceScore(int[] dice,YachtCombination stepType)
        {
            // Get the first and the last dice for calculation.
            int first = dice[0];
            int last = dice[dice.Length -1];
            switch (stepType)
            {
                case YachtCombination.Yacht:
                    // If all dice are same.
                    if (Times(dice, first) == 5)
                    {
                        return 50;
                    }
                    return 0;
                case YachtCombination.LargeStraight:
                    // Dice is 2-6.
                    if (FollowingDice(dice) && last == 6)
                    {
                        return 30;
                    }
                    return 0;
                case YachtCombination.SmallStraight:
                    // Dice is 1-5.
                    if (FollowingDice(dice) && last == 5)
                    {
                        return 30;
                    }
                    return 0;
                case YachtCombination.FourOfAKind:
                    // 4 Dice are identical.
                    if (Times(dice, first) >= 4 || Times(dice, last) >= 4)
                    {
                        return Sum(dice, null);
                    }
                    return 0;
                case YachtCombination.FullHouse:
                    // 2 dice of the same value and 3 dice on the save value but different value from the other two.
                    if ((Times(dice, first) == 3 && Times(dice, last) == 2) ||
                    (Times(dice, first) == 2 && Times(dice, last) == 3))
                    {
                        return Sum(dice, null);
                    }
                    return 0;
                case YachtCombination.Choise:
                    return Sum(dice, null);
                case YachtCombination.Sixes:
                case YachtCombination.Fives:
                case YachtCombination.Fours:
                case YachtCombination.Threes:
                case YachtCombination.Twos:
                case YachtCombination.Ones:
                    // Calculate the some of the current value.
                    return Sum(dice, (int)stepType);
                default:
                    throw new Exception("Cannot calculate Value for this dice");
            }
        }

            
        /// <summary>
        /// Calculate the sum of the dice.
        /// </summary>
        /// <param name="dice">The dice to calculate their sum</param>
        /// <param name="value">The die to compare.</param>
        /// <returns>The sum of the dice.</returns>
        byte Sum(int[] dice, int? value)
        {
            byte sum = 0;

            for (int i = 0; i < dice.Length; i++)
            {
                if ((value.HasValue && dice[i] == value.Value))
                {
                    sum += (byte)dice[i];
                }
            }

            return sum;
        }

        /// <summary>
        /// Calculate how much time occurs die in the dice array.
        /// </summary>
        /// <param name="dice">The dice for calculation.</param>
        /// <param name="value">The die to compare.</param>
        /// <returns>Times of dice occurs in the dice array.</returns>
        int Times(int[] dice, int value)
        {
            int count = 0;

            for (int i = 0; i < dice.Length; i++)
            {
                if (dice[i] == value)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Check if array of die is following.
        /// </summary>
        /// <param name="dice">The array of die.</param>
        /// <returns>Returns if the dice is following.</returns>
        bool FollowingDice(int[] dice)
        {
            int count = 0;
            for (int i = 0; i < dice.Length - 1; i++)
            {
                if (dice[i] + 1 == dice[i + 1])
                {
                    count++;
                }
            }
            return count == dice.Length - 1;
        }
        
    }
}
