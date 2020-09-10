#region File Description
//-----------------------------------------------------------------------------
// VersusReadyScreen.cs
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
    /// It sets up the players’ robots for versus mode and 
    /// the kill point for the victory.
    /// After setting up is done, it moves to versus game screen.  
    /// It accepts the input from the two players.
    /// </summary>
    public class VersusReadyScreen : GameMenuScreen
    {
        #region Fields

        const int killPoint = 3;    //  kill point in versus game

        int[] focusIndex = new int[2];

        TimeSpan activeElapsedTime = TimeSpan.Zero;

        GameSceneNode refScene2DRoot = null;

        GameSprite2D spriteBG = null;
        GameSprite2D spriteSelect = null;
        GameSprite2D spriteTextImage = null;

        Sprite2DObject spriteObjBG = null;
        Sprite2DObject spriteObjVersus = null;
        Sprite2DObject[] spriteObjSelectMech = null;    //  four robots
        Sprite2DObject[] spriteObjSelectCursor = null;  //  two players cursors

        //  Robot Image positions of the player mech
        Vector2[] cursorScreenPosition = new Vector2[4]
        {
            new Vector2(246, 485),      //  Grund
            new Vector2(700, 485),      //  Mark
            new Vector2(1142, 485),     //  Kiev
            new Vector2(1586, 485),     //  Yager
        };

        const int selectPlayerWidth = 384;
        const int selectPlayerHeight = 382;
        const int playerGrundPosX = 100;
        const int playerGrundPosY = 315;
        const int playerMarkPosX = 545;
        const int playerMarkPosY = 315;
        const int playerKievPosX = 990;
        const int playerKievPosY = 315;
        const int playerYagerPosX = 1435;
        const int playerYagerPosY = 315;
        const int imageVSPosX = 860;
        const int imageVSPosY = 100;
        const int imageVSWidth = 176;
        const int imageVSHeight = 102;
        const int image1PWidth = 100;
        const int image1PHeight = 60;

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public VersusReadyScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5f);
            TransitionOffTime = TimeSpan.FromSeconds(1.0f);

            this.spriteBG = new GameSprite2D();
            this.spriteSelect = new GameSprite2D();
            this.spriteTextImage = new GameSprite2D();
            spriteObjSelectCursor = new Sprite2DObject[2];
        }

        /// <summary>
        /// initializes this screen. 
        /// </summary>
        public override void InitializeScreen()
        {
            this.activeElapsedTime = TimeSpan.Zero;

            //  Create a 2D layer
            FrameworkCore.RenderContext.CreateScene2DLayer(1);
            this.refScene2DRoot = FrameworkCore.Scene2DLayers[0];

            //  Play a title music
            GameSound.Play(SoundTrack.ReadyRoom);

            SetHorizontalEntryIndex(0, 0);
            SetHorizontalEntryIndex(1, 1);

            focusIndex[0] = 0;      //  default set
            focusIndex[1] = 1;      //  default set
            
            this.refScene2DRoot.AddChild(this.spriteBG);
            spriteObjBG = this.spriteBG.AddSprite(0, "Versus bg image");
                        
            this.refScene2DRoot.AddChild(this.spriteSelect);
            spriteObjSelectMech = new Sprite2DObject[4];

            //  Create the Grund image
            spriteObjSelectMech[0] = this.spriteSelect.AddSprite(0, "Grund");

            //  Create the Mark image
            spriteObjSelectMech[1] = this.spriteSelect.AddSprite(1, "Mark");

            //  Create the Kiev image
            spriteObjSelectMech[2] = this.spriteSelect.AddSprite(2, "Kiev");

            //  Create the Yager image
            spriteObjSelectMech[3] = this.spriteSelect.AddSprite(3, "Yager");

            //  Entry select player image
            AddMenuEntry(spriteObjSelectMech[0]);
            AddMenuEntry(spriteObjSelectMech[1]);
            AddMenuEntry(spriteObjSelectMech[2]);
            AddMenuEntry(spriteObjSelectMech[3]);
                        
            this.refScene2DRoot.AddChild(this.spriteTextImage);

            //  Create the VS image
            spriteObjVersus = this.spriteTextImage.AddSprite(0, "VS text");

            //  1P image
            spriteObjSelectCursor[0] = this.spriteTextImage.AddSprite(1, "1P");

            //  2P image
            spriteObjSelectCursor[1] = this.spriteTextImage.AddSprite(2, "2P");

            //  Calculate all image size
            OnSize();

            //  Play a open menu sound
            GameSound.Play(SoundTrack.MenuOpen);
        }

        /// <summary>
        /// finalizes this screen. 
        /// </summary>
        public override void FinalizeScreen()
        {
            FrameworkCore.RenderContext.ClearScene3DRoot(false);
            FrameworkCore.RenderContext.ClearScene2DLayer(false);

            GameSound.StopAll();

            for (int i = 0; i < this.GameScreenManager.ScreenInput.Length; i++)
                this.GameScreenManager.ScreenInput[i].Reset();
        }

        /// <summary>
        /// loads graphics contents. 
        /// loads all the necessary images for the menu.
        /// </summary>
        public override void LoadContent()
        {
            //  Load the title image
            this.spriteBG.Create(1, "Textures/VS_Back");

            //  Load the select image
            this.spriteSelect.Create(4, "Textures/VS_Select");

            //  Load the select image
            this.spriteTextImage.Create(3, "Textures/VS_Text");                    
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
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
        /// <param name="verticalEntryIndex">vertical index of selected entry</param>
        /// <param name="horizontalEntryIndex">horizontal index of selected entry</param>
        public override void OnSelectedEntry(int inputIndex, 
                                             int verticalEntryIndex, 
                                             int horizontalEntryIndex)
        {
            //  It prevents more than one input.
            if (inputIndex != 0) return;

            if (ScreenState == ScreenState.Active)
            {
                VersusGameInfo versusInfo = new VersusGameInfo();

                versusInfo.playerSpec = new string[2];

                for (int i = 0; i < 2; i++)
                {
                    switch (focusIndex[i])
                    {
                        case 0:     //  Grund
                            {
                                versusInfo.playerSpec[i] = 
                                    "Data/Players/VersusGrund.spec";
                            }
                            break;
                        case 1:     //  Mark
                            {
                                versusInfo.playerSpec[i] = 
                                    "Data/Players/VersusMark.spec";
                            }
                            break;
                        case 2:     //  Kiev
                            {
                                versusInfo.playerSpec[i] =
                                    "Data/Players/VersusKiev.spec";
                            }
                            break;
                        case 3:     //  Yager
                            {
                                versusInfo.playerSpec[i] = 
                                    "Data/Players/VersusYager.spec";
                            }
                            break;
                    }
                }

                //  Set to kill point
                versusInfo.killPoint = killPoint;

                //  Set to versus information
                RobotGameGame.VersusGameInfo = versusInfo;

                //  Play a select sound
                GameSound.Play(SoundTrack.MenuClose);

                //  versus game start!!
                NextScreen = new LoadingScreen();
                NextScreen.NextScreen = new VersusStageScreen();

                TransitionOffTime = TimeSpan.FromSeconds(1.0f);
                ExitScreen();
            }
        }

        /// <summary>
        /// gets automatically called when the entry menu gets focused.
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
            if (inputIndex != 0 && inputIndex != 1) return;

            //  focused player mech
            {
                int focusAdd = 1;
                if (focusIndex[inputIndex] > horizontalEntryIndex)
                    focusAdd = -1;

                int horizontalFocus = horizontalEntryIndex;

                //  first, check out of range
                if (horizontalFocus >= MenuEntries.Count)
                {
                    horizontalFocus = 0;

                    SetHorizontalEntryIndex(inputIndex, horizontalFocus);
                }
                else if (horizontalFocus < 0)
                {
                    horizontalFocus = MenuEntries.Count - 1;

                    SetHorizontalEntryIndex(inputIndex, horizontalFocus);
                }

                //  Cannot be focus same selection
                if (inputIndex == 0 && horizontalFocus == focusIndex[1])
                {
                    horizontalFocus = focusIndex[1] + focusAdd;

                    SetHorizontalEntryIndex(inputIndex, horizontalFocus);
                }
                else if (inputIndex == 1 && horizontalFocus == focusIndex[0])
                {
                    horizontalFocus = focusIndex[0] + focusAdd;

                    SetHorizontalEntryIndex(inputIndex, horizontalFocus);
                }

                //  second, check out of range
                if (horizontalFocus >= MenuEntries.Count)
                {
                    horizontalFocus = 0;

                    SetHorizontalEntryIndex(inputIndex, horizontalFocus);
                }
                else if (horizontalFocus < 0)
                {
                    horizontalFocus = MenuEntries.Count - 1;

                    SetHorizontalEntryIndex(inputIndex, horizontalFocus);
                }

                focusIndex[inputIndex] = horizontalFocus;

                //  Scaling image size and positioning for screen resolution
                Vector2 scale = new Vector2((float)FrameworkCore.ViewWidth /
                                            (float)ViewerWidth.Width1080,
                                            (float)FrameworkCore.ViewHeight /
                                            (float)ViewerHeight.Height1080);

                int selectIndex = this.focusIndex[inputIndex];

                spriteObjSelectCursor[inputIndex].ScreenPosition =
                                    this.cursorScreenPosition[selectIndex] * scale;
            }            

            //  Play the focusing sound
            GameSound.Play(SoundTrack.MenuFocus);
        }

        /// <summary>
        /// calling when cancel key pressed.
        /// </summary>
        public override void OnCancel(int inputIndex)
        {
            //  It prevents more than one input.
            if (inputIndex != 0) return;

            NextScreen = new MainMenuScreen();
            TransitionOffTime = TimeSpan.FromSeconds(1.0f);
            ExitScreen();

            //  Play the select sound
            GameSound.Play(SoundTrack.MenuClose);
        }

        /// <summary>
        /// calling when exit key pressed.
        /// </summary>
        public override void OnExit(int inputIndex)
        {
            MessageBoxScreen messageBox = 
                new MessageBoxScreen("Are you sure you want to exit?");

            //  Register message box handle method
            messageBox.Accepted += ReturnToTitleAccepted;

            GameScreenManager.AddScreen(messageBox, true);


            //  Play the open menu sound
            GameSound.Play(SoundTrack.MenuClose);
        }

        /// <summary>
        /// returns to main menu.
        /// </summary>
        void ReturnToTitleAccepted(object sender, EventArgs e)
        {
            //  Accepeted to message box menu
            NextScreen = new MainMenuScreen();

            TransitionOffTime = TimeSpan.FromSeconds(1.0f);
            ExitScreen();

            //  Play the select sound
            GameSound.Play(SoundTrack.MenuClose);
        }

        /// <summary>
        /// blinks the kill point texts.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="verticalEntryIndex"></param>
        /// <param name="horizontalEntryIndex"></param>
        public override void OnUpdateEntry(GameTime gameTime, 
                                            int[] verticalEntryIndex, 
                                            int[] horizontalEntryIndex)
        {
        }

        /// <summary>
        /// calling when screen size has changed.
        /// </summary>
        /// <param name="viewRect">new view area</param>
        public override void OnSize(Rectangle newRect)
        {
            //  Scaling image size and positioning for screen resolution
            Vector2 scale = new Vector2((float)FrameworkCore.ViewWidth /
                                        (float)ViewerWidth.Width1080,
                                        (float)FrameworkCore.ViewHeight /
                                        (float)ViewerHeight.Height1080);

            //  Resizing the title image
            spriteObjBG.ScreenSize = new Vector2(FrameworkCore.ViewWidth,
                                                 FrameworkCore.ViewHeight);

            spriteObjBG.SourceRectangle = new Rectangle(0,0,
                                            this.spriteBG.TextureResource.Width,
                                            this.spriteBG.TextureResource.Height);

            int selectPlayerScaledWidth = (int)((float)selectPlayerWidth * scale.X);
            int selectPlayerScaledHeight = (int)((float)selectPlayerHeight * scale.Y);

            //  Resizing the Grund image
            spriteObjSelectMech[0].ScreenRectangle = new Rectangle(
                                        (int)((float)playerGrundPosX * scale.X),
                                        (int)((float)playerGrundPosY * scale.Y),
                                        selectPlayerScaledWidth,
                                        selectPlayerScaledHeight);

            spriteObjSelectMech[0].SourceRectangle = new Rectangle(
                                        99, 580,
                                        selectPlayerWidth, selectPlayerHeight);

            //  Resizing the Mark image
            spriteObjSelectMech[1].ScreenRectangle = new Rectangle(
                                        (int)((float)playerMarkPosX * scale.X),
                                        (int)((float)playerMarkPosY * scale.Y),
                                        selectPlayerScaledWidth,
                                        selectPlayerScaledHeight);

            spriteObjSelectMech[1].SourceRectangle = new Rectangle(
                                        1435, 580,
                                        selectPlayerWidth, selectPlayerHeight);

            //  Resizing the Kiev image
            spriteObjSelectMech[2].ScreenRectangle = new Rectangle(
                                        (int)((float)playerKievPosX * scale.X),
                                        (int)((float)playerKievPosY * scale.Y),
                                        selectPlayerScaledWidth,
                                        selectPlayerScaledHeight);

            spriteObjSelectMech[2].SourceRectangle = new Rectangle(
                                        990, 580,
                                        selectPlayerWidth, selectPlayerHeight);

            //  Resizing the Yager image 
            spriteObjSelectMech[3].ScreenRectangle = new Rectangle(
                                        (int)((float)playerYagerPosX * scale.X),
                                        (int)((float)playerYagerPosY * scale.Y),
                                        selectPlayerScaledWidth,
                                        selectPlayerScaledHeight);

            spriteObjSelectMech[3].SourceRectangle = new Rectangle(
                                        545, 580,
                                        selectPlayerWidth, selectPlayerHeight);

            //  Resizing the VS image
            spriteObjVersus.ScreenRectangle = new Rectangle(
                                        (int)((float)imageVSPosX * scale.X),
                                        (int)((float)imageVSPosY * scale.Y),
                                        (int)((float)imageVSWidth * scale.X),
                                        (int)((float)imageVSHeight * scale.Y));

            spriteObjVersus.SourceRectangle = new Rectangle(262, 258,
                                                    imageVSWidth, imageVSHeight);

            //  Resizing 1P image
            {
                int screenX = (int)this.cursorScreenPosition[focusIndex[0]].X;
                int screenY = (int)this.cursorScreenPosition[focusIndex[0]].Y;
                int scaledX = (int)((float)screenX * scale.X);
                int scaledY = (int)((float)screenY * scale.Y);

                int scaledWidth = (int)((float)image1PWidth * scale.X);
                int scaledHeight = (int)((float)image1PHeight * scale.Y);

                spriteObjSelectCursor[0].ScreenRectangle = new Rectangle(
                                        scaledX, scaledY,
                                        scaledWidth, scaledHeight);

                spriteObjSelectCursor[0].SourceRectangle = new Rectangle(
                                        82, 560,
                                        image1PWidth, image1PHeight);

                //  Resizing 2P image
                screenX = (int)this.cursorScreenPosition[focusIndex[1]].X;
                screenY = (int)this.cursorScreenPosition[focusIndex[1]].Y;
                scaledX = (int)((float)screenX * scale.X);
                scaledY = (int)((float)screenY * scale.Y);

                spriteObjSelectCursor[1].ScreenRectangle = new Rectangle(
                                        scaledX, scaledY,
                                        scaledWidth, scaledHeight);

                spriteObjSelectCursor[1].SourceRectangle = new Rectangle(
                                        532, 560,
                                        image1PWidth, image1PHeight);
            }
        }
    }
}
