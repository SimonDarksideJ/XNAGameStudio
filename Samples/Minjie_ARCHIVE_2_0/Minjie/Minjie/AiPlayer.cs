#region File Description
//-----------------------------------------------------------------------------
// AiPlayer.cs
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
    /// <summary>
    /// Player controlled by artificial intelligence.
    /// </summary>
    class AiPlayer : Player
    {
        /// <summary>
        /// The depth of the searching.
        /// </summary>
        const int maximumDepth = 3;

        /// <summary>
        /// The maximum number of nodes that the algorithm can visit before giving up.
        /// </summary>
        const int nodesVisitedLimit = 7000;

        /// <summary>
        /// The number of total pieces that triggers the end of completely random moves.
        /// </summary>
        const int randomMoveCutoff = 25;

        /// <summary>
        /// The number of total pieces that triggers the removal of 
        /// randomness from the heuristic calculation.
        /// </summary>
        const int randomHeuristicCutoff = 90;

        /// <summary>
        /// Random number generator for the AI player.
        /// </summary>
        static Random random = new Random();


        /// <summary>
        /// Create a new AI player object.
        /// </summary>
        /// <param name="boardColor">The color of this player.</param>
        public AiPlayer(BoardColors boardColor) : base(boardColor) { }


        /// <summary>
        /// Update the AI player.
        /// </summary>
        /// <param name="board">The current game board.</param>
        public override void Update(Board board)
        {
            // safety-check the board parameter
            if (board == null)
            {
                throw new ArgumentNullException("board");
            }

            // if it's not our turn, then we're done
            if (board.CurrentColor != BoardColor)
            {
                return;
            }

            // generate a new move.
            Move move;
            bool moveFound = false;
            // in order to begin the game with some variety, the AI will make random
            // acceptable moves until 25 moves have been made, then switch to minimax
            int totalPieces = board.GetColorCount(BoardColor) + 
                board.GetOppositeColorCount(BoardColor);
            if (totalPieces  <= randomMoveCutoff)
            {
                moveFound = GenerateMoveRandom(board, out move);
            }
            else
            {
                moveFound = GenerateMoveMiniMax(board, maximumDepth, out move);
            }

            // apply the move
            if (moveFound)
            {
                board.ApplyMove(BoardColor, move);
            }
            else
            {
                board.Pass(BoardColor);
            }
        }


        /// <summary>
        /// Generate a random move for the player to make.
        /// </summary>
        /// <param name="board">The current board.</param>
        /// <param name="move">Receives the chosen move.</param>
        /// <returns>If true, a valid move was found.</returns>
        private bool GenerateMoveRandom(Board board, out Move move)
        {
            // safety-check the board parameter
            if (board == null)
            {
                throw new ArgumentNullException("board");
            }

            move = new Move(-1, -1);

            // check if there is a valid move
            if (!board.HasValidMove(BoardColor))
            {
                return false;
            }

            // search for a new move randomly
            do
            {
                move = new Move(random.Next(board.BoardSize),
                    random.Next(board.BoardSize));
            }
            while (!board.IsValidMove(BoardColor, move));

            return true;
        }


        /// <summary>
        /// Generate a move for the player to make based on the MiniMax algorithm.
        /// </summary>
        /// <param name="board">The current board.</param>
        /// <param name="maximumDepth">The maximum depth of the algorithm.</param>
        /// <param name="move">Receives the chosen move.</param>
        /// <returns>If true, a valid move was found.</returns>
        private bool GenerateMoveMiniMax(Board board, int maximumDepth, out Move move)
        {
            // safety-check the board parameter
            if (board == null)
            {
                throw new ArgumentNullException("board");
            }

            move = new Move(-1, -1);

            // check if there is a valid move
            if (!board.HasValidMove(BoardColor))
            {
                return false;
            }

            // start the recursive search
            int nodesVisitedCount = 0;
            CalculateMiniMaxHeuristic(board, maximumDepth, 
                ref nodesVisitedCount, out move); 

            // determine if it's a valid space - if it is, we know it's a valid move
            return board.IsValidSpace(move.Row, move.Column);
        }


        /// <summary>
        /// Calculate the heuristic for the state of the board, recursively.
        /// </summary>
        /// <param name="board">The current state of the board in the search.</param>
        /// <param name="depth">The current depth of the search.</param>
        /// <param name="nodesVisitedCount">The total number of nodes visited.</param>
        /// <param name="move">The best move.</param>
        /// <returns>The heuristic for this node in the search.</returns>
        private int CalculateMiniMaxHeuristic(Board board, int depth, 
            ref int nodesVisitedCount, out Move move)
        {
            // safety-check the board parameter
            if (board == null)
            {
                throw new ArgumentNullException("board");
            }

            bool myTurn = board.CurrentColor == BoardColor;
            BoardColors currentColor = myTurn ? BoardColor : 
              (BoardColor == BoardColors.White ? BoardColors.Black : BoardColors.White);
            int heuristic = myTurn ? Int32.MaxValue * -1 : Int32.MaxValue;

            nodesVisitedCount++;
            move = new Move(-1, -1);

            // cut short the algorithm if it's time to give up
            if (nodesVisitedCount >= nodesVisitedLimit)
            {
                return 0;
            }

            // at the bottom of the tree, just calculate the value
            if (depth <= 0)
            {
                return CalculateMiniMaxHeuristic(board);
            }

            for (int row = 0; row < board.BoardSize; row++)
            {
                for (int column = 0; column < board.BoardSize; column++)
                {
                    Move newMove = new Move(row, column);
                    Board newBoard = board.CheckMove(BoardColor, newMove);
                    if (newBoard != null)
                    {
                        Move deeperMove; // we only care about moves in the top level 
                        int newHeuristic = CalculateMiniMaxHeuristic(newBoard, 
                            depth - 1, ref nodesVisitedCount, out deeperMove);
                        if (nodesVisitedCount >= nodesVisitedLimit)
                        {
                            move = newMove;
                            return 0;
                        }
                        if ((myTurn && (newHeuristic > heuristic)) ||
                            (!myTurn && (newHeuristic < heuristic)))
                        {
                            move = newMove;
                            heuristic = newHeuristic;                            
                        }
                    }
                }
            }

            return heuristic;
        }


        /// <summary>
        /// Calculate the heuristic for the state of the board, based on this player.
        /// </summary>
        /// <param name="board">The current state of the board in the search.</param>
        /// <returns>The heuristic for this node in the search.</returns>
        private int CalculateMiniMaxHeuristic(Board board)
        {
            // safety-check the board parameter
            if (board == null)
            {
                throw new ArgumentNullException("board");
            }

            int myPieces = board.GetColorCount(BoardColor);
            int otherPieces = board.GetOppositeColorCount(BoardColor);
            int heuristic = myPieces - otherPieces;
            // in order to stop the algorithm from going straight to the nearest edge, 
            // introduce some randomness 
            if (myPieces + otherPieces <= randomHeuristicCutoff)
            {
                heuristic += (int)(2.0 * random.NextDouble());
            }
            return heuristic;
        }
    }
}
