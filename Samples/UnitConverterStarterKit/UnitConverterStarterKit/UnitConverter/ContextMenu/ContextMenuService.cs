// Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
//
// Use of this source code is subject to the terms of the Microsoft
// license agreement under which you licensed this source code.
// If you did not accept the terms of the license agreement,
// you are not authorized to use this source code.
// For the terms of the license, please see the license agreement
// signed by you and Microsoft.
// THE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//

using System.Windows;


namespace Microsoft.Phone.Applications.Common.Controls

{
    /// <summary>
    /// Provides service to controls to attach ContextMenu.
    /// </summary>
    public static class ContextMenuService
    {
        /// <summary>
        /// Identifies the ContextMenu attached property.
        /// </summary>
        public static readonly DependencyProperty ContextMenuProperty
            = DependencyProperty.RegisterAttached(
                "ContextMenu",
                typeof(ContextMenu),
                typeof(ContextMenuService),
                new PropertyMetadata(null, OnContextMenuChanged));

        /// <summary>
        /// Handles changes to the ContextMenu DependencyProperty.
        /// </summary>
        /// <param name="o">DependencyObject that changed.</param>
        /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
        private static void OnContextMenuChanged(DependencyObject o
            , DependencyPropertyChangedEventArgs e)
        {
            var element = o as FrameworkElement;

            var oldContextMenu = e.OldValue as ContextMenu;
            if (oldContextMenu != null)
            {
                oldContextMenu.Owner = null;
            }

            var newContextMenu = e.NewValue as ContextMenu;
            if (newContextMenu != null)
            {
                newContextMenu.Owner = element;
            }
        }

        /// <summary>
        /// Gets the value of the ContextMenu property of the specified object.
        /// </summary>
        /// <param name="obj">Object to query concerning the ContextMenu property.</param>
        /// <returns>Value of the ContextMenu property.</returns>
        public static ContextMenu GetContextMenu(DependencyObject obj)
        {
            return (ContextMenu)obj.GetValue(ContextMenuProperty);
        }

        /// <summary>
        /// Sets the value of the ContextMenu property of the specified object.
        /// </summary>
        /// <param name="obj">Object to set the property on.</param>
        /// <param name="value">Value to set.</param>
        public static void SetContextMenu(DependencyObject obj, ContextMenu value)
        {
            obj.SetValue(ContextMenuProperty, value);
        }
    }
}
