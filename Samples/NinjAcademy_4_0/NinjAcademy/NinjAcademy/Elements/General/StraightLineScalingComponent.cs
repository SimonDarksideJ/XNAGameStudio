#region File Description
//-----------------------------------------------------------------------------
// StraightLineScalingComponent.cs
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
    /// A component that moves in a straight line while scaling.
    /// </summary>    
    class StraightLineScalingComponent : StraightLineMovementComponent
    {
        #region Fields/Properties


        // Amount left to change the scale in order to reach the desired scale. This is always positive, as it does
        // not indicate the nature of the scale change.
        float remainingScaleAmount;
        float scaleVelocity;

        /// <summary>
        /// The scale used when drawing the component.
        /// </summary>
        public float Scale { get; set; }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new instance of the straight line motion, scaling game component.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Game screen where the component will be presented.</param>
        /// <param name="texture">Texture asset which represents the component.</param>
        public StraightLineScalingComponent(Game game, GameplayScreen gameScreen, Texture2D texture)
            : base(game, gameScreen, texture)
        {
        }

        /// <summary>
        /// Creates a new instance of the straight line motion, scaling game component.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Game screen where the component will be presented.</param>
        /// <param name="animation">Animation object which represents the component.</param>
        public StraightLineScalingComponent(Game game, GameplayScreen gameScreen, Animation animation)
            : base(game, gameScreen, animation)
        {
        }


        #endregion

        #region Update


        /// <summary>
        /// Updates the component's scale and call the base update to update its position.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float scaleChange = scaleVelocity * elapsedSeconds;
            Scale += scaleChange;
            remainingScaleAmount -= Math.Abs(scaleChange);

            // Check whether scaling is complete
            if (remainingScaleAmount <= 0)
            {
                // The scale may have changed more than we intended, so change it back appropriately
                Scale += remainingScaleAmount * -Math.Sign(scaleChange);                
                scaleVelocity = 0;
            }
        }


        #endregion

        #region Rendering


        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            animation.Draw(spriteBatch, Position, 0, VisualCenter, Scale, SpriteEffects.None, 0);

            spriteBatch.End();
        }


        #endregion

        #region Public Methods
 

        /// <summary>
        /// Causes the component to move from a specified location, in a straight, to another location while also
        /// changing its scale.
        /// </summary>
        /// <param name="time">The time it should take the component to reach its destination and 
        /// desired scale.</param>
        /// <param name="initialPosition">Component's starting point.</param>        
        /// <param name="destinationPosition">The component's movement destination.</param>
        /// <param name="initialScale">Component's initial scale.</param>
        /// <param name="destinationScale">Component's destination scale.</param>
        /// <remarks>Scaling will complete once movement is also complete, so you may use the 
        /// <see cref="StraightLineMovementComponent.FinishedMoving"/> event for such notifications.</remarks>
        public void MoveAndScale(TimeSpan time, Vector2 initialPosition, Vector2 destinationPosition,
            float initialScale, float destinationScale)
        {
            float desiredScaleChange = destinationScale - initialScale;
            Scale = initialScale;
            float scaleDelta = destinationScale - initialScale;
            remainingScaleAmount = Math.Abs(scaleDelta);
            scaleVelocity = scaleDelta / (float)time.TotalSeconds;

            Move(time, initialPosition, destinationPosition);
        }       

        #endregion
    }
}
