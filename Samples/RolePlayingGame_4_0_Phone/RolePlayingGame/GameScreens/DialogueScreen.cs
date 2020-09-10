#region File Description
//-----------------------------------------------------------------------------
// DialogueScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Display of conversation dialog between the player and the npc
    /// </summary>
    class DialogueScreen : GameScreen
    {
        #region Graphics Data


        private Texture2D backgroundTexture;
        private Vector2 backgroundPosition;
        private Texture2D fadeTexture;

        protected Texture2D selectButtonTexture;
        protected Vector2 selectPosition;
        protected Vector2 selectButtonPosition;

        protected Vector2 backPosition;
        protected Texture2D backButtonTexture;
        protected Vector2 backButtonPosition;

        private Texture2D scrollTexture;
        private Vector2 scrollPosition;

        private Texture2D lineTexture;
        private Vector2 topLinePosition;
        private Vector2 bottomLinePosition;

        private Vector2 titlePosition;
        private Vector2 dialogueStartPosition;

        Rectangle lowerArrow;
        Rectangle upperArrow;


        #endregion


        #region Text Data


        /// <summary>
        /// The title text shown at the top of the screen.
        /// </summary>
        private string titleText;

        /// <summary>
        /// The title text shown at the top of the screen.
        /// </summary>
        public string TitleText
        {
            get { return titleText; }
            set { titleText = value; }
        }


        /// <summary>
        /// The dialogue shown in the main portion of this dialog.
        /// </summary>
        private string dialogueText;

        /// <summary>
        /// The dialogue shown in the main portion of this dialog, broken into lines.
        /// </summary>
        private List<string> dialogueList = new List<string>();

        /// <summary>
        /// The dialogue shown in the main portion of this dialog.
        /// </summary>
        public string DialogueText
        {
            get { return dialogueText; }
            set
            {
                // trim the new value
                string trimmedValue = value.Trim();
                // if it's a match for what we already have, then this is trivial
                if (dialogueText == trimmedValue)
                {
                    return;
                }
                // assign the new value
                dialogueText = trimmedValue;
                // break the text into lines
                if (String.IsNullOrEmpty(dialogueText))
                {
                    dialogueList.Clear();
                }
                else
                {
                    dialogueList = Fonts.BreakTextIntoList(dialogueText, 
                        Fonts.DescriptionFont, maxWidth);
                }
                // set which lines ar edrawn
                startIndex = 0;
                endIndex = drawMaxLines;
                if (endIndex > dialogueList.Count)
                {
                    dialogueStartPosition = ScaledVector2.GetScaledVector(271f,
                        375f - ((dialogueList.Count - startIndex) *
                            (Fonts.DescriptionFont.LineSpacing) / 2));
                    endIndex = dialogueList.Count;
                }
                else
                {
                    dialogueStartPosition = ScaledVector2.GetScaledVector(271f, 225f);
                }
            }
        }


        /// <summary>
        /// The text shown next to the A button, if any.
        /// </summary>
        private string selectText = "Continue";

        /// <summary>
        /// The text shown next to the A button, if any.
        /// </summary>
        public string SelectText
        {
            get { return selectText; }
            set 
            {
                if (selectText != value)
                {
                    selectText = value;
                    if (selectButtonTexture != null)
                    {
                        selectPosition.X = selectButtonPosition.X -
                            Fonts.ButtonNamesFont.MeasureString(selectText).X - 10f;
                        selectPosition.Y = selectButtonPosition.Y;
                    }
                }
            }
        }


        /// <summary>
        /// The text shown next to the B button, if any.
        /// </summary>
        private string backText = "Back";

        /// <summary>
        /// The text shown next to the B button, if any.
        /// </summary>
        public string BackText
        {
            get { return backText; }
            set { backText = value; }
        }


        /// <summary>
        /// Maximum width of each line in pixels
        /// </summary>
        private static readonly int maxWidth = (int)(705 * ScaledVector2.ScaleFactor);


        /// <summary>
        /// Starting index of the list to be displayed
        /// </summary>
        private int startIndex = 0;


        /// <summary>
        /// Ending index of the list to be displayed
        /// </summary>
        private int endIndex = drawMaxLines;


        /// <summary>
        /// Maximum number of lines to draw in the screen
        /// </summary>
        private const int drawMaxLines = 7;


        #endregion


        #region Initialization


        /// <summary>
        /// Construct a new DialogueScreen object.
        /// </summary>
        /// <param name="mapEntry"></param>
        public DialogueScreen()
        {
            this.IsPopup = true; 
        }


        /// <summary>
        /// Load the graphics content
        /// </summary>
        /// <param name="batch">SpriteBatch object</param>
        /// <param name="screenWidth">Width of the screen</param>
        /// <param name="screenHeight">Height of the screen</param>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            fadeTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");
            backgroundTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\PopupScreen");
            scrollTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\ScrollButtons");
            selectButtonTexture = content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            backButtonTexture = content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            lineTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\SeparationLine");

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            upperArrow = new Rectangle((int)scrollPosition.X - 40, (int)scrollPosition.Y - 40,
                             80, 80);
            lowerArrow = new Rectangle((int)scrollPosition.X - 40, (int)scrollPosition.Y + 100,
                             80, 80);

            backgroundPosition.X = (viewport.Width - (backgroundTexture.Width * ScaledVector2.DrawFactor)) / 2;
            backgroundPosition.Y = (viewport.Height - (backgroundTexture.Height * ScaledVector2.DrawFactor)) / 2;


            selectButtonPosition.X = viewport.Width / 2 + 130 * ScaledVector2.ScaleFactor;
            selectButtonPosition.Y = backgroundPosition.Y + 520f * ScaledVector2.ScaleFactor;
            selectPosition.X = selectButtonPosition.X -
                Fonts.ButtonNamesFont.MeasureString(selectText).X - 10f;
            selectPosition.Y = selectButtonPosition.Y;

            backPosition.X = viewport.Width / 2 - 140f * ScaledVector2.ScaleFactor;
            backPosition.Y = backgroundPosition.Y + 520f * ScaledVector2.ScaleFactor;
            backButtonPosition.X = backPosition.X - (backButtonTexture.Width * 
                ScaledVector2.DrawFactor) - 10;
            backButtonPosition.Y = backPosition.Y;

            scrollPosition = backgroundPosition + ScaledVector2.GetScaledVector(820f, 200f);

            topLinePosition.X = (viewport.Width - (lineTexture.Width * ScaledVector2.DrawFactor)) / 
                2 - 30f * ScaledVector2.ScaleFactor;
            topLinePosition.Y = 200f * ScaledVector2.ScaleFactor;

            bottomLinePosition.X = topLinePosition.X;
            bottomLinePosition.Y = 550f * ScaledVector2.ScaleFactor;

            titlePosition.X = (viewport.Width -
                Fonts.HeaderFont.MeasureString(titleText).X) / 2;
            titlePosition.Y = backgroundPosition.Y + 70f * ScaledVector2.ScaleFactor;
        }


        #endregion


        #region Updating


        /// <summary>
        /// Handles user input to the dialog.
        /// </summary>
        public override void HandleInput()
        {
            bool upperClicked = false;
            bool lowerClicked = false;

            if (InputManager.IsButtonClicked(upperArrow))
            {
                upperClicked = true;
            }

            if (InputManager.IsButtonClicked(lowerArrow))
            {
                lowerClicked = true;
            }



            // Scroll up
            if (InputManager.IsActionTriggered(InputManager.Action.CursorUp) || upperClicked)
            {
                if (startIndex > 0)
                {
                    startIndex--;
                    endIndex--;
                }
            }
            // Scroll down
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorDown) || lowerClicked)
            {
                if (startIndex < dialogueList.Count - drawMaxLines)
                {
                    endIndex++;
                    startIndex++;
                }
            }

            // Press Select or back
            else if (InputManager.IsActionTriggered(InputManager.Action.Back) 
                ||   InputManager.IsButtonClicked(new Rectangle(
                     (int)backButtonPosition.X,
                     (int)backButtonPosition.Y,
                     (int)(backButtonTexture.Width * ScaledVector2.DrawFactor),
                     (int)(backButtonTexture.Height * ScaledVector2.DrawFactor)))
                || InputManager.IsButtonClicked(new Rectangle(
                     (int)selectButtonPosition.X,
                     (int)selectButtonPosition.Y,
                     (int)(selectButtonTexture.Width * ScaledVector2.DrawFactor),
                     (int)(selectButtonTexture.Height * ScaledVector2.DrawFactor))))
            {
                ExitScreen();
                return;
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// draws the dialog.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Vector2 textPosition = dialogueStartPosition;

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // draw the fading screen
            spriteBatch.Draw(fadeTexture, ScaledVector2.GetScaledVector(1280, 720),null, Color.White,0f,
                Vector2.Zero,ScaledVector2.DrawFactor,SpriteEffects.None,0f);

            // draw popup background
            spriteBatch.Draw(backgroundTexture, backgroundPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // draw the top line
            spriteBatch.Draw(lineTexture, topLinePosition,null, Color.White,0f,
            Vector2.Zero, ScaledVector2.DrawFactor,SpriteEffects.None,0f);

            // draw the bottom line
            spriteBatch.Draw(lineTexture, bottomLinePosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // draw scrollbar
            spriteBatch.Draw(scrollTexture, scrollPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // draw title
            spriteBatch.DrawString(Fonts.HeaderFont, titleText, titlePosition,
                Fonts.CountColor);

            // draw the dialogue
            for (int i = startIndex; i < endIndex; i++)
            {
                spriteBatch.DrawString(Fonts.DescriptionFont, dialogueList[i],
                    textPosition, Fonts.CountColor);
                textPosition.Y += Fonts.DescriptionFont.LineSpacing;
            }

            // draw the Back button and adjoining text
            if (!String.IsNullOrEmpty(backText))
            {
                Vector2 backTextPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, backText,
                    new Rectangle((int)backButtonPosition.X, (int)backButtonPosition.Y,
                        backButtonTexture.Width, backButtonTexture.Height));

                spriteBatch.Draw(backButtonTexture, backButtonPosition, null, Color.White, 0f,
                        Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

                spriteBatch.DrawString(Fonts.ButtonNamesFont, backText, backTextPosition,
                    Color.White);

            }

            // draw the Select button and adjoining text
            if (!String.IsNullOrEmpty(selectText))
            {
                selectPosition.X = selectButtonPosition.X -
                    Fonts.ButtonNamesFont.MeasureString(selectText).X - 10f;
                selectPosition.Y = selectButtonPosition.Y;

                Vector2 selectTextPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, selectText,
                        new Rectangle((int)selectButtonPosition.X, (int)selectButtonPosition.Y,
                            selectButtonTexture.Width, selectButtonTexture.Height));

                spriteBatch.Draw(selectButtonTexture, selectButtonPosition, null, Color.White, 0f,
                     Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

                spriteBatch.DrawString(Fonts.ButtonNamesFont, selectText, selectTextPosition ,
                    Color.White);

            }

            spriteBatch.End();
        }


        #endregion
    }
}