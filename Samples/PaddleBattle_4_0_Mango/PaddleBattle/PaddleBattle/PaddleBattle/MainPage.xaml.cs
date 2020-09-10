#region File Description
//-----------------------------------------------------------------------------
// MainPage.xaml.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace PaddleBattle
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            // Set the game's settings as the DataContext for our page, to hook up the databinding
            // for the sounds checkbox
            DataContext = (Application.Current as App).Settings;
        }

        // Respond to pressing the "Play" button by navigating to the game
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/GamePage.xaml", UriKind.Relative));
        }
    }
}