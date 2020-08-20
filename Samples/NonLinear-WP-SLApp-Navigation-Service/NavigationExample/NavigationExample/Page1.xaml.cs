using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Diagnostics;

namespace NavigationExample
{
    public partial class Page1 : PhoneApplicationPage
    {
        static int _PageCount = 0;

        // Constructor
        public Page1()
        {
            InitializeComponent();
            WindowsPhoneRecipes.Logger.Instance.AddLine("PageCount = " + _PageCount);

            _PageCount++;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            WindowsPhoneRecipes.Logger.Instance.AddLine();

            int pageCount=0;

            if (this.NavigationContext.QueryString.ContainsKey("PageCount"))
            {
                int.TryParse(this.NavigationContext.QueryString["PageCount"], out pageCount);
            }
            this.txtPageCount.Text = "Page count = " + pageCount.ToString();            
        }

        // Fregmented navigation is not supported.
        private void btnMoveNext_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Page1.xaml", UriKind.RelativeOrAbsolute));
        }

        // This creates a new instance of the page
        private void btnMoveNextNew_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Page1.xaml?PageCount=" + _PageCount.ToString(), UriKind.RelativeOrAbsolute));
        }

        private void btnMoveToPage2_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Page2.xaml", UriKind.RelativeOrAbsolute));
        }

        private void btnAppBarLog_Click(object sender, EventArgs e)
        {
            // toggles the log area visibilty
            if (svLog.Visibility == Visibility.Collapsed)
            {
                txbLog.Text =WindowsPhoneRecipes.Logger.Instance.Log.ToString();
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