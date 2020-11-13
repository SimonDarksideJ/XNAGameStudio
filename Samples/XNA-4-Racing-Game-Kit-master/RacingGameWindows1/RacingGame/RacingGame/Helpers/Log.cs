#region File Description
//-----------------------------------------------------------------------------
// Log.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace RacingGame.Helpers
{
    /// <summary>
    /// Log will create automatically a log file and write
    /// log/error/debug info for simple runtime error checking, very useful
    /// for minor errors, such as missing files.
    /// The application can still continue working, but this log provides
    /// an easy support to find out what files are missing (in this example).
    ///
    /// Note: I don't use this class anymore for big projects, but its small
    /// and handy for smaller projects and nice to log non-debugable stuff.
    /// 
    /// Also, there's no reason to do this on the Xbox 360, since you cannot
    /// pull the log file back down to your computer.
    /// </summary>
    public static class Log
    {
        #region Variables
#if !XBOX360
        /// <summary>
        /// Writer
        /// </summary>
        private static StreamWriter writer = null;

        /// <summary>
        /// Log filename
        /// </summary>
        private const string LogFilename = "Log.txt";
#endif
        #endregion

        #region Static constructor to create log file
        /// <summary>
        /// Static constructor
        /// </summary>
        public static void Initialize()
        {
#if !XBOX360
            try
            {
                IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();

                FileStream file;
                if (!isolatedStorageFile.FileExists(LogFilename))
                    file = isolatedStorageFile.CreateFile(LogFilename);
                else
                {
                    file = isolatedStorageFile.OpenFile(LogFilename, FileMode.OpenOrCreate,
                                                        FileAccess.Write, FileShare.ReadWrite);
                }

                // Check if file is too big (more than 2 MB),
                // in this case we just kill it and create a new one :)
                if (file.Length > 2 * 1024 * 1024)
                {
                    file.Close();
                    file = isolatedStorageFile.CreateFile(LogFilename);
                }
                // Associate writer with that, when writing to a new file,
                // make sure UTF-8 sign is written, else don't write it again!
                if (file.Length == 0)
                    writer = new StreamWriter(file,
                        System.Text.Encoding.UTF8);
                else
                    writer = new StreamWriter(file);

                // Go to end of file
                writer.BaseStream.Seek(0, SeekOrigin.End);

                // Enable auto flush (always be up to date when reading!)
                writer.AutoFlush = true;

                // Add some info about this session
                writer.WriteLine("");
                writer.WriteLine("/// Session started at: " + DateTime.Now.ToString());
                writer.WriteLine("/// RacingGame");
                writer.WriteLine("");
            }
            catch (IOException)
            {
                // Ignore any file exceptions, if file is not
                // createable (e.g. on a CD-Rom) it doesn't matter.
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore any file exceptions, if file is not
                // createable (e.g. on a CD-Rom) it doesn't matter.
            }
#endif
        }
        #endregion

        #region Write log entry
        /// <summary>
        /// Writes a LogType and info/error message string to the Log file
        /// </summary>
        static public void Write(string message)
        {
#if !XBOX360
            // Can't continue without valid writer
            if (writer == null)
                return;

            try
            {
                DateTime ct = DateTime.Now;
                string s = "[" + ct.Hour.ToString("00") + ":" +
                    ct.Minute.ToString("00") + ":" +
                    ct.Second.ToString("00") + "] " +
                    message;
                writer.WriteLine(s);

#if DEBUG
                // In debug mode write that message to the console as well!
                System.Console.WriteLine(s);
#endif
            }
            catch (IOException)
            {
                // Ignore any file exceptions, if file is not
                // createable (e.g. on a CD-Rom) it doesn't matter.
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore any file exceptions, if file is not
                // createable (e.g. on a CD-Rom) it doesn't matter.
            }
#endif
        }
        #endregion
    }
}
