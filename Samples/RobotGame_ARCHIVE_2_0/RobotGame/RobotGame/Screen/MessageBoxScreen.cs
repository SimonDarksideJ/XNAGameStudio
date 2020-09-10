#region File Description
//-----------------------------------------------------------------------------
// MessageBoxScreen.cs
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
using RobotGameData;
using RobotGameData.Screen;
using RobotGameData.Resource;
using RobotGameData.GameObject;
using RobotGameData.Text;
using RobotGameData.Render;
#endregion

namespace RobotGame
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    class MessageBoxScreen : GameScreen
    {
        #region Fields

        string message = null;
        string controls = null;
        GameSceneNode refScene2DTopRoot = null;
        GameSprite2D spriteBox = null;
        Sprite2DObject spriteObjBox = null;
        SpriteFont messageFont = null;
        TextItem textMessageItem = null;
        TextItem textControlsItem = null;
        float inputTermTime = 0.0f;

        // The background includes a border somewhat larger than the text itself.
        Vector2 outlinePad = new Vector2(60f, 60f);

        #endregion

        #region Events

        public event EventHandler<EventArgs> Accepted;
        public event EventHandler<EventArgs> Cancelled;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageBoxScreen(string message)
        {        
            IsPopup = true;

            this.message = message;

            TransitionOnTime = TimeSpan.FromSeconds(0.2f);
            TransitionOffTime = TimeSpan.FromSeconds(0.2f);

            //  Create messagebox font
            this.messageFont = FrameworkCore.FontManager.CreateFont("MessageFont", 
                "Font/RobotGame_font");

            this.controls = "\n(A) button OK\n(B) button Cancel";
          
            this.refScene2DTopRoot = FrameworkCore.Scene2DTopLayer;
        }

        /// <summary>
        /// initializes this screen.
        /// </summary>
        public override void InitializeScreen()
        {
            inputTermTime = 0.0f;
        }

        /// <summary>
        /// finalizes this screen. 
        /// </summary>
        public override void FinalizeScreen() { }

        /// <summary>
        /// loads graphics contents.  This uses the shared ContentManager
        /// provided by the ScreenManager, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded instance.
        /// </summary>
        public override void LoadContent()
        {
            spriteBox = new GameSprite2D();
            spriteBox.Create(1, "Textures/Text_Window");

            refScene2DTopRoot.AddChild(spriteBox);
            spriteObjBox = spriteBox.AddSprite(0, "MessageBox frame");


            textMessageItem = 
                new TextItem(messageFont, message, 0, 0, new Color(136, 217, 224));
            FrameworkCore.TextManager.AddText(textMessageItem);

            textControlsItem = 
                new TextItem(messageFont, controls, 0, 0,Color.White);
            FrameworkCore.TextManager.AddText(textControlsItem);

            //  Resizing all images
            OnSize();
        }

        public override void UnloadContent()
        {
            this.refScene2DTopRoot.RemoveChild(this.spriteBox, false);
            if (FrameworkCore.TextManager != null)
            {
                FrameworkCore.TextManager.RemoveText(this.textControlsItem);
                FrameworkCore.TextManager.RemoveText(this.textMessageItem);
            }

            this.spriteBox = null;
            this.textMessageItem = null;
            this.textControlsItem = null;
        }


        #endregion

        #region Handle Input

        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(GameTime gameTime)
        {
            GameScreenInput input = FrameworkCore.ScreenManager.SingleInput;

            if (inputTermTime > 0.25f)
            {
                if (input.MenuSelect)
                {
                    // Raise the accepted event, then exit the message box.
                    if (Accepted != null)
                        Accepted(this, EventArgs.Empty);

                    ExitScreen();
                }
                else if (input.MenuCancel || input.MenuExit)
                {
                    // Raise the cancelled event, then exit the message box.
                    if (Cancelled != null)
                        Cancelled(this, EventArgs.Empty);

                    ExitScreen();
                }
            }
            else
            {
                inputTermTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }


        #endregion

        #region Draw

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, 
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Darken down any other screens that were drawn beneath the popup.
            FrameworkCore.ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            // Fade the popup alpha during transitions.
            spriteObjBox.Color = new Color(255, 255, 255, TransitionAlpha);

            Color messageColor = textMessageItem.Color;
            textMessageItem.Color = new Color(messageColor.R, messageColor.G, 
                messageColor.B, TransitionAlpha);

            Color controlsColor = textControlsItem.Color;
            textControlsItem.Color = new Color(controlsColor.R, controlsColor.G,
                controlsColor.B, TransitionAlpha);
        }

        #endregion

        /// <summary>
        /// calling when screen size has changed
        /// </summary>
        /// <param name="viewRect">new view area</param>
        public override void OnSize(Rectangle newRect)
        {
            Vector2 viewportSize = new Vector2(FrameworkCore.ViewWidth,
                                               FrameworkCore.ViewHeight);
            Vector2 scale = new Vector2((float)FrameworkCore.ViewWidth /
                                        (float)ViewerWidth.Width1080,
                                        (float)FrameworkCore.ViewHeight /
                                        (float)ViewerHeight.Height1080) * 1.3f;

            Vector2 messageSize =
                messageFont.MeasureString(textMessageItem.Text) * scale;
            Vector2 controlSize =
                messageFont.MeasureString(textControlsItem.Text) * scale;
            Vector2 totalTextSize = new Vector2(Math.Max(messageSize.X, controlSize.X),
                messageSize.Y + controlSize.Y);

            //  Message text
            textMessageItem.Position = viewportSize / 2 - totalTextSize / 2;
            textMessageItem.Font = messageFont;
            textMessageItem.Scale = scale.X;

            //  Message text
            textControlsItem.Position = new Vector2(textMessageItem.Position.X, 
                textMessageItem.Position.Y + messageSize.Y);
            textControlsItem.Font = messageFont;
            textControlsItem.Scale = scale.X;

            //  Message box frame
            Vector2 outlinePadScaled = outlinePad * scale;
            spriteObjBox.ScreenRectangle = new Rectangle(
                (int)(textMessageItem.Position.X - outlinePadScaled.X),
                (int)(textMessageItem.Position.Y - outlinePadScaled.Y),
                (int)(totalTextSize.X + (outlinePadScaled.X * 2)),
                (int)(totalTextSize.Y + (outlinePadScaled.Y * 2)));
        }

    }
}
