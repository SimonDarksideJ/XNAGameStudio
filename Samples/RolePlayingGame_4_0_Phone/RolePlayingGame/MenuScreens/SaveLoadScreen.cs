#region File Description
//-----------------------------------------------------------------------------
// SaveLoadScreen.cs
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
    /// Displays a list of existing save games, 
    /// allowing the user to save, load, or delete.
    /// </summary>
    class SaveLoadScreen : GameScreen
    {
        public enum SaveLoadScreenMode
        {
            Save,
            Load,
        };

        /// <summary>
        /// The mode of this screen.
        /// </summary>
        private SaveLoadScreenMode mode;


        /// <summary>
        /// The current selected slot.
        /// </summary>
        private int currentSlot;


        #region Graphics Data


        private Texture2D backgroundTexture;
        private Vector2 backgroundPosition;

        private Texture2D plankTexture;
        private Vector2 plankPosition;

        private Texture2D backTexture;
        private Vector2 backPosition;

        private Texture2D deleteTexture;
        private Vector2 deletePosition = ScaledVector2.GetScaledVector(400f, 595);
        private Vector2 deleteTextPosition = ScaledVector2.GetScaledVector(410f,595);

        private Texture2D selectTexture;
        private Vector2 selectPosition;

        private Texture2D lineBorderTexture;
        private Vector2 lineBorderPosition;

        private Texture2D highlightTexture;
        private Texture2D arrowTexture;

        private Vector2 titleTextPosition;
        private Vector2 backTextPosition;
        private Vector2 selectTextPosition;


        #endregion


        #region Initialization


        /// <summary>
        /// Create a new SaveLoadScreen object.
        /// </summary>
        public SaveLoadScreen(SaveLoadScreenMode mode) : base()
        {
            this.mode = mode;

            // refresh the save game descriptions
            Session.RefreshSaveGameDescriptions();
        }


        /// <summary>
        /// Loads the graphics content for this screen.
        /// </summary>
        public override void LoadContent()
        {
            // load the textures
            ContentManager content = ScreenManager.Game.Content;
            backgroundTexture = 
                content.Load<Texture2D>(@"Textures\MainMenu\MainMenu");
            plankTexture = 
                content.Load<Texture2D>(@"Textures\MainMenu\MainMenuPlank03");
            backTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            selectTexture =
                content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            deleteTexture =
                content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            lineBorderTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\LineBorder");
            highlightTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\HighlightLarge");
            arrowTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\SelectionArrow");

            // calculate the image positions
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            backgroundPosition = new Vector2(
                (viewport.Width - backgroundTexture.Width * ScaledVector2.DrawFactor) / 2,
                (viewport.Height - backgroundTexture.Height * ScaledVector2.DrawFactor) / 2); 
            plankPosition = backgroundPosition + new Vector2(
                backgroundTexture.Width * ScaledVector2.DrawFactor / 2 - plankTexture.Width 
                * ScaledVector2.DrawFactor / 2, 
                60f * ScaledVector2.ScaleFactor);
            backPosition = backgroundPosition + ScaledVector2.GetScaledVector(220, 590);
            selectPosition = backgroundPosition + ScaledVector2.GetScaledVector(900, 590);
            deletePosition = backgroundPosition + ScaledVector2.GetScaledVector(400f, 590);
            lineBorderPosition = backgroundPosition + ScaledVector2.GetScaledVector(200, 570);

            // calculate the text positions
            titleTextPosition = new Vector2(
                plankPosition.X + (plankTexture.Width * ScaledVector2.DrawFactor - 
                    Fonts.HeaderFont.MeasureString("Load").X) / 2,
                plankPosition.Y + (plankTexture.Height * ScaledVector2.DrawFactor - 
                    Fonts.HeaderFont.MeasureString("Load").Y) / 2);
            backTextPosition = new Vector2(backPosition.X + 30, backPosition.Y + 5);
            deleteTextPosition.X += deleteTexture.Width * ScaledVector2.DrawFactor;
            selectTextPosition = new Vector2(
                selectPosition.X - Fonts.ButtonNamesFont.MeasureString("Select").X - 5,
                selectPosition.Y + 5);

            base.LoadContent();
        }


        #endregion


        #region Handle Input


        /// <summary>
        /// Respond to user input.
        /// </summary>
        public override void HandleInput()
        {
            bool backClicked = false;
            bool selectClicked = false;
            bool deleteClicked = false;
            foreach (var item in ItemPositionMapping)
            {
                if (InputManager.IsButtonClicked(item.Key))
                {
                    currentSlot = item.Value;
                    break;
                }
            }


            if (InputManager.IsButtonClicked(new Rectangle
                 ((int)(backPosition.X ),
                 (int)backPosition.Y,
                 (int)(backTexture.Width * ScaledVector2.DrawFactor),
                 (int)(backTexture.Height * ScaledVector2.DrawFactor))))
            {
                backClicked = true;
            }
            if (InputManager.IsButtonClicked(new Rectangle
                ((int)selectPosition.X,
                (int)selectPosition.Y,
                (int)(selectTexture.Width * ScaledVector2.DrawFactor),
                (int)(selectTexture.Height * ScaledVector2.DrawFactor))))
            {
                selectClicked = true;
            }
            if (InputManager.IsButtonClicked(new Rectangle
                 ((int)deletePosition.X ,
                 (int)deletePosition.Y,
                 (int)(deleteTexture.Width * ScaledVector2.DrawFactor),
                 (int)(deleteTexture.Height * ScaledVector2.DrawFactor))))
            {
                deleteClicked = true;
            }

            // handle exiting the screen
            if (InputManager.IsActionTriggered(InputManager.Action.Back) || backClicked)
            {
                ExitScreen();
                return;
            }
            
            // handle selecting a save game
            if (selectClicked &&
                (Session.SaveGameDescriptions != null))
            {
                switch (mode)
                {
                    case SaveLoadScreenMode.Load:
                        if ((currentSlot >= 0) &&
                            (currentSlot < Session.SaveGameDescriptions.Count) &&
                            (Session.SaveGameDescriptions[currentSlot] != null))
                        {
                            if (Session.IsActive)
                            {
                                MessageBoxScreen messageBoxScreen = new MessageBoxScreen(
                                    "Are you sure you want to load this game?");
                                messageBoxScreen.Accepted += 
                                    ConfirmLoadMessageBoxAccepted;
                                ScreenManager.AddScreen(messageBoxScreen);
                            }
                            else
                            {
                                ConfirmLoadMessageBoxAccepted(null, EventArgs.Empty);
                            }
                        }
                        break;

                    case SaveLoadScreenMode.Save:
                        if ((currentSlot >= 0) && 
                            (currentSlot <= Session.SaveGameDescriptions.Count))
                        {
                            if (currentSlot == Session.SaveGameDescriptions.Count)
                            {
                                ConfirmSaveMessageBoxAccepted(null, EventArgs.Empty);
                            }
                            else
                            {
                                MessageBoxScreen messageBoxScreen = new MessageBoxScreen(
                                   "Are you sure you want to overwrite this save game?");
                                messageBoxScreen.Accepted += 
                                    ConfirmSaveMessageBoxAccepted;
                                ScreenManager.AddScreen(messageBoxScreen);
                            }
                        }
                        break;
                }

            }
            // handle deletion
            else if (InputManager.IsActionTriggered(InputManager.Action.DropUnEquip) || deleteClicked &&
                (Session.SaveGameDescriptions != null))
            {
                if ((currentSlot >= 0) &&
                    (currentSlot < Session.SaveGameDescriptions.Count) &&
                    (Session.SaveGameDescriptions[currentSlot] != null))
                {
                    MessageBoxScreen messageBoxScreen = new MessageBoxScreen(
                        "Are you sure you want to delete this save game?");
                    messageBoxScreen.Accepted += ConfirmDeleteMessageBoxAccepted;
                    ScreenManager.AddScreen(messageBoxScreen);
                }
            }
            // handle cursor-down
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorDown) &&
                (Session.SaveGameDescriptions != null))
            {
                int maximumSlot = Session.SaveGameDescriptions.Count;
                if (mode == SaveLoadScreenMode.Save)
                {
                    maximumSlot = Math.Min(maximumSlot + 1, 
                        Session.MaximumSaveGameDescriptions);
                }
                if (currentSlot < maximumSlot - 1)
                {
                    currentSlot++;
                }
            }
            // handle cursor-up
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorUp) &&
                (Session.SaveGameDescriptions != null))
            {
                if (currentSlot >= 1)
                {
                    currentSlot--;
                }
            }
        }


        /// <summary>
        /// Callback for the Save Game confirmation message box.
        /// </summary>
        void ConfirmSaveMessageBoxAccepted(object sender, EventArgs e)
        {
            if ((currentSlot >= 0) &&
                (currentSlot <= Session.SaveGameDescriptions.Count))
            {
                if (currentSlot == Session.SaveGameDescriptions.Count)
                {
                    Session.SaveSession(null);
                }
                else
                {
                    Session.SaveSession(Session.SaveGameDescriptions[currentSlot]);
                }
                ExitScreen();
            }
        }


        /// <summary>
        /// Delegate type for the save-game-selected-to-load event.
        /// </summary>
        /// <param name="saveGameDescription">
        /// The description of the file to load.
        /// </param>
        public delegate void LoadingSaveGameHandler(
            SaveGameDescription saveGameDescription);

        /// <summary>
        /// Fired when a save game is selected to load.
        /// </summary>
        /// <remarks>
        /// Loading save games exits multiple screens, 
        /// so we use events to move backwards.
        /// </remarks>
        public event LoadingSaveGameHandler LoadingSaveGame;


        /// <summary>
        /// Callback for the Load Game confirmation message box.
        /// </summary>
        void ConfirmLoadMessageBoxAccepted(object sender, EventArgs e)
        {
            if ((Session.SaveGameDescriptions != null) && (currentSlot >= 0) &&
                (currentSlot < Session.SaveGameDescriptions.Count) &&
                (Session.SaveGameDescriptions[currentSlot] != null))
            {
                ExitScreen();
                if (LoadingSaveGame != null)
                {
                    LoadingSaveGame(Session.SaveGameDescriptions[currentSlot]);
                }
            }
        }


        /// <summary>
        /// Callback for the Delete Game confirmation message box.
        /// </summary>
        void ConfirmDeleteMessageBoxAccepted(object sender, EventArgs e)
        {
            if ((Session.SaveGameDescriptions != null) && (currentSlot >= 0) &&
                (currentSlot < Session.SaveGameDescriptions.Count) &&
                (Session.SaveGameDescriptions[currentSlot] != null))
            {
                Session.DeleteSaveGame(Session.SaveGameDescriptions[currentSlot]);
            }
        }


        #endregion


        #region Drawing

        public Dictionary<Rectangle, int> ItemPositionMapping = new Dictionary<Rectangle, int>();

        /// <summary>
        /// Draws the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ItemPositionMapping.Clear();
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, backgroundPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            spriteBatch.Draw(plankTexture, plankPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            spriteBatch.Draw(lineBorderTexture, lineBorderPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            spriteBatch.Draw(backTexture, backPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            string text = "Back";
            Vector2 textPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont,text,
                new Rectangle((int)backPosition.X,(int)backPosition.Y,
                    backTexture.Width,backTexture.Height));

            spriteBatch.DrawString(Fonts.ButtonNamesFont, text,textPosition, Color.White);

            spriteBatch.DrawString(Fonts.HeaderFont, 
                (mode == SaveLoadScreenMode.Load ? "Load" : "Save"), 
                titleTextPosition, Fonts.TitleColor);

            if ((Session.SaveGameDescriptions != null))
            {
                for (int i = 0; i < Session.SaveGameDescriptions.Count; i++)
                {
                    Vector2 descriptionTextPosition = ScaledVector2.GetScaledVector(295f,
                        200f + i * (Fonts.GearInfoFont.LineSpacing + 80f));
                    ItemPositionMapping.Add(new Rectangle((int)descriptionTextPosition.X - 80,
                        (int)descriptionTextPosition.Y-25,600, 75), i);
                    Color descriptionTextColor = Color.Black;

                    // if the save game is selected, draw the highlight color
                    if (i == currentSlot)
                    {
                        descriptionTextColor = Fonts.HighlightColor;
                        spriteBatch.Draw(highlightTexture,
                            descriptionTextPosition + ScaledVector2.GetScaledVector(-100, -23),null,
                            Color.White,0f,
                            Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                        spriteBatch.Draw(arrowTexture,
                            descriptionTextPosition + ScaledVector2.GetScaledVector(-75, -15), null,
                            Color.White,0f,
                            Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

                        spriteBatch.Draw(deleteTexture, deletePosition,null,Color.White,0f,
                            Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

                        string deleteText = "Delete";
                        Vector2 deleteFontPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, deleteText,
                            new Rectangle((int)deletePosition.X, (int)deletePosition.Y,
                                deleteTexture.Width, deleteTexture.Height));

                        spriteBatch.DrawString(Fonts.ButtonNamesFont, deleteText, deleteFontPosition, Color.White);

                        spriteBatch.Draw(selectTexture, selectPosition,null, Color.White,0f,
                            Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

                        string selectText = "Select";
                        Vector2 selectFontPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, selectText,
                             new Rectangle((int)selectPosition.X, (int)selectPosition.Y,
                             selectTexture.Width, selectTexture.Height));

                        spriteBatch.DrawString(Fonts.ButtonNamesFont, selectText, selectFontPosition, Color.White);
                    }

                    spriteBatch.DrawString(Fonts.GearInfoFont,
                        Session.SaveGameDescriptions[i].ChapterName,
                        descriptionTextPosition , descriptionTextColor);
                    descriptionTextPosition.X = 650 * ScaledVector2.ScaleFactor;
                    spriteBatch.DrawString(Fonts.GearInfoFont,
                        Session.SaveGameDescriptions[i].Description,
                        descriptionTextPosition, descriptionTextColor);
                }

                // if there is space for one, add an empty entry
                if ((mode == SaveLoadScreenMode.Save) &&
                    (Session.SaveGameDescriptions.Count <
                        Session.MaximumSaveGameDescriptions))
                {
                    int i = Session.SaveGameDescriptions.Count;
                    Vector2 descriptionTextPosition = ScaledVector2.GetScaledVector(
                        295f,
                        200f + i * (Fonts.GearInfoFont.LineSpacing + 80f));
                    Color descriptionTextColor = Color.Black;

                    // if the save game is selected, draw the highlight color
                    if (i == currentSlot)
                    {
                        descriptionTextColor = Fonts.HighlightColor;
                        spriteBatch.Draw(highlightTexture,
                            descriptionTextPosition + ScaledVector2.GetScaledVector(-100, -23),null,
                            Color.White,0f,
                            Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                        spriteBatch.Draw(arrowTexture,
                            descriptionTextPosition + ScaledVector2.GetScaledVector(-75, -15),null,
                            Color.White,0f,
                        Vector2.Zero, ScaledVector2.DrawFactor,SpriteEffects.None,0f);
                        spriteBatch.Draw(selectTexture, selectPosition, null, Color.White, 0f,
                            Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

                        string selectText = "Select";
                        Vector2 selectFontPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, selectText,
                             new Rectangle((int)selectPosition.X, (int)selectPosition.Y,
                             selectTexture.Width, selectTexture.Height));

                        spriteBatch.DrawString(Fonts.ButtonNamesFont, selectText,
                            selectFontPosition, Color.White);
                    }

                    ItemPositionMapping.Add(new Rectangle((int)descriptionTextPosition.X - 80,
                      (int)descriptionTextPosition.Y -25, 600, 75), i);
                    spriteBatch.DrawString(Fonts.GearInfoFont, "-------empty------",
                        descriptionTextPosition, descriptionTextColor);
                    descriptionTextPosition.X = 650 * ScaledVector2.ScaleFactor;
                    spriteBatch.DrawString(Fonts.GearInfoFont, "-----",
                        descriptionTextPosition, descriptionTextColor);
                }
            }

            // if there are no slots to load, report that
            if (Session.SaveGameDescriptions == null)
            {
                spriteBatch.DrawString(Fonts.GearInfoFont, 
                    "No Storage Device Available",
                    ScaledVector2.GetScaledVector(295f, 200f), Color.Black);
            }
            else if ((mode == SaveLoadScreenMode.Load) &&
                (Session.SaveGameDescriptions.Count <= 0))
            {
                spriteBatch.DrawString(Fonts.GearInfoFont, 
                    "No Save Games Available",
                    ScaledVector2.GetScaledVector(295f, 200f), Color.Black);
            }


            spriteBatch.End();
        }

        #endregion
    }
}