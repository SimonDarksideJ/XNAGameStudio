#region File Description
//-----------------------------------------------------------------------------
// ServiceConstants.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;


#endregion

namespace YachtServices
{
    static class ServiceConstants
    {
        public const byte NullScore = 255;

        public const string LeftMessageString = "Left Game";
        public const string JoinMessageString = "Join Game";
        public const string GameStateMessageString = "Game State";
        public const string NewGameMessageString = "New Game";
        public const string AvailableGamesMessageString = "Available Games";
        public const string BannedMessageString = "Player Banned";
        public const string GameOverMessageString = "Game Over";
        public const string AIMessageString = "AI";
    }
}
