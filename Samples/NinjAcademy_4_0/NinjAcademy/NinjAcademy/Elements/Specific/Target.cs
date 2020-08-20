#region File Description
//-----------------------------------------------------------------------------
// Target.cs
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
    /// Possible target positions on the screen.
    /// </summary>
    enum TargetPosition
    {
        Upper,
        Middle,
        Lower,
        Anywhere
    }

    class Target : StraightLineMovementComponent
    {
        #region Properties


        /// <summary>
        /// Gets or sets the target's designation, which is based on its position.
        /// </summary>
        public TargetPosition Designation { get; set; }

        /// <summary>
        /// Whether this target is golden or not.
        /// </summary>
        public bool IsGolden { get; set; }

        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new instance of the target component.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Game screen where the component will be presented.</param>
        /// <param name="texture">Texture which represents the component.</param>
        public Target(Game game, GameplayScreen gameScreen, Texture2D texture)
            : base(game, gameScreen, texture)
        {
        }

        /// <summary>
        /// Creates a new instance of the target component.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Game screen where the component will be presented.</param>
        /// <param name="animation">Animation which represents the component.</param>
        public Target(Game game, GameplayScreen gameScreen, Animation animation)
            : base(game, gameScreen, animation)
        {
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Checks whether a specified position constitutes as a hit on the target.
        /// </summary>
        /// <param name="hitLocation">The position to check. The position's Z component is expected to be 0.</param>
        /// <returns>True if the position is contained in the target and false otherwise.</returns>
        public bool CheckHit(Vector3 hitLocation)
        {
            BoundingSphere exactBounds = new BoundingSphere(new Vector3(Position, 0), GameConstants.TargetRadius);

            return exactBounds.Contains(hitLocation) == ContainmentType.Contains;
        }


        #endregion
    }
}
