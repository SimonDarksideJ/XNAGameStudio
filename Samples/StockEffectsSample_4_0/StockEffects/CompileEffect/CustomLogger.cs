#region File Description
//-----------------------------------------------------------------------------
// CustomLogger.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Content.Pipeline;
#endregion

namespace CompileEffect
{
    /// <summary>
    /// Custom logger class for capturing Content Pipeline output messages. This implementation
    /// just prints messages to the console, and throws an exception if there are any warnings.
    /// </summary>
    class CustomLogger : ContentBuildLogger
    {
        /// <summary>
        /// Logs a low priority message.
        /// </summary>
        public override void LogMessage(string message, params object[] messageArgs)
        {
            Console.WriteLine(message, messageArgs);
        }


        /// <summary>
        /// Logs a high priority message.
        /// </summary>
        public override void LogImportantMessage(string message, params object[] messageArgs)
        {
            Console.WriteLine(message, messageArgs);
        }


        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
        {
            throw new Exception("Warning: " + string.Format(message, messageArgs));
        }
    }
}
