#region File Description
//-----------------------------------------------------------------------------
// Player.cs
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

namespace Spacewar
{
    /// <summary>
    /// Represents the current state of each player in the game
    /// </summary>
    public class Player
    {
        /// <summary>
        /// The current ship class the player is using
        /// </summary>
        private ShipClass shipClass = ShipClass.Pencil;

        /// <summary>
        /// The current skin the player ship is using
        /// </summary>
        private int skin;

        /// <summary>
        /// The amount of cash the current player has to spend
        /// </summary>
        private int cash;

        /// <summary>
        /// The current weapon the player is using
        /// </summary>
        private ProjectileType projectileType = ProjectileType.Peashooter;

        /// <summary>
        /// The current score for this player
        /// </summary>
        private int score;

        /// <summary>
        /// The current health level for this player
        /// </summary>
        private int health = 5;

        #region Properties
        public ShipClass ShipClass
        {
            get
            {
                return shipClass;
            }
            set
            {
                shipClass = value;
            }
        }

        public int Cash
        {
            get
            {
                return cash;
            }
            set
            {
                cash = value;
            }
        }

        public int Score
        {
            get
            {
                return score;
            }
            set
            {
                score = value;
            }
        }

        public int Skin
        {
            get
            {
                return skin;
            }
            set
            {
                skin = value;
            }
        }

        public int Health
        {
            get
            {
                return health;
            }
            set
            {
                health = value;
            }
        }

        public ProjectileType ProjectileType
        {
            get
            {
                return projectileType;
            }
            set
            {
                projectileType = value;
            }
        }
        #endregion

    }
}
