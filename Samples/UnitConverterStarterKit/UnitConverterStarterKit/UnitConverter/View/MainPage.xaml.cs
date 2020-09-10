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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Applications.UnitConverter.Helpers;
using Microsoft.Phone.Applications.UnitConverter.View;
using Microsoft.Phone.Applications.UnitConverter.ViewModel;

namespace Microsoft.Phone.Applications.UnitConverter
{
    /// <summary>
    /// Main page code behind
    /// </summary>
    public partial class MainPage : PageCommon
    {
        /// <summary>
        /// View model
        /// </summary>
        internal MainPageViewModel viewModel = new MainPageViewModel();

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainPage()
            : base()
        {
            this.DataContext = this.viewModel;
            InitializeComponent();
        }


        /// <summary>
        /// Handles the Page layout event for the main page.
        /// This event will be raised when the page has rendered, or almost 
        /// fully rendered.
        /// This event can be raised multiple times, so we check to see if we 
        /// were just launched. If we were, then we perform additional work to 
        /// load configuration data needed by the application. This allows the 
        /// initial page render to be as fast as possible
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        protected override void PageLayoutUpdated(object sender, EventArgs e)
        {
            base.PageLayoutUpdated(sender, e);
            if (ApplicationState.ApplicationStartup == AppOpenState.Launching)
            {
                this.viewModel.DeferStartup(this.SignalFavoritesAreLoaded);
                ApplicationState.ApplicationStartup = AppOpenState.None;
            }
        }

        /// <summary>
        /// Signals the favorites are loaded and that we can enable the navigation
        /// buttons on the page. This is called from a thread pool function, so
        ///  we need to update the UI on the UI thread
        /// </summary>
        private void SignalFavoritesAreLoaded()
        {
            Dispatcher.BeginInvoke(() => this.viewModel.AllowNavigation = true);
        }

        /// <summary>
        /// Called when we are leaving the page.
        /// We clear the constructorCalled flag so that we will be able to 
        /// determine if we were tomb stoned or not if we receive an activation event
        /// </summary>
        /// <param name="e"> Navigation event arguments</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.viewModel.SyncStateToAppState();
            ApplicationState.AddAppObjects(false);
        }

        /// <summary>
        /// Called when we have a page to navigated event
        /// This event is raised whenever we visit the page. This can occur for the 
        /// following situations
        /// 1) Application Launch. We need to perform initial app initialization
        /// 2) Application Activation where we are tomb stoned, or we are still in memory
        ///    If we have been tomb stoned, we need to read objects from the application
        ///    service, and then perform any initialization on the restored objects.
        ///    If we received and activation event, but we were not tomb stoned, then 
        ///    we actually don't need to do anything except clear the main application 
        ///    state flag and exit. This is because all objects are still in memory.
        /// 3) We have returned from the conversions page in the application.
        /// 
        /// </summary>
        /// <param name="e">The Navigation event args</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.viewModel.ProcessNavigatedToEvent(this.IsPageActivated,
                this.SignalFavoritesAreLoaded);
        }

        /// <summary>
        /// Called when Conversion button is clicked to go to the conversions page.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event data.</param>
        private void OnConversionsClicked(object sender, RoutedEventArgs e)
        {
            this.viewModel.SyncStateToAppState();
            this.viewModel.AllowNavigation =
                !this.ForwardNavigate(ApplicationState.CategoryPageName, UriKind.Relative);
        }

        /// <summary>
        /// Called when Add to favorites button is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event data.</param>
        internal void OnFavoritesClicked(object sender, RoutedEventArgs e)
        {
            this.viewModel.AllowNavigation = false;
            if (this.viewModel.AddToFavorite(this.SignalFavoritesAreLoaded))
            {
                this.viewModel.AllowNavigation = true;
            }
        }

        #region Keyboard Handlers
        /// <summary>
        /// Called when user clicks any digit or decimal
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event data.</param>
        internal void OnClickNumber(object sender, RoutedEventArgs e)
        {
            Button b = (Button)(sender);
            this.viewModel.NumberKeyHandler(b);
        }

        /// <summary>
        /// Called when User clicks on the back button on the keypad. Remove one characater
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event data.</param>
        internal void OnClickBack(object sender, RoutedEventArgs e)
        {
            this.viewModel.BackKeyHandler();
        }

        /// <summary>
        /// Called when User clicks on the back button on the keypad. Remove one character
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event data.</param>
        internal void OnClickSign(object sender, RoutedEventArgs e)
        {
            this.viewModel.SignKeyHandler();
        }

        /// <summary>
        /// Called when user presses the clear key on the keypad
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event data.</param>
        internal void OnClickClear(object sender, RoutedEventArgs e)
        {
            this.viewModel.ClearKeyHandler();
        }

        /// <summary>
        /// Button handler to switch which unit is the source or target
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event data</param>
        internal void OnClickSourceTargetUnit(object sender, RoutedEventArgs e)
        {
            this.viewModel.SwitchSourceTargetUnit();
        }
        #endregion
        
    }
}
