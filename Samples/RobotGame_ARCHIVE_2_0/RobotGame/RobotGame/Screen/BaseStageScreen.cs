#region File Description
//-----------------------------------------------------------------------------
// BaseStageScreen.cs
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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using RobotGameData;
using RobotGameData.Screen;
using RobotGameData.Resource;
using RobotGameData.Input;
using RobotGameData.GameObject;
using RobotGameData.Camera;
using RobotGameData.Collision;
using RobotGameData.Render;
using RobotGameData.Text;
using RobotGameData.ParticleSystem;
#endregion

namespace RobotGame
{
    /// <summary>
    /// It contains World model, skybox, collision model, collision layers, 
    /// and GameLevel classes that are used by every stage in the game.
    /// Also, it loads/processes the resources for the Hud image, 
    /// which also gets used by different stages, and displays the status 
    /// of the player on Hud.
    /// OnPostDraw3DScreen() function processes the post-screen effect shader. 
    /// </summary>
    public abstract class BaseStageScreen : GameScreen
    {
        #region Fields

        protected GameWorld world = null;
        protected GameSkybox skybox = null;
        protected GameModel moveCollisionModel = null;
        protected GameModel hitCollisionModel = null;
        protected GameLevel gameLevel = null;

        protected bool isMissionClear = false;
        protected bool isMissionFailed = false;
        protected bool isFinishedVersus = false;
        protected bool postScreenEffects = true;

        const int imageAimingWidth = 300;
        const int imageAimingHeight = 298;
        const int imageWeaponWidth = 200;
        const int imageWeaponHeight = 115;

        protected Viewer refViewer = null;
        protected CollisionContext refCollisionContext = null;
        protected RenderContext refRenderContext = null;
        protected ParticleManager refParticleManager = null;
        protected GraphicsDevice refGraphicsDevice = null;

        //  Scene nodes
        protected GameSceneNode refScene3DRoot = null;
        protected GameSceneNode refSceneHudRoot = null;
        protected GameSceneNode refSceneMissionRoot = null;

#if DEBUG
        // debug-only free camera mode
        float freeCameraSpeedAmount = 20.0f;
        float freeCameraTurnAmount = 65.0f;
        protected TextItem camPosition = null;
#endif

        //  PostScreen effect
        protected RenderTarget2D renderTarget1 = null;
        protected RenderTarget2D renderTarget2 = null;
        protected ResolveTexture2D resolveTexture = null;
        protected Effect boosterEffect = null;
        protected Effect postScreenEffect = null;
        protected Effect gaussianBlurEffect = null;

        protected EffectParameter paramCenterX = null;
        protected EffectParameter paramCenterY = null;
        protected EffectParameter paramTexWidth = null;
        protected EffectParameter paramTexHeight = null;
        protected EffectParameter paramWaveWidth = null;
        protected EffectParameter paramWaveMag = null;

        protected EffectParameter paramBloomThreshold = null;
        protected EffectParameter paramBloomIntensity = null;
        protected EffectParameter paramBloomSaturation = null;
        protected EffectParameter paramBaseIntensity = null;
        protected EffectParameter paramBaseSaturation = null;

        //  Hud
        protected const float MissionResultVisibleTime = 1.5f;
        protected float missionResultElapsedTime = 0.0f;
        protected int[] playerOldLife = null;
        protected float[] playerReduceArmorElapsedTime = null;
        protected float[] pickupMessageElapsedTime = null;

        protected GameSprite2D spriteHudAiming = null;
        protected GameSprite2D spriteHudState = null;

        protected SpriteFont fontHud = null;
        protected GameText[] textHudCurrentAmmo = null;
        protected GameText[] textHudRemainAmmo = null;
        protected GameText[] textPickup = null;
        protected GameText[,] textControlHelper = null;

        protected Sprite2DObject[] spriteObjHudAimingSite = null;
        protected Sprite2DObject[] spriteObjHudAlertSite = null;
        protected Sprite2DObject[] spriteObjHudArmorFrame = null;
        protected Sprite2DObject[] spriteObjHudArmorState = null;
        protected Sprite2DObject[] spriteObjHudWeaponWindow = null;
        protected Sprite2DObject[] spriteObjHudBoosterCoolTime = null;
        protected Sprite2DObject[] spriteObjHudPickupCoolTime = null;
        protected Sprite2DObject[] spriteObjHudWeaponMachineGun = null;
        protected Sprite2DObject[] spriteObjHudWeaponShotgun = null;
        protected Sprite2DObject[] spriteObjHudWeaponHandgun = null;

        //  Mission
        protected GameSprite2D spriteMission = null;
        protected Sprite2DObject spriteObjMissionClear = null;
        protected Sprite2DObject spriteObjMissionFailed = null;

        //  Sound
        protected Cue soundBGM = null;

        protected TracerBulletManager tracerBulletManager = null;

        protected readonly Color hudPickupColor = new Color(136, 217, 224);

        #endregion

        #region Properties

        public CollisionContext CollisionContext
        {
            get { return this.refCollisionContext; }
        }

        public RenderContext RenderContext
        {
            get { return this.refRenderContext; }
        }

        public Viewer Viewer
        {
            get { return this.refViewer; }
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return this.refGraphicsDevice; }
        }

        public ParticleManager ParticleManager
        {
            get { return this.refParticleManager; }
        }

        public bool IsMissionClear
        {
            get { return isMissionClear; }
            set { isMissionClear = value; }
        }

        public bool IsMissionFailed
        {
            get { return isMissionFailed; }
            set { isMissionFailed = value; }
        }

        public bool IsFinishedVersus
        {
            get { return isFinishedVersus; }
            set { isFinishedVersus = value; }
        }

        public GameLevel GameLevel
        {
            get { return this.gameLevel; }
        }

