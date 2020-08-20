// Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
//
//
// Use of this source code is subject to the terms of the Microsoft
// license agreement under which you licensed this source code.
// If you did not accept the terms of the license agreement,
// you are not authorized to use this source code.
// For the terms of the license, please see the license agreement
// signed by you and Microsoft.
// THE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Applications.Common;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;


namespace Microsoft.Phone.Applications.Level
{
    public partial class App : Application
    {
        public App()
        {
            // Frame rate counter debug code:
            // ------------------------------
            // Host.Settings.EnableFrameRateCounter = true; // Shows frame rate (SW + HW) on the top of the screen
            // Host.Settings.EnableCacheVisualization = true; // Invert colorized cached areas
            // Host.Settings.EnableRedrawRegions = true; // this flips random colors on any new SW redrawed area, making it easy to visualized

            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            Application_LaunchOrActivate(true);
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            Application_LaunchOrActivate(false);
        }

        private static void Application_Obscured(object sender, ObscuredEventArgs e)
        {
            AccelerometerHelper.Instance.IsActive = false;
        }

        private static void Application_Unobscured(object sender, EventArgs e)
        {
            AccelerometerHelper.Instance.IsActive = true;
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            AccelerometerHelper.Instance.IsActive = false;
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            AccelerometerHelper.Instance.IsActive = false;
        }

        /// <summary>
        /// Common method for launching or activate
        /// </summary>
        /// <param name = "isLaunch">first launch</param>
        private static void Application_LaunchOrActivate(bool isLaunch)
        {
            AccelerometerHelper.Instance.IsActive = true;
            // This application does not have any particular state to restore on activate
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (!Debugger.IsAttached)
            {
                e.Handled = true;
                Debug.WriteLine(e.ExceptionObject.Message);
            }
        }


        #region Phone application initialization

        // Easy access to the root frame
        public PhoneApplicationFrame RootFrame { get; private set; }

        // Avoid double-initialization-
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            RootFrame.Obscured += new EventHandler<ObscuredEventArgs>(Application_Obscured);
            RootFrame.Unobscured += new EventHandler(Application_Unobscured);

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        // Code to execute if a navigation fails
        void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #endregion

    }
}
