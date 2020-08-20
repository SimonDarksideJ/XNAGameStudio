#region File Description
//-----------------------------------------------------------------------------
// GameConfiguration.cs
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

namespace NinjAcademy
{
    /// <summary>
    /// Describes the game's behavior during a specific stage.
    /// </summary>
    public class GamePhase
    {
        #region Properties


        /// <summary>
        /// The phase's duration. A negative time span indicates an infinite duration.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// An array containing the Target appearance interval for each conveyor belt.
        /// </summary>
        /// <remarks>The array is expected to consist of three members.</remarks>
        public TimeSpan[] TargetAppearanceIntervals {get; set;}

        /// <summary>
        /// An array containing the Target appearance probability for each conveyor belt.
        /// </summary>
        /// <remarks>The array is expected to consist of three members. Should contain values 
        /// between 0 and 1.</remarks>
        public double[] TargetAppearanceProbabilities { get; set; }

        /// <summary>
        /// The chance for any target that appears to be a gold target instead.
        /// </summary>
        /// <remarks>Should be values between 0 and 1.</remarks>
        public double GoldTargetProbablity { get; set; }

        /// <summary>
        /// Interval after which there is a chance for bamboo to be launched.
        /// </summary>
        public TimeSpan BambooAppearanceInterval { get; set; }

        /// <summary>
        /// The chance for bamboo to be launched each <see cref="BambooAppearanceInterval"/>.
        /// </summary>
        /// <remarks>Should be values between 0 and 1.</remarks>
        public double BambooAppearanceProbablity { get; set; }

        /// <summary>
        /// Interval after which there is a chance for dynamite to be launched.
        /// </summary>
        public TimeSpan DynamiteAppearanceInterval { get; set; }

        /// <summary>
        /// The chance for dynamite to be launched each <see cref="DynamiteAppearanceInterval"/>.
        /// </summary>
        /// <remarks>Should be values between 0 and 1.</remarks>
        public double DynamiteAppearanceProbablity { get; set; }

        /// <summary>
        /// An array depicting the probability of different amounts of dynamite sticks being thrown. The i-th member
        /// indicates the probability for i+1 sticks of dynamite. The members of the array should sum up to 1.
        /// </summary>
        public double[] DynamiteAmountProbabilities { get; set; }


        #endregion
    }

    /// <summary>
    /// Depicts the game's configuration.
    /// </summary>
    public class GameConfiguration
    {
        #region Properties


        /// <summary>
        /// The player's initial amount of hit-points.
        /// </summary>
        public int PlayerLives { get; set; }

        /// <summary>
        /// Points received for hitting a regular target.
        /// </summary>
        public int PointsPerTarget { get; set; }

        /// <summary>
        /// Points received for hitting a gold target.
        /// </summary>
        public int PointsPerGoldTarget { get; set; }

        /// <summary>
        /// Points received for slicing bamboo.
        /// </summary>
        public int PointsPerBamboo { get; set; }

        /// <summary>
        /// A list of all the different game phases.
        /// </summary>
        public List<GamePhase> Phases { get; set; }


        #endregion
    }
}
