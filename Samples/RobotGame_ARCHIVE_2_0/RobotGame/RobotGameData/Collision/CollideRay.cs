#region File Description
//-----------------------------------------------------------------------------
// CollideRay.cs
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
    /// It's a collision ray.
    /// </summary>
    public class CollideRay : CollideElement
    {
        #region Fields

        /// <summary>
        /// local position of the bounding ray
        /// </summary>
        protected Vector3 localPosition = Vector3.Zero;

        /// <summary>
        /// local direction of the bounding ray
        /// </summary>
        protected Vector3 localDirection = Vector3.Zero;

        /// <summary>
        /// Bounding ray
        /// </summary>
        protected Ray ray;

        #endregion

        #region Properties

        public Ray Ray
        {
            get { return ray; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">position of the ray</param>
        /// <param name="direction">direction of the ray</param>
        public CollideRay(Vector3 position, Vector3 direction)
            : base()
        {
            localPosition = position;
            localDirection = direction;

            ray = new Ray(localPosition, localDirection);
        }

        /// <summary>
        /// Set to new transform matrix.
        /// </summary>
        public override void Transform(Matrix matrix)
        {
            ray.Position = Vector3.Transform(localPosition, matrix);
            ray.Direction = Vector3.Transform(localDirection, matrix);
            ray.Direction.Normalize();

            base.Transform(matrix);
        }
    }
}