        public TracerBulletManager TracerBulletManager
        {
            get { return this.tracerBulletManager; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        protected BaseStageScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(5.0f);
            TransitionOffTime = TimeSpan.FromSeconds(2.0f);

            this.refScene3DRoot = FrameworkCore.Scene3DRoot;
            this.refViewer = FrameworkCore.Viewer;
            this.refRenderContext = FrameworkCore.RenderContext;
            this.refGraphicsDevice = FrameworkCore.Game.GraphicsDevice;
            this.refCollisionContext = FrameworkCore.CollisionContext;
            this.refParticleManager = FrameworkCore.ParticleManager;

            this.gameLevel = new GameLevel();

            this.playerOldLife = new int[2];
            this.playerReduceArmorElapsedTime = new float[2];
            this.pickupMessageElapsedTime = new float[2];
            this.playerReduceArmorElapsedTime = new float[2];

            this.missionResultElapsedTime = 0.0f;
        }

        /// <summary>
        /// initializes this screen. 
        /// </summary>
        public override void InitializeScreen()
        {
            this.refScene3DRoot.RemoveAllChild(true);

            this.refScene3DRoot.AddChild(this.GameLevel.SceneWorldRoot);
            this.refScene3DRoot.AddChild(this.GameLevel.SceneMechRoot);
            this.refScene3DRoot.AddChild(this.GameLevel.SceneParticleRoot);
            this.refScene3DRoot.AddChild(this.GameLevel.SceneCollisionRoot);

            RobotGameGame.CurrentStage = this;

            //  Free Camera
            FreeCamera freeCamera = new FreeCamera();

            freeCamera.SetView(new Vector3(0.0f, 0.0f, 10.0f),
                                Vector3.Forward, Vector3.Up);

            freeCamera.SetPespective(MathHelper.PiOver4,
                                (float)Viewer.ViewWidth,
                                (float)Viewer.ViewHeight,
                                1.0f, 10000.0f);

            ViewCamera freeViewCamera = new ViewCamera(freeCamera, null);
            Viewer.AddCamera("Free", freeViewCamera);

            //  PostScreen effect
            {
                paramCenterX = boosterEffect.Parameters["xCenter"];
                paramCenterY = boosterEffect.Parameters["yCenter"];
                paramTexWidth = boosterEffect.Parameters["texWidth"];
                paramTexHeight = boosterEffect.Parameters["texHeight"];
                paramWaveWidth = boosterEffect.Parameters["width"];
                paramWaveMag = boosterEffect.Parameters["mag"];

                paramBloomThreshold = postScreenEffect.Parameters["BloomThreshold"];
                paramBloomIntensity = postScreenEffect.Parameters["BloomIntensity"];
                paramBloomSaturation = postScreenEffect.Parameters["BloomSaturation"];
                paramBaseIntensity = postScreenEffect.Parameters["BaseIntensity"];
                paramBaseSaturation = postScreenEffect.Parameters["BaseSaturation"];

                //  Entry the post screen processing function
                FrameworkCore.RenderContext.RenderingPostDraw3D += OnPostDraw3DScreen;
            }

            //  Tracer bullets
            {
                tracerBulletManager = new TracerBulletManager();
                tracerBulletManager.AddBulletInstance(0, 32,
                    "Particles/Spark_Horizontal01",
                    RobotGameGame.CurrentGameLevel.SceneParticleRoot);
            }

            this.refScene3DRoot.Initialize();

            //  Create 2D Scene layer for Hud
            this.refRenderContext.CreateScene2DLayer(2);
            this.refSceneHudRoot = FrameworkCore.Scene2DLayers[0];
            this.refSceneMissionRoot = FrameworkCore.Scene2DLayers[1];

#if DEBUG
            Vector2 pos = new Vector2(0, 20);
            pos = FrameworkCore.ClampSafeArea(pos);

            camPosition = FrameworkCore.TextManager.AddText(FrameworkCore.DebugFont,
                    "CAM :", (int)pos.X, (int)pos.Y, Color.White);

            camPosition.Visible = true;
#endif

            FrameworkCore.GameEventManager.Enable = true;
        }

        /// <summary>
        /// finalizes this screen. 
        /// remove all scene nodes.
        /// stop all sounds.
        /// </summary>
        public override void FinalizeScreen()
        {
            RobotGameGame.CurrentStage = null;

            this.refViewer.RemoveAllCamera();
            this.refCollisionContext.ClearAllLayer();
            this.refParticleManager.ClearAllParticles();

            //  Release all models
            this.refRenderContext.ClearScene3DRoot(false);
            this.refRenderContext.ClearScene2DLayer(false);

            this.gameLevel.ClearAllEnemies();
            this.gameLevel.ClearAllPlayers();

            GameSound.StopAll();

            Viewer.RemoveCamera("Free");

            FrameworkCore.ParticleManager.Clear();

            FrameworkCore.RenderContext.RenderingPostDraw3D -= OnPostDraw3DScreen;

            FrameworkCore.SoundManager.SetVolume(
                SoundCategory.Default.ToString(), 1.0f);
            FrameworkCore.SoundManager.SetVolume(
                SoundCategory.Effect.ToString(), 1.0f);
            FrameworkCore.SoundManager.SetVolume(
                SoundCategory.Music.ToString(), 1.0f);

            FrameworkCore.GameEventManager.ClearAllEvent();

#if DEBUG
            FrameworkCore.TextManager.RemoveText(camPosition);
#endif

            GC.Collect();
        }

        /// <summary>
        /// calling when screen size has changed.
        /// re-create all render targets.
        /// </summary>
        /// <param name="viewRect">new view area</param>
        public override void OnSize(Rectangle newRect)
        {
            Viewport viewport = FrameworkCore.CurrentViewport;

            //  Free Camera
            ViewCamera freeViewCamera = Viewer.GetViewCamera("Free");
            freeViewCamera.Resize(0, viewport.X, viewport.Y,
                                     viewport.Width, viewport.Height);

            RestoreRenderTarget();
        }

        /// <summary>
        /// create every render target.
        /// </summary>
        public void RestoreRenderTarget()
        {
            PresentationParameters pp =
                FrameworkCore.Game.GraphicsDevice.PresentationParameters;

            SurfaceFormat format = pp.BackBufferFormat;

            resolveTexture = new ResolveTexture2D(GraphicsDevice,
                                        pp.BackBufferWidth,
                                        pp.BackBufferHeight,
                                        1,
                                        format);
            renderTarget1 = new RenderTarget2D(GraphicsDevice,
                                            pp.BackBufferWidth / 4,
                                            pp.BackBufferHeight / 4,
                                            1, format);
            renderTarget2 = new RenderTarget2D(GraphicsDevice,
                                            pp.BackBufferWidth / 4,
                                            pp.BackBufferHeight / 4,
                                            1, format);
        }

        #endregion

        #region Load data

        /// <summary>
        /// remove all scene nodes.
        /// </summary>
        public void UnloadBaseContent()
        {
            this.refScene3DRoot.RemoveAllChild(true);

            tracerBulletManager.UnloadContent();
            tracerBulletManager = null;
        }

        /// <summary>
        /// loads graphics contents. 
        /// creates the Hud and removes the loading image.
        /// </summary>
        /// <param name="loadAllContent"></param>
        public override void LoadContent()
        {
            this.fontHud = FrameworkCore.FontManager.CreateFont(
                                                "Hud Font", "Font/RobotGame_font");

            //  Load a booster effect
            GameResourceEffect loadedEffect =
                    FrameworkCore.ResourceManager.LoadEffect("Effects/BoosterEffect");

            boosterEffect = loadedEffect.Effect;

            //  Load a glow effect
            loadedEffect =
                    FrameworkCore.ResourceManager.LoadEffect("Effects/PostScreen");

            postScreenEffect = loadedEffect.Effect;

            // Load a gaussian blur effect
            loadedEffect =
                    FrameworkCore.ResourceManager.LoadEffect("Effects/GaussianBlur");

            gaussianBlurEffect = loadedEffect.Effect;

            FrameworkCore.GameEventManager.Enable = false;

            RestoreRenderTarget();

            base.LoadContent();
        }

        #endregion

        #region Update & Draw

        /// <summary>
        /// checks the mission objective during game play and updates the player input 
        /// and  Hud information.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            bool updateScene = (IsActive || IsMissionClear || IsMissionFailed ||
                                IsFinishedVersus);

            bool playerInput = true;

            if (updateScene)
            {
                //  check current mission game state
                CheckMission(gameTime);

                //  If game is active, resume all sounds
                if (GameSound.IsPauseAll())
                    GameSound.ResumeAll();

                //  Updates tracer bullets
                if (tracerBulletManager != null)
                    tracerBulletManager.Update(gameTime);
            }
            else
            {
                //  If game is deactive, pause all sounds
                if (!GameSound.IsPauseAll())
                    GameSound.PauseAll();
            }

