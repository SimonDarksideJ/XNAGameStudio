#region File Description
//-----------------------------------------------------------------------------
// YachtPlayer.cs
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
#endregion

namespace Yacht
{

    abstract class YachtPlayer
    {
        #region Properties


        /// <summary>
        /// The player's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The dice handler used by the player.
        /// </summary>
        public DiceHandler DiceHandler { get; private set; }

        /// <summary>
        /// The game state handler used by the player.
        /// </summary>
        public GameStateHandler GameStateHandler { get; set; }


        #endregion

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="name">The name of the player.</param>
        /// <param name="diceHandler">The <see cref="DiceHandler"/> that the player will use.</param>
        public YachtPlayer(string name, DiceHandler diceHandler)
        {
            Name = name;
            DiceHandler = diceHandler;
        }

        /// <summary>
        /// Allows the player to perform a portion of his playing logic.
        /// </summary>
        public virtual void PerformPlayerLogic() 
        { 
        }

        /// <summary>
        /// Draws information related to the player.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use for drawing.</param>
        public virtual void Draw(SpriteBatch spriteBatch) 
        { 
        }
    }
}
