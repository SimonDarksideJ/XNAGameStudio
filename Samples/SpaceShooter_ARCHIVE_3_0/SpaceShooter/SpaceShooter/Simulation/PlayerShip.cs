#region File Description
//-----------------------------------------------------------------------------
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SpaceShooter
{
    class PlayerShip : SpaceShip
    {
        public PlayerShip(Game game, ParticleManager particles)
            : base(game, particles)
        {
            Reset(new Vector3(0, 0, 0));

            SpaceShipControlType = ShipControlType.PlayerControl;
        }

        public override void Update(GameTime gameTime)
        {
            ReadSpaceShipInputs(PlayerIndex.One);

            base.Update(gameTime);
        }

        /// <summary>
        /// Reads the keyboard and Xbox 360 Controller to determine what the player is doing
        /// </summary>
        /// <param name="playerIndex"></param>
        public override void ReadSpaceShipInputs(PlayerIndex playerIndex)
        {
            GamePadState gamePadState = GamePad.GetState(playerIndex);
            ShipInput.Reset();

            // Pitch, Yaw, and Roll.
            ShipInput.PitchAngle = gamePadState.ThumbSticks.Right.Y;
            ShipInput.YawAngle = gamePadState.ThumbSticks.Right.X;
            ShipInput.RollAngle = gamePadState.ThumbSticks.Left.X;

            // Keyboard control
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up))
                ShipInput.PitchAngle = 1.0f;

            if (keyState.IsKeyDown(Keys.Down))
                ShipInput.PitchAngle = -1.0f;

            if (keyState.IsKeyDown(Keys.Left))
                ShipInput.YawAngle = -1.0f;

            if (keyState.IsKeyDown(Keys.Right))
                ShipInput.YawAngle = 1.0f;

            if (keyState.IsKeyDown(Keys.A))
                ShipInput.RollAngle = -1.0f;

            if (keyState.IsKeyDown(Keys.D))
                ShipInput.RollAngle = 1.0f;

            ShipInput.SpeedOffset = gamePadState.ThumbSticks.Left.Y;

            ShipInput.Fired = (gamePadState.Triggers.Right > 0.5f);

            // Keyboard control - pt2
            if (keyState.IsKeyDown(Keys.W))
                ShipInput.SpeedOffset = 1.0f;

            if (keyState.IsKeyDown(Keys.S))
                ShipInput.SpeedOffset = -1.0f;

            if (keyState.IsKeyDown(Keys.Space))
                ShipInput.Fired = true;

            base.ReadSpaceShipInputs(playerIndex);
        }

        protected override void ExecuteControl(GameTime gameTime, float dt, Matrix m)
        {
            base.ExecuteControl(gameTime, dt, m);
        }

        public override void Draw(GameTime gameTime, Camera camera)
        {
            // No need to draw the targeting HUD box on the player ship!
            drawHudBox = false;

            base.Draw(gameTime, camera);
        }
    }
}