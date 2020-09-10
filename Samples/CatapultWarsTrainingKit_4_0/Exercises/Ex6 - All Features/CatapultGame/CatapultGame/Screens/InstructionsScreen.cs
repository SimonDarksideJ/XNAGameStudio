#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GameStateManagement;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace CatapultGame
{
    class InstructionsScreen : GameScreen
    {
        #region Fields
        Texture2D background;
        SpriteFont font;
        bool isLoading;
        bool isTwoHumanPlayers;
        GameplayScreen gameplayScreen;
        System.Threading.Thread thread;
        #endregion

        #region Initialization
        public InstructionsScreen(bool twoHumans)
        {
            EnabledGestures = GestureType.Tap;

            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            isTwoHumanPlayers = twoHumans;
        }
        #endregion

        #region Loading
        public override void LoadContent()
        {
            background = Load<Texture2D>("Textures/Backgrounds/instructions");
            font = Load<SpriteFont>("Fonts/MenuFont");
        }
        #endregion

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // If additional thread is running, skip
            if (null != thread)
            {
                // If additional thread finished loading and the screen is not exiting
                if (thread.ThreadState ==
                    System.Threading.ThreadState.Stopped && !IsExiting)
                {
                    isLoading = false;

                    // Exit the screen and show the gameplay screen 
                    // with pre-loaded assets
                    ExitScreen();
                    ScreenManager.AddScreen(gameplayScreen, null);
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        #region Handle input
        public override void HandleInput(InputState input)
        {
            if (isLoading == true)
            {
                base.HandleInput(input);
                return;
            }

            foreach (var gesture in input.Gestures)
            {
                if (gesture.GestureType == GestureType.Tap)
                {
                    // Create a new instance of the gameplay screen
                    gameplayScreen = new GameplayScreen(isTwoHumanPlayers);
                    gameplayScreen.ScreenManager = ScreenManager;

                    // Start loading the resources in additional thread
                    thread = new System.Threading.Thread(
                        new System.Threading.ThreadStart(gameplayScreen.LoadAssets));
                    isLoading = true;
                    thread.Start();
                }
            }

            base.HandleInput(input);
        }
        #endregion

        #region Render
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw Background
            spriteBatch.Draw(background, new Vector2(0, 0),
                 new Color(255, 255, 255, TransitionAlpha));

            // If loading gameplay screen resource in the 
            // background show "Loading..." text
            if (isLoading)
            {
                string text = "Loading...";
                Vector2 size = font.MeasureString(text);
                Vector2 position = new Vector2(
                    (ScreenManager.GraphicsDevice.Viewport.Width - size.X) / 2, 
                    (ScreenManager.GraphicsDevice.Viewport.Height - size.Y) / 2);
                spriteBatch.DrawString(font, text, position, Color.Black);
                spriteBatch.DrawString(font, text, position - new Vector2(-4, 4), new Color(255f, 150f, 0f));

            }

            spriteBatch.End();
        }
        #endregion
    }
}
