#region File Description
//-----------------------------------------------------------------------------
// WaypointList.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace PathDrawing
{
    /// <summary>
    /// WaypointList is a queue of locations that our Tank should drive towards.
    /// </summary>
    public class WaypointList : Queue<Vector2>
    {
        /// <summary>
        /// Gets the position in the queue at the given index.
        /// </summary>
        public Vector2 this[int index]
        {
            get
            {
                Vector2 value = Vector2.Zero;

                // We use a foreach loop because a Queue<T> doesn't have any way to index
                foreach (var v in this)
                {
                    index--;
                    if (index < 0)
                    {
                        value = v;
                        break;
                    }
                }

                return value;
            }
        }
    }
}
