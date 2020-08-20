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

namespace WindowsPhoneRecipes
{
	public partial class ItemPage 
	{
		public string Category { get; set; }
		public string ItemId { get; set; }

		public ItemPage()
		{
			InitializeComponent();
            
            WindowsPhoneRecipes.Logger.Instance.AddLine();
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

            WindowsPhoneRecipes.Logger.Instance.AddLine();

			Category = this.NavigationContext.QueryString["cat"];
			ItemId = this.NavigationContext.QueryString["item"];

			if (DataContext == null)
				this.DataContext = this;

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

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            WindowsPhoneRecipes.Logger.Instance.AddLine();

            this.ApplicationBar.IsVisible = false;
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

		private void btnCategory_Click(object sender, RoutedEventArgs e)
		{
            NavigationService.Navigate(new Uri("/CategoryPage.xaml?cat=" + Category, UriKind.Relative));
		}

		private void btnCategoryList_Click(object sender, RoutedEventArgs e)
		{
            NavigationService.Navigate(new Uri("/CategoriesListingPage.xaml", UriKind.Relative));
		}

		private void btnMenu_Click(object sender, RoutedEventArgs e)
		{
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
		}

		private void btnItem_Click(object sender, RoutedEventArgs e)
		{
            NavigationService.Navigate(new Uri("/ItemPage.xaml?cat=" + Category + "&item=" + (ItemId == "1" ? "2" : "1"), UriKind.Relative));
		}

	}
}