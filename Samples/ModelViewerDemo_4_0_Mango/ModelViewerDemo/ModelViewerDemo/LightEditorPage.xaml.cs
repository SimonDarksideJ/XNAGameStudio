#region File Description
//-----------------------------------------------------------------------------
// LightEditorPage.xaml.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Globalization;
using System.Windows;
using System.Windows.Navigation;

namespace ModelViewerDemo
{
    public partial class LightEditorPage : BasePage
    {
        public LightEditorPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            int lightIndex = int.Parse(NavigationContext.QueryString["light"], CultureInfo.InvariantCulture);

            switch (lightIndex)
            {
                case 1:
                    DataContext = Renderer.Current.State.Light1;
                    break;
                case 2:
                    DataContext = Renderer.Current.State.Light2;
                    break;
                case 3:
                    DataContext = Renderer.Current.State.Light3;
                    break;
            }

            base.OnNavigatedTo(e);
        }

        private void diffuseBtn_Click(object sender, RoutedEventArgs e)
        {
            diffuseEditor.Visibility = System.Windows.Visibility.Visible;
            specularEditor.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void specularBtn_Click(object sender, RoutedEventArgs e)
        {
            diffuseEditor.Visibility = System.Windows.Visibility.Collapsed;
            specularEditor.Visibility = System.Windows.Visibility.Visible;
        }

        private void resetBtn_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as LightState).ResetToDefaults();
        }
    }
}