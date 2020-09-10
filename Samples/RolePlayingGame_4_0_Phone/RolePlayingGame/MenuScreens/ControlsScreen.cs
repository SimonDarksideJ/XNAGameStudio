#region File Description
//-----------------------------------------------------------------------------
// ControlsScreen.cs
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
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Displays the in-game controls to the user.
    /// </summary>
    /// <remarks>One possible extension would be to enable control remapping.</remarks>
    class ControlsScreen : GameScreen
    {
        #region Private Types


        /// <summary>
        /// Holds the GamePad control info to display
        /// </summary>
        private struct GamePadInfo
        {
            public string text;
            public Vector2 textPosition;
        }


        /// <summary>
        /// Holds the Keyboard control info to display
        /// </summary>
        private struct KeyboardInfo
        {
            public InputManager.ActionMap[] totalActionList;
            public int selectedIndex;
        }


        #endregion


        #region Graphics Data


        private Texture2D backgroundTexture;
        private Texture2D plankTexture;

        private Vector2 plankPosition;
        private Vector2 titlePosition;
        private Vector2 actionPosition;
        private Vector2 key1Position;
        private Vector2 key2Position;

        private Texture2D baseBorderTexture;
        private Vector2 baseBorderPosition = ScaledVector2.GetScaledVector(200, 570);

        private Texture2D scrollUpTexture;
        private Texture2D scrollDownTexture;
        private Vector2 scrollUpPosition = ScaledVector2.GetScaledVector(990, 235);
        private Vector2 scrollDownPosition = ScaledVector2.GetScaledVector(990, 490);

        private Texture2D rightTriggerButton;
        private Texture2D leftTriggerButton;
        private Vector2 rightTriggerPosition;
        private Vector2 leftTriggerPosition;

        private Texture2D controlPadTexture;
        private Vector2 controlPosition = ScaledVector2.GetScaledVector(450, 180);

        private Texture2D keyboardTexture;
        private Vector2 keyboardPosition = ScaledVector2.GetScaledVector(305, 185);

        private float chartLine1Position;
        private float chartLine2Position;
        private float chartLine3Position;
        private float chartLine4Position;

        private Texture2D backTexture;
        private readonly Vector2 backPosition = ScaledVector2.GetScaledVector(225, 610);


        #endregion


        #region Control Display Data


        private bool isShowControlPad;

        private GamePadInfo[] leftStrings = new GamePadInfo[7];
        private GamePadInfo[] rightStrings = new GamePadInfo[6];
        private KeyboardInfo keyboardInfo;

        private int startIndex = 0;
        private const int maxActionDisplay = 6;


        #endregion


        #region Initialization


        /// <summary>
        /// Creates a new ControlsScreen object.
        /// </summary>
        public ControlsScreen()
            : base()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);

            chartLine1Position = keyboardPosition.X + 30 * ScaledVector2.ScaleFactor;
            chartLine2Position = keyboardPosition.X + 340 * ScaledVector2.ScaleFactor;
            chartLine3Position = keyboardPosition.X + 510 * ScaledVector2.ScaleFactor;
            chartLine4Position = keyboardPosition.X + 670 * ScaledVector2.ScaleFactor;

            isShowControlPad = true;
        }



        /// <summary>
        /// Loads the graphics content required for this screen.
        /// </summary>
        public override void LoadContent()
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            keyboardInfo.totalActionList = InputManager.ActionMaps;
            keyboardInfo.selectedIndex = 0;

            const int leftStringsPosition = (int)(400);
            const int rightStringPosition = (int)(818);

            float height = Fonts.DescriptionFont.LineSpacing - 5;

            // Set the data for gamepad control to display
            leftStrings[0].text = "Page Left";
            leftStrings[0].textPosition = ScaledVector2.GetScaledVector(leftStringsPosition -
                Fonts.DescriptionFont.MeasureString(leftStrings[0].text).X, 170);

            leftStrings[1].text = "N/A";
            leftStrings[1].textPosition = ScaledVector2.GetScaledVector(leftStringsPosition -
                Fonts.DescriptionFont.MeasureString(leftStrings[1].text).X, 220);

            leftStrings[2].text = "Main Menu";
            leftStrings[2].textPosition = ScaledVector2.GetScaledVector(leftStringsPosition -
                Fonts.DescriptionFont.MeasureString(leftStrings[2].text).X, 290);

            leftStrings[3].text = "Exit Game";
            leftStrings[3].textPosition = ScaledVector2.GetScaledVector(leftStringsPosition -
                Fonts.DescriptionFont.MeasureString(leftStrings[3].text).X, 340);

            leftStrings[4].text = "Navigation";
            leftStrings[4].textPosition = ScaledVector2.GetScaledVector(leftStringsPosition -
                Fonts.DescriptionFont.MeasureString(leftStrings[4].text).X, 400);

            leftStrings[5].text = "Navigation";
            leftStrings[5].textPosition = ScaledVector2.GetScaledVector(leftStringsPosition -
                Fonts.DescriptionFont.MeasureString(leftStrings[5].text).X, 455);

            leftStrings[6].text = "N/A";
            leftStrings[6].textPosition = ScaledVector2.GetScaledVector(leftStringsPosition -
                Fonts.DescriptionFont.MeasureString(leftStrings[6].text).X, 510);


            rightStrings[0].text = "Page Right";
            rightStrings[0].textPosition = ScaledVector2.GetScaledVector(rightStringPosition, 170);

            rightStrings[1].text = "N/A";
            rightStrings[1].textPosition = ScaledVector2.GetScaledVector(rightStringPosition, 230);

            rightStrings[2].text = "Character Management";
            rightStrings[2].textPosition = ScaledVector2.GetScaledVector(rightStringPosition, 295);

            rightStrings[3].text = "Back";
            rightStrings[3].textPosition = ScaledVector2.GetScaledVector(rightStringPosition, 355);

            rightStrings[4].text = "OK";
            rightStrings[4].textPosition = ScaledVector2.GetScaledVector(rightStringPosition, 435);

            rightStrings[5].text = "Drop Gear";
            rightStrings[5].textPosition = ScaledVector2.GetScaledVector(rightStringPosition, 510);

            ContentManager content = ScreenManager.Game.Content;
            backgroundTexture =
                content.Load<Texture2D>(@"Textures\MainMenu\MainMenu");
            keyboardTexture =
                content.Load<Texture2D>(@"Textures\MainMenu\KeyboardBkgd");
            plankTexture =
                content.Load<Texture2D>(@"Textures\MainMenu\MainMenuPlank03");
            backTexture =
                content.Load<Texture2D>(@"Textures\Buttons\BButton");
            baseBorderTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\LineBorder");
            controlPadTexture =
                content.Load<Texture2D>(@"Textures\MainMenu\ControlJoystick");
            scrollUpTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\ScrollUp");
            scrollDownTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\ScrollDown");
            rightTriggerButton =
                content.Load<Texture2D>(@"Textures\Buttons\RightTriggerButton");
            leftTriggerButton =
                content.Load<Texture2D>(@"Textures\Buttons\LeftTriggerButton");

            plankPosition.X = (backgroundTexture.Width * ScaledVector2.DrawFactor) / 2 - 
                (plankTexture.Width * ScaledVector2.DrawFactor) / 2;
            plankPosition.Y = 60 * ScaledVector2.ScaleFactor;


            rightTriggerPosition.X = 900 * ScaledVector2.ScaleFactor;
            rightTriggerPosition.Y = 50 * ScaledVector2.ScaleFactor;

            leftTriggerPosition.X = 320 * ScaledVector2.ScaleFactor;
            leftTriggerPosition.Y = 50 * ScaledVector2.ScaleFactor;

            base.LoadContent();
        }


        #endregion


        #region Updating


        /// <summary>
        /// Handles user input.
        /// </summary>
        public override void HandleInput()
        {
            bool backClicked = false;
            if (InputManager.IsButtonClicked(new Rectangle
                 ((int)backPosition.X - backTexture.Width, 
                 (int)backPosition.Y ,
                 backTexture.Width * 6, backTexture.Height * 2)))
            {
                backClicked = true;
            }
            // exit the screen
            if (InputManager.IsActionTriggered(InputManager.Action.Back) || backClicked)
            {
                ExitScreen();
            }
            // toggle between keyboard and gamepad controls
            else if (InputManager.IsActionTriggered(InputManager.Action.PageLeft) ||
                InputManager.IsActionTriggered(InputManager.Action.PageRight))
            {
                isShowControlPad = !isShowControlPad;
            }
            // scroll through the keyboard controls
            if (isShowControlPad == false)
            {
                if (InputManager.IsActionTriggered(InputManager.Action.CursorDown))
                {
                    if (startIndex < keyboardInfo.totalActionList.Length -
                        maxActionDisplay)
                    {
                        startIndex++;
                        keyboardInfo.selectedIndex++;
                    }
                }
                if (InputManager.IsActionTriggered(InputManager.Action.CursorUp))
                {
                    if (startIndex > 0)
                    {
                        startIndex--;
                        keyboardInfo.selectedIndex--;
                    }
                }
            }
        }

        
        #endregion


        #region Drawing


        /// <summary>
        /// Draws the control screen
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Vector2 textPosition = Vector2.Zero;

            spriteBatch.Begin();

            // Draw the background texture
            spriteBatch.Draw(backgroundTexture, Vector2.Zero,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // Draw the back icon and text
            spriteBatch.Draw(backTexture, backPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            spriteBatch.DrawString(Fonts.ButtonNamesFont, "Back",
                new Vector2(backPosition.X + 55 * ScaledVector2.ScaleFactor, 
                    backPosition.Y + 5 * ScaledVector2.ScaleFactor), Color.White);

            // Draw the plank
            spriteBatch.Draw(plankTexture, plankPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // Draw the base border
            spriteBatch.Draw(baseBorderTexture, baseBorderPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // draw the control pad screen
            if (isShowControlPad)
            {
                spriteBatch.Draw(controlPadTexture, controlPosition,null,Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

                for (int i = 0; i < leftStrings.Length; i++)
                {
                    spriteBatch.DrawString(Fonts.DescriptionFont, leftStrings[i].text,
                        leftStrings[i].textPosition, Color.Black);
                }

                for (int i = 0; i < rightStrings.Length; i++)
                {
                    spriteBatch.DrawString(Fonts.DescriptionFont, rightStrings[i].text,
                        rightStrings[i].textPosition, Color.Black);
                }

                // Near left trigger
                spriteBatch.DrawString(Fonts.PlayerStatisticsFont, "Keyboard",
                    new Vector2(leftTriggerPosition.X + (leftTriggerButton.Width -
                    Fonts.PlayerStatisticsFont.MeasureString("Keyboard").X) / 2,
                    rightTriggerPosition.Y + 85 * ScaledVector2.ScaleFactor),
                    Color.Black);

                // Near right trigger
                spriteBatch.DrawString(Fonts.PlayerStatisticsFont, "Keyboard",
                    new Vector2(rightTriggerPosition.X + (rightTriggerButton.Width -
                    Fonts.PlayerStatisticsFont.MeasureString("Keyboard").X) / 2,
                    rightTriggerPosition.Y + 85 * ScaledVector2.ScaleFactor),
                    Color.Black);

                // Draw the title text
                titlePosition.X = plankPosition.X + (plankTexture.Width -
                    Fonts.HeaderFont.MeasureString("Gamepad").X) / 2;
                titlePosition.Y = plankPosition.Y + (plankTexture.Height -
                    Fonts.HeaderFont.MeasureString("Gamepad").Y) / 2;
                spriteBatch.DrawString(Fonts.HeaderFont, "Gamepad", titlePosition,
                    Fonts.TitleColor);
            }
            else // draws the keyboard screen
            {
                float spacing = 47 * ScaledVector2.ScaleFactor;
                string keyboardString;

                spriteBatch.Draw(keyboardTexture, keyboardPosition,null, Color.White,0f,
                    Vector2.Zero,ScaledVector2.DrawFactor,SpriteEffects.None,0f);
                for (int j = 0, i = startIndex; i < startIndex + maxActionDisplay;
                    i++, j++)
                {
                    keyboardString = InputManager.GetActionName((InputManager.Action)i);
                    textPosition.X = chartLine1Position + 
                        ((chartLine2Position - chartLine1Position) -
                        Fonts.DescriptionFont.MeasureString(keyboardString).X) / 2;
                    textPosition.Y = (253 + (spacing * j)) * ScaledVector2.ScaleFactor;

                    // Draw the action
                    spriteBatch.DrawString(Fonts.DescriptionFont, keyboardString, textPosition, Color.Black);

                    // Draw the key one
                    keyboardString = 
                        keyboardInfo.totalActionList[i].keyboardKeys[0].ToString();
                    textPosition.X = chartLine2Position + 
                        ((chartLine3Position - chartLine2Position) -
                        Fonts.DescriptionFont.MeasureString(keyboardString).X) / 2;
                    spriteBatch.DrawString(Fonts.DescriptionFont, keyboardString, 
                        textPosition, Color.Black);

                    // Draw the key two
                    if (keyboardInfo.totalActionList[i].keyboardKeys.Count > 1)
                    {
                        keyboardString = keyboardInfo.totalActionList[i].
                            keyboardKeys[1].ToString();
                        textPosition.X = chartLine3Position + 
                            ((chartLine4Position - chartLine3Position) -
                        Fonts.DescriptionFont.MeasureString(keyboardString).X) / 2;
                        spriteBatch.DrawString(Fonts.DescriptionFont, keyboardString, 
                            textPosition, Color.Black);
                    }
                    else
                    {
                        textPosition.X = chartLine3Position + 
                            ((chartLine4Position - chartLine3Position) -
                            Fonts.DescriptionFont.MeasureString("---").X) / 2;
                        spriteBatch.DrawString(Fonts.DescriptionFont, "---",
                            textPosition, Color.Black);
                    }
                }

                // Draw the Action
                actionPosition.X = chartLine1Position + 
                    ((chartLine2Position - chartLine1Position) -
                        Fonts.CaptionFont.MeasureString("Action").X) / 2;
                actionPosition.Y = 180 * ScaledVector2.ScaleFactor;
                spriteBatch.DrawString(Fonts.CaptionFont, "Action", actionPosition,
                    Fonts.CaptionColor);

                // Draw the Key 1
                key1Position.X = chartLine2Position + 
                    ((chartLine3Position - chartLine2Position) -
                    Fonts.CaptionFont.MeasureString("Key 1").X) / 2;
                key1Position.Y = 180 * ScaledVector2.ScaleFactor;
                spriteBatch.DrawString(Fonts.CaptionFont, "Key 1", key1Position,
                    Fonts.CaptionColor);

                // Draw the Key 2
                key2Position.X = chartLine3Position + 
                    ((chartLine4Position - chartLine3Position) -
                    Fonts.CaptionFont.MeasureString("Key 2").X) / 2;
                key2Position.Y = 180 * ScaledVector2.ScaleFactor;
                spriteBatch.DrawString(Fonts.CaptionFont, "Key 2", key2Position,
                    Fonts.CaptionColor);

                // Near left trigger
                spriteBatch.DrawString(Fonts.PlayerStatisticsFont, "Gamepad",
                    new Vector2(leftTriggerPosition.X + (leftTriggerButton.Width -
                    Fonts.PlayerStatisticsFont.MeasureString("Gamepad").X) / 2,
                    rightTriggerPosition.Y + 85 * ScaledVector2.ScaleFactor), Color.Black);

                // Near right trigger
                spriteBatch.DrawString(Fonts.PlayerStatisticsFont, "Gamepad",
                    new Vector2(rightTriggerPosition.X + (rightTriggerButton.Width -
                    Fonts.PlayerStatisticsFont.MeasureString("Gamepad").X) / 2,
                    rightTriggerPosition.Y + 85 * ScaledVector2.ScaleFactor), Color.Black);

                // Draw the title text
                titlePosition.X = plankPosition.X + (plankTexture.Width -
                    Fonts.HeaderFont.MeasureString("Keyboard").X) / 2;
                titlePosition.Y = plankPosition.Y + (plankTexture.Height -
                    Fonts.HeaderFont.MeasureString("Keyboard").Y) / 2;
                spriteBatch.DrawString(Fonts.HeaderFont, "Keyboard", titlePosition,
                    Fonts.TitleColor);

                // Draw the scroll textures
                spriteBatch.Draw(scrollUpTexture, scrollUpPosition,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                spriteBatch.Draw(scrollDownTexture, scrollDownPosition,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
        }


        #endregion
    }
}