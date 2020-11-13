#region File Description
//-----------------------------------------------------------------------------
// MainMenu.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Sounds;
#endregion

namespace RacingGame.GameScreens
{
    /// <summary>
    /// Main menu
    /// </summary>
    class MainMenu : IGameScreen
    {
        #region Constants
        static readonly Rectangle[] ButtonRects = new Rectangle[]
            {
                UIRenderer.MenuButtonPlayGfxRect,
                UIRenderer.MenuButtonHighscoresGfxRect,
                UIRenderer.MenuButtonOptionsGfxRect,
                UIRenderer.MenuButtonHelpGfxRect,
                UIRenderer.MenuButtonQuitGfxRect,
            };
        static readonly Rectangle[] TextRects = new Rectangle[]
            {
                UIRenderer.MenuTextPlayGfxRect,
                UIRenderer.MenuTextHighscoresGfxRect,
                UIRenderer.MenuTextOptionsGfxRect,
                UIRenderer.MenuTextHelpGfxRect,
                UIRenderer.MenuTextQuitGfxRect,
            };
        const int NumberOfButtons = 5,
            ActiveButtonWidth = 132,
            InactiveButtonWidth = 108,
            DistanceBetweenButtons = 14;

        /// <summary>
        /// The amount of time idle at the menu before returning to the splash screen
        /// </summary>
        const float TimeOutMenu = 60000.0f;
        #endregion

        #region Variables
        /// <summary>
        /// Start with button 0 being selected (play game)
        /// </summary>
        int selectedButton = 0;

        private int SelectedButton
        {
            get
            {
                return selectedButton;
            }

            set
            {
                selectedButton = value;
                idleTime = 0.0f;
            }
        }

        /// <summary>
        /// Current button sizes for scaling up/down smooth effect.
        /// </summary>
        float[] currentButtonSizes =
            new float[NumberOfButtons] { 1, 0, 0, 0, 0 };

        /// <summary>
        /// Ignore the mouse unless it moves;
        /// this is so the mouse does not disrupt game pads and keyboard
        /// </summary>
        bool ignoreMouse = true;

        float idleTime = 0.0f;

		bool musicHasStarted = false;
        #endregion

		#region Constructor
		public MainMenu()
		{

		}
		#endregion

		#region Update
		/// <summary>
		/// Handle starting the menu music.
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update(GameTime gameTime)
		{
			// Start playing the menu music
			if (!musicHasStarted)
			{
				Sound.Play(Sound.Sounds.MenuMusic);
				musicHasStarted = true;
			}
		}
		#endregion

		#region Render
		/// <summary>
        /// Interpolate rectangle
        /// </summary>
        /// <param name="rect1">Rectangle 1</param>
        /// <param name="rect2">Rectangle 2</param>
        /// <param name="interpolation">Interpolation</param>
        /// <returns>Rectangle</returns>
        internal static Rectangle InterpolateRect(
            Rectangle rect1, Rectangle rect2,
            float interpolation)
        {
            return new Rectangle(
                (int)Math.Round(
                rect1.X * interpolation + rect2.X * (1 - interpolation)),
                (int)Math.Round(
                rect1.Y * interpolation + rect2.Y * (1 - interpolation)),
                (int)Math.Round(
                rect1.Width * interpolation + rect2.Width * (1 - interpolation)),
                (int)Math.Round(
                rect1.Height * interpolation + rect2.Height * (1 - interpolation)));
        }

        float pressedLeftMs = 0;
        float pressedRightMs = 0;
        /// <summary>
        /// Render
        /// </summary>
        /// <returns>Bool</returns>
        public bool Render()
        {
            // This starts both menu and in game post screen shader!
            BaseGame.UI.PostScreenMenuShader.Start();

            // Render background and black bar
            BaseGame.UI.RenderMenuBackground();
            BaseGame.UI.RenderBlackBar(280, 192);

            // Show logos
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
                0, 0, ActiveButtonWidth, ButtonRects[0]);
            Rectangle inactiveRect = BaseGame.CalcRectangleCenteredWithGivenHeight(
                0, 0, InactiveButtonWidth, ButtonRects[0]);
            int totalWidth = activeRect.Width +
                (NumberOfButtons - 1) * inactiveRect.Width +
                (NumberOfButtons - 1) * BaseGame.XToRes(DistanceBetweenButtons);
            int xPos = BaseGame.XToRes(512) - totalWidth / 2;
            int yPos = BaseGame.YToRes(316);

