#region File Description
//-----------------------------------------------------------------------------
// HitPointsComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#endregion

namespace NinjAcademy
{
    /// <summary>
    /// A component used to display and maintain the game score.
    /// </summary>
    class ScoreComponent : TextDisplayComponent
    {
        #region Fields/Properties


        int score;

        /// <summary>
        /// The score to display.
        /// </summary>
        public int Score 
        { 
            get
            {
                return score;
            }
            set
            {
                score = value;
                Text = String.Format("Score: {0}", score);
            }
        }        


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new instance of the component.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="font">Font for displaying the score.</param>        
        public ScoreComponent(Game game, SpriteFont font)
            : base(game, font)
        {
            Position = GameConstants.ScorePosition;
        }


        #endregion        
    }
}
