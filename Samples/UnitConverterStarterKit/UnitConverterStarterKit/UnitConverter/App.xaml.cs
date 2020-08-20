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
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Applications.UnitConverter.Helpers;
using Microsoft.Phone.Applications.Common.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Text;

[assembly: CLSCompliant(false)]
namespace Microsoft.Phone.Applications.UnitConverter
{

    
    /// <summary>
    /// Unit Converter main application class
    /// </summary>
    public partial class App : Application
    {


        /// <summary>
        /// Gets or sets the root frame.
        /// </summary>
        /// <value>The root frame.</value>
        public PhoneApplicationFrame RootFrame { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {

            // Global handler for uncaught exceptions. 
            // Note that exceptions thrown by ApplicationBarItem.Click will not get caught here.
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();
          
        }


        /// <summary>
        /// Code to execute when the application is launching (eg, from Start)
        /// This code will not execute when the application is reactivated
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event Data</param>
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            ApplicationState.ApplicationStartup = AppOpenState.Launching;
            TiltEffect.SetIsTiltEnabled(RootFrame, true);
        }

      

        /// <summary>
        /// Code to execute when the application is activated (brought to foreground)
        /// This code will not execute when the application is first launched
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event Data</param>
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            ApplicationState.ApplicationStartup = AppOpenState.Activated;
            TiltEffect.SetIsTiltEnabled(RootFrame, true);
        }

    
        /// <summary>
        /// Code to execute when the application is deactivated (sent to background)
        /// This code will not execute when the application is closing
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event Data</param>
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            ApplicationState.ApplicationStartup = AppOpenState.Deactivated;
        }


        /// <summary>
        /// Code to execute when the application is closing (eg, user hit Back)
        /// This code will not execute when the application is deactivated</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event Data</param>
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            ApplicationState.ApplicationStartup = AppOpenState.Closing;
        }


        /// <summary>
        /// Code to execute if a navigation fails
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }


        /// <summary>
        /// Code to execute on Unhandled Exceptions
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
#if DEBUG

            // In case any exception occurs here, we don't want another unhandled exception!
            try
            {
                StringBuilder b = new StringBuilder();
                b.AppendLine(e.ExceptionObject.ToString());
                foreach (ErrorLog l in ApplicationState.ErrorLog)
                {
                    b.AppendLine(l.LogEvent + " " + l.Message);
                }
                MessageBox.Show( b.ToString());
                System.Diagnostics.Debug.WriteLine("-----------------------Managed Exception Hit--------------------");
                Exception exp = e.ExceptionObject;
                while (exp != null)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("\type    = {0}", exp.GetType().ToString()));
                    System.Diagnostics.Debug.WriteLine(string.Format("\tmessage = {0}", exp.Message));
                    System.Diagnostics.Debug.WriteLine(string.Format(exp.StackTrace));
                    exp = exp.InnerException;
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine("Diagnostic output exception was " + e1.ToString());
            }
#endif
        }
       

        #region Phone application initialization

        // Avoid double-initialization
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

        #endregion
    }
}
