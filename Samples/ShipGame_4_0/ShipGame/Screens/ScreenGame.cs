#region File Description
//-----------------------------------------------------------------------------
// ScreenGame.cs
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
    public class ScreenGame : Screen
    {
        ScreenManager screenManager;    // screen manager
        GameManager gameManager;         // game manager

        // constructor
        public ScreenGame(ScreenManager manager, GameManager game)
        {
            screenManager = manager;
            gameManager = game;
        }

        // called before screen shows
        public override void SetFocus(ContentManager content, bool focus)
        {
            // if getting focus
            if (focus == true)
            {
                // load all resources
                gameManager.LoadFiles(content);
            }
            else // loosing focus
            {
                // free all resources
                gameManager.UnloadFiles();
            }
        }

        // process input
        public override void ProcessInput(float elapsedTime, InputManager input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            gameManager.ProcessInput(elapsedTime, input);

            int i, j = (int)gameManager.GameMode;
            for (i = 0; i < j; i++)
                if (input.IsKeyPressed(i,Keys.Escape) || input.IsButtonPressedBack(i))
                {
                    gameManager.GetPlayer(i).Score = -1;
                    screenManager.SetNextScreen(ScreenType.ScreenEnd);
                    gameManager.PlaySound("menu_cancel");
                }
        }

        // update screen
        public override void Update(float elapsedTime)
        {
            // update game
            gameManager.Update(elapsedTime);

            // check if any player have reached the score limit
            // if so, changes to the end screen
            int i, j = (int)gameManager.GameMode;
            for (i = 0; i < j; i++)
                if (gameManager.GetPlayer(i).Score == GameOptions.MaxPoints)
                    screenManager.SetNextScreen(ScreenType.ScreenEnd, 
                        GameOptions.FadeColor, GameOptions.FadeTime);
        }

        // draw 3D scene
        public override void Draw3D(GraphicsDevice gd)
        {
            // draw the 3d game scene
            gameManager.Draw3D(gd);
        }

        // draw 2D gui
        public override void Draw2D(GraphicsDevice gd, FontManager font)
        {
            // draw 2D game gui
            gameManager.Draw2D(font);
        }
    }
}
