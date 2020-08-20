#region File Description
//-----------------------------------------------------------------------------
// ScreenHelp.cs
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
    public class ScreenHelp : Screen
    {
        ScreenManager screenManager;    // screen manager
        GameManager gameManager;         // game manager

        Texture2D textureControls;    // controlls text texture
        Texture2D textureDisplay;     // controller texture
        Texture2D textureContinue;    // continue text texture

        // constructor
        public ScreenHelp(ScreenManager manager, GameManager game)
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
                textureControls = content.Load<Texture2D>(
                                    "screens/controls");
                textureDisplay = content.Load<Texture2D>(
                                    "screens/controls_display");
                textureContinue = content.Load<Texture2D>(
                                    "screens/continue");
            }
            else  // loosing focus
            {
                // free all resources
                textureControls = null;
                textureDisplay = null;
                textureContinue = null;
            }
        }

        // process input
        public override void ProcessInput(float elapsedTime, InputManager input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            for (int i = 0; i < 2; i++)
            {
                // Any key/button to go back
                if (input.IsButtonPressedA(i) ||
                    input.IsButtonPressedB(i) ||
                    input.IsButtonPressedX(i) ||
                    input.IsButtonPressedY(i) ||
                    input.IsButtonPressedLeftShoulder(i) ||
                    input.IsButtonPressedRightShoulder(i) ||
                    input.IsButtonPressedLeftStick(i) ||
                    input.IsButtonPressedRightStick(i) ||
                    input.IsButtonPressedBack(i) ||
                    input.IsButtonPressedStart(i) ||
                    input.IsKeyPressed(i, Keys.Enter) ||
                    input.IsKeyPressed(i, Keys.Escape) ||
                    input.IsKeyPressed(i, Keys.Space))
                {
                    screenManager.SetNextScreen(ScreenType.ScreenIntro);
                    gameManager.PlaySound("menu_cancel");
                }
            }
        }

        // update screen
        public override void Update(float elapsedTime)
        {
        }

        // draw 3D scene
        public override void Draw3D(GraphicsDevice gd)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            // clear background
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

            // draw background animation
            screenManager.DrawBackground(gd);
        }

        // draw 2D gui
        public override void Draw2D(GraphicsDevice gd, FontManager font)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            Rectangle rect = new Rectangle(0, 0, 0, 0);

            int screenSizeX = gd.Viewport.Width;
            int screenSizeY = gd.Viewport.Height;

            // draw controlls text aligned to top of screen
            rect.Width = textureControls.Width;
            rect.Height = textureControls.Height;
            rect.X = screenSizeX / 2 - rect.Width / 2;
            rect.Y = 40;
            screenManager.DrawTexture(textureControls, rect, 
                Color.White, BlendState.AlphaBlend);

            // draw controller texture centered in screen
            rect.Width = textureDisplay.Width;
            rect.Height = textureDisplay.Height;
            rect.X = screenSizeX / 2 - rect.Width / 2;
            rect.Y = screenSizeY / 2 - rect.Height / 2 + 10;
            screenManager.DrawTexture(textureDisplay, rect, 
                Color.White, BlendState.AlphaBlend);

            // draw continue message aligned to bottom of screen
            rect.Width = textureContinue.Width;
            rect.Height = textureContinue.Height;
            rect.X = screenSizeX / 2 - rect.Width / 2;
            rect.Y = screenSizeY - rect.Height - 60;
            screenManager.DrawTexture(textureContinue, rect, 
                Color.White, BlendState.AlphaBlend);
        }
    }
}
