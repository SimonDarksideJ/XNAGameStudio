//-----------------------------------------------------------------------------
// EditCurve.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace Xna.Tools
{
    /// <summary>
    /// This class contains curve editting related information.
    /// </summary>
    public class EditCurve
    {
        #region Properties
        /// <summary>
        /// Sets/Gets Control that owns this EditCurve.
        /// </summary>
        public Control Owner { get { return owner; } set { owner = value; } }

        /// <summary>
        /// Sets/Gets Id of this EditCurve.
        /// </summary>
        public long Id { get { return id; } set { id = value; } }

        /// <summary>
        /// Sets/Gets Name of this EditCurve.
        /// </summary>
        public string Name
        {
            get { return state.Name; }
            set { state.Name = value; FireStateChangeEvent(); }
        }

        /// <summary>
        /// Sets/Gets Color of this EditCurve.
        /// </summary>
        public System.Drawing.Color Color
        {
            get { return color; }
            set { color = value; FireStateChangeEvent(); }
        }

        /// <summary>
        /// Gets EditCurveKeyCollection.
        /// </summary>
        public EditCurveKeyCollection Keys { get { return keys; } }

        /// <summary>
        /// Sets/Gets PreLoop
        /// </summary>
        public CurveLoopType PreLoop
        {
            get { return OriginalCurve.PreLoop; }
            set
            {
                state.PreLoop = OriginalCurve.PreLoop = value;
                FireStateChangeEvent();
            }
        }

        /// <summary>
        /// Sets/Gets PostLoop
        /// </summary>
        public CurveLoopType PostLoop
        {
            get { return OriginalCurve.PostLoop; }
            set {
                state.PostLoop = OriginalCurve.PostLoop = value;
                FireStateChangeEvent();
            }
        }

        /// <summary>
        /// Gets original curve object.
        /// </summary>
        public Curve OriginalCurve { get { return originalCurve; } }

        /// <summary>
        /// Sets/Gets Visible that represents this EditCurve rendered or not
        /// in CurveControl.
        /// </summary>
        public bool Visible { get { return visible; } set { visible = value; } }

        /// <summary>
        /// Sets/Gets Editable that represents this EditCurve could update states.
        /// </summary>
        public bool Editable { get { return editable; } set { editable = value; } }

        /// <summary>
        /// Sets/Gets Dirty flag for file saving.
        /// </summary>
        public bool Dirty { get { return dirty; } set { dirty = value; } }

        /// <summary>
        /// Get selections.
        /// </summary>
        public EditCurveKeySelection Selection { get { return selection; } }

        /// <summary>
        /// Occures after EditCurve state (Name, Visible, Editable, PreLoop,
        /// and PostLoop) changed.
        /// </summary>
        public event EventHandler StateChanged;

        #endregion

        #region Constructors
        public EditCurve(string name, System.Drawing.Color curveColor, CommandHistory commandHistory)
        {
            originalCurve = new Curve();
            this.color = curveColor;
            keys = new EditCurveKeyCollection(this);

            state.Name = name;
            state.PreLoop = OriginalCurve.PreLoop;
            state.PostLoop = OriginalCurve.PostLoop;

            this.commandHistory = commandHistory;
        }

        public EditCurve(string name, System.Drawing.Color curveColor, Curve curve,
                            CommandHistory commandHistory)
        {
            originalCurve = curve;
            this.color = curveColor;
            keys = new EditCurveKeyCollection(this);

            state.Name = name;
            state.PreLoop = OriginalCurve.PreLoop;
            state.PostLoop = OriginalCurve.PostLoop;

            this.commandHistory = commandHistory;
        }
        #endregion

        public override string ToString()
        {
            return state.Name;
        }

        #region Public Methods

        /// <summary>
        /// Evaluate this curve at given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public float Evaluate(float position)
        {
            return OriginalCurve.Evaluate(position);
        }

        /// <summary>
        /// Begin update curve parameters.
        /// </summary>
        /// <remarks>It records curve satte and key values modification between
        /// BeginUpdate and EndUpdate method.</remarks>
        public void BeginUpdate()
        {
            if (inUpdating)
                throw new InvalidOperationException("BeginUpdate called twice.");

            modifiedKeys = new Dictionary<long, EditCurveKey>();
            savedState = (EditCurveState)state.Clone();

            inUpdating = true;
        }

        /// <summary>
        /// EndUpdate and generate Undo/Redo command buffer if there is any
        /// modification happend since BeginUpdate called.
        /// </summary>
        public void EndUpdate()
        {
            if (!inUpdating)
                throw new InvalidOperationException(
                    "You must call BeginUpdate before call EndUpdate.");

            // Compare modified key values.
            if (modifiedKeys != null && modifiedKeys.Count > 0)
            {
                List<EditCurveKey> oldKeyValues =
                    new List<EditCurveKey>(modifiedKeys.Count);

                List<EditCurveKey> newKeyValues =
                    new List<EditCurveKey>(modifiedKeys.Count);

                foreach (EditCurveKey savedKey in modifiedKeys.Values)
                {
                    EditCurveKey curKey;
                    if (keys.TryGetValue(savedKey.Id, out curKey))
                    {
                        if (!curKey.Equals(savedKey))
                        {
                            // Saved value is already cloned.
                            oldKeyValues.Add(savedKey);
                            newKeyValues.Add(curKey.Clone());
                        }
                    }
                }

                if (newKeyValues.Count != 0)
                {
                    dirty = true;
                    if (commandHistory != null)
                        commandHistory.Add(new EditCurveKeyUpdateCommand(
                            this, oldKeyValues, newKeyValues));
                }
            }
            modifiedKeys = null;

            // Compare states
            bool stateChanged = state != savedState;
            if (commandHistory != null && stateChanged)
                commandHistory.Add(new EditCurveStateChangeCommand(
                    this, savedState, state));

            savedState = null;
            inUpdating = false;

            if (stateChanged) FireStateChangeEvent();
        }

        /// <summary>
        /// Select keys and key tangents.
        /// </summary>
        /// <param name="selectRegion">Selection region in unit coordinate.</param>
        /// <param name="tangentScale">Tangent scale in unit coordinate.</param>
        /// <param name="keyView"></param>
        /// <param name="tangentView"></param>
        /// <param name="toggleSelection"></param>
        /// <param name="singleSelection"></param>
        public void Select(BoundingBox selectRegion, Vector2 tangentScale,
                            EditCurveView keyView, EditCurveView tangentView,
                                bool toggleSelection, bool singleSelect)
        {
            if (!Editable) return;

            EditCurveKeySelection newSelection = new EditCurveKeySelection();

            // Check Intersection of Keys and Tangents.
            if (keyView != EditCurveView.Never)
            {
                ICollection<EditCurveKey> targetKeys =
                        (keyView == EditCurveView.Always) ?
                        (ICollection<EditCurveKey>)keys :
                        (ICollection<EditCurveKey>)selectedKeys.Values;
                newSelection.SelectKeys(targetKeys, selectRegion, singleSelect);
            }

            // Check Tangents if any keys are not selected.
            if (newSelection.Count == 0 && tangentView != EditCurveView.Never)
            {
                ICollection<EditCurveKey> targetKeys
                    = (tangentView == EditCurveView.Always) ?
                        (ICollection<EditCurveKey>)keys :
                        (ICollection<EditCurveKey>)selectedKeys.Values;

                newSelection.SelectTangents(targetKeys, selectRegion, tangentScale,
                                                singleSelect);
            }

            if (toggleSelection)
                newSelection = EditCurveKeySelection.ToggleSelection(
                                                    selection, newSelection);

            ApplySelection(newSelection, true);
        }

        /// <summary>
        /// Clear selection.
        /// </summary>
        public void ClearSelection()
        {
            ApplySelection(new EditCurveKeySelection(), true);
        }

        /// <summary>
        /// Apply key selection.
        /// </summary>
        /// <param name="newSelection"></param>
        /// <param name="generateCommand"></param>
        public void ApplySelection(EditCurveKeySelection newSelection,
                                    bool generateCommand)
        {
            // Re-create selected keys and store selection information from
            // new selection.
            selectedKeys.Clear();;
            foreach (long id in newSelection.Keys)
            {
                EditCurveKey key = keys.GetValue(id);
                key.Selection = newSelection[id];
                selectedKeys.Add(key.Id, key);
            }

            // Clear de-selected keys selection information.
            foreach (long id in selection.Keys)
            {
                if (!newSelection.ContainsKey(id))
                {
                    EditCurveKey key;
                    if ( keys.TryGetValue(id, out key))
                        key.Selection = EditCurveSelections.None;
                }
            }

            // Invoke selection change event.
            if ( generateCommand == true && !newSelection.Equals(selection) &&
                commandHistory != null)
                commandHistory.Add(new SelectCommand(this, newSelection, selection));

            // Update selection.
            selection = newSelection;
        }

        /// <summary>
        /// Move selected keys or tangents.
        /// </summary>
        /// <param name="newPos"></param>
        /// <param name="prevPos"></param>
        public void Move(Vector2 newPos, Vector2 prevPos)
        {
            if (!Editable || !Visible) return;

            Vector2 delta = newPos - prevPos;

            foreach (EditCurveKey key in selectedKeys.Values)
            {
                MarkModify(key);

                int oldIdx = keys.IndexOf(key);

                // Move tangents.
                if ((key.Selection & (EditCurveSelections.TangentIn |
                                        EditCurveSelections.TangentOut)) != 0)
                {
                    // Compute delta angle.
                    Vector2 pos = new Vector2(key.Position, key.Value);

                    float minAngle = MathHelper.ToRadians(MinTangentAngle);
                    float maxAngle = MathHelper.ToRadians(MaxTangentAngle);

                    const float epsilon = 1e-5f;

                    if ((key.Selection & EditCurveSelections.TangentIn) != 0)
                    {
                        // Compute delta angle.
                        double prevAngle =
                                    Math.Atan2(prevPos.Y - pos.Y, pos.X - prevPos.X);
                        double newAngle =
                                    Math.Atan2(newPos.Y - pos.Y, pos.X - newPos.X);

                        double da = prevAngle - newAngle;

                        float d = GetDistanceOfKeys(oldIdx, 0);
                        if (Math.Abs(d) > epsilon)
                        {
                            float tn = key.TangentIn / d;
                            key.TangentIn = (float)Math.Tan(MathHelper.Clamp(
                                (float)(Math.Atan(tn) + da), minAngle, maxAngle)) * d;

                            if (Single.IsNaN(key.TangentIn))
                                key.TangentIn = key.TangentIn;

                            key.TangentInType = EditCurveTangent.Fixed;
                        }
                    }

                    if ((key.Selection & EditCurveSelections.TangentOut) != 0)
                    {
                        // Compute delta angle.
                        double prevAngle =
                            Math.Atan2(prevPos.Y - pos.Y, prevPos.X - pos.X);
                        double newAngle =
                            Math.Atan2(newPos.Y - pos.Y, newPos.X - pos.X);
                        double da = newAngle - prevAngle;

                        float d = GetDistanceOfKeys(oldIdx, 1);
                        if (Math.Abs(d) > epsilon)
                        {
                            float tn = key.TangentOut / d;
                            key.TangentOut = (float)Math.Tan(MathHelper.Clamp(
                                (float)(Math.Atan(tn) + da), minAngle, maxAngle)) * d;
                            key.TangentOutType = EditCurveTangent.Fixed;
                        }
                    }
                }

                // Move key position.
                keys.RemoveAt(oldIdx);    // remove key from curve once.

                if ((key.Selection & EditCurveSelections.Key) != 0)
                {
                    key.OriginalKey = new CurveKey(
                        key.Position + delta.X, key.Value + delta.Y,
                        key.TangentIn, key.TangentOut, key.Continuity);
                }

                // Then store updated node back to the curve.
                keys.Add(key);

                // Compute auto-generated tangents.
                int newIdx = keys.IndexOf(key);
                ComputeTangents(newIdx);

                if (newIdx != oldIdx)
                    ComputeTangents(oldIdx);

            }

        }

        /// <summary>
        /// Updated specified key values.
        /// </summary>
        public void UpdateKey( long keyId, float newPosition, float newValue)
        {
            if (!Editable || !Visible) return;

            EditCurveKey key;
            keys.TryGetValue(keyId, out key);
            MarkModify(key);

            int oldIdx = keys.IndexOf(key);

            // Move key position.
            keys.RemoveAt(oldIdx);    // remove key from curve once.
            key.OriginalKey = new CurveKey( newPosition, newValue,
                                        key.TangentIn, key.TangentOut, key.Continuity);

            // Then store updated node back to the curve.
            keys.Add(key);

            // Compute auto-generated tangents.
            int newIdx = keys.IndexOf(key);
            ComputeTangents(newIdx);

            if (newIdx != oldIdx)
                ComputeTangents(oldIdx);

            dirty = true;
        }

        /// <summary>
        /// Add new key at given position.
        /// </summary>
        /// <param name="pos"></param>
        public void AddKey(Vector2 pos)
        {
            EnsureUpdating("AddKey");

            // Create new key.
            EditCurveKey key = new EditCurveKey(EditCurveKey.GenerateUniqueId(),
                                                    new CurveKey(pos.X, pos.Y));
            key.Selection = EditCurveSelections.Key;

            // Generate add key command and execute it.
            EditCurveKeyAddRemoveCommand command =
                new EditCurveKeyAddRemoveCommand(this, key, selection);

            command.Execute();
            if (commandHistory != null) commandHistory.Add(command);
        }

        /// <summary>
        /// Remove selected keys.
        /// </summary>
        public void RemoveKeys()
        {
            if (!Editable) return;

            if (selectedKeys.Count != 0)
            {
                // Generate Remove keys command and execute it.
                EditCurveKeyAddRemoveCommand command =
                    new EditCurveKeyAddRemoveCommand(this, selectedKeys.Values);

                command.Execute();
                if (commandHistory != null) commandHistory.Add(command);

                // Clear selection
                selection.Clear();
                selectedKeys.Clear();

                dirty = true;
            }
        }

        /// <summary>
        /// Apply new EditCurveState.
        /// </summary>
        /// <param name="newState"></param>
        public void ApplyState(EditCurveState newState)
        {
            if (newState == null) throw new ArgumentNullException("newState");

            inUpdating = true;
            Name = newState.Name;
            PreLoop = newState.PreLoop;
            PostLoop = newState.PostLoop;
            inUpdating = false;
            FireStateChangeEvent();
        }

        /// <summary>
        /// Apply given key values.
        /// </summary>
        /// <param name="newKeyValues"></param>
        public void ApplyKeyValues(ICollection<EditCurveKey> newKeyValues)
        {
            foreach (EditCurveKey newKeyValue in newKeyValues)
            {
                EditCurveKey key = newKeyValue.Clone();
                // Update key value.
                keys.Remove(keys.GetValue(key.Id));
                keys.Add(key);

                // Also, update key values if that key is selected.
                if (selectedKeys.ContainsKey(key.Id))
                {
                    selectedKeys.Remove(key.Id);
                    selectedKeys.Add(key.Id, key);
                }

                dirty = true;
            }
        }

        /// <summary>
        /// Set specfied tangent type to selected tangents.
        /// </summary>
        /// <param name="targetTangent">target tangent (In/Out)</param>
        /// <param name="tangentType"></param>
        public void SetTangents(EditCurveSelections targetTangent,
                                        EditCurveTangent tangentType)
        {
            if (!Editable) return;

            EnsureUpdating("SetTangents");

            // Change tangent type of selcted nodes.
            foreach (EditCurveKey key in selectedKeys.Values)
            {
                MarkModify(key);

                if (tangentType == EditCurveTangent.Stepped)
                {
                    SetKeyContinuity(key, CurveContinuity.Step);
                }
                else
                {
                    SetKeyContinuity(key, CurveContinuity.Smooth);

                    if ((targetTangent & EditCurveSelections.TangentIn) != 0)
                        key.TangentInType = tangentType;

                    if ((targetTangent & EditCurveSelections.TangentOut) != 0)
                        key.TangentOutType = tangentType;
                }
            }

            // Then, compute tangents.
            foreach (EditCurveKey key in selectedKeys.Values)
                ComputeTangents(keys.IndexOf(key));
        }

        /// <summary>
        /// Compute specfied index key tangents.
        /// </summary>
        /// <param name="idx"></param>
        public void ComputeTangents(int keyIndex)
        {
            if (keyIndex < 0 || keyIndex >keys.Count ||keyIndex > Int32.MaxValue - 2)
                throw new ArgumentOutOfRangeException("keyIndex");

            // Compute neighbors tangents too.
            for (int i = keyIndex - 1; i < keyIndex + 2; ++i)
            {
                if (i >= 0 && i < keys.Count)
                {
                    EditCurveKey key = keys[i];

                    MarkModify(key);

                    float tangentInValue = key.TangentIn;
                    float tangentOutValue = key.TangentOut;
                    CurveTangent tangentIn = Convert(key.TangentInType);
                    CurveTangent tangentOut = Convert(key.TangentOutType);

                    OriginalCurve.ComputeTangent(i, tangentIn, tangentOut);

                    if (Single.IsNaN(key.TangentIn)) key.TangentIn = 0.0f;
                    if (Single.IsNaN(key.TangentOut)) key.TangentOut = 0.0f;

                    // Restore original value if EditCurveTanget is fixed.
                    if (key.TangentInType == EditCurveTangent.Fixed)
                        key.TangentIn = tangentInValue;

                    if (key.TangentOutType == EditCurveTangent.Fixed)
                        key.TangentOut = tangentOutValue;
                }
            }
        }

        /// <summary>
        /// Get distance between given index key position and previous/next key.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="direction">0:previeous key, 1:next key</param>
        /// <returns></returns>
        public float GetDistanceOfKeys(int index, int direction)
        {
            float result = 1.0f;

            if (direction == 0)
            {
                // From previous key.
                if (index > 0)
                {
                    result = keys[index].Position - keys[index - 1].Position;
                }
                else if (OriginalCurve.PreLoop == CurveLoopType.Oscillate &&
                            keys.Count > 1)
                {
                    result = keys[1].Position - keys[0].Position;
                }
            }
            else
            {
                // From next key.
                if (index < keys.Count - 1)
                {
                    result = keys[index + 1].Position - keys[index].Position;
                }
                else if (OriginalCurve.PostLoop == CurveLoopType.Oscillate &&
                            keys.Count > 1)
                {
                    result = keys[index].Position - keys[index - 1].Position;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns selected key list.
        /// </summary>
        /// <returns></returns>
        public EditCurveKey[] GetSelectedKeys()
        {
            EditCurveKey[] keys = new EditCurveKey[selectedKeys.Count];
            int idx = 0;
            foreach (EditCurveKey key in selectedKeys.Values)
                keys[idx++] = key;

            return keys;
        }

        /// <summary>
        /// Load a Curve from given filename.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="name"></param>
        /// <param name="color"></param>
        /// <param name="commandHistory"></param>
        /// <returns></returns>
        public static EditCurve LoadFromFile(string filename, string name,
                                            System.Drawing.Color color, CommandHistory commandHistory)
        {
            EditCurve editCurve = null;
            using (XmlReader xr = XmlReader.Create(filename))
            {
                Curve curve = IntermediateSerializer.Deserialize<Curve>(xr,
                                                    Path.GetDirectoryName(filename));
                editCurve = new EditCurve(name, color, curve, commandHistory);
            }

            return editCurve;
        }

        /// <summary>
        /// Save this curve to given filename.
        /// </summary>
        /// <param name="filename"></param>
        public void Save(string filename)
        {
            using (XmlWriter xw = XmlWriter.Create(filename))
            {
                IntermediateSerializer.Serialize(xw, originalCurve,
                                            Path.GetDirectoryName(filename));
                dirty = false;
            }
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Mark modified key.
        /// </summary>
        private void MarkModify(EditCurveKey key)
        {
            // Clone and save current EditCurveKey to modified keys.
            if (modifiedKeys != null && !modifiedKeys.ContainsKey(key.Id))
            {
                modifiedKeys.Add(key.Id, key.Clone());
                dirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="continuity"></param>
        private void SetKeyContinuity(EditCurveKey key, CurveContinuity continuity)
        {
            if (key.Continuity != continuity)
            {
                MarkModify(key);

                key.Continuity = continuity;
                if (continuity == CurveContinuity.Step)
                {
                    key.TangentIn = key.TangentOut = 0;
                    key.TangentInType = key.TangentInType = EditCurveTangent.Flat;
                }
            }
        }

        /// <summary>
        /// Convert EditCurveTanget to CurveTangent.
        /// </summary>
        /// <param name="tangent"></param>
        /// <returns></returns>
        private static CurveTangent Convert(EditCurveTangent tangent)
        {
            return (tangent == EditCurveTangent.Fixed) ?
                        CurveTangent.Flat : (CurveTangent)tangent;
        }

        private void FireStateChangeEvent()
        {
            dirty = true;

            if (inUpdating) return;

            if (StateChanged != null) StateChanged(this, EventArgs.Empty);
            if (Owner != null)
                Owner.Invalidate();
        }

        private void EnsureUpdating(string operationName)
        {
            if (!inUpdating)
                throw new InvalidOperationException(String.Format(
                    "You have to call BeginUpdate before call {0}", operationName));
        }
        #endregion

        #region Private Constants
        const float MinTangentAngle = -89.99999f;
        const float MaxTangentAngle = +89.99999f;
        #endregion

        #region Properties wrap members

        private Control owner;
        private long id;
        private System.Drawing.Color color = System.Drawing.Color.Red;
        private Curve originalCurve;
        private EditCurveKeyCollection keys;
        private bool editable = true;
        private bool visible = true;
        private bool dirty = false;

        #endregion

        #region Private members

        private CommandHistory commandHistory;

        private Dictionary<long, EditCurveKey> selectedKeys =
                                                new Dictionary<long, EditCurveKey>();
        private EditCurveKeySelection selection = new EditCurveKeySelection();

        private Dictionary<long, EditCurveKey> modifiedKeys;

        private EditCurveState state = new EditCurveState();
        private EditCurveState savedState;

        private bool inUpdating;

        #endregion

    }

}
