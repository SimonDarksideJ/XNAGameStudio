#region File Description
//-----------------------------------------------------------------------------
// VersusStageScreen.cs
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
using RobotGameData.Helper;
using RobotGameData.Text;
using RobotGameData.ParticleSystem;
using RobotGameData.GameEvent;
#endregion

namespace RobotGame
{
    #region Structure

    public class VersusGameInfo
    {
        public string[] playerSpec = null;
        public int killPoint = 0;
    };

    #endregion

    /// <summary>
    /// It loads the level information for versus mode and 
    /// processes the necessary information for the split screen.
    /// It creates two cameras for two players and, in RobotGameData’s RenderContext, 
    /// gives commands so that two screens are drawn on a frame.
    /// It processes each of two Hud information and 
    /// judges the victory condition of versus play.
    /// </summary>
    public class VersusStageScreen : BaseStageScreen
    {
        #region Fields

        const int image1PWidth = 100;
        const int image1PHeight = 60;
        const int imageWinWidth = 464;
        const int imageWinHeight = 164;
        const int imageLoseWidth = 604;
        const int imageLoseHeight = 168;

        protected GameSprite2D spriteHudVersus = null;

        protected GameText[] textKillScore = null;
        protected GameText[] textKillConditionScore = null;

        protected Sprite2DObject spriteObjHudVersus1P = null;
        protected Sprite2DObject spriteObjHudVersus2P = null;
        protected Sprite2DObject spriteObjHudVersusWin = null;
        protected Sprite2DObject spriteObjHudVersusLose = null;

        #endregion

        #region Initialization

        /// <summary>
        /// initializes this screen. 
        /// </summary>
        public override void InitializeScreen()
        {
            base.InitializeScreen();

            FrameworkCore.Viewer.ClearColor = Color.Black;

            NextScreen = new VersusReadyScreen();
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

            RobotGameGame.VersusGameInfo = null;
        }

