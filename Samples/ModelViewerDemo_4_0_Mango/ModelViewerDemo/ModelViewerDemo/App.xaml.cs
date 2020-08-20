#region File Description
//-----------------------------------------------------------------------------
// App.xaml.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ModelViewerDemo
{
	public partial class App : Application, IServiceProvider
	{
		/// <summary>
		/// Provides easy access to the root frame of the Phone Application.
		/// </summary>
		/// <returns>The root frame of the Phone Application.</returns>
		public PhoneApplicationFrame RootFrame { get; private set; }

		/// <summary>
		/// Provides access to a ContentManager for the application.
		/// </summary>
		public ContentManager Content { get; private set; }

		/// <summary>
		/// Provides access to a GameTimer that is set up to pump the FrameworkDispatcher.
		/// </summary>
		public GameTimer FrameworkDispatcherTimer { get; private set; }

		/// <summary>
		/// Constructor for the Application object.
		/// </summary>
		public App()
		{
			// Global handler for uncaught exceptions. 
			UnhandledException += Application_UnhandledException;

			// Show graphics profiling information while debugging.
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// Display the current frame rate counters.
				Application.Current.Host.Settings.EnableFrameRateCounter = true;

				// Show the areas of the app that are being redrawn in each frame.
				//Application.Current.Host.Settings.EnableRedrawRegions = true;

				// Enable non-production analysis visualization mode, 
				// which shows areas of a page that are being GPU accelerated with a colored overlay.
				//Application.Current.Host.Settings.EnableCacheVisualization = true;
			}

			// Standard Silverlight initialization
			InitializeComponent();

			// Phone-specific initialization
			InitializePhoneApplication();

			// XNA initialization
			InitializeXnaApplication();
		}

		// Code to execute when the application is launching (eg, from Start)
		// This code will not execute when the application is reactivated
		private void Application_Launching(object sender, LaunchingEventArgs e)
		{
		}

		// Code to execute when the application is activated (brought to foreground)
		// This code will not execute when the application is first launched
		private void Application_Activated(object sender, ActivatedEventArgs e)
		{
		}

		// Code to execute when the application is deactivated (sent to background)
		// This code will not execute when the application is closing
		private void Application_Deactivated(object sender, DeactivatedEventArgs e)
		{
		}

		// Code to execute when the application is closing (eg, user hit Back)
		// This code will not execute when the application is deactivated
		private void Application_Closing(object sender, ClosingEventArgs e)
		{
		}

		// Code to execute if a navigation fails
		private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
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

		#region XNA application initialization

		// Performs initialization of the XNA types required for the application.
		private void InitializeXnaApplication()
		{
			// Create the ContentManager so the application can load precompiled assets
			Content = new ContentManager(this, "Content");

			// Create a GameTimer to pump the XNA FrameworkDispatcher
			FrameworkDispatcherTimer = new GameTimer();
			FrameworkDispatcherTimer.FrameAction += FrameworkDispatcherFrameAction;
			FrameworkDispatcherTimer.Start();
		}

		// An event handler that pumps the FrameworkDispatcher each frame.
		// FrameworkDispatcher is required for a lot of the XNA events and
		// for certain functionality such as SoundEffect playback.
		private void FrameworkDispatcherFrameAction(object sender, EventArgs e)
		{
			FrameworkDispatcher.Update();
		}

		#endregion

		#region IServiceProvider Members

		/// <summary>
		/// Gets a service from the ApplicationLifetimeObjects collection.
		/// </summary>
		/// <param name="serviceType">The type of service to retrieve.</param>
		/// <returns>The first item in the ApplicationLifetimeObjects collection of the requested type.</returns>
		public object GetService(Type serviceType)
		{
			// Find the first item that matches the requested type
			foreach (object item in ApplicationLifetimeObjects)
			{
				if (serviceType.IsAssignableFrom(item.GetType()))
					return item;
			}

			// Throw an exception if there was no matching item
			throw new InvalidOperationException("No object in the ApplicationLifetimeObjects is assignable to " + serviceType);
		}

		#endregion
	}
}