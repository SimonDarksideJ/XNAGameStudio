#region File Description
//-----------------------------------------------------------------------------
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
#endregion

namespace SpaceShooter
{

    // It is the base class for camera.
    public class Camera : DrawableGameComponent
    {
        protected Matrix view;
        protected Matrix xform;
        protected float fovy;
        protected Matrix proj;
        protected BoundingFrustum bf;
        protected float aspectRatio;
        protected float near;
        protected float far;

        protected Vector3 position;
        protected Quaternion rotation;

        public Matrix View
        {
            get { return view; }
            set
            {
                view = value;
                Matrix.Invert(ref view, out xform);
            }
        }

        public Matrix Transform
        {
            get { return xform; }
            set
            {
                xform = value;
                Matrix.Invert(ref xform, out view);
            }
        }

        public float FieldOfView
        {
            get { return fovy; }
            set
            {
                fovy = value;
                CreateProjection();
            }
        }

        public float AspectRatio
        {
            get { return aspectRatio; }
        }


        public Matrix Projection
        {
            get { return proj; }
        }

        public float Near
        {
            get { return near; }
        }

        public float Far
        {
            get { return far; }
        }

        public BoundingFrustum BF
        {
            get { return bf; }
        }

        public Camera(Game game) :
            base(game)
        {
            View = Matrix.CreateLookAt(new Vector3(0, 10, 0), Vector3.Zero, Vector3.Up);
            fovy = MathHelper.ToRadians(45.0f);
            aspectRatio = 1.0f;
            near = 0.1f;
            far = 4000.0f;
            CreateProjection();

            bf = new BoundingFrustum(Matrix.Identity);
        }

        public void SetProjectionParams(float fovy, float aspectRatio, float near, float far)
        {
            this.fovy = fovy;
            this.aspectRatio = aspectRatio;
            this.near = near;
            this.far = far;
            CreateProjection();

            bf.Matrix = view * proj;
        }

        void CreateProjection()
        {
            proj = Matrix.CreatePerspectiveFieldOfView(fovy, aspectRatio, near, far);
        }


        public Vector3 CameraPosition
        {
            get { return position; }
            set { position = value; }
        }

        public Quaternion CameraRotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
    }
}