        /// <summary>
        /// creates two cameras and viewports for split-screen.
        /// </summary>
        public void InitCamera()
        {
            //  Make two mutiple cameras
            ViewCamera followViewCamera = new ViewCamera();
            FollowCamera[] followCamera = new FollowCamera[2]
            {
                new FollowCamera(),
                new FollowCamera(),
            };

            int playerCount = GameLevel.PlayerCountInLevel;

            for (int i = 0; i < playerCount; i++)
            {
                GamePlayer player = GameLevel.GetPlayerInLevel(i);

                followCamera[i].SetPespective(
                    MathHelper.ToRadians(this.GameLevel.Info.FOV),
                    (float)FrameworkCore.ViewWidth,
                    (float)FrameworkCore.ViewHeight / playerCount,
                    1.0f, 1000.0f);

                // Follow camera offset position setting
                followCamera[i].TargetOffset =
                                        player.SpecData.CameraTargetOffset;

                followCamera[i].PositionOffset =
                                        player.SpecData.CameraPositionOffset;

                int splitX = 0;
                int splitY = 0;
                int splitWidth = 0;
                int splitHeight = 0;

                //  1P viewport area
                if (i == 0)
                {
                    splitX = 0;
                    splitY = 0;
                }
                //  2P viewport area
                else if (i == 1)
                {
                    splitX = 0;
                    splitY = FrameworkCore.ViewHeight / playerCount;
                }

                splitWidth = FrameworkCore.ViewWidth;
                splitHeight = FrameworkCore.ViewHeight / playerCount;

                followViewCamera.Add(followCamera[i],
                                new Rectangle(splitX, splitY, splitWidth, splitHeight));
            }

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
        /// loads every model and particle to be used in the versus stage.
        /// Plays the background music of the versus stage.
        /// </summary>
        public override void LoadContent()
        {
            //  Load a level data (i.e. player, enemies, world, items)
            LoadLevel("Data/Stage/VersusStage.level");

            CreatePlayer();            

            //  This funciton should be called after all the resource loading is done
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

        #region Hud 

        /// <summary>
        /// creates Hud images for versus game.
        /// </summary>
        public override void CreateHud()
        {
            ViewCamera viewCamera = FrameworkCore.CurrentCamera;
            int viewCount = viewCamera.Count;

            this.textKillScore = new GameText[viewCount];
            this.textKillConditionScore = new GameText[viewCount];
            Color scoreColor = new Color(136, 217, 224);

            // kill score and condition score
            for( int i=0; i<viewCount; i++)
            {
                this.textKillScore[i] = new GameText(
                        this.fontHud, "0", 0, 0, scoreColor);

                this.refSceneHudRoot.AddChild(this.textKillScore[i]);

                this.textKillConditionScore[i] = new GameText(
                    this.fontHud,
                    string.Format("/ {0}", RobotGameGame.VersusGameInfo.killPoint),
                    0, 0, scoreColor);

                this.refSceneHudRoot.AddChild(this.textKillConditionScore[i]);
            }

            this.spriteHudVersus = new GameSprite2D();
            this.spriteHudVersus.Create(4, "Textures/VS_Text");
            this.refSceneHudRoot.AddChild(this.spriteHudVersus);

            //  1P image
            this.spriteObjHudVersus1P =
                    this.spriteHudVersus.AddSprite(0, "Versus 1P");

            //  2P image
            this.spriteObjHudVersus2P =
                    this.spriteHudVersus.AddSprite(1, "Versus 2P");

            //  You win
            this.spriteObjHudVersusWin =
                    this.spriteHudVersus.AddSprite(2, "Versus Win");

            this.spriteObjHudVersusWin.Visible = false;

            //  You lose
            this.spriteObjHudVersusLose =
                    this.spriteHudVersus.AddSprite(3, "Versus Lose");

            this.spriteObjHudVersusLose.Visible = false;

            base.CreateHud();
        }

        /// <summary>
        /// configures the visibility of the Hud image on screen.
        /// </summary>
        /// <param name="visible"></param>
        public override void SetVisibleHud(bool visible)
        {
            base.SetVisibleHud(visible);

            this.spriteObjHudVersus1P.Visible = visible;
            this.spriteObjHudVersus2P.Visible = visible;

            for (int i = 0; i < GameLevel.PlayerCountInLevel; i++)
            {
                this.textKillScore[i].Visible = visible;
                this.textKillConditionScore[i].Visible = visible;
            }
        }

        /// <summary>
        /// calling when screen size has changed.
        /// changes the sizes of camera and Hud when the screen size is changed.
        /// </summary>
        /// <param name="viewRect">new view area</param>
        public override void OnSize(Rectangle newRect)
        {
            base.OnSize(newRect);

            int playerCount = GameLevel.PlayerCountInLevel;
            Viewport viewport = FrameworkCore.CurrentViewport;

            //  split-screen Camera
            ViewCamera followViewCamera = Viewer.GetViewCamera("Follow");

            for (int i = 0; i < playerCount; i++)
            {
                GamePlayer player = GameLevel.GetPlayerInLevel(i);

                int splitX = 0;
                int splitY = 0;
                int splitWidth = 0;
                int splitHeight = 0;

                //  1P viewport area
                if (i == 0)
                {
                    splitX = 0;
                    splitY = 0;
                }
                //  2P viewport area
                else if (i == 1)
                {
                    splitX = 0;
                    splitY = viewport.Height / playerCount;
                }

                splitWidth = viewport.Width;
                splitHeight = viewport.Height / playerCount;

                //  Resizing camera
                followViewCamera.Resize(i,
                            splitX, splitY, splitWidth, splitHeight);
            }

            //  Resize Hud
            {
                int posX = 0, posY = 0, scaledWidth = 0, scaledHeight = 0, offsetY = 0;
                ViewCamera viewCamera = FrameworkCore.CurrentCamera;
                int viewCount = viewCamera.Count;

                float scaleFactor = 1.0f;

                if (GameLevel.Info.GamePlayType == GamePlayTypeId.Versus)
                    scaleFactor = 0.8f;

                //  Scaling image size and positioning for screen resolution
                Vector2 sizeScale = new Vector2(
                                        (float)FrameworkCore.ViewWidth * scaleFactor /
                                        (float)ViewerWidth.Width1080,
                                        (float)FrameworkCore.ViewHeight * scaleFactor /
                                        (float)ViewerHeight.Height1080);

                Vector2 posScale = new Vector2(
                                        (float)FrameworkCore.ViewWidth * scaleFactor /
                                        (float)ViewerWidth.Width1080,
                                        (float)FrameworkCore.ViewHeight * scaleFactor /
                                        (float)ViewerHeight.Height1080);

                //  if versus mode, apply width offset rate
                if (GameLevel.Info.GamePlayType == GamePlayTypeId.Versus)
                    posScale.X *= 1.3f;

                for (int i = 0; i < viewCount; i++)
                {
                    Viewport view = viewCamera.GetViewport(i);

                    offsetY = (int)view.Y;

                    // kill score and condition score
                    this.textKillScore[i].PosX = (int)(215 * posScale.X);
                    this.textKillScore[i].PosY = 
                                            (int)((float)view.Height * 0.68f) + offsetY;

                    this.textKillConditionScore[i].PosX = (int)(300 * posScale.X);
                    this.textKillConditionScore[i].PosY =
                                            (int)((float)view.Height * 0.76f) + offsetY;

                    this.textKillScore[i].Scale = 1.6f;
                    this.textKillConditionScore[i].Scale = 0.8f;

                    float textScale = 1.4f * (float)FrameworkCore.ViewWidth / 
                                      (float)ViewerWidth.Width720;

                    this.textKillScore[i].Scale *= textScale;
                    this.textKillConditionScore[i].Scale *= textScale;
                }

                //  1P image
                this.spriteObjHudVersus1P.SourceRectangle = new Rectangle(
                                                    82, 560,
                                                    image1PWidth, image1PHeight);

                this.spriteObjHudVersus1P.ScreenRectangle = new Rectangle(
                                    (int)(FrameworkCore.ViewWidth * 0.05f),
                                    (int)(FrameworkCore.ViewHeight / 2 * 0.76f),
                                    (int)((float)image1PWidth * sizeScale.X),
                                    (int)((float)image1PHeight * sizeScale.Y));

                //  2P image
                this.spriteObjHudVersus2P.SourceRectangle = new Rectangle(
                                                    532, 560,
                                                    image1PWidth, image1PHeight);

                offsetY = (FrameworkCore.ViewHeight / 2);
                this.spriteObjHudVersus2P.ScreenRectangle = new Rectangle(
                    (int)(FrameworkCore.ViewWidth * 0.05f),
                    (int)((FrameworkCore.ViewHeight / 2 * 0.76f) + offsetY),
                    (int)((float)image1PWidth * sizeScale.X),
                    (int)((float)image1PHeight * sizeScale.Y));

                this.spriteObjHudVersusWin.SourceRectangle = 
                    new Rectangle(108, 55, imageWinWidth, imageWinHeight);
                this.spriteObjHudVersusLose.SourceRectangle = 
                    new Rectangle(48, 370, imageLoseWidth, imageLoseHeight);

                if (this.isFinishedVersus)
                {
                    for (int i = 0; i < GameLevel.PlayerCountInLevel; i++)
                    {
                        GamePlayer player = GameLevel.GetPlayerInLevel(i);

                        //  You win
                        if (RobotGameGame.VersusGameInfo.killPoint <= player.KillPoint)
                        {          
                            scaledWidth = (int)((float)imageWinWidth * sizeScale.X);
                            scaledHeight = (int)((float)imageWinHeight * sizeScale.Y);

                            posX = (int)((FrameworkCore.ViewWidth / 2) - 
                                        (scaledWidth / 2));

                            posY = (int)((FrameworkCore.ViewHeight / 2) - 
                                        (scaledHeight / 2));

                            if (player.PlayerIndex == PlayerIndex.One)
                                posY -= FrameworkCore.ViewHeight / 4;
                            else if (player.PlayerIndex == PlayerIndex.Two)
                                posY += FrameworkCore.ViewHeight / 4;

                            this.spriteObjHudVersusWin.ScreenRectangle = new Rectangle(
                                                            posX, posY, 
                                                            scaledWidth, scaledHeight);
                        }
                        //  You lose
                        else
                        {      
                            scaledWidth = (int)((float)imageLoseWidth * sizeScale.X);
                            scaledHeight = (int)((float)imageLoseHeight * sizeScale.Y);

                            posX = (int)((FrameworkCore.ViewWidth / 2) - 
                                        (scaledWidth / 2));

                            posY = (int)((FrameworkCore.ViewHeight / 2) - 
                                        (scaledHeight / 2));

                            if (player.PlayerIndex == PlayerIndex.One)
                                posY -= FrameworkCore.ViewHeight / 4;
                            else if (player.PlayerIndex == PlayerIndex.Two)
                                posY += FrameworkCore.ViewHeight / 4;

                            this.spriteObjHudVersusLose.ScreenRectangle = 
                                new Rectangle(posX, posY, scaledWidth, scaledHeight);
                        }
                    }
                }

                //  Resizing Hud
                ResizeHud();
            }
        }

        /// <summary>
        /// configures the visibility of the kill point on screen.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void DisplayKillPoint(int index, int value)
        {
            this.textKillScore[index].Text = value.ToString();
        }

        #endregion

        #region Mission

        /// <summary>
        /// checks the winning condition of the versus play.
        /// Any player who has destroyed the other as many as the kill point wins.
        /// </summary>
        protected override void CheckMission(GameTime gameTime)
        {
            if (isFinishedVersus == false)
            {
                for (int i = 0; i < GameLevel.PlayerCountInLevel; i++)
                {
                    GamePlayer player = GameLevel.GetPlayerInLevel(i);

                    //  Whoever wins the set points first, the versus mode will end.
                    if (RobotGameGame.VersusGameInfo.killPoint <= player.KillPoint)
                    {
                        isFinishedVersus = true;
                    }
                }
            }

            if (isFinishedVersus)
            {
                //  Visible mission result image
                if (missionResultElapsedTime > MissionResultVisibleTime)
                {
                    this.spriteObjHudVersusWin.Visible = true;
                    this.spriteObjHudVersusLose.Visible = true;
                }
                else
                {
                    if (missionResultElapsedTime < MissionResultVisibleTime)
                    {
                        missionResultElapsedTime +=
                                        (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }

                    this.spriteObjHudVersusWin.Visible = false;
                    this.spriteObjHudVersusLose.Visible = false;
                }

                if (GameSound.IsPlaying(soundBGM))
                {
                    float scaleFactor = 1.0f;
                    if (GameLevel.Info.GamePlayType == GamePlayTypeId.Versus)
                        scaleFactor = 0.8f;

                    //  Scale the image size and position for screen resolution
                    Vector2 sizeScale = 
                            new Vector2((float)FrameworkCore.ViewWidth * scaleFactor /
                                        (float)ViewerWidth.Width1080,
                                        (float)FrameworkCore.ViewHeight * scaleFactor /
                                        (float)ViewerHeight.Height1080);

                    for (int i = 0; i < GameLevel.PlayerCountInLevel; i++)
                    {
                        GamePlayer player = GameLevel.GetPlayerInLevel(i);

                        //  Player sound and action stop
                        player.MissionEnd();

                        //  Win!!
                        if (RobotGameGame.VersusGameInfo.killPoint <= player.KillPoint)
                        {
                            int scaledWidth = 
                                (int)((float)imageWinWidth * sizeScale.X);
                            int scaledHeight = 
                                (int)((float)imageWinHeight * sizeScale.Y);

                            int posX = (int)((FrameworkCore.ViewWidth / 2) - 
                                        (scaledWidth / 2));

                            int posY = (int)((FrameworkCore.ViewHeight / 2) - 
                                        (scaledHeight / 2));

                            if( player.PlayerIndex == PlayerIndex.One)
                                posY -= FrameworkCore.ViewHeight / 4;
                            else if( player.PlayerIndex == PlayerIndex.Two)
                                posY += FrameworkCore.ViewHeight / 4;

                            this.spriteObjHudVersusWin.ScreenRectangle = new Rectangle(
                                                            posX, posY,
                                                            scaledWidth, scaledHeight);
                        }
                        //  Lose!!
                        else
                        {
                            int scaledWidth = 
                                (int)((float)imageLoseWidth * sizeScale.X);
                            int scaledHeight = 
                                (int)((float)imageLoseHeight * sizeScale.Y);

                            int posX = (int)((FrameworkCore.ViewWidth / 2) - 
                                        (scaledWidth / 2));

                            int posY = (int)((FrameworkCore.ViewHeight / 2) - 
                                        (scaledHeight / 2));

                            if (player.PlayerIndex == PlayerIndex.One)
                                posY -= FrameworkCore.ViewHeight / 4;
                            else if (player.PlayerIndex == PlayerIndex.Two)
                                posY += FrameworkCore.ViewHeight / 4;

                            this.spriteObjHudVersusLose.ScreenRectangle = new Rectangle(
                                                            posX, posY,
                                                            scaledWidth, scaledHeight);
                        }
                    }

                    //  Stop background music
                    GameSound.Stop(soundBGM);

                    //  Play success music
                    GameSound.Play(SoundTrack.MissionClear);

                    //  Invisible all of the Hud
                    SetVisibleHud(false);

                    //  Go to main menu
                    NextScreen = new VersusReadyScreen();
                    TransitionOffTime = TimeSpan.FromSeconds(8.0f);
                    ExitScreen();
                }
            }
            else
            {
                // Respawns a destroyed player in the game.
                for (int i = 0; i < GameLevel.PlayerCountInLevel; i++)
                {
                    GamePlayer player = GameLevel.GetPlayerInLevel(i);
                                        
                    if (player.IsFinishedDead)
                    {
                        int otherSidePlayerIndex = -1;

                        if( i==0 )
                            otherSidePlayerIndex = 1;
                        else if( i==1 )
                            otherSidePlayerIndex = 0;

                        GamePlayer otherSidePlayer = 
                            GameLevel.GetPlayerInLevel(otherSidePlayerIndex);

                        //  Find the farthest positions for enemy positions.
                        RespawnInLevel respawn = GameLevel.FindRespawnMostFar(
                                            otherSidePlayer.WorldTransform.Translation);

                        //  Set to respawn position
                        player.SpawnPoint = Matrix.CreateRotationY(
                            MathHelper.ToRadians(respawn.SpawnAngle)) * 
                            Matrix.CreateTranslation(respawn.SpawnPoint);

                        //  Reset the player info
                        player.Reset(true);
                    }
                }
            }
        }

        #endregion

        #region Players

        /// <summary>
        /// creates two players characters for versus play.
        /// For single stage levels, the level information(.level) dictates player; 
        /// however, for versus stage level, since the player’s choice among 
        /// the four robots will make difference per each game, it is created here.
        protected void CreatePlayer()
        {
            for (int i = 0; i < RobotGameGame.VersusGameInfo.playerSpec.Length; i++)
            {
                CreatePlayer(RobotGameGame.VersusGameInfo.playerSpec[i],
                            RobotGameGame.CurrentGameLevel.SceneMechRoot);
            }
        }

        /// <summary>
        /// creates a player character.
        /// </summary>
        /// <param name="specFileName">player spec file(.spec)</param>
        /// <param name="sceneParent">3D scene parent node</param>
        protected void CreatePlayer(string specFileName, NodeBase sceneParent)
        {
            GamePlayerSpec spec = new GamePlayerSpec();
            spec = 
                (GamePlayerSpec)GameDataSpecManager.Load(specFileName, spec.GetType());

            GamePlayer player = null;

            switch (GameLevel.PlayerCountInLevel)
            {
                case 0:
                    {                        
                        player = new GamePlayer(ref spec, PlayerIndex.One);

                        RobotGameGame.SinglePlayer = player;
                        FrameworkCore.GameEventManager.TargetScene = player;
                    }
                    break;
                case 1:
                    {
                        player = new GamePlayer(ref spec, PlayerIndex.Two);
                    }
                    break;
                default:
                    throw new InvalidOperationException(
                        "Added player count is overflow");
            }            

            //  Entry enemies in 3D Scene root node
            sceneParent.AddChild(player);

            //  Create rotation axis
            Matrix rot = Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f));
            player.SetRootAxis(rot);

            //  Set material
            RenderMaterial material = new RenderMaterial();
            material.alpha = 1.0f;
            material.diffuseColor = new Color(210, 210, 210);
            material.specularColor = new Color(60, 60, 60);
            material.emissiveColor = new Color(30, 30, 30);
            material.specularPower = 24;

            material.vertexColorEnabled = false;
            material.preferPerPixelLighting = false;

            player.Material = material;
            player.ActiveFog = true;
            player.ActiveLighting = true;

            //  Create collision data
            Vector3 centerPos = Vector3.Transform(
                            new Vector3(0.0f, spec.MechRadius, 0.0f),
                            Matrix.Invert(rot));

            CollideSphere collide = new CollideSphere(centerPos,
                                                spec.MechRadius);

            player.EnableCulling = true;
            player.SetCollide(collide);
            player.ActionIdle();

            //  Add collide
            RobotGameGame.CurrentGameLevel.CollisionVersusTeam[
                            GameLevel.PlayerCountInLevel].AddCollide(collide);

            RobotGameGame.CurrentGameLevel.CollisionLayerAllMech.AddCollide(collide);

            //  Set the respawn position
            if (player.PlayerIndex == PlayerIndex.One)
            {
                int count = GameLevel.Info.RespawnInLevelList.Count;
                int rndIndex = HelperMath.Randomi(0, count);

                RespawnInLevel respawn = GameLevel.Info.RespawnInLevelList[rndIndex];

                player.SpawnPoint = 
                    Matrix.CreateRotationY(MathHelper.ToRadians(respawn.SpawnAngle)) *
                    Matrix.CreateTranslation(respawn.SpawnPoint);
            }
            else if (player.PlayerIndex == PlayerIndex.Two)
            {
                GamePlayer gamePlayerOne = GameLevel.GetPlayerInLevel(0);

                RespawnInLevel respawn = 
                    GameLevel.FindRespawnMostFar(gamePlayerOne.SpawnPoint.Translation);

                player.SpawnPoint =
                    Matrix.CreateRotationY(MathHelper.ToRadians(respawn.SpawnAngle)) *
                    Matrix.CreateTranslation(respawn.SpawnPoint);
            }

            GameLevel.AddPlayer(player);            
        }

        #endregion

    }
}
