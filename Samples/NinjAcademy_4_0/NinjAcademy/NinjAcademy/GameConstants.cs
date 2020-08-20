#region File Description
//-----------------------------------------------------------------------------
// GameConstants.cs
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


#endregion

namespace NinjAcademy
{
    /// <summary>
    /// Defines constants used mainly for positioning UI elements, but also for defining the game's behavior.
    /// </summary>
    static class GameConstants
    {
        // Menu screen constants
        public const int MainMenuLeft = 400;
        public const int MainMenuTop = 265;
        public const int MainMenuEntryGap = 70;
        
        // Pause screen constants
        public const int PauseMenuLeft = 420;
        public const int PauseMenuTop = 320;
        public const int PauseMenuEntryGap = 70;

        // HUD constants
        public static readonly Vector2 HitPointsOrigin = new Vector2(740, 15);
        public const float HitPointsSpace = 5;
        public static readonly Vector2 ScorePosition = new Vector2(10, 15);

        // Launch constants
        public static readonly Vector2 LaunchAcceleration = new Vector2(0, 500);
        public const float OffScreenYCoordinate = 500;

        // Sword slash constants
        public static readonly TimeSpan SwordSlashFadeDuration = TimeSpan.FromMilliseconds(150);

        // Target constants
        public static readonly Vector2 UpperTargetAreaTopLeft = new Vector2(208, 169);
        public static readonly Vector2 MiddleTargetAreaTopLeft = new Vector2(208, 247);
        public static readonly Vector2 LowerTargetAreaTopLeft = new Vector2(208, 320);

        public static readonly Vector2 UpperTargetAreaBottomRight = new Vector2(591, 226);
        public static readonly Vector2 MiddleTargetAreaBottomRight = new Vector2(591, 298);
        public static readonly Vector2 LowerTargetAreaBottomRight = new Vector2(591, 370);

        public static readonly Vector2 UpperTargetOrigin = new Vector2(750, 200);
        public static readonly Vector2 UpperTargetDestination = new Vector2(50, 200);
        public static readonly Vector2 MiddleTargetOrigin = new Vector2(50, 273);
        public static readonly Vector2 MiddleTargetDestination = new Vector2(750, 273);
        public static readonly Vector2 LowerTargetOrigin = new Vector2(750, 344);
        public static readonly Vector2 LowerTargetDestination = new Vector2(50, 344);
        public const float TargetSpeed = 75;

        public const float TargetRadius = 28;

        // Drawing order constants
        public const int HUDDrawOrder = 50;
        public const int RoomDrawOrder = 20;
        public const int DefaultDrawOrder = 30;
        public const int TargetDrawOrder = 10;
        public const int FallingTargetDrawOrder = 5;
        public const int ThrowingStarsDrawOrder = 25;
        public const int SwordSlashDrawOrder = 40;

        // Throwing star constants
        public static readonly Vector2 ThrowingStarOrigin = new Vector2(400, 510);
        public static readonly TimeSpan ThrowingStarFlightDuration = TimeSpan.FromMilliseconds(250);
        public const float ThrowingStarEndScale = 0.25f;

        // High-score screen constants
        public const int HighScorePlaceLeftMargin = 70;
        public const int HighScoreNameLeftMargin = 120;
        public const int HighScoreScoreLeftMargin = 575;
        public const int HighScoreTitleTopMargin = 25;
        public const int HighScoreTopMargin = 100;
        public const int HighScoreVerticalJump = 50;
    }
}
