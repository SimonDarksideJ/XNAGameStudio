#region File Description
//-----------------------------------------------------------------------------
// CollideSphere.cs
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
    /// It's a collision sphere.
    /// </summary>
    public class CollideSphere : CollideElement
    {
        #region Fields

        /// <summary>
        /// local center of the bounding sphere
        /// </summary>
        protected Vector3 localCenter = Vector3.Zero;

        /// <summary>
        /// Bounding sphere
        /// </summary>
        protected BoundingSphere boundingSphere;

        #endregion

        #region Properties

        public BoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
        }

        public Vector3 LocalCenter
        {
            get { return localCenter; }
        }

        public Vector3 WorldCenter
        {
            get { return BoundingSphere.Center; }
        }

        /// <summary>
        /// Collision Radius of this sphere
        /// </summary>
        public float Radius
        {
            get { return BoundingSphere.Radius; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="center">center position of the sphere</param>
        /// <param name="radius">radius of the sphere</param>
        public CollideSphere(Vector3 center, float radius)
            : base()
        {
            localCenter = center;

            boundingSphere = new BoundingSphere(localCenter, radius);
        }

        /// <summary>
        /// Transform the bounding sphere.
        /// </summary>
        public override void Transform(Matrix matrix)
        {
            boundingSphere.Center = Vector3.Transform(localCenter, matrix);

            matrix.Translation = boundingSphere.Center;

            base.Transform(matrix);
        }
    }
}
