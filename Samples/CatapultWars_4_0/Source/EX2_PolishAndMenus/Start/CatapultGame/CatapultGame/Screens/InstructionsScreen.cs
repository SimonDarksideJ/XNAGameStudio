#region File Description
//-----------------------------------------------------------------------------
// InstructionsScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GameStateManagement;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace CatapultGame
{
    class InstructionsScreen : GameScreen
    {
        Texture2D background;

        public InstructionsScreen()
        {
            EnabledGestures = GestureType.Tap;

            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void LoadContent()
        {
            background = Load<Texture2D>("Textures/Backgrounds/instructions");           
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw Background
            spriteBatch.Draw(background, new Vector2(0, 0),
                    new Color(255, 255, 255, TransitionAlpha));

            spriteBatch.End();
        }

        public override void HandleInput(InputState input)
        {
            foreach (var gesture in input.Gestures)
            {
                if (gesture.GestureType == GestureType.Tap)
                {
                    ExitScreen();
                    //ScreenManager.AddScreen(new GameplayScreen(), null);
                }
            }

            base.HandleInput(input);
        }
    }
}
