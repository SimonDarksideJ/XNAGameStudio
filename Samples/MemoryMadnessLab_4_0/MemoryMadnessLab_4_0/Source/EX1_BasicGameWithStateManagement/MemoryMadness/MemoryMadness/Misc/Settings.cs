#region File Description

//-----------------------------------------------------------------------------
// Settings.cs
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

namespace MemoryMadness
{
    enum ButtonColors
    {
        Red,
        Yellow,
        Blue,
        Green
    }

    enum LevelState
    {
        NotReady,
        Ready,
        Flashing,
        Started,
        InProcess,
        Fault,
        Success,
        FinishedOk,
        FinishedFail
    }

    enum TouchInputState
    {
        Idle,
        GracePeriod
    }

    static class Settings
    {
        // Amount of buttons
        public static int ButtonAmount = 4;

        // Sliding doors animation constants
        public static int DoorsAnimationStep = 5;
        public static Vector2 LeftDoorClosedPosition = new Vector2(0, 389);
        public static Vector2 LeftDoorOpenedPosition = new Vector2(-90, 389);
        public static Vector2 RightDoorClosedPosition = new Vector2(345, 389);
        public static Vector2 RightDoorOpenedPosition = new Vector2(435, 389);

        // Color button locations
        public static Vector2 RedButtonPosition = new Vector2(23,44);
        public static Vector2 GreenButtonPosition = new Vector2(269, 44);
        public static Vector2 BlueButtonPosition = new Vector2(23, 568);
        public static Vector2 YellowButtonPosition = new Vector2(269, 568);

        // Color button positions in the texture strip, with their sizes
        public static Vector2 ButtonSize = new Vector2(185, 185);
        public static Rectangle RedButtonDim = new Rectangle((int)ButtonSize.X * 0, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);
        public static Rectangle RedButtonLit = new Rectangle((int)ButtonSize.X * 1, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);

        public static Rectangle YellowButtonDim = new Rectangle((int)ButtonSize.X * 2, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);
        public static Rectangle YellowButtonLit = new Rectangle((int)ButtonSize.X * 3, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);

        public static Rectangle GreenButtonDim = new Rectangle((int)ButtonSize.X * 4, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);
        public static Rectangle GreenButtonLit = new Rectangle((int)ButtonSize.X * 5, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);

        public static Rectangle BlueButtonDim = new Rectangle((int)ButtonSize.X * 6, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);
        public static Rectangle BlueButtonLit = new Rectangle((int)ButtonSize.X * 7, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);
    }
}
