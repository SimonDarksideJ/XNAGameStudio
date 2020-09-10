#region File Description
//-----------------------------------------------------------------------------
// ParticleDemo.cs
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
using SkinnedModel;
#endregion

namespace XnaGraphicsDemo
{
    /// <summary>
    /// Demo shows how to use SpriteBatch.
    /// </summary>
    class ParticleDemo : MenuComponent
    {
        const int MaxParticles = 5000;

        struct Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Size;
            public float Rotation;
            public float Spin;
            public Color Color;
        }

        Particle[] particles = new Particle[MaxParticles];

        int firstParticle;
        int particleCount;

        FloatMenuEntry spawnRate;
        float spawnCounter;

        Texture2D cat;

        Random random = new Random();


        /// <summary>
        /// Constructor.
        /// </summary>
        public ParticleDemo(DemoGame game)
            : base(game)
        {
            Entries.Add(spawnRate = new FloatMenuEntry() { Text = "spawn rate" });

            // This menu option for changing the resolution is currently disabled,
            // because the image scaler feature is not yet implemented in the CTP release.
            /*
            Entries.Add(new ResolutionMenu(game.graphics));
            */

            Entries.Add(new MenuEntry
            {
                Text = "back",
                Clicked = delegate
                {
                    // Before we quit back out of this menu, reset back to the default resolution.
                    if (game.Graphics.PreferredBackBufferWidth != 480)
                    {
                        game.Graphics.PreferredBackBufferWidth = 480;
                        game.Graphics.PreferredBackBufferHeight = 800;

                        game.Graphics.ApplyChanges();
                    }
                    
                    Game.SetActiveMenu(0);
            } });
        }


        /// <summary>
        /// Resets the menu state.
        /// </summary>
        public override void Reset()
        {
            firstParticle = 0;
            particleCount = 0;
            spawnRate.Value = 0.2f;

            base.Reset();
        }


        /// <summary>
        /// Loads content for this demo.
        /// </summary>
        protected override void LoadContent()
        {
            cat = Game.Content.Load<Texture2D>("cat");
        }


        /// <summary>
        /// Updates the particle system.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            int i = firstParticle;

            for (int j = particleCount; j > 0; j--)
            {
                // Move a particle.
                particles[i].Position += particles[i].Velocity;
                particles[i].Rotation += particles[i].Spin;
                particles[i].Velocity.Y += 0.1f;

                // Retire old particles?
                const float borderPadding = 96;

                if (i == firstParticle)
                {
                    if ((particles[i].Position.X < -borderPadding) ||
                        (particles[i].Position.X > 480 + borderPadding) ||
                        (particles[i].Position.Y < -borderPadding) ||
                        (particles[i].Position.Y > 800 + borderPadding))
                    {
                        if (++firstParticle >= MaxParticles)
                            firstParticle = 0;

                        particleCount--;
                    }
                }

                if (++i >= MaxParticles)
                    i = 0;
            }

            // Spawn new particles?
            spawnCounter += spawnRate.Value * 10;

            while (spawnCounter > 1)
            {
                SpawnParticle(null);
                spawnCounter--;
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Helper creates a new cat particle.
        /// </summary>
        void SpawnParticle(Vector2? position)
        {
            if (particleCount >= MaxParticles)
                return;

            int i = firstParticle + particleCount;

            if (i >= MaxParticles)
                i -= MaxParticles;

            particles[i].Position = position ?? new Vector2((float)random.NextDouble() * 480, (float)random.NextDouble() * 800);
            particles[i].Velocity = new Vector2((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f) * 10f;
            particles[i].Size = (float)random.NextDouble() * 0.5f + 0.5f;
            particles[i].Rotation = 0;
            particles[i].Spin = ((float)random.NextDouble() - 0.5f) * 0.1f;

            if (position.HasValue)
            {
                // Explicitly positioned particles have no tint.
                particles[i].Color = Color.White;
            }
            else
            {
                // Randomly positioned particles have random tint colors.
                byte r = (byte)(128 + random.NextDouble() * 127);
                byte g = (byte)(128 + random.NextDouble() * 127);
                byte b = (byte)(128 + random.NextDouble() * 127);

                particles[i].Color = new Color(r, g, b);
            }

            particleCount++;
        }


        /// <summary>
        /// Draws the cat particle system.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            DrawTitle("particles", Color.CornflowerBlue, Color.Lerp(Color.Blue, Color.CornflowerBlue, 0.85f));

            SpriteBatch.Begin(0, null, null, null, null, null, Game.ScaleMatrix);

            Vector2 origin = new Vector2(cat.Width, cat.Height) / 2;

            int i = firstParticle + particleCount - 1;

            if (i >= MaxParticles)
                i -= MaxParticles;

            for (int j = 0; j < particleCount; j++)
            {
                SpriteBatch.Draw(cat, particles[i].Position, null, particles[i].Color, particles[i].Rotation, origin, particles[i].Size, 0, 0);

                if (--i < 0)
                    i = MaxParticles - 1;
            }

            SpriteBatch.End();

            base.Draw(gameTime);
        }


        /// <summary>
        /// Dragging on the menu background creates new particles.
        /// </summary>
        protected override void OnDrag(Vector2 delta)
        {
            SpawnParticle(LastTouchPoint);
        }


        /// <summary>
        /// Custom menu entry subclass for cycling through different backbuffer resolutions.
        /// </summary>
        class ResolutionMenu : MenuEntry
        {
            GraphicsDeviceManager graphics;


            public ResolutionMenu(GraphicsDeviceManager graphics)
            {
                this.graphics = graphics;
            }


            public override string Text
            {
                get { return string.Format("{0}x{1}", graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight); }
                set { }
            }


            public override void OnClicked()
            {
                switch (graphics.PreferredBackBufferWidth)
                {
                    case 480:
                        graphics.PreferredBackBufferWidth = 360;
                        graphics.PreferredBackBufferHeight = 600;
                        break;

                    case 360:
                        graphics.PreferredBackBufferWidth = 240;
                        graphics.PreferredBackBufferHeight = 400;
                        break;

                    case 240:
                        graphics.PreferredBackBufferWidth = 480;
                        graphics.PreferredBackBufferHeight = 800;
                        break;
                }

                graphics.ApplyChanges();

                base.OnClicked();
            }
        }
    }
}
