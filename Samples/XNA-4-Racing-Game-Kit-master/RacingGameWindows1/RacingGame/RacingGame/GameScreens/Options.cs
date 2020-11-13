#region File Description
//-----------------------------------------------------------------------------
// Options.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Properties;
using RacingGame.Sounds;
#endregion

namespace RacingGame.GameScreens
{
    /// <summary>
    /// Options
    /// </summary>
    /// <returns>IGame screen</returns>
    class Options : IGameScreen
    {
        #region Constants
        readonly Rectangle
            Line4ArrowGfxRect = new Rectangle(154, 284, 62, 39),
            Line5ArrowGfxRect = new Rectangle(160, 354, 62, 39),
            Line6ArrowGfxRect = new Rectangle(72, 437, 62, 39),
            Resolution640x480GfxRect = new Rectangle(339, 112, 98, 32),
            Resolution800x600GfxRect = new Rectangle(454, 112, 98, 32),
            Resolution1024x768GfxRect = new Rectangle(575, 112, 108, 32),
            Resolution1280x1024GfxRect = new Rectangle(704, 112, 116, 32),
            ResolutionAutoGfxRect = new Rectangle(838, 112, 69, 32),
            FullscreenGfxRect = new Rectangle(339, 182, 105, 36),
            PostScreenEffectsGfxRect = new Rectangle(339, 226, 206, 36),
            ShadowsGfxRect = new Rectangle(616, 226, 90, 36),
            HighDetailGfxRect = new Rectangle(784, 226, 120, 36),
            SoundGfxRect = new Rectangle(384, 281, 448, 39),
            MusicGfxRect = new Rectangle(384, 354, 448, 39),
            SensitivityGfxRect = new Rectangle(384, 428, 448, 39);
        #endregion

        #region Variables
        /// <summary>
        /// Current player name, copied from the settings file.
        /// Can be changed in this screen and will be saved to the settings file.
        /// Just a variable here to make it easier to change the name and
        /// because of performance (reading Settings every frame is not good).
        /// </summary>
        string currentPlayerName = GameSettings.Default.PlayerName;
        #endregion

        #region Constructor
        int currentOptionsNumber = 0;
        int currentResolution = 4;
        bool fullscreen = true;
        bool usePostScreenShaders = true;
        bool useShadowMapping = true;
        bool useHighDetail = true;
        float currentMusicVolume = 1.0f;
        float currentSoundVolume = 1.0f;
        float currentSensitivity = 1.0f;
        /// <summary>
        /// Create options
        /// </summary>
        public Options()
        {
            // Current resolution:
            // 0=640x480, 1=800x600, 2=1024x768, 3=1280x1024, 4=auto (default)
            if (BaseGame.Width == 640 && BaseGame.Height == 480)
                currentResolution = 0;
            if (BaseGame.Width == 800 && BaseGame.Height == 600)
                currentResolution = 1;
            if (BaseGame.Width == 1024 && BaseGame.Height == 768)
                currentResolution = 2;
            if (BaseGame.Width == 1280 && BaseGame.Height == 1024)
                currentResolution = 3;

            // Get graphics detail settings
            fullscreen = BaseGame.Fullscreen;
            usePostScreenShaders = BaseGame.UsePostScreenShaders;
            useShadowMapping = BaseGame.AllowShadowMapping;
            useHighDetail = BaseGame.HighDetail;

            // Get music and sound volume
            currentMusicVolume = GameSettings.Default.MusicVolume;
            currentSoundVolume = GameSettings.Default.SoundVolume;

            // Get sensitivity
            currentSensitivity = GameSettings.Default.ControllerSensitivity;
        }
        #endregion

		#region Update
		/// <summary>
		/// Unimplemented
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update(GameTime gameTime)
		{

		}
		#endregion

        #region Run
        /// <summary>
        /// Render game screen. Called each frame.
        /// </summary>
        public bool Render()
        {
            #region Background
            // This starts both menu and in game post screen shader!
            BaseGame.UI.PostScreenMenuShader.Start();

            // Render background and black bar
            BaseGame.UI.RenderMenuBackground();

            // Options header
            int posX = 10;
            int posY = 18;
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                posX += 36;
                posY += 26;
            }
            BaseGame.UI.Headers.RenderOnScreenRelative1600(
                posX, posY, UIRenderer.HeaderOptionsGfxRect);

            // Options background
            BaseGame.UI.OptionsScreen.RenderOnScreenRelative4To3(
                0, 125, BaseGame.UI.OptionsScreen.GfxRectangle);
            #endregion

