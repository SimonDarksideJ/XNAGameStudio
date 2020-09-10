#region File Description
//-----------------------------------------------------------------------------
// Camera.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Spacewar
{
    public class Camera
    {
        /// <summary>
        /// A global projection matrix since it never changes
        /// </summary>
        private Matrix projection;

        /// <summary>
        /// A global view matrix since it never changes
        /// </summary>
        private Matrix view;

        /// <summary>
        /// The Camera position which never changes
        /// </summary>
        private Vector3 viewPosition;

        #region Properties
        public Matrix Projection
        {
            get
            {
                return projection;
            }
        }

        public Matrix View
        {
            get
            {
                return view;
            }
        }

        public Vector3 ViewPosition
        {
            get
            {
                return viewPosition;
            }
            set
            {
                viewPosition = value;
                view = Matrix.CreateLookAt(viewPosition, Vector3.Zero, Vector3.Up);
            }
        }
        #endregion

        public Camera(float fov, float aspectRatio, float nearPlane, float farPlane)
        {
            projection = Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio, nearPlane, farPlane);
        }
    }
}

