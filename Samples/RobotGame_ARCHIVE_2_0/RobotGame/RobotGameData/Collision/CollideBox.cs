#region File Description
//-----------------------------------------------------------------------------
// CollideBox.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RobotGameData.Collision
{
    /// <summary>
    /// It's a collision box.
    /// </summary>
    public class CollideBox : CollideElement
    {
        #region Fields

        /// <summary>
        /// min local position of the bounding box
        /// </summary>
        protected Vector3 localMin = Vector3.Zero;

        /// <summary>
        /// max local position of the bounding box
        /// </summary>
        protected Vector3 localMax = Vector3.Zero;

        /// <summary>
        /// Bounding box
        /// </summary>
        protected BoundingBox boundingBox;

        #endregion

        #region Properties

        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="min">min size of the box</param>
        /// <param name="max">max size of the box</param>
        public CollideBox(Vector3 min, Vector3 max) : base()
        {
            localMin = min;
            localMax = max;

            boundingBox = new BoundingBox(localMin, localMax);
        }

        /// <summary>
        /// Transform the bounding box.
        /// </summary>
        public override void Transform(Matrix matrix)
        {
            boundingBox.Min = Vector3.Transform(localMin, matrix);
            boundingBox.Max = Vector3.Transform(localMax, matrix);

            base.Transform(matrix);
        }
    }
}
