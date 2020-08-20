#region File Description
//-----------------------------------------------------------------------------
// EndingAnimationComponent.cs
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
    /// A component that disappears once its animation becomes inactive.
    /// </summary>
    class DisappearingAnimationComponent : AnimatedComponent
    {        
        #region Initialization


        /// <summary>
        /// Creates a new component instance.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Game screen where the component will be presented.</param>
        /// <param name="animation">An animation that the component will display.</param>        
        public DisappearingAnimationComponent(Game game, GameplayScreen gameScreen, Animation animation)
            : base(game, gameScreen, animation)
        {
        }


        #endregion

        #region Update


        /// <summary>
        /// Updates the component's animation and makes it disappear if it is inactive.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!animation.IsActive)
            {
                Enabled = false;
                Visible = false;
            }
        }


        #endregion

        #region Render


        /// <summary>
        /// Draws the component's animation, using its visual center as the drawing origin.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            animation.Draw(spriteBatch, Position, 0, VisualCenter, 1, SpriteEffects.None, 0);

            spriteBatch.End();
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Shows the animation component at a specified position.
        /// </summary>
        /// <param name="position">Position where the component should be drawn. The animation's visual center
        /// will be drawn at this position.</param>
        public void Show(Vector2 position)
        {
            Position = position;
            animation.PlayFromFrameIndex(0);
            Enabled = true;
            Visible = true;
        }


        #endregion
    }
}
