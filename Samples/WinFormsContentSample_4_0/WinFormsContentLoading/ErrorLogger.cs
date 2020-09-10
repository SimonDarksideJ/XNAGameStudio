#region File Description
//-----------------------------------------------------------------------------
// ErrorLogger.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Build.Framework;
#endregion

namespace WinFormsContentLoading
{
    /// <summary>
    /// Custom implementation of the MSBuild ILogger interface records
    /// content build errors so we can later display them to the user.
    /// </summary>
    class ErrorLogger : ILogger
    {
        /// <summary>
        /// Initializes the custom logger, hooking the ErrorRaised notification event.
        /// </summary>
        public void Initialize(IEventSource eventSource)
        {
            if (eventSource != null)
            {
                eventSource.ErrorRaised += ErrorRaised;
            }
        }


        /// <summary>
        /// Shuts down the custom logger.
        /// </summary>
        public void Shutdown()
        {
        }


        /// <summary>
        /// Handles error notification events by storing the error message string.
        /// </summary>
        void ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            errors.Add(e.Message);
        }


        /// <summary>
        /// Gets a list of all the errors that have been logged.
        /// </summary>
        public List<string> Errors
        {
            get { return errors; }
        }

        List<string> errors = new List<string>();


        #region ILogger Members

        
        /// <summary>
        /// Implement the ILogger.Parameters property.
        /// </summary>
        string ILogger.Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        string parameters;


        /// <summary>
        /// Implement the ILogger.Verbosity property.
        /// </summary>
        LoggerVerbosity ILogger.Verbosity
        {
            get { return verbosity; }
            set { verbosity = value; }
        }

        LoggerVerbosity verbosity = LoggerVerbosity.Normal;


        #endregion
    }
}
