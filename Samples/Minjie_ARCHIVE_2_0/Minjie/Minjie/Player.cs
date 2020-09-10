#region File Description
//-----------------------------------------------------------------------------
// Player.cs
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
    /// Implementation for all players, regardless of type.
    /// </summary>
    abstract class Player
    {
        private BoardColors boardColor;
        public BoardColors BoardColor
        {
            get { return boardColor; }
        }

        protected Player(BoardColors boardColor)
        {
            if ((boardColor != BoardColors.Black) && (boardColor != BoardColors.White))
            {
                throw new ArgumentException("Invalid board color.");
            }
            this.boardColor = boardColor;
        }

        public abstract void Update(Board board);
    }
}
