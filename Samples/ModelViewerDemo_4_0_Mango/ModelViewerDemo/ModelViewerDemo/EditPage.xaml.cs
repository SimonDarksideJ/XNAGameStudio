#region File Description
//-----------------------------------------------------------------------------
// EditPage.xaml.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Windows.Navigation;
using Microsoft.Xna.Framework;

namespace ModelViewerDemo
{
	public partial class EditPage : BasePage
	{
		public EditPage()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
            lightsButton.Visibility = Renderer.Current.State.EnableLighting ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
			Renderer.Current.State.AnimateCameraPosition(new Vector3(1.5f, .5f, 1f));
			Renderer.Current.State.AnimateCameraTarget(new Vector3(0f, .5f, 0f));

			base.OnNavigatedTo(e);
		}

		private void lightsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LightsPage.xaml", UriKind.Relative));
		}

		private void animationButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AnimationsPage.xaml", UriKind.Relative));
		}

		private void renderingButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            NavigationService.Navigate(new Uri("/RendererSettingsPage.xaml", UriKind.Relative));
		}
	}
}