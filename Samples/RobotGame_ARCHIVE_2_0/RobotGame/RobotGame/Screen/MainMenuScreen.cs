#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
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
using Microsoft.Xna.Framework.Input;
using RobotGameData;
using RobotGameData.Render;
using RobotGameData.Screen;
using RobotGameData.Resource;
using RobotGameData.GameObject;
using RobotGameData.Input;
#endregion

namespace RobotGame
{
    /// <summary>
    /// It provides START, VERSUS, and EXIT buttons to the menu screen.  
    /// When one of them gets selected, the game moves to the related stage screen.  
    /// You can also exit from the program at this menu screen.
    /// </summary>
    public class MainMenuScreen : GameMenuScreen
    {
        #region Fields

        TimeSpan activeElapsedTime = TimeSpan.Zero;

        int oldEntryIndex = 0;

        GameSceneNode refScene2DRoot = null;
        GameSprite2D spriteMain = null;
        GameSprite2D spriteButton = null;

        Sprite2DObject spriteObjMain = null;
        Sprite2DObject spriteObjStartButton = null;
        Sprite2DObject spriteObjVersusButton = null;
        Sprite2DObject spriteObjExitButton = null;

        const int buttonScreenWidth = 274;
        const int buttonScreenHeight = 94;
        const int startButtonScreenX = 1429;
        const int startButtonScreenY = 526;

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainMenuScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5f);
            TransitionOffTime = TimeSpan.FromSeconds(1.0f);

