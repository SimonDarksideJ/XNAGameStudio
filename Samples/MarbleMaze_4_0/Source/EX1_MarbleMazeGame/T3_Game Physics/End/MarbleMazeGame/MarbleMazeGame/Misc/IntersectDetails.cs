#region File Description
//-----------------------------------------------------------------------------
// IntersectDetails.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace MarbleMazeGame
{
    public struct IntersectDetails
    {
        #region Fields
        public bool IntersectWithGround;
        public bool IntersectWithFloorSides;
        public bool IntersectWithWalls;

        public Triangle IntersectedGroundTriangle;
        public IEnumerable<Triangle> IntersectedFloorSidesTriangle;
        public IEnumerable<Triangle> IntersectedWallTriangle; 
        #endregion
    }
}
