#region File Description
//-----------------------------------------------------------------------------
// YachtEventArgs.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using YachtServices;


#endregion

namespace Yacht
{    
    /// <summary>
    /// Event arguments containing a game state.
    /// </summary>
    public class YachtGameStateEventArgs : EventArgs
    {
        /// <summary>
        /// Game state information.
        /// </summary>
        public GameState GameState { get; set; }
    }

    /// <summary>
    /// Event arguments containing a list of available games.
    /// </summary>
    public class YachtAvailableGamesEventArgs : EventArgs
    {
        /// <summary>
        /// A list of available games.
        /// </summary>
        public AvailableGames AvailableGames { get; set; }
    }

    /// <summary>
    /// Event arguments containing a byte array that represents a score card.
    /// </summary>
    public class YachtScoreCardEventArgs : EventArgs
    {
        /// <summary>
        /// Byte array representing a score card.
        /// </summary>
        public byte[] ScoreCard { get; set; }
    }

    /// <summary>
    /// Event arguments containing information about the player who won the game.
    /// </summary>
    public class YachtGameOverEventArgs : EventArgs
    {
        /// <summary>
        /// Information about the winning player.
        /// </summary>
        public EndGameInformation EndGameState { get; set; }
    }

    /// <summary>
    /// Event arguments containing a single boolean.
    /// </summary>
    public class BooleanEventArgs : EventArgs
    {
        /// <summary>
        /// Boolean data.
        /// </summary>
        public bool Answer { get; set; }
    }

    /// <summary>
    /// Event arguments containing a single string.
    /// </summary>
    public class StringEventArgs : EventArgs
    {
        /// <summary>
        /// String data.
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Event arguments containing an exception object.
    /// </summary>
    public class ExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Error data.
        /// </summary>
        public Exception Error { get; set; }
    }
}
