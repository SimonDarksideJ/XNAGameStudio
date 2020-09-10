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
    /// A component used to display and maintain the player's hit points.
    /// </summary>
    class HitPointsComponent : RestorableStateComponent
    {
        #region Fields/Properties


        SpriteBatch spriteBatch;

        Texture2D fullTexture;
        Texture2D emptyTexture;

        /// <summary>
        /// The maximal amount of hit points.
        /// </summary>
        public int TotalHitPoints { get; set; }

        /// <summary>
        /// The current amount of hit points.
        /// </summary>
        public int CurrentHitPoints { get; set; }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new instance of the component.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="fullTexture">Texture depicting a full unit of HP.</param>
        /// <param name="emptyTexture">Texture depicting a depleted unit of HP.</param>
        public HitPointsComponent(Game game, Texture2D fullTexture, Texture2D emptyTexture)
            : base(game)
        {
            this.fullTexture = fullTexture;
            this.emptyTexture = emptyTexture;

            spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
        }


        #endregion

        #region Rendering


        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            int missingHP = TotalHitPoints - CurrentHitPoints;
            Vector2 drawingPosition = GameConstants.HitPointsOrigin;

            spriteBatch.Begin();

            for (int i = 0; i < missingHP; i++)
            {
                spriteBatch.Draw(emptyTexture, drawingPosition, Color.White);
                drawingPosition.X -= (emptyTexture.Width + GameConstants.HitPointsSpace);
            }
            for (int i = 0; i < CurrentHitPoints; i++)
            {
                spriteBatch.Draw(fullTexture, drawingPosition, Color.White);
                drawingPosition.X -= (fullTexture.Width + GameConstants.HitPointsSpace);
            }

            spriteBatch.End();
        }


        #endregion
    }
}
