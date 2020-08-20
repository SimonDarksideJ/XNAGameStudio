#region File Description
//-----------------------------------------------------------------------------
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SpaceShooter
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class RockTrailSmoke : ParticleSystem
    {
        public RockTrailSmoke(Game game)
            : base(game)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "SmokeTexture";

            settings.MaxParticles = 4000;

            settings.Duration = TimeSpan.FromSeconds(10);

            settings.DurationRandomness = 3.0f;

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.MinHorizontalVelocity = -0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = -0;
            settings.MaxVerticalVelocity = 0;

            settings.MinColor = new Color(64, 64, 64, 192);
            settings.MaxColor = new Color(128, 128, 128, 192);

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 0.4f;
            settings.MaxStartSize = 0.88f;

            settings.MinEndSize = 0.88f;
            settings.MaxEndSize = 1.2f;

            settings.MinPositionOffset = -1;
            settings.MaxPositionOffset = 1;
        }
    }
}