            #region Edit player name
            // Edit player name
            int xPos = BaseGame.XToRes(352);
            int yPos = BaseGame.YToRes768(125 + 65 - 20);
            TextureFont.WriteText(xPos, yPos,
                currentPlayerName +
                // Add blinking |
                ((int)(BaseGame.TotalTime / 0.35f) % 2 == 0 ? "|" : ""));
            Input.HandleKeyboardInput(ref currentPlayerName);
            #endregion

#if !XBOX360
            #region Select resolution
            // Select resolution
            // Use inverted color for selection (see below for sprite blend mode)
            Color selColor = new Color(255, 156, 0, 160);

            Rectangle res0Rect = BaseGame.CalcRectangleKeep4To3(
                Resolution640x480GfxRect);
            res0Rect.Y += BaseGame.YToRes768(125);
            bool inRes0Rect = Input.MouseInBox(res0Rect);
            if (currentResolution == 0)
                BaseGame.UI.OptionsScreen.RenderOnScreen(
                    res0Rect, Resolution640x480GfxRect,
                    selColor, BlendState.AlphaBlend);
            if (inRes0Rect && Input.MouseLeftButtonJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                currentResolution = 0;
            }

            Rectangle res1Rect = BaseGame.CalcRectangleKeep4To3(
                Resolution800x600GfxRect);
            res1Rect.Y += BaseGame.YToRes768(125);
            bool inRes1Rect = Input.MouseInBox(res1Rect);
            if (currentResolution == 1)
                BaseGame.UI.OptionsScreen.RenderOnScreen(
                    res1Rect, Resolution800x600GfxRect,
                    selColor, BlendState.AlphaBlend);
            if (inRes1Rect && Input.MouseLeftButtonJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                currentResolution = 1;
            }

            Rectangle res2Rect = BaseGame.CalcRectangleKeep4To3(
                Resolution1024x768GfxRect);
            res2Rect.Y += BaseGame.YToRes768(125);
            bool inRes2Rect = Input.MouseInBox(res2Rect);
            if (currentResolution == 2)
                BaseGame.UI.OptionsScreen.RenderOnScreen(
                    res2Rect, Resolution1024x768GfxRect,
                    selColor, BlendState.AlphaBlend);
            if (inRes2Rect && Input.MouseLeftButtonJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                currentResolution = 2;
            }

            Rectangle res3Rect = BaseGame.CalcRectangleKeep4To3(
                Resolution1280x1024GfxRect);
            res3Rect.Y += BaseGame.YToRes768(125);
            bool inRes3Rect = Input.MouseInBox(res3Rect);
            if (currentResolution == 3)
                BaseGame.UI.OptionsScreen.RenderOnScreen(
                    res3Rect, Resolution1280x1024GfxRect,
                    selColor, BlendState.AlphaBlend);
            if (inRes3Rect && Input.MouseLeftButtonJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                currentResolution = 3;
            }

            Rectangle res4Rect = BaseGame.CalcRectangleKeep4To3(
                ResolutionAutoGfxRect);
            res4Rect.Y += BaseGame.YToRes768(125);
            bool inRes4Rect = Input.MouseInBox(res4Rect);
            if (currentResolution == 4)
                BaseGame.UI.OptionsScreen.RenderOnScreen(
                    res4Rect, ResolutionAutoGfxRect,
                    selColor, BlendState.AlphaBlend);
            if (inRes4Rect && Input.MouseLeftButtonJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                currentResolution = 4;
            }
            #endregion

            #region Graphics options

            Rectangle fsRect = BaseGame.CalcRectangleKeep4To3(
                FullscreenGfxRect);
            fsRect.Y += BaseGame.YToRes768(125);
            bool inFsRect = Input.MouseInBox(fsRect);
            if (fullscreen)
                BaseGame.UI.OptionsScreen.RenderOnScreen(
                    fsRect, FullscreenGfxRect,
                    selColor, BlendState.AlphaBlend);
            if (inFsRect && Input.MouseLeftButtonJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                fullscreen = !fullscreen;
            }

            Rectangle pseRect = BaseGame.CalcRectangleKeep4To3(
                PostScreenEffectsGfxRect);
            pseRect.Y += BaseGame.YToRes768(125);
            bool inPseRect = Input.MouseInBox(pseRect);
            if (usePostScreenShaders)
                BaseGame.UI.OptionsScreen.RenderOnScreen(
                    pseRect, PostScreenEffectsGfxRect,
                    selColor, BlendState.AlphaBlend);
            if (inPseRect && Input.MouseLeftButtonJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                usePostScreenShaders = !usePostScreenShaders;
            }

