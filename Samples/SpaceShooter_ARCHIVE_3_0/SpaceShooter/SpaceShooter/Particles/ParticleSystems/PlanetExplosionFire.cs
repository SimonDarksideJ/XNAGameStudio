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
    /// Custom particle system for creating the fiery part of the explosions.
    /// </summary>
    class PlanetExplosionFire : ParticleSystem
    {
        public PlanetExplosionFire(Game game)
            : base(game)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "SmokeTexture";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(2);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -1;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            settings.EndVelocity = 1;

            settings.MinColor = new Color(128, 64, 32);
            settings.MaxColor = new Color(255, 128, 64);

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 4;
            settings.MaxStartSize = 6;

            settings.MinEndSize = 6;
            settings.MaxEndSize = 8;

            settings.MinPositionOffset = -1.5f;
            settings.MaxPositionOffset = 1.5f;

            // Use additive blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;
        }
    }
}