#region File Description
//-----------------------------------------------------------------------------
// ListScreen.cs
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
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
#endregion

namespace RolePlaying
{
    abstract class ListScreen<T> : GameScreen
    {

        Rectangle upperArrow = new Rectangle(710, 155, 35, 35);
        Rectangle lowerArrow = new Rectangle(710, 285, 35, 35);

        #region Graphics Data

        protected readonly Vector2 iconOffset = ScaledVector2.GetScaledVector(0f, 0f);
        protected readonly Vector2 descriptionTextPosition = ScaledVector2.GetScaledVector(200, 530);
        protected readonly Vector2 listPositionTopPosition = ScaledVector2.GetScaledVector(1160, 338);
        protected readonly Vector2 listPositionBottomPosition = ScaledVector2.GetScaledVector(1160, 370);

        protected Texture2D backgroundTexture;
        protected Texture2D fadeTexture;

        protected Texture2D listTexture;
        protected readonly Vector2 listTexturePosition = ScaledVector2.GetScaledVector(187f, 170f);
        protected readonly Vector2 listEntryStartPosition = ScaledVector2.GetScaledVector(200f, 165);
        protected float listLineSpacing = 50 ;//* ScaledVector2.DrawFactor ;

        protected Texture2D plankTexture;
        protected Vector2 plankTexturePosition;
        protected string titleText = String.Empty;

        protected Texture2D goldTexture;
        protected readonly Vector2 goldTexturePosition = ScaledVector2.GetScaledVector(490f, 640f);
        protected string goldText = String.Empty;
        protected readonly Vector2 goldTextPosition = ScaledVector2.GetScaledVector(565f, 648f);

        private Texture2D highlightTexture;
        protected readonly Vector2 highlightStartPosition = ScaledVector2.GetScaledVector(170f, 237f);
        protected Texture2D selectionArrowTexture;
        protected readonly Vector2 selectionArrowPosition = ScaledVector2.GetScaledVector(135f, 245f);

        protected Texture2D leftTriggerTexture = null;
        protected readonly Vector2 leftTriggerTexturePosition = ScaledVector2.GetScaledVector(340f, 50f);
        protected string leftTriggerText = String.Empty;

        private Texture2D rightTriggerTexture = null;
        private readonly Vector2 rightTriggerTexturePosition = ScaledVector2.GetScaledVector(900f, 50f);
        protected string rightTriggerText = String.Empty;

        protected Texture2D leftQuantityArrowTexture;
        protected Texture2D rightQuantityArrowTexture;

        protected Texture2D backButtonTexture;
        protected Vector2 backButtonTexturePosition = ScaledVector2.GetScaledVector(150f, 570f);
        protected string backButtonText = String.Empty;

        protected Texture2D selectButtonTexture;
        protected readonly Vector2 selectButtonTexturePosition = ScaledVector2.GetScaledVector(950f, 570f);
        protected string selectButtonText = String.Empty;

        protected Texture2D xButtonTexture;
        protected readonly Vector2 xButtonTexturePosition = ScaledVector2.GetScaledVector(400f, 570f);
        protected string xButtonText = String.Empty;
        protected Vector2 xButtonTextPosition = ScaledVector2.GetScaledVector(250f, 645f); // + tex width

        protected Texture2D yButtonTexture;
        protected readonly Vector2 yButtonTexturePosition = ScaledVector2.GetScaledVector(580f, 570f);
        protected string yButtonText = String.Empty;


        #endregion


        #region Data Access


        /// <summary>
        /// Get the list that this screen displays.
        /// </summary>
        /// <returns></returns>
        public abstract ReadOnlyCollection<T> GetDataList();


        #endregion


        #region List Navigation


        /// <summary>
        /// The index of the selected entry.
        /// </summary>
        private int selectedIndex = 0;

