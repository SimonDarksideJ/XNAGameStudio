#region File Description
//-----------------------------------------------------------------------------
// LocalPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Minjie
{
    /// <summary>
    /// Player controlled by local input.
    /// </summary>
    class LocalPlayer : Player
    {
        private static InputState inputState;
        public static InputState InputState
        {
            get { return inputState; }
            set { inputState = value; }
        }

        private static Point cursorPosition;
        public static Point CursorPosition
        {
            get { return cursorPosition; }
        }

        private PlayerIndex playerIndex;
        private bool checkValidMove = true;


        /// <summary>
        /// Create a new LocalPlayer object.
        /// </summary>
        /// <param name="boardColor">The color of this player.</param>
        /// <param name="playerIndex">The player index for this color.</param>
        /// <param name="boardSize"></param>
        public LocalPlayer(BoardColors boardColor, PlayerIndex playerIndex, 
            int boardSize)
            : base(boardColor) 
        {
            if ((playerIndex != PlayerIndex.One) && (playerIndex != PlayerIndex.Two))
            {
                throw new ArgumentException("Invalid player index.");
            }
            this.playerIndex = playerIndex;

            // start in the center of the board
            cursorPosition = new Point(boardSize / 2, boardSize / 2);
        }


        public override void Update(Board board)
        {
            // safety check the parameter
            if (board == null)
            {
                throw new ArgumentNullException("board");
            }
            // quit now if it's not my turn
            if (board.CurrentColor != BoardColor)
            {
                return;
            }

            // check if we have a valid move
            if (checkValidMove)
            {
                if (board.HasValidMove(BoardColor))
                {
                    checkValidMove = false; // already checked, waiting for selection
                }
                else
                {
                    board.Pass(BoardColor);
                }
            }

            // if we don't have any input, don't bother
            if (inputState == null)
            {
                return;
            }

            // apply selection-movement input and generate a desired movement
            Point cursorMovement = new Point(0, 0);
            float degrees = (int)SimpleArcCamera.Rotation % 360 - 180;
            if (Math.Abs(degrees) < 45)
            {
                if (inputState.IsPieceSelectionUp(playerIndex))
                {
                    cursorMovement.Y--;
                }
                if (inputState.IsPieceSelectionDown(playerIndex))
                {
                    cursorMovement.Y++;
                }
                if (inputState.IsPieceSelectionLeft(playerIndex))
                {
                    cursorMovement.X--;
                }
                if (inputState.IsPieceSelectionRight(playerIndex))
                {
                    cursorMovement.X++;
                }
            }
            else if ((degrees >= 45) && (degrees < 135))
            {
                if (inputState.IsPieceSelectionUp(playerIndex))
                {
                    cursorMovement.X++;
                }
                if (inputState.IsPieceSelectionDown(playerIndex))
                {
                    cursorMovement.X--;
                }
                if (inputState.IsPieceSelectionLeft(playerIndex))
                {
                    cursorMovement.Y--;
                }
                if (inputState.IsPieceSelectionRight(playerIndex))
                {
                    cursorMovement.Y++;
                }
            }
            else if (Math.Abs(degrees) >= 135)
            {
                if (inputState.IsPieceSelectionUp(playerIndex))
                {
                    cursorMovement.Y++;
                }
                if (inputState.IsPieceSelectionDown(playerIndex))
                {
                    cursorMovement.Y--;
                }
                if (inputState.IsPieceSelectionLeft(playerIndex))
                {
                    cursorMovement.X++;
                }
                if (inputState.IsPieceSelectionRight(playerIndex))
                {
                    cursorMovement.X--;
                }
            }
            else if ((degrees > -135) && (degrees <= -45))
            {
                if (inputState.IsPieceSelectionUp(playerIndex))
                {
                    cursorMovement.X--;
                }
                if (inputState.IsPieceSelectionDown(playerIndex))
                {
                    cursorMovement.X++;
                }
                if (inputState.IsPieceSelectionLeft(playerIndex))
                {
                    cursorMovement.Y++;
                }
                if (inputState.IsPieceSelectionRight(playerIndex))
                {
                    cursorMovement.Y--;
                }
            }
            // check for valid move and apply
            if (board.IsValidSpace(cursorPosition.X + cursorMovement.X, 
                cursorPosition.Y + cursorMovement.Y))
            {
                cursorPosition.X += cursorMovement.X;
                cursorPosition.Y += cursorMovement.Y;
            }

            // apply play-piece input
            if (inputState.IsPlayPiece(playerIndex))
            {
                Move move = new Move(cursorPosition.X, cursorPosition.Y);
                if (board.IsValidMove(BoardColor, move))
                {
                    board.ApplyMove(BoardColor, move);
                    checkValidMove = true;
                }
                else
                {
                    AudioManager.PlayCue("Drop_Illegal");
                }
            }
        }
    }
}