            Rectangle smRect = BaseGame.CalcRectangleKeep4To3(
                ShadowsGfxRect);
            smRect.Y += BaseGame.YToRes768(125);
            bool inSmRect = Input.MouseInBox(smRect);
            if (useShadowMapping)
                BaseGame.UI.OptionsScreen.RenderOnScreen(
                    smRect, ShadowsGfxRect,
                    selColor, BlendState.AlphaBlend);
            if (inSmRect && Input.MouseLeftButtonJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                useShadowMapping = !useShadowMapping;
            }

            Rectangle hdRect = BaseGame.CalcRectangleKeep4To3(
                HighDetailGfxRect);
            hdRect.Y += BaseGame.YToRes768(125);
            bool inHdRect = Input.MouseInBox(hdRect);
            if (useHighDetail)
                BaseGame.UI.OptionsScreen.RenderOnScreen(
                    hdRect, HighDetailGfxRect,
                    selColor, BlendState.AlphaBlend);
            if (inHdRect && Input.MouseLeftButtonJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                useHighDetail = !useHighDetail;
            }
            #endregion
#endif

            #region Sound volume
            Rectangle soundRect = BaseGame.CalcRectangleKeep4To3(
                SoundGfxRect);
            soundRect.Y += BaseGame.YToRes768(125);
            if (Input.MouseInBox(soundRect))
            {
                if (Input.MouseLeftButtonJustPressed)
                {
                    currentSoundVolume =
                        (Input.MousePos.X - soundRect.X) / (float)soundRect.Width;
                    Sound.Play(Sound.Sounds.Highlight);
                }
            }

            // Handel controller input
            if (currentOptionsNumber == 0)
            {
                if (Input.GamePadLeftJustPressed ||
                    Input.KeyboardLeftJustPressed)
                {
                    currentSoundVolume -= 0.1f;
                    Sound.Play(Sound.Sounds.Highlight);
                }
                if (Input.GamePadRightJustPressed ||
                    Input.KeyboardRightJustPressed)
                {
                    currentSoundVolume += 0.1f;
                    Sound.Play(Sound.Sounds.Highlight);
                }
                if (currentSoundVolume < 0)
                    currentSoundVolume = 0;
                if (currentSoundVolume > 1)
                    currentSoundVolume = 1;
            }

            // Render slider handle
            Rectangle gfxRect = UIRenderer.SelectionRadioButtonGfxRect;
            BaseGame.UI.Buttons.RenderOnScreen(new Rectangle(
                soundRect.X + (int)(soundRect.Width * currentSoundVolume) -
                BaseGame.XToRes(gfxRect.Width) / 2,
                soundRect.Y,
                BaseGame.XToRes(gfxRect.Width), BaseGame.YToRes768(gfxRect.Height)),
                gfxRect);
            #endregion

            #region Music volume
            Rectangle musicRect = BaseGame.CalcRectangleKeep4To3(
                MusicGfxRect);
            musicRect.Y += BaseGame.YToRes768(125);
            if (Input.MouseInBox(musicRect))
            {
                if (Input.MouseLeftButtonJustPressed)
                {
                    currentMusicVolume =
                        (Input.MousePos.X - musicRect.X) / (float)musicRect.Width;
                    Sound.Play(Sound.Sounds.Highlight);
                }
            }

            // Handel controller input
            if (currentOptionsNumber == 1)
            {
                if (Input.GamePadLeftJustPressed ||
                    Input.KeyboardLeftJustPressed)
                {
                    currentMusicVolume -= 0.1f;
                    Sound.Play(Sound.Sounds.Highlight);
                }
                if (Input.GamePadRightJustPressed ||
                    Input.KeyboardRightJustPressed)
                {
                    currentMusicVolume += 0.1f;
                    Sound.Play(Sound.Sounds.Highlight);
                }
                if (currentMusicVolume < 0)
                    currentMusicVolume = 0;
                if (currentMusicVolume > 1)
                    currentMusicVolume = 1;
            }

            // Render slider handle
            BaseGame.UI.Buttons.RenderOnScreen(new Rectangle(
                musicRect.X + (int)(musicRect.Width * currentMusicVolume) -
                BaseGame.XToRes(gfxRect.Width) / 2,
                musicRect.Y,
                BaseGame.XToRes(gfxRect.Width), BaseGame.YToRes768(gfxRect.Height)),
                gfxRect);
            #endregion

            Sound.SetVolumes(currentSoundVolume, currentMusicVolume);

            #region Controller sensitivity
            Rectangle sensitivityRect = BaseGame.CalcRectangleKeep4To3(
                SensitivityGfxRect);
            sensitivityRect.Y += BaseGame.YToRes768(125);
            if (Input.MouseInBox(sensitivityRect))
            {
                if (Input.MouseLeftButtonJustPressed)
                {
                    currentSensitivity =
                        (Input.MousePos.X - sensitivityRect.X) /
                        (float)sensitivityRect.Width;
                    Sound.Play(Sound.Sounds.Highlight);
                }
            }