        /// <summary>
        /// The index of the selected entry.
        /// </summary>
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    EnsureVisible(selectedIndex);
                }
            }
        }


        /// <summary>
        /// Ensure that the given index is visible on the screen.
        /// </summary>
        public void EnsureVisible(int index)
        {
            if (index < startIndex)
            {
                // if it's above the current selection, set the first entry
                startIndex = index;
            }
            if (selectedIndex > (endIndex - 1))
            {
                startIndex += selectedIndex - (endIndex - 1);
            }
            // otherwise, it should be in the current selection already
            // -- note that the start and end indices are checked in Draw.
        }




        /// <summary>
        /// Move the current selection up one entry.
        /// </summary>
        protected virtual void MoveCursorUp()
        {
            if (SelectedIndex > 0)
            {
                SelectedIndex--;
            }
        }


        /// <summary>
        /// Move the current selection down one entry.
        /// </summary>
        protected virtual void MoveCursorDown()
        {
            SelectedIndex++;   // safety-checked in Draw()
        }


        /// <summary>
        /// Decrease the selected quantity by one.
        /// </summary>
        protected virtual void MoveCursorLeft() { }


        /// <summary>
        /// Increase the selected quantity by one.
        /// </summary>
        protected virtual void MoveCursorRight() { }
        
        
        /// <summary>
        /// The first index displayed on the screen from the list.
        /// </summary>
        private int startIndex = 0;

        /// <summary>
        /// The first index displayed on the screen from the list.
        /// </summary>
        public int StartIndex
        {
            get { return startIndex; }
            set { startIndex = value; } // safety-checked in Draw
        }


        /// <summary>
        /// The last index displayed on the screen from the list.
        /// </summary>
        private int endIndex = 0;

        /// <summary>
        /// The last index displayed on the screen from the list.
        /// </summary>
        public int EndIndex
        {
            get { return endIndex; }
            set { endIndex = value; }   // safety-checked in Draw
        }


        /// <summary>
        /// The maximum number of list entries that the screen can show at once.
        /// </summary>
        public const int MaximumListEntries = 4;


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new ListScreen object.
        /// </summary>
        public ListScreen()
            : base() 
        {
            this.IsPopup = true;
            
        }
 

        /// <summary>
        /// Load the graphics content from the content manager.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            // load the background textures
            fadeTexture = content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");
            backgroundTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\GameScreenBkgd");
            listTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\InfoDisplay");
            plankTexture =
                content.Load<Texture2D>(@"Textures\MainMenu\MainMenuPlank03");
            goldTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\GoldIcon");

            // load the foreground textures
            highlightTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\HighlightLarge");
            selectionArrowTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\SelectionArrow");

            leftQuantityArrowTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\QuantityArrowLeft");
            rightQuantityArrowTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\QuantityArrowRight");
            backButtonTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            selectButtonTexture =
                content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            xButtonTexture =
                content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            yButtonTexture =
                content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");

            // calculate the centered positions
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            plankTexturePosition = new Vector2(
                viewport.X + (viewport.Width - (plankTexture.Width * ScaledVector2.DrawFactor)) / 2f,
                40f * ScaledVector2.ScaleFactor);


            base.LoadContent();
        }


        #endregion


        #region Input Handling


        /// <summary>
        /// Handle user input.
        /// </summary>
        public override void HandleInput()
        {
            Vector2 position = listEntryStartPosition + new Vector2(0f, listLineSpacing / 2);
            if (startIndex >= 0)
            {
                for (int index = startIndex; index < endIndex; index++)
                {
                    if(InputManager.IsButtonClicked(new Rectangle((int)position.X,(int)position.Y,
                        250,50)))
                    {
                        selectedIndex = index;
                        var dataList = GetDataList();
                        SelectTriggered(dataList[index]);
                        return;
                    }

                    position.Y += listLineSpacing;
                }
            }


            bool backClicked = false;
            bool okClicked = false;
            bool lowerClicked = false;
            bool upperClicked = false;
            bool buttonXClicked = false;
            bool buttonYClicked = false;

            if (InputManager.IsButtonClicked(lowerArrow))
            {
                lowerClicked = true;
            }

            if (InputManager.IsButtonClicked(upperArrow))
            {
                upperClicked = true;
            }

            if (InputManager.IsButtonClicked(new Rectangle(
                (int)backButtonTexturePosition.X,
                (int)backButtonTexturePosition.Y,
                (int)(backButtonTexture.Width * ScaledVector2.DrawFactor),
                (int)(backButtonTexture.Height * ScaledVector2.DrawFactor))))
            {
                backClicked = true;
            }


            if (InputManager.IsButtonClicked(new Rectangle(
                (int)selectButtonTexturePosition.X,
                (int)selectButtonTexturePosition.Y ,
                (int)(selectButtonTexture.Width * ScaledVector2.DrawFactor),
                (int)(selectButtonTexture.Height * ScaledVector2.DrawFactor))))
            {
                okClicked = true;
            }

            if (InputManager.IsButtonClicked(new Rectangle(
                (int)xButtonTexturePosition.X,
                (int)xButtonTexturePosition.Y,
                (int)(xButtonTexture.Width * ScaledVector2.DrawFactor),
                (int)(xButtonTexture.Height * ScaledVector2.DrawFactor))))
            {
                buttonXClicked = true;
            }
            if (InputManager.IsButtonClicked(new Rectangle(
                (int)yButtonTexturePosition.X,
                (int)yButtonTexturePosition.Y,
                (int)(yButtonTexture.Width * ScaledVector2.DrawFactor),
                (int)(yButtonTexture.Height * ScaledVector2.DrawFactor))))
            {
                buttonYClicked = true;
            }

            if (InputManager.IsActionTriggered(InputManager.Action.PageLeft))
            {
                PageScreenLeft();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.PageRight))
            {
                PageScreenRight();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorUp) ||upperClicked)
            {
                MoveCursorUp();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorDown) || lowerClicked)
            {
                MoveCursorDown();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.IncreaseAmount))
            {
                MoveCursorRight();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.DecreaseAmount))
            {
                MoveCursorLeft();
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.Back) || backClicked)
            {
                BackTriggered();
            }
            else if (okClicked)
            {
                ReadOnlyCollection<T> dataList = GetDataList();
                if ((selectedIndex >= 0) && (selectedIndex < dataList.Count))
                {
                    SelectTriggered(dataList[selectedIndex]);
                }
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.DropUnEquip) || buttonXClicked)
            {
                ReadOnlyCollection<T> dataList = GetDataList();
                if ((selectedIndex >= 0) && (selectedIndex < dataList.Count))
                {
                    ButtonXPressed(dataList[selectedIndex]);
                }
            }
            else if (InputManager.IsActionTriggered(InputManager.Action.TakeView) || buttonYClicked)
            {
                ReadOnlyCollection<T> dataList = GetDataList();
                if ((selectedIndex >= 0) && (selectedIndex < dataList.Count))
                {
                    ButtonYPressed(dataList[selectedIndex]);
                }
            }
            base.HandleInput();
        }


        /// <summary>
        /// Switch to the screen to the "left" of this one in the UI, if any.
        /// </summary>
        protected virtual void PageScreenLeft() { }


        /// <summary>
        /// Switch to the screen to the "right" of this one in the UI, if any.
        /// </summary>
        protected virtual void PageScreenRight() { }


        /// <summary>
        /// Respond to the triggering of the Back action.
        /// </summary>
        protected virtual void BackTriggered()
        {
            ExitScreen();
        }


        /// <summary>
        /// Respond to the triggering of the Select action.
        /// </summary>
        protected virtual void SelectTriggered(T entry) { }


        /// <summary>
        /// Respond to the triggering of the X button (and related key).
        /// </summary>
        protected virtual void ButtonXPressed(T entry) { }


        /// <summary>
        /// Respond to the triggering of the Y button (and related key).
        /// </summary>
        protected virtual void ButtonYPressed(T entry) { }


        #endregion


        #region Drawing


        /// <summary>
        /// Draws the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // get the content list
            ReadOnlyCollection<T> dataList = GetDataList();

            // turn off the buttons if the list is empty
            if (dataList.Count <= 0)
            {
                selectButtonText = String.Empty;
                xButtonText = String.Empty;
                yButtonText = String.Empty;
            }

            // fix the indices for the current list size
            SelectedIndex = (int)MathHelper.Clamp(SelectedIndex, 0, dataList.Count - 1);
            startIndex = (int)MathHelper.Clamp(startIndex, 0, 
                dataList.Count - MaximumListEntries);
            endIndex = Math.Min(startIndex + MaximumListEntries, dataList.Count);

            spriteBatch.Begin();

            DrawBackground();
            if (dataList.Count > 0)
            {
                DrawListPosition(SelectedIndex + 1, dataList.Count);
            }
            DrawButtons();
            DrawPartyGold();
            DrawColumnHeaders();
            DrawTitle();

            // draw each item currently shown
            Vector2 position = listEntryStartPosition + 
                new Vector2(0f, listLineSpacing / 2);
            if (startIndex >= 0)
            {
                for (int index = startIndex; index < endIndex; index++)
                {
                    T entry = dataList[index];
                    if (index == selectedIndex)
                    {
                        DrawSelection(position);
                        DrawEntry(entry, position, true);
                        DrawSelectedDescription(entry);
                    }
                    else
                    {
                        DrawEntry(entry, position, false);
                    }
                    position.Y += listLineSpacing;
                }
            }
            
            spriteBatch.End();
        }


        /// <summary>
        /// Draw the entry at the given position in the list.
        /// </summary>
        /// <param name="entry">The entry to draw.</param>
        /// <param name="position">The position to draw the entry at.</param>
        /// <param name="isSelected">If true, this entry is selected.</param>
        protected abstract void DrawEntry(T entry, Vector2 position, bool isSelected);


        /// <summary>
        /// Draw the selection graphics over the selected item.
        /// </summary>
        /// <param name="position"></param>
        protected virtual void DrawSelection(Vector2 position)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Draw(highlightTexture, 
                new Vector2(highlightStartPosition.X, position.Y),null, Color.White,0f,
                Vector2.Zero,ScaledVector2.DrawFactor,SpriteEffects.None,0f);
            spriteBatch.Draw(selectionArrowTexture,
                new Vector2(selectionArrowPosition.X, position.Y + 10f),null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
        }


        /// <summary>
        /// Draw the background of the screen.
        /// </summary>
        protected virtual void DrawBackground()
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            var fadePosition = new Vector2(ScreenManager.Game.GraphicsDevice.Viewport.Bounds.X,
                ScreenManager.Game.GraphicsDevice.Viewport.Bounds.Y);

            spriteBatch.Draw(fadeTexture,
                new Rectangle(0, 0, ScreenManager.Game.GraphicsDevice.Viewport.Width, 
                    ScreenManager.Game.GraphicsDevice.Viewport.Height),
                Color.Black);

            spriteBatch.Draw(backgroundTexture, Vector2.Zero, 
                null, Color.White, 0f,
                Vector2.Zero, ScaledVector2.DrawFactor /*+ 0.25f*/, SpriteEffects.None, 0f);

            spriteBatch.Draw(listTexture, listTexturePosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor,SpriteEffects.None,0f);
        }


        /// <summary>
        /// Draw the current list position in the appropriate location on the screen.
        /// </summary>
        /// <param name="position">The current position in the list.</param>
        /// <param name="total">The total elements in the list.</param>
        protected virtual void DrawListPosition(int position, int total)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // draw the top number - the current position in the list
            string listPositionTopText = position.ToString();
            Vector2 drawPosition = listPositionTopPosition;
            drawPosition.X -= (float)Math.Ceiling(
                Fonts.GearInfoFont.MeasureString(listPositionTopText).X / 2);
            spriteBatch.DrawString(Fonts.GearInfoFont, listPositionTopText,
                drawPosition, Fonts.CountColor);

            // draw the bottom number - the current position in the list
            string listPositionBottomText = total.ToString();
            drawPosition = listPositionBottomPosition;
            drawPosition.X -= (float)Math.Ceiling(
                Fonts.GearInfoFont.MeasureString(listPositionBottomText).X / 2);
            spriteBatch.DrawString(Fonts.GearInfoFont, listPositionBottomText,
                drawPosition, Fonts.CountColor);
        }

        
        /// <summary>
        /// Draw the party gold text.
        /// </summary>
        protected virtual void DrawPartyGold()
        {
            if (!IsActive)
            {
                return;
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Draw(goldTexture, goldTexturePosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            spriteBatch.DrawString(Fonts.ButtonNamesFont,
                Fonts.GetGoldString(Session.Party.PartyGold), goldTextPosition,
                Color.White);
        }


        /// <summary>
        /// Draw the description of the selected item.
        /// </summary>
        protected abstract void DrawSelectedDescription(T entry);


        /// <summary>
        /// Draw the column headers above the list.
        /// </summary>
        protected abstract void DrawColumnHeaders();


        /// <summary>
        /// Draw all of the buttons used by the screen.
        /// </summary>
        protected virtual void DrawButtons()
        {
            if (!IsActive)
            {
                return;
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // draw the left trigger texture and text
            if ((leftTriggerTexture != null) && !String.IsNullOrEmpty(leftTriggerText))
            {
                Vector2 position = leftTriggerTexturePosition + new Vector2(
                    leftTriggerTexture.Width / 2f - (float)Math.Ceiling(
                    Fonts.PlayerStatisticsFont.MeasureString(leftTriggerText).X / 2f),
                    60f * ScaledVector2.ScaleFactor);

            }

            // draw the right trigger texture and text
            if ((rightTriggerTexture != null) && !String.IsNullOrEmpty(rightTriggerText))
            {
                Vector2 position = rightTriggerTexturePosition + new Vector2(
                    rightTriggerTexture.Width / 2f - (float)Math.Ceiling(
                    Fonts.PlayerStatisticsFont.MeasureString(rightTriggerText).X / 2f),
                    60f * ScaledVector2.ScaleFactor);
            }

            // draw the left trigger texture and text
            if ((backButtonTexture != null) && !String.IsNullOrEmpty(backButtonText))
            {
                spriteBatch.Draw(backButtonTexture, backButtonTexturePosition,null,Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

                Vector2 backPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, backButtonText,
                    new Rectangle((int)backButtonTexturePosition.X, (int)backButtonTexturePosition.Y,
                        backButtonTexture.Width, backButtonTexture.Height));
                spriteBatch.DrawString(Fonts.ButtonNamesFont, backButtonText,
                    backPosition, Color.White);
            }

            // draw the left trigger texture and text
            if ((selectButtonTexture != null) && !String.IsNullOrEmpty(selectButtonText))
            {
                spriteBatch.Draw(selectButtonTexture, selectButtonTexturePosition,null,Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);


                Vector2 selectPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, selectButtonText,
                        new Rectangle((int)selectButtonTexturePosition.X, (int)selectButtonTexturePosition.Y,
                            selectButtonTexture.Width, selectButtonTexture.Height));
                spriteBatch.DrawString(Fonts.ButtonNamesFont, selectButtonText,
                    selectPosition, Color.White);
            }

            // draw the left trigger texture and text
            if ((xButtonTexture != null) && !String.IsNullOrEmpty(xButtonText))
            {
                spriteBatch.Draw(xButtonTexture, xButtonTexturePosition,null,Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);


                Vector2 xPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, xButtonText,
                        new Rectangle((int)xButtonTexturePosition.X, (int)xButtonTexturePosition.Y,
                            xButtonTexture.Width, xButtonTexture.Height));
                spriteBatch.DrawString(Fonts.ButtonNamesFont, xButtonText,
                    xPosition, Color.White);
            }

            // draw the left trigger texture and text
            if ((yButtonTexture != null) && !String.IsNullOrEmpty(yButtonText))
            {
                spriteBatch.Draw(yButtonTexture, yButtonTexturePosition,null,Color.White,0f,
                    Vector2.Zero,  ScaledVector2.DrawFactor, SpriteEffects.None, 0f);


                Vector2 yPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, yButtonText,
                        new Rectangle((int)yButtonTexturePosition.X, (int)yButtonTexturePosition.Y,
                            yButtonTexture.Width, yButtonTexture.Height));

                spriteBatch.DrawString(Fonts.ButtonNamesFont, yButtonText,
                    yPosition, Color.White);
            }
        }


        /// <summary>
        /// Draw the title of the screen, if any.
        /// </summary>
        protected virtual void DrawTitle()
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // draw the left trigger texture and text
            if ((plankTexture != null) && !String.IsNullOrEmpty(titleText))
            {
                Vector2 titleTextSize = Fonts.HeaderFont.MeasureString(titleText);
                Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
                Vector2 position = new Vector2(
                    (float)Math.Floor(viewport.X + viewport.Width / 2 -
                    titleTextSize.X / 2f) + 15, 60f * ScaledVector2.ScaleFactor);
                spriteBatch.Draw(plankTexture, plankTexturePosition,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                spriteBatch.DrawString(Fonts.HeaderFont, titleText, position,
                    Fonts.TitleColor,0f,Vector2.Zero,0.75f,SpriteEffects.None,0f);
            }
        }


        #endregion
    }
}
