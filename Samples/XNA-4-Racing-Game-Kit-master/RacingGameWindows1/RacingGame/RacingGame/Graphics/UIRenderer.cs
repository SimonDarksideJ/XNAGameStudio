#region File Description
//-----------------------------------------------------------------------------
// UIRenderer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using RacingGame.GameLogic;
using RacingGame.Helpers;
using RacingGame.Shaders;
using RacingGame.Tracks;
using RacingGame.GameScreens;
#endregion

namespace RacingGame.Graphics
{
    /// <summary>
    /// Helper class to render all UI 2D stuff. Rendering textures is very easy
    /// with XNA using the Sprite class, but we still have to create
    /// SpriteBatches. This class helps and handles everything for us.
    /// </summary>
    public class UIRenderer : IDisposable
    {
        #region Constants
        /// <summary>
        /// Graphic rectangles for displaying UI stuff.
        /// </summary>
        public readonly static Rectangle
            BackgroundGfxRect = new Rectangle(0, 0, 1024, 640),
            RacingGameLogoGfxRect = new Rectangle(0, 1024 - 375, 1024, 374),
            BottomLeftBorderGfxRect = new Rectangle(2, 984, 38, 37),
            BottomRightBorderGfxRect = new Rectangle(42, 984, 38, 37),
            PressStartGfxRect = new Rectangle(2, 1, 631, 45),
            HeaderChooseCarGfxRect = new Rectangle(0, 212, 512, 100),
            HeaderOptionsGfxRect = new Rectangle(512, 212, 512, 100),
            HeaderSelectTrackGfxRect = new Rectangle(0, 312, 512, 100),
            HeaderHelpGfxRect = new Rectangle(512, 312, 512, 100),
            HeaderHighscoresGfxRect = new Rectangle(0, 412, 512, 100),
            BlackBarGfxRect = new Rectangle(99, 999, 1, 1),
            OptionsRadioButtonGfxRect = new Rectangle(128, 980, 25, 25),
            MenuButtonPlayGfxRect = new Rectangle(0, 0, 212, 212),
            MenuButtonHighscoresGfxRect = new Rectangle(212, 0, 212, 212),
            MenuButtonOptionsGfxRect = new Rectangle(2 * 212, 0, 212, 212),
            MenuButtonHelpGfxRect = new Rectangle(3 * 212, 0, 212, 212),
            MenuButtonQuitGfxRect = new Rectangle(212, 240, 212, 212),
            MenuButtonSelectionGfxRect = new Rectangle(3 * 212, 240, 212, 212),
            MenuTextPlayGfxRect = new Rectangle(0, 214, 212, 24),
            MenuTextHighscoresGfxRect = new Rectangle(212, 214, 212, 24),
            MenuTextOptionsGfxRect = new Rectangle(2 * 212, 214, 212, 24),
            MenuTextHelpGfxRect = new Rectangle(3 * 212, 214, 212, 24),
            MenuTextQuitGfxRect = new Rectangle(212, 240 + 214, 212, 24),
            BigArrowGfxRect = new Rectangle(867, 242, 127, 178),
            TrackButtonBeginnerGfxRect = new Rectangle(0, 480, 212, 352),
            TrackButtonAdvancedGfxRect = new Rectangle(212, 480, 212, 352),
            TrackButtonExpertGfxRect = new Rectangle(2 * 212, 480, 212, 352),
            TrackButtonSelectionGfxRect = new Rectangle(3 * 212, 480, 212, 352),
            TrackTextBeginnerGfxRect = new Rectangle(0, 834, 212, 24),
            TrackTextAdvancedGfxRect = new Rectangle(212, 834, 212, 24),
            TrackTextExpertGfxRect = new Rectangle(2 * 212, 834, 212, 24),
            BottomButtonSelectionGfxRect = new Rectangle(212 * 2, 240, 212, 92),
            BottomButtonAButtonGfxRect = new Rectangle(0, 872, 212, 92),
            BottomButtonBButtonGfxRect = new Rectangle(212, 872, 212, 92),
            BottomButtonBackButtonGfxRect = new Rectangle(2 * 212, 872, 212, 92),
            BottomButtonStartButtonGfxRect = new Rectangle(3 * 212, 872, 212, 92),
            SelectionArrowGfxRect = new Rectangle(874, 426, 53, 39),
            SelectionRadioButtonGfxRect = new Rectangle(935, 427, 39, 39);

