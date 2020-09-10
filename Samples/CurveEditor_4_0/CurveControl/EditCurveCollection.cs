//-----------------------------------------------------------------------------
// EditCurveCollection.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Xna.Tools
{
    /// <summary>
    /// EditCurve event argument that passes EditCurve instance.
    /// </summary>
    public class EditCurveEventArgs : EventArgs
    {
        public EditCurve Curve;

        public EditCurveEventArgs(EditCurve curve)
        {
            Curve = curve;
        }
    }

    /// <summary>
    /// EditCurveCollection that fires event when collection has been changed.
    /// </summary>
    public class EditCurveCollection : Collection<EditCurve>
    {
        /// <summary>
        /// Occurs when EditCurve adding to Collection.
        /// </summary>
        public event EventHandler<EditCurveEventArgs> AddingCurve;

        /// <summary>
        /// Occurs when EditCurve removing from Collection
        /// </summary>
        public event EventHandler<EditCurveEventArgs> RemovingCurve;

        /// <summary>
        /// Occurs when EditCurve collection changed.
        /// </summary>
        public event EventHandler Changed;

        #region Overrided methods

        protected override void RemoveItem(int index)
        {
            if (RemovingCurve != null)
                RemovingCurve(this, new EditCurveEventArgs(Items[index]));

            if (Changed != null)
                Changed(this, EventArgs.Empty);

            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            if (RemovingCurve != null)
            {
                foreach (EditCurve item in Items)
                    RemovingCurve(this, new EditCurveEventArgs(item));
            }

            if (Changed != null) Changed(this, EventArgs.Empty);
            base.ClearItems();
        }

        protected override void InsertItem(int index, EditCurve item)
        {
            if (AddingCurve != null)
                AddingCurve(this, new EditCurveEventArgs(item));

            if (Changed != null)
                Changed(this, EventArgs.Empty);

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, EditCurve item)
        {
            if (RemovingCurve != null)
                RemovingCurve(this, new EditCurveEventArgs(Items[index]));

            if (AddingCurve != null)
                AddingCurve(this, new EditCurveEventArgs(item));

            if (Changed != null) Changed(this, EventArgs.Empty);

            base.SetItem(index, item);
        }
        #endregion

    }
}
