#region File Description
//-----------------------------------------------------------------------------
// Directories.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace RacingGame.Helpers
{
    /// <summary>
    /// Helper class which stores all used directories.
    /// </summary>
    class Directories
    {
        #region Game base directory
        /// <summary>
        /// We can use this to relocate the whole game directory to another
        /// location. Used for testing (everything is stored on a network drive).
        /// </summary>
        public static readonly string GameBaseDirectory =
            // Update to support Xbox360:
            "";
        #endregion

        #region Directories
        /// <summary>
        /// Content directory for all our textures, models and shaders.
        /// </summary>
        /// <returns>String</returns>
        public static string ContentDirectory
        {
            get
            {
                return "Content";// Path.Combine(GameBaseDirectory, "Content");
            }
        }

        /// <summary>
        /// Sounds directory, for some reason XAct projects don't produce
        /// any content files (bug?). We just load them ourself!
        /// </summary>
        /// <returns>String</returns>
        public static string SoundsDirectory
        {
            get
            {
                return Path.Combine(GameBaseDirectory, "Content\\Audio");
            }
        }

        /// <summary>
        /// Default Screenshots directory.
        /// </summary>
        /// <returns>String</returns>
        public static string ScreenshotsDirectory
        {
            get
            {
                return Path.Combine(GameBaseDirectory, "Screenshots");
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Private constructor to prevent instantiation.
        /// </summary>
        private Directories()
        {
        }
        #endregion
    }
}