            this.spriteMain = new GameSprite2D();
            this.spriteButton = new GameSprite2D();
        }

        /// <summary>
        /// initializes this screen. 
        /// play a BGM sound.
        /// </summary>
        public override void InitializeScreen()
        {            
            this.activeElapsedTime = TimeSpan.Zero;

            //  creates a 2D layer.
            FrameworkCore.RenderContext.CreateScene2DLayer(1);
            this.refScene2DRoot = FrameworkCore.Scene2DLayers[0];
            
            this.refScene2DRoot.AddChild(this.spriteMain);
            spriteObjMain = this.spriteMain.AddSprite(0, "MainTitle");
            
            this.refScene2DRoot.AddChild(this.spriteButton);

            //  Initialize start button
            spriteObjStartButton = this.spriteButton.AddSprite(0, "Start Button");
            AddMenuEntry(spriteObjStartButton); //  Entry a start button

            //  Initialize versus button
            spriteObjVersusButton = this.spriteButton.AddSprite(1, "Versus Button");
            AddMenuEntry(spriteObjVersusButton);//  Entry a versus button

            //  Initialize exit button
            spriteObjExitButton = this.spriteButton.AddSprite(2, "Exit Button");
            AddMenuEntry(spriteObjExitButton);  //  Entry a exit button

            //  calculates all image size.
            OnSize();

            //  play the title music.
            GameSound.Play(SoundTrack.MainTitle);

            //  play an open menu sound.
            GameSound.Play(SoundTrack.MenuOpen);
        }

        /// <summary>
        /// finalizes this screen. 
        /// clear all scene nodes.
        /// </summary>
        public override void FinalizeScreen()
        {
            FrameworkCore.RenderContext.ClearScene3DRoot(false);
            FrameworkCore.RenderContext.ClearScene2DLayer(false);

            GameSound.StopAll();
        }

        /// <summary>
        /// loads graphics contents.
        /// loads all images about the main menu.
        /// </summary>
        public override void LoadContent()
        {
            //  loads a title image.
            {
                this.spriteMain.Create(1, "Textures/MainTitle");
            }

            //  loads button images.
            {
                this.spriteButton.Create(3, "Textures/MainMenuButton");
            }
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            //  unloads a logo image.
            this.refScene2DRoot.RemoveAllChild(true);
        }

        /// <summary>
        /// moves to the registered next stage when screen fades out.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            if (IsActive)
                this.activeElapsedTime += gameTime.ElapsedGameTime;

            //  If transition off, jump to first stage 
            if (ScreenState == ScreenState.Finished)
            {
                FrameworkCore.ScreenManager.AddScreen(NextScreen, true);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                FrameworkCore.ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
            else if (TransitionPosition <= 0)
                FrameworkCore.ScreenManager.FadeBackBufferToBlack(0);
        }

        public override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);
        }

        /// <summary>
        /// Moves to selected menu.
        /// </summary>
        /// <param name="inputIndex">an index of the input</param>
        /// <param name="verticalEntryIndex">
        /// a vertical index of selected entry
        /// </param>
        /// <param name="horizontalEntryIndex">
        /// a horizontal index of selected entry
        /// </param>
        public override void OnSelectedEntry(int inputIndex, 
                                             int verticalEntryIndex, 
                                             int horizontalEntryIndex)
        {
            //  It prevents more than one input.
            if (inputIndex != 0) return;

            if (ScreenState == ScreenState.Active)
            {
                //  If selected a start button, jump to the first stage screen
                if (verticalEntryIndex == 0)
                {
                    NextScreen = new LoadingScreen();
                    NextScreen.NextScreen = new FirstStageScreen();

                    TransitionOffTime = TimeSpan.FromSeconds(1.0f);
                    ExitScreen();
                }
                //  If selected a versus button, jump to the versus ready screen
                else if (verticalEntryIndex == 1)
                {
                    NextScreen = new VersusReadyScreen();

                    TransitionOffTime = TimeSpan.FromSeconds(1.0f);
                    ExitScreen();
                }
                //  If selected a exit button, exit the program
                else if (verticalEntryIndex == 2)
                {
                    // Allows the default game to exit on Xbox 360 and Windows
                    MessageBoxScreen messageBox = 
                        new MessageBoxScreen("Are you sure you want to exit?");

                    //  Register message box handle method
                    messageBox.Accepted += RobotGameGame.ExitAccepted;

                    GameScreenManager.AddScreen(messageBox, true);
                }

                //  Play the select sound
                GameSound.Play(SoundTrack.MenuClose);
            }
        }

        /// <summary>
        /// is called automatically when entry menu gets focused.
        /// </summary>
        /// <param name="inputIndex">an index of the input</param>
        /// <param name="verticalEntryIndex">
        /// a vertical index of focused entry
        /// </param>
        /// <param name="horizontalEntryIndex">
        /// a horizontal index of focused entry
        /// </param>
        public override void OnFocusEntry(int inputIndex,
                                          int verticalEntryIndex,
                                          int horizontalEntryIndex)
        {
            if (verticalEntryIndex >= MenuEntries.Count)
            {
                int value = MenuEntries.Count - 1;

                for (int i = 0; i < InputCount; i++)
                    SetVerticalEntryIndex(i, value);

                verticalEntryIndex = value;
            }
            else if (verticalEntryIndex < 0)
            {
                for (int i = 0; i < InputCount; i++)
                    SetVerticalEntryIndex(i, 0);

                verticalEntryIndex = 0;
            }

            //  Play the focusing sound
            if (oldEntryIndex != verticalEntryIndex)
            {
                for (int i = 0; i < InputCount; i++)
                    SetVerticalEntryIndex(i, verticalEntryIndex);

                GameSound.Play(SoundTrack.MenuFocus);

                oldEntryIndex = verticalEntryIndex;
            }
        }

        /// <summary>
        /// is called when the exit button is pressed.
        /// </summary>
        public override void OnExit(int inputIndex)
        {
            if (!this.IsActive) return;

            MessageBoxScreen messageBox = 
                new MessageBoxScreen("Are you sure you want to exit?");

            //  Set to message box handle method
            messageBox.Accepted += RobotGameGame.ExitAccepted;

            GameScreenManager.AddScreen(messageBox, true);

            //  Play the select sound
            GameSound.Play(SoundTrack.MenuClose);
        }

        /// <summary>
        /// always gets called when the menu gets updated.
        /// </summary>
        /// <param name="gameTime">game time</param>
        /// <param name="verticalEntryIndex">
        /// a vertical index of focused entry
        /// </param>
        /// <param name="horizontalEntryIndex">
        /// a horizontal index of focused entry
        /// </param>
        public override void OnUpdateEntry(GameTime gameTime, 
                                          int[] verticalEntryIndex,
                                          int[] horizontalEntryIndex)
        {
            if (IsActive)
            {
                Sprite2DObject selectedButton = MenuEntries[verticalEntryIndex[0]];

                //  Clear alpha value of the all entry buttons
                spriteObjStartButton.Alpha = 85;
                spriteObjVersusButton.Alpha = 85;
                spriteObjExitButton.Alpha = 85;

                // set the selected alpha
                selectedButton.Alpha = 255;
            }
        }

        /// <summary>
        /// calling when this screen size has changed
        /// changes the size of every image of the main menu.
        /// </summary>
        /// <param name="viewRect">new view area</param>
        public override void OnSize(Rectangle newRect)
        {
            base.OnSize(newRect);

            if (spriteObjMain != null)
            {
                float aspect = (float)newRect.Width / (float)newRect.Height;

                //  Resizing logo image
                if (1.6f > aspect) 
                {
                    // no wide screen
                    int recalcWidth = (int)((aspect / 1.6f) *
                                       this.spriteMain.TextureResource.Width);

                    spriteObjMain.SourceRectangle = new Rectangle(
                        (this.spriteMain.TextureResource.Width - recalcWidth) - 80,
                        0,
                        recalcWidth,
                        this.spriteMain.TextureResource.Height);

                    spriteObjMain.ScreenSize = 
                        new Vector2(newRect.Width, newRect.Height);
                }
                else
                {
                    spriteObjMain.SourceRectangle = new Rectangle(
                                                0, 0, 
                                                this.spriteMain.TextureResource.Width, 
                                                this.spriteMain.TextureResource.Height);

                    spriteObjMain.ScreenSize = 
                        new Vector2(newRect.Width, newRect.Height);
                }
            }

            if (spriteObjStartButton != null)
            {
                //  Resizing logo image
                spriteObjStartButton.ScreenSize = 
                                    new Vector2(newRect.Width, newRect.Height);

                Vector2 scale = new Vector2((float)FrameworkCore.ViewWidth /
                                            (float)ViewerWidth.Width1080,
                                            (float)FrameworkCore.ViewHeight /
                                            (float)ViewerHeight.Height1080);

                int buttonScaledWidth = (int)((float)buttonScreenWidth * scale.X);
                int buttonScaledHeight = (int)((float)buttonScreenHeight * scale.Y);

                //  Resizing start button
                int startButtonScaledX = (int)((float)startButtonScreenX * scale.X);
                int startButtonScaledY = (int)((float)startButtonScreenY * scale.Y);

                spriteObjStartButton.ScreenRectangle = new Rectangle(
                                                                startButtonScaledX,
                                                                startButtonScaledY,
                                                                buttonScaledWidth,
                                                                buttonScaledHeight);

                spriteObjStartButton.SourceRectangle = new Rectangle(45, 33,
                                                                buttonScreenWidth, 
                                                                buttonScreenHeight);

                //  Resizing versus button
                int versusButtonScreenX = startButtonScreenX;

                int versusButtonScreenY = startButtonScreenY + 
                                            buttonScreenHeight + 32;

                int versusButtonScaledX = 
                            (int)((float)versusButtonScreenX * scale.X);

                int versusButtonScaledY = 
                            (int)((float)versusButtonScreenY * scale.Y);

                spriteObjVersusButton.ScreenRectangle = new Rectangle(
                                                            versusButtonScaledX,
                                                            versusButtonScaledY,
                                                            buttonScaledWidth,
                                                            buttonScaledHeight);

                spriteObjVersusButton.SourceRectangle = new Rectangle(
                                                            390, 33,
                                                            buttonScreenWidth, 
                                                            buttonScreenHeight);

                //  Resizing exit button
                int exitButtonScreenX = startButtonScreenX;

                int exitButtonScreenY =
                            versusButtonScreenY + buttonScreenHeight + 32;

                int exitButtonScaledX =
                            (int)((float)exitButtonScreenX * scale.X);

                int exitButtonScaledY =
                            (int)((float)exitButtonScreenY * scale.Y);

                spriteObjExitButton.ScreenRectangle = new Rectangle(
                                                            exitButtonScaledX,
                                                            exitButtonScaledY,
                                                            buttonScaledWidth,
                                                            buttonScaledHeight);

                spriteObjExitButton.SourceRectangle = new Rectangle(
                                                            700, 33,
                                                            buttonScreenWidth, 
                                                            buttonScreenHeight);


            }
        }
    }
}
