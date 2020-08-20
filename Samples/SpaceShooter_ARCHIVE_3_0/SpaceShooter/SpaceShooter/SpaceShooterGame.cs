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
    /// This is the main type for your game
    /// </summary>
    /// 
    public class SpaceShooterGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpaceShip playerShip;
        SpaceShip enemyShip;

        ChaseCamera chaseCamera;

        BoltManager boltManager;

        SkyBox skyBox;

        MotionField motionField;

        ParticleManager particles;

        Planet earth;
        Atmosphere air;
        Sun sun;

        BloomComponent bloomComponent;

        public SoundEffect ShipHitSound;
        public SoundEffect BoltExplodeSound;
        public SoundEffect ShipCollideSound;

        public BoltManager Bolts
        {
            get { return boltManager; }
        }

        public SpaceShooterGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;
            graphics.MinimumVertexShaderProfile = ShaderProfile.VS_2_0;

            chaseCamera = new ChaseCamera(this);

            particles = new ParticleManager(this);
            particles.Camera = chaseCamera;
            Components.Add(particles);

            playerShip = new PlayerShip(this, particles);

            enemyShip = new SpaceShip(this, particles);

            boltManager = new BoltManager(this, particles);

            skyBox = new SkyBox(this);
            skyBox.Visible = false;
            Components.Add(skyBox);

            earth = new Planet(this);
            air = new Atmosphere(this);
            sun = new Sun(this);

            motionField = new MotionField(this);

            bloomComponent = new BloomComponent(this);
            Components.Add(bloomComponent);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            playerShip.Initialize();

            enemyShip.Initialize();

            //Start enemy ship in front of us.
            Matrix playerOrientation = Matrix.CreateFromQuaternion(playerShip.Rotation);
            enemyShip.Position = playerOrientation.Forward * 100.0f;

            motionField.Initialize();

            Vector3 earthPos = Vector3.Right * 10000.0f;
            earth.Initialize(6356.75f, new Vector4(30.0f / 256.0f, 98 / 256.0f, 142 / 256.0f, 201 / 256.0f), 6356.75f, earthPos);

            air.Initialize(6356.75f, 35.0f, new Vector4(30.0f / 256.0f, 98 / 256.0f, 142 / 256.0f, 201 / 256.0f), 6356.75f, earthPos);

            sun.Initialize(8000, Vector3.Forward * 1000000.0f);

            // Set the camera
            chaseCamera.SetProjectionParams(MathHelper.ToRadians(45.0f), (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height, 1.0f, 3000000.0f);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            playerShip.LoadContent("ship1");
            enemyShip.LoadContent("ship2");

            earth.LoadContent(this, "EarthDiffuse", "earthbump", "earthSpec", "earthNight");
            air.LoadContent(this, "clouds");
            sun.LoadContent(this, "sun", Color.LightGoldenrodYellow, Color.LightGoldenrodYellow);

            BoltExplodeSound = Content.Load<SoundEffect>(@"audio\BoltFire");
            ShipHitSound = Content.Load<SoundEffect>(@"audio\ShipHit");
            ShipCollideSound = Content.Load<SoundEffect>(@"audio\CarCrashMinor");
        }

        /// <summary>
        /// Detects Collisions between game objects
        /// </summary>
        /// <param name="gameTime"></param>
        void DetectCollisions(GameTime gameTime)
        {
            for (int i = 0; i < boltManager.Bolts.Count; i++)
            {
                if (boltManager.Bolts[i].Owner != playerShip)
                {
                    Vector3 direction = boltManager.Bolts[i].Velocity;
                    direction.Normalize();

                    Ray bulletRay = new Ray(boltManager.Bolts[i].Position, direction);
                    BoundingSphere bulletSphere = boltManager.Bolts[i].BSphere;
                    bulletSphere.Radius = boltManager.Bolts[i].Velocity.Length() * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (enemyShip.BSphere.Intersects(bulletRay).HasValue &&
                        enemyShip.BSphere.Intersects(bulletSphere))
                    {
                        enemyShip.Hit(boltManager.Bolts[i]);

                        Vector3 hitPosition = enemyShip.Position;
                        hitPosition -= direction * enemyShip.BSphere.Radius * 2.0f;
                        boltManager.Bolts[i].Hit(hitPosition);
                        break;
                    }
                }

                if (boltManager.Bolts[i].Owner != enemyShip)
                {
                    Vector3 direction = boltManager.Bolts[i].Velocity;
                    direction.Normalize();

                    Ray bulletRay = new Ray(boltManager.Bolts[i].Position, direction);
                    BoundingSphere bulletSphere = boltManager.Bolts[i].BSphere;
                    bulletSphere.Radius = boltManager.Bolts[i].Velocity.Length() * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (enemyShip.BSphere.Intersects(bulletRay).HasValue &&
                        enemyShip.BSphere.Intersects(bulletSphere))
                    {
                        enemyShip.Hit(boltManager.Bolts[i]);

                        Vector3 hitPosition = enemyShip.Position;
                        hitPosition -= direction * enemyShip.BSphere.Radius * 2.0f;
                        boltManager.Bolts[i].Hit(hitPosition);
                        break;
                    }
                }
            }

            // Check if the ships are colliding!
            if (!enemyShip.IsDestroyed && enemyShip.BSphere.Intersects(playerShip.BSphere))
            {
                // Only need to call one ship hitting the other.
                // We adjust both ships with one call!
                enemyShip.Hit(playerShip);
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            //Run collision detection.
            DetectCollisions(gameTime);

            // Update the ships
            playerShip.Update(gameTime);
            enemyShip.Update(gameTime);

            // Update the Camera from the playerShip!
            // Update the Chase Camera
            chaseCamera.TargetPosition = playerShip.Position;
            chaseCamera.TargetRotation = playerShip.Rotation;
            chaseCamera.Update(gameTime);

            //Update the Bolts
            boltManager.Update(gameTime);

            //Update the background
            earth.Update(gameTime);
            air.Update(gameTime);
            sun.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// Helper function to draw Basic Effect Models
        /// </summary>
        /// <param name="model"></param>
        /// <param name="worldMatrix"></param>
        /// <param name="camera"></param>
        public static void DrawModel(Model model, Matrix worldMatrix, Camera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["DirectionalLight"].SetValue(Vector3.Forward);
                    effect.Parameters["DirectionalLightColor"].SetValue(Color.White.ToVector4());
                    effect.Parameters["specularPower"].SetValue(20.0f);
                    effect.Parameters["specularIntensity"].SetValue(1.0f);
                    effect.Parameters["cameraPosition"].SetValue(Vector3.Zero);
                    effect.Parameters["World"].SetValue(worldMatrix);
                    effect.Parameters["View"].SetValue(camera.View);
                    effect.Parameters["Projection"].SetValue(camera.Projection);

                    // Draw the mesh, using the effects set above.
                    mesh.Draw();
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Clear screen to black
            GraphicsDevice.Clear(Color.Black);

            //Draw the skybox
            skyBox.Draw(gameTime, chaseCamera);

            // Turn on depth buffering for the models, effects and local backgrounds
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            // Draw the local planet(s) and sun(s)
            earth.Draw(gameTime, chaseCamera);
            air.Draw(gameTime, chaseCamera);

            sun.Draw(gameTime, chaseCamera);

            //Draw the motion effect particles
            motionField.Draw(gameTime, chaseCamera);

            // Draw the ships
            playerShip.Draw(gameTime, chaseCamera);
            enemyShip.Draw(gameTime, chaseCamera);

            // Draw the weapons
            boltManager.Draw(gameTime, chaseCamera);

            // Draw the Heads Up Display Items here
            if (enemyShip.IsDestroyed)
            {
                spriteBatch.Begin();
                Vector2 stringLength = enemyShip.HudFont.MeasureString("You Win!");
                spriteBatch.DrawString(enemyShip.HudFont, "You Win!", new Vector2((GraphicsDevice.Viewport.Width - stringLength.X) / 2.0f, (GraphicsDevice.Viewport.Height - stringLength.Y) / 2.0f), Color.Yellow);
                spriteBatch.End();
            }
            else if (playerShip.IsDestroyed)
            {
                spriteBatch.Begin();
                Vector2 stringLength = enemyShip.HudFont.MeasureString("You Lose!");
                spriteBatch.DrawString(enemyShip.HudFont, "You Lose!", new Vector2((GraphicsDevice.Viewport.Width - stringLength.X) / 2.0f, (GraphicsDevice.Viewport.Height - stringLength.Y) / 2.0f), Color.Red);
                spriteBatch.End();
            }
            else
            {
                // draw the target box around the current (only!) target
                // along with range information.
                enemyShip.DrawHeadsUpDisplay(chaseCamera, playerShip);
            }

            // This draws the particle effects plus any other game components set to draw
            base.Draw(gameTime);
        }
    }

    #region Entry Point

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SpaceShooterGame game = new SpaceShooterGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
