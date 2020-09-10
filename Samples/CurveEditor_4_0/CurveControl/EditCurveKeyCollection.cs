//-----------------------------------------------------------------------------
// EditCurveKeyCollection.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Xna.Tools
{
    /// <summary>
    /// This class provides same functionality of CurveEditCollection but
    /// this contains EditCurveKey instead of CurveKey.
    /// You have to use any curve key operations via this class because this class
    /// also manipulates original curve keys.
    /// </summary>
    public class EditCurveKeyCollection : ICollection<EditCurveKey>
    {
        #region Constructors.

        /// <summary>
        /// Create new instance of EditCurveKeyCollection from Curve instance.
        /// </summary>
        /// <param name="curve"></param>
        internal EditCurveKeyCollection(EditCurve owner)
        {
            // Generate EditCurveKey list from Curve class.
            this.owner = owner;
            foreach (CurveKey key in owner.OriginalCurve.Keys)
            {
                // Add EditCurveKey to keys.
                int index = owner.OriginalCurve.Keys.IndexOf(key);
                EditCurveKey newKey =
                    new EditCurveKey(EditCurveKey.GenerateUniqueId(), key);
                keys.Insert(index, newKey);
                idToKeyMap.Add(newKey.Id, newKey);
            }
        }

        #endregion

        #region IList<EditCurveKey> like Members

        /// <summary>
        /// Determines the index of a specfied CurveKey in the EditCurveKeyCollection.
        /// </summary>
        /// <param name="item">CurveKey to locate in the EditCurveKeyCollection</param>
        /// <returns>The index of value if found in the EditCurveKeyCollection;
        /// otherwise -1.</returns>
        public int IndexOf(EditCurveKey item)
        {
            for (int i = 0; i < keys.Count; ++i)
            {
                if (keys[i].Id == item.Id) return i;
            }

            return -1;
        }

        /// <summary>
        /// Remove a CurveKey from at the specfied index.
        /// </summary>
        /// <param name="index">The Zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            EditCurveKey key = keys[index];

            idToKeyMap.Remove(key.Id);
            keys.RemoveAt(index);
            owner.OriginalCurve.Keys.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the element at the specfied index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to
        /// get or set.</param>
        /// <returns>The element at the specfied index.</returns>
        public EditCurveKey this[int index]
        {
            get
            {
                return keys[index];
            }
            set
            {
                if (value == null)
                {
                    throw new System.ArgumentNullException();
                }

                // If new value has same position, it just change values.
                float curPosition = keys[index].OriginalKey.Position;
                if (curPosition == value.OriginalKey.Position)
                {
                    keys[index] = value;
                    owner.OriginalCurve.Keys[index] = value.OriginalKey;
                }
                else
                {
                    // Otherwise, remove given index key and add new one.
                    RemoveAt(index);
                    Add(value);

                    owner.Dirty = true;
                }
            }
        }

        #endregion

        #region ICollection<EditCurveKey> Members

        /// <summary>
        /// Add an item to the EditCurveKeyCollection
        /// </summary>
        /// <param name="item">CurveKey to add to the EditCurveKeyCollection</param>
        public void Add(EditCurveKey item)
        {
            if (item == null)
                throw new System.ArgumentNullException("item");

            // Add CurveKey to original curve.
            owner.OriginalCurve.Keys.Add(item.OriginalKey);

            // Add EditCurveKey to keys.
            int index = owner.OriginalCurve.Keys.IndexOf(item.OriginalKey);
            keys.Insert(index, item);
            idToKeyMap.Add(item.Id, item);

            owner.Dirty = true;
        }

        /// <summary>
        /// Removes all EditCurveKeys from the EditCurveKeyCollection.
        /// </summary>
        public void Clear()
        {
            owner.OriginalCurve.Keys.Clear();
            keys.Clear();
            idToKeyMap.Clear();

            owner.Dirty = true;
        }

        /// <summary>
        /// Determines whether the ExpandEnvironmentVariables
        /// contains a specific EditCurveKey.
        /// </summary>
        /// <param name="item">The EditCurveKey to locate in
        /// the EditEditCurveKeyCollection. </param>
        /// <returns>true if the EditCurveKey is found in
        /// the ExpandEnvironmentVariables; otherwise, false. </returns>
        public bool Contains(EditCurveKey item)
        {
            if (item == null)
                throw new System.ArgumentNullException("item");

            return keys.Contains(item);
        }

        public void CopyTo(EditCurveKey[] array, int arrayIndex)
        {
            throw new NotImplementedException(
                "The method or operation is not implemented.");
        }

        /// <summary>
        /// Gets the number of elements contained in the EditCurveKeyCollection.
        /// </summary>
        public int Count
        {
            get { return keys.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the EditEditCurveKeyCollection is
        /// read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific EditCurveKey from
        /// the EditCurveKeyCollection.
        /// </summary>
        /// <param name="item">The EditCurveKey to remove from
        /// the EditCurveKeyCollection. </param>
        /// <returns>true if item is successfully removed; otherwise, false.
        /// This method also returns false if item was not found in
        /// the EditCurveKeyCollection. </returns>
        public bool Remove(EditCurveKey item)
        {
            if (item == null)
                throw new System.ArgumentNullException("item");

            bool result = owner.OriginalCurve.Keys.Remove(item.OriginalKey);
            idToKeyMap.Remove(item.Id);

            owner.Dirty = true;

            return keys.Remove(item) && result;
        }

        #endregion

        #region IEnumerable<EditCurveKey> Members

        /// <summary>
        /// Returns an enumerator that iterates through the EditCurveKeyCollection. 
        /// </summary>
        /// <returns>A IEnumerator&gt;EditCurveKey&lt;
        /// for the EditCurveKeyCollection. </returns>
        public IEnumerator<EditCurveKey> GetEnumerator()
        {
            return keys.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the EditCurveKeyCollection. 
        /// </summary>
        /// <returns>A IEnumerator for the EditCurveKeyCollection. </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)keys).GetEnumerator();
        }
        #endregion

        /// <summary>
        /// Tries to look up a EditCurveKey by id.
        /// </summary>
        public bool TryGetValue(long keyId, out EditCurveKey value)
        {
            return idToKeyMap.TryGetValue(keyId, out value);
        }

        /// <summary>
        /// look up a EditCurveKey by id.
        /// </summary>
        public EditCurveKey GetValue(long keyId)
        {
            return idToKeyMap[keyId];
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public EditCurveKeyCollection Clone()
        {
            EditCurveKeyCollection newKeys = new EditCurveKeyCollection();
            newKeys.keys = new List<EditCurveKey>(keys);
            return newKeys;
        }

        #region Private methods.

        /// <summary>
        /// Private default construction.
        /// </summary>
        private EditCurveKeyCollection()
        {
        }

        #endregion

        #region Private members.

        /// <summary>
        /// Owner of this collection
        /// </summary>
        private EditCurve owner;

        /// <summary>
        /// EditCurveKey that contains EditCurveKey
        /// </summary>
        private List<EditCurveKey>  keys = new List<EditCurveKey>();

        /// <summary>
        /// Id to EditCurveKey map.
        /// </summary>
        private Dictionary<long, EditCurveKey> idToKeyMap =
                                                new Dictionary<long, EditCurveKey>();

        #endregion
    }
}