            for (int num = 0; num < NumberOfButtons; num++)
            {
                // Is this button currently selected?
                bool selected = num == SelectedButton;

                // Increase size if selected, decrease otherwise
                currentButtonSizes[num] +=
                    (selected ? 1 : -1) * BaseGame.MoveFactorPerSecond * 2;
                if (currentButtonSizes[num] < 0)
                    currentButtonSizes[num] = 0;
                if (currentButtonSizes[num] > 1)
                    currentButtonSizes[num] = 1;

                // Use this size to build rect
                Rectangle thisRect =
                    InterpolateRect(activeRect, inactiveRect, currentButtonSizes[num]);
                Rectangle renderRect = new Rectangle(
                    xPos, yPos - (thisRect.Height - inactiveRect.Height) / 2,
                    thisRect.Width, thisRect.Height);
                BaseGame.UI.Buttons.RenderOnScreen(renderRect, ButtonRects[num],
                    selected ? Color.White : new Color(192, 192, 192, 192));

                // Add border effect if selected
                if (selected)
                    BaseGame.UI.Buttons.RenderOnScreen(renderRect,
                        UIRenderer.MenuButtonSelectionGfxRect);

                // Also add text below button
                Rectangle textRenderRect = new Rectangle(
                    xPos, renderRect.Bottom + BaseGame.YToRes(5),
                    renderRect.Width,
                    renderRect.Height * TextRects[0].Height / ButtonRects[0].Height);
                if (selected)
                    BaseGame.UI.Buttons.RenderOnScreen(textRenderRect, TextRects[num],
                        selected ? Color.White : new Color(192, 192, 192, 192));

                // Also check if the user hovers with the mouse over this button
                if (Input.MouseInBox(renderRect))
                    mouseIsOverButton = num;

                xPos += thisRect.Width + BaseGame.XToRes(DistanceBetweenButtons);
            }

            if (!ignoreMouse && mouseIsOverButton >= 0)
                SelectedButton = mouseIsOverButton;

            // Handle input, we have 2 modes: Immediate response if one of the
            // keys is pressed to move our selected menu entry and after a timeout we
            // move even more. Controlling feels more natural this way.
            if (Input.KeyboardLeftPressed ||
                Input.GamePadLeftPressed)
                pressedLeftMs += BaseGame.ElapsedTimeThisFrameInMilliseconds;
            else
                pressedLeftMs = 0;
            if (Input.KeyboardRightPressed ||
                Input.GamePadRightPressed)
                pressedRightMs += BaseGame.ElapsedTimeThisFrameInMilliseconds;
            else
                pressedRightMs = 0;

            // Handle GamePad input, and also allow keyboard input
            if (Input.GamePadLeftJustPressed ||
                Input.KeyboardLeftJustPressed ||
                (pressedLeftMs > 250 &&
                (Input.KeyboardLeftPressed ||
                Input.GamePadLeftPressed)))
            {
                pressedLeftMs -= 250;
                Sound.Play(Sound.Sounds.Highlight);
                SelectedButton =
                    (SelectedButton + NumberOfButtons - 1) % NumberOfButtons;
                ignoreMouse = true;
            }
            else if (Input.GamePadRightJustPressed ||
                Input.KeyboardRightJustPressed ||
                (pressedRightMs > 250 &&
                (Input.KeyboardRightPressed ||
                Input.GamePadRightPressed)))
            {
                pressedRightMs -= 250;
                Sound.Play(Sound.Sounds.Highlight);
                SelectedButton = (SelectedButton + 1) % NumberOfButtons;
                ignoreMouse = true;
            }

            // If user presses the mouse button or the game pad A or Space,
            // start the game screen for the currently selected game part.
            if ((mouseIsOverButton >= 0 && Input.MouseLeftButtonJustPressed) ||
                Input.GamePadAJustPressed ||
                Input.KeyboardSpaceJustPressed)
            {
                idleTime = 0.0f;

                // Start game screen
                switch (SelectedButton)
                {
                    case 0:
                        RacingGameManager.AddGameScreen(new CarSelection());
                        break;
                    case 1:
                        RacingGameManager.AddGameScreen(new Highscores());
                        break;
                    case 2:
                        RacingGameManager.AddGameScreen(new Options());
                        break;
                    case 3:
                        RacingGameManager.AddGameScreen(new Help());
                        break;
                    case 4:
                        // Exit
                        return true;
                }
            }

            if (Input.KeyboardEscapeJustPressed ||
                Input.GamePadBackJustPressed)
                return true;

            // If the game sits idle at the menu for too long,
            // return to the splash screen
            idleTime += BaseGame.ElapsedTimeThisFrameInMilliseconds;
            if (idleTime > TimeOutMenu)
            {
                idleTime = 0.0f;
                RacingGameManager.AddGameScreen(new SplashScreen());
            }

            return false;
        }
        #endregion
    }
}