        /// <summary>
        /// In game gfx rects. Tacho is our speedometer we see on the screen.
        /// </summary>
        public static readonly Rectangle
            LapsGfxRect = new Rectangle(381, 132, 222, 160),
            TachoGfxRect = new Rectangle(0, 0, 343, 341),
            TachoArrowGfxRect = new Rectangle(347, 0, 28, 186),
            TachoMphGfxRect = new Rectangle(184, 256, 148, 72),
            TachoGearGfxRect = new Rectangle(286, 149, 52, 72),
            CurrentAndBestGfxRect = new Rectangle(381, 2, 342, 128),
            CurrentTimePosGfxRect = new Rectangle(540, 8, 170, 52),
            BestTimePosGfxRect = new Rectangle(540, 72, 170, 52),
            TrackNameGfxRect = new Rectangle(726, 2, 282, 62),
            Best5GfxRect = new Rectangle(726, 66, 282, 62);
        #endregion

        #region Variables
        /// <summary>
        /// Background
        /// </summary>
        Texture background;
        /// <summary>
        /// Buttons
        /// </summary>
        Texture buttons;
        /// <summary>
        /// Headers
        /// </summary>
        Texture headers;
        /// <summary>
        /// Help screen texture
        /// </summary>
        Texture helpScreen;
        /// <summary>
        /// Options screen texture
        /// </summary>
        Texture optionsScreen;
        /// <summary>
        /// Mouse cursor
        /// </summary>
        Texture mouseCursor;
        /// <summary>
        /// Font
        /// </summary>
        TextureFont font;

        /// <summary>
        /// Post screen menu shader for cool screen effects like the TV effect,
        /// stripes, a flash effect and some color correction.
        /// </summary>
        PostScreenMenu postScreenMenuShader;

        /// <summary>
        /// Post screen game shader for glow, color correction and motion blur
        /// effects in the game.
        /// </summary>
        PostScreenGlow postScreenGameShader;

        /// <summary>
        /// Sky cube background rendering
        /// </summary>
        PreScreenSkyCubeMapping skyCube = null;

        /// <summary>
        /// And finally the lens flare we render at the end.
        /// </summary>
        LensFlare lensFlare = null;

        /// <summary>
        /// Ingame texture for the speed, gear, time, rank, laps, etc.
        /// </summary>
        Texture ingame = null;

        /// <summary>
        /// Trophies
        /// </summary>
        Texture[] trophies = new Texture[3];
        #endregion

        #region Properties
        /// <summary>
        /// Buttons
        /// </summary>
        /// <returns>Texture</returns>
        public Texture Buttons
        {
            get
            {
                return buttons;
            }
        }

        /// <summary>
        /// Headers
        /// </summary>
        /// <returns>Texture</returns>
        public Texture Headers
        {
            get
            {
                return headers;
            }
        }

        /// <summary>
        /// Help screen
        /// </summary>
        /// <returns>Texture</returns>
        public Texture HelpScreen
        {
            get
            {
                return helpScreen;
            }
        }

        /// <summary>
        /// Options screen
        /// </summary>
        /// <returns>Texture</returns>
        public Texture OptionsScreen
        {
            get
            {
                return optionsScreen;
            }
        }

        /// <summary>
        /// Ingame
        /// </summary>
        /// <returns>Texture</returns>
        public Texture Ingame
        {
            get
            {
                return ingame;
            }
        }

        /// <summary>
        /// Trophy types we can get at the end of each game.
        /// Gold means new record. Silver means place 2
        /// and Bronse is used for everything below (since we won ^^).
        /// </summary>
        public enum TrophyType
        {
            Gold,
            Silver,
            Bronze,
        }

        /// <summary>
        /// Get trophy texture
        /// </summary>
        /// <param name="trophyType">Trophy type</param>
        /// <returns>Texture</returns>
        public Texture GetTrophyTexture(TrophyType trophyType)
        {
            return trophies[(int)trophyType];
        }

        /// <summary>
        /// Post screen menu shader
        /// </summary>
        /// <returns>Post screen menu</returns>
        public PostScreenMenu PostScreenMenuShader
        {
            get
            {
                return postScreenMenuShader;
            }
        }

        public PostScreenGlow PostScreenGlowShader
        {
            get
            {
                return postScreenGameShader;
            }
        }

