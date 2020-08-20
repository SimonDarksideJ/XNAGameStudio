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
using System;

namespace Microsoft.Phone.Applications.UnitConverter.Model
{
    /// <summary>
    /// Category page state information for saving for restoring and passing state
    /// back to the main page
    /// </summary>
    public class CategoryPageState : CommonPageState
    {

        /// <summary>
        /// True if the context menu is displayed
        /// </summary>
        public bool IsContextMenuDisplayed { get; set; }

        /// <summary>
        /// Pivot index of the current page
        /// </summary>
        public Int32 PivotSelectedIndex { get; set; }

        /// <summary>
        /// True if this is the favorite category
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// True if we are to apply the changes on the main page
        /// </summary>
        public bool Apply { get; set; }

    }
}
