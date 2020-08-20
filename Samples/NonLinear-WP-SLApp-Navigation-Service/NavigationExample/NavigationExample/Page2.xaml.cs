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

namespace NavigationExample
{
    public partial class Page2 : PhoneApplicationPage
    {
        public Page2()
        {
            InitializeComponent();

            WindowsPhoneRecipes.Logger.Instance.AddLine();
        }

        private void btnNavigateToMainPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Page1.xaml", UriKind.RelativeOrAbsolute));
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