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

using System.Windows.Controls;

namespace Microsoft.Phone.Applications.UnitConverter.View
{
    /// <summary>
    /// Wrap the small user control that will house two list views
    /// </summary>
    public partial class TwoListBoxes : UserControl
    {
        /// <summary>
        /// Gets or sets the from list box.
        /// </summary>
        /// <value>From list box.</value>
        public ListBox FromListBox { get; private set; }

        /// <summary>
        /// Gets or sets the to list box.
        /// </summary>
        /// <value>To list box.</value>
        public ListBox ToListBox { get; private set; }

        /// <summary>
        /// Delegate for the from list box selection changed handler
        /// </summary>
        public event SelectionChangedEventHandler FromSelectionChanged;

        /// <summary>
        /// Delegate for the to list box selection changed handler
        /// </summary>
        public event SelectionChangedEventHandler ToSelectionChanged;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TwoListBoxes()
        {
            InitializeComponent();
            this.FromListBox = fromListView;
            this.ToListBox = toListView;
        }

        /// <summary>
        /// Handle the from list box selection events
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event data.</param>
        private void OnFromSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChangedEventHandler handler = this.FromSelectionChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        /// <summary>
        /// Handle the to list box selection events
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event data.</param>
        private void OnToSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChangedEventHandler handler = this.ToSelectionChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }
    }
}
