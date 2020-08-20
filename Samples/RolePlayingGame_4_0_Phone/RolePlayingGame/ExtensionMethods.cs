#region File Description
//-----------------------------------------------------------------------------
// ExtensionMethods.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace RolePlaying
{
    public enum GestureDirection
    {
        Up,
        Down,
        Right,
        Left,
        None
    }

    public static class ExtensionMethods
    {
        public static GestureDirection GetDirection(this GestureSample gesture)
        {
            GestureDirection direction = GestureDirection.None;

            if (gesture.Delta.Y != 0)
            {
                if (gesture.Delta.Y > 0)
                {
                    direction = GestureDirection.Down;
                }
                else
                {
                    direction = GestureDirection.Up;
                }
            }
            else if (gesture.Delta.X != 0)
            {
                if (gesture.Delta.X > 0)
                {
                    direction = GestureDirection.Right;
                }
                else
                {
                    direction = GestureDirection.Left;
                }
            }
            return direction;
        }
    }
}
