/* 
    Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
    
*/

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

namespace CustomIndeterminateProgressBarSample
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void toggleButton_Click(object sender, RoutedEventArgs e)
        {
            customIndeterminateProgressBar.IsIndeterminate = !(customIndeterminateProgressBar.IsIndeterminate);

            if (customIndeterminateProgressBar.Visibility == Visibility.Collapsed)
            {
                customIndeterminateProgressBar.Visibility = Visibility.Visible;
            }
            else
            {
                customIndeterminateProgressBar.Visibility = Visibility.Collapsed;
            }
        }   
    }
}