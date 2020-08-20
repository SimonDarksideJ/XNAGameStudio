#region File Description
//-----------------------------------------------------------------------------
// ScaledVector2.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace RolePlayingGameData
{
    public static class ScaledVector2
    {
        public const float ScaleFactor = 0.625f;
        public const float DrawFactor = 1.333f;


        public static Vector2 GetScaledVector()
        {
            return new Vector2();
        }

        public static Vector2 GetScaledVector(int x, int y)
        {
            return GetScaledVector((float)x, (float)y);
        }

        public static Vector2 GetScaledVector(float x, float y)
        {
            return new Vector2(x, y) * ScaleFactor;
        }
    }
}