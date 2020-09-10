//-----------------------------------------------------------------------------
// Enumerations.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using Microsoft.Xna.Framework;

namespace Xna.Tools
{
    [Flags]
    public enum EditCurveSelections
    {
        None        = 0,
        TangentIn   = (1 << 0),
        TangentOut  = (1 << 1),
        Key         = (1 << 2),
        Tangents    = TangentIn|TangentOut,
    }

    public enum CurveSmoothness
    {
        Coarse,
        Rough,
        Medium,
        Fine
    }

    public enum EditCurveView
    {
        Always,
        Never,
        ActiveOnly,
        OnActiveKeys,
    }

    public enum EditCurveTangent
    {
        Flat    = CurveTangent.Flat,
        Linear  = CurveTangent.Linear,
        Smooth  = CurveTangent.Smooth,
        Stepped = 10,
        Fixed   = 11
    }

}
