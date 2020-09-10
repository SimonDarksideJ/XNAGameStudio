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
    class NovaSparks : ParticleSystem
    {
        public NovaSparks(Game game)
            : base(game)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "ParticleTexture";

            settings.MaxParticles = 2000;

            settings.Duration = TimeSpan.FromSeconds(8);

            settings.DurationRandomness = 6.0f;

            settings.EmitterVelocitySensitivity = 0.01f;

            settings.MinHorizontalVelocity = -8f;
            settings.MaxHorizontalVelocity = 8f;

            settings.MinVerticalVelocity = -8f;
            settings.MaxVerticalVelocity = 8f;

            settings.MinColor = new Color(255, 255, 0, 255);
            settings.MaxColor = new Color(255, 128, 0, 255);

            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 0;

            settings.MinStartSize = 0.125f;
            settings.MaxStartSize = 0.25f;

            settings.MinEndSize = 0.25f;
            settings.MaxEndSize = .50f;

            settings.MinPositionOffset = -3;
            settings.MaxPositionOffset = 3;

        }
    }
}