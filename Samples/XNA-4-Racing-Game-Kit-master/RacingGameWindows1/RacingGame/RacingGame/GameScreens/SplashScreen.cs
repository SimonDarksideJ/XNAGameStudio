#region File Description
//-----------------------------------------------------------------------------
// SplashScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.Graphics;
using RacingGame.GameLogic;
using RacingGame.Shaders;
#endregion

namespace RacingGame.GameScreens
{
    /// <summary>
    /// Splash screen
    /// </summary>
    class SplashScreen : IGameScreen
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

        #region RenderSplashScreen
        /// <summary>
        /// Render splash screen
        /// </summary>
        public bool Render()
        {
            BaseGame.UI.UpdateCarInMenu();

            ShadowMapShader.PrepareGameShadows();

            // Render background and black bar
            BaseGame.UI.RenderGameBackground();
            BaseGame.UI.RenderMenuTrackBackground();
            BaseGame.UI.RenderBlackBar(518, 61);

            // Show shadows we calculated above
            if (BaseGame.AllowShadowMapping)
                ShaderEffect.shadowMapping.ShowShadows();

            // Show Press Start to continue. 
            if ((int)(BaseGame.TotalTime / 0.375f) % 3 != 0)
                BaseGame.UI.Headers.RenderOnScreen(
                    BaseGame.CalcRectangleCenteredWithGivenHeight(
                    512, 518 + 61 / 2, 26, UIRenderer.PressStartGfxRect),
                    UIRenderer.PressStartGfxRect);

            // Clicking or pressing start will go to the menu
            return Input.MouseLeftButtonJustPressed ||
                Input.KeyboardSpaceJustPressed ||
                Input.KeyboardEscapeJustPressed ||
                Input.GamePadStartPressed;
        }
        #endregion
    }
}
