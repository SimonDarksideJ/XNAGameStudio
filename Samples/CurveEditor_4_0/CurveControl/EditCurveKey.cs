//-----------------------------------------------------------------------------
// EditCurveKey.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Xna.Tools
{
    /// <summary>
    /// This class provide editing related informaiton.
    /// </summary>
    public class EditCurveKey : IEquatable<EditCurveKey>
    {
        #region Properties

        /// <summary>
        /// Sets/Gets Id.
        /// </summary>
        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Sets/Gets Selection state.
        /// </summary>
        public EditCurveSelections Selection
        {
            get { return selection; }
            set { selection = value; }
        }

        /// <summary>
        /// Sets/Gets tangent out type.
        /// </summary>
        public EditCurveTangent TangentOutType
        {
            get { return tangentOutType; }
            set { tangentOutType = value; }
        }

        /// <summary>
        /// Sets/Gets  tangent in type.
        /// </summary>
        public EditCurveTangent TangentInType
        {
            get { return tangentInType; }
            set { tangentInType = value; }
        }

        /// <summary>
        /// Gets original CurveKey instance.
        /// </summary>
        public CurveKey OriginalKey
        {
            get { return originalKey; }
            set { originalKey = value; }
        }

        /// <summary>
        /// Gets key position.
        /// </summary>
        public float Position   { get { return originalKey.Position; } }

        /// <summary>
        /// Sets/Gets key value.
        /// </summary>
        public float Value
        {
            get { return originalKey.Value; }
            set { originalKey.Value = value; }
        }

        /// <summary>
        /// Sets/Gets TangentIn.
        /// </summary>
        public float TangentIn
        {
            get { return originalKey.TangentIn; }
            set { originalKey.TangentIn = value; }
        }

        /// <summary>
        /// Sets/Gets TangentOut.
        /// </summary>
        public float TangentOut
        {
            get { return originalKey.TangentOut; }
            set { originalKey.TangentOut = value; }
        }

        /// <summary>
        /// Sets/Gets Continuity.
        /// </summary>
        public CurveContinuity Continuity
        {
            get { return originalKey.Continuity; }
            set { originalKey.Continuity = value; }
        }

        #endregion

        #region Constructors
        public EditCurveKey(long id, CurveKey key)
        {
            Id = id;
            OriginalKey = key;

            TangentOutType = TangentInType = EditCurveTangent.Smooth;
        }
        #endregion

        /// <summary>
        /// Clone this Key.
        /// </summary>
        /// <returns></returns>
        public EditCurveKey Clone()
        {
            EditCurveKey a = new EditCurveKey(Id, OriginalKey.Clone());
            a.Selection         = Selection;
            a.TangentInType     = TangentInType;
            a.TangentOutType    = TangentOutType;

            return a;
        }

        /// <summary>
        /// Compare against another key.
        /// </summary>
        /// <returns></returns>
        public bool Equals(EditCurveKey other)
        {
            return (
                other != null &&
                Id == other.Id &&
                OriginalKey.Equals( other.OriginalKey )
                );
        }

        /// <summary>
        /// Generate Unique Id for EditCurveKey
        /// We need to assign unique Id for each EditCurveKey for undo/redo.
        /// </summary>
        /// <returns></returns>
        public static long GenerateUniqueId()
        {
            // Even if you add 1000 EditCurveKeys for each second, it'll takes
            // Half billion years to reach maximum number of long.
            // So, this code should be safe enough.
            return currentId++;
        }

        private static long currentId;

        #region Properties wrap members.

        public long id;
        private EditCurveSelections selection = EditCurveSelections.None;
        private EditCurveTangent tangentOutType = EditCurveTangent.Smooth;
        private EditCurveTangent tangentInType = EditCurveTangent.Smooth;
        private CurveKey originalKey;

        #endregion

    }
}
