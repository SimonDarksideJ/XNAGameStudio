#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region File Information
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
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace CatapultGame
{
    internal class Player : DrawableGameComponent
    {
        #region Variables/Fields
        protected CatapultGame curGame;
        protected SpriteBatch spriteBatch;

        // Constants used for calculating shot strength
        public const float MinShotStrength = 150;
        public const float MaxShotStrength = 400;

        // Public variables used by Gameplay class
        public Catapult Catapult { get; set; }
        public int Score { get; set; }
        public string Name { get; set; }

        public Player Enemy
        {
            set
            {
                Catapult.Enemy = value;
                Catapult.Self = this;
            }
        }

        public bool IsActive { get; set; }
        #endregion

        #region Initialization
        public Player(Game game)
            : base(game)
        {
            curGame = (CatapultGame)game;
        }

        public Player(Game game, SpriteBatch screenSpriteBatch)
            : this(game)
        {
            spriteBatch = screenSpriteBatch;
        }

        public override void Initialize()
        {
            Score = 0;

            base.Initialize();
        }
        #endregion

        #region Update and Render
        public override void Update(GameTime gameTime)
        {
            // Update catapult related to the player
            Catapult.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Draw related catapults
            Catapult.Draw(gameTime);
            base.Draw(gameTime);
        }
        #endregion
    }
}
