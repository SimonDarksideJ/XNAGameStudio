//-----------------------------------------------------------------------------
// Program.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Xna.Tools
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Generate default application container and add Curve editor to
            // container.
            AppContainer appContainer = new AppContainer();
            CurveEditor curveEditor = new CurveEditor();
            appContainer.Add(curveEditor);

            Application.Run(curveEditor);
        }
    }
}