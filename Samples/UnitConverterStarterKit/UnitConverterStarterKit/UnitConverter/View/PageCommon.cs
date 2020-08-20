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
using Microsoft.Phone.Controls;

namespace Microsoft.Phone.Applications.UnitConverter.View
{
    /// <summary>
    /// Common functions for all pages in the application
    /// </summary>
    public class PageCommon : PhoneApplicationPage
    {

        /// <summary>
        /// True when the page is activated. Set true in the constructor, and 
        /// false when we navigate away from the page.
        /// </summary>
        protected bool IsPageActivated { get; private set; }


        /// <summary>
        /// Common base class for pages in the application
        /// </summary>
        public PageCommon()
            : base()
        {
            this.Loaded += this.PageLoaded;
            this.LayoutUpdated += new EventHandler(this.PageLayoutUpdated);
            this.IsPageActivated = true;
        }

      


        /// <summary>
        /// Handles the Loaded event of the MainPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        protected virtual void PageLoaded(object sender, RoutedEventArgs e)
        {
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
        protected virtual void PageLayoutUpdated(object sender, EventArgs e)
        {
            this.LayoutUpdated -= new EventHandler(this.PageLayoutUpdated);
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
            this.IsPageActivated = false;
        }

        /// <summary>
        /// Called when we are navigating to the page
        /// </summary>
        /// <param name="e">Event Data</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }


        /// <summary>
        /// Forward Navigate to a new page
        /// </summary>
        /// <param name="page">page to navigate to</param>
        /// <param name="uriType">Type of the URI</param>
        /// <returns>true if the navigation was successful, false otherwise</returns>
        protected bool ForwardNavigate(string page, UriKind uriType)
        {
            bool isNavigateSuccessful =  false;
            try
            {
                isNavigateSuccessful = NavigationService.Navigate( new Uri(page, uriType));
            }
            catch (Exception ex)
            {
                ApplicationState.ErrorLog.Add(
                    new ErrorLog( " Nav  Failure", ex.Message));
            }
            return isNavigateSuccessful;
        }

        /// <summary>
        /// Handle a reverse navigate request
        /// </summary>
        /// <returns>true if the navigation was successful, false otherwise</returns>
        protected bool ReverseNavigate()
        {
            bool isNavigateSuccessful = false;
            try
            {
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                    isNavigateSuccessful = true;
                }
            }
         
            catch (Exception ex)
            {
                ApplicationState.ErrorLog.Add(
                    new ErrorLog(" Nav Back Failure", ex.Message));
            }
            return isNavigateSuccessful;
        }
    }
}
