#region File Description
//-----------------------------------------------------------------------------
// WorldRules.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#endregion

namespace VectorRumble
{
    public enum AsteroidDensity
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    public enum WallStyle
    {
        None = 0,
        One = 1,
        Two = 2,
        Three = 3
    }

    /// <summary>
    /// Adjustable game settings.
    /// </summary>
    public static class WorldRules
    {
        public static int ScoreLimit = 10;
        public static AsteroidDensity AsteroidDensity = AsteroidDensity.Low;
        public static WallStyle WallStyle = WallStyle.Three;
        public static bool MotionBlur = true;
        public static bool NeonEffect = true;
    }
}
