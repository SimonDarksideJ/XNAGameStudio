//-----------------------------------------------------------------------------
// NativeMethods.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;

namespace Xna.Tools
{
    static class NativeMethods
    {
        [DllImport("user32.dll")]
        static extern IntPtr LoadCursorFromFile(string lpFileName);

        /// <summary>
        /// Load Color cursor from resource byre array.
        /// </summary>
        /// <param name="cursorData"></param>
        /// <returns></returns>
        static public Cursor LoadCursor(byte[] cursorData)
        {
            string tmpPath = Path.GetTempFileName();
            File.WriteAllBytes(tmpPath, cursorData);
            Cursor result = new Cursor(LoadCursorFromFile(tmpPath));
            File.Delete(tmpPath);
            return result;
        }

    }
}
