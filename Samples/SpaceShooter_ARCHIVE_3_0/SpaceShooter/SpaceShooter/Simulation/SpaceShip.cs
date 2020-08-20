#region File Description
//-----------------------------------------------------------------------------
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SpaceShooter
{

    public enum ShipControlType
    {
        PlayerControl = 0,
        AIControl = 1,
        NetworkControl = 2
    }

    public class SpaceShipInput
    {
        public float PitchAngle;
        public float YawAngle;
        public float RollAngle;
        public float SpeedOffset;
        public bool Fired;

        public void Reset()
        {
            PitchAngle = YawAngle = RollAngle = SpeedOffset = 0.0f;
            Fired = false;
        }
    }

    public class SpaceShipAIData
    {
        // How often do we change what we are doing?
        public float DecisionInterval = 2.0f;
        public float CurrentDecisionTime = 0.0f;

        public float RollInput = 0.0f;
        public float PitchInput = 0.0f;
        public float YawInput = 0.0f;

        public float ThrustInput = 0.0f;

        public Random RandomNumber = new Random();
    }

    public class SpaceShip : DrawableGameComponent
    {
        // Ship Simulation data
        Vector3 position;
        Quaternion rotateQuat;

        bool destroyed;

        //Ship engine data
        Vector3 actualVelocity;

        float engineSpeed = 0.0f;         // Current Engine Speed
        float maxEngineSpeed = 50.0f;
        float minEngineSpeed = -20.0f;

        // Weapon fire data
        float refireDelay = 0.2f;
        float lastFireTime = 0.0f;
        float boltDamage = 10.0f;
        float boltSpeed = 500.0f;
        float boltRange = 3.0f;  //In seconds of duration

        // Ship manuever data
        float pitchSpeed = MathHelper.ToRadians(90.0f);
        float yawSpeed = MathHelper.ToRadians(45.0f);
        float rollSpeed = MathHelper.ToRadians(90.0f);

        // Ship structure data
        float maxDamage = 35.0f;
        float currentDamage;
        BoundingSphere bSphere;

        ShipControlType spaceShipControlType = ShipControlType.AIControl;
        SpaceShipAIData spaceShipAIData;

        SpaceShipInput shipInput;

        Model shipModel;

        ParticleManager particles;
        public ParticleEmitter emitter;

        float sizeInPixels;

        Texture2D smallDot;
        SpriteFont hudFont;

        SpriteBatch localSprites;

        protected bool drawHudBox = true;

        VertexBuffer hudVB;
        VertexDeclaration hudDecl;

        Effect hudEffect;
        EffectParameter screenSize;

        VertexPositionColor[] hudVertices;

        const int numHudLines = 4;

        public SpaceShipInput ShipInput
        {
            get { return shipInput; }
            set { shipInput = value; }
        }

        public Vector3 ActualVelocity
        {
            get { return actualVelocity; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Quaternion Rotation
        {
            get { return rotateQuat; }
            set { rotateQuat = value; }
        }

        public float PitchSpeed
        {
            get { return pitchSpeed; }
        }

        public float YawSpeed
        {
            get { return yawSpeed; }
        }

        public float RollSpeed
        {
            get { return rollSpeed; }
        }

        public float LastFireTime
        {
            get { return lastFireTime; }
            set { lastFireTime = value; }
        }

        public float RefireDelay
        {
            get { return refireDelay; }
        }

        public ShipControlType SpaceShipControlType
        {
            get { return spaceShipControlType; }
            set { spaceShipControlType = value; }
        }

        public Model ShipModel
        {
            get { return shipModel; }
        }

        public BoundingSphere BSphere
        {
            get
            {
                bSphere.Center = position;
                return bSphere;
            }
        }

        public bool IsDestroyed
        {
            get { return destroyed; }
        }

        public SpriteFont HudFont
        {
            get { return hudFont; }
        }

        public SpaceShip(Game game, ParticleManager particles)
            : base(game)
        {
            Reset(Vector3.Zero);

            // Data drive these...
            pitchSpeed = MathHelper.ToRadians(90.0f);
            yawSpeed = MathHelper.ToRadians(45.0f);
            rollSpeed = MathHelper.ToRadians(90.0f);

            shipInput = new SpaceShipInput();
            spaceShipAIData = new SpaceShipAIData();

            currentDamage = 0.0f;
            destroyed = false;
            this.particles = particles;

            emitter = particles.CreateRockEmitter(Vector3.Zero);

            hudVertices = new VertexPositionColor[numHudLines + 1];
            for (int i = 0; i < numHudLines + 1; i++)
                hudVertices[i] = new VertexPositionColor(Vector3.Zero, Color.Red);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public virtual void LoadContent(string name)
        {
            shipModel = Game.Content.Load<Model>(@"ships\" + name);
            bSphere = shipModel.Meshes[0].BoundingSphere;

            localSprites = new SpriteBatch(Game.GraphicsDevice);
            smallDot = Game.Content.Load<Texture2D>(@"textures\dot");

            hudFont = Game.Content.Load<SpriteFont>(@"fonts\HUDFont");

            hudEffect = Game.Content.Load<Effect>(@"shaders\simpleScreen");
            screenSize = hudEffect.Parameters["screenSize"];

            hudDecl = new VertexDeclaration(Game.GraphicsDevice, VertexPositionColor.VertexElements);

            hudVB = new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionColor), numHudLines + 1, BufferUsage.None);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (!destroyed)
            {
                // This is AI for non-player ships
                // This could also be network control for networked player ships
                switch (spaceShipControlType)
                {
                    case ShipControlType.AIControl:
                        AIControl(gameTime);
                        break;

                    case ShipControlType.PlayerControl:
                        PlayerControl(gameTime);
                        break;

                    case ShipControlType.NetworkControl:
                        NetworkControl(gameTime);
                        break;
                }

                if (actualVelocity.Length() > 5.0f)
                    emitter.Update(gameTime, position);
            }

            base.Update(gameTime);
        }

        public void Reset(Vector3 pos)
        {
            position = pos;
            rotateQuat = Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(210));
            engineSpeed = 0.0f;
        }

        public virtual void ReadSpaceShipInputs(PlayerIndex playerIndex)
        {
        }

        protected virtual void NetworkControl(GameTime gameTime)
        {
        }

        protected virtual void AIControl(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Matrix m = Matrix.CreateFromQuaternion(rotateQuat);

            // Have AI figure out where to set the inputs here...
            spaceShipAIData.CurrentDecisionTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (spaceShipAIData.CurrentDecisionTime > spaceShipAIData.DecisionInterval)
            {
                // Enough time has past. Do we want to change our behavior?
                // For now, check a random number and change if it changes.
                // We change behavior about 50% of the time every two seconds
                if (spaceShipAIData.RandomNumber.NextDouble() > 0.5f)
                {
                    spaceShipAIData.RollInput = 1.0f - 2.0f * (float)spaceShipAIData.RandomNumber.NextDouble();
                    spaceShipAIData.PitchInput = 1.0f - 2.0f * (float)spaceShipAIData.RandomNumber.NextDouble();
                    spaceShipAIData.ThrustInput = 0.5f + 0.5f * (float)spaceShipAIData.RandomNumber.NextDouble();
                }

                spaceShipAIData.CurrentDecisionTime = 0.0f;
            }
            else
            {
                spaceShipAIData.RollInput -= (spaceShipAIData.RollInput / 2.0f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                spaceShipAIData.PitchInput -= (spaceShipAIData.PitchInput / 2.0f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            shipInput.RollAngle = spaceShipAIData.RollInput;
            shipInput.PitchAngle = spaceShipAIData.PitchInput;
            shipInput.SpeedOffset = spaceShipAIData.ThrustInput;

            ExecuteControl(gameTime, dt, m);
        }

        protected virtual void PlayerControl(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Matrix m = Matrix.CreateFromQuaternion(rotateQuat);

            ExecuteControl(gameTime, dt, m);
        }

        protected virtual void ExecuteControl(GameTime gameTime, float dt, Matrix m)
        {
            Quaternion pitch = Quaternion.CreateFromAxisAngle(m.Right, ShipInput.PitchAngle * dt * -pitchSpeed);
            Quaternion yaw = Quaternion.CreateFromAxisAngle(m.Up, ShipInput.YawAngle * dt * -yawSpeed);
            Quaternion roll = Quaternion.CreateFromAxisAngle(m.Backward, ShipInput.RollAngle * dt * -rollSpeed);

            rotateQuat = yaw * pitch * roll * rotateQuat;
            rotateQuat.Normalize();

            if (Math.Sign(ShipInput.SpeedOffset) == -1)
                engineSpeed = -ShipInput.SpeedOffset * minEngineSpeed;
            else
                engineSpeed = ShipInput.SpeedOffset * maxEngineSpeed;

            actualVelocity = m.Forward * engineSpeed;

            position.X += actualVelocity.X * dt;
            position.Y += actualVelocity.Y * dt;
            position.Z += actualVelocity.Z * dt;

            LastFireTime -= dt;
            if ((LastFireTime <= 0.0f) && (ShipInput.Fired))
            {
                Vector3 projectileVelocity = m.Forward * boltSpeed + actualVelocity;
                ((SpaceShooter.SpaceShooterGame)Game).Bolts.FireBolt("mgun_proj", projectileVelocity, boltRange, boltDamage, this);
                LastFireTime = refireDelay;
            }
        }

        public virtual void Hit(Bolt bolt)
        {
            currentDamage += bolt.Damage;
            ((SpaceShooter.SpaceShooterGame)Game).ShipHitSound.Play();

            if (currentDamage > maxDamage)
            {
                destroyed = true;
                particles.CreateNova(position);

                ((SpaceShooter.SpaceShooterGame)Game).ShipHitSound.Play(1.0f);
                ((SpaceShooter.SpaceShooterGame)Game).BoltExplodeSound.Play(1.0f);
            }
        }

        public virtual void Hit(SpaceShip ship)
        {
            currentDamage += 0.3f; // Ships hitting each other don't do much damage.  Must happen alot ;}
            ship.currentDamage += 0.3f;

            particles.CreatePlanetExplosion(position, actualVelocity);

            // When we hit another ship, generate random velocity
            // give one ship this velocity, give other ship the opposite of this velocity
            // for about half a second of time...
            actualVelocity = new Vector3((1.0f - 2.0f * (float)spaceShipAIData.RandomNumber.NextDouble()) * maxEngineSpeed,
                                         (1.0f - 2.0f * (float)spaceShipAIData.RandomNumber.NextDouble()) * maxEngineSpeed,
                                         (1.0f - 2.0f * (float)spaceShipAIData.RandomNumber.NextDouble()) * maxEngineSpeed);

            ship.actualVelocity = -actualVelocity;

            float dt = 0.5f;
            position.X += actualVelocity.X * dt;
            position.Y += actualVelocity.Y * dt;
            position.Z += actualVelocity.Z * dt;

            ship.position.X += ship.actualVelocity.X * dt;
            ship.position.Y += ship.actualVelocity.Y * dt;
            ship.position.Z += ship.actualVelocity.Z * dt;

            ((SpaceShooter.SpaceShooterGame)Game).ShipCollideSound.Play();

            if (currentDamage > maxDamage)
            {
                destroyed = true;
                particles.CreateNova(position);

                ((SpaceShooter.SpaceShooterGame)Game).ShipHitSound.Play(1.0f);
                ((SpaceShooter.SpaceShooterGame)Game).ShipCollideSound.Play(1.0f);
            }
        }

        public virtual bool IsVisible(Camera camera)
        {
            foreach (ModelMesh mesh in shipModel.Meshes)
            {
                BoundingSphere localSphere = mesh.BoundingSphere;
                localSphere.Center += position;

                ContainmentType contains = camera.BF.Contains(localSphere);
                if (contains == ContainmentType.Contains || contains == ContainmentType.Intersects)
                    return true;
            }

            return false;
        }

        public virtual void DrawHeadsUpDisplay(Camera camera, SpaceShip targetor)
        {
            // Draw Head's Up display box around ship.
            // If we are set to draw the HUD box!
            // We probably ONLY want to draw this box around the current target!
            // For now, since we have only one, logic is pretty easy!
            if (IsVisible(camera) && drawHudBox)
            {
                // Draw ship as a dot with a text tag in the distance
                Matrix viewProj = camera.View * camera.Projection;
                Vector4 screenPos = Vector4.Transform(position, viewProj);

                Vector2 screenViewport = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

                float halfScreenY = screenViewport.Y / 2.0f;
                float halfScreenX = screenViewport.X / 2.0f;

                float screenY = ((screenPos.Y / screenPos.W) * halfScreenY) + halfScreenY;
                float screenX = ((screenPos.X / screenPos.W) * halfScreenX) + halfScreenX;

                hudVertices[0].Position.X = screenX - sizeInPixels;
                hudVertices[0].Position.Y = screenY - sizeInPixels;

                hudVertices[1].Position.X = screenX + sizeInPixels;
                hudVertices[1].Position.Y = screenY - sizeInPixels;

                hudVertices[2].Position.X = screenX + sizeInPixels;
                hudVertices[2].Position.Y = screenY + sizeInPixels;

                hudVertices[3].Position.X = screenX - sizeInPixels;
                hudVertices[3].Position.Y = screenY + sizeInPixels;

                hudVertices[4].Position.X = screenX - sizeInPixels;
                hudVertices[4].Position.Y = screenY - sizeInPixels;

                hudVB.SetData<VertexPositionColor>(hudVertices);

                hudEffect.Begin();
                hudEffect.Techniques[0].Passes[0].Begin();
                screenSize.SetValue(screenViewport);
                hudEffect.CommitChanges();

                GraphicsDevice.RenderState.DepthBufferEnable = false;
                GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

                GraphicsDevice.VertexDeclaration = hudDecl;
                GraphicsDevice.Vertices[0].SetSource(hudVB, 0, VertexPositionColor.SizeInBytes);

                GraphicsDevice.DrawPrimitives(PrimitiveType.LineStrip, 0, numHudLines);

                GraphicsDevice.Vertices[0].SetSource(null, 0, 0);

                GraphicsDevice.RenderState.DepthBufferEnable = true;
                GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

                hudEffect.Techniques[0].Passes[0].End();
                hudEffect.End();

                screenY = halfScreenY - ((screenPos.Y / screenPos.W) * halfScreenY);

                float range = (position - targetor.Position).Length();

                localSprites.Begin();
                if(range < 9999.0f)
                    localSprites.DrawString(hudFont, range.ToString("0000", CultureInfo.CurrentCulture), new Vector2(screenX + sizeInPixels + 5, screenY - sizeInPixels), Color.Red);
                else
                    localSprites.DrawString(hudFont, "XXXX", new Vector2(screenX + sizeInPixels + 5, screenY - sizeInPixels), Color.Red);

                localSprites.End();
            }
        }

        public virtual void Draw(GameTime gameTime, Camera camera)
        {
            if (!destroyed && IsVisible(camera))
            {
                float distance = (position - camera.CameraPosition).Length();
                sizeInPixels = 10.0f;
                float radius = shipModel.Meshes[0].BoundingSphere.Radius;
                if (distance > radius)
                {
                    float angularSize = (float)Math.Tan(radius / distance);
                    sizeInPixels = angularSize * GraphicsDevice.Viewport.Height / camera.FieldOfView;
                }

                if (sizeInPixels > 0.0f)
                {
                    // Draw ship as a dot with a text tag in the distance
                    Matrix viewProj = camera.View * camera.Projection;
                    Vector4 screenPos = Vector4.Transform(position, viewProj);

                    Vector2 screenViewport = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

                    if (sizeInPixels < 5.0f)
                    {
                        float HudScale = .5f * (sizeInPixels / 6.0f);

                        float halfScreenY = screenViewport.Y / 2.0f;
                        float halfScreenX = screenViewport.X / 2.0f;

                        float screenY = halfScreenY - ((screenPos.Y / screenPos.W) * halfScreenY);
                        float screenX = ((screenPos.X / screenPos.W) * halfScreenX) + halfScreenX;

                        localSprites.Begin();
                        localSprites.Draw(smallDot, new Vector2(screenX, screenY), null,
                                      Color.Green, 0.0f, new Vector2(16, 16), 0.25f + HudScale, SpriteEffects.None, 0.0f);
                        localSprites.End();
                    }
                    else
                    {
                        Matrix worldMatrix = Matrix.CreateFromQuaternion(rotateQuat);
                        worldMatrix.Translation = position;

                        SpaceShooter.SpaceShooterGame.DrawModel(shipModel, worldMatrix, camera);
                    }
                }
            }

            base.Draw(gameTime);
        }
    }
}