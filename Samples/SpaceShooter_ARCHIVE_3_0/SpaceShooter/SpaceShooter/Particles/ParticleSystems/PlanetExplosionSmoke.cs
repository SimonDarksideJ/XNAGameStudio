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
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class PlanetExplosionSmoke : ParticleSystem
    {
        public PlanetExplosionSmoke(Game game)
            : base(game)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "SmokeTexture";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(3);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -2;
            settings.MaxHorizontalVelocity = 2;

            settings.MinVerticalVelocity = -2;
            settings.MaxVerticalVelocity = 2;

            settings.EndVelocity = 2;

            settings.MinColor = new Color(16, 16, 16, 192);
            settings.MaxColor = new Color(160, 160, 160, 192);

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 6;
            settings.MaxStartSize = 8;

            settings.MinEndSize = 8;
            settings.MaxEndSize = 12;

            settings.MinPositionOffset = -2;
            settings.MaxPositionOffset = 2;

            // Use alpha blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.InverseSourceAlpha;
        }
    }
}