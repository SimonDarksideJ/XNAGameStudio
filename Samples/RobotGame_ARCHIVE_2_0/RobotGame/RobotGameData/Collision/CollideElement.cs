#region File Description
//-----------------------------------------------------------------------------
// CollideElement.cs
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
using RobotGameData.GameInterface;
#endregion

namespace RobotGameData.Collision
{
    /// <summary>
    /// It's a basic element of collision.
    /// </summary>
    public abstract class CollideElement : INamed
    {
        #region Fields

        protected string name = String.Empty;
        protected int id = 0;
        protected Matrix transformMatrix = Matrix.Identity;
        protected object owner = null;
        protected CollisionLayer parentLayer = null;

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int Id
        {
            get
            {
                if (id == 0)
                {
                    id = GetHashCode();
                }
                return id;
            }
            set { id = value; }
        }

        public Matrix TransformMatrix
        {
            get { return transformMatrix; }
        }

        public CollisionLayer ParentLayer
        {
            get { return parentLayer; }
            set { parentLayer = value; }
        }

        public object Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        #endregion


        /// <summary>
        /// Removes this element in the collison layer.
        /// </summary>
        public void RemoveInLayer()
        {
            if (parentLayer != null)
            {
                parentLayer.RemoveCollide(this);
                parentLayer = null;
            }
        }

        /// <summary>
        /// Set to new transform matrix.
        /// </summary>
        public virtual void Transform(Matrix matrix)
        {
            transformMatrix = matrix;
        }
    }
}
