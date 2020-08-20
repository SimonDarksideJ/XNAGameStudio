//-----------------------------------------------------------------------------
// SingleControlScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UserInterfaceSample.Controls;


namespace UserInterfaceSample
{
    /// <summary>
    /// A screen containing single Control. This class serves as a bridge between the 'Controls'
    /// UI system and the 'ScreenManager' UI system.
    /// </summary>
    public class SingleControlScreen : GameScreen
    {
        /// <summary>
        /// The sole Control in this screen. Derived classes can do what they like with it.
        /// </summary>
        protected Control RootControl;

        public override void Draw(GameTime gameTime)
        {
            if (RootControl != null)
            {
                Control.BatchDraw(RootControl, ScreenManager.GraphicsDevice, ScreenManager.SpriteBatch, Vector2.Zero, gameTime);
            }

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            RootControl.Update(gameTime);

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputState input)
        {
            // cancel the current screen if the user presses the back button
            PlayerIndex player;
            if (input.IsNewButtonPress(Buttons.Back, null, out player))
            {
                ExitScreen();
            }

            RootControl.HandleInput(input);

            base.HandleInput(input);
        }
    }
}
