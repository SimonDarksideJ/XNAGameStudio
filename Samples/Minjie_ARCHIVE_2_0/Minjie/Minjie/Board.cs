#region File Description
//-----------------------------------------------------------------------------
// Board.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#endregion

namespace Minjie
{
    class Board
    {
        private BoardColors[,] spaces;
        public BoardColors this[int row, int column]
        {
            get
            {
                // safety-check the parameters
                if (!IsValidSpace(row, column))
                {
                    throw new ArgumentException("Invalid space.");
                }
                // retrieve the value
                // -- the borders are an internal implementation detail, ignored here
                return spaces[row + 1, column + 1];
            }
            private set
            {
                // safety-check the parameters
                if (!IsValidSpace(row, column))
                {
                    throw new ArgumentException("Invalid space.");
                }

                // check the old value and decrement accordingly
                switch (spaces[row + 1, column + 1])
                {
                    case BoardColors.White:
                        whitePieceCount--;
                        break;

                    case BoardColors.Black:
                        blackPieceCount--;
                        break;
                }

                // assign the new value
                spaces[row + 1, column + 1] = value;

                // check the new value and increment accordingly
                switch (spaces[row + 1, column + 1])
                {
                    case BoardColors.White:
                        whitePieceCount++;
                        break;

                    case BoardColors.Black:
                        blackPieceCount++;
                        break;
                }
            }
        }

        private int boardSize;
        public int BoardSize
        {
            get { return boardSize; }
        }

        private int whitePieceCount = 0;
        public int WhitePieceCount
        {
            get { return whitePieceCount; }
        }

        private int blackPieceCount = 0;
        public int BlackPieceCount
        {
            get { return blackPieceCount; }
        }

        public int PieceCount
        {
            get { return WhitePieceCount + BlackPieceCount; }
        }

        private BoardColors currentColor = BoardColors.White;
        public BoardColors CurrentColor
        {
            get { return currentColor; }
        }

        private Move lastMove;
        public Move LastMove
        {
            get { return lastMove; }
        }


        private bool whitePassed = false, blackPassed = false;
        public bool GameOver
        {
            get
            {
                return ((whitePieceCount + blackPieceCount >= boardSize * boardSize) ||
                    (whitePassed && blackPassed));
            }
        }


        /// <summary>
        /// Construct a new Board object.
        /// </summary>
        /// <param name="boardSize">The size of the board, which must be even.</param>
        public Board(int boardSize)
        {
            // safety check on the parameter
            if (boardSize <= 0)
            {
                throw new ArgumentOutOfRangeException("boardSize");
            }
            // the board size must be even
            if ((boardSize % 2) != 0)
            {
                throw new ArgumentException("The board size must be even.");
            }

            this.boardSize = boardSize;

            // create a new array of spaces
            // -- a border is added to the size to simplify boundary checking
            spaces = new BoardColors[boardSize + 2, boardSize + 2];
        }




        /// <summary>
        /// Initialize the board to the standard starting positions.
        /// </summary>
        public void Initialize()
        {
            // set the original values
            for (int i = 0; i < boardSize + 2; i++)
            {
                // the border is always illegal
                spaces[i, 0] = spaces[0, i] = spaces[i, boardSize + 1] =
                    spaces[boardSize + 1, i] = BoardColors.Illegal;
                if ((i > 0) && (i < boardSize + 1))
                {
                    for (int j = 1; j < boardSize + 1; j++)
                    {
                        spaces[i, j] = BoardColors.Empty;
                    }
                }
            }

            // set up the starting position
            this[boardSize / 2 - 1, boardSize / 2 - 1] = BoardColors.White;
            this[boardSize / 2 - 1, boardSize / 2] = BoardColors.Black;
            this[boardSize / 2, boardSize / 2 - 1] = BoardColors.Black;
            this[boardSize / 2, boardSize / 2] = BoardColors.White;
        }


        /// <summary>
        /// Pass to the next player instead of making a move.
        /// </summary>
        /// <param name="player">The color of the player passing.</param>
        public void Pass(BoardColors boardColor)
        {
            // safety-check the player
            if (boardColor != currentColor)
            {
                throw new InvalidOperationException("Move made out of turn.");
            }

            switch (boardColor)
            {
                case BoardColors.Black:
                    blackPassed = true;
                    currentColor = BoardColors.White;
                    break;

                case BoardColors.White:
                    whitePassed = true;
                    currentColor = BoardColors.Black;
                    break;
            }
        }


