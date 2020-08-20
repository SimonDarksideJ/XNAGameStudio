//-----------------------------------------------------------------------------
// CurveControlCommands.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace Xna.Tools
{
    /// <summary>
    /// Command that saves old and new EditCurveKey selection.
    /// </summary>
    public class SelectCommand : ICommand
    {
        public SelectCommand(EditCurve curve, EditCurveKeySelection newSelection,
                                                EditCurveKeySelection oldSelection)
        {
            this.curve = curve;
            this.oldSelection = oldSelection;
            this.newSelection = newSelection;
        }

        #region ICommand Members

        public void Execute()
        {
            curve.ApplySelection(newSelection, false);
        }

        public void Unexecute()
        {
            curve.ApplySelection(oldSelection, false);
        }

        #endregion

        EditCurve curve;
        EditCurveKeySelection oldSelection;
        EditCurveKeySelection newSelection;
    }

    /// <summary>
    /// Command that saves add or remove EditCurveKey information.
    /// </summary>
    public class EditCurveKeyAddRemoveCommand : ICommand
    {
        public EditCurveKeyAddRemoveCommand(EditCurve curve,
                                                ICollection<EditCurveKey> deleteKeys)
        {
            this.curve = curve;
            addKey = false;

            keys = new List<EditCurveKey>(deleteKeys.Count);
            foreach ( EditCurveKey key in deleteKeys )
                keys.Add(key.Clone());
        }

        public EditCurveKeyAddRemoveCommand(EditCurve curve, EditCurveKey addKey,
                                                    EditCurveKeySelection selection)
        {
            this.curve = curve;
            this.addKey = true;
            this.selection = selection.Clone();

            keys = new List<EditCurveKey>();
            keys.Add(addKey.Clone());
        }

        #region ICommand Members

        public void Execute()
        {
            if (addKey)
                AddKeys();
            else
                RemoveKeys();
        }

        public void Unexecute()
        {
            if (addKey)
                RemoveKeys();
            else
                AddKeys();
        }

        #endregion

        #region Private Methods
        private void AddKeys()
        {
            foreach (EditCurveKey savedKey in keys)
            {
                EditCurveKey addingKey = savedKey.Clone();

                curve.Keys.Add(addingKey);

                // Removing key requires re-compute neighbor keys tangents.
                curve.ComputeTangents(curve.Keys.IndexOf(addingKey));
            }
            curve.ApplySelection(new EditCurveKeySelection(keys), false);
        }

        private void RemoveKeys()
        {
            foreach (EditCurveKey savedKey in keys)
            {
                long keyId = savedKey.Id;

                EditCurveKey key;
                curve.Keys.TryGetValue(keyId, out key);

                // Remember key index.
                int idx = curve.Keys.IndexOf(key);

                // Remove key from keys.
                curve.Keys.Remove(key);

                // Removing key requires re-compute neighbor keys tangents.
                curve.ComputeTangents(idx);
            }

            if ( selection != null )
                curve.ApplySelection(selection, false);
        }
        #endregion

        EditCurve curve;
        EditCurveKeySelection selection;
        List<EditCurveKey> keys;
        bool addKey;
    }

    /// <summary>
    /// Command that saves multiple EditCurveKey values.
    /// </summary>
    public class EditCurveKeyUpdateCommand : ICommand
    {
        public EditCurveKeyUpdateCommand(EditCurve curve,
                                            ICollection<EditCurveKey> oldKeyValues,
                                            ICollection<EditCurveKey> newKeyValues)
        {
            this.curve = curve;
            this.oldKeyValues = oldKeyValues;
            this.newKeyValues = newKeyValues;
        }

        #region ICommand Members

        public void Execute()
        {
            curve.ApplyKeyValues(newKeyValues);
        }

        public void Unexecute()
        {
            curve.ApplyKeyValues(oldKeyValues);
        }

        #endregion

        EditCurve curve;
        ICollection<EditCurveKey> oldKeyValues;
        ICollection<EditCurveKey> newKeyValues;
    }

    /// <summary>
    /// Command that saves EditCurveState.
    /// </summary>
    public class EditCurveStateChangeCommand : ICommand
    {
        public EditCurveStateChangeCommand(EditCurve curve, EditCurveState oldState,
                                                                EditCurveState newState)
        {
            if (oldState == null) throw new ArgumentNullException("oldState");
            if (newState == null) throw new ArgumentNullException("newState");

            this.curve = curve;
            this.oldState = (EditCurveState)oldState.Clone();
            this.newState = (EditCurveState)newState.Clone();
        }

        #region ICommand Members

        public void Execute()
        {
            curve.ApplyState(newState);
        }

        public void Unexecute()
        {
            curve.ApplyState(oldState);
        }

        #endregion

        EditCurve curve;
        EditCurveState newState;
        EditCurveState oldState;
    }

}
