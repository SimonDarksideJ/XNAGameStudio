#region File Description
//-----------------------------------------------------------------------------
// ProceduralPrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace SoccerPitch
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SoccerPitchGame game = new SoccerPitchGame())
            {
                game.Run();
            }
        }
    }
#endif
}

