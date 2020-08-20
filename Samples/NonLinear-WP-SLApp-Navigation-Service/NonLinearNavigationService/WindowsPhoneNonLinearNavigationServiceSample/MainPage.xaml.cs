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
using System.Windows.Navigation;
using System.Diagnostics;

namespace WindowsPhoneRecipes
{
	public partial class MainPage
	{
		// Constructor
		public MainPage()
		{
			InitializeComponent();

            WindowsPhoneRecipes.Logger.Instance.AddLine();
		}

		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			// Navigate to the new page
            NavigationService.Navigate(new Uri("/CategoriesListingPage.xaml", UriKind.Relative));
		}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WindowsPhoneRecipes.Logger.Instance.AddLine();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            WindowsPhoneRecipes.Logger.Instance.AddLine();
        }


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // due to bug, BackKey doesnt send navigation events so we handle this myself
            if (NavigationService.CanGoBack)
            {
                e.Cancel = true;
                NavigationService.GoBack();
            }
        }

        private void btnAppBarLog_Click(object sender, EventArgs e)
        {
            // toggles the log area visibilty
            if (svLog.Visibility == Visibility.Collapsed)
            {
                txbLog.Text = WindowsPhoneRecipes.Logger.Instance.Log.ToString();
                svLog.Visibility = Visibility.Visible;
                ContentPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                svLog.Visibility = Visibility.Collapsed;
                ContentPanel.Visibility = Visibility.Visible;
            }
        }

	}
}
