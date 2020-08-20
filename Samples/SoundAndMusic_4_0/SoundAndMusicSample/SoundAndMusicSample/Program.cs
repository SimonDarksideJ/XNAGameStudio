#region File Information
//-----------------------------------------------------------------------------
// Program.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#endregion Using Statements

namespace SoundAndMusicSample
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SoundAndMusicSampleGame game = new SoundAndMusicSampleGame())
            {
                game.Run();
            }
        }
    }
#endif
}

