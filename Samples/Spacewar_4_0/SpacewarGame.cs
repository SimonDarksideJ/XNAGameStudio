#region File Description
//-----------------------------------------------------------------------------
// SpacewarGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace Spacewar
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    partial class SpacewarGame : Microsoft.Xna.Framework.Game
    {
        // these are the size of the offscreen drawing surface
        // in general, no one wants to change these as there
        // are all kinds of UI calculations and positions based
        // on these dimensions.
        const int FixedDrawingWidth = 1280;
        const int FixedDrawingHeight = 720;

        // these are the size of the output window, ignored
        // on Xbox 360
        private int preferredWindowWidth = 1280;
        private int preferredWindowHeight = 720;

        private static ContentManager contentManager;

        /// <summary>
        /// The game settings from settings.xml
        /// </summary>
        private static Settings settings = new Settings();

        private static Camera camera;

        /// <summary>
        /// Information about the players such as score, health etc
        /// </summary>
        private static Player[] players;

        /// <summary>
        /// The current game state
        /// </summary>
        private static GameState gameState = GameState.Started;

        /// <summary>
        /// Which game board are we playing on
        /// </summary>
        private static int gameLevel;

        /// <summary>
        /// Stores game paused state
        /// </summary>
        private bool paused;

        private GraphicsDeviceManager graphics;

        private bool enableDrawScaling;
        private RenderTarget2D drawBuffer;
        private SpriteBatch spriteBatch;

        private static Screen currentScreen;

        private static PlatformID currentPlatform;

        private static KeyboardState keyState;
        private bool justWentFullScreen;

        #region Properties
        public static GameState GameState
        {
            get
            {
                return gameState;
            }
        }

        public static int GameLevel
        {
            get
            {
                return gameLevel;
            }
            set
            {
                gameLevel = value;
            }
        }

        public static Camera Camera
        {
            get
            {
                return camera;
            }
        }

        public static Settings Settings
        {
            get
            {
                return settings;
            }
        }

        public static Player[] Players
        {
            get
            {
                return players;
            }
        }

        public static ContentManager ContentManager
        {
            get
            {
                return contentManager;
            }
        }

        public static PlatformID CurrentPlatform
        {
            get
            {
                return currentPlatform;
            }
        }

        public static KeyboardState KeyState
        {
            get
            {
                return keyState;
            }
        }
        #endregion

        public SpacewarGame()
        {
#if XBOX360
            // we might as well use the xbox in all its glory
            preferredWindowWidth = FixedDrawingWidth;
            preferredWindowHeight = FixedDrawingHeight;
            enableDrawScaling = false;
#else
            enableDrawScaling = true;
#endif

            this.graphics = new Microsoft.Xna.Framework.GraphicsDeviceManager(this);
            this.graphics.PreferredBackBufferWidth = preferredWindowWidth;
            this.graphics.PreferredBackBufferHeight = preferredWindowHeight;            

            // Game should run as fast as possible.
            IsFixedTimeStep = false;
        }        

        protected override void Initialize()
        {
            // game initialization code here

            //Uncomment this line to force a save of the default settings file. Useful when you had added things to settings.cs
            //NOTE in VS this will go in DEBUG or RELEASE - need to copy up to main project
            //Settings.Save("settings.xml");

            settings = Settings.Load("settings.xml");

            currentPlatform = System.Environment.OSVersion.Platform;

            //Initialise the sound
            Sound.Initialize();

            Window.Title = Settings.WindowTitle;

            base.Initialize();
        }

        protected override void BeginRun()
        {
            Sound.PlayCue(Sounds.TitleMusic);

            //Kick off the game by loading the logo splash screen
            ChangeState(GameState.LogoSplash);

            float fieldOfView = (float)Math.PI / 4;
            float aspectRatio = (float)FixedDrawingWidth / (float)FixedDrawingHeight;
            float nearPlane = 10f;
            float farPlane = 700f;

            camera = new Camera(fieldOfView, aspectRatio, nearPlane, farPlane);
            camera.ViewPosition = new Vector3(0, 0, 500);

            base.BeginRun();
        }

        protected override void Update(GameTime gameTime)
        {
            TimeSpan elapsedTime = gameTime.ElapsedGameTime;
            TimeSpan time = gameTime.TotalGameTime;

            // The time since Update was called last
            float elapsed = (float)elapsedTime.TotalSeconds;

            GameState changeState = GameState.None;

            keyState = Keyboard.GetState();
            XInputHelper.Update(this, keyState);

            if ((keyState.IsKeyDown(Keys.RightAlt) || keyState.IsKeyDown(Keys.LeftAlt)) && keyState.IsKeyDown(Keys.Enter) && !justWentFullScreen)
            {
                ToggleFullScreen();
                justWentFullScreen = true;
            }

            if (keyState.IsKeyUp(Keys.Enter))
            {
                justWentFullScreen = false;
            }

            if (XInputHelper.GamePads[PlayerIndex.One].BackPressed ||
                XInputHelper.GamePads[PlayerIndex.Two].BackPressed)
            {
                if (gameState == GameState.PlayEvolved || gameState == GameState.PlayRetro)
                {
                    paused = !paused;
                }

                if (gameState == GameState.LogoSplash)
                {
                    this.Exit();
                }
            }

            //Reload settings file?
            if (XInputHelper.GamePads[PlayerIndex.One].YPressed)
            {
                //settings = Settings.Load("settings.xml");
                //GC.Collect();
            }

            if (!paused)
            {
                //Update everything
                changeState = currentScreen.Update(time, elapsedTime);

                // Update the AudioEngine - MUST call this every frame!!
                Sound.Update();

                //If either player presses start then reset the game
                if (XInputHelper.GamePads[PlayerIndex.One].StartPressed ||
                    XInputHelper.GamePads[PlayerIndex.Two].StartPressed)
                {
                    changeState = GameState.LogoSplash;
                }

                if (changeState != GameState.None)
                {
                    ChangeState(changeState);
                }
            }

            base.Update(gameTime);
        }

        protected override bool BeginDraw()
        {
            if (!base.BeginDraw())
                return false;

            BeginDrawScaling();

            return true;
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, 
                Color.CornflowerBlue, 1.0f, 0);            

            base.Draw(gameTime);

            currentScreen.Render();
        }

        protected override void EndDraw()
        {
            EndDrawScaling();

            base.EndDraw();
        }

        internal void ChangeState(GameState NextState)
        {
            //Logo spash can come from ANY state since its the place you go when you restart
            if (NextState == GameState.LogoSplash)
            {
                if (currentScreen != null)
                    currentScreen.Shutdown();

                currentScreen = new TitleScreen(this);
                gameState = GameState.LogoSplash;
            }
            else if (gameState == GameState.LogoSplash && NextState == GameState.ShipSelection)
            {
                Sound.PlayCue(Sounds.MenuAdvance);

                //This is really where the game starts so setup the player information
                players = new Player[2] { new Player(), new Player() };

                //Start at level 1
                gameLevel = 1;

                currentScreen.Shutdown();
                currentScreen = new SelectionScreen(this);
                gameState = GameState.ShipSelection;
            }
            else if (gameState == GameState.PlayEvolved && NextState == GameState.ShipUpgrade)
            {
                currentScreen.Shutdown();
                currentScreen = new ShipUpgradeScreen(this);
                gameState = GameState.ShipUpgrade;
            }
            else if ((gameState == GameState.ShipSelection || GameState == GameState.ShipUpgrade) && NextState == GameState.PlayEvolved)
            {
                Sound.PlayCue(Sounds.MenuAdvance);

                currentScreen.Shutdown();
                currentScreen = new EvolvedScreen(this);
                gameState = GameState.PlayEvolved;
            }
            else if (gameState == GameState.LogoSplash && NextState == GameState.PlayRetro)
            {
                //Game starts here for retro
                players = new Player[2] { new Player(), new Player() };

                currentScreen.Shutdown();
                currentScreen = new RetroScreen(this);
                gameState = GameState.PlayRetro;
            }
            else if (gameState == GameState.PlayEvolved && NextState == GameState.Victory)
            {
                currentScreen.Shutdown();
                currentScreen = new VictoryScreen(this);
                gameState = GameState.Victory;
            }
            else
            {
                //This is a BAD thing and should never happen
                // What does this map to on XBox 360?
                //Debug.Assert(false, String.Format("Invalid State transition {0} to {1}", gameState.ToString(), NextState.ToString()));
            }
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            contentManager = new ContentManager(Services);

            if (currentScreen != null)
                currentScreen.OnCreateDevice();

            Font.Init(this);

            if (enableDrawScaling)
            {
                PresentationParameters pp = graphics.GraphicsDevice.PresentationParameters;

                drawBuffer = new RenderTarget2D(graphics.GraphicsDevice,
                                                FixedDrawingWidth, FixedDrawingHeight,
                                                true, SurfaceFormat.Color,
                                                DepthFormat.Depth24Stencil8, pp.MultiSampleCount,
                                                RenderTargetUsage.DiscardContents);

                spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            }
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            if (drawBuffer != null)
            {
                drawBuffer.Dispose();
                drawBuffer = null;
            }

            if (spriteBatch != null)
            {
                spriteBatch.Dispose();
                spriteBatch = null;
            }

            Font.Dispose();

            if (contentManager != null)
            {
                contentManager.Dispose();
                contentManager = null;
            }
        }

        private void ToggleFullScreen()
        {
            PresentationParameters presentation = graphics.GraphicsDevice.PresentationParameters;

            if (presentation.IsFullScreen)
            {   // going windowed
                graphics.PreferredBackBufferWidth = preferredWindowWidth;
                graphics.PreferredBackBufferHeight = preferredWindowHeight;
            }
            else
            {
                // going fullscreen, use desktop resolution to minimize display mode changes
                // this also has the nice effect of working around some displays that lie about 
                // supporting 1280x720
                GraphicsAdapter adapter = graphics.GraphicsDevice.Adapter;
                graphics.PreferredBackBufferWidth = adapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = adapter.CurrentDisplayMode.Height;
            }

            graphics.ToggleFullScreen();
        }

        private void BeginDrawScaling()
        {
            if (enableDrawScaling && drawBuffer != null)
            {
                graphics.GraphicsDevice.SetRenderTarget(drawBuffer);
            }
        }

        private void EndDrawScaling()
        {
            // copy our offscreen surface to the backbuffer with appropriate
            // letterbox bars

            if (!enableDrawScaling || drawBuffer == null)
                return;

            graphics.GraphicsDevice.SetRenderTarget(null);

            PresentationParameters presentation = graphics.GraphicsDevice.PresentationParameters;

            float outputAspect = (float)presentation.BackBufferWidth / (float)presentation.BackBufferHeight;
            float preferredAspect = (float)FixedDrawingWidth / (float)FixedDrawingHeight;

            Rectangle dst;

            if (outputAspect <= preferredAspect)
            {
                // output is taller than it is wider, bars on top/bottom

                int presentHeight = (int)((presentation.BackBufferWidth / preferredAspect) + 0.5f);
                int barHeight = (presentation.BackBufferHeight - presentHeight) / 2;

                dst = new Rectangle(0, barHeight, presentation.BackBufferWidth, presentHeight);
            }
            else
            {
                // output is wider than it is tall, bars left/right

                int presentWidth = (int)((presentation.BackBufferHeight * preferredAspect) + 0.5f);
                int barWidth = (presentation.BackBufferWidth - presentWidth) / 2;

                dst = new Rectangle(barWidth, 0, presentWidth, presentation.BackBufferHeight);
            }

            // clear to get black bars
            graphics.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

            // draw a quad to get the draw buffer to the back buffer
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(drawBuffer, dst, Color.White);
            spriteBatch.End();
        }
    }
}
