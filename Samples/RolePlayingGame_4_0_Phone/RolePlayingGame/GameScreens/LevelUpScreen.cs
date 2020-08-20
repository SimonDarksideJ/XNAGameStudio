#region File Description
//-----------------------------------------------------------------------------
// LevelUpScreen.cs
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
    /// Displays all the players that have leveled up
    /// </summary>
    class LevelUpScreen : GameScreen
    {
        private int index;
        private List<Player> leveledUpPlayers;
        private List<Spell> spellList = new List<Spell>();


        #region Graphics content


        private Texture2D backTexture;
        private Texture2D selectIconTexture;
        private Texture2D portraitBackTexture;
        private Texture2D headerTexture;
        private Texture2D lineTexture;
        private Texture2D scrollUpTexture;
        private Texture2D scrollDownTexture;
        private Texture2D fadeTexture;
        private Color color;
        private Color colorName = new Color(241, 173, 10);
        private Color colorClass = new Color(207, 130, 42);
        private Color colorText = new Color(76, 49, 8);


        #endregion


        #region Positions


        private Vector2 backgroundPosition;
        private Vector2 textPosition;
        private Vector2 levelPosition;
        private Vector2 iconPosition;
        private Vector2 linePosition;
        private Vector2 selectPosition;
        private Vector2 selectIconPosition;
        private Vector2 screenSize;
        private Vector2 titlePosition;
        private Vector2 scrollUpPosition;
        private Vector2 scrollDownPosition;
        private Vector2 spellUpgradePosition;
        private Vector2 portraitPosition;
        private Vector2 playerNamePosition;
        private Vector2 playerLvlPosition;
        private Vector2 playerClassPosition;
        private Vector2 topLinePosition;
        private Vector2 playerDamagePosition;
        private Vector2 headerPosition;
        private Vector2 backPosition;
        private Rectangle fadeDest;


        #endregion


        #region Dialog Strings


        private readonly string titleText = "Level Up";
        private readonly string selectString = "Continue";


        #endregion


        #region Scrolling Text Navigation


        private int startIndex;
        private int endIndex;
        private const int maxLines = 3;
        private static readonly float lineSpacing = 74 * ScaledVector2.ScaleFactor;


        #endregion


        #region Initialization

        
        /// <summary>
        /// Constructs a new LevelUpScreen object.
        /// </summary>
        /// <param name="leveledUpPlayers"></param>
        public LevelUpScreen(List<Player> leveledUpPlayers)
        {
            if ((leveledUpPlayers == null) || (leveledUpPlayers.Count <= 0))
            {
                throw new ArgumentNullException("leveledUpPlayers");
            }

            this.IsPopup = true;
            this.leveledUpPlayers = leveledUpPlayers;

            index = 0;

            GetSpellList();

            AudioManager.PushMusic("LevelUp",false);
            this.Exiting += new EventHandler(LevelUpScreen_Exiting);
        }


        void LevelUpScreen_Exiting(object sender, EventArgs e)
        {
            AudioManager.PopMusic();
        }


        /// <summary>
        /// Load the graphics content
        /// </summary>
        /// <param name="sprite">SpriteBatch</param>
        /// <param name="screenWidth">Width of the screen</param>
        /// <param name="screenHeight">Height of the screen</param>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            backTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\PopupScreen");
            selectIconTexture =
                content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            portraitBackTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\PlayerSelected");
            headerTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\Caption");
            lineTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\SeparationLine");
            scrollUpTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\ScrollUp");
            scrollDownTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\ScrollDown");
            fadeTexture = 
                content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            backgroundPosition.X = (viewport.Width - backTexture.Width * ScaledVector2.DrawFactor ) / 2;
            backgroundPosition.Y = (viewport.Height - backTexture.Height * ScaledVector2.DrawFactor) / 2;

            screenSize = new Vector2(viewport.Width, viewport.Height);
            fadeDest = new Rectangle(0, 0, viewport.Width, viewport.Height);

            titlePosition.X = (screenSize.X -
                Fonts.HeaderFont.MeasureString(titleText).X) / 2;
            titlePosition.Y = backgroundPosition.Y + lineSpacing;

            selectIconPosition.X = screenSize.X / 2 + 185 * ScaledVector2.ScaleFactor;
            selectIconPosition.Y = backgroundPosition.Y + 520f * ScaledVector2.ScaleFactor;
            selectPosition.X = selectIconPosition.X -
                Fonts.ButtonNamesFont.MeasureString(selectString).X - 10f * ScaledVector2.ScaleFactor;
            selectPosition.Y = selectIconPosition.Y;

            portraitPosition = backgroundPosition + ScaledVector2.GetScaledVector(143f, 155f);
            backPosition = backgroundPosition + ScaledVector2.GetScaledVector(140f, 120f);

            playerNamePosition = backgroundPosition + ScaledVector2.GetScaledVector(220f, 130f);
            playerClassPosition = backgroundPosition + ScaledVector2.GetScaledVector(220f, 155f);
            playerLvlPosition = backgroundPosition + ScaledVector2.GetScaledVector(220f, 175f);

            topLinePosition = backgroundPosition + ScaledVector2.GetScaledVector(380f, 160f);
            textPosition = backgroundPosition + ScaledVector2.GetScaledVector(335f, 0);
            levelPosition = backgroundPosition + ScaledVector2.GetScaledVector(540f, 320f);
            iconPosition = backgroundPosition + ScaledVector2.GetScaledVector(155f, 303f);
            linePosition = backgroundPosition + ScaledVector2.GetScaledVector(142f, 285f);

            scrollUpPosition = backgroundPosition + ScaledVector2.GetScaledVector(810f, 300f);
            scrollDownPosition = backgroundPosition + ScaledVector2.GetScaledVector(810f, 480f);

            playerDamagePosition = backgroundPosition + ScaledVector2.GetScaledVector(560f, 130f);
            spellUpgradePosition = backgroundPosition + ScaledVector2.GetScaledVector(380f, 250f);

            headerPosition = backgroundPosition + ScaledVector2.GetScaledVector(120f, 248f);
        }


        #endregion


        #region Updating


        /// <summary>
        /// Handles user input.
        /// </summary>
        public override void HandleInput()
        {
            bool okClicked = false;
            if (InputManager.IsButtonClicked(new Rectangle(
                (int)selectIconPosition.X, (int)selectIconPosition.Y,
                (int)(selectIconTexture.Width * ScaledVector2.DrawFactor),
                (int)(selectIconTexture.Height * ScaledVector2.DrawFactor))))
            {
                okClicked = true;
            }

            // exit without bothering to see the rest
            if (InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                ExitScreen();
            }
            // advance to the next player to have leveled up
            else if (okClicked)
            {
                if (leveledUpPlayers.Count <= 0)
                {
                    // no players at all
                    ExitScreen();
                    return;
                }
                if (index < leveledUpPlayers.Count - 1)
                {
                    // move to the next player
                    index++;
                    GetSpellList();
                }
                else
                {
                    // no more players
                    ExitScreen();
                    return;
                }
            }
            // Scroll up
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorUp))
            {
                if (startIndex > 0)
                {
                    startIndex--;
                    endIndex--;
                }
            }
            // Scroll down
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorDown))
            {
                if (startIndex < spellList.Count - maxLines)
                {
                    endIndex++;
                    startIndex++;
                }
            }
        }


        /// <summary>
        /// Get the spell list
        /// </summary>
        private void GetSpellList()
        {
            spellList.Clear();

            if ((leveledUpPlayers.Count > 0) &&
                (leveledUpPlayers[index].CharacterLevel <=
                    leveledUpPlayers[index].CharacterClass.LevelEntries.Count))
            {
                List<Spell> newSpells = 
                    leveledUpPlayers[index].CharacterClass.LevelEntries[
                        leveledUpPlayers[index].CharacterLevel - 1].Spells;
                if ((newSpells == null) || (newSpells.Count <= 0))
                {
                    startIndex = 0;
                    endIndex = 0;
                }
                else
                {
                    spellList.AddRange(leveledUpPlayers[index].Spells);
                    spellList.RemoveAll(delegate(Spell spell)
                    {
                        return !newSpells.Exists(delegate(Spell newSpell)
                        {
                            return spell.AssetName == newSpell.AssetName;
                        });
                    });
                    startIndex = 0;
                    endIndex = Math.Min(maxLines, spellList.Count);
                }
            }
            else
            {
                startIndex = 0;
                endIndex = 0;
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Vector2 currentTextPosition = textPosition;
            Vector2 currentIconPosition = iconPosition;
            Vector2 currentLinePosition = linePosition;
            Vector2 currentLevelPosition = levelPosition;

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw the fading screen
            spriteBatch.Draw(fadeTexture, new Rectangle(0,0,
                ScreenManager.GraphicsDevice.Viewport.Width,
                ScreenManager.GraphicsDevice.Viewport.Height),Color.White);

            // Draw the popup background
            spriteBatch.Draw(backTexture, backgroundPosition, null,Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // Draw the title
            spriteBatch.DrawString(Fonts.HeaderFont, titleText, titlePosition,
                Fonts.TitleColor);

            DrawPlayerStats();

            // Draw the spell upgrades caption
            spriteBatch.Draw(headerTexture, headerPosition, null,Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            spriteBatch.DrawString(Fonts.PlayerNameFont, "Spell Upgrades",
                spellUpgradePosition, colorClass);

            // Draw the horizontal separating lines
            for (int i = 0; i <= maxLines - 1; i++)
            {
                currentLinePosition.Y += lineSpacing;
                spriteBatch.Draw(lineTexture, currentLinePosition,null, Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            }

            // Draw the spell upgrade details
            for (int i = startIndex; i < endIndex; i++)
            {
                // Draw the spell icon
                spriteBatch.Draw(spellList[i].IconTexture, currentIconPosition,null,Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

                // Draw the spell name
                spriteBatch.DrawString(Fonts.GearInfoFont, spellList[i].Name,
                    currentTextPosition, Fonts.CountColor);

                // Draw the spell level
                spriteBatch.DrawString(Fonts.GearInfoFont, "Spell Level " +
                    spellList[i].Level.ToString(),
                    currentLevelPosition, Fonts.CountColor);

                // Increment to next line position
                currentTextPosition.Y += lineSpacing;
                currentLevelPosition.Y += lineSpacing;
                currentIconPosition.Y += lineSpacing;
            }

            // Draw the scroll bars
            spriteBatch.Draw(scrollUpTexture, scrollUpPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            spriteBatch.Draw(scrollDownTexture, scrollDownPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // Draw the select button and its corresponding text
            spriteBatch.Draw(selectIconTexture, selectIconPosition,null, Color.White,0f,
                Vector2.Zero,ScaledVector2.DrawFactor,SpriteEffects.None,0f); 

            Vector2 selectTextPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, selectString,
                    new Rectangle((int)selectIconPosition.X, (int)selectIconPosition.Y,
                        selectIconTexture.Width, selectIconTexture.Height));

            spriteBatch.DrawString(Fonts.ButtonNamesFont, selectString, selectTextPosition,Color.White);
            spriteBatch.End();
        }


        /// <summary>
        /// Draw the player stats here
        /// </summary>
        private void DrawPlayerStats()
        {
            Vector2 position = topLinePosition;
            Vector2 posDamage = playerDamagePosition;
            Player player = leveledUpPlayers[index];
            int level = player.CharacterLevel;
            CharacterLevelingStatistics levelingStatistics =
                player.CharacterClass.LevelingStatistics;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // Draw the portrait            
            spriteBatch.Draw(portraitBackTexture, backPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            spriteBatch.Draw(player.ActivePortraitTexture, portraitPosition,null,Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // Print the character name
            spriteBatch.DrawString(Fonts.PlayerNameFont,
                player.Name, playerNamePosition, colorName);

            // Draw the Class Name
            spriteBatch.DrawString(Fonts.PlayerNameFont,
                player.CharacterClass.Name, playerClassPosition, colorClass);

            // Draw the character level
            spriteBatch.DrawString(Fonts.PlayerNameFont, "LEVEL: " +
                level.ToString(), playerLvlPosition, Color.Gray);

            // Draw the character Health Points
            SetColor(levelingStatistics.LevelsPerHealthPointsIncrease == 0 ? 0 :
                (level % levelingStatistics.LevelsPerHealthPointsIncrease) *
                levelingStatistics.HealthPointsIncrease);
            spriteBatch.DrawString(Fonts.PlayerStatisticsFont, "HP: " +
                player.CurrentStatistics.HealthPoints + "/" +
                player.CharacterStatistics.HealthPoints,
                position, color);

            // Draw the character Mana Points
            position.Y += Fonts.GearInfoFont.LineSpacing;
            SetColor(levelingStatistics.LevelsPerMagicPointsIncrease == 0 ? 0 :
                (level % levelingStatistics.LevelsPerMagicPointsIncrease) *
                levelingStatistics.MagicPointsIncrease);
            spriteBatch.DrawString(Fonts.PlayerStatisticsFont, "MP: " +
                player.CurrentStatistics.MagicPoints + "/" +
                player.CharacterStatistics.MagicPoints,
                position, color);

            // Draw the physical offense
            SetColor(levelingStatistics.LevelsPerPhysicalOffenseIncrease == 0 ? 0 :
                (level % levelingStatistics.LevelsPerPhysicalOffenseIncrease) *
                levelingStatistics.PhysicalOffenseIncrease);
            spriteBatch.DrawString(Fonts.PlayerStatisticsFont, "PO: " +
                player.CurrentStatistics.PhysicalOffense, posDamage, color);

            // Draw the physical defense
            posDamage.Y += Fonts.PlayerStatisticsFont.LineSpacing;
            SetColor(levelingStatistics.LevelsPerPhysicalDefenseIncrease == 0 ? 0 :
                (level % levelingStatistics.LevelsPerPhysicalDefenseIncrease) *
                levelingStatistics.PhysicalDefenseIncrease);
            spriteBatch.DrawString(Fonts.PlayerStatisticsFont, "PD: " +
                player.CurrentStatistics.PhysicalDefense, posDamage, color);

            // Draw the Magic offense
            posDamage.Y += Fonts.PlayerStatisticsFont.LineSpacing;
            SetColor(levelingStatistics.LevelsPerMagicalOffenseIncrease == 0 ? 0 :
                (level % levelingStatistics.LevelsPerMagicalOffenseIncrease) *
                levelingStatistics.MagicalOffenseIncrease);
            spriteBatch.DrawString(Fonts.PlayerStatisticsFont, "MO: " +
                player.CurrentStatistics.MagicalOffense, posDamage, color);

            // Draw the Magical defense
            posDamage.Y += Fonts.PlayerStatisticsFont.LineSpacing;
            SetColor(levelingStatistics.LevelsPerMagicalDefenseIncrease == 0 ? 0 :
                (level % levelingStatistics.LevelsPerMagicalDefenseIncrease) *
                levelingStatistics.MagicalDefenseIncrease);
            spriteBatch.DrawString(Fonts.PlayerStatisticsFont, "MD: " +
                player.CurrentStatistics.MagicalDefense, posDamage, color);
        }


        /// <summary>
        /// Set the current color based on whether the value has changed.
        /// </summary>
        /// <param name="change">State of levelled up values</param>
        public void SetColor(int value)
        {
            if (value > 0)
            {
                color = Color.Green;
            }
            else if (value < 0)
            {
                color = Color.Red;
            }
            else
            {
                color = colorText;
            }
        }


        #endregion
    }
}