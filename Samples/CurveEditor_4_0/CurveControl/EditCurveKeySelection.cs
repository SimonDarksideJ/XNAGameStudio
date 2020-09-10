//-----------------------------------------------------------------------------
// EditCurveKeySelection.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using Microsoft.Xna.Framework;

namespace Xna.Tools
{
    /// <summary>
    /// This class contains EditCurveKey selction information.
    /// </summary>
    [Serializable]
    public class EditCurveKeySelection : Dictionary<long, EditCurveSelections>,
                                            IEquatable<EditCurveKeySelection>
    {
        #region Constructors.

        /// <summary>
        /// Create new instance of EditCurveKeySelection from EditCurve.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns>New instance of EditCurveKeySelection</returns>
        public EditCurveKeySelection(ICollection<EditCurveKey> keys) : this()
        {
            foreach (EditCurveKey key in keys)
            {
                if ( key.Selection != EditCurveSelections.None )
                    Select(key.Id, key.Selection);
            }
        }

        public EditCurveKeySelection()
            : base()
        {
        }

        protected EditCurveKeySelection(SerializationInfo info,
            StreamingContext context): base(info, context)
        {
        }

        #endregion

        #region Equality override methods.
        public override bool Equals(object obj)
        {
            bool isSame = false;
            EditCurveKeySelection other = obj as EditCurveKeySelection;
            if (other != null)
                isSame = Equals(other);
            return isSame;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (long key in Keys)
                hashCode += key.GetHashCode() + this[key].GetHashCode();

            return hashCode;
        }

        public bool  Equals(EditCurveKeySelection other)
        {
            if (other == null) return false;

            if (this.Count != other.Count) return false;
            foreach (long key in Keys)
            {
                if (!other.ContainsKey(key)) return false;
                if (this[key] != other[key]) return false;
            }

            return true;
        }
        #endregion

        /// <summary>
        /// Add CurveKey to selection.
        /// </summary>
        /// <param name="id">EditCurveKey id</param>
        /// <param name="slectTypes">slection type</param>
        public void Select(long id, EditCurveSelections selectTypes)
        {
            // Update selectTypes if it already in selection, otherwise,
            // just add to selection.
            if (ContainsKey(id))
                this[id] = this[id] | selectTypes;
            else
                Add(id, selectTypes);
        }

        /// <summary>
        /// Create clone of a selection.
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public EditCurveKeySelection Clone()
        {
            EditCurveKeySelection newSelection = new EditCurveKeySelection();
            foreach (long keyId in Keys)
                newSelection.Add(keyId, this[keyId]);

            return newSelection;
        }

        /// <summary>
        /// Compare given EditCueveKey collection and this selection.
        /// </summary>
        /// <returns>It returns true if seleciton are same,
        /// otherwise it returns false.</returns>
        public bool CompareSelection(ICollection<EditCurveKey> editCurveKeys)
        {
            if (this.Count != editCurveKeys.Count) return false;

            foreach ( EditCurveKey key in editCurveKeys )
            {
                if (!ContainsKey(key.Id)) return false;
                if (this[key.Id] != key.Selection) return false;
            }

            return true;
        }

        /// <summary>
        /// Select intersected EditCurveKey.
        /// </summary>
        /// <param name="editCurveKeys">Selection target EditCurveKeys.</param>
        /// <param name="selectRegion">Select region in unit coordinate.</param>
        /// <param name="singleSelect">It select only one EditCurveKey
        /// if this value is true.</param>
        /// <returns>It returns true if any EditCurveKey selected;
        /// otherwise it returns false.</returns>
        public bool SelectKeys(ICollection<EditCurveKey> editCurveKeys,
                                    BoundingBox selectRegion, bool singleSelect)
        {
            bool selected = false;

            foreach (EditCurveKey key in editCurveKeys)
            {
                Vector3 p =
                    new Vector3(key.OriginalKey.Position, key.OriginalKey.Value, 0);

                if (selectRegion.Contains(p) != ContainmentType.Disjoint)
                {
                    Select(key.Id, EditCurveSelections.Key);
                    selected = true;

                    if (singleSelect) break;
                }
            }

            return selected;
        }

        /// <summary>
        /// Select intersected tangents.
        /// </summary>
        /// <param name="editCurveKeys">Selection target EditCurveKeys.</param>
        /// <param name="selectRegion">Select region in unit coordinate.</param>
        /// <param name="tangentLength">Tangent length in unit coordinate.</param>
        /// <param name="singleSelect">It select only one EditCurveKey
        /// if this value is true.</param>
        /// <returns>It returns true if any EditCurveKey selected;
        /// otherwise it returns false.</returns>
        public bool SelectTangents(ICollection<EditCurveKey> editCurveKeys,
                    BoundingBox selectRegion, Vector2 tangentScale, bool singleSelect)
        {
            bool selected = false;

            foreach (EditCurveKey key in editCurveKeys)
            {
                CurveKey orgKey = key.OriginalKey;

                // User can't select stepped continuity key tangents.
                if (orgKey.Continuity == CurveContinuity.Step) continue;

                Vector3 rayOrigin = new Vector3(orgKey.Position, orgKey.Value, 0);
                for (int i = 0; i < 2; ++i)
                {
                    float tsx = tangentScale.X;
                    float tsy = tangentScale.Y;

                    Vector3 dir = (i == 0) ?
                        new Vector3(-tsx, -orgKey.TangentIn * tsy, 0) :
                        new Vector3(tsx, orgKey.TangentOut * tsy, 0);

                    float length = dir.Length();
                    dir.Normalize();
                    Ray ray = new Ray(rayOrigin, dir);

                    Nullable<float> result;
                    selectRegion.Intersects(ref ray, out result);
                    if (result.HasValue && result.Value < length)
                    {
                        Select( key.Id, (i==0)?
                            EditCurveSelections.TangentIn:
                            EditCurveSelections.TangentOut);
                        selected = true;

                        if (singleSelect) break;
                    }
                }
            }

            return selected;
        }

        /// <summary>
        /// Create new selection that result of toggle selection given two selections.
        /// </summary>
        /// <param name="selection">Current selection</param>
        /// <param name="newSelection">New selection</param>
        /// <returns>Toggle selection result.</returns>
        public static EditCurveKeySelection ToggleSelection(
                    EditCurveKeySelection selection, EditCurveKeySelection newSelection)
        {
            EditCurveKeySelection result = selection.Clone();

            foreach (long key in newSelection.Keys)
            {
                EditCurveSelections s;
                if (result.TryGetValue(key, out s))
                {
                    // Both selection contains same key, toggle selections value.
                    s = s ^ newSelection[key];

                    if (s != EditCurveSelections.None)
                        result[key] = s;
                    else
                        result.Remove(key);
                }
                else
                {
                    result.Add(key, newSelection[key]);
                }
            }

            return result;
        }

    }
}
