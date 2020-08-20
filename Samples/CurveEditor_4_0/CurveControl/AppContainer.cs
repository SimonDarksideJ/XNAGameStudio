#region File Description
//-----------------------------------------------------------------------------
// AppContainer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

//-----------------------------------------------------------------------------
// AppContainer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#region Using Statements
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections.Generic;
using System.Text;

#endregion



namespace Xna.Tools
{
    /// <summary>
    /// This class provide container instance that has IServiceContainer
    /// </summary>
    public class AppContainer : Container
    {
        ServiceContainer services = new ServiceContainer();

        protected override object GetService(Type service)
        {
            object so = services.GetService(service);
            if (so == null) so = base.GetService(service);
            return so;
        }
    }
}
