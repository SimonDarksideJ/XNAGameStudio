#region File Description
//-----------------------------------------------------------------------------
// LoadingScreen.cs
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
using RobotGameData.Screen;
using RobotGameData.Resource;
using RobotGameData.GameObject;
using RobotGameData.Input;
using RobotGameData.Render;
#endregion

namespace RobotGame
{
    /// <summary>
    /// It reads in the loading image and displays on screen.  
    /// However, it does not remove from the screen.
    /// Until the loading of the files, which are necessary for the stage, is complete, 
    /// the loading image must be kept on screen.  After the loading is done, 
    /// the loading image must be gone.  Before the loading of the resources 
    /// for the stage, the loading image must be displayed first and then 
    /// comes the resource loading.
    /// </summary>
    public class LoadingScreen : GameScreen
    {
        #region Fields

        GameSceneNode refScene2DRoot = null;

        GameSprite2D spriteBG = null;
        Sprite2DObject loadingSprite = null;

        bool loadNextScreenGraphicsContent = false;
        float drawingWaitTime = 0.2f;

        GameSprite2D spriteLoadingText = null;
        Sprite2DObject spriteObjNowLoading = null;
        Sprite2DObject spriteObjPressToContinue = null;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoadingScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5f);
            TransitionOffTime = TimeSpan.FromSeconds(0.5f);

            this.spriteBG = new GameSprite2D();
            this.spriteLoadingText = new GameSprite2D();
        }

        /// <summary>
        /// initializes this screen. 
        /// </summary>
        public override void InitializeScreen()
        {
            //  creates a 2D layer.
            FrameworkCore.RenderContext.CreateScene2DLayer(1);
            this.refScene2DRoot = FrameworkCore.Scene2DLayers[0];

            this.refScene2DRoot.AddChild(this.spriteBG);
            this.refScene2DRoot.AddChild(this.spriteLoadingText);

            //  loading image.
            {
                loadingSprite = this.spriteBG.AddSprite(0, "Loading screen");

                //  Matching logo image size with screen size.
                loadingSprite.ScreenSize = new Vector2(FrameworkCore.ViewWidth,
                                                       FrameworkCore.ViewHeight);

                float aspect = (float)FrameworkCore.ViewWidth /
                               (float)FrameworkCore.ViewHeight;

                if (1.6f > aspect) // no wide screen
                {
                    int recalcWidth = (int)(aspect / 1.6f *
                                            this.spriteBG.TextureResource.Width);

                    loadingSprite.SourceRectangle = new Rectangle(
                        ((this.spriteBG.TextureResource.Width - recalcWidth) / 2) - 140,
                        0,
                        recalcWidth,
                        this.spriteBG.TextureResource.Height);
                }
            }

            //  loading text.
            {
                spriteObjNowLoading = 
                        this.spriteLoadingText.AddSprite(0, "Loading text");

                spriteObjNowLoading.SourceRectangle = new Rectangle(362, 64, 333, 57);
                spriteObjNowLoading.Visible = true;

                spriteObjPressToContinue = 
                        this.spriteLoadingText.AddSprite(1, "Press A to Continue");

                spriteObjPressToContinue.SourceRectangle = 
                    new Rectangle(170, 139, 653, 76);
                spriteObjPressToContinue.Visible = false;
            }

            //  Calculate all image size
            OnSize();
        }

        /// <summary>
        /// finalizes this screen. 
        /// </summary>
        public override void FinalizeScreen()
        {
            GameSound.StopAll();

            FrameworkCore.ScreenManager.FadeBackBufferToBlack(255);
        }

        /// <summary>
        /// loads graphics contents. 
        /// creates the loading image to be displayed on screen.
        /// </summary>
        public override void LoadContent()
        {
            //  loads image
            this.spriteBG.Create(1, "Textures/Loading");
            this.spriteLoadingText.Create(2, "Textures/Loading_Text");
        }

        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            FrameworkCore.RenderContext.ClearScene2DLayer(false);
        }

        /// <summary>
        /// calling when screen size has changed
        /// </summary>
        /// <param name="viewRect">new view area</param>
        public override void OnSize(Rectangle newRect)
        {
            base.OnSize(newRect);

            float scaleFactor = 0.8f;

            //  Scaling image size and positioning for screen resolution.
            float sizeScale = (float)newRect.Width / (float)ViewerWidth.Width1080;

            Vector2 posScale = new Vector2((float)newRect.Width /
                                            (float)ViewerWidth.Width1080,
                                            (float)newRect.Height /
                                            (float)ViewerHeight.Height1080);

            //  Resizing logo image
            loadingSprite.ScreenSize = new Vector2(newRect.Width, newRect.Height);

            spriteObjNowLoading.ScreenRectangle = new Rectangle(
                                    (int)(270 * posScale.X),
                                    (int)(140 * posScale.Y),
                                    (int)(333 * sizeScale),
                                    (int)(57 * sizeScale));

            spriteObjPressToContinue.ScreenRectangle = new Rectangle(
                                    (int)(742 * posScale.X),
                                    (int)(885 * posScale.Y),
                                    (int)(653 * sizeScale * scaleFactor),
                                    (int)(76 * sizeScale * scaleFactor));
        }

        #endregion

        #region Update & Draw

        /// <summary>
        /// moves to the registered next page when screen fades out.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            if (loadNextScreenGraphicsContent == false)
            {
                //  wait for drawing loading image.
                if (drawingWaitTime > 0.0f)
                {
                    drawingWaitTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    spriteObjNowLoading.Visible = true;
                    spriteObjPressToContinue.Visible = false;

                    FrameworkCore.ScreenManager.FadeBackBufferToBlack(0);
                }
                else
                {
                    drawingWaitTime = 0.0f;

                    //  loads every graphic content of the next screen.
                    this.NextScreen.LoadContent();

                    //  done.
                    loadNextScreenGraphicsContent = true;

                    spriteObjNowLoading.Visible = false;
                    spriteObjPressToContinue.Visible = true;
                }
            }
            else
            {
                //  Blink the point menu
                double time = gameTime.TotalGameTime.TotalSeconds;
                float blinkAlpha = Math.Abs((float)Math.Sin(time * 2.5f));

                if (blinkAlpha < 0.1f)
                    blinkAlpha = 0.1f;

                spriteObjPressToContinue.Alpha = (byte)(blinkAlpha * 255.0f);
            }

            //  If transition off, jump to next stage.
            if (ScreenState == ScreenState.Finished)
            {
                if (NextScreen == null)
                    throw new InvalidOperationException(
                                "Cannot jump to screen. Not set a next screen");

                FrameworkCore.ScreenManager.AddScreen(NextScreen, false);
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

        #endregion

        public override void HandleInput(GameTime gameTime)
        {
            InputComponent input =
                        FrameworkCore.InputManager.GetInputComponent(PlayerIndex.One);
            
            if (loadNextScreenGraphicsContent )
            {
                //  Press A to Continue.
                if (input.IsStrokeControlPad(ControlPad.A) ||
                    input.IsStrokeControlPad(ControlPad.Start) ||
                    input.IsStrokeKey(Keys.Space) ||
                    (!input.IsPressKey(Keys.LeftAlt) && 
                        !input.IsPressKey(Keys.RightAlt) &&
                    input.IsStrokeKey(Keys.Enter)))
                {
                    ExitScreen();
                }
            }
        }
    }
}
