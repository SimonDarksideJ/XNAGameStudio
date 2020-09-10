#region File Description
//-----------------------------------------------------------------------------
// RendererSettingsPage.xaml.cs
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
	public partial class RendererSettingsPage : BasePage
	{
        public RendererSettingsPage()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			Renderer.Current.State.AnimateCameraPosition(new Vector3(1.5f, 0.1f, -1f));
			Renderer.Current.State.AnimateCameraTarget(new Vector3(.5f, .25f, 0f));

			base.OnNavigatedTo(e);
		}
	}
}