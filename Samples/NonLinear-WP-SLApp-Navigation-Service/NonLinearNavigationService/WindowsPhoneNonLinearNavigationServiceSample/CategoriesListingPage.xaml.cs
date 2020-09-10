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
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WindowsPhoneRecipes
{
	public partial class CategoriesListingPage 
	{
		public CategoriesListingPage()
		{
			InitializeComponent();

            WindowsPhoneRecipes.Logger.Instance.AddLine();
		}

		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			// Navigate to the new page
            NavigationService.Navigate(new Uri("/CategoryPage.xaml?cat=" + (sender as Button).Name.Substring(7), UriKind.Relative));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void btnMenu_Click(object sender, RoutedEventArgs e)
		{
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            WindowsPhoneRecipes.Logger.Instance.AddLine();

            // due to bug, BackKey doesnt send navigation events so we handle this myself
            if (NavigationService.CanGoBack)
            {
                e.Cancel = true;
                NavigationService.GoBack();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WindowsPhoneRecipes.Logger.Instance.AddLine();

            // If we are in recursive back - DO NOT DO ANY WORK ON PAGE
            // Developers - make sure you have no specific logic that you need to take care of here
            if (NonLinearNavigationService.Instance.IsRecursiveBackNavigation == true)
            {
                WindowsPhoneRecipes.Logger.Instance.AddLine("IsRecursiveBackNavigation = true");
                return;
            }
            //else
            /*
             * DO WORK HERE - like animation, data biding, and so on...
             */
            this.ApplicationBar.IsVisible = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            WindowsPhoneRecipes.Logger.Instance.AddLine();

            // on navigating away from the page, hide the appbar so we dont see it if we are recursive back
            this.ApplicationBar.IsVisible = false;
        }
	}
}