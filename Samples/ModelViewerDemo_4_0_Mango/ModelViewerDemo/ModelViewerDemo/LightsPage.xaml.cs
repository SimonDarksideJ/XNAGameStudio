#region File Description
//-----------------------------------------------------------------------------
// LightsPage.xaml.cs
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
    public partial class LightsPage : BasePage
    {
        public LightsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Renderer.Current.State.AnimateCameraPosition(new Vector3(-2f, 2f, 2f));
            Renderer.Current.State.AnimateCameraTarget(new Vector3(0f, .5f, 0f));

            base.OnNavigatedTo(e);
        }

        private void light1Btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LightEditorPage.xaml?light=1", UriKind.Relative));
        }

        private void light2Btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LightEditorPage.xaml?light=2", UriKind.Relative));
        }

        private void light3Btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LightEditorPage.xaml?light=3", UriKind.Relative));
        }

        private void resetBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Renderer.Current.State.Light1.ResetToDefaults();
            Renderer.Current.State.Light2.ResetToDefaults();
            Renderer.Current.State.Light3.ResetToDefaults();
        }
    }
}