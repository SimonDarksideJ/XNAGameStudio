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
    class NovaFire : ParticleSystem
    {
        public NovaFire(Game game)
            : base(game)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "SmokeTexture";

            settings.MaxParticles = 2000;

            settings.Duration = TimeSpan.FromSeconds(4);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -8;
            settings.MaxHorizontalVelocity = 8;

            settings.MinVerticalVelocity = -8;
            settings.MaxVerticalVelocity = 8;

            settings.EndVelocity = -0.5f;

            settings.MinColor = new Color(128, 64, 32);
            settings.MaxColor = new Color(255, 128, 64);

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 32;
            settings.MaxStartSize = 64;

            settings.MinEndSize = 128;
            settings.MaxEndSize = 192;

            settings.MinPositionOffset = -6f;
            settings.MaxPositionOffset = 6f;

            // Use additive blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;
        }
    }
}