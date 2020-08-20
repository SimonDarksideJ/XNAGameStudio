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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SpaceShooter
{
    public class BoltManager : DrawableGameComponent
    {
        List<Bolt> bolts;

        ParticleManager particles;

        public List<Bolt> Bolts
        {
            get { return bolts; }
        }

        public BoltManager(Game game, ParticleManager particles)
            : base(game)
        {
            bolts = new List<Bolt>();
            this.particles = particles;
        }

        public void FireBolt(string name, Vector3 velocity, float duration, float damage, SpaceShip ship)
        {
            Bolt proj = new Bolt();
            proj.Initialize(velocity, damage, duration, ship);
            proj.LoadContent(Game, name);

            bolts.Add(proj);
            ((SpaceShooter.SpaceShooterGame)Game).BoltExplodeSound.Play();
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < bolts.Count; i++)
            {
                if (!bolts[i].Update(gameTime))
                {
                    particles.CreateRockExplosion(bolts[i].Position, bolts[i].Velocity);

                    bolts.RemoveAt(i);
                    i++;
                }
            }
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            foreach (Bolt bullet in bolts)
            {
                bullet.Draw(Game.GraphicsDevice, camera);
            }
        }
    }

    public class Bolt
    {
        Model boltModel;

        Vector3 position;
        Vector3 velocity;
        Quaternion rotation;

        float timeRemaining;

        float damage;

        SpaceShip owner;

        BoundingSphere bSphere;

        public SpaceShip Owner
        {
            get { return owner; }
        }

        public float Damage
        {
            get { return damage; }
        }

        public Vector3 Position
        {
            get { return position; }
        }

        public Vector3 Velocity
        {
            get { return velocity; }
        }

        public BoundingSphere BSphere
        {
            get
            {
                bSphere.Center = position;
                return bSphere;
            }
        }

        public Bolt()
        {
        }

        public void Initialize(Vector3 velocity, float damage, float duration, SpaceShip ship)
        {
            this.velocity = velocity;
            timeRemaining = duration;
            position = ship.Position;
            rotation = ship.Rotation;
            this.damage = damage;
            owner = ship;
        }

        public void LoadContent(Game game, string name)
        {
            boltModel = game.Content.Load<Model>(@"ships\" + name);
            bSphere = new BoundingSphere(Vector3.Zero, 0.5f);
        }

        public bool Update(GameTime gameTime)
        {
            position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            timeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeRemaining <= 0.0f)
                return false;

            return true;
        }

        public void Hit(Vector3 pos)
        {
            timeRemaining = 0.0f;
            position = pos;
        }

        public void Draw(GraphicsDevice device, Camera camera)
        {
            Matrix worldMatrix = Matrix.CreateFromQuaternion(rotation);
            worldMatrix.Translation = position;

            SpaceShooter.SpaceShooterGame.DrawModel(boltModel, worldMatrix, camera);
        }
    }
}