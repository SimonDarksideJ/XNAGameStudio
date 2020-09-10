#region File Description
//-----------------------------------------------------------------------------
// PlayingScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Pickture
{
    /// <summary>
    /// The main game screen. This is where the player spends most of his or her time,
    /// trying to solve the puzzle!
    /// </summary>
    class PlayingScreen : GameScreen
    {
        Board board;

        // Coordinates of the chip focused for GamePad input
        int activeChipX;
        int activeChipY;

        /// <summary>
        /// The chip focused for GamePad input.
        /// </summary>
        public Chip ActiveChip
        {
            get
            {
                return board.GetChip(activeChipX, activeChipY);
            }
        }

        public PlayingScreen(Board board)
        {
            this.board = board;

            // Always start the active chip on the top row
            activeChipY = board.Height - 1;

            // And the left most chip on that row
            if (board.TwoSided)
            {
                // When two sided, the Shuffling screen will have flipped the board, so
                // the left side of the view is really the high end of the board
                activeChipX = board.Width - 1;
                if (ActiveChip == null)
                    activeChipX--;
            }
            else
            {
                activeChipX = 0;
                if (ActiveChip == null)
                    activeChipX++;
            }

            // Show through to the BoardScreen
            IsPopup = true;
        }

        public override void Update(GameTime gameTime,
            bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);            

            if (ActiveChip != null)
            {
                // Highlight the active chip with a pulsating glow
                double elapsed = gameTime.TotalRealTime.TotalSeconds;
                ActiveChip.GlowScale =
                    ((float)Math.Sin(elapsed * Math.PI) + 1.0f) / 2.0f;
            }
        }

        public override void HandleInput(InputState input)
        {
            if (input.MenuCancel)
            {
                MessageBoxScreen messageBox = new MessageBoxScreen(
                "Are you sure you want to quit this game?\nYour progress will be lost");

                messageBox.Accepted += QuitMessageBoxAccepted;
                ScreenManager.AddScreen(messageBox);
            }

            // Do not handle controls while the camera is flipping
            if (board.Camera.IsFlipping)
                return;

            // Some controls only apply when the puzzle is two sided
            if (board.TwoSided)
            {
                // Handle flipping camera to other side of board
                if (input.FlipCameraLeft)
                {
                    board.Camera.Flip(Camera.FlipDirection.Left);
                }
                else if (input.FlipCameraRight)
                {
                    board.Camera.Flip(Camera.FlipDirection.Right);
                }                
                // Handle flipping the active chip
                else if (input.FlipUp)
                {
                    if (board.Camera.Side == Camera.BoardSide.Front)
                        ActiveChip.Flip(Chip.RevolveDirection.Up);
                    else
                        ActiveChip.Flip(Chip.RevolveDirection.Down);
                }
                else if (input.FlipDown)
                {
                    if (board.Camera.Side == Camera.BoardSide.Front)
                        ActiveChip.Flip(Chip.RevolveDirection.Down);
                    else
                        ActiveChip.Flip(Chip.RevolveDirection.Up);
                }
                else if (input.FlipLeft)
                {
                    ActiveChip.Flip(Chip.RevolveDirection.Left);
                }
                else if (input.FlipRight)
                {
                    ActiveChip.Flip(Chip.RevolveDirection.Right);
                }
            }

            // Handle selecting a new active chip
            HandleChipSelection(input);

            // Handle shifting
            if (input.ShiftActiveChip && !board.IsShifting)
            {
                board.Shift(activeChipX, activeChipY);
                if (board.IsShifting)
                {
                    // When a shift occurs, the active chip moves with the shift
                    SetActiveChip(activeChipX + board.ShiftX,
                        activeChipY + board.ShiftY);
                }
            }

            // Did any moves during this update result in the puzzle being sovled?
            if (this.board.IsPuzzleComplete())
            {
                // Transition to the CompletedScreen
                ExitScreen();
                foreach (GameScreen screen in ScreenManager.GetScreens())
                {
                    if (screen is BoardScreen)
                        screen.ExitScreen();
                }
                ScreenManager.AddScreen(new CompletedScreen(board.CurrentPictureSet));
            }
        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box.
        /// </summary>
        void QuitMessageBoxAccepted(object sender, EventArgs e)
        {
            ExitToMenu();
        }

        /// <summary>
        /// Exits this screen and the board screen below it.
        /// </summary>
        void ExitToMenu()
        {
            ExitScreen();
            foreach (GameScreen screen in ScreenManager.GetScreens())
            {
                if (screen is BoardScreen)
                    screen.ExitScreen();
            }

            ScreenManager.AddScreen(new MainMenuScreen());
        }

        void HandleChipSelection(InputState input)
        {
            int moveX = 0;
            int moveY = 0;

            // Determine a movement vector
            if (input.MenuUp)
                moveY = 1;
            else if (input.MenuDown)
                moveY = -1;
            else if (input.MenuLeft)
                moveX = -1;
            else if (input.MenuRight)
                moveX = 1;

            // Reflect the X axis when looking at the back side of the board
            if (board.Camera.Side == Camera.BoardSide.Back)
                moveX *= -1;

            // Try to move in the desired direction. Attempts up to two steps to
            // support skipping over the blank space.
            for (int i = 1; i <= 2; i++)
            {
                int x = activeChipX + moveX * i;
                int y = activeChipY + moveY * i;

                if (0 <= x && x < board.Width &&
                    0 <= y && y < board.Height &&
                    board.GetChip(x, y) != null)
                {
                    SetActiveChip(x, y);
                    break;
                }
            }
        }

        void SetActiveChip(int x, int y)
        {
            if (activeChipX == x && activeChipY == y)
                return;

            // Remove any glow from the no-longer active chip
            if (ActiveChip != null)
                ActiveChip.GlowScale = 0.0f;

            activeChipX = x;
            activeChipY = y;
        }
    }
}
