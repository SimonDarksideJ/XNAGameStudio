#region File Description
//-----------------------------------------------------------------------------
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace SpaceShooter
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ParticleManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        NovaFire novaFire;
        NovaSmoke novaSmoke;
        NovaSparks novaSparks;

        RockTrailSmoke rockTrailSmoke;
        RockTrailSparks rockTrailSparks;

        PlanetExplosionFire planetExplosionFire;
        PlanetExplosionSmoke planetExplosionSmoke;

        RockExplosionSparks rockExplosionSparks;
        RockExplosionSmoke rockExplosionSmoke;
        RockExplosionFire rockExplosionFire;

        List<ParticleSystem> particleSystems;

        Camera camera;

        public Camera Camera
        {
            get
            {
                return camera;
            }

            set
            {
                camera = value;
                for (int i = 0; i < particleSystems.Count; ++i)
                    particleSystems[i].Camera = camera;
            }
        }

        public ParticleManager(Game game)
            : base(game)
        {
            particleSystems = new List<ParticleSystem>();

            rockTrailSmoke = new RockTrailSmoke(game);
            rockTrailSparks = new RockTrailSparks(game);
            planetExplosionFire = new PlanetExplosionFire(game);
            planetExplosionSmoke = new PlanetExplosionSmoke(game);

            rockExplosionSparks = new RockExplosionSparks(game);
            rockExplosionSmoke = new RockExplosionSmoke(game);
            rockExplosionFire = new RockExplosionFire(game);

            novaFire = new NovaFire(game);
            novaSmoke = new NovaSmoke(game);
            novaSparks = new NovaSparks(game);

            particleSystems.Add(rockTrailSmoke);
            particleSystems.Add(rockTrailSparks);

            particleSystems.Add(planetExplosionFire);
            particleSystems.Add(planetExplosionSmoke);

            particleSystems.Add(rockExplosionSparks);
            particleSystems.Add(rockExplosionSmoke);
            particleSystems.Add(rockExplosionFire);

            particleSystems.Add(novaFire);
            particleSystems.Add(novaSmoke);
            particleSystems.Add(novaSparks);
        }

        public override void Initialize()
        {
            for (int i = 0; i < particleSystems.Count; ++i)
                particleSystems[i].Initialize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < particleSystems.Count; ++i)
                particleSystems[i].Update(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            for (int i = 0; i < particleSystems.Count; ++i)
                particleSystems[i].Draw(gameTime);

            base.Draw(gameTime);
        }

        public void CreateNova(Vector3 position)
        {
            for (int i = 0; i < 512; i++)
                novaFire.AddParticle(position, Vector3.Zero);

            for (int i = 0; i < 256; i++)
                novaSparks.AddParticle(position, Vector3.Zero);
        }

        public ParticleEmitter CreateRockEmitter(Vector3 position)
        {
            ParticleEmitter emitter = new ParticleEmitter(128.0f, position, rockTrailSmoke, rockTrailSparks);
            return emitter;
        }

        public void CreateRockExplosion(Vector3 position, Vector3 velocity)
        {
            velocity.Normalize();

            for (int i = 0; i < 256; i++)
                rockExplosionFire.AddParticle(position, velocity);

            for (int i = 0; i < 128; i++)
                rockExplosionSparks.AddParticle(position, Vector3.Zero);

            for (int i = 0; i < 64; i++)
                rockExplosionSmoke.AddParticle(position, Vector3.Zero);
        }

        public void CreatePlanetExplosion(Vector3 position, Vector3 velocity)
        {

            velocity.Normalize();

            for (int i = 0; i < 256; i++)
                planetExplosionFire.AddParticle(position, Vector3.Zero);

            velocity *= 3.0f;

            for (int i = 0; i < 128; i++)
                planetExplosionSmoke.AddParticle(position, velocity);
        }
    }
}