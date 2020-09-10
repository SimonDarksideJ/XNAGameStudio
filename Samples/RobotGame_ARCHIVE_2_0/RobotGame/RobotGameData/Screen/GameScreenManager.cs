#region File Description
//-----------------------------------------------------------------------------
// GameScreenManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData.Resource;
using RobotGameData.GameObject;
#endregion

namespace RobotGameData.Screen
{
    /// <summary>
    /// The screen manager is a component which manages one or more GameScreen
    /// instances. It maintains a stack of screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes input to the
    /// topmost active screen.
    /// </summary>
    public class GameScreenManager : DrawableGameComponent
    {
        #region Fields

        List<GameScreen> screens = new List<GameScreen>();
        List<GameScreen> screensToUpdate = new List<GameScreen>();

        GameScreenInput[] screenInput = new GameScreenInput[4];
        GameScreen currentScreen = null;
        GameSceneNode scene2DFadeRoot = null;
        
        GameSprite2D fadeSprite = null;        
        Sprite2DObject fadeObject = null;
        
        bool traceEnabled;

        #endregion

        #region Properties

        /// <summary>
        /// Expose access to our Game instance (this is protected in the
        /// default GameComponent, but we want to make it public).
        /// </summary>
        new public Game Game
        {
            get { return base.Game; }
        }
                
        /// <summary>
        /// A content manager used to load data that is shared between multiple
        /// screens. This is never unloaded, so if a screen requires a large amount
        /// of temporary data, it should create a local content manager instead.
        /// </summary>
        public static ContentManager Content
        {
            get { return FrameworkCore.ContentManager; }
        }

        /// <summary>
        /// A default SpriteBatch shared by all the screens. This saves
        /// each screen having to bother creating their own local instance.
        /// </summary>
        public static SpriteBatch SpriteBatch
        {
            get { return FrameworkCore.RenderContext.SpriteBatch; }
        }

        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool TraceEnabled
        {
            get { return traceEnabled; }
            set { traceEnabled = value; }
        }

        public GameScreen CurrentScreen
        {
            get { return currentScreen; }
        }

        public int InputCount
        {
            get { return screenInput.Length; }
        }

        public GameScreenInput SingleInput
        {
            get { return screenInput[0]; }
        }

        public GameScreenInput[] ScreenInput
        {
            get { return screenInput; }
        }

        #endregion

        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public GameScreenManager(Game game)
            : base(game)
        {
            scene2DFadeRoot = FrameworkCore.Scene2DFadeLayer;
            fadeSprite = new GameSprite2D();

            scene2DFadeRoot.AddChild(fadeSprite);

            //  Controller
            screenInput[0] = new GameScreenInput(PlayerIndex.One);
            screenInput[1] = new GameScreenInput(PlayerIndex.Two);
            screenInput[2] = new GameScreenInput(PlayerIndex.Three);
            screenInput[3] = new GameScreenInput(PlayerIndex.Four);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {            
            // Load content belonging to the screen manager.
            fadeSprite.Create(1, "blank");

            fadeObject = fadeSprite.AddSprite(0, "Screen fade");
            
            fadeObject.ScreenSize = 
                    new Vector2(FrameworkCore.ViewWidth, FrameworkCore.ViewHeight);

            fadeObject.Visible = false;

            // Tell each of the screens to load their content.
            foreach (GameScreen screen in screens)
            {
                screen.LoadContent();
            }
        }

        public void OnSize(Rectangle newRect)
        {
            if (fadeObject != null)
                fadeObject.ScreenSize = new Vector2(newRect.Width, newRect.Height);

            foreach (GameScreen screen in screens)
            {
                screen.OnSize(newRect);
            }
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Unload content belonging to the screen manager.
            scene2DFadeRoot.RemoveAllChild(true);

            // Tell each of the screens to unload their content.
            foreach (GameScreen screen in screens)
            {
                screen.UnloadContent();
            }
        }

        #region Update and Draw

        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            screensToUpdate.Clear();

            foreach (GameScreen screen in screens)
                screensToUpdate.Add(screen);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                GameScreen screen = screensToUpdate[screensToUpdate.Count - 1];

                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput(gameTime);

                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }

            // Print debug trace?
            if (traceEnabled)
                TraceScreens();
        }


        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        void TraceScreens()
        {
            List<string> screenNames = new List<string>();

            foreach (GameScreen screen in screens)
                screenNames.Add(screen.GetType().Name);

            Trace.WriteLine(string.Join(", ", screenNames.ToArray()));
        }


        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            foreach (GameScreen screen in screens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;
             
                screen.Draw(gameTime);
            }          
        }

        #endregion

        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen, bool callLoadContent)
        {
            screen.GameScreenManager = this;

            // If we have a graphics device, tell the screen to load content.
            if (GraphicsDevice != null && callLoadContent)
            {
                screen.LoadContent();
            }

            screen.InitializeScreen();

            screens.Add(screen);

            currentScreen = screen;

            for (int i = 0; i < screenInput.Length; i++)
                screenInput[i].Reset();
        }

        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            screen.FinalizeScreen();

            // If we have a graphics device, tell the screen to unload content.
            if (GraphicsDevice != null)
            {
                screen.UnloadContent();
            }

            if (currentScreen == screen)
                currentScreen = null; 
            
            screens.Remove(screen);
            screensToUpdate.Remove(screen);
        }


        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(int alpha)
        {
            if (alpha > 0)
            {
                fadeObject.Visible = true;
                fadeObject.Color = new Color(0, 0, 0, (byte)alpha);
            }
            else
            {
                fadeObject.Visible = false;
            }
        }
    }
}
