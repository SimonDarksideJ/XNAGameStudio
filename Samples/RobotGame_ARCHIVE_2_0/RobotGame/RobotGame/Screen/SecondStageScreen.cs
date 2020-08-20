#region File Description
//-----------------------------------------------------------------------------
// SecondStageScreen.cs
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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using RobotGameData;
using RobotGameData.Screen;
using RobotGameData.Sound;
using RobotGameData.Resource;
using RobotGameData.Input;
using RobotGameData.GameObject;
using RobotGameData.Camera;
using RobotGameData.Collision;
using RobotGameData.Render;
using RobotGameData.Text;
using RobotGameData.ParticleSystem;
using RobotGameData.GameEvent;
#endregion

namespace RobotGame
{
    /// <summary>
    /// It loads the resources for the second stage, 
    /// such as world, player, enemies, items, etc.
    /// All the required resources are defined in the stage level file(.level).
    /// </summary>
    public class SecondStageScreen : SingleStageScreen
    {
        #region Initialization

        /// <summary>
        /// initializes this screen.
        /// </summary>
        public override void InitializeScreen()
        {
            base.InitializeScreen();

            NextScreen = new MainMenuScreen();
            TransitionOffTime = TimeSpan.FromSeconds(8.0f);

            FrameworkCore.RenderContext.ClearColor = new Color(18, 17, 26);

            //  initializes for world everything.
            InitWorld();

            //  initializes for camera for this stage.
            InitCamera();

            //  Load Hud
            CreateHud();

            for (int i = 0; i < GameLevel.PlayerCountInLevel; i++)
            {
                GamePlayer gamePlayer = GameLevel.GetPlayerInLevel(i);

                //  Update selected weapon image in the Hud
                SetCurrentWeaponHud(i, gamePlayer.CurrentWeapon.WeaponType);
            }

            //  Play a background music
            soundBGM = GameSound.Play(SoundTrack.SecondStage);
        }

        /// <summary>
        /// finalizes this screen. 
        /// </summary>
        public override void FinalizeScreen()
        {
            base.FinalizeScreen();

            Viewer.RemoveCamera("Follow");
        }
                        
        /// <summary>
        /// creates a third person view camera which follows the player.
        /// </summary>
        public void InitCamera()
        {
            //  Follow Camera
            FollowCamera followCamera = new FollowCamera();

            followCamera.SetPespective(MathHelper.ToRadians(this.GameLevel.Info.FOV),
                                (float)GraphicsDevice.Viewport.Width,
                                (float)GraphicsDevice.Viewport.Height,
                                1.0f, 10000.0f);

            // Follow camera offset position setting
            followCamera.TargetOffset = 
                            GameLevel.SinglePlayer.SpecData.CameraTargetOffset;

            followCamera.PositionOffset = 
                            GameLevel.SinglePlayer.SpecData.CameraPositionOffset;

            ViewCamera followViewCamera = new ViewCamera(followCamera, 
                new Rectangle(0, 0, FrameworkCore.ViewWidth, FrameworkCore.ViewHeight));

            Viewer.AddCamera("Follow", followViewCamera);

            Viewer.SetCurrentCamera("Follow");
        }

        /// <summary>
        /// initializes the world.
        /// </summary>
        public static void InitWorld()
        {
            //  Environment smoke
            {
                GameParticle.PlayParticle(ParticleType.EnvironmentSmoke,
                        Matrix.CreateTranslation(new Vector3(-17.0f, -0.2f, -387.0f)),
                        Matrix.Identity);

                GameParticle.PlayParticle(ParticleType.EnvironmentSmoke,
                        Matrix.CreateTranslation(new Vector3(-17.0f, -0.2f, -315.0f)),
                        Matrix.Identity);

                GameParticle.PlayParticle(ParticleType.EnvironmentSmoke,
                       Matrix.CreateTranslation(new Vector3(12f, -0.2f, -282.0f)),
                        Matrix.Identity);

                GameParticle.PlayParticle(ParticleType.EnvironmentSmoke,
                        Matrix.CreateTranslation(new Vector3(2.5f, -0.2f, -107.0f)),
                        Matrix.Identity);

                GameParticle.PlayParticle(ParticleType.EnvironmentSmoke,
                        Matrix.CreateTranslation(new Vector3(7.5f, -0.2f, -32.0f)),
                        Matrix.Identity);
            }

            // Envirionment fire
            {
                GameParticle.PlayParticle(ParticleType.EnvironmentFire,
                        Matrix.CreateTranslation(new Vector3(17.0f, -0.5f, -422.0f)),
                        Matrix.Identity);

                GameParticle.PlayParticle(ParticleType.EnvironmentFire,
                        Matrix.CreateTranslation(new Vector3(17.0f, -0.5f, -351.0f)),
                        Matrix.Identity);

                GameParticle.PlayParticle(ParticleType.EnvironmentFire,
                        Matrix.CreateTranslation(new Vector3(-17.0f, -0.5f, -231.0f)),
                        Matrix.Identity);

                GameParticle.PlayParticle(ParticleType.EnvironmentFire,
                        Matrix.CreateTranslation(new Vector3(2.7f, -0.5f, -176.0f)),
                        Matrix.Identity);

                GameParticle.PlayParticle(ParticleType.EnvironmentFire,
                        Matrix.CreateTranslation(new Vector3(-17.0f, -0.5f, -73.0f)),
                        Matrix.Identity);

                GameParticle.PlayParticle(ParticleType.EnvironmentFire,
                        Matrix.CreateTranslation(new Vector3(-7.0f, -0.5f, -33.0f)),
                        Matrix.Identity);

                GameParticle.PlayParticle(ParticleType.EnvironmentFire,
                        Matrix.CreateTranslation(new Vector3(-17f, -0.5f, -2.8f)),
                        Matrix.Identity);
            }
        }

        #endregion

        #region Load data

        /// <summary>
        /// load graphics contents. 
        /// loads every model and particle that are to be used in the second stage.
        /// Plays the background music of the second stage.
        /// </summary>
        public override void LoadContent()
        {
            //  Load a level data (i.e. player, enemies, world, items)
            LoadLevel("Data/Stage/SecondStage.level");

            //  This funciton should be called after all the resource loading is done.
            base.LoadContent();
        }

        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            UnloadBaseContent();
        }

        #endregion

        #region Hud

        /// <summary>
        /// calling when screen size has changed.
        /// changes the sizes of camera and Hud when the screen size has been changed.
        /// </summary>
        /// <param name="viewRect">new view area</param>
        public override void OnSize(Rectangle newRect)
        {
            base.OnSize(newRect);

            Viewport viewport = FrameworkCore.CurrentViewport;

            //  Follow Camera
            ViewCamera followViewCamera = Viewer.GetViewCamera("Follow");

            followViewCamera.Resize(0,
                        viewport.X, viewport.Y,
                        viewport.Width, viewport.Height);

            //  Resizing Hud
            ResizeHud();
        }

        #endregion

        #region Update & Draw

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        #endregion

        #region HandleInput

        public override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);
        }
        
        #endregion
    }
}
