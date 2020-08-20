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
    class RockTrailSparks : ParticleSystem
    {
        public RockTrailSparks(Game game)
            : base(game)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "ParticleTexture";

            settings.MaxParticles = 4000;

            settings.Duration = TimeSpan.FromSeconds(6);

            settings.DurationRandomness = 2.0f;

            settings.EmitterVelocitySensitivity = 0.01f;

            settings.MinHorizontalVelocity = -0.25f;
            settings.MaxHorizontalVelocity = 0.25f;

            settings.MinVerticalVelocity = -0.25f;
            settings.MaxVerticalVelocity = 0.25f;

            settings.MinColor = new Color(255, 255, 0, 255);
            settings.MaxColor = new Color(255, 128, 0, 255);

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 0.15f;
            settings.MaxStartSize = 0.15f;

            settings.MinEndSize = 0.250f;
            settings.MaxEndSize = .45f;

            settings.MinPositionOffset = -.1f;
            settings.MaxPositionOffset = .1f;

        }
    }
}