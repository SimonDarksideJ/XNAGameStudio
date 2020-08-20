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
	public partial class CategoryPage 
	{
		public string Category { get; set; }
         
		public CategoryPage()
		{
			InitializeComponent();
            
            WindowsPhoneRecipes.Logger.Instance.AddLine();
		}

		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			// Navigate to the new page
            NavigationService.Navigate(new Uri("/ItemPage.xaml?cat=" + Category + "&item=" + (sender as Button).Name.Substring(7), UriKind.Relative));
		}

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void btnCategoryList_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/CategoriesListingPage.xaml", UriKind.Relative));
        }


		// When page is navigated to, set data context 
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

            WindowsPhoneRecipes.Logger.Instance.AddLine();

			Category = this.NavigationContext.QueryString["cat"];

			if (DataContext == null)
				this.DataContext = this;

            //simulate a big load that you want to avoid during recursive back navigation 
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

            //simulate large UI blocking work - that should be avoided all togerher
            //System.Threading.Thread.Sleep(5000);

		}

        // FOR DEBUGGING ONLY!
        //static int x = 0;
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            WindowsPhoneRecipes.Logger.Instance.AddLine();

            // on navigating away from the page, hide the appbar so we dont see it if we are recursive back
            this.ApplicationBar.IsVisible = false;

            // FOR DEBUGGING only! Simualte user canceling nav event during recursive back
            //if (x == 0)
            //{
            //    x++;
            //}
            //else
            //{
            //    //for debuging user canceling navigation
            //    // e.Cancel = true;
            //}
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // due to a bug, BackKey doesnt route navigation events so we handle it
            if (NavigationService.CanGoBack)
            {
                e.Cancel = true;
                NavigationService.GoBack();
            }
        }


	}
}