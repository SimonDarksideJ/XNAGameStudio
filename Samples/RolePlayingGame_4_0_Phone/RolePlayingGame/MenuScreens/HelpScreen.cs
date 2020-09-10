#region File Description
//-----------------------------------------------------------------------------
// HelpScreen.cs
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
    /// Shows the help screen, explaining the basic game idea to the user.
    /// </summary>
    class HelpScreen : GameScreen
    {
        #region Fields


        private Texture2D backgroundTexture;

        private Texture2D plankTexture;
        private Vector2 plankPosition;
        private Vector2 titlePosition;

        private string helpText =
            "Welcome, hero!  You must meet new comrades, earn necessary " +
            "experience, gold, spells, and the equipment required to challenge " +
            "and defeat the evil Tamar, who resides in his lair, known as the " +
            "Unspoken Tower.  Be wary!  The Unspoken Tower is filled with " +
            "monstrosities that only the most hardened of heroes could possibly " +
            "face.  Good luck!";

        private List<string> textLines;

        private Texture2D scrollUpTexture;
        private readonly Vector2 scrollUpPosition = ScaledVector2.GetScaledVector(980, 200);
        private Texture2D scrollDownTexture;
        private readonly Vector2 scrollDownPosition = ScaledVector2.GetScaledVector(980, 460);

        private Texture2D lineBorderTexture;
        private readonly Vector2 linePosition = ScaledVector2.GetScaledVector(200, 570);

        private Texture2D backTexture;
        private readonly Vector2 backPosition = ScaledVector2.GetScaledVector(220, 600);

        private int startIndex;
        private const int maxLineDisplay = 8;


        #endregion


        #region Initialization

        public HelpScreen() 
            : base() 
        {
            textLines = Fonts.BreakTextIntoList(helpText, Fonts.DescriptionFont, 
                (int)(590 * ScaledVector2.ScaleFactor));
        }
        
        /// <summary>
        /// Loads the graphics content for this screen
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();

            ContentManager content = ScreenManager.Game.Content;

            backgroundTexture = content.Load<Texture2D>(@"Textures\MainMenu\MainMenu");
            plankTexture =
                content.Load<Texture2D>(@"Textures\MainMenu\MainMenuPlank03");
            backTexture =
                content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            scrollUpTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\ScrollUp");
            scrollDownTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\ScrollDown");
            lineBorderTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\LineBorder");

            plankPosition.X = backgroundTexture.Width * ScaledVector2.DrawFactor  / 2
                - plankTexture.Width *ScaledVector2.DrawFactor / 2;
            plankPosition.Y = 60 * ScaledVector2.ScaleFactor;

            titlePosition.X = plankPosition.X + (plankTexture.Width * ScaledVector2.DrawFactor -
                Fonts.HeaderFont.MeasureString("Help").X) / 2;
            titlePosition.Y = plankPosition.Y + (plankTexture.Height * ScaledVector2.DrawFactor -
                Fonts.HeaderFont.MeasureString("Help").Y) / 2;
        }


        #endregion


        #region Updating


        /// <summary>
        /// Handles user input.
        /// </summary>
        public override void HandleInput()
        {
            bool upperClicked = false;
            bool lowerClicked = false;
            bool backClicked = false;

            if (InputManager.IsButtonClicked(new Rectangle
                     ((int)scrollUpPosition.X - scrollUpTexture.Width, 
                     (int)scrollUpPosition.Y - scrollUpTexture.Height,
                     scrollUpTexture.Width * 2, scrollUpTexture.Height * 2)))
            {
                upperClicked = true; 
            }

            if (InputManager.IsButtonClicked(new Rectangle
                    ((int)scrollDownPosition.X - scrollDownTexture.Width, 
                    (int)scrollDownPosition.Y - scrollDownTexture.Height,
                    scrollDownTexture.Width * 2, scrollDownTexture.Height * 2)))
            {
                lowerClicked = true;
            }

            if (InputManager.IsButtonClicked(new Rectangle
                    ((int)backPosition.X, (int)backPosition.Y,
                    (int)(backTexture.Width * ScaledVector2.DrawFactor),
                    (int)(backTexture.Height * ScaledVector2.DrawFactor))))
            {
                backClicked = true;
            }


            // exits the screen
            if (InputManager.IsActionTriggered(InputManager.Action.Back) || backClicked)
            {
                ExitScreen();
                return;
            }
            // scroll down
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorDown)
                || lowerClicked)
            {
                // Traverse down the help text
                if (startIndex + maxLineDisplay < textLines.Count)
                {
                    startIndex += 1;
                }
            }
            // scroll up
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorUp)
                || upperClicked)
            {
                // Traverse up the help text
                if (startIndex > 0)
                {
                    startIndex -= 1;
                }
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draws the help screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, Vector2.Zero,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            spriteBatch.Draw(plankTexture, plankPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            spriteBatch.Draw(backTexture, backPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            spriteBatch.Draw(lineBorderTexture, linePosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            string text = "Back";
            Vector2 textPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, text,
                new Rectangle((int)backPosition.X, (int)backPosition.Y,
                    backTexture.Width, backTexture.Height));

            spriteBatch.DrawString(Fonts.ButtonNamesFont, text,textPosition,Color.White);

            spriteBatch.Draw(scrollUpTexture, scrollUpPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            spriteBatch.Draw(scrollDownTexture, scrollDownPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            spriteBatch.DrawString(Fonts.HeaderFont, "Help", titlePosition,
                Fonts.TitleColor);

            for (int i = 0; i < maxLineDisplay; i++)
            {
                spriteBatch.DrawString(Fonts.DescriptionFont, textLines[startIndex + i],
                    ScaledVector2.GetScaledVector(360, 200 + (Fonts.DescriptionFont.LineSpacing + 10) * i),
                    Color.Black);
            }

            spriteBatch.End();
        }


        #endregion
    }
}