            //  Active ?
            if (IsActive)
            {
                FrameworkCore.GameEventManager.Enable = true;

                //  The camera follows to the player
                if (FrameworkCore.CurrentCamera.FirstCamera is FollowCamera)
                {
                    ViewCamera viewCamera = FrameworkCore.CurrentCamera;

                    for (int i = 0; i < viewCamera.Count; i++)
                    {
                        FollowCamera followCamera =
                                        viewCamera.GetCamera(i) as FollowCamera;

                        GamePlayer player = GameLevel.GetPlayerInLevel(i);

                        //  follows to player transform
                        followCamera.SetFollow(player.WorldTransform);
                    }
                }

                //  If completed this mission, player stop
                if (IsMissionClear || IsMissionFailed || IsFinishedVersus)
                    playerInput = false;

                //  The sky follows to current camera position
                if (RobotGameGame.CurrentGameLevel.GameSky != null)
                {
                    if (RobotGameGame.CurrentGameLevel.GameSky.FollowOwner)
                    {
                        RobotGameGame.CurrentGameLevel.GameSky.SetBasisPosition(
                            FrameworkCore.CurrentCamera.FirstCamera.Position);
                    }
                }
            }
            else
            {
                playerInput = false;

                FrameworkCore.GameEventManager.Enable = false;
            }

            //  input handle the player
            for (int i = 0; i < gameLevel.PlayerCountInLevel; i++)
            {
                GamePlayer player = GameLevel.GetPlayerInLevel(i);
                player.EnableHandleInput = playerInput;
            }

            //  Update 3D scene ?
            if (this.refScene3DRoot != null)
                this.refScene3DRoot.Enabled = updateScene;

            //  Update Hud scene ?
            if (this.refSceneHudRoot != null)
                this.refSceneHudRoot.Enabled = updateScene;

            //  Update Hud Information
            if (updateScene && GameLevel.SinglePlayer != null)
            {
                UpdateHud(gameTime);
            }

            if (ScreenState == ScreenState.Finished)
            {
                GameScreenManager.AddScreen(NextScreen, true);
            }