        /// <summary>
        /// Apply a move to the board.
        /// </summary>
        /// <param name="boardColor">The color of the player making the move.</param>
        /// <param name="move">The move.</param>
        /// <remarks>
        /// The move is assumed to be valid before entering this function.
        /// </remarks>
        public void ApplyMove(BoardColors boardColor, Move move)
        {
            // safety-check the player
            if (boardColor != currentColor)
            {
                throw new InvalidOperationException("Move made out of turn.");
            }

            // set the new piece
            lastMove = move;
            this[move.Row, move.Column] = currentColor;

            // flip lines in all directions
            BoardColors otherColor = BoardColors.Black;
            switch (boardColor)
            {
                case BoardColors.Black:
                    blackPassed = false;
                    otherColor = BoardColors.White;
                    break;

                case BoardColors.White:
                    whitePassed = false;
                    otherColor = BoardColors.Black;
                    break;
            }
            for (int directionRow = -1; directionRow <= 1; directionRow++)
            {
                for (int directionColumn = -1; directionColumn <= 1; directionColumn++)
                {
                    // use the spaces array to check as it includes the illegal boundary
                    int currentRow = move.Row + 1 + directionRow;
                    int currentColumn = move.Column + 1 + directionColumn;
                    while (spaces[currentRow, currentColumn] == otherColor)
                    {
                        if (spaces[currentRow + directionRow,
                            currentColumn + directionColumn] == currentColor)
                        {
                            while (spaces[currentRow, currentColumn] == otherColor)
                            {
                                this[currentRow - 1, currentColumn - 1] = currentColor;
                                currentRow -= directionRow;
                                currentColumn -= directionColumn;
                            }
                            break;
                        }
                        else if (spaces[currentRow + directionRow,
                            currentColumn + directionColumn] == otherColor)
                        {
                            currentRow += directionRow;
                            currentColumn += directionColumn;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            // it's now the other player's turn
            currentColor = otherColor;
        }


        /// <summary>
        /// Determines if the given row and column are valid for this board.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>If true, it represents a valid space on the board.</returns>
        public bool IsValidSpace(int row, int column)
        {
            return ((row >= 0) && (row < boardSize) &&
                    (column >= 0) && (column < boardSize));
        }


        /// <summary>
        /// Checks if a move is valid.
        /// </summary>
        /// <param name="boardColor">The color of the player making the move.</param>
        /// <param name="move">The move.</param>
        /// <returns>If true, the move is valid.</returns>
        public bool IsValidMove(BoardColors boardColor, Move move)
        {
            // if it's not the right turn, it's not a valid move
            if (boardColor != currentColor)
            {
                return false;
            }

            // if it's not empty, then it's never a possible move
            if (this[move.Row, move.Column] != BoardColors.Empty)
            {
                return false;
            }

            // check for possible lines
            BoardColors otherColor = (currentColor == BoardColors.White) ?
                BoardColors.Black : BoardColors.White;
            for (int directionRow = -1; directionRow <= 1; directionRow++)
            {
                for (int directionColumn = -1; directionColumn <= 1; directionColumn++)
                {
                    int currentRow = move.Row + 1 + directionRow;
                    int currentColumn = move.Column + 1 + directionColumn;
                    while (spaces[currentRow, currentColumn] == otherColor)
                    {
                        if (spaces[currentRow + directionRow,
                            currentColumn + directionColumn] == currentColor)
                        {
                            return true;
                        }
                        else if (spaces[currentRow + directionRow,
                            currentColumn + directionColumn] == otherColor)
                        {
                            currentRow += directionRow;
                            currentColumn += directionColumn;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Checks if a player has any valid move on the board.
        /// </summary>
        /// <param name="boardColor">The color of the player to check.</param>
        /// <returns>If true, a valid move exists.</returns>
        public bool HasValidMove(BoardColors boardColor)
        {
            for (int row = 0; row < boardSize; row++)
            {
                for (int column = 0; column < boardSize; column++)
                {
                    if (IsValidMove(boardColor, new Move(row, column)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Create an alternate version of the board where a certain move was made.
        /// </summary>
        /// <param name="boardColor">The color of the player making the move.</param>
        /// <param name="move">The move.</param>
        /// <returns>The board as of the new move.</returns>
        /// <remarks>Useful for previewing and AI algorithms.</remarks>
        public Board CheckMove(BoardColors boardColor, Move move)
        {
            // make sure it's a valid move
            if (!IsValidMove(boardColor, move))
            {
                return null;
            }

            // create a copy of the existing board
            Board board = new Board(boardSize);
            board.blackPieceCount = blackPieceCount;
            board.whitePieceCount = whitePieceCount;
            board.currentColor = currentColor;
            // array is of value type, so shallow is fine
            board.spaces = spaces.Clone() as BoardColors[,];

            board.ApplyMove(boardColor, move);

            return board;
        }


        /// <summary>
        /// Get the number of pieces for a given color.
        /// </summary>
        /// <param name="boardColor">The board color - black or white.</param>
        /// <returns>The number of spaces that match the argument.</returns>
        public int GetColorCount(BoardColors boardColor)
        {
            switch (boardColor)
            {
                case BoardColors.Black:
                    return BlackPieceCount;

                case BoardColors.White:
                    return WhitePieceCount;

                default:
                    throw new ArgumentException("Invalid board color.");
            }
        }


        /// <summary>
        /// Get the number of pieces for the other color than the one passed.
        /// </summary>
        /// <param name="boardColor">The board color - black or white.</param>
        /// <returns>The number of spaces that don't the argument.</returns>
        public int GetOppositeColorCount(BoardColors boardColor)
        {
            switch (boardColor)
            {
                case BoardColors.Black:
                    return WhitePieceCount;

                case BoardColors.White:
                    return BlackPieceCount;

                default:
                    throw new ArgumentException("Invalid board color.");
            }
        }
    }
}
