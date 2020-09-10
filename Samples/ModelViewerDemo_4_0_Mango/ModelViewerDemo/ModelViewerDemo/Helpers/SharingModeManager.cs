#region File Description
//-----------------------------------------------------------------------------
// SharingModeManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Windows;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ModelViewerDemo
{
    /// <summary>
    /// SharingModeManager should be added into the ApplicationLifetimeObjects AFTER
    /// the SharedGraphicsDeviceManager and BEFORE any other objects that will want to
    /// leverage the GraphicsDevice. The SharingModeManager is responsible for managing
    /// the sharing mode for the application to enable it as soon as possible and disable
    /// it when the application is deactivating. This means it is only useful for apps
    /// that want to always use XNA for rendering.
    /// </summary>
    public sealed class SharingModeManager : IApplicationLifetimeAware, IApplicationService
    {
        void IApplicationLifetimeAware.Started()
        {
            GraphicsDevice graphicsDevice = SharedGraphicsDeviceManager.Current.GraphicsDevice;

            graphicsDevice.SetSharingMode(true);

            // Hook the application's Activated and Deactivated events so we make sure to turn off sharing mode
            // when we're deactivating and turn it back on when activating.
            PhoneApplicationService.Current.Activated += (s, e) => graphicsDevice.SetSharingMode(true);
            PhoneApplicationService.Current.Deactivated += (s, e) => graphicsDevice.SetSharingMode(false);
        }

        void IApplicationLifetimeAware.Exited()
        {
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);
        }

        // Most of the interface methods need no implementation
        void IApplicationLifetimeAware.Starting() { }
        void IApplicationLifetimeAware.Exiting() { }
        void IApplicationService.StartService(ApplicationServiceContext context) { }
        void IApplicationService.StopService() { }
    }
}