            // Allows the default game to exit on Xbox 360 and Windows
            if (IsActive && FrameworkCore.ScreenManager.SingleInput.PauseGame)
            {
                MessageBoxScreen messageBox = 
                    new MessageBoxScreen("Are you sure you want to exit?");

                //  Register message box handle method
                messageBox.Accepted += ReturnToTitleAccepted;

                GameScreenManager.AddScreen(messageBox, true);
            }

#if DEBUG
            camPosition.Text = string.Format("CAM : {0}", 
                FrameworkCore.CurrentCamera.FirstCamera.Position.ToString());
#endif
        }

        public override void Draw(GameTime gameTime)
        {
            if (TransitionPosition > 0)
            {
                FrameworkCore.ScreenManager.FadeBackBufferToBlack(
                    255 - TransitionAlpha);
            }
            else if (TransitionPosition <= 0)
            {
                FrameworkCore.ScreenManager.FadeBackBufferToBlack(0);
            }
        }

        /// <summary>
        /// processes the post-screen effect shader (bloom and booster effects). 
        /// </summary>
        internal void OnPostDraw3DScreen(object sender, EventArgs e)
        {
            if (!postScreenEffects)
            {
                return;
            }

            SpriteBatch sprite = FrameworkCore.RenderContext.SpriteBatch;

            Viewport defaultViewport = FrameworkCore.DefaultViewport;

            // Copy back buffer to resolveTexture
            GraphicsDevice.ResolveBackBuffer(resolveTexture);

            ////////////// PostScreen (Bloom effect)
            // pass 0 : using a shader that extracts only 
            //          the brightest parts of the image.
            DrawPostScreenEffect(resolveTexture, renderTarget1, postScreenEffect, 0);

            // Pass 2: using a shader to apply a horizontal gaussian blur filter.
            SetBlurEffectParameters(1.0f / (float)renderTarget1.Width, 0);
            DrawPostScreenEffect(renderTarget1, renderTarget2, gaussianBlurEffect, 0);

            // Pass 3: using a shader to apply a vertical gaussian blur filter.
            SetBlurEffectParameters(0, 1.0f / (float)renderTarget1.Height);
            DrawPostScreenEffect(renderTarget2, renderTarget1, gaussianBlurEffect, 0);

            // Pass 4: using a shader that combines them to produce
            //         the final glow result.
            SetBloomEffectParameters();
            DrawFinalScreenEffect(resolveTexture, renderTarget1, postScreenEffect, 1);

            bool anyBoosting = false;
            for (int index = 0; index < FrameworkCore.CurrentCamera.Count; index++)
            {
                GamePlayer player = GameLevel.GetPlayerInLevel(index);

                //  it checks whether to display the booster effect.
                if (player.IsActiveBooster && !player.IsDelayBooster)
                {
                    anyBoosting = true;
                    // Copy back buffer to resolveTexture 
                    GraphicsDevice.ResolveBackBuffer(resolveTexture);
                    break;
                }
            }

            //  No effect
            if (anyBoosting == false ||
                (FrameworkCore.CurrentCamera.FirstCamera is FollowCamera) == false)
            {
                return;
            }

            //  Temporarily disable the depth stencil buffer.
            GraphicsDevice.RenderState.DepthBufferEnable = false;

            ////////////// BoosterEffect (Radial Blur effect)
            //  the booster effect, due to the split screen, 
            //  has to have a separate shader applied.
            for (int index = 0; index < GameLevel.PlayerCountInLevel; index++)
            {
                GamePlayer player = GameLevel.GetPlayerInLevel(index);
                Viewport currentViewport =
                    FrameworkCore.CurrentCamera.GetViewport(index);

                if (player.IsActiveBooster && !player.IsDelayBooster)
                {
                    //  effect screen position (0 to 1)
                    float xCenter = 0.5f *
                        ((float)currentViewport.Width / (float)defaultViewport.Width) +
                        ((float)currentViewport.X / (float)defaultViewport.Width);

                    float yCenter = 0.5f *
                        ((float)currentViewport.Height / (float)defaultViewport.Height) +
                        ((float)currentViewport.Y / (float)defaultViewport.Height);

                    //  We will be draw two pass using the effect shader
                    for (int i = 0; i < boosterEffect.CurrentTechnique.Passes.Count; i++)
                    {
                        EffectPass pass = boosterEffect.CurrentTechnique.Passes[i];

                        sprite.Begin(SpriteBlendMode.AlphaBlend,
                                 SpriteSortMode.Immediate, SaveStateMode.None);

                        paramCenterX.SetValue(xCenter);
                        paramCenterY.SetValue(yCenter);

                        //  pass 1. blur effect
                        if (i == 0)
                        {
                            paramTexWidth.SetValue(currentViewport.Width);
                            paramTexHeight.SetValue(currentViewport.Height);
                        }
                        //  pass 2. wave effect
                        else if (i == 1)
                        {
                            //  The length of the wave.  Bigger the value, 
                            //  the faster it spreads. (default is 1.0)
                            paramWaveWidth.SetValue(player.BoosterWaveEffectTime * 2.5f);

                            //  The deflection distortion value.  Bigger the value, 
                            //  more deflected it gets. (default is 1.0)
                            paramWaveMag.SetValue(2.0f - player.BoosterWaveEffectTime);
                        }

                        // Begin Effect
                        boosterEffect.Begin();
                        pass.Begin();

                        sprite.Draw(resolveTexture,
                            new Rectangle(currentViewport.X, currentViewport.Y,
                                          currentViewport.Width, currentViewport.Height),
                            new Rectangle(currentViewport.X, currentViewport.Y,
                                          currentViewport.Width, currentViewport.Height),
                            Color.White);

                        pass.End();
                        boosterEffect.End();

                        sprite.End();
                    }
                }
                else
                {
                    sprite.Begin(SpriteBlendMode.AlphaBlend,
                             SpriteSortMode.Immediate, SaveStateMode.None);
                    sprite.Draw(resolveTexture,
                        new Rectangle(currentViewport.X, currentViewport.Y,
                                      currentViewport.Width, currentViewport.Height),
                        new Rectangle(currentViewport.X, currentViewport.Y,
                                      currentViewport.Width, currentViewport.Height),
                        Color.White);
                    sprite.End();
                }

                // Restore the original depth stencil buffer.
                GraphicsDevice.RenderState.DepthBufferEnable = false;
            }
        }

        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawPostScreenEffect(RenderTarget2D rtTexture, RenderTarget2D renderTarget,
                                  Effect effect, int pass)
        {
            GraphicsDevice.SetRenderTarget(0, renderTarget);

            DrawPostScreenEffect(rtTexture.GetTexture(), effect, pass,
                new Point(renderTarget.Width, renderTarget.Height));
        }
        
        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawPostScreenEffect(Texture2D texture, RenderTarget2D renderTarget,
                                  Effect effect, int pass)
        {
            GraphicsDevice.SetRenderTarget(0, renderTarget);

            DrawPostScreenEffect(texture, effect, pass,
                new Point(renderTarget.Width, renderTarget.Height));
        }


        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        static void DrawPostScreenEffect(Texture2D texture, Effect effect, 
            int pass, Point size)
        {
            SpriteBatch sprite = FrameworkCore.RenderContext.SpriteBatch;

            sprite.Begin(SpriteBlendMode.None,
                              SpriteSortMode.Immediate,
                              SaveStateMode.None);

            // Begin the custom effect.
            effect.Begin();
            effect.CurrentTechnique.Passes[pass].Begin();

            // Draw the texture.
            sprite.Draw(texture,
                        new Rectangle(0, 0, size.X, size.Y),
                        Color.White);

            sprite.End();

            // End the custom effect.
            effect.CurrentTechnique.Passes[pass].End();
            effect.End();
        }

        /// <summary>
        /// Helper for drawing a texture into the base rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFinalScreenEffect(Texture2D baseTexture, RenderTarget2D renderTarget,
                                  Effect effect, int pass)
        {
            Viewport defaultViewport = FrameworkCore.DefaultViewport;
            SpriteBatch sprite = FrameworkCore.RenderContext.SpriteBatch;

            GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.Textures[1] = baseTexture;

            sprite.Begin(SpriteBlendMode.None,
                              SpriteSortMode.Immediate,
                              SaveStateMode.None);

            // Begin the custom effect.
            effect.Begin();
            effect.CurrentTechnique.Passes[pass].Begin();

            // Draw the texture.
            sprite.Draw(renderTarget.GetTexture(),
                        new Rectangle(0, 0, defaultViewport.Width,
                            defaultViewport.Height),
                        new Rectangle(0, 0, renderTarget.Width, renderTarget.Height),
                        Color.White);

            sprite.End();

            // End the custom effect.
            effect.CurrentTechnique.Passes[pass].End();
            effect.End();

            GraphicsDevice.Textures[1] = null;
        }

        void SetBloomEffectParameters()
        {
            paramBloomThreshold.SetValue(
                        RobotGameGame.CurrentGameLevel.Info.ParameterBloomThreshold);

            paramBloomIntensity.SetValue(
                        RobotGameGame.CurrentGameLevel.Info.ParameterBloomIntensity);

            paramBloomSaturation.SetValue(
                        RobotGameGame.CurrentGameLevel.Info.ParameterBloomSaturation);

            paramBaseIntensity.SetValue(
                        RobotGameGame.CurrentGameLevel.Info.ParameterBaseIntensity);

            paramBaseSaturation.SetValue(
                        RobotGameGame.CurrentGameLevel.Info.ParameterBaseSaturation);
        }

        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }

        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        static float ComputeGaussian(float n)
        {
            float theta = RobotGameGame.CurrentGameLevel.Info.ParameterBlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        #endregion

        #region HandleInput

        /// <summary>
        /// processes inputs of the current player and the free camera.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            InputComponent input =
                        FrameworkCore.InputManager.GetInputComponent(PlayerIndex.One);

            //  Cannot be input all control
            if (IsActive == false) return;

            //  Toggle a camera mode (free or follow)
#if DEBUG
            if ((input.IsStrokeKey(Keys.Enter) && 
                !input.IsPressKey(Keys.LeftAlt) &&
                !input.IsPressKey(Keys.RightAlt)) || 
                input.IsStrokeControlPad(ControlPad.LeftStick))
            {
                //  Free camera type
                if (FrameworkCore.CurrentCamera.FirstCamera is FreeCamera)              
                {
                    this.refViewer.SetCurrentCamera("Follow");

                    SetVisibleHud(true);

                    //  Update selected weapon image in the Hud
                    for( int i=0; i<GameLevel.PlayerCountInLevel; i++)
                    {
                        GamePlayer player = GameLevel.GetPlayerInLevel(i);

                        SetCurrentWeaponHud(i, player.CurrentWeapon.WeaponType);
                    }
                }
                //  Follow camera type
                else if (FrameworkCore.CurrentCamera.FirstCamera is FollowCamera)       
                {
                    this.refViewer.SetCurrentCamera("Free");

                    SetVisibleHud(false);
                }
            }

            //  Control the FreeCamera
            if (FrameworkCore.CurrentCamera.FirstCamera is FreeCamera)
            {
                FreeCamera camera = 
                    FrameworkCore.CurrentCamera.FirstCamera as FreeCamera;
                
                if (input.IsPressKey(Keys.Up) || 
                    input.GetThumbStickAmount(Trigger.Right).Y > 0.0f)
                {
                    camera.Rotate(new Vector3(0.0f, freeCameraTurnAmount, 0.0f));
                }
                else if (input.IsPressKey(Keys.Down) || 
                         input.GetThumbStickAmount(Trigger.Right).Y < 0.0f)
                {
                    camera.Rotate(new Vector3(0.0f, -freeCameraTurnAmount, 0.0f));
                }

                if (input.IsPressKey(Keys.Right) || 
                    input.GetThumbStickAmount(Trigger.Right).X > 0.0f)
                {
                    camera.Rotate(new Vector3(-freeCameraTurnAmount, 0.0f, 0.0f));
                }
                else if (input.IsPressKey(Keys.Left) || 
                         input.GetThumbStickAmount(Trigger.Right).X < 0.0f)
                {
                    camera.Rotate(new Vector3(freeCameraTurnAmount, 0.0f, 0.0f));
                }

                if (input.IsPressKey(Keys.W) || 
                    input.GetThumbStickAmount(Trigger.Left).Y > 0.0f)
                {
                    camera.MoveForward(freeCameraSpeedAmount);
                }

                if (input.IsPressKey(Keys.S) || 
                    input.GetThumbStickAmount(Trigger.Left).Y < 0.0f)
                {
                    camera.MoveForward(-freeCameraSpeedAmount);
                }

                if (input.IsPressKey(Keys.A) || 
                    input.GetThumbStickAmount(Trigger.Left).X < 0.0f)
                {
                    camera.MoveSide(-freeCameraSpeedAmount);
                }

                if (input.IsPressKey(Keys.D) || 
                    input.GetThumbStickAmount(Trigger.Left).X > 0.0f)
                {
                    camera.MoveSide(freeCameraSpeedAmount);
                }
            }
#endif

            //  Player input handle from user key
            for (int i = 0; i < GameLevel.PlayerCountInLevel; i++)
            {
                GamePlayer player = GameLevel.GetPlayerInLevel(i);

                player.HandleInput(gameTime);
            }
        }

        /// <summary>
        /// returns to the main menu.
        /// </summary>
        void ReturnToTitleAccepted(object sender, EventArgs e)
        {
            //  Accepeted to message box menu
            NextScreen = new MainMenuScreen();

            TransitionOffTime = TimeSpan.FromSeconds(0.5f);
            ExitScreen();
        }

        #endregion

        #region Hud

        /// <summary>
        /// creates the Hud image which is communally used in a stage and 
        /// configures the Hud information.
        /// </summary>
        public virtual void CreateHud()
        {
            int viewCount = FrameworkCore.CurrentCamera.Count;

            //  Hud Aiming
            {
                int spriteCount = 0;

                this.spriteHudAiming = new GameSprite2D();
                this.spriteHudAiming.Create(2 * viewCount, "Textures/Hud_Aiming");

                this.refSceneHudRoot.AddChild(this.spriteHudAiming);

                spriteObjHudAimingSite = new Sprite2DObject[viewCount];
                spriteObjHudAlertSite = new Sprite2DObject[viewCount];

                for (int i = 0; i < viewCount; i++)
                {
                    //  Aiming Site
                    this.spriteObjHudAimingSite[i] =
                    this.spriteHudAiming.AddSprite(spriteCount++, "Firing site");

                    //  Alert Site
                    this.spriteObjHudAlertSite[i] =
                        this.spriteHudAiming.AddSprite(spriteCount++, "Alert site");

                    this.spriteObjHudAlertSite[i].Visible = false;
                }
            }

            // Hud State (Armor and ammo)
            {
                int spriteCount = 0;

                this.spriteHudState = new GameSprite2D();
                this.spriteHudState.Create(8 * viewCount, "Textures/Hud1");

                this.refSceneHudRoot.AddChild(this.spriteHudState);

                Color textColor = new Color(136, 217, 224);

                this.spriteObjHudArmorFrame = new Sprite2DObject[viewCount];
                this.spriteObjHudArmorState = new Sprite2DObject[viewCount];
                this.spriteObjHudWeaponWindow = new Sprite2DObject[viewCount];
                this.spriteObjHudBoosterCoolTime = new Sprite2DObject[viewCount];
                this.spriteObjHudPickupCoolTime = new Sprite2DObject[viewCount];
                this.spriteObjHudWeaponMachineGun = new Sprite2DObject[viewCount];
                this.spriteObjHudWeaponShotgun = new Sprite2DObject[viewCount];
                this.spriteObjHudWeaponHandgun = new Sprite2DObject[viewCount];
                this.textHudCurrentAmmo = new GameText[viewCount];
                this.textHudRemainAmmo = new GameText[viewCount];
                this.textPickup = new GameText[viewCount];
                this.textControlHelper = new GameText[viewCount, 2];

                for (int i = 0; i < viewCount; i++)
                {
                    //  Armor frame   
                    this.spriteObjHudArmorFrame[i] =
                            this.spriteHudState.AddSprite(spriteCount++, "Armor Frame");

                    //  Armor state bar
                    this.spriteObjHudArmorState[i] =
                            this.spriteHudState.AddSprite(spriteCount++, "Armor State");

                    //  Weapon window
                    this.spriteObjHudWeaponWindow[i] =
                            this.spriteHudState.AddSprite(spriteCount++, "Weapon Ammo");

                    //  Booster Cool Time
                    this.spriteObjHudBoosterCoolTime[i] =
                        this.spriteHudState.AddSprite(spriteCount++,
                        "Booster Cool Time");

                    //  Display message for pickup items
                    this.textPickup[i] = new GameText(this.fontHud,
                                                string.Format("Text pick up {0}", i),
                                                0, 0, hudPickupColor);

                    this.textPickup[i].Visible = false;

                    this.refSceneHudRoot.AddChild(this.textPickup[i]);

                    //  Pickup cool time bar
                    this.spriteObjHudPickupCoolTime[i] = this.spriteHudState.AddSprite(
                                            spriteCount++, "Pickup Cool Time Bar");

                    this.spriteObjHudPickupCoolTime[i].Color = new Color(200, 200, 255);
                    this.spriteObjHudPickupCoolTime[i].Visible = false;

                    //  MachineGun Image
                    this.spriteObjHudWeaponMachineGun[i] =
                        this.spriteHudState.AddSprite(spriteCount++,
                        "MachineGun Image");

                    this.spriteObjHudWeaponMachineGun[i].Visible = false;

                    //  Shotgun Image
                    this.spriteObjHudWeaponShotgun[i] =
                            this.spriteHudState.AddSprite(spriteCount++,
                            "Shotgun Image");

                    this.spriteObjHudWeaponShotgun[i].Visible = false;

                    //  Handgun Image
                    this.spriteObjHudWeaponHandgun[i] =
                            this.spriteHudState.AddSprite(spriteCount++,
                            "Handgun Image");

                    this.spriteObjHudWeaponHandgun[i].Visible = false;

                    // Weapon Infomation 
                    this.textHudCurrentAmmo[i] =
                                new GameText(this.fontHud, "0", 0, 0, textColor);

                    this.refSceneHudRoot.AddChild(this.textHudCurrentAmmo[i]);

                    this.textHudRemainAmmo[i] =
                                new GameText(this.fontHud, "0", 0, 0, textColor);

                    this.refSceneHudRoot.AddChild(this.textHudRemainAmmo[i]);

                    //  Control helper on screen
                    for (int column = 0; column < 2; column++)
                    {
                        this.textControlHelper[i, column] = new GameText(
                                                this.fontHud,
                                                String.Empty,
                                                0, 0,
                                                Color.White);

                        this.refSceneHudRoot.AddChild(this.textControlHelper[i, column]);
                    }
                }

                SetVisibleHud(true);
            }

            //  Mission Result
            {
                this.spriteMission = new GameSprite2D();
                this.spriteMission.Create(2, "Textures/Mission");

                this.refSceneMissionRoot.AddChild(this.spriteMission);

                //  Mission Complete
                this.spriteObjMissionClear =
                                    this.spriteMission.AddSprite(0, "Mission Complete");

                this.spriteObjMissionClear.Visible = false;


                //  Mission Failed
                this.spriteObjMissionFailed =
                                    this.spriteMission.AddSprite(1, "Mission Failed");

                this.spriteObjMissionFailed.Visible = false;
            }

            //  Calculate all image size
            OnSize();
        }

        /// <summary>
        /// changes the size and location of the Hud image according to the screen size 
        /// when the current screen is resized.
        /// </summary>
        public void ResizeHud()
        {
            Rectangle rect;
            int posX = 0, posY = 0;
            int scaledWidth = 0, scaledHeight = 0, currentWidth = 0, currentHeight = 0;
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

            //  if versus mode, re-calculate width scale
            if (GameLevel.Info.GamePlayType == GamePlayTypeId.Versus)
                posScale.X *= 1.3f;

            ViewCamera viewCamera = FrameworkCore.CurrentCamera;
            int viewCount = viewCamera.Count;

            int scale_width = (int)((float)imageAimingWidth * sizeScale.Y);
            int scale_height = (int)((float)imageAimingHeight * sizeScale.Y);

            for (int i = 0; i < viewCount; i++)
            {
                Viewport view = viewCamera.GetViewport(i);

                int posYOffset = (int)((float)(80 / viewCount) * posScale.Y);

                posX = view.X + (view.Width / 2) - (scale_width / 2);
                posY = view.Y + (view.Height / 2) - (scale_height / 2) - posYOffset;

                //  Hud Aiming Site
                this.spriteObjHudAimingSite[i].SourceRectangle =
                            new Rectangle(73, 50, imageAimingWidth, imageAimingHeight);

                this.spriteObjHudAimingSite[i].ScreenRectangle =
                            new Rectangle(posX, posY, scale_width, scale_height);

                //  Hud Alert Site
                this.spriteObjHudAlertSite[i].SourceRectangle =
                            new Rectangle(428, 50, imageAimingWidth, imageAimingHeight);

                this.spriteObjHudAlertSite[i].ScreenRectangle =
                                new Rectangle(posX, posY, scale_width, scale_height);

                //  Armor frame   
                this.spriteObjHudArmorFrame[i].SourceRectangle =
                                        new Rectangle(39, 37, 932, 94);

                posX = Math.Max((int)(90 * posScale.X) + view.X, 
                    (int)(0.05f * view.Width));
                posY = Math.Max((int)(75 / viewCount * posScale.Y) + view.Y, 
                    (int)(0.05f * view.Height * viewCount));

                this.spriteObjHudArmorFrame[i].ScreenRectangle = new Rectangle(
                                        posX, posY,
                                        (int)(926 * sizeScale.X),
                                        (int)(82 * sizeScale.Y));

                //  Armor state bar
                this.spriteObjHudArmorState[i].SourceRectangle =
                                        new Rectangle(41, 166, 727, 24);

                if (GameLevel.Info.GamePlayType == GamePlayTypeId.Versus)
                {
                    rect = new Rectangle(posX + (int)(76 * posScale.X),
                                        posY + (int)(23 * posScale.Y),
                                        (int)(727 * sizeScale.X),
                                        (int)(24 * sizeScale.Y));
                }
                else
                {
                    rect = new Rectangle(posX + (int)(99 * posScale.X),
                                        posY + (int)(24 * posScale.Y),
                                        (int)(727 * sizeScale.X),
                                        (int)(24 * sizeScale.Y));
                }

                this.spriteObjHudArmorState[i].ScreenRectangle = rect;

                //  Weapon window
                int offsetX = view.X;
                int offsetY = view.Y;

                posX = (int)(1375 * posScale.X) + offsetX;
                posY = (int)(820 / viewCount * posScale.Y) + offsetY;

                this.spriteObjHudWeaponWindow[i].SourceRectangle =
                                        new Rectangle(42, 260, 440, 184);

                this.spriteObjHudWeaponWindow[i].ScreenRectangle = new Rectangle(
                                        posX, posY,
                                        (int)(453 * sizeScale.X),
                                        (int)(183 * sizeScale.Y));

                //  Booster Cool Time
                this.spriteObjHudBoosterCoolTime[i].SourceRectangle =
                                        new Rectangle(50, 241, 375, 12);

                if (GameLevel.Info.GamePlayType == GamePlayTypeId.Versus)
                {
                    rect = new Rectangle(posX + (int)(30 * posScale.X),
                                        posY + (int)(10 * posScale.Y),
                                        (int)(375 * sizeScale.X),
                                        (int)(12 * sizeScale.Y));
                }
                else
                {
                    rect = new Rectangle(posX + (int)(40 * posScale.X),
                                        posY + (int)(10 * posScale.Y),
                                        (int)(375 * sizeScale.X),
                                        (int)(12 * sizeScale.Y));
                }

                this.spriteObjHudBoosterCoolTime[i].ScreenRectangle = rect;

                //  Display message for pickup items
                this.textPickup[i].PosX = (int)(view.Width * 0.81f) + offsetX;
                this.textPickup[i].PosY = (int)(view.Height * 0.51f) + offsetY;
                this.textPickup[i].Scale = sizeScale.X * 1.5f;

                //  Control helper on screen
                for (int column = 0; column < 2; column++)
                {
                    Vector2 textSize = this.fontHud.MeasureString(
                                            this.textControlHelper[i, column].Text);

                    posX = (int)(((float)view.Width * 0.5f) -
                                    ((textSize.X / 2) * posScale.X)) + offsetX;

                    posY = (int)(((float)view.Height * (0.7f + (column * 0.05f)) -
                                    ((textSize.Y / 2) * posScale.Y))) + offsetY;

                    this.textControlHelper[i, column].PosX = posX;
                    this.textControlHelper[i, column].PosY = posY;
                    this.textControlHelper[i, column].Scale = sizeScale.Y;
                }

                //  Pickup cool time bar
                this.spriteObjHudPickupCoolTime[i].SourceRectangle =
                                    new Rectangle(40, 165, 180, 24);

                this.spriteObjHudPickupCoolTime[i].ScreenRectangle = new Rectangle(
                        (int)(view.Width / 2) - (int)(180 * sizeScale.X / 2) + offsetX,
                        (int)(view.Height * 0.82f) + view.Y,
                        (int)(180 * sizeScale.X),
                        (int)(24 * sizeScale.Y));

                // Weapon information
                {
                    if (GameLevel.Info.GamePlayType == GamePlayTypeId.Versus)
                    {
                        rect = new Rectangle(
                                (int)this.spriteObjHudWeaponWindow[i].ScreenPosition.X +
                                (int)(33 * posScale.X),
                                (int)this.spriteObjHudWeaponWindow[i].ScreenPosition.Y +
                                (int)(42 * posScale.Y),
                                (int)(imageWeaponWidth * sizeScale.X),
                                (int)(imageWeaponHeight * sizeScale.Y));
                    }
                    else
                    {

                        rect = new Rectangle(
                                (int)this.spriteObjHudWeaponWindow[i].ScreenPosition.X +
                                (int)(43 * posScale.X),
                                (int)this.spriteObjHudWeaponWindow[i].ScreenPosition.Y +
                                (int)(42 * posScale.Y),
                                (int)(imageWeaponWidth * sizeScale.X),
                                (int)(imageWeaponHeight * sizeScale.Y));
                    }

                    //  MachineGun Image
                    this.spriteObjHudWeaponMachineGun[i].SourceRectangle =
                                            new Rectangle(512, 312,
                                            imageWeaponWidth, imageWeaponHeight);

                    this.spriteObjHudWeaponMachineGun[i].ScreenRectangle = rect;

                    //  Shotgun Image
                    this.spriteObjHudWeaponShotgun[i].SourceRectangle =
                                            new Rectangle(717, 233,
                                            imageWeaponWidth, imageWeaponHeight);

                    this.spriteObjHudWeaponShotgun[i].ScreenRectangle = rect;

                    //  Handgun Image
                    this.spriteObjHudWeaponHandgun[i].SourceRectangle =
                                            new Rectangle(720, 350,
                                            imageWeaponWidth, imageWeaponHeight);

                    this.spriteObjHudWeaponHandgun[i].ScreenRectangle = rect;

                    // Weapon Infomation 
                    this.textHudCurrentAmmo[i].Scale = sizeScale.X * 1.4f;
                    this.textHudRemainAmmo[i].Scale = sizeScale.X * 1.4f;
                }
            }


            //  Mission Complete
            currentWidth = (int)((float)FrameworkCore.ViewWidth *
                                ((879 / (float)ViewerWidth.Width1080)));

            currentHeight = (int)((float)FrameworkCore.ViewWidth *
                                ((88 / (float)ViewerWidth.Width1080)));

            posX = (int)((FrameworkCore.ViewWidth / 2) - (currentWidth / 2));
            posY = (int)((FrameworkCore.ViewHeight / 2) - (currentHeight / 2));
            scaledWidth = (int)((float)879 * sizeScale.X);
            scaledHeight = (int)((float)88 * sizeScale.Y);

            this.spriteObjMissionClear.SourceRectangle =
                                new Rectangle(60, 107, 879, 88);

            this.spriteObjMissionClear.ScreenRectangle =
                                new Rectangle(posX, posY, scaledWidth, scaledHeight);

            //  Mission Failed
            currentWidth = (int)((float)FrameworkCore.ViewWidth *
                         ((902 / (float)ViewerWidth.Width1080)));

            currentHeight = (int)((float)FrameworkCore.ViewHeight *
                          ((89 / (float)ViewerWidth.Width1080)));

            posX = (int)((FrameworkCore.ViewWidth / 2) - (currentWidth / 2));
            posY = (int)((FrameworkCore.ViewHeight / 2) - (currentHeight / 2));
            scaledWidth = (int)((float)902 * sizeScale.X);
            scaledHeight = (int)((float)89 * sizeScale.Y);

            this.spriteObjMissionFailed.SourceRectangle =
                                new Rectangle(58, 285, 902, 89);

            this.spriteObjMissionFailed.ScreenRectangle =
                                new Rectangle(posX, posY, scaledWidth, scaledHeight);
        }

        /// <summary>
        /// configures the visibility of the Hud image on screen.
        /// </summary>
        /// <param name="visible">visibility flag</param>
        public virtual void SetVisibleHud(bool visible)
        {
            //  visible all of the Hud
            for (int i = 0; i < GameLevel.PlayerCountInLevel; i++)
            {
                this.textHudCurrentAmmo[i].Visible = visible;
                this.textHudRemainAmmo[i].Visible = visible;

                this.spriteObjHudAimingSite[i].Visible = visible;
                this.spriteObjHudAlertSite[i].Visible = false;

                this.spriteObjHudArmorFrame[i].Visible = visible;
                this.spriteObjHudArmorState[i].Visible = visible;
                this.spriteObjHudWeaponWindow[i].Visible = visible;
                this.spriteObjHudBoosterCoolTime[i].Visible = visible;
                this.spriteObjHudPickupCoolTime[i].Visible = false;

                this.spriteObjHudWeaponMachineGun[i].Visible = false;
                this.spriteObjHudWeaponShotgun[i].Visible = false;
                this.spriteObjHudWeaponHandgun[i].Visible = false;

                this.textPickup[i].Visible = visible;

                for (int j = 0; j < 2; j++)
                {
                    if (this.textControlHelper[i, j] != null)
                        this.textControlHelper[i, j].Visible = visible;
                }
            }
        }

        /// <summary>
        /// configures the visibility of the selected weapon on the current screen.
        /// </summary>
        /// <param name="index">an index of the player</param>
        /// <param name="type">type of the weapon</param>
        public void SetCurrentWeaponHud(int index, WeaponType type)
        {
            //  Change to weapon image int the Hud
            switch (type)
            {
                case WeaponType.PlayerMachineGun:
                    {
                        this.spriteObjHudWeaponMachineGun[index].Visible = true;
                        this.spriteObjHudWeaponShotgun[index].Visible = false;
                        this.spriteObjHudWeaponHandgun[index].Visible = false;
                    }
                    break;
                case WeaponType.PlayerShotgun:
                    {
                        this.spriteObjHudWeaponMachineGun[index].Visible = false;
                        this.spriteObjHudWeaponShotgun[index].Visible = true;
                        this.spriteObjHudWeaponHandgun[index].Visible = false;
                    }
                    break;
                case WeaponType.PlayerHandgun:
                    {
                        this.spriteObjHudWeaponMachineGun[index].Visible = false;
                        this.spriteObjHudWeaponShotgun[index].Visible = false;
                        this.spriteObjHudWeaponHandgun[index].Visible = true;
                    }
                    break;
                default:
                    {
                        this.spriteObjHudWeaponMachineGun[index].Visible = false;
                        this.spriteObjHudWeaponShotgun[index].Visible = false;
                        this.spriteObjHudWeaponHandgun[index].Visible = false;
                    }
                    break;
            }
        }

        /// <summary>
        /// updates Hud images using Hud information.
        /// </summary>
        protected void UpdateHud(GameTime gameTime)
        {
            float scaleFactor = 1.0f;

            if (GameLevel.Info.GamePlayType == GamePlayTypeId.Versus)
                scaleFactor = 0.8f;

            Vector2 textSize = Vector2.Zero;

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

            //  Update players info in Hud (player's life, bullets, weapon)
            for (int i = 0; i < GameLevel.PlayerCountInLevel; i++)
            {
                GamePlayer player = GameLevel.GetPlayerInLevel(i);
                GameWeapon weapon = player.CurrentWeapon;

                // Update player's life bar image
                {
                    float armorGaugeRate = (float)player.Life /
                                                (float)player.SpecData.Life;

                    int size_ScaledWidth = (int)(sizeScale.X * 727);
                    int size_ScaledHeight = (int)(sizeScale.Y * 24);
                    int size_CalcWidth = (int)(size_ScaledWidth * armorGaugeRate);

                    this.spriteObjHudArmorState[i].ScreenSize =
                                        new Vector2(size_CalcWidth, size_ScaledHeight);

                    if (this.playerOldLife[i] > player.Life)
                    {
                        //    To be red state
                        this.playerReduceArmorElapsedTime[i] = 0.25f;
                    }

                    this.playerOldLife[i] = player.Life;

                    if (this.playerReduceArmorElapsedTime[i] > 0.0f)
                    {
                        //  Player's life to be red color during few seconds
                        this.spriteObjHudArmorState[i].Color = new Color(255, 0, 0);

                        this.playerReduceArmorElapsedTime[i] -=
                                    (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        //  normal state
                        this.spriteObjHudArmorState[i].Color = new Color(255, 255, 255);
                    }
                }

                //  Booster cool time
                {
                    int size_ScaledWidth = (int)(sizeScale.X * 375);
                    int size_ScaledHeight = (int)(sizeScale.Y * 12);
                    int size_CalcWidth = (int)((float)size_ScaledWidth *
                                                    player.BoosterCoolTimeRate);

                    this.spriteObjHudBoosterCoolTime[i].ScreenSize =
                                    new Vector2(size_CalcWidth, size_ScaledHeight);
                }

                //  Pickup cool time
                {
                    int size_ScaledWidth = (int)(sizeScale.X * 180);
                    int size_ScaledHeight = (int)(sizeScale.Y * 24);

                    int size_CalcWidth = (int)((float)size_ScaledWidth *
                                                    player.PickupCoolTimeRate);

                    this.spriteObjHudPickupCoolTime[i].ScreenSize =
                                    new Vector2(size_CalcWidth, size_ScaledHeight);
                }

                //  Update Weapon window
                {
                    //  current ammo of player's weapon
                    this.textHudCurrentAmmo[i].Text = weapon.CurrentAmmo.ToString();
                    textSize = 
                        this.fontHud.MeasureString(this.textHudCurrentAmmo[i].Text);

                    this.textHudCurrentAmmo[i].PosX =
                        (int)(this.spriteObjHudWeaponWindow[i].ScreenPosition.X +
                            ((int)((float)310 * posScale.X)) -
                            (textSize.X * this.textHudCurrentAmmo[i].Scale * 0.5f));

                    this.textHudCurrentAmmo[i].PosY =
                        (int)this.spriteObjHudWeaponWindow[i].ScreenPosition.Y +
                            ((int)((float)40 * posScale.Y));

                    //  total reamain ammo of player's weapon
                    this.textHudRemainAmmo[i].Text = weapon.RemainAmmo.ToString();
                    textSize = 
                        this.fontHud.MeasureString(this.textHudRemainAmmo[i].Text);

                    this.textHudRemainAmmo[i].PosX =
                        (int)(this.spriteObjHudWeaponWindow[i].ScreenPosition.X +
                            ((int)((float)310 * posScale.X)) -
                            (textSize.X * this.textHudRemainAmmo[i].Scale * 0.5f));

                    this.textHudRemainAmmo[i].PosY =
                        (int)this.spriteObjHudWeaponWindow[i].ScreenPosition.Y +
                            ((int)((float)110 * posScale.Y));
                }

                //  Update pickup item message
                {
                    if (this.pickupMessageElapsedTime[i] > 0.0f)
                    {
                        this.pickupMessageElapsedTime[i] -=
                            (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        this.pickupMessageElapsedTime[i] = 0.0f;

                        this.textPickup[i].Visible = false;
                    }
                }

                //  Update Aiming site
                {
                    if (player.IsFiring)
                        this.spriteObjHudAlertSite[i].Visible = true;
                    else
                        this.spriteObjHudAlertSite[i].Visible = false;
                }
            }
        }

        /// <summary>
        /// configures the message to be displayed on screen when an item or 
        /// a weapon is picked up.
        /// The messages are displayed on the right side of the screen.
        /// </summary>
        /// <param name="index">an index of the player</param>
        /// <param name="message">a message</param>
        /// <param name="duration">duration of message on screen</param>
        public void DisplayPickup(int index, string message, float duration)
        {
            //  Display pickup information during few time
            this.pickupMessageElapsedTime[index] = duration;

            this.textPickup[index].Text = message;
            this.textPickup[index].Visible = true;
        }

        /// <summary>
        /// Displays the given text for the given player to specify a game control.
        /// It is displayed on the bottom of the screen.
        /// </summary>
        /// <param name="index">an index of the player</param>
        /// <param name="column">column number of helper message</param>
        /// <param name="message">a message</param>
        public void DisplayControlHelper(int index, int column, string message)
        {
            ViewCamera viewCamera = FrameworkCore.CurrentCamera;

            float currentViewX = viewCamera.GetViewport(index).X;
            float currentViewY = viewCamera.GetViewport(index).Y;
            float currentViewWidth = viewCamera.GetViewport(index).Width;
            float currentViewHeight = viewCamera.GetViewport(index).Height;

            float scaleFactor = 1.0f;

            if (GameLevel.Info.GamePlayType == GamePlayTypeId.Versus)
                scaleFactor = 0.8f;

            //  Scaling image size and positioning for screen resolution
            Vector2 scale = new Vector2((float)FrameworkCore.ViewWidth * scaleFactor /
                                        (float)ViewerWidth.Width1080,
                                        (float)FrameworkCore.ViewHeight * scaleFactor /
                                        (float)ViewerHeight.Height1080) * 1.6f;

            Vector2 textSize = this.fontHud.MeasureString(message);

            int posx = (int)(((float)currentViewWidth * 0.5f) -
                            ((textSize.X / 2) * scale.X)) + (int)currentViewX;

            int posy = (int)(((float)currentViewHeight * (0.7f + (column * 0.05f)) -
                            ((textSize.Y / 2) * scale.Y))) + (int)currentViewY;

            this.textControlHelper[index, column].PosX = posx;
            this.textControlHelper[index, column].PosY = posy;
            this.textControlHelper[index, column].Scale = scale.X;
            this.textControlHelper[index, column].Text = message;

            this.textControlHelper[index, column].Visible = true;
        }

        /// <summary>
        /// makes the help message to be displayed on screen invisible.
        /// </summary>
        /// <param name="index">an index of the player</param>
        /// <param name="column">column number of heler message</param>
        public void DisableControlHelper(int index, int column)
        {
            if (this.textControlHelper[index, column] != null)
            {
                this.textControlHelper[index, column].Text = String.Empty;
                this.textControlHelper[index, column].Visible = false;
            }
        }

        /// <summary>
        /// makes every help message to be displayed on screen invisible.
        /// </summary>
        public void DisableAllControlHelper()
        {
            for (int index = 0; index < GameLevel.PlayerCountInLevel; index++)
            {
                for (int column = 0; column < GameLevel.PlayerCountInLevel; column++)
                {
                    if (this.textControlHelper[index, column] != null)
                    {
                        this.textControlHelper[index, column].Text = String.Empty;
                        this.textControlHelper[index, column].Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// configures the visibility of the cool-time gauge 
        /// when picking up the weapon on world.
        /// </summary>
        /// <param name="index">an index of the player</param>
        /// <param name="visible">visiblity flag</param>
        public void DisplayPickupCoolTime(int index, bool visible)
        {
            this.spriteObjHudPickupCoolTime[index].Visible = visible;
        }

        #endregion

        #region Game Level

        /// <summary>
        /// loads every game resource relevant to the level.
        /// It loads every game class, such as players, weapons, items, etc., and 
        /// registers collision data to the collision layer of the stage.
        /// </summary>
        /// <param name="levelFile"></param>
        /// <param name="sceneParent"></param>
        public void LoadLevel(string levelFile)
        {
            //  Load all players, enemies, items, weapons, and level data
            GameLevel.LoadLevel(levelFile);

            //  Entry collision of the units to layer in the level
            switch (RobotGameGame.CurrentGameLevel.Info.GamePlayType)
            {
                case GamePlayTypeId.StageClear:
                    {
                        this.playerOldLife[0] =
                            RobotGameGame.CurrentGameLevel.GetPlayerInLevel(0).Life;
                    }
                    break;
                case GamePlayTypeId.Versus:
                    {
                        int count = RobotGameGame.CurrentGameLevel.PlayerCountInLevel;

                        for (int i = 0; i < count; i++)
                        {
                            this.playerOldLife[i] =
                                RobotGameGame.CurrentGameLevel.GetPlayerInLevel(i).Life;
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Mission

        protected virtual void CheckMission(GameTime gameTime) { }

        #endregion
    }
}
