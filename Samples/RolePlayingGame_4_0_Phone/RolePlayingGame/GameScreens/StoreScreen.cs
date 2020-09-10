#region File Description
//-----------------------------------------------------------------------------
// StoreScreen.cs
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
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Draws the options available in a store - typically to buy or sell gear.
    /// </summary>
    class StoreScreen : GameScreen
    {
        private Store store = null;


        #region Graphics Data


        private Texture2D shopDrawScreen;
        private Texture2D highlightItem;
        private Texture2D selectionArrow;
        private Texture2D conversationStrip;
        private Texture2D plankTexture;
        private Texture2D fadeTexture;
        private Texture2D goldIcon;

        private readonly Vector2 textPosition = ScaledVector2.GetScaledVector(620, 210);
        private readonly Vector2 partyGoldPosition = ScaledVector2.GetScaledVector(565, 648);
        private readonly Vector2 shopKeeperPosition = ScaledVector2.GetScaledVector(290, 370);
        private readonly Vector2 welcomeMessagePosition = ScaledVector2.GetScaledVector(470, 460);
        private readonly Vector2 conversationStripPosition = ScaledVector2.GetScaledVector(240, 405);
        private readonly Vector2 goldIconPosition = ScaledVector2.GetScaledVector(490, 640);
        private readonly Vector2 highlightItemOffset = ScaledVector2.GetScaledVector(400, 20);
        private readonly Vector2 selectionArrowOffset = ScaledVector2.GetScaledVector(100, 16);

        private Vector2 shopNamePosition;
        private Vector2 plankPosition;
        private Vector2 titleBarMidPosition;
        private Vector2 placeTextMid;
        private Rectangle screenRect;

        private int currentCursor;
        private const float interval = 70 * ScaledVector2.ScaleFactor;


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new StoreScreen object for the given store.
        /// </summary>
        public StoreScreen(Store store)
        {
            // check the parameter
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            this.IsPopup = true;
            this.store = store;

            titleBarMidPosition = new Vector2(
                -Fonts.HeaderFont.MeasureString(store.Name).X / 2, 0f);
            placeTextMid = Fonts.ButtonNamesFont.MeasureString("Select");
         
        }


        /// <summary>
        /// Loads the graphics content from the content manager.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            shopDrawScreen = 
                content.Load<Texture2D>(@"Textures\GameScreens\GameScreenBkgd");
            highlightItem = 
                content.Load<Texture2D>(@"Textures\GameScreens\HighlightLarge");
            selectionArrow = 
                content.Load<Texture2D>(@"Textures\GameScreens\SelectionArrow");
            fadeTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");
            conversationStrip =
                content.Load<Texture2D>(@"Textures\GameScreens\ConversationStrip");
            goldIcon = 
                content.Load<Texture2D>(@"Textures\GameScreens\GoldIcon");
            plankTexture = 
                content.Load<Texture2D>(@"Textures\MainMenu\MainMenuPlank03");

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            screenRect = new Rectangle(viewport.X, viewport.Y,
                viewport.Width, viewport.Height);
            plankPosition = new Vector2(
                (viewport.Width - plankTexture.Width * ScaledVector2.DrawFactor ) / 2, 66f * ScaledVector2.ScaleFactor);
            shopNamePosition = new Vector2(
                (viewport.Width - Fonts.HeaderFont.MeasureString(store.Name).X) / 2, 
                90f * ScaledVector2.ScaleFactor);
        }


        #endregion


        #region Updating


        /// <summary>
        /// Handles user input.
        /// </summary>
        public override void HandleInput()
        {
            if (InputManager.IsButtonClicked(new Rectangle((int)textPosition.X, (int)textPosition.Y,
                120, 40)))
            {
                ScreenManager.AddScreen(new StoreBuyScreen(store));
                currentCursor = 0;
                return;
            }
            if (InputManager.IsButtonClicked(new Rectangle((int)textPosition.X, 
                (int)textPosition.Y + (int)interval, 120, 40)))
            {
                currentCursor = 1;
                ScreenManager.AddScreen(new StoreSellScreen(store));
                return;
            }
            if (InputManager.IsButtonClicked(new Rectangle((int)textPosition.X, 
                (int)textPosition.Y + (int)(interval * 2), 120, 40)))
            {
                ExitScreen();
                currentCursor = 2;
                return;
            }

            // exits the screen
            if (InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                ExitScreen();
                return;
            }
            // select one of the buttons
            else if (InputManager.IsActionTriggered(InputManager.Action.Ok))
            {
                if (currentCursor == 0)
                {
                    ScreenManager.AddScreen(new StoreBuyScreen(store));
                }
                else if (currentCursor == 1)
                {
                    ScreenManager.AddScreen(new StoreSellScreen(store));
                }
                else
                {
                    ExitScreen();
                }
                return;
            }
            // move the cursor up
            else if (InputManager.IsActionTriggered(InputManager.Action.MoveCharacterUp))
            {
                currentCursor--;
                if (currentCursor < 0)
                {
                    currentCursor = 0;
                }
            }
            // move the cursor down
            else if (InputManager.IsActionTriggered(InputManager.Action.MoveCharacterDown))
            {
                currentCursor++;
                if (currentCursor > 2)
                {
                    currentCursor = 2;
                }
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // Draw Shop Main Menu
            spriteBatch.Begin();

            // Draw Shop Main Menu Screen
            DrawMainMenu();

            // Draw Buttons
            if (IsActive)
            {
                DrawButtons();
            }

            // Measure Title of the Screen
            spriteBatch.Draw(plankTexture, plankPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // Draw the Title of the Screen
            spriteBatch.DrawString(Fonts.HeaderFont, store.Name,
                shopNamePosition, Fonts.TitleColor);

            // Draw Conversation Strip
            spriteBatch.Draw(conversationStrip, conversationStripPosition,null,Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // Draw Shop Keeper
            spriteBatch.Draw(store.ShopkeeperTexture, shopKeeperPosition,null,Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // Draw Shop Info
            spriteBatch.DrawString(Fonts.DescriptionFont,
                Fonts.BreakTextIntoLines(store.WelcomeMessage, (int)(60 * ScaledVector2.ScaleFactor),4),
                welcomeMessagePosition, Fonts.DescriptionColor);

            spriteBatch.End();
        }


        /// <summary>
        /// Draws the main menu for the store.
        /// </summary>
        private void DrawMainMenu()
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            Vector2 arrowPosition = Vector2.Zero;
            Vector2 highlightPosition = Vector2.Zero;
            Vector2 position = textPosition;

            // Draw faded screen
            spriteBatch.Draw(fadeTexture, new Vector2(screenRect.X,screenRect.Y),null, Color.White,0f,
                Vector2.Zero,ScaledVector2.DrawFactor,SpriteEffects.None,0f);

            spriteBatch.Draw(shopDrawScreen, new Vector2(screenRect.X,screenRect.Y),null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            arrowPosition.X = textPosition.X - selectionArrowOffset.X;
            arrowPosition.Y = textPosition.Y - selectionArrowOffset.Y;

            highlightPosition.X = textPosition.X - highlightItemOffset.X;
            highlightPosition.Y = textPosition.Y - highlightItemOffset.Y;

            float scaleFactor = 1.5f;
            Vector2 fixPosition = new Vector2(-20, -20);

            // "Buy" is highlighted
            if (currentCursor == 0)
            {
                spriteBatch.Draw(highlightItem, highlightPosition,null,Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                spriteBatch.Draw(selectionArrow, arrowPosition,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                spriteBatch.DrawString(Fonts.GearInfoFont, "Buy", position,
                    Fonts.HighlightColor, 0f, Vector2.Zero, scaleFactor, SpriteEffects.None, 0f);

                position.Y += interval;
                spriteBatch.DrawString(Fonts.GearInfoFont, "Sell", position,
                    Fonts.DisplayColor, 0f, Vector2.Zero, scaleFactor, SpriteEffects.None, 0f);

                position.Y += interval;
                spriteBatch.DrawString(Fonts.GearInfoFont, "Leave", position,
                    Fonts.DisplayColor, 0f, Vector2.Zero, scaleFactor, SpriteEffects.None, 0f);
            }
            // "Sell" is highlighted
            else if (currentCursor == 1)
            {
                position = textPosition;
                spriteBatch.DrawString(Fonts.GearInfoFont, "Buy", position,
                    Fonts.DisplayColor, 0f, Vector2.Zero, scaleFactor, SpriteEffects.None, 0f);

                highlightPosition.Y += interval;
                arrowPosition.Y += interval;
                position.Y += interval;

                spriteBatch.Draw(highlightItem, highlightPosition,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                spriteBatch.Draw(selectionArrow, arrowPosition,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                spriteBatch.DrawString(Fonts.GearInfoFont, "Sell", position,
                    Fonts.HighlightColor, 0f, Vector2.Zero, scaleFactor, SpriteEffects.None, 0f);
                position.Y += interval;
                spriteBatch.DrawString(Fonts.GearInfoFont, "Leave", position,
                    Fonts.DisplayColor, 0f, Vector2.Zero, scaleFactor, SpriteEffects.None, 0f);
            }
            // "Leave" is highlighted
            else if (currentCursor == 2)
            {
                position = textPosition;
                spriteBatch.DrawString(Fonts.GearInfoFont, "Buy", position,
                    Fonts.DisplayColor, 0f, Vector2.Zero, scaleFactor, SpriteEffects.None, 0f);

                position.Y += interval;
                spriteBatch.DrawString(Fonts.GearInfoFont, "Sell", position,
                    Fonts.DisplayColor, 0f, Vector2.Zero, scaleFactor, SpriteEffects.None, 0f);

                highlightPosition.Y += interval + interval;
                arrowPosition.Y += interval + interval;
                position.Y += interval;

                spriteBatch.Draw(highlightItem, highlightPosition,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                spriteBatch.Draw(selectionArrow, arrowPosition, null,Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                spriteBatch.DrawString(Fonts.GearInfoFont, "Leave", position,
                    Fonts.HighlightColor, 0f, Vector2.Zero, scaleFactor, SpriteEffects.None, 0f);
            }
        }


        /// <summary>
        /// Draws the buttons.
        /// </summary>
        private void DrawButtons()
        {
            if (!IsActive)
            {
                return;
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // Draw Gold Text
            spriteBatch.DrawString(Fonts.ButtonNamesFont,
                Fonts.GetGoldString(Session.Party.PartyGold), partyGoldPosition,
                Color.White);

            // Draw Gold Icon
            spriteBatch.Draw(goldIcon, goldIconPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
        }


        #endregion
    }
}
