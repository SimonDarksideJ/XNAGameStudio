#region File Description
//-----------------------------------------------------------------------------
// StaticTextureComponent.cs
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
    /// A component that simply displays a texture at a specified location.
    /// </summary>
    class StaticTextureComponent : TexturedDrawableGameComponent
    {
        #region Initialization


        /// <summary>
        /// Creates a new component instance.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Game screen where the component will be presented.</param>
        /// <param name="texture">Texture asset which represents the component.</param>
        /// <param name="position">Position where the component should be drawn.</param>
        public StaticTextureComponent(Game game, GameplayScreen gameScreen, Texture2D texture, Vector2 position)
            : base(game, gameScreen, texture)
        {
            Position = position;
        }


        #endregion

        #region Rendering


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            spriteBatch.Begin();

            spriteBatch.Draw(texture, Position, Color.White);

            spriteBatch.End();
        }


        #endregion
    }
}