            // Handel controller input
            if (currentOptionsNumber == 2)
            {
                if (Input.GamePadLeftJustPressed ||
                    Input.KeyboardLeftJustPressed)
                {
                    currentSensitivity -= 0.1f;
                    Sound.Play(Sound.Sounds.Highlight);
                }
                if (Input.GamePadRightJustPressed ||
                    Input.KeyboardRightJustPressed)
                {
                    currentSensitivity += 0.1f;
                    Sound.Play(Sound.Sounds.Highlight);
                }
                if (currentSensitivity < 0)
                    currentSensitivity = 0;
                if (currentSensitivity > 1)
                    currentSensitivity = 1;
            }

            // Render slider handle
            BaseGame.UI.Buttons.RenderOnScreen(new Rectangle(
                sensitivityRect.X +
                (int)(sensitivityRect.Width * currentSensitivity) -
                BaseGame.XToRes(gfxRect.Width) / 2,
                sensitivityRect.Y,
                BaseGame.XToRes(gfxRect.Width), BaseGame.YToRes768(gfxRect.Height)),
                gfxRect);
            #endregion

            #region Show selected line
            Rectangle[] lineArrowGfxRects = new Rectangle[]
            {
                Line4ArrowGfxRect,
                Line5ArrowGfxRect,
                Line6ArrowGfxRect,
            };
            for (int num = 0; num < lineArrowGfxRects.Length; num++)
            {
                Rectangle lineRect = BaseGame.CalcRectangleKeep4To3(
                    lineArrowGfxRects[num]);
                lineRect.Y += BaseGame.YToRes768(125);
                lineRect.X -= BaseGame.XToRes(8 + (int)Math.Round(8 *
                    Math.Sin(BaseGame.TotalTime / 0.21212f)));

                // Draw selection arrow
                if (currentOptionsNumber == num)
                    BaseGame.UI.Buttons.RenderOnScreen(
                        lineRect, UIRenderer.SelectionArrowGfxRect, Color.White);
            }

            // Game pad selection
            if (Input.GamePadUpJustPressed ||
                Input.KeyboardUpJustPressed)
            {
                Sound.Play(Sound.Sounds.Highlight);
                currentOptionsNumber = (lineArrowGfxRects.Length +
                    currentOptionsNumber - 1) % lineArrowGfxRects.Length;
            }
            else if (Input.GamePadDownJustPressed ||
                Input.KeyboardDownJustPressed)
            {
                Sound.Play(Sound.Sounds.Highlight);
                currentOptionsNumber = (currentOptionsNumber + 1) %
                    lineArrowGfxRects.Length;
            }
            #endregion

            #region Bottom buttons
            BaseGame.UI.RenderBottomButtons(true);
            #endregion

            #region Apply settings when quitting
            if (Input.KeyboardEscapeJustPressed ||
                Input.GamePadBJustPressed ||
                Input.GamePadBackJustPressed ||
                BaseGame.UI.backButtonPressed)
            {
                // Apply settings, for xbox only set music/sound and sensitivity!
                GameSettings.Default.PlayerName = currentPlayerName;
                switch (currentResolution)
                {
                    case 0:
                        GameSettings.Default.ResolutionWidth = 640;
                        GameSettings.Default.ResolutionHeight = 480;
                        break;
                    case 1:
                        GameSettings.Default.ResolutionWidth = 800;
                        GameSettings.Default.ResolutionHeight = 600;
                        break;
                    case 2:
                        GameSettings.Default.ResolutionWidth = 1024;
                        GameSettings.Default.ResolutionHeight = 768;
                        break;
                    case 3:
                        GameSettings.Default.ResolutionWidth = 1280;
                        GameSettings.Default.ResolutionHeight = 1024;
                        break;
                    case 4:
                        // Try to use best resolution available
                        GameSettings.Default.ResolutionWidth = 0;
                        GameSettings.Default.ResolutionHeight = 0;
                        break;
                }
                GameSettings.Default.Fullscreen = fullscreen;
                GameSettings.Default.PostScreenEffects = usePostScreenShaders;
                GameSettings.Default.ShadowMapping = useShadowMapping;
                GameSettings.Default.HighDetail = useHighDetail;
                GameSettings.Default.MusicVolume = currentMusicVolume;
                GameSettings.Default.SoundVolume = currentSoundVolume;
                GameSettings.Default.ControllerSensitivity = currentSensitivity;

                // Save all
                GameSettings.Save();
                // Update game settings
                BaseGame.CheckOptionsAndPSVersion();

                return true;
            }
            #endregion

            return false;
        }
        #endregion
    }
}
