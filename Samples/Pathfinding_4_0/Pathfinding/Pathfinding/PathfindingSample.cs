#region File Description
//-----------------------------------------------------------------------------
// PathfindingGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace Pathfinding
{

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Pathfinding : Microsoft.Xna.Framework.Game
    {
        #region Constants        
        
        const int bottomUIHeight = 80;
        const int sliderButtonWidth = 20;

        #endregion

        #region Fields
        private KeyboardState currentKeyboardState;
        private GamePadState currentGamePadState;

        private Texture2D ButtonA;
        private Texture2D ButtonB;
        private Texture2D ButtonX;
        private Texture2D ButtonY;
        private Texture2D onePixelWhite;

        private Rectangle buttonStartStop = new Rectangle(5, 405, 110, 30);
        private Rectangle buttonReset = new Rectangle(125, 405, 110, 30);
        private Rectangle buttonNextMap = new Rectangle(245, 405, 110, 30);
        private Rectangle buttonPathfinding = new Rectangle(365, 405, 270, 30);
        private Rectangle barTimeStep = new Rectangle(125, 450, 200, 20);

        private Vector2 pathStatusPosition;
        private Vector2 searchStepsPosition;

        private Rectangle gameplayArea;

        private SpriteFont HUDFont;
                
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Map map;
        Tank tank;
        PathFinder pathFinder;
        #endregion        

        public Pathfinding()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;
#endif    

            map = new Map();
            tank = new Tank();
            pathFinder = new PathFinder();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            // TODO: Add your initialization logic here

            gameplayArea = GraphicsDevice.Viewport.TitleSafeArea;
            
            // The bottom part of the screen houses controls for the buttons                        
            gameplayArea.Height -= bottomUIHeight;

            pathStatusPosition = new Vector2(645, 425);
            searchStepsPosition = new Vector2(645, 445);

            TouchPanel.EnabledGestures = GestureType.Tap;
                                                        
            map.UpdateMapViewport(gameplayArea);
            tank.Initialize(map);
            pathFinder.Initialize(map);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            ButtonA = Content.Load<Texture2D>("xboxControllerButtonA");
            ButtonB = Content.Load<Texture2D>("xboxControllerButtonB");
            ButtonX = Content.Load<Texture2D>("xboxControllerButtonX");
            ButtonY = Content.Load<Texture2D>("xboxControllerButtonY");

            HUDFont = Content.Load<SpriteFont>("HUDFont");

            onePixelWhite = new Texture2D(GraphicsDevice,1,1);
            onePixelWhite.SetData<Color>(new Color[] { Color.White });

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            map.LoadContent(Content);
            tank.LoadContent(Content);
            pathFinder.LoadContent(Content);

            // TODO: use this.Content to load your game content here
            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        #region Update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();            

            if (map.MapReload)
            {
                map.ReloadMap();
                map.UpdateMapViewport(gameplayArea);
                tank.Reset();
                pathFinder.Reset();
            }

            if (pathFinder.SearchStatus == SearchStatus.PathFound && !tank.Moving)
            {
                foreach (Point point in pathFinder.FinalPath())
                {
                    tank.Waypoints.Enqueue(map.MapToWorld(point, true));
                }
                tank.Moving = true;
            }
            pathFinder.Update(gameTime);
            tank.Update(gameTime);
            base.Update(gameTime);
        }
      
        #endregion

        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {            
            GraphicsDevice.Clear(Color.Black);

            map.Draw(spriteBatch);
            pathFinder.Draw(spriteBatch);
            tank.Draw(spriteBatch);
            
            DrawHUD(spriteBatch);
            DrawPathStatus(spriteBatch);
            
            base.Draw(gameTime);
        }

        /// <summary>
        /// Helper function used by Draw. It is used to draw the slider bars
        /// </summary>
        private void DrawBar(Rectangle bar, float barWidthNormalized, string label)
        {                     
            // Calculate how wide the bar should be, and then draw it.            
            bar.Height /= 2;
            spriteBatch.Draw(onePixelWhite, bar, Color.White);

            // Draw the slider
            spriteBatch.Draw(onePixelWhite, new Rectangle(bar.X + (int)(bar.Width * barWidthNormalized),
                             bar.Y - bar.Height / 2, sliderButtonWidth, bar.Height * 2), Color.Orange);

            // Finally, draw the label to the left of the bar.
            Vector2 labelSize = HUDFont.MeasureString(label);
            Vector2 labelPosition = new Vector2(bar.X - 10 - labelSize.X, bar.Y - bar.Height / 2);
            spriteBatch.DrawString(HUDFont, label, labelPosition, Color.White);
        }

        /// <summary>
        /// Helper function used to draw the buttons
        /// </summary>
        /// <param name="button"></param>
        /// <param name="label"></param>
        private void DrawButton(Rectangle button, string label)
        {
            spriteBatch.Draw(onePixelWhite, button, Color.Orange);
            spriteBatch.DrawString(HUDFont, label, new Vector2(button.Left + 10, button.Top + 5), Color.Black);
        }
        /// <summary>
        /// Helper function used to draw the HUD
        /// </summary>
        /// <param name="spriteBatch"></param>
        private void DrawHUD(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            DrawBar(barTimeStep, pathFinder.TimeStep, "Time Step:");

            #if WINDOWS_PHONE
            DrawButton(buttonStartStop, "Start/Stop");
            DrawButton(buttonReset, "Reset");
            DrawButton(buttonPathfinding, "Pathfinding mode: " + pathFinder.SearchMethod.ToString());
            DrawButton(buttonNextMap, "Next Map");            
#else
            float textureWidth = ButtonA.Width;
            
            spriteBatch.Draw(ButtonA, new Vector2(10, 400), Color.White);            
            spriteBatch.DrawString(
                HUDFont, " Start/Stop",
                new Vector2(10 + textureWidth, 400), Color.White);

            spriteBatch.Draw(ButtonB, new Vector2(150, 400), Color.White);            
            spriteBatch.DrawString(
                HUDFont, " Reset",
                new Vector2(150 + textureWidth, 400), Color.White);

            spriteBatch.Draw(ButtonY, new Vector2(250, 400), Color.White);
            spriteBatch.DrawString(
                HUDFont, " Next map",
                new Vector2(250 + textureWidth, 400), Color.White);

            spriteBatch.Draw(ButtonX, new Vector2(400, 400), Color.White);            
            spriteBatch.DrawString(
                HUDFont, " Pathfinding mode: " + pathFinder.SearchMethod.ToString(),
                new Vector2(400 + textureWidth, 400), Color.White);

            
#endif            
            spriteBatch.End();
        }

        /// <summary>
        /// Helper function used to draw the path status of the tank
        /// </summary>
        /// <param name="spriteBatch"></param>
        private void DrawPathStatus(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            
            string stepString = string.Format("Search Steps: {0}",
                pathFinder.TotalSearchSteps);
            spriteBatch.DrawString(HUDFont, stepString, searchStepsPosition,
                Color.White);
                        
            switch (pathFinder.SearchStatus)
            {
                case SearchStatus.Stopped:                    
                    spriteBatch.DrawString(
                        HUDFont, "Not Searching", pathStatusPosition, Color.White);
                    break;
                case SearchStatus.Searching:                    
                    spriteBatch.DrawString(
                        HUDFont, "Searching...", pathStatusPosition, Color.White);
                    break;
                case SearchStatus.PathFound:                    
                    spriteBatch.DrawString(
                        HUDFont, "Path Found!", pathStatusPosition, Color.Green);
                    break;
                case SearchStatus.NoPath:                    
                    spriteBatch.DrawString(
                        HUDFont, "No Path Found!", pathStatusPosition, Color.Red);
                    break;
                default:
                    break;
            }
            spriteBatch.End();
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Handle input for the sample
        /// </summary>
        void HandleInput()
        {
            KeyboardState previousKeyboardState = currentKeyboardState;
            GamePadState previousGamePadState = currentGamePadState;

            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentKeyboardState = Keyboard.GetState();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (previousGamePadState.Buttons.A == ButtonState.Released &&
                currentGamePadState.Buttons.A == ButtonState.Pressed ||
                previousKeyboardState.IsKeyUp(Keys.A) &&
                currentKeyboardState.IsKeyDown(Keys.A))
            {
                pathFinder.IsSearching = !pathFinder.IsSearching;
            }

            if (previousGamePadState.Buttons.B == ButtonState.Released &&
                currentGamePadState.Buttons.B == ButtonState.Pressed ||
                previousKeyboardState.IsKeyUp(Keys.B) &&
                currentKeyboardState.IsKeyDown(Keys.B))
            {
                map.MapReload = true;
            }

            if (previousGamePadState.Buttons.X == ButtonState.Released &&
                currentGamePadState.Buttons.X == ButtonState.Pressed ||
                previousKeyboardState.IsKeyUp(Keys.X) &&
                currentKeyboardState.IsKeyDown(Keys.X))
            {
                pathFinder.NextSearchType();
            }

            if (previousGamePadState.Buttons.Y == ButtonState.Released &&
                currentGamePadState.Buttons.Y == ButtonState.Pressed ||
                previousKeyboardState.IsKeyUp(Keys.Y) &&
                currentKeyboardState.IsKeyDown(Keys.Y))
            {
                map.CycleMap();
            }

            if (previousGamePadState.DPad.Right == ButtonState.Released &&
                currentGamePadState.DPad.Right == ButtonState.Pressed ||
                previousKeyboardState.IsKeyUp(Keys.Right) &&
                currentKeyboardState.IsKeyDown(Keys.Right))
            {
                pathFinder.TimeStep += .1f;
            }

            if (previousGamePadState.DPad.Left == ButtonState.Released &&
               currentGamePadState.DPad.Left == ButtonState.Pressed ||
               previousKeyboardState.IsKeyUp(Keys.Left) &&
               currentKeyboardState.IsKeyDown(Keys.Left))
            {
                pathFinder.TimeStep -= .1f;
            }

            pathFinder.TimeStep = MathHelper.Clamp(pathFinder.TimeStep, 0f, 1f);               

            TouchCollection rawTouch = TouchPanel.GetState();

            // Use raw touch for the sliders
            if (rawTouch.Count > 0)
            {
                // Only grab the first one
                TouchLocation touchLocation = rawTouch[0];

                // Create a collidable rectangle to determine if we touched the controls
                Rectangle touchRectangle = new Rectangle((int)touchLocation.Position.X,
                                                         (int)touchLocation.Position.Y, 10, 10);

                // Have the sliders rely on the raw touch to function properly
                if (barTimeStep.Intersects(touchRectangle))
                {
                    pathFinder.TimeStep = (float)(touchRectangle.X - barTimeStep.X) / (float)barTimeStep.Width;
                }
            }

            // Next we handle all of the gestures. since we may have multiple gestures available,
            // we use a loop to read in all of the gestures. this is important to make sure the 
            // TouchPanel's queue doesn't get backed up with old data
            while (TouchPanel.IsGestureAvailable)
            {
                // Read the next gesture from the queue
                GestureSample gesture = TouchPanel.ReadGesture();

                // Create a collidable rectangle to determine if we touched the controls
                Rectangle touch = new Rectangle((int)gesture.Position.X, (int)gesture.Position.Y, 20, 20);

                // We can use the type of gesture to determine our behavior
                switch (gesture.GestureType)
                {
                    case GestureType.Tap:
                        if (buttonStartStop.Intersects(touch))
                        {
                            pathFinder.IsSearching = !pathFinder.IsSearching;        
                        }
                        else if (buttonReset.Intersects(touch))
                        {
                            map.MapReload = true;
                        }
                        else if (buttonPathfinding.Intersects(touch))
                        {
                            pathFinder.NextSearchType();
                        }
                        else if (buttonNextMap.Intersects(touch))
                        {
                            map.CycleMap();
                        }
                        break;
                }
            }
        }
        #endregion
    }   
}
