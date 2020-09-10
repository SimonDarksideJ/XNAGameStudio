#region File Description
//-----------------------------------------------------------------------------
// InstructionScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GameStateManagement;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;
using System.Text.RegularExpressions;
using YachtServices;


#endregion

namespace Yacht
{
    /// <summary>
    /// A screen which displays the game instructions.
    /// </summary>

    class InstructionScreen : GameScreen
    {
        #region Fields


        Texture2D background;
        SpriteFont font;
        bool isExit = false;
        bool screenExited = false;
        string name;
        bool askName;

        bool isInvalidName;
        string invalidName;


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new screen instance.
        /// </summary>
        /// <param name="askName">Whether or not to ask the player for his name.</param>

        public InstructionScreen(bool askName)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            EnabledGestures = GestureType.Tap;
            this.askName = askName;
        }


        #endregion

        #region Loading


        /// <summary>
        /// Load screen resources.
        /// </summary>
        public override void LoadContent()
        {
            background = Load<Texture2D>(@"Images\instruction");
            font = Load<SpriteFont>(@"Fonts\MenuFont");
        }


        #endregion

        #region Update and Render


        /// <summary>
        /// Advance to the gameplay screen on a tap.
        /// </summary>
        /// <param name="input">Player input information.</param>
        public override void HandleInput(InputState input)
        {
            if (input.IsPauseGame(null))
            {
                ExitScreen();

                ScreenManager.AddScreen(new MainMenuScreen(), null);
            }

            if (!isExit)
            {
                if (input.Gestures.Count > 0 &&
                    input.Gestures[0].GestureType == GestureType.Tap)
                {
                    if (askName)
                    {
                        Guide.BeginShowKeyboardInput(PlayerIndex.One, "Enter your name", String.Empty, "Player1",
                            EnterNameDialogEnded, null);
                    }
                    else
                    {
                        isExit = true;
                    }

                }
            }
        }

        /// <summary>
        /// Called once the player has selected a name for himself.
        /// </summary>
        /// <param name="result">Dialog result containing the text entered by the user.</param>
        private void EnterNameDialogEnded(IAsyncResult result)
        {
            name = Guide.EndShowKeyboardInput(result);

            if (name == null)
            {
                ExitScreen();
                ScreenManager.AddScreen(new MainMenuScreen(), null);
            }
            else if (StringUtility.IsNameValid(name))
            {
                isExit = true;
            }
            else
            {
                isInvalidName = true;
                invalidName = name;
            }
        }

        /// <summary>
        /// Updates the screen.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="otherScreenHasFocus">Whether another screen has the focus currently.</param>
        /// <param name="coveredByOtherScreen">Whether this screen is covered by another screen.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (isInvalidName)
            {
                isInvalidName = false;

                Guide.BeginShowKeyboardInput(PlayerIndex.One, "Enter your name", "The name is not valid.", invalidName,
                    EnterNameDialogEnded, null);

                return;
            }

            if (isExit && !screenExited)
            {
                // Move on to the gameplay screen
                foreach (GameScreen screen in ScreenManager.GetScreens())
                {
                    screen.ExitScreen();
                }


                ScreenManager.AddScreen(new GameplayScreen(name, GameTypes.Offline), null);

                screenExited = true;
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Render the screen.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw the background
            spriteBatch.Draw(background, ScreenManager.GraphicsDevice.Viewport.Bounds,
                 Color.White * TransitionAlpha);

            if (isExit)
            {
                Rectangle safeArea = ScreenManager.SafeArea;
                string text = "Loading...";
                Vector2 measure = font.MeasureString(text);
                Vector2 textPosition = new Vector2(safeArea.Center.X - measure.X / 2,
                    safeArea.Center.Y - measure.Y / 2);
                spriteBatch.DrawString(font, text, textPosition, Color.Black);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }


        #endregion
    }
}
