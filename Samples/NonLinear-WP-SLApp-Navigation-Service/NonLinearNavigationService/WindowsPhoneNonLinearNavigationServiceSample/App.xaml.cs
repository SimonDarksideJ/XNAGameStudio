// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WindowsPhoneRecipes
{
	public partial class App : Application
	{
		// Easy access to the root frame
		public PhoneApplicationFrame RootFrame { get; private set; }

		// Constructor
		public App()
		{
			// Global handler for uncaught exceptions. 
			// Note that exceptions thrown by ApplicationBarItem.Click will not get caught here.
			UnhandledException += Application_UnhandledException;

			// Standard Silverlight initialization
			InitializeComponent();

			// Phone-specific initialization
			InitializePhoneApplication();


            WindowsPhoneRecipes.Logger.Instance.AddLine();
            
            ///////////////////////////////////////////////////////////////////////////////////
            // Init the NonLinearNavigationService, make sure you call it last in the app CTOR
            // (this has minimal impact on load time)
            ///////////////////////////////////////////////////////////////////////////////////
            NonLinearNavigationService.Instance.Initialize(RootFrame);
		}

		// Code to execute when the application is launching (eg, from Start)
		// This code will not execute when the application is reactivated
		
		private void Application_Launching(object sender, LaunchingEventArgs e)
		{
            WindowsPhoneRecipes.Logger.Instance.AddLine();
		}

		// Code to execute when the application is activated (brought to foreground)
		// This code will not execute when the application is first launched
		private void Application_Activated(object sender, ActivatedEventArgs e)
		{
            WindowsPhoneRecipes.Logger.Instance.AddLine();
		}


		// Code to execute when the application is deactivated (sent to background)
		// This code will not execute when the application is closing
		private void Application_Deactivated(object sender, DeactivatedEventArgs e)
		{
            WindowsPhoneRecipes.Logger.Instance.AddLine();
		}

		// Code to execute when the application is closing (eg, user hit Back)
		// This code will not execute when the application is deactivated
		private void Application_Closing(object sender, ClosingEventArgs e)
		{
            WindowsPhoneRecipes.Logger.Instance.AddLine();
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
		
		// Code to execute on Unhandled Exceptions
		private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// An unhandled exception has occurred; break into the debugger
				System.Diagnostics.Debugger.Break();
			}
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
