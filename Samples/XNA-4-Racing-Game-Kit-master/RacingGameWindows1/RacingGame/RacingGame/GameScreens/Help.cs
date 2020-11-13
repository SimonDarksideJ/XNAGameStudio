#region File Description
//-----------------------------------------------------------------------------
// Help.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Properties;
using Microsoft.Xna.Framework;
#endregion

namespace RacingGame.GameScreens
{
    /// <summary>
    /// Help
    /// </summary>
    /// <returns>IGame screen</returns>
    class Help : IGameScreen
	{
		#region Update
		/// <summary>
		/// Unimplemented
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update(GameTime gameTime)
		{

		}
		#endregion

		#region Render
		/// <summary>
        /// Render game screen. Called each frame.
        /// </summary>
        public bool Render()
        {
            // This starts both menu and in game post screen shader!
            BaseGame.UI.PostScreenMenuShader.Start();

            // Render background and black bar
            BaseGame.UI.RenderMenuBackground();

            // Help header
            int posX = 10;
            int posY = 18;
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                posX += 36;
                posY += 26;
            }
            BaseGame.UI.Headers.RenderOnScreenRelative1600(
                posX, posY, UIRenderer.HeaderHelpGfxRect);

            // Help
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                BaseGame.UI.HelpScreen.RenderOnScreen(
                    BaseGame.CalcRectangleKeep4To3(
                    25, 130, BaseGame.UI.HelpScreen.GfxRectangle.Width - 50,
                    BaseGame.UI.HelpScreen.GfxRectangle.Height - 12),
                    BaseGame.UI.HelpScreen.GfxRectangle);
            }
            else
            {
                BaseGame.UI.HelpScreen.RenderOnScreenRelative4To3(
                    0, 125, BaseGame.UI.HelpScreen.GfxRectangle);
            }

            BaseGame.UI.RenderBottomButtons(true);

            if (Input.KeyboardEscapeJustPressed ||
                Input.GamePadBJustPressed ||
                Input.GamePadBackJustPressed ||
                Input.MouseLeftButtonJustPressed)
                return true;

            return false;
        }
        #endregion
    }
}