        /// <summary>
        /// Sky cube map texture
        /// </summary>
        /// <returns>Texture cube</returns>
        public TextureCube SkyCubeMapTexture
        {
            get
            {
                return skyCube.SkyCubeMapTexture;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create user interface renderer
        /// </summary>
        public UIRenderer()
        {
            background = new Texture("background.png");
            buttons = new Texture("buttons.png");
            headers = new Texture("headers.png");
#if XBOX360
            helpScreen = new Texture("HelpScreenXbox360.png");
            optionsScreen = new Texture("OptionsScreenXbox360.png");
#else
            helpScreen = new Texture("HelpScreenWindows.png");
            optionsScreen = new Texture("OptionsScreenWindows.png");
#endif
            mouseCursor = new Texture("MouseCursor.png");
            ingame = new Texture("Ingame.png");
            trophies[0] = new Texture("pokal1");
            trophies[1] = new Texture("pokal2");
            trophies[2] = new Texture("pokal3");
            font = new TextureFont();

            postScreenMenuShader = new PostScreenMenu();
            postScreenGameShader = new PostScreenGlow();
            skyCube = new PreScreenSkyCubeMapping();
            lensFlare = new LensFlare(LensFlare.DefaultSunPos);
            BaseGame.LightDirection = LensFlare.DefaultLightPos;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (background != null)
                    background.Dispose();
                if (buttons != null)
                    buttons.Dispose();
                if (headers != null)
                    headers.Dispose();
                if (helpScreen != null)
                    helpScreen.Dispose();
                if (optionsScreen != null)
                    optionsScreen.Dispose();
                if (mouseCursor != null)
                    mouseCursor.Dispose();
                if (font != null)
                    font.Dispose();
                if (postScreenMenuShader != null)
                    postScreenMenuShader.Dispose();
                if (postScreenGameShader != null)
                    postScreenGameShader.Dispose();
                if (skyCube != null)
                    skyCube.Dispose();
                if (lensFlare != null)
                    lensFlare.Dispose();
                if (ingame != null)
                    ingame.Dispose();
            }
        }
        #endregion

        #region Add time fadeup effect
        /// <summary>
        /// Time fadeup modes
        /// </summary>
        public enum TimeFadeupMode
        {
            Plus,
            Minus,
            Normal,
        }

        class TimeFadeupText
        {
            public string text;
            public Color color;
            public float showTimeMs;
            public const float MaxShowTimeMs = 2250;

            public TimeFadeupText(string setText, Color setColor)
            {
                text = setText;
                color = setColor;
                showTimeMs = MaxShowTimeMs;
            }
        }
        List<TimeFadeupText> fadeupTexts = new List<TimeFadeupText>();

        /// <summary>
        /// Add time fadeup effect. Used in the game to show times, plus (green)
        /// means we took longer for this checkpoint, minus (green) means we did
        /// good.
        /// </summary>
        /// <param name="timeMilisec">Time</param>
        /// <param name="mode">Mode</param>
        public void AddTimeFadeupEffect(int timeMilliseconds, TimeFadeupMode mode)
        {
            string text =
                //min
                ((timeMilliseconds / 1000) / 60) + ":" +
                //sec
                ((timeMilliseconds / 1000) % 60).ToString("00") + "." +
                //ms
                ((timeMilliseconds / 10) % 100).ToString("00");
            Color col = Color.White;
            if (mode == TimeFadeupMode.Plus)
            {
                text = "+ " + text;
                col = Color.Red;
            }
            else if (mode == TimeFadeupMode.Minus)
            {
                text = "- " + text;
                col = Color.Green;
            }

            fadeupTexts.Add(new TimeFadeupText(text, col));
        }

        /// <summary>
        /// Render all time fadeup effects, move them up and fade them out.
        /// </summary>
        public void RenderTimeFadeupEffects()
        {
            for (int num = 0; num < fadeupTexts.Count; num++)
            {
                TimeFadeupText fadeupText = fadeupTexts[num];
                fadeupText.showTimeMs -= BaseGame.ElapsedTimeThisFrameInMilliseconds;
                if (fadeupText.showTimeMs < 0)
                {
                    fadeupTexts.Remove(fadeupText);
                    num--;
                }
                else
                {
                    // Fade out
                    float alpha = 1.0f;
                    if (fadeupText.showTimeMs < 1500)
                        alpha = fadeupText.showTimeMs / 1500.0f;
                    // Move up
                    float moveUpAmount =
                        (TimeFadeupText.MaxShowTimeMs - fadeupText.showTimeMs) /
                        TimeFadeupText.MaxShowTimeMs;

                    // Calculate screen position
                    TextureFont.WriteTextCentered(BaseGame.Width / 2,
                        BaseGame.Height / 3 - (int)(moveUpAmount * BaseGame.Height / 3),
                        fadeupText.text,
                        ColorHelper.ApplyAlphaToColor(fadeupText.color, alpha),
                        2.25f);
                }
            }
        }
        #endregion

        #region Menu UI
        #region Render game background
        /// <summary>
        /// Render game background
        /// </summary>
        public void RenderGameBackground()
        {
            // Show game background (sky and lensflare)
            skyCube.RenderSky();

            // Render lens flare on top of 3d stuff
            if (Track.disableLensFlareInTunnel == false)
                lensFlare.Render(Color.White);
        }
        #endregion

        #region Render menu background
        private Vector3 oldCarForward = Vector3.Zero;
        private Vector3 oldCarUp = Vector3.Zero;
        private float carMenuTime = 0.0f;
        private Vector3 carPos = RacingGameManager.Player.CarPosition;
        private int randomCarNumber = RandomHelper.GetRandomInt(3);
        private Color randomCarColor = RandomHelper.RandomColor;

        /// <summary>
        /// Render menu track background
        /// </summary>
        public void RenderMenuTrackBackground()
        {
            // Only render track if we are not doing any 3d data on the screen
            if (RacingGameManager.InCarSelectionScreen == false)
            {
                RacingGameManager.Landscape.Render();

                RacingGameManager.CarModel.RenderCar(
                    randomCarNumber,
                    randomCarColor,
                    false,
                    RacingGameManager.Player.CarRenderMatrix);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateCarInMenu()
        {
            // Advance menu car preview time
            carMenuTime += BaseGame.ElapsedTimeThisFrameInMilliseconds / 1000.0f;
            if (carMenuTime > RacingGameManager.Landscape.BestReplay.LapTime)
                carMenuTime -= RacingGameManager.Landscape.BestReplay.LapTime;

            // Use data from replay
            Matrix carMatrix =
                RacingGameManager.Landscape.BestReplay.GetCarMatrixAtTime(
                carMenuTime);

            // Interpolate carPos a little
            carPos = carMatrix.Translation;

            // Set carPos for camera (else the car will drive away from us ^^)
            RacingGameManager.Player.SetCarPosition(
                carPos, carMatrix.Forward, carMatrix.Up);

            // Put camera behind car, but make it move smoothly
            Vector3 newCarForward = carMatrix.Forward;
            Vector3 newCarUp = carMatrix.Up;
            if (oldCarForward == Vector3.Zero)
                oldCarForward = newCarForward;
            if (oldCarUp == Vector3.Zero)
                oldCarUp = newCarUp;
            oldCarForward = oldCarForward * 0.95f + newCarForward * 0.05f;
            oldCarUp = oldCarUp * 0.95f + newCarUp * 0.05f;
            RacingGameManager.Player.SetCameraPosition(
                // Mix camera positions, interpolate slowly, much smoother camera!
                carPos + oldCarForward * 13 - oldCarUp * 1.3f);

            RacingGameManager.Player.Update();
        }

        /// <summary>
        /// Render menu background
        /// </summary>
        public void RenderMenuBackground()
        {
            BaseGame.UI.UpdateCarInMenu();

            // Render game background
            RenderGameBackground();

            // And show track
            RenderMenuTrackBackground();

            // Render background with transparency
            // The background itself should be rendered before this ^^
            background.RenderOnScreen(
                BaseGame.ResolutionRect, BackgroundGfxRect,
                ColorHelper.ApplyAlphaToColor(Color.White, 0.85f));

            // Show RacingGame logo bouncing with the music
            float bounceSize = 1.005f +
                (float)Math.Sin(BaseGame.TotalTime / 0.46f) * 0.045f *
                (float)Math.Cos(BaseGame.TotalTime / 0.285f);
            background.RenderOnScreen(
                BaseGame.CalcRectangleWithBounce(362, 36, 601, 218, bounceSize),
                RacingGameLogoGfxRect);
        }
        #endregion

        #region RenderBlackBar
        /// <summary>
        /// Render black bar
        /// </summary>
        /// <param name="yPos">Y position</param>
        /// <param name="height">Height</param>
        public void RenderBlackBar(int yPos, int height)
        {
            buttons.RenderOnScreen(
                BaseGame.CalcRectangle(0, yPos, 1024, height),
                BlackBarGfxRect,
                ColorHelper.ApplyAlphaToColor(Color.White, 0.85f));
        }
        #endregion

        #region RenderBottomButtons
        public bool backButtonPressed = false;
        /// <summary>
        /// Render bottom buttons (select, back, etc.)
        /// </summary>
        /// <param name="onlyBack">Only back</param>
        public bool RenderBottomButtons(bool onlyBack)
        {
            Rectangle bButtonRect = BaseGame.CalcRectangleCenteredWithGivenHeight(
                0, 587, 48, BottomButtonBButtonGfxRect);
            bButtonRect.X = BaseGame.Width - bButtonRect.Width - BaseGame.XToRes(25 + 25);
            bool overBButton = Input.MouseInBox(bButtonRect);
            int xAdd = BaseGame.XToRes(16);
            int yAdd = BaseGame.YToRes(9);
            if (overBButton)
                bButtonRect = new Rectangle(
                    bButtonRect.X - xAdd / 2, bButtonRect.Y - yAdd / 2,
                    bButtonRect.Width + xAdd, bButtonRect.Height + yAdd);
            buttons.RenderOnScreen(bButtonRect, BottomButtonBButtonGfxRect);

            // Is mouse over button?
            if (overBButton)
                buttons.RenderOnScreen(bButtonRect, BottomButtonSelectionGfxRect);

            // Store value if back button was pressed;
            backButtonPressed = overBButton && Input.MouseLeftButtonJustPressed;

            // Don't show button a if there is nothing to select here
            if (onlyBack)
                return false;

            Rectangle aButtonRect = BaseGame.CalcRectangleCenteredWithGivenHeight(
                0, 587, 48, BottomButtonAButtonGfxRect);
            aButtonRect.X = BaseGame.Width -
                aButtonRect.Width * 2 - BaseGame.XToRes(55 + 25);
            bool overAButton = Input.MouseInBox(aButtonRect);
            if (overAButton)
                aButtonRect = new Rectangle(
                    aButtonRect.X - xAdd / 2, aButtonRect.Y - yAdd / 2,
                    aButtonRect.Width + xAdd, aButtonRect.Height + yAdd);
            buttons.RenderOnScreen(aButtonRect, BottomButtonAButtonGfxRect);

            // Is mouse over button?
            if (overAButton)
            {
                buttons.RenderOnScreen(aButtonRect, BottomButtonSelectionGfxRect);
                if (Input.MouseLeftButtonJustPressed)
                    return true;
            }
            return false;
        }
        #endregion
        #endregion

        #region Game UI
        /// <summary>
        /// Render game user interface
        /// </summary>
        /// <param name="currentGameTime">Current game time</param>
        /// <param name="bestLapTime">Best lap time</param>
        /// <param name="lapNumber">Lap number</param>
        /// <param name="speed">Speed</param>
        /// <param name="gear">Gear</param>
        /// <param name="trackName">Track name</param>
        /// <param name="top5LapTimes">Top 5 lap times</param>
        public void RenderGameUI(int currentGameTime, int bestLapTime,
            int lapNumber, float speed, int gear, float acceleration,
            string trackName, int[] top5LapTimes)
        {
            if (top5LapTimes == null)
                throw new ArgumentNullException("top5LapTimes");

            Color baseUIColor = Color.White;//ColorHelper.HalfAlpha;

            // If game is over, set speed, gear and acceleration to 0
            if (RacingGameManager.Player.GameOver)
            {
                speed = 0;
                gear = 1;
                acceleration = 0;
            }

            // More distance to the screen borders on the Xbox 360 to fit better into
            // the save region. Calculate all rectangles for each platform,
            // then they will be used the same way on both platforms.
#if XBOX360
            // Draw all boxes and background stuff
            Rectangle lapsRect = BaseGame.CalcRectangle1600(
                60, 46, LapsGfxRect.Width, LapsGfxRect.Height);
            ingame.RenderOnScreen(lapsRect, LapsGfxRect, baseUIColor);
            
            Rectangle timesRect = BaseGame.CalcRectangle1600(
                60, 46, CurrentAndBestGfxRect.Width, CurrentAndBestGfxRect.Height);
            timesRect.Y = BaseGame.Height-timesRect.Bottom;
            ingame.RenderOnScreen(timesRect, CurrentAndBestGfxRect, baseUIColor);
            
            Rectangle trackNameRect = BaseGame.CalcRectangle1600(
                60, 46, TrackNameGfxRect.Width, TrackNameGfxRect.Height);
            trackNameRect.X = BaseGame.Width-trackNameRect.Right;
            ingame.RenderOnScreen(trackNameRect, TrackNameGfxRect, baseUIColor);
            Rectangle top5Rect1 = BaseGame.CalcRectangle1600(
                60, 4, Best5GfxRect.Width, Best5GfxRect.Height);
            top5Rect1.X = trackNameRect.X;
            int top5Distance = top5Rect1.Y;
            top5Rect1.Y += trackNameRect.Bottom;
            ingame.RenderOnScreen(top5Rect1, Best5GfxRect, baseUIColor);
            Rectangle top5Rect2 = new Rectangle(top5Rect1.X,
                top5Rect1.Bottom+top5Distance, top5Rect1.Width, top5Rect1.Height);
            ingame.RenderOnScreen(top5Rect2, Best5GfxRect, baseUIColor);
            Rectangle top5Rect3 = new Rectangle(top5Rect1.X,
                top5Rect2.Bottom+top5Distance, top5Rect1.Width, top5Rect1.Height);
            ingame.RenderOnScreen(top5Rect3, Best5GfxRect, baseUIColor);
            Rectangle top5Rect4 = new Rectangle(top5Rect1.X,
                top5Rect3.Bottom+top5Distance, top5Rect1.Width, top5Rect1.Height);
            ingame.RenderOnScreen(top5Rect4, Best5GfxRect, baseUIColor);
            Rectangle top5Rect5 = new Rectangle(top5Rect1.X,
                top5Rect4.Bottom+top5Distance, top5Rect1.Width, top5Rect1.Height);
            ingame.RenderOnScreen(top5Rect5, Best5GfxRect, baseUIColor);

            Rectangle tachoRect = BaseGame.CalcRectangle1600(
                60, 46, TachoGfxRect.Width, TachoGfxRect.Height);
            tachoRect.X = BaseGame.Width-tachoRect.Right;
            tachoRect.Y = BaseGame.Height-tachoRect.Bottom;
#else
            // Draw all boxes and background stuff
            Rectangle lapsRect = BaseGame.CalcRectangle1600(
                10, 10, LapsGfxRect.Width, LapsGfxRect.Height);
            ingame.RenderOnScreen(lapsRect, LapsGfxRect, baseUIColor);

            Rectangle timesRect = BaseGame.CalcRectangle1600(
                10, 10, CurrentAndBestGfxRect.Width, CurrentAndBestGfxRect.Height);
            timesRect.Y = BaseGame.Height - timesRect.Bottom;
            ingame.RenderOnScreen(timesRect, CurrentAndBestGfxRect, baseUIColor);

            Rectangle trackNameRect = BaseGame.CalcRectangle1600(
                10, 10, TrackNameGfxRect.Width, TrackNameGfxRect.Height);
            trackNameRect.X = BaseGame.Width - trackNameRect.Right;
            ingame.RenderOnScreen(trackNameRect, TrackNameGfxRect, baseUIColor);
            Rectangle top5Rect1 = BaseGame.CalcRectangle1600(
                10, 4, Best5GfxRect.Width, Best5GfxRect.Height);
            top5Rect1.X = trackNameRect.X;
            int top5Distance = top5Rect1.Y;
            top5Rect1.Y += trackNameRect.Bottom;
            ingame.RenderOnScreen(top5Rect1, Best5GfxRect, baseUIColor);
            Rectangle top5Rect2 = new Rectangle(top5Rect1.X,
                top5Rect1.Bottom + top5Distance, top5Rect1.Width, top5Rect1.Height);
            ingame.RenderOnScreen(top5Rect2, Best5GfxRect, baseUIColor);
            Rectangle top5Rect3 = new Rectangle(top5Rect1.X,
                top5Rect2.Bottom + top5Distance, top5Rect1.Width, top5Rect1.Height);
            ingame.RenderOnScreen(top5Rect3, Best5GfxRect, baseUIColor);
            Rectangle top5Rect4 = new Rectangle(top5Rect1.X,
                top5Rect3.Bottom + top5Distance, top5Rect1.Width, top5Rect1.Height);
            ingame.RenderOnScreen(top5Rect4, Best5GfxRect, baseUIColor);
            Rectangle top5Rect5 = new Rectangle(top5Rect1.X,
                top5Rect4.Bottom + top5Distance, top5Rect1.Width, top5Rect1.Height);
            ingame.RenderOnScreen(top5Rect5, Best5GfxRect, baseUIColor);

            Rectangle tachoRect = BaseGame.CalcRectangle1600(
                10, 10, TachoGfxRect.Width, TachoGfxRect.Height);
            tachoRect.X = BaseGame.Width - tachoRect.Right;
            tachoRect.Y = BaseGame.Height - tachoRect.Bottom;
#endif

            // Rest can stay the same because we use the rectangles from now on
            ingame.RenderOnScreen(tachoRect, TachoGfxRect, baseUIColor);

            // Ok, now add the numbers, text and speed values
            TextureFontBigNumbers.WriteNumber(
                lapsRect.X + BaseGame.XToRes1600(15),
                lapsRect.Y + BaseGame.YToRes1200(12),
                lapNumber);

            // Current and best game times
            Color highlightColor = new Color(255, 185, 80);
            int blockHeight = BaseGame.YToRes1200(74);
            TextureFont.WriteGameTime(
                timesRect.X + BaseGame.XToRes1600(154),
                timesRect.Y + blockHeight / 2 - TextureFont.Height / 2,
                currentGameTime,
                highlightColor);
            TextureFont.WriteGameTime(
                timesRect.X + BaseGame.XToRes1600(154),
                timesRect.Y + timesRect.Height / 2 + blockHeight / 2 -
                TextureFont.Height / 2,
                bestLapTime,
                Color.White);

            // Track name
            TextureFont.WriteTextCentered(
                trackNameRect.X + trackNameRect.Width / 2,
                trackNameRect.Y + blockHeight / 2,
                trackName);

            // Top 5
            // Possible improvement: Show currentRank here (insert us)
            Color rankColor =
                bestLapTime == top5LapTimes[0] ?
                highlightColor : Color.White;
            TextureFont.WriteTextCentered(
                top5Rect1.X + BaseGame.XToRes(32) / 2,
                top5Rect1.Y + blockHeight / 2,
                "1.", rankColor, 1.0f);
            TextureFont.WriteGameTime(
                top5Rect1.X + BaseGame.XToRes(35 + 15),
                top5Rect1.Y + blockHeight / 2 - TextureFont.Height / 2,
                top5LapTimes[0], rankColor);

            rankColor =
                bestLapTime == top5LapTimes[1] ?
                highlightColor : Color.White;
            TextureFont.WriteTextCentered(
                top5Rect2.X + BaseGame.XToRes(32) / 2,
                top5Rect2.Y + blockHeight / 2,
                "2.", rankColor, 1.0f);
            TextureFont.WriteGameTime(
                top5Rect2.X + BaseGame.XToRes(35 + 15),
                top5Rect2.Y + blockHeight / 2 - TextureFont.Height / 2,
                top5LapTimes[1], rankColor);

            rankColor =
                bestLapTime == top5LapTimes[2] ?
                highlightColor : Color.White;
            TextureFont.WriteTextCentered(
                top5Rect3.X + BaseGame.XToRes(32) / 2,
                top5Rect3.Y + blockHeight / 2,
                "3.", rankColor, 1.0f);
            TextureFont.WriteGameTime(
                top5Rect3.X + BaseGame.XToRes(35 + 15),
                top5Rect3.Y + blockHeight / 2 - TextureFont.Height / 2,
                top5LapTimes[2], rankColor);

            rankColor =
                bestLapTime == top5LapTimes[3] ?
                highlightColor : Color.White;
            TextureFont.WriteTextCentered(
                top5Rect4.X + BaseGame.XToRes(32) / 2,
                top5Rect4.Y + blockHeight / 2,
                "4.", rankColor, 1.0f);
            TextureFont.WriteGameTime(
                top5Rect4.X + BaseGame.XToRes(35 + 15),
                top5Rect4.Y + blockHeight / 2 - TextureFont.Height / 2,
                top5LapTimes[3], rankColor);

            rankColor =
                bestLapTime == top5LapTimes[4] ?
                highlightColor : Color.White;
            TextureFont.WriteTextCentered(
                top5Rect5.X + BaseGame.XToRes(32) / 2,
                top5Rect5.Y + blockHeight / 2,
                "5.", rankColor, 1.0f);
            TextureFont.WriteGameTime(
                top5Rect5.X + BaseGame.XToRes(35 + 15),
                top5Rect5.Y + blockHeight / 2 - TextureFont.Height / 2,
                top5LapTimes[4], rankColor);

            // Acceleration
            Point tachoPoint = new Point(
                tachoRect.X +
                BaseGame.XToRes1600(194),
                tachoRect.Y +
                BaseGame.YToRes1200(194));
            //ingame.RenderOnScreenWithRotation(
            //    tachoPoint, TachoArrowGfxRect, -acceleration*2);
            if (acceleration < 0)
                acceleration = 0;
            if (acceleration > 1)
                acceleration = 1;
            float rotation = -2.33f + acceleration * 2.5f;
            int tachoArrowWidth = BaseGame.XToRes1600(TachoArrowGfxRect.Width);
            int tachoArrowHeight = BaseGame.YToRes1200(TachoArrowGfxRect.Height);
            Vector2 rotationPoint = new Vector2(
                TachoArrowGfxRect.Width / 2,
                TachoArrowGfxRect.Height - 13);
            ingame.RenderOnScreenWithRotation(
                new Rectangle(tachoPoint.X, tachoPoint.Y,
                tachoArrowWidth, tachoArrowHeight),
                TachoArrowGfxRect,
                rotation, rotationPoint);

            // Speed in mph
            TextureFontBigNumbers.WriteNumber(
                tachoRect.X + BaseGame.XToRes1600(TachoMphGfxRect.X),
                tachoRect.Y + BaseGame.YToRes1200(TachoMphGfxRect.Y),
                TachoMphGfxRect.Height,
                (int)Math.Round(speed));

            // Gear
            TextureFontBigNumbers.WriteNumber(
                tachoRect.X + BaseGame.XToRes1600(TachoGearGfxRect.X),
                tachoRect.Y + BaseGame.YToRes1200(TachoGearGfxRect.Y),
                TachoGearGfxRect.Height,
                Math.Min(5, gear));
        }
        #endregion

        #region Render
        bool showFps =
#if DEBUG
 true;
#else
 false;
#endif

        /// <summary>
        /// Render all UI elements at the end of the frame, will also
        /// render the mouse cursor if we got a mouse attached.
        /// 
        /// Render all ui elements that we collected this frame.
        /// Flush user interface graphics, this are mainly all
        /// Texture.RenderOnScreen calls we made this frame so far.
        /// Used in UIRenderer.RenderTextsAndMouseCursor, but can also be
        /// called to force rendering UI at a specific point in the code.
        /// </summary>
        public static void Render(LineManager2D lineManager2D)
        {
            if (lineManager2D == null)
                throw new ArgumentNullException("lineManager2D");

            // Disable depth buffer for UI
            BaseGame.Device.DepthStencilState = DepthStencilState.None;

            // Draw all sprites
            Texture.additiveSprite.End();
            Texture.alphaSprite.End();
            //SpriteHelper.DrawAllSprites();

            // Render all 2d lines
            lineManager2D.Render();

            // Restore depth buffer?
            BaseGame.Device.DepthStencilState = DepthStencilState.Default;
        }

        /// <summary>
        /// Render texts and mouse cursor, which is done at the very end
        /// of our render loop.
        /// </summary>
        public void RenderTextsAndMouseCursor()
        {
#if DEBUG
            // Show fps
            if (Input.KeyboardF1JustPressed ||
                // Also allow toggeling with gamepad
                (Input.GamePad.Buttons.LeftShoulder == ButtonState.Pressed &&
                Input.GamePadYJustPressed))
                showFps = !showFps;
#endif
            if (showFps)
                TextureFont.WriteText(
                    BaseGame.XToRes(200), BaseGame.YToRes(26),
                    "Fps: " + BaseGame.Fps + " " +
                    BaseGame.Width + "x" + BaseGame.Height);

            // Render font texts
            RenderTimeFadeupEffects();
            font.WriteAll();

            // Render mouse
            // For the xbox, there is no mouse support, don't show cursor!
            if (Input.MouseDetected &&
                // Also don't show cursor in game!
                RacingGameManager.ShowMouseCursor)
            {
                Texture.alphaSprite.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                Texture.additiveSprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                // Use our SpriteHelper logic to render the mouse cursor now!
                mouseCursor.RenderOnScreen(Input.MousePos);

                Texture.additiveSprite.End();
                Texture.alphaSprite.End();

                //SpriteHelper.DrawAllSprites();
            }
        }
        #endregion
    }
}