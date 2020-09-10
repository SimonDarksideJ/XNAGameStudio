#region File Description
//-----------------------------------------------------------------------------
// ThrowingStar.cs
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


#endregion

namespace NinjAcademy
{
    /// <summary>
    /// Represents a throwing star.
    /// </summary>
    class ThrowingStar : StraightLineScalingComponent
    {
        #region Fields


        Random random;


        #endregion        

        #region Initialization


        /// <summary>
        /// Creates a new instance of the throwing star component.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Game screen where the component will be presented.</param>
        /// <param name="animation">Animation object which represents the component.</param>
        public ThrowingStar(Game game, GameplayScreen gameScreen, Animation animation)
            : base(game, gameScreen, animation)
        {
            random = new Random();
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Launches the throwing star to a specified destination. Enables the component and makes it visible as well.
        /// </summary>
        /// <param name="destination">Throwing star's destination.</param>
        public void Throw(Vector2 destination)
        {
            Enabled = true;
            Visible = true;

            // cause each star thrown to spin differently            
            animation.FrameIndex = random.Next(animation.FrameCount);

            MoveAndScale(GameConstants.ThrowingStarFlightDuration, GameConstants.ThrowingStarOrigin,
                destination, 1, GameConstants.ThrowingStarEndScale);
        }


        #endregion
    }
}
