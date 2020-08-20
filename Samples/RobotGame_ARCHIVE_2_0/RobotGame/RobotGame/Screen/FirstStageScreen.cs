#region File Description
//-----------------------------------------------------------------------------
// FirstStageScreen.cs
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
    /// It loads the resources for the first stage, 
    /// such as world, player, enemies, items, etc.
    /// All the required resources are defined in the stage level file(.level).
    /// </summary>
    public class FirstStageScreen : SingleStageScreen
    {
        #region Initialization

        /// <summary>
        /// initializes this screen. 
        /// configures the second stage, which will be visited 
        /// when the first stage is cleared.
        /// </summary>
        public override void InitializeScreen()
        {
            base.InitializeScreen();

            NextScreen = new LoadingScreen();
            NextScreen.NextScreen = new SecondStageScreen();
            TransitionOffTime = TimeSpan.FromSeconds(8.0f);

            FrameworkCore.RenderContext.ClearColor = Color.Black;

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
            soundBGM = GameSound.Play(SoundTrack.FirstStage);
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
        /// initializes the 3rd person view camera which follows player.
        /// </summary>
        public void InitCamera()
        {
            //  Follow Camera
            FollowCamera followCamera = new FollowCamera();

            followCamera.SetPespective(MathHelper.ToRadians(this.GameLevel.Info.FOV),
                                (float)FrameworkCore.ViewWidth,
                                (float)FrameworkCore.ViewHeight,
                                1.0f, 1000.0f);

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
            //  Play background particles
            GameParticle.PlayParticle(ParticleType.EnvironmentSmoke,
                        Matrix.CreateTranslation(new Vector3(-68.0f, 4.0f, 42.0f)),
                        Matrix.CreateRotationY(MathHelper.ToRadians(-180.0f)));

            GameParticle.PlayParticle(ParticleType.EnvironmentSmoke,
                        Matrix.CreateTranslation(new Vector3(40.0f, 4.0f, 69.0f)),
                        Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)));

            GameParticle.PlayParticle(ParticleType.EnvironmentSmoke,
                        Matrix.CreateTranslation(new Vector3(82.0f, 7.5f, 37.0f)),
                        Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)));
        }

        #endregion

        #region Load data

        /// <summary>
        /// loads graphics contents. 
        /// loads every model and particle that are to be used in the first stage.
        /// It plays the background music of the first stage.
        /// </summary>
        public override void LoadContent()
        {
            //  Load a level data (i.e. player, enemies, world, items)
            LoadLevel("Data/Stage/FirstStage.level");                      

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
        /// changes the sizes of camera and Hud when the screen size is changed.
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

        #endregion
    }
}
