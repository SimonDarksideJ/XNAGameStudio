#region File Description
//-----------------------------------------------------------------------------
// Program.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using RacingGame.Helpers;
using RacingGame.Properties;
using Microsoft.Xna.Framework;
#if !XBOX360
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
#endif
#endregion

namespace RacingGame
{
    /// <summary>
    /// Program
    /// </summary>
    static class Program
    {
        #region Main
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Arguments</param>
#if !XBOX360
        [STAThread]
#endif
        static void Main()
        {
            StartGame();
        }
        #endregion

        #region StartGame
        /// <summary>
        /// Start game, is in a seperate method for 2 reasons: We want to catch
        /// any exceptions here, but not for the unit tests and we also allow
        /// the unit tests to call this method if we don't want to unit test
        /// in debug mode.
        /// </summary>
        public static void StartGame()
        {
#if !XBOX360
            try
            {
#endif
                using (RacingGameManager game = new RacingGameManager())
                {
                    game.Run();
                }
#if !XBOX360
            }
            catch (NoSuitableGraphicsDeviceException)
            {

                MessageBox.Show("Pixel and vertex shaders 2.0 or greater are required.",
                    "RacingGame",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (NotSupportedException ex/*OutOfVideoMemoryException*/)
            {
                GameSettings.SetMinimumGraphics();

                Console.WriteLine(ex.Message);
                Console.Write(ex.StackTrace);
                MessageBox.Show("Insufficent video memory.\n\n" +
                    "The graphics settings have been reconfigured to the minimum. " +
                    "Please restart the application. \n\nIf you continue to receive " +
                    "this error message, your system may not meet the " +
                    "minimum requirements.",
                    "RacingGame",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif
        }
        #endregion
    }
}
