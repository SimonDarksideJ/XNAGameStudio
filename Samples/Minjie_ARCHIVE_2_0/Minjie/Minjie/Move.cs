#region File Description
//-----------------------------------------------------------------------------
// Move.cs
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
    /// Data for one move in the game, indicating the location of the new piece.
    /// </summary>
    struct Move
    {
        private int row;
        public int Row
        {
            get { return row; }
        }

        private int column;
        public int Column
        {
            get { return column; }
        }

        public Move(int row, int column)
        {
            this.row = row;
            this.column = column;
        }
    }
}
