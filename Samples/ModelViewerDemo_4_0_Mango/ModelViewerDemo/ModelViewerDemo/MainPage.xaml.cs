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
using System.Windows.Navigation;
using Microsoft.Xna.Framework;

namespace ModelViewerDemo
{
	public partial class MainPage : BasePage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			Renderer.Current.State.AnimateCameraPosition(new Vector3(1f, 2f, 1.5f));
			Renderer.Current.State.AnimateCameraTarget(Vector3.Zero);

			base.OnNavigatedTo(e);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			NavigationService.Navigate(new Uri("/EditPage.xaml", UriKind.Relative));
		}

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/FreeSpinPage.xaml", UriKind.Relative));
        }
	}
}