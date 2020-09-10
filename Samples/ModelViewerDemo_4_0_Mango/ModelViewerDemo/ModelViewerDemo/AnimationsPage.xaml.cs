#region File Description
//-----------------------------------------------------------------------------
// AnimationsPage.xaml.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Windows.Navigation;
using Microsoft.Xna.Framework;

namespace ModelViewerDemo
{
    public partial class AnimationsPage : BasePage
    {
        public AnimationsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Renderer.Current.State.AnimateCameraPosition(new Vector3(0f, .8f, 2.3f));
            Renderer.Current.State.AnimateCameraTarget(new Vector3(0f, -.4f, 0f));

            base.OnNavigatedTo(e);
        }
    }
}