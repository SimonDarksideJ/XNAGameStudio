#region File Description
//-----------------------------------------------------------------------------
// ScreenLevel.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion



namespace ShipGame
{
    public class ScreenLevel : Screen
    {
        ScreenManager screenManager;    // screen manager
        GameManager gameManager;         // game manager

        const int NumberLevels = 2;   // number of available levels to choose from

        // name for each level
        String[] levels = new String[NumberLevels] { "level1", "level2" };

        // screen shot for each level
        Texture2D[] levelShots = new Texture2D[NumberLevels];

        Texture2D selectBack;     // select and back texture
        Texture2D changeLevel;    // change level texture

        int selection = 0;

        // constructor
        public ScreenLevel(ScreenManager manager, GameManager game)
        {
            screenManager = manager;
            gameManager = game;
        }

        // called before screen shows
        public override void SetFocus(ContentManager content, bool focus)
        {
            // if getting focus
            if (focus)
            {
                // load all resources
                for (int i = 0; i < NumberLevels; i++)
                    levelShots[i] = content.Load<Texture2D>(
                            "screens/" + levels[i] + "_screen");
                selectBack = content.Load<Texture2D>(
                            "screens/select_back");
                changeLevel = content.Load<Texture2D>(
                            "screens/change_level");
            }
            else // loosing focus
            {
                // free all resources
                for (int i = 0; i < NumberLevels; i++)
                    levelShots[i] = null;
                selectBack = null;
                changeLevel = null;
            }
        }

        public override void ProcessInput(float elapsedTime, InputManager input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            int i, j = (int)gameManager.GameMode;
            for (i = 0; i < j; i++)
            {
                // select
                if (input.IsKeyPressed(i, Keys.Enter) || input.IsButtonPressedA(i))
                {
                    gameManager.SetLevel(levels[selection]);
                    screenManager.SetNextScreen(ScreenType.ScreenGame);
                    gameManager.PlaySound("menu_select");
                }

                // cancel
                if (input.IsKeyPressed(i, Keys.Escape) || input.IsButtonPressedB(i))
                {
                    gameManager.SetLevel(null);
                    screenManager.SetNextScreen(ScreenType.ScreenPlayer);
                    gameManager.PlaySound("menu_cancel");
                }

                // change selection (previous)
                if (input.IsKeyPressed(i, Keys.Left) ||
                    input.IsButtonPressedDPadLeft(i) ||
                    input.IsButtonPressedLeftStickLeft(i))
                {
                    if (selection == 0)
                        selection = levels.GetLength(0) - 1;
                    else
                        selection = selection - 1;
                    gameManager.PlaySound("menu_change");
                }

                // change selection (next)
                if (input.IsKeyPressed(i, Keys.Right) ||
                    input.IsButtonPressedDPadRight(i) ||
                    input.IsButtonPressedLeftStickRight(i))
                {
                    selection = (selection + 1) % levels.GetLength(0);
                    gameManager.PlaySound("menu_change");
                }
            }
        }

        public override void Update(float elapsedTime)
        {
        }

        public override void Draw3D(GraphicsDevice gd)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            // clear background
            gd.Clear(Color.Black);

            // draw background animation
            screenManager.DrawBackground(gd);
        }

        public override void Draw2D(GraphicsDevice gd, FontManager font)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            int screenSizeX = gd.Viewport.Width;
            int screenSizeY = gd.Viewport.Height;

            Rectangle rect = new Rectangle(0, 0, 0, 0);

            // draw level screen shot
            rect.Width = levelShots[selection].Width;
            rect.Height = levelShots[selection].Height;
            rect.X = (screenSizeX - rect.Width) / 2;
            rect.Y = (screenSizeY - rect.Height) / 2 + 30;
            screenManager.DrawTexture(levelShots[selection], rect,
                Color.White, BlendState.AlphaBlend);

            // draw back and select buttons
            rect.Width = selectBack.Width;
            rect.Height = selectBack.Height;
            rect.X = (screenSizeX - rect.Width) / 2;
            rect.Y = 30;
            screenManager.DrawTexture(selectBack, rect,
                Color.White, BlendState.AlphaBlend);

            // draw change level text
            rect.Width = changeLevel.Width;
            rect.Height = changeLevel.Height;
            rect.X = (screenSizeX - rect.Width) / 2;
            rect.Y = screenSizeY - rect.Height - 30;
            screenManager.DrawTexture(changeLevel, rect,
                Color.White, BlendState.AlphaBlend);
        }
    }
}
