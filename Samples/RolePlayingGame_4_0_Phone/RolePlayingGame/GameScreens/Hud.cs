#region File Description
//-----------------------------------------------------------------------------
// Hud.cs
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
    /// Displays each player's basic statistics and the combat action menu.
    /// </summary>
    class Hud
    {
        private ScreenManager screenManager;

        public static float HudHeight;//; = 183 * ScaledVector2.ScaleFactor;

        private Rectangle firstCombatMenuPosition = Rectangle.Empty;
        private float heightInterval = 65f * ScaledVector2.ScaleFactor;

        public static bool IsActive { get; set; }

        #region Graphics Data


        private Texture2D backgroundHudTexture;
        private Texture2D topHudTexture;
        private Texture2D combatPopupTexture;
        private Texture2D activeCharInfoTexture;
        private Texture2D inActiveCharInfoTexture;
        private Texture2D cantUseCharInfoTexture;
        private Texture2D selectionBracketTexture;
        private Texture2D menuTexture;
        private Texture2D statsTexture;
        private Texture2D deadPortraitTexture;
        private Texture2D charSelFadeLeftTexture;
        private Texture2D charSelFadeRightTexture;
        private Texture2D charSelArrowLeftTexture;
        private Texture2D charSelArrowRightTexture;
        private Texture2D actionTexture;
        private Texture2D yButtonTexture;
        private Texture2D startButtonTexture;

        private Vector2 topHudPosition = ScaledVector2.GetScaledVector(353f, 30f);
        private Vector2 charSelLeftPosition = ScaledVector2.GetScaledVector(70f, 600f);
        private Vector2 charSelRightPosition = ScaledVector2.GetScaledVector(1170f, 600f);
        private Vector2 yButtonPosition = ScaledVector2.GetScaledVector(0f, 560f + 70f);
        private Vector2 startButtonPosition = ScaledVector2.GetScaledVector(0f, 560f + 35f);
        private Vector2 yTextPosition = ScaledVector2.GetScaledVector(0f, 560f + 130f);
        private Vector2 startTextPosition = ScaledVector2.GetScaledVector(0f, 560f + 70f);
        private Vector2 actionTextPosition = ScaledVector2.GetScaledVector(640f, 55f);
        private Vector2 backgroundHudPosition;// = ScaledVector2.GetScaledVector(0f, 525f);
        private Vector2 portraitPosition = ScaledVector2.GetScaledVector(640f, 55f);
        private Vector2 startingInfoPosition;// = ScaledVector2.GetScaledVector(0f, 550f);
        private Vector2 namePosition;
        private Vector2 levelPosition;
        private Vector2 detailPosition;

        private readonly Color activeNameColor = new Color(200, 200, 200);
        private readonly Color inActiveNameColor = new Color(100, 100, 100);
        private readonly Color nonSelColor = new Color(86, 26, 5);
        private readonly Color selColor = new Color(229, 206, 144);


        #endregion


        #region Action Text


        /// <summary>
        /// The text that is shown in the action bar at the top of the combat screen.
        /// </summary>
        private string actionText = String.Empty;

        /// <summary>
        /// The text that is shown in the action bar at the top of the combat screen.
        /// </summary>
        public string ActionText
        {
            get { return actionText; }
            set { actionText = value; }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Creates a new Hud object using the given ScreenManager.
        /// </summary>
        public Hud(ScreenManager screenManager)
        {
            // check the parameter
            if (screenManager == null)
            {
                throw new ArgumentNullException("screenManager");
            }
            this.screenManager = screenManager;

            IsActive = true;
        }
        

        /// <summary>
        /// Load the graphics content from the content manager.
        /// </summary>
        public void LoadContent()
        {
            ContentManager content = (screenManager.Game as RolePlayingGame).StaticContent;

            backgroundHudTexture = 
                content.Load<Texture2D>(@"Textures\HUD\HudBkgd");
            topHudTexture = 
                content.Load<Texture2D>(@"Textures\HUD\CombatStateInfoStrip");
            activeCharInfoTexture =
                content.Load<Texture2D>(@"Textures\HUD\PlankActive");
            inActiveCharInfoTexture =
                content.Load<Texture2D>(@"Textures\HUD\PlankInActive");
            cantUseCharInfoTexture = 
                content.Load<Texture2D>(@"Textures\HUD\PlankCantUse");
            selectionBracketTexture = 
                content.Load<Texture2D>(@"Textures\HUD\SelectionBrackets");
            deadPortraitTexture = 
                content.Load<Texture2D>(@"Textures\Characters\Portraits\Tombstone");
            combatPopupTexture = 
                content.Load<Texture2D>(@"Textures\HUD\CombatPopup");
            charSelFadeLeftTexture =
                content.Load<Texture2D>(@"Textures\Buttons\CharSelectFadeLeft");
            charSelFadeRightTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\CharSelectFadeRight");
            charSelArrowLeftTexture =
                content.Load<Texture2D>(@"Textures\Buttons\CharSelectHlLeft");
            charSelArrowRightTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\CharSelectHlRight");
            actionTexture = 
                content.Load<Texture2D>(@"Textures\HUD\HudSelectButton");
            yButtonTexture =
                content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            startButtonTexture = 
                content.Load<Texture2D>(@"Textures\Buttons\StartButton");
            menuTexture = 
                content.Load<Texture2D>(@"Textures\HUD\Menu");
            statsTexture =
                content.Load<Texture2D>(@"Textures\HUD\Stats");

            HudHeight = backgroundHudTexture.Height * ScaledVector2.DrawFactor;
            backgroundHudPosition = new Vector2(0, screenManager.GraphicsDevice.Viewport.Height - HudHeight);
            startingInfoPosition = new Vector2(backgroundHudPosition.X, backgroundHudPosition.Y + 30);

        }


        #endregion

        public void HandleInput()
        {
            if (!CombatEngine.IsActive)
            {
                if (InputManager.IsButtonClicked(new Rectangle
                    ((int)yButtonPosition.X,
                    (int)yButtonPosition.Y,
                    yButtonTexture.Width  , yButtonTexture.Height)))
                {
                    if (StatClicked != null)
                    {
                        StatClicked(this, EventArgs.Empty);
                    }
                }
            }
        }

        public event EventHandler StatClicked;

        #region Drawing


        /// <summary>
        /// Draw the screen.
        /// </summary>
        public void Draw()
        {
            if (!Hud.IsActive)
            {
                return;
            }
            SpriteBatch spriteBatch = screenManager.SpriteBatch;

            spriteBatch.Begin();
            startingInfoPosition.X = 640f * ScaledVector2.ScaleFactor;

            startingInfoPosition.X -= (Session.Party.Players.Count / 2 * 200f) * ScaledVector2.ScaleFactor;
            if (Session.Party.Players.Count % 2 != 0)
            {
                startingInfoPosition.X -= 100f * ScaledVector2.ScaleFactor;
            } 

            spriteBatch.Draw(backgroundHudTexture, backgroundHudPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor , SpriteEffects.None, 0f);

            if (CombatEngine.IsActive)
            {
                DrawForCombat();
            }
            else
            {
                DrawForNonCombat();
            }

            spriteBatch.End();
        }


        public static Dictionary<CombatantPlayer, Vector2> PlayerPositionMapping = 
            new Dictionary<CombatantPlayer, Vector2>();
        /// <summary>
        /// Draws HUD for Combat Mode
        /// </summary>
        private void DrawForCombat()
        {
            SpriteBatch spriteBatch = screenManager.SpriteBatch;
            Vector2 position = startingInfoPosition;
            Hud.PlayerPositionMapping.Clear();
            foreach (CombatantPlayer combatantPlayer in CombatEngine.Players)
            {
                Hud.PlayerPositionMapping.Add(combatantPlayer , position);
                DrawCombatPlayerDetails(combatantPlayer, position);
                position.X += activeCharInfoTexture.Width * ScaledVector2.DrawFactor - 6f * ScaledVector2.ScaleFactor;
            }

            charSelLeftPosition.X = startingInfoPosition.X - 5f * ScaledVector2.ScaleFactor - 
                charSelArrowLeftTexture.Width * ScaledVector2.DrawFactor;
            charSelRightPosition.X = position.X + 5f * ScaledVector2.ScaleFactor;
            // Draw character Selection Arrows
            if (CombatEngine.IsPlayersTurn)
            {
                spriteBatch.Draw(charSelArrowLeftTexture, charSelLeftPosition,null,Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                spriteBatch.Draw(charSelArrowRightTexture, charSelRightPosition,null,Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(charSelFadeLeftTexture, charSelLeftPosition,null,Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                spriteBatch.Draw(charSelFadeRightTexture, charSelRightPosition, null,Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            }

            if (actionText.Length > 0)
            {
                spriteBatch.Draw(topHudTexture, topHudPosition,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                // Draw Action Text
                Fonts.DrawCenteredText(spriteBatch, Fonts.PlayerStatisticsFont,
                    actionText, actionTextPosition, Color.Black);
            }
        }


        /// <summary>
        /// Draws HUD for non Combat Mode
        /// </summary>
        private void DrawForNonCombat()
        {
            SpriteBatch spriteBatch = screenManager.SpriteBatch;

            Vector2 position = startingInfoPosition;

            foreach (Player player in Session.Party.Players)
            {
                DrawNonCombatPlayerDetails(player, position);

                position.X += (inActiveCharInfoTexture.Width * ScaledVector2.DrawFactor)  - 6f 
                    * ScaledVector2.ScaleFactor;
            }

            yTextPosition.X = position.X + 115f * ScaledVector2.ScaleFactor;
            yButtonPosition.X = position.X + 120f * ScaledVector2.ScaleFactor;


            spriteBatch.Draw(yButtonTexture, yButtonPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            Vector2 statePosition = new Vector2(
                yButtonPosition.X + yButtonTexture.Width * 2f / 2 - statsTexture.Width * 2 / 2,
                yButtonPosition.Y + yButtonTexture.Height * 2f / 2 - statsTexture.Height * 2 / 2);

            // Draw Select Button
            spriteBatch.Draw(statsTexture, statePosition - new Vector2(26,10), null, Color.White, 0f,
                Vector2.Zero, new Vector2(2f, ScaledVector2.DrawFactor), SpriteEffects.None, 0f);

            startTextPosition.X = startingInfoPosition.X - 
                (startButtonTexture.Width * ScaledVector2.DrawFactor)  - 25f * ScaledVector2.ScaleFactor;
            startButtonPosition.X = startingInfoPosition.X - 
                (startButtonTexture.Width * ScaledVector2.DrawFactor) - 10f * ScaledVector2.ScaleFactor;

        }


        enum PlankState
        {
            Active,
            InActive,
            CantUse,
        }


        /// <summary>
        /// Draws Player Details
        /// </summary>
        /// <param name="playerIndex">Index of player details to draw</param>
        /// <param name="position">Position where to draw</param>
        private void DrawCombatPlayerDetails(CombatantPlayer player, Vector2 position)
        {
            SpriteBatch spriteBatch = screenManager.SpriteBatch;

            PlankState plankState;
            bool isPortraitActive = false;
            bool isCharDead = false;
            Color color;

            portraitPosition.X = position.X + 7f * ScaledVector2.ScaleFactor;
            portraitPosition.Y = position.Y + 7f * ScaledVector2.ScaleFactor;

            namePosition.X = position.X + 84f * ScaledVector2.ScaleFactor;
            namePosition.Y = position.Y + 12f * ScaledVector2.ScaleFactor;

            levelPosition.X = position.X + 84f * ScaledVector2.ScaleFactor;
            levelPosition.Y = position.Y + 39f * ScaledVector2.ScaleFactor;

            detailPosition.X = position.X + 25f * ScaledVector2.ScaleFactor;
            detailPosition.Y = position.Y + 66f * ScaledVector2.ScaleFactor;

            position.X -= 2 * ScaledVector2.ScaleFactor;
            position.Y -= 4 * ScaledVector2.ScaleFactor;

            if (player.IsTurnTaken)
            {
                plankState = PlankState.CantUse;

                isPortraitActive = false;
            }
            else
            {
                plankState = PlankState.InActive;

                isPortraitActive = true;
            }

            if (((CombatEngine.HighlightedCombatant == player) && !player.IsTurnTaken) ||
                (CombatEngine.PrimaryTargetedCombatant == player) ||
                (CombatEngine.SecondaryTargetedCombatants.Contains(player)))
            {
                plankState = PlankState.Active;
            }

            if (player.IsDeadOrDying)
            {
                isCharDead = true;
                isPortraitActive = false;
                plankState = PlankState.CantUse;
            }

            // Draw Info Slab
            if (plankState == PlankState.Active)
            {
                color = activeNameColor;

                spriteBatch.Draw(activeCharInfoTexture, position,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

                // Draw Brackets
                if ((CombatEngine.HighlightedCombatant == player) && !player.IsTurnTaken)
                {
                    spriteBatch.Draw(selectionBracketTexture, position,null, Color.White,0f,
                        Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                }

                if (isPortraitActive &&
                    (CombatEngine.HighlightedCombatant == player) &&
                    (CombatEngine.HighlightedCombatant.CombatAction == null) &&
                    !CombatEngine.IsDelaying)
                {
                    position.X += (activeCharInfoTexture.Width * ScaledVector2.DrawFactor ) / 2;
                    position.X -= (combatPopupTexture.Width * ScaledVector2.DrawFactor) / 2;
                    position.Y -= (combatPopupTexture.Height * ScaledVector2.DrawFactor);
                    // Draw Action
                    DrawActionsMenu(position);
                }
            }
            else if (plankState == PlankState.InActive)
            {
                color = inActiveNameColor;
                spriteBatch.Draw(inActiveCharInfoTexture, position,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            }
            else
            {
                color = Color.Black;
                spriteBatch.Draw(cantUseCharInfoTexture, position,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            }

            if (isCharDead)
            {
                spriteBatch.Draw(deadPortraitTexture, portraitPosition,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            }
            else
            {
                // Draw Player Portrait
                DrawPortrait(player.Player, portraitPosition, plankState);
            }

            // Draw Player Name
            spriteBatch.DrawString(Fonts.PlayerStatisticsFont,
                player.Player.Name,
                namePosition, color);

            color = Color.Black;
            // Draw Player Details
            spriteBatch.DrawString(Fonts.HudDetailFont,
                "Lvl: " + player.Player.CharacterLevel,
                levelPosition, color);

            spriteBatch.DrawString(Fonts.HudDetailFont,
                "HP: " + player.Statistics.HealthPoints +
                "/" + player.Player.CharacterStatistics.HealthPoints,
                detailPosition, color);

            detailPosition.Y += 30f * ScaledVector2.ScaleFactor;
            spriteBatch.DrawString(Fonts.HudDetailFont,
                "MP: " + player.Statistics.MagicPoints +
                "/" + player.Player.CharacterStatistics.MagicPoints,
                detailPosition, color);
        }


        /// <summary>
        /// Draws Player Details
        /// </summary>
        /// <param name="playerIndex">Index of player details to draw</param>
        /// <param name="position">Position where to draw</param>
        private void DrawNonCombatPlayerDetails(Player player, Vector2 position)
        {
            SpriteBatch spriteBatch = screenManager.SpriteBatch;

            PlankState plankState;
            bool isCharDead = false;
            Color color;

            portraitPosition.X = position.X + 7f * ScaledVector2.ScaleFactor;
            portraitPosition.Y = position.Y + 7f * ScaledVector2.ScaleFactor;

            namePosition.X = position.X + 84f * ScaledVector2.ScaleFactor;
            namePosition.Y = position.Y + 12f * ScaledVector2.ScaleFactor;

            levelPosition.X = position.X + 84f * ScaledVector2.ScaleFactor;
            levelPosition.Y = position.Y + 39f * ScaledVector2.ScaleFactor;

            detailPosition.X = position.X + 25f * ScaledVector2.ScaleFactor;
            detailPosition.Y = position.Y + 66f * ScaledVector2.ScaleFactor;

            position.X -= 2 * ScaledVector2.ScaleFactor;
            position.Y -= 4 * ScaledVector2.ScaleFactor;

            plankState = PlankState.Active;

            // Draw Info Slab
            if (plankState == PlankState.Active)
            {
                color = activeNameColor;

                spriteBatch.Draw(activeCharInfoTexture, position,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            }
            else if (plankState == PlankState.InActive)
            {
                color = inActiveNameColor;
                spriteBatch.Draw(inActiveCharInfoTexture, position,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            }
            else
            {
                color = Color.Black;
                spriteBatch.Draw(cantUseCharInfoTexture, position,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            }

            if (isCharDead)
            {
                spriteBatch.Draw(deadPortraitTexture, portraitPosition,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            }
            else
            {
                // Draw Player Portrait
                DrawPortrait(player, portraitPosition, plankState);
            }

            // Draw Player Name
            spriteBatch.DrawString(Fonts.PlayerStatisticsFont,
                player.Name,
                namePosition, color);

            color = Color.Black;
            // Draw Player Details
            spriteBatch.DrawString(Fonts.HudDetailFont,
                "Lvl: " + player.CharacterLevel,
                levelPosition, color);

            spriteBatch.DrawString(Fonts.HudDetailFont,
                "HP: " + player.CurrentStatistics.HealthPoints +
                "/" + player.CharacterStatistics.HealthPoints,
                detailPosition, color);

            detailPosition.Y += 30f * ScaledVector2.ScaleFactor;
            spriteBatch.DrawString(Fonts.HudDetailFont,
                "MP: " + player.CurrentStatistics.MagicPoints +
                "/" + player.CharacterStatistics.MagicPoints,
                detailPosition, color);
        }


        /// <summary>
        /// Draw the portrait of the given player at the given position.
        /// </summary>
        private void DrawPortrait(Player player, Vector2 position, 
            PlankState plankState)
        {
            switch (plankState)
            {
                case PlankState.Active:
                    screenManager.SpriteBatch.Draw(player.ActivePortraitTexture, position,null, Color.White,0f,
                        Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                    break;
                case PlankState.InActive:
                    screenManager.SpriteBatch.Draw(player.InactivePortraitTexture, position,null, Color.White,0f,
                        Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                    break;
                case PlankState.CantUse:
                    screenManager.SpriteBatch.Draw(player.UnselectablePortraitTexture, position,null, Color.White,0f,
                        Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                    break;
            }
        }
        
        
        #endregion


        #region Combat Action Menu


        /// <summary>
        /// The list of entries in the combat action menu.
        /// </summary>
        private string[] actionList = new string[5]
            {
                "Attack",
                "Spell",
                "Item",
                "Defend",
                "Flee",
            };


        /// <summary>
        /// The currently highlighted item.
        /// </summary>
        private int highlightedAction = 0;



        /// <summary>
        /// Handle user input to the actions menu.
        /// </summary>
        public void UpdateActionsMenu()
        {
            bool isMenuItemPressed = false;
            if (firstCombatMenuPosition != Rectangle.Empty)
            {
                int x = firstCombatMenuPosition.X;
                for (int playerCount = 0; playerCount < CombatEngine.Players.Count; playerCount++)
                {
                    for (int actionIndex = 0; actionIndex < actionList.Length; actionIndex++)
                    {
                        float yPosition = firstCombatMenuPosition.Y;
                        if (actionIndex + 1 > 1)
                        {
                            yPosition = yPosition + (heightInterval * actionIndex + 1);
                        }
                        Rectangle currentActionPosition = new Rectangle(x, (int)yPosition,
                            (int)(firstCombatMenuPosition.Width * ScaledVector2.ScaleFactor),
                            (int)(firstCombatMenuPosition.Height * ScaledVector2.ScaleFactor));
                        if (InputManager.IsButtonClicked(currentActionPosition))
                        {
                            highlightedAction = actionIndex;
                            isMenuItemPressed = true;
                            break;
                        }
                    }
                    x += (int)(activeCharInfoTexture.Width * ScaledVector2.DrawFactor - 6f * ScaledVector2.ScaleFactor);
                }
            }
          
            // select an action
            if (isMenuItemPressed)
            {
                switch (actionList[highlightedAction])
                {
                    case "Attack":
                        {
                            ActionText = "Performing a Melee Attack";
                            CombatEngine.HighlightedCombatant.CombatAction =
                                new MeleeCombatAction(CombatEngine.HighlightedCombatant);
                            CombatEngine.HighlightedCombatant.CombatAction.Target =
                                CombatEngine.FirstEnemyTarget;
                        }
                        break;

                    case "Spell":
                        {
                            SpellbookScreen spellbookScreen = new SpellbookScreen(
                                CombatEngine.HighlightedCombatant.Character,
                                CombatEngine.HighlightedCombatant.Statistics);
                            spellbookScreen.SpellSelected +=
                                new SpellbookScreen.SpellSelectedHandler(
                                spellbookScreen_SpellSelected);
                            Session.ScreenManager.AddScreen(spellbookScreen);
                        }
                        break;

                    case "Item":
                        {
                            InventoryScreen inventoryScreen = new InventoryScreen(true);
                            inventoryScreen.GearSelected +=
                                new InventoryScreen.GearSelectedHandler(
                                inventoryScreen_GearSelected);
                            Session.ScreenManager.AddScreen(inventoryScreen);
                        }
                        break;

                    case "Defend":
                        {
                            ActionText = "Defending";
                            CombatEngine.HighlightedCombatant.CombatAction =
                                new DefendCombatAction(
                                CombatEngine.HighlightedCombatant);
                            CombatEngine.HighlightedCombatant.CombatAction.Start();
                        }
                        break;

                    case "Flee":
                        CombatEngine.AttemptFlee();
                        break;
                }
                return;
            }
        }


        /// <summary>
        /// Recieves the spell from the Spellbook screen and casts it.
        /// </summary>
        void spellbookScreen_SpellSelected(Spell spell)
        {
            if (spell != null)
            {
                ActionText = "Casting " + spell.Name;
                CombatEngine.HighlightedCombatant.CombatAction =
                    new SpellCombatAction(CombatEngine.HighlightedCombatant, spell);
                if (spell.IsOffensive)
                {
                    CombatEngine.HighlightedCombatant.CombatAction.Target =
                        CombatEngine.FirstEnemyTarget;
                }
                else
                {
                    CombatEngine.HighlightedCombatant.CombatAction.Target =
                        CombatEngine.HighlightedCombatant;
                }
            }
        }


        /// <summary>
        /// Receives the item back from the Inventory screen and uses it.
        /// </summary>
        void inventoryScreen_GearSelected(Gear gear)
        {
            Item item = gear as Item;
            if (item != null)
            {
                ActionText = "Using " + item.Name;
                CombatEngine.HighlightedCombatant.CombatAction =
                    new ItemCombatAction(CombatEngine.HighlightedCombatant, item);
                if (item.IsOffensive)
                {
                    CombatEngine.HighlightedCombatant.CombatAction.Target =
                        CombatEngine.FirstEnemyTarget;
                }
                else
                {
                    CombatEngine.HighlightedCombatant.CombatAction.Target =
                        CombatEngine.HighlightedCombatant;
                }
            }
        }


        /// <summary>
        /// Draws the combat action menu.
        /// </summary>
        /// <param name="position">The position of the menu.</param>
        private void DrawActionsMenu(Vector2 position)
        {
            ActionText = "Choose an Action";

            SpriteBatch spriteBatch = screenManager.SpriteBatch;

            Vector2 arrowPosition;
      

            //position.Y = position.Y - combatPopupTexture.Height;
            spriteBatch.Draw(combatPopupTexture,position,null, Color.White,0f,
            Vector2.Zero,ScaledVector2.DrawFactor  ,SpriteEffects.None,0f);

            position.Y += 18f * ScaledVector2.ScaleFactor;
            arrowPosition = position;

            arrowPosition.X += 4f * ScaledVector2.ScaleFactor;
            arrowPosition.Y += 2f * ScaledVector2.ScaleFactor;
            arrowPosition.Y += heightInterval * (int)highlightedAction;
            spriteBatch.Draw(actionTexture, arrowPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor * 2f, SpriteEffects.None, 0f);

            position.Y += 4f * ScaledVector2.ScaleFactor;
            position.X += 50f * ScaledVector2.ScaleFactor;

            if(firstCombatMenuPosition == Rectangle.Empty)
            {
                firstCombatMenuPosition = new Rectangle((int)position.X , (int)position.Y,
                    (int)(actionTexture.Width * ScaledVector2.DrawFactor), (int)heightInterval);
            }

            position.X += 18;
            // Draw Action Text
            for (int i = 0; i < actionList.Length; i++)
            {
                spriteBatch.DrawString(Fonts.GearInfoFont, actionList[i], position,
                    i == highlightedAction ? selColor : nonSelColor,0f,Vector2.Zero,1.75f,SpriteEffects.None,0f);
                position.Y += heightInterval;
            }
        }


        #endregion
    }
}
