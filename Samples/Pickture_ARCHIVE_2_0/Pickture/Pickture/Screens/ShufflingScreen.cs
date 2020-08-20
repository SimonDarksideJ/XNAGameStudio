#region File Description
//-----------------------------------------------------------------------------
// ShufflingScreen.cs
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Pickture
{
    /// <summary>
    /// Right before the player attempts to solve the puzzle, the puzzle is shown
    /// completed (both sides if applicable) and then is animated shuffling.
    /// </summary>
    class ShufflingScreen : GameScreen
    {
        Board board;

        // Amount of the the player has been studying the board
        float studyTime;
        // The max amount of time the player may study a side of the board
        const float sideStudyDuration = 30.0f;

        // False when studying, true once shuffling has begun
        bool isShuffling = false;

        // Shuffling occurs until some fixed number of shifts have been performed
        int shiftsRemaining;

        // While chips are being shuffled, they can also be flipped at a max rate
        TimeSpan timeBetweenFlips;
        TimeSpan lastFlipTime;

        /// <summary>
        /// Max portion of the chips which should be flipping simultaneously.
        /// </summary>
        const float MaxFlippingPortion = 0.25f;


        public ShufflingScreen(Board board)
        {
            this.board = board;

            // Calculate how many shifts to perform for the shuffle
            shiftsRemaining = board.Width * board.Height * 2;

            // Determine the interval of time between flips
            // This interval is based on the max portion of the board which will be
            // allowed to flip simultaneously
            timeBetweenFlips = TimeSpan.FromSeconds(Chip.FlipDuration /
                    (board.Width * board.Height * MaxFlippingPortion));    

            // Show the board on the screen under this
            IsPopup = true;
        }

        public override void Update(GameTime gameTime,
            bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // If studying the board
            if (!isShuffling)
            {
                studyTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (board.TwoSided)
                {
                    // If player has used all the time to study both sides
                    if (studyTime > 2 * sideStudyDuration)
                    {
                        isShuffling = true;
                    }
                    // If player has used all the time to study the first side
                    else if (studyTime > sideStudyDuration &&
                        board.Camera.Side == Camera.BoardSide.Front)
                    {
                        // Show the other side of the board
                        if (RandomHelper.NextBool())
                            board.Camera.Flip(Camera.FlipDirection.Right);
                        else
                            board.Camera.Flip(Camera.FlipDirection.Left);
                    }
                }
                else
                {
                    // If player has used all the time to study the only side
                    if (studyTime > sideStudyDuration)
                        isShuffling = true;
                }
            }

            if (isShuffling)
            {
                if (shiftsRemaining > 0 && !board.IsShifting)
                {
                    int x, y;

                    // Alternate horizontal and vertical shifts
                    if (shiftsRemaining % 2 == 0)
                    {
                        // Select a random non-empty spot to shift from along the X axis
                        x = RandomHelper.Random.Next(board.Width - 1);
                        if (x >= board.EmptyX)
                            x++;
                        y = board.EmptyY;
                    }
                    else
                    {
                        // Select a random non-empty spot to shift from along the Y axis
                        x = board.EmptyX;
                        y = RandomHelper.Random.Next(board.Height - 1);
                        if (y >= board.EmptyY)
                            y++;
                    }

                    board.Shift(x, y);
                    shiftsRemaining--;
                }

                // If enough time has passed since the last flip
                if (board.TwoSided &&
                    (gameTime.TotalGameTime - lastFlipTime) > timeBetweenFlips)
                {
                    // Flip a random chip in a random direction

                    int x = RandomHelper.Random.Next(board.Width);
                    int y = RandomHelper.Random.Next(board.Height);

                    Chip chip = board.GetChip(x, y);
                    if (chip != null)
                        chip.Flip(Chip.GetRandomDirection());

                    lastFlipTime = gameTime.TotalGameTime;
                }

                // When done shifting, transition to the playing screen
                if (shiftsRemaining == 0)
                {
                    ExitScreen();
                    ScreenManager.AddScreen(new PlayingScreen(board));
                }
            }            
            
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputState input)
        {
            // Allow the player to skip study time
            if (!isShuffling && input.MenuSelect)
            {
                if (studyTime < sideStudyDuration)
                    studyTime = sideStudyDuration;
                else if (studyTime < sideStudyDuration * 2)
                    studyTime = sideStudyDuration * 2;
            }
            else if (input.MenuCancel)
            {
                ExitToMenu();
            }
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
    }
}