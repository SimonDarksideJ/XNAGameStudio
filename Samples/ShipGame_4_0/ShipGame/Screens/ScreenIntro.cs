#region File Description
//-----------------------------------------------------------------------------
// ScreenIntro.cs
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
    public class ScreenIntro : Screen
    {
        ScreenManager screenManager;    // screen manager
        GameManager gameManager;        // game manager

        int menuSelection;              // current menu selection
        float menuTime;                 // menu time for animation

        Texture2D textureLogo;          // logo texture
        Texture2D textureLens;          // lens texture
        
        Texture2D textureCursorAnim;    // cursor textures
        Texture2D textureCursorBullet;
        Texture2D textureCursorArrow;

        // menu itens
        const int NumberMenuItems = 4;
        String[] menuNames = new String[NumberMenuItems] 
                 { "menu_sp", "menu_mp", "menu_hp", "menu_qg" };

        // menu textures without hover
        Texture2D[] textureMenu = new Texture2D[NumberMenuItems];
        // menu textures with hover
        Texture2D[] textureMenuHover = new Texture2D[NumberMenuItems];

        // constructor
        public ScreenIntro(ScreenManager manager, GameManager game)
        {
            screenManager = manager;
            gameManager = game;
        }

        // called before screen shows or stops showing
        public override void SetFocus(ContentManager content, bool focus)
        {
            // if getting focus
            if (focus)
            {
                // load all resources
                gameManager.GameMode = GameMode.SinglePlayer;

                textureLogo = content.Load<Texture2D>(
                                            "screens/intro_logo");
                textureLens = content.Load<Texture2D>(
                                            "screens/intro_lens");

                textureCursorAnim = content.Load<Texture2D>(
                                            "screens/cursor_anim");
                textureCursorArrow = content.Load<Texture2D>(
                                            "screens/cursor_arrow");
                textureCursorBullet = content.Load<Texture2D>(
                                            "screens/cursor_bullet");

                for (int i = 0; i < NumberMenuItems; i++)
                {
                    textureMenu[i] = content.Load<Texture2D>(
                                    "screens/" + menuNames[i]);
                    textureMenuHover[i] = content.Load<Texture2D>(
                                    "screens/" + menuNames[i] + "_hover");
                }
            }
            else // loosing focus
            {
                // free all resources
                textureLogo = null;
                textureLens = null;
                textureCursorAnim = null;
                textureCursorArrow = null;
                textureCursorBullet = null;

                for (int i = 0; i < NumberMenuItems; i++)
                {
                    textureMenu[i] = null;
                    textureMenuHover[i] = null;
                }
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
                // A button or enter to select menu option
                if (input.IsButtonPressedA(i) || 
                    input.IsButtonPressedStart(i) || 
                    input.IsKeyPressed(i, Keys.Enter) || 
                    input.IsKeyPressed(i, Keys.Space))
                {
                    switch (menuSelection)
                    {
                        case 0:
                            // single player
                            gameManager.GameMode = GameMode.SinglePlayer;
                            screenManager.SetNextScreen(ScreenType.ScreenPlayer);
                            break;
                        case 1:
                            // multi player
                            gameManager.GameMode = GameMode.MultiPlayer;
                            screenManager.SetNextScreen(ScreenType.ScreenPlayer);
                            break;
                        case 2:
                            // help
                            screenManager.SetNextScreen(ScreenType.ScreenHelp);
                            break;
                        case 3:
                            // exit game
                            screenManager.Exit();
                            break;
                    }
                    gameManager.PlaySound("menu_select");
                }

                // up/down keys change menu sel
                if (input.IsKeyPressed(i,Keys.Up) || 
                    input.IsButtonPressedDPadUp(i) ||
                    input.IsButtonPressedLeftStickUp(i))
                {
                    menuSelection = 
                        (menuSelection == 0 ? NumberMenuItems - 1 : menuSelection - 1);
                    gameManager.PlaySound("menu_change");
                }
                if (input.IsKeyPressed(i, Keys.Down) || 
                    input.IsButtonPressedDPadDown(i) ||
                    input.IsButtonPressedLeftStickDown(i))
                {
                    menuSelection = (menuSelection + 1) % NumberMenuItems;
                    gameManager.PlaySound("menu_change");
                }
            }
        }

        // update screen
        public override void Update(float elapsedTime)
        {
            // accumulate elapsed time
            menuTime += elapsedTime;
        }

        // draw 3D scene
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

        // draw the animated cursor
        void DrawCursor(int x, int y)
        {
            Rectangle rect = new Rectangle(0, 0, 0, 0);

            float rotation = menuTime * 2;

            // draw animated cursor texture
            rect.X = x - textureCursorAnim.Width / 2;
            rect.Y = y - textureCursorAnim.Height / 2;
            rect.Width = textureCursorAnim.Width;
            rect.Height = textureCursorAnim.Height;
            screenManager.DrawTexture(textureCursorAnim, rect, rotation, 
                Color.White, BlendState.AlphaBlend);

            // draw bullet cursor texture
            rect.X = x - textureCursorBullet.Width / 2;
            rect.Y = y - textureCursorBullet.Height / 2;
            rect.Width = textureCursorBullet.Width;
            rect.Height = textureCursorBullet.Height;
            screenManager.DrawTexture(textureCursorBullet, rect,
                Color.White, BlendState.AlphaBlend);

            // draw arrow cursor texture
            rect.X = x - textureCursorArrow.Width / 2 + 32;
            rect.Y = y - textureCursorArrow.Height / 2;
            rect.Width = textureCursorArrow.Width;
            rect.Height = textureCursorArrow.Height;
            screenManager.DrawTexture(textureCursorArrow, rect,
                Color.White, BlendState.AlphaBlend);
        }

        // draw 2D gui
        public override void Draw2D(GraphicsDevice gd, FontManager font)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            // screen rect
            Rectangle rect = new Rectangle(gd.Viewport.X, gd.Viewport.Y, 
                            gd.Viewport.Width, gd.Viewport.Height);
            
            // draw lens flare texture
            screenManager.DrawTexture(textureLens, rect,
                Color.White, BlendState.Additive);
            
            // draw logo texture
            screenManager.DrawTexture(textureLogo, rect,
                Color.White, BlendState.AlphaBlend);

            // draw menu itens
            int Y = rect.Height - 200;
            for (int i = 0; i < NumberMenuItems; i++)
            {
                // if item selected
                if (i == menuSelection)
                {
                    rect.X = 540;
                    rect.Y = Y;
                    rect.Width = textureMenuHover[i].Width;
                    rect.Height = textureMenuHover[i].Height;
                    screenManager.DrawTexture(textureMenuHover[i], rect,
                        Color.White, BlendState.AlphaBlend);

                    // draw cursor left of selected item
                    DrawCursor(rect.X - 60, rect.Y + 19);

                    Y += 50;
                }
                else // item not selected
                {
                    rect.X = 540;
                    rect.Y = Y;
                    rect.Width = textureMenu[i].Width;
                    rect.Height = textureMenu[i].Height;

                    screenManager.DrawTexture(textureMenu[i], rect,
                        Color.White, BlendState.AlphaBlend);

                    Y += 40;
                }
            }
        }
    }
}
