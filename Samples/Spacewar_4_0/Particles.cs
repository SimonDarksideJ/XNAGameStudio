#region File Description
//-----------------------------------------------------------------------------
// Particles.cs
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

namespace Spacewar
{
    /// <summary>
    /// A class that represents a group of particles
    /// </summary>
    public class Particles : SceneItem
    {
        private static string particleTexture = @"textures\circle";

        private static Random random = new Random();

        private static SpriteBatch batch;

        public Particles(Game game)
            : base(game)
        {
            if (game != null)
            {
                IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));
                batch = new SpriteBatch(graphicsService.GraphicsDevice);
            }
        }

        /// <summary>
        /// Draws the particles in a batch
        /// </summary>
        public override void Render()
        {
            if (Count != 0)
            {
                IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));
                GraphicsDevice device = graphicsService.GraphicsDevice;

                batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                    DepthStencilState.None, RasterizerState.CullCounterClockwise);

                int particleCount = 0;

                foreach (SceneItem particle in this)
                {
                    if (particle is Particle)
                    {
                        batch.Draw(SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + particleTexture),
                            new Vector2(particle.Position.X, particle.Position.Y),
                            null, new Color(((Particle)particle).Color), 0,
                            new Vector2(16, 16), .2f,
                            SpriteEffects.None, particle.Position.Z);
                        particleCount++;
                    }
                }

                batch.End();

                //We DON'T need to call base class here. We are handling batching all the children ourselves
                //base.Render();
            }
        }

        /// <summary>
        /// Adds particles to represent vapour trail
        /// </summary>
        /// <param name="world">Start position of the particle</param>
        /// <param name="direction">Direction the ship is heading. Particles will be lined up along this vec</param>
        public void AddShipTrail(Matrix world, Vector2 direction)
        {
            //Move source point into screen space
            Vector4 source = Vector4.Transform(new Vector4(0, 0, 130000, 1), world * SpacewarGame.Camera.View * SpacewarGame.Camera.Projection);
            //and into pixels
            Vector2 source2D = new Vector2((int)((source.X / source.W + 1f) / 2f * 1280), (int)((-source.Y / source.W + 1f) / 2f * 720));

            direction = Vector2.Normalize(direction);

            for (int i = 0; i < 70; i++)
            {
                float trailDistance = random.Next(50);
                float trailOffset = random.Next(21) - 10;

                Add(new Particle(this.GameInstance,
                        new Vector2(source2D.X + trailDistance * direction.X + trailOffset * direction.Y, source2D.Y + trailDistance * direction.Y + trailOffset * direction.X),
                        new Vector2(trailDistance * trailOffset * direction.Y / 5, trailDistance * trailOffset * direction.X / 5),
                        new Vector4(1f, 1f, .5f, .5f),
                        new Vector4(.2f, .2f, 0f, .2f),
                        new TimeSpan(0, 0, 2)));
            }
        }

        /// <summary>
        /// Adds particles to represent vapour trail
        /// </summary>
        /// <param name="world">Start position of the particle</param>
        /// <param name="direction">Direction the rocket is heading. Particles will be lined up along this vec</param>
        public void AddRocketTrail(Matrix world, Vector2 direction)
        {
            //Move source point into screen space
            Vector4 source = Vector4.Transform(new Vector4(0, 0, 291, 1), world * SpacewarGame.Camera.View * SpacewarGame.Camera.Projection);
            //and into pixels
            Vector2 source2D = new Vector2((int)((source.X / source.W + 1f) / 2f * 1280), (int)((-source.Y / source.W + 1f) / 2f * 720));

            direction = Vector2.Normalize(direction);

            for (int i = 0; i < 20; i++)
            {
                float trailDistance = random.Next(50);

                Add(new Particle(this.GameInstance,
                        new Vector2(source2D.X, source2D.Y) + trailDistance * -direction,
                        -direction,
                        new Vector4(1f, 1f, .5f, .5f),
                        new Vector4(.2f, .2f, 0f, .2f),
                        new TimeSpan(0, 0, 1)));
            }
        }

        /// <summary>
        /// Adds particles that look like an explosion
        /// </summary>
        /// <param name="position"></param>
        public void AddExplosion(Vector3 position)
        {
            //Move source point into screen space
            Vector4 source = Vector4.Transform(position, SpacewarGame.Camera.View * SpacewarGame.Camera.Projection);
            //and into pixels
            Vector2 source2D = new Vector2((int)((source.X / source.W + 1f) / 2f * 1280), (int)((-source.Y / source.W + 1f) / 2f * 720));

            for (int i = 0; i < 300; i++)
            {
                Vector2 velocity = (float)random.Next(100) * Vector2.Normalize(new Vector2((float)(random.NextDouble() - .5), (float)(random.NextDouble() - .5)));
                Add(new Particle(this.GameInstance,
                        new Vector2(source2D.X, source2D.Y),
                        velocity,
                        (i > 70) ? new Vector4(1.0f, 0f, 0f, 1) : new Vector4(.941f, .845f, 0f, 1),
                        new Vector4(.2f, .2f, .2f, 0f),
                        new TimeSpan(0, 0, 0, 0, random.Next(1000) + 500)));
            }
        }

        public override void OnCreateDevice()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));
            batch = new SpriteBatch(graphicsService.GraphicsDevice);
        }
    }
}
