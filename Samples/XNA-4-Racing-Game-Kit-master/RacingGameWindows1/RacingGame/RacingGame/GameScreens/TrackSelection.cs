#region File Description
//-----------------------------------------------------------------------------
// TrackSelection.cs
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
using RacingGame.Helpers;
using RacingGame.Properties;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Sounds;
using RacingGame.Landscapes;
#endregion

namespace RacingGame.GameScreens
{
    /// <summary>
    /// Track selection screen
    /// </summary>
    /// <returns>IGame screen</returns>
    class TrackSelection : IGameScreen
    {
        #region Constants
        static readonly Rectangle[] ButtonRects = new Rectangle[]
            {
                UIRenderer.TrackButtonBeginnerGfxRect,
                UIRenderer.TrackButtonAdvancedGfxRect,
                UIRenderer.TrackButtonExpertGfxRect,
            };
        static readonly Rectangle[] TextRects = new Rectangle[]
            {
                UIRenderer.TrackTextBeginnerGfxRect,
                UIRenderer.TrackTextAdvancedGfxRect,
                UIRenderer.TrackTextExpertGfxRect,
            };
        const int NumberOfButtons = 3,
            ActiveButtonWidth = 132,
            InactiveButtonWidth = 108,
            DistanceBetweenButtons = 32;
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

        #region Render
        /// <summary>
        /// Start with button 0 being selected (beginner track)
        /// Update: Now use advanced track as default, looks better in replays.
        /// </summary>
        static int selectedButton = 1;

        /// <summary>
        /// Selected track number
        /// </summary>
        /// <returns>Int</returns>
        static public int SelectedTrackNumber
        {
            get
            {
                return selectedButton;
            }
        }

        /// <summary>
        /// Selected track
        /// </summary>
        /// <returns>Track level</returns>
        static public RacingGameManager.Level SelectedTrack
        {
            get
            {
                return (RacingGameManager.Level)selectedButton;
            }
        }

        /// <summary>
        /// Current button sizes for scaling up/down smooth effect.
        /// </summary>
        float[] currentButtonSizes =
            new float[NumberOfButtons] { 1, 0, 0 };

        /// <summary>
        /// Ignore the mouse unless it moves;
        /// this is so the mouse does not disrupt game pads and keyboard
        /// </summary>
        bool ignoreMouse = true;

        /// <summary>
        /// Render game screen. Called each frame.
        /// </summary>
        public bool Render()
        {
            // This starts both menu and in game post screen shader!
            BaseGame.UI.PostScreenMenuShader.Start();

            // Render background and black bar
            BaseGame.UI.RenderMenuBackground();
            BaseGame.UI.RenderBlackBar(220, 280);

            // Track header
            int posX = 10;
            int posY = 18;
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                posX += 36;
                posY += 26;
            }
            BaseGame.UI.Headers.RenderOnScreenRelative1600(
                posX, posY, UIRenderer.HeaderSelectTrackGfxRect);

            // Little helper to keep track if mouse is actually over a button.
            // Required because buttons are selected even when not hovering over
            // them for GamePad support, but we still want the mouse only to
            // be apply when we are actually over the button.
            int mouseIsOverButton = -1;

            // If the user manipulated the mouse, stop ignoring the mouse
            // This allows the mouse to override the game pad or keyboard selection
            if (Input.HasMouseMoved || Input.MouseLeftButtonJustPressed)
                ignoreMouse = false;

            // Show buttons
            // Part 1: Calculate global variables for our buttons
            Rectangle activeRect = BaseGame.CalcRectangleCenteredWithGivenHeight(
                0, 0,
                ActiveButtonWidth * ButtonRects[0].Height / ButtonRects[0].Width,
                ButtonRects[0]);
            Rectangle inactiveRect = BaseGame.CalcRectangleCenteredWithGivenHeight(
                0, 0,
                InactiveButtonWidth * ButtonRects[0].Height / ButtonRects[0].Width,
                ButtonRects[0]);
            int totalWidth = activeRect.Width +
                2 * inactiveRect.Width +
                2 * BaseGame.XToRes(DistanceBetweenButtons);
            int xPos = BaseGame.XToRes(512) - totalWidth / 2;
            int yPos = BaseGame.YToRes(258);
            for (int num = 0; num < NumberOfButtons; num++)
            {
                // Is this button currently selected?
                bool selected = num == selectedButton;

                // Increase size if selected, decrease otherwise
                currentButtonSizes[num] +=
                    (selected ? 1 : -1) * BaseGame.MoveFactorPerSecond * 2;
                if (currentButtonSizes[num] < 0)
                    currentButtonSizes[num] = 0;
                if (currentButtonSizes[num] > 1)
                    currentButtonSizes[num] = 1;

                Rectangle thisRect = MainMenu.
                    InterpolateRect(activeRect, inactiveRect, currentButtonSizes[num]);
                Rectangle renderRect = new Rectangle(
                    xPos, yPos - (thisRect.Height - inactiveRect.Height) / 2,
                    thisRect.Width, thisRect.Height);
                BaseGame.UI.Buttons.RenderOnScreen(renderRect, ButtonRects[num],
                    selected ? Color.White : new Color(192, 192, 192, 192));

                // Add border effect if selected
                if (selected)
                    BaseGame.UI.Buttons.RenderOnScreen(renderRect,
                        UIRenderer.TrackButtonSelectionGfxRect);

                // Also add text below button
                Rectangle textRenderRect = new Rectangle(
                    xPos, renderRect.Bottom + BaseGame.YToRes(5),
                    renderRect.Width,
                    renderRect.Height * TextRects[0].Height / ButtonRects[0].Height);
                if (selected)
                    BaseGame.UI.Buttons.RenderOnScreen(textRenderRect, TextRects[num],
                        selected ? Color.White : Color.Gray);

                // Also check if the user hovers with the mouse over this button
                if (Input.MouseInBox(renderRect))
                    mouseIsOverButton = num;

                xPos += thisRect.Width + BaseGame.XToRes(DistanceBetweenButtons);
            }

            if (!ignoreMouse && mouseIsOverButton >= 0)
                selectedButton = mouseIsOverButton;

            // Handle GamePad input, and also allow keyboard input
            if (Input.GamePadLeftJustPressed ||
                Input.KeyboardLeftJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                selectedButton =
                    (selectedButton + NumberOfButtons - 1) % NumberOfButtons;
                ignoreMouse = true;
            }
            else if (Input.GamePadRightJustPressed ||
                Input.KeyboardRightJustPressed)
            {
                Sound.Play(Sound.Sounds.ButtonClick);
                selectedButton = (selectedButton + 1) % NumberOfButtons;
                ignoreMouse = true;
            }

            bool aButtonPressed = BaseGame.UI.RenderBottomButtons(false);
            // If user presses the mouse button or the game pad A or Space,
            // start the game screen for the currently selected game part.
            if ((mouseIsOverButton >= 0 && Input.MouseLeftButtonJustPressed) ||
                aButtonPressed ||
                Input.GamePadAJustPressed ||
                Input.KeyboardSpaceJustPressed)
            {
                // Track selection is handled through SelectedTrackNumber
                RacingGameManager.AddGameScreen(new GameScreen());
            }

            if (Input.KeyboardEscapeJustPressed ||
                Input.GamePadBJustPressed ||
                Input.GamePadBackJustPressed ||
                BaseGame.UI.backButtonPressed)
                return true;

            return false;
        }
        #endregion
    }
}
