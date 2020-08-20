// Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
//
//
// Use of this source code is subject to the terms of the Microsoft
// license agreement under which you licensed this source code.
// If you did not accept the terms of the license agreement,
// you are not authorized to use this source code.
// For the terms of the license, please see the license agreement
// signed by you and Microsoft.
// THE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//

namespace Microsoft.Phone.Applications.UnitConverter.Helpers
{

    /// <summary>
    /// Hint to assist page loading to optimize work
    /// </summary>
    public enum AppOpenState
    {
        /// <summary>
        /// Internal state. object state is reset to this after initial page load
        /// </summary>
        None,

        /// <summary>
        /// App is launching
        /// </summary>
        Launching,

        /// <summary>
        /// App is being activated
        /// </summary>
        Activated,

        /// <summary>
        /// App is being deactivated
        /// </summary>
        Deactivated,

        /// <summary>
        /// App is closing
        /// </summary>
        Closing
    }